using RepairOrder.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Extensions;

namespace RepairOrder.Core
{
    public class RepairOrderIBMS
    {
        // Local storage
        private readonly string _repairOrdersFile = "RepairOrders.json";
        private readonly string _repairInfosFile = "RepairOrderInfos.json";
        private readonly string _repairPhotosFile = "RepairPhotos.json";
        private readonly string _repairWaitForUploads = "RepairOrders_WaitForUpload.json";

        // Server info
        private string SERVER_PORT = string.Empty;
        private string API_RepairOrders => $"{SERVER_PORT}/api/Eqpt/RepairOrders";
        private string API_RepairOrderInfo => $"{SERVER_PORT}/api/Eqpt/RepairOrder";
        private string API_GetPhoto => $"{SERVER_PORT}/api/Eqpt/Photo";

        public RepairOrderIBMS(string serverPort)
        {
            SERVER_PORT = serverPort;
            LocalFileSystem.CheckOrCreateFile(_repairOrdersFile, new List<RepairOrderDto>());
        }

        #region LoadRepairOrders
        public void LoadRepairOrders(string buildingCode, Action<List<RepairOrderDto>> repairsResponse, bool skipLocal = false)
        {
            APIHelper.LoadDataAsync<List<RepairOrderDto>>(
                loadLocalData: GetLocalRepairsAsync,
                loadServerData: () => GetServerRepairsAsync(buildingCode),
                onDataLoaded: repairsResponse,
                saveData: SaveRepairOrders,
                skipLocal: skipLocal
            );
        }

        public async Task<List<RepairOrderDto>> GetLocalRepairsAsync()
            => await LocalFileSystem.GetLocalDataAsync<List<RepairOrderDto>>(_repairOrdersFile);
        public void SaveRepairOrders(List<RepairOrderDto> repairOrderInfos)
            => LocalFileSystem.SaveData(repairOrderInfos, _repairOrdersFile);

        private async Task<List<RepairOrderDto>> GetServerRepairsAsync(string buildingCode, bool refreshedToken = false)
        {
            DateTime today = DateTime.Now;
            string timeFormat = "yyyy-MM-dd";
            string createTime1 = today.AddYears(-2).ToString(timeFormat);
            string createTime2 = today.AddDays(1).ToString(timeFormat); // set to tomorrow so it can get today's order

            var formFields = new List<KeyValue>
            {
                new KeyValue("BuildingCode", buildingCode),
                new KeyValue("CreateTime1", createTime1),
                new KeyValue("CreateTime2", createTime2),
            };

            return await APIHelper.SendServerFormRequestAsync<List<RepairOrderDto>>(API_RepairOrders, HttpMethod.POST, formFields, refreshedToken);
        }
        #endregion

        #region LoadRepairOrderInfos
        public void LoadOrderInfos(string buildingCode, string repairSn, Action<List<RepairOrderInfoDto>> repairInfoReponse)
        {
            APIHelper.LoadDataAsync<List<RepairOrderInfoDto>>(
                loadServerData: () => GetServerOrderInfosAsync(buildingCode, repairSn),
                onDataLoaded: (data) => repairInfoReponse?.Invoke(data),
                skipLocal: true // RepairOrderInfo 隨著 RepairOrder 變更
            );
        }

        public async Task<Dictionary<string, RepairOrderInfoDto>> GetLocalOrderInfosAsync()
            => await LocalFileSystem.GetLocalDataAsync<Dictionary<string, RepairOrderInfoDto>>(_repairInfosFile);

        public void SaveLocalOrderInfos(Dictionary<string, RepairOrderInfoDto> repairOrderInfos)
            => LocalFileSystem.SaveData(repairOrderInfos, _repairInfosFile);

        public async Task<List<RepairOrderInfoDto>> GetServerOrderInfosAsync(string buildingCode, string repairSn, bool refreshedToken = false)
        {
            var formFields = new List<KeyValue>
            {
                new KeyValue("BuildingCode", buildingCode),
                new KeyValue("RepairSn", repairSn),
            };

            return await APIHelper.SendServerFormRequestAsync<List<RepairOrderInfoDto>>(API_RepairOrderInfo, HttpMethod.GET, formFields, refreshedToken);
        }
        #endregion

        #region AddRepairOrder
        public void AddServerRepairOrder(RepairOrderUploadDto addRepair, Action<RepairOrderUploadResponseDto> addRepairReponse)
        {
            APIHelper.LoadDataAsync<RepairOrderUploadResponseDto>(
                loadServerData: () => AddServerRepairOrder(addRepair),
                onDataLoaded: (data) => addRepairReponse?.Invoke(data),
                skipLocal: true // 只會上傳不會讀檔
            );
        }

        public async Task<RepairOrderUploadResponseDto> AddServerRepairOrder(RepairOrderUploadDto addRepair, bool refreshedToken = false)
        {
            var formFields = new List<KeyValue>
            {
                new KeyValue("BuildingCode", addRepair.BuildingCode),
                new KeyValue("DeviceType", addRepair.DeviceType),
                new KeyValue("DeviceCode", addRepair.DeviceCode),
                new KeyValue("DeviceDescription", addRepair.DeviceDescription),
                new KeyValue("Issuer", addRepair.Issuer),
                new KeyValue("Issue", addRepair.Issue)
            };

            for (int i = 0; i < addRepair.PhotoList.Count; i++)
                formFields.Add(new KeyValue("Photos", addRepair.PhotoList[i]));

            return await APIHelper.SendServerFormRequestAsync<RepairOrderUploadResponseDto>(API_RepairOrderInfo, HttpMethod.PUT, formFields, refreshedToken);
        }
        #endregion

        #region Photos
        public async Task<Dictionary<string, string>> GetLocalPhotoMapAsync()
            => await LocalFileSystem.GetLocalDataAsync<Dictionary<string, string>>(_repairPhotosFile);

        public void SaveRepairPhotoMap(Dictionary<string, string> photoMap)
            => LocalFileSystem.SaveData(photoMap, _repairPhotosFile);
        #endregion

        #region WaitForUploads
        public async Task<List<RepairOrderUploadDto>> GetNotUploadsAsync()
            => await LocalFileSystem.GetLocalDataAsync<List<RepairOrderUploadDto>>(_repairWaitForUploads);

        public void SaveNotUploads(List<RepairOrderUploadDto> waitForUploads)
            => LocalFileSystem.SaveData(waitForUploads, _repairWaitForUploads);
        #endregion

        #region GetPhoto
        public async Task<string> GetServerPhotoAsync(string photoSn, bool refreshedToken = false)
        {
            var formFields = new List<KeyValue> { new KeyValue("Sn", photoSn) };
            PhotoResponseDto responseData = await APIHelper.SendServerFormRequestAsync<PhotoResponseDto>(
                API_GetPhoto,
                HttpMethod.GET,
                formFields,
                refreshedToken
            );

            return responseData.Photo;
        }
        #endregion
    }

}
