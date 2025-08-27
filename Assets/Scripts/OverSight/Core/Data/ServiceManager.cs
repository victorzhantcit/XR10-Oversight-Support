using AutodeskPlatformService.Core;
using AutodeskPlatformService.Dtos;
using Inspection.Core;
using Inspection.Dtos;
using Oversight.Clipping;
using Oversight.Dtos;
using Oversight.Utils;
using RepairOrder.Core;
using RepairOrder.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Extensions;
using UnityEngine;
using User.Core;
using User.Dtos;

namespace Oversight.Core
{
    public class ServiceManager : MonoBehaviour
    {
        // Pre-processing by unity inspector, will not change in runtime, 
        public string IBMS_PLATFORM_SERVER = "http://192.168.0.101:5020";
        public string BUILDING_CODE = "RG";

        // �M�שҨϥΨ쪺�A��
        [SerializeField] private AutoDeskAPI _aps; // Monobehavior script
        [SerializeField] private List<StatusColor> _statusColors;

        private MockServiceIBMS _mockServer;
        private InspectionIBMS _inspOrderService;
        private RepairOrderIBMS _repairOrderService;
        private NoteService _noteService;

        private void Awake()
        {
            StatusColorConvert.InitMap(_statusColors);

            // LocalFileSystem ��l�]�w
            string _dataFolder = Path.Combine(Application.persistentDataPath, "Data");
            LocalFileSystem.SetDataFolder(_dataFolder);

            // ����API�I�s (Development only)
            _mockServer = new MockServiceIBMS();

            // �������A��
            AuthService.Initialize(IBMS_PLATFORM_SERVER);
            _inspOrderService = new InspectionIBMS(IBMS_PLATFORM_SERVER, BUILDING_CODE);
            _repairOrderService = new RepairOrderIBMS(IBMS_PLATFORM_SERVER);
            _noteService = new NoteService();

            // APIHelper ��l�]�w
            APIHelper.OnAPILogCallback += DebugLogBase;
        }

        private void OnDestroy()
        {
            APIHelper.OnAPILogCallback -= DebugLogBase;
        }

        private void DebugLogBase(string message, bool isWarning = false)
        {
            string finalMessage = "[iBMS Service] " + message;
            if (!isWarning) Debug.Log(finalMessage);
            else Debug.LogWarning(finalMessage);
        }

        public bool IsNetworkAvailable => Application.internetReachability != NetworkReachability.NotReachable;

        #region ���a�A�� (�D Web API)
        public async void GetLocalNotes(Action<List<NoteDto>> notes)
            => notes(await _noteService.GetLocalNotes());

        public void SaveNotesData(List<NoteDto> notes)
            => _noteService.SaveNotesData(notes);

        // �����ϯȮM�|��������{��k�|���w�סA�]�t�Ϥ��ݭn������ҫ�����m�����A�ݭn���������M�|�Ϥ��W�Ǭy�{�A�ݰQ��
        public async void GetClippingCompareImages(Action<List<ImageSelectorDtos>> images)
            => images(await _mockServer.GetClippingCompareImagesAsync(BUILDING_CODE));
        #endregion

        #region �ϥΪ�
        public async void ApplicationLoginUser(UserLoginIBMSPlatformDto userData, Action<bool> result)
            => result?.Invoke(await AuthService.LoginUserOnIBMSPlatform(userData));

        public async Task<UserPermissionDto> GetUserPermissionOnServer()
            => await AuthService.GetUserPermissionOnOnIBMSPlatform();

        public void GetAPSModelData(Action<Dictionary<string, APSModelInfo>> response)
            => _aps.GetAPSModelInfoAsync(response);

        // DeviceLifecycle API �|�������AMock server �Ȯɨ��N
        public async void GetDeviceLifecycleAsync(Action<DeviceLifecycleManager> responseLifecycle)
        {
            var lifecycle = await _mockServer.GetDeviceLifecycleAsync(BUILDING_CODE);
            responseLifecycle?.Invoke(lifecycle);
        }

        public void LoadDeviceInfos(Action<List<DeviceInfoDto>> deviceInfoResult)
        {
            GetServerDeviceInfosAsync(deviceInfoResult);
        }

        // DeviceInfo API �|�������AMock server �Ȯɨ��N
        private async void GetServerDeviceInfosAsync(Action<List<DeviceInfoDto>> callback)
        {
            var data = await _mockServer.GetDeviceInfoAsync(BUILDING_CODE);
            callback?.Invoke(data);
        }
        #endregion

        #region ����

        public void LoadRepairOrders(string buildingCode, Action<List<RepairOrderDto>> repairsResponse, bool skipLocal = false)
            => _repairOrderService.LoadRepairOrders(buildingCode, repairsResponse, skipLocal);

        public void LoadRepairOrderInfo(string buildingCode, string repairSn, Action<List<RepairOrderInfoDto>> repairInfoReponse)
            => _repairOrderService.LoadOrderInfos(buildingCode, repairSn, repairInfoReponse);

        public async Task<Dictionary<string, RepairOrderInfoDto>> GetLocalRepairInfosAsync()
            => await _repairOrderService.GetLocalOrderInfosAsync();

        public void SaveRepairInfos(Dictionary<string, RepairOrderInfoDto> localRepairInfos)
            => _repairOrderService.SaveLocalOrderInfos(localRepairInfos);
        
        public void AddRepairOrder(RepairOrderUploadDto addRepair, Action<RepairOrderUploadResponseDto> addRepairReponse)
            => _repairOrderService.AddServerRepairOrder(addRepair, addRepairReponse);

        public async Task<RepairOrderUploadResponseDto> AddRepairOrder(RepairOrderUploadDto addRepair)
            => await _repairOrderService.AddServerRepairOrder(addRepair);

        public async Task<Dictionary<string, string>> GetLocalRepairPhotoMapAsync()
            => await _repairOrderService.GetLocalPhotoMapAsync();

        public void SaveRepairPhotoMap(Dictionary<string, string> photoMap)
            => _repairOrderService.SaveRepairPhotoMap(photoMap);

        public async Task<string> GetServerPhotoAsync(string photoSn, bool refreshedToken = false)
            => await _repairOrderService.GetServerPhotoAsync(photoSn, refreshedToken);

        public async Task<List<RepairOrderUploadDto>> GetNotUploadRepairsAsync()
            => await _repairOrderService.GetNotUploadsAsync();

        public void SaveNotUploadRepairs(List<RepairOrderUploadDto> waitForUploads)
            => _repairOrderService.SaveNotUploads(waitForUploads);
        #endregion

        #region ����
        public async void GetLocalInspOrders(Action<List<InspectionDto>> inspOrders)
            => inspOrders?.Invoke(await _inspOrderService.GetLocalInspectionListAsync());

        public void SaveInspOrders(List<InspectionDto> inspOrders)
            => _inspOrderService.SaveInspectionList(inspOrders);

        public void LoadServerInspOrders(Action<List<InspectionDto>> onLoaded)
            => _inspOrderService.LoadInspectionList(onLoaded, true);

        public async void GetNotUploadInspOrders(Action<Queue<OfflineUploadDto>> waitForUploads)
            => waitForUploads?.Invoke(await _inspOrderService.GetWaitUploadQueueAsync());

        public async Task<Queue<OfflineUploadDto>> GetNotUploadInspOrders()
            => await _inspOrderService.GetWaitUploadQueueAsync();

        public void SaveNotUploadInspOrders(Queue<OfflineUploadDto> waitForUploads)
            => _inspOrderService.SaveWaitUploadQueue(waitForUploads);

        public void PostInspOrderResult(List<KeyValue> formData, Action<OrderDeviceSubmitResponseDto> submitResponse)
            => _inspOrderService.SubmitOrder(formData, submitResponse);

        public void PostInspDeviceResult(List<KeyValue> formData, Action<OrderDeviceSubmitResponseDto> submitResponse)
            => _inspOrderService.SubmitDevice(formData, submitResponse);

        public string AddInspPhoto(string photoBase64)
            => _inspOrderService.AddPhoto(photoBase64);

        public void RemoveInspPhoto(string sns)
            => _inspOrderService.RemovePhoto(sns);

        public void UpdateInspPhotoSns(string localPhotoSns, string remotePhotoSns)
            => _inspOrderService.UpdatePhotoSns(localPhotoSns, remotePhotoSns);

        public void PostInspStart(List<KeyValue> formData, Action<string> submitResponse)
            => _inspOrderService.PostStartInspection(formData, submitResponse);

        public void PostInspDeviceUpdate(List<KeyValue> formData, Action<string> submitResponse)
            => _inspOrderService.PostUpdateDevice(formData, submitResponse);

        public string GetInspPhotoBase64(string sns)
            => _inspOrderService.GetPhotoBase64(sns);

        public bool IsInspAPIReturnPureString(string url)
            => _inspOrderService.IsReturnPureString(url);
        public Texture2D GetInspPhotoTexture(string photoSns)
            => _inspOrderService.GetPhotoTexture(photoSns);
        #endregion
    }
}
