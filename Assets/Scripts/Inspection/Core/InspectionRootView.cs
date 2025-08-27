using Inspection.Dtos;
using MRTK.Extensions;
using Newtonsoft.Json;
using Oversight.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Extensions;
using UnityEngine;

namespace Inspection.Core
{
    [Serializable]
    public class StatusColors
    {
        public string StatusLabel;
        public Color Color;
    }

    public class InspectionRootView : MonoBehaviour
    {
        public delegate void OnAddPhotoClicked(Action<string> callback);
        public event OnAddPhotoClicked NotifyCapture;

        private readonly string DATE_FORMAT_DB = "yyyy-MM-dd HH:mm:ss";
        private readonly string DATE_FORMAT_LOCAL = "yyyy-MM-dd HH:mm";

        [Header("Dependency")]
        [SerializeField] private ServiceManager _service;
        [SerializeField] private QRCodeDetector _qrCodeDetector;
        [SerializeField] private TransparentPromptDialog _promptDialog;
        [SerializeField] private List<StatusColors> _statusColors = new List<StatusColors>();

        [Header("UI / Slate")]
        [SerializeField] private InspectionListView _inspectionListView;
        [SerializeField] private InspectionInfoView _inspectionInfoView;
        [SerializeField] private InspectionDeviceView _inspectionDeviceView;

        private readonly Stack<ViewType> _viewHistory = new Stack<ViewType>();
        private List<InspectionDto> _inspections;
        private InspectionDto _currentInspection;
        private OrderDeviceDto _currentDevice;

        [Header("UI / Dialog")]
        [SerializeField] private DialogPoolHandler _dialogPoolHandler;
        [SerializeField] private CanvasInputFieldDialog _submitDialog;

        private bool _isEventRegistered = false;

        public enum ViewType
        {
            Main,
            Info,
            Device,
            Screenshot,
            Hide
        }

        private void Awake() => _viewHistory.Push(ViewType.Main);
        private void Start() => Initialize();
        private void OnDestroy() => UnregisterEvents();

        private void Initialize()
        {
            _inspectionDeviceView.AssignService(_service, this);
            RegisterEvents();
            SetView(ViewType.Main);
            SyncDataFromDatabase();
        }

        private void RegisterEvents()
        {
            if (_isEventRegistered) return;
            _isEventRegistered = true;
            Debug.Log("Register events");
            _inspectionListView.OnInspctionItemClicked += HandleInspectionItemClicked;
            _inspectionInfoView.OnDeviceItemClicked += HandleDeviceItemClicked;
        }

        private void UnregisterEvents()
        {
            if (!_isEventRegistered) return;
            if (_inspectionListView != null) _inspectionListView.OnInspctionItemClicked -= HandleInspectionItemClicked;
            if (_inspectionInfoView != null) _inspectionInfoView.OnDeviceItemClicked -= HandleDeviceItemClicked;
            //if (_service != null) _service.OrderDownloaded -= HandleOrderDownloaded;
            Debug.Log("Unregister events");
            _isEventRegistered = false;
        }

        private void SetView(ViewType viewType)
        {
            _inspectionListView.SetVisible(viewType == ViewType.Main);
            _inspectionInfoView.SetVisible(viewType == ViewType.Info);
            _inspectionDeviceView.SetVisible(viewType == ViewType.Device);
            // 如果新頁面比最後紀錄來的更深層，則紀錄新的View
            Debug.Log("Set View: " + viewType);
            if (viewType > _viewHistory.Peek()) _viewHistory.Push(viewType);
        }

        public void SwitchToPreviousView()
        {
            // 確保 _viewHistory 不會彈出到空 (始終保留第一個 View)
            if (_viewHistory.Count > 1) _viewHistory.Pop();
            SetView(_viewHistory.Peek());
        }
        
        public void RefreshCurrentPage()
        {
            SetView(_viewHistory.Peek());
        }

        private void HandleOrderDownloaded(bool success, List<InspectionDto> inspectionList, bool isLocalData)
        {
            if (isLocalData == false)
                ShowDialog((success) ? "已同步資料為最新狀態" : "資料下載失敗，請檢查網路或連繫管理員！", null, () => _inspectionListView.SetUpdateButtonEnable(true));
            UpdateInspectionData(inspectionList);
        }

        public void HandleQrCodeDetected(string qrContent, Pose pose)
        {
            Debug.Log("QR Code content: " + qrContent);

            UpdatePromptDialog(false);
            EnableQRScanner(false);

            // 取得 qrContent 中的 jsonData，範例: qrContent = https://ip:port/?queryPara={jsonData}
            Match match = Regex.Match(qrContent, @"\{(.*)\}");

            if (!match.Success)
            {
                ValidateDeviceQrContent(null);
                Debug.Log("QR Code Not Match");
                return;
            }

            string parseResult = match.Value;

            try
            {
                var deviceIdentify = JsonConvert.DeserializeObject<StartInspectionDto>(parseResult);

                ValidateDeviceQrContent(deviceIdentify);
                Debug.Log("QR Code Matched");
            }
            catch (Exception ex)
            {
                ValidateDeviceQrContent(null);
                Debug.LogException(ex);
            }
        }

        public void UpdatePromptDialog(bool enabled, string message = "", bool isQrHint = false)
            => _promptDialog.Setup(enabled, message, isQrHint, () => OnPromptClosed());

        private void OnPromptClosed()
        {
            EnableQRScanner(false);
            ShowSlateContent(true);
        }

        public void EnableQRScanner(bool enabled)
        {
            string hintText = enabled ? $"掃描 {_currentDevice.deviceDescription} \n巡檢 QR Code" : string.Empty;

            UpdatePromptDialog(enabled, hintText, enabled);
            if (enabled)
                _qrCodeDetector.OnQrCodeDetected += HandleQrCodeDetected;
            else
                _qrCodeDetector.OnQrCodeDetected -= HandleQrCodeDetected;
            _qrCodeDetector.EnableArMarker(enabled);
        }

        public void ValidateDeviceQrContent(StartInspectionDto qrDeviceData)
        {
            // QR 並非目標設備，繼續掃描，是目標設備則設定狀態為處理中並進入設備檢查介面
            if (qrDeviceData == null || !IsQrMatchTargetDevice(qrDeviceData))
                ShowDialog($"此並非設備 {_currentDevice.deviceDescription} 的 QR Code", null, () => ShowSlateContent(true));
            else
                ShowDialog($"正確的設備，是否開始檢查？", null, () => PostStartInspection(), () => ShowSlateContent(true));
        }

        public void ShowSlateContent(bool enabled)
        {
            //this.gameObject.SetActive(enabled);
            //if (enabled) RefreshCurrentPage();
            //this.gameObject.SetActive(enabled);

                //if (enabled)
                //{
                //    this.gameObject.SetActive(true);
                //    // 確保所有子物件可視
                //    foreach (Transform slateChild in this.transform)
                //        slateChild.gameObject.SetActive(true);

                //    SwitchToPreviousView();
                //}
                //else
                //{
                //    SetView(ViewType.Hide);
                //    // 確保所有子物件可視
                //    foreach (Transform slateChild in this.transform)
                //        slateChild.gameObject.SetActive(false);

                //    this.gameObject.SetActive(false);
                //}
        }

        private bool IsQrMatchTargetDevice(StartInspectionDto qrDeviceData) =>
            qrDeviceData.BuildingCode == _currentInspection.buildingCode &&
            qrDeviceData.DeviceCode == _currentDevice.deviceCode;

        public void UpdateInspectionData(List<InspectionDto> inspectionList)
        {
            if (inspectionList == null) return;

            _inspections = inspectionList;
            _inspectionListView.UpdateData(_inspections);
        }

        public void SyncDataFromDatabase()
        {
            _service.GetLocalInspOrders(inspOrders => HandleOrderDownloaded(inspOrders != null, inspOrders, true));

            _inspectionListView.SetUpdateButtonEnable(false);

            if (_service.IsNetworkAvailable) HandleSyncWhenOnline();
            else HandleSyncWhenOffline();
        }

        private void HandleSyncWhenOnline()
        {
            _service.GetNotUploadInspOrders(waitForUploads =>
            {
                Debug.Log($"WaitForUpload? {waitForUploads.Count > 0}");
                if (waitForUploads.Count > 0)
                    ReconnectNotUploadedCheck(waitForUploads);
                else
                    LoadServerInspectionList();
            });
        }

        private void LoadServerInspectionList()
        {
            _service.LoadServerInspOrders(inspOrders => HandleOrderDownloaded(inspOrders != null, inspOrders, false));
        }

        private async void ReconnectNotUploadedCheck(Queue<OfflineUploadDto> waitForUpload)
        {
            if (waitForUpload.Count == 0)
            {
                Debug.Log("No pending uploads in the queue.");
                _inspectionListView.SetUpdateButtonEnable(true);
                LoadServerInspectionList();
                return;
            }

            Debug.Log($"Found {waitForUpload.Count} items pending upload. Attempting to reconnect and upload...");

            int initialCount = waitForUpload.Count;

            while (waitForUpload.Count > 0)
            {
                var uploadItem = waitForUpload.Peek(); // 取出隊列最前面的元素但不移除

                bool isReturnPureString = _service.IsInspAPIReturnPureString(uploadItem.Url);

                // 調用 PostRequest 進行上傳
                var uploadResponseStatusCode = await APIHelper.SendServerFormRequestAsync<int>
                (
                    uploadItem.Url,
                    HttpMethod.POST, 
                    uploadItem.Data,
                    returnHttpStatus: true
                );

                if (uploadResponseStatusCode >= 200 && uploadResponseStatusCode <= 299)
                {
                    Debug.Log($"Upload successful for: {uploadItem.Url}"); 
                    waitForUpload.Dequeue(); // 成功後移除該項
                }
                else
                {
                    Debug.LogWarning($"Upload failed for: {uploadItem.Url}. Response was empty.");
                    Debug.LogWarning($"Stopping upload process. Remaining items will be retried later.");
                    break; // 如果上傳失敗，退出循環
                }
            }

            _service.SaveNotUploadInspOrders(waitForUpload); // 保存更新後的隊列狀態
            if (waitForUpload.Count > 0)
                Debug.Log($"Stopping upload process when uploading! Successfully uploaded {initialCount - waitForUpload.Count} items, remain {waitForUpload.Count}.");
            else
            {
                Debug.Log($"Reconnect and upload complete. Successfully uploaded {initialCount - waitForUpload.Count} items");
                LoadServerInspectionList();
            }

            _inspectionListView.SetUpdateButtonEnable(true);
        }

        public void DeviceRespondInputClicked()
            => _submitDialog.Setup("巡檢總結", null, (message) => _inspectionDeviceView.OnDeviceRespondTextChanged(message));

        public void ReuploadRejectedButtonClicked()
        {
            _submitDialog.Setup("完工說明", null, (message) =>
            {
                if (string.IsNullOrEmpty(message)) return;

                ResubmitOrderDto resubmitOrderDto = new ResubmitOrderDto(_currentInspection, message);
                List<KeyValue> requestData = KeyValueConverter.ToKeyValues(resubmitOrderDto);

                _service.PostInspOrderResult(requestData, (response) => HandleOrderSumbitResponse(response));
            });
        }

        private void HandleOrderSumbitResponse(OrderDeviceSubmitResponseDto response)
        {
            DateTime submitDateTime;

            if (response == null)
            {
                submitDateTime = DateTime.Now;
                ShowDialog("上傳至雲端時發生錯誤", "已保存資料於本機，請確認已連線至網路，待網路恢復後再上傳");
            }
            else
            {
                submitDateTime = DateTime.ParseExact(response.SubmitTime, DATE_FORMAT_DB, null);
                ShowDialog($"退件單 {_currentInspection.recordSn} 已重新上傳！");
            }

            _currentInspection.rejectTime = null;
            _currentInspection.comment = null;
            _currentInspection.SetStatusToSubmitted(submitDateTime.ToString(DATE_FORMAT_LOCAL));

            SwitchToPreviousView();
        }

        public void SubmitDeviceButtonClicked() => SubmitDeviceInspectResult();

        private void SubmitDeviceInspectResult()
        {
            DeviceSubmitDto deviceUploadDto = new DeviceSubmitDto(_service.GetInspPhotoBase64);

            bool validUploadFormat = deviceUploadDto.CheckAndSetDeviceUploadDto(
                _currentInspection.recordSn,
                _currentDevice,
                _currentInspection.orderDevices.Count,
                _currentDevice.afPhotoSns
            );

            if (!validUploadFormat)
            {
                ShowDialog(
                    $"{_currentInspection.recordSn} {_currentDevice.deviceDescription} 巡檢資料未完整",
                    deviceUploadDto.InvalidResult
                );
                return;
            }

            List<KeyValue> requestData = KeyValueConverter.ToKeyValues(deviceUploadDto);

            _service.PostInspDeviceResult(requestData, (response) => HandleDeviceSumbitResponse(response));
        }

        private void HandleDeviceSumbitResponse(OrderDeviceSubmitResponseDto response)
        {
            Debug.Log("response is null " + response == null);
            if (response == null)
            {
                HandleOfflineDeviceSubmitData();
                return;
            }
                
            DateTime submitDateTime = DateTime.ParseExact(response.SubmitTime, DATE_FORMAT_DB, null);

            _currentDevice.SetStatusToSubmitted();
            _currentDevice.manMinute = response.ManMinute;
            _currentDevice.submitTime = submitDateTime.ToString(DATE_FORMAT_LOCAL);
            _currentDevice.afPhotoSns = response.PhotoSns;

            string[] responsePhotoSns = response.PhotoSns.Split(',');

            for (int i = 0; i < responsePhotoSns.Length; i++)
            {
                string remotePhotoSns = responsePhotoSns[i];
                string localPhotoSns = _inspectionDeviceView.DevicePhotoSns[i];

                _service.UpdateInspPhotoSns(localPhotoSns, remotePhotoSns);
                _inspectionDeviceView.DevicePhotoSns[i] = remotePhotoSns;
            }

            Debug.Log($"成功上傳 {_currentDevice.deviceDescription} 的巡檢紀錄！");
            UpdateSubmittedInfoView();
            SwitchToPreviousView();

            // 儲存資料
            _service.SaveInspOrders(_inspections);

            ShowDialog($"成功上傳 {_currentDevice.deviceDescription} 的巡檢紀錄！");
        }

        private void HandleOfflineDeviceSubmitData()
        {
            DateTime deviceStartTime = DateTime.ParseExact(_currentDevice.startTime, DATE_FORMAT_LOCAL, null);
            DateTime currrentDateTime = DateTime.Now;
            TimeSpan deviceManTime = currrentDateTime - deviceStartTime;

            _currentDevice.SetStatusToNotUploaded();
            _currentDevice.manMinute = (int)deviceManTime.TotalMinutes;
            _currentDevice.submitTime = currrentDateTime.ToString(DATE_FORMAT_LOCAL);
            _currentDevice.afPhotoSns = string.Join(',', _inspectionDeviceView.DevicePhotoSns);

            SwitchToPreviousView();
            ShowDialog("上傳至雲端時發生錯誤", "已保存資料於本機，請確認已連線至網路，待網路恢復後再上傳");
        }

        private void UpdateSubmittedInfoView()
        {
            Debug.Log("UpdateSubmittedInfoView " + _currentInspection.sn);

            bool isAllDeviceSubmitted = true;
            int isNotUploadCount = 0;
            int isNotSubmittedCount = 0;

            // 檢查每個設備的狀態
            for (int i = 0; i < _currentInspection.orderDevices.Count; i++)
            {
                OrderDeviceDto device = _currentInspection.orderDevices[i];

                if (device.IsNotUploaded)
                {
                    Debug.Log($"device.IsNotUploaded {device.deviceDescription}");
                    isNotUploadCount++;
                    isAllDeviceSubmitted = false; // 如果有未上傳設備，設置為 false
                    continue;
                }
                if (!device.IsSubmitted)
                {
                    isNotSubmittedCount++;
                    isAllDeviceSubmitted = false; // 如果有未提交設備，設置為 false
                    continue;
                }
            }

            // 非退件狀態更新
            if (isNotUploadCount > 0) _currentInspection.SetStatusToNotUploaded();
            else if (isNotSubmittedCount > 0) _currentInspection.SetStatusToProcessing(_currentDevice.submitTime);
            else if (isAllDeviceSubmitted)
            {
                if (string.IsNullOrEmpty(_currentInspection.rejectTime))
                    _currentInspection.SetStatusToSubmitted(_currentDevice.submitTime);
                else
                    _currentInspection.SetStatusToRejected();
            }

            // 更新 UI 顯示
            _inspectionInfoView.SetInfo(_currentInspection);
            _inspectionListView.UpdateData(_inspections);

            // 儲存資料
            _service.SaveInspOrders(_inspections);
        }

        public void PostStartInspection(Action supplementCallback = null)
        {
            StartInspectionDto startInspectData = new StartInspectionDto();
            
            startInspectData.Initialize(_currentInspection.recordSn, _currentDevice);

            List<KeyValue> requestData = KeyValueConverter.ToKeyValues(startInspectData);

            if (string.IsNullOrEmpty(_currentInspection.rejectTime))
                _service.PostInspStart(requestData, (response) => HandleStartTimeResponse(response, supplementCallback));
            else
                _service.PostInspDeviceUpdate(requestData, (response) => HandleUpdateRejectedInspection(response, supplementCallback));
        }

        private void HandleStartTimeResponse(string responseTime, Action supplementCallback = null)
        {
            bool isOfflineValidated = responseTime == null;
            string dateTimeString = string.Empty;

            if (isOfflineValidated) dateTimeString = DateTime.Now.ToString(DATE_FORMAT_LOCAL);
            else dateTimeString = DateTime.ParseExact(responseTime, DATE_FORMAT_DB, null).ToString(DATE_FORMAT_LOCAL);

            ShowSlateContent(true);

            _currentInspection.SetStatusToProcessing(dateTimeString);
            _currentDevice.SetStatusToProcessing(dateTimeString, isOfflineValidated);

            _inspectionInfoView.UpdateInfoViewStatusAndDate();

            _service.SaveInspOrders(_inspections);
            _inspectionDeviceView.SetDetails(_currentDevice);
            SetView(ViewType.Device);


            supplementCallback?.Invoke();
        }

        private void HandleUpdateRejectedInspection(string responseTime, Action supplementCallback = null)
        {
            bool isOfflineValidated = responseTime == null;
            string dateTimeString = DateTime.Now.ToString(DATE_FORMAT_LOCAL);

            _currentInspection.SetStatusToProcessing(dateTimeString);
            _currentDevice.UpdateStatusWhenRejected(isOfflineValidated);

            _inspectionInfoView.UpdateInfoViewStatusAndDate();

            _service.SaveInspOrders(_inspections);
            _inspectionDeviceView.SetDetails(_currentDevice);
            SetView(ViewType.Device);

            supplementCallback?.Invoke();
        }

        private void HandleSyncWhenOffline()
        {
            ShowDialog("資料同步失敗", "請等待恢復網路環境後，再透過主頁面更新資料", () => _inspectionListView.SetUpdateButtonEnable(true));
            UpdateInspectionData(_inspections);
        }

        private void HandleInspectionItemClicked(int itemIndex)
        {
            _currentInspection = _inspections[itemIndex];
            _inspectionInfoView.SetInfo(_currentInspection);
            SetView(ViewType.Info);
        }

        private void HandleDeviceItemClicked(int itemIndex)
        {
            _currentDevice = _currentInspection.orderDevices[itemIndex];

            if (_currentDevice.IsPending) StartValidateDevice();
            else EnterDeviceView();
        }

        private void StartValidateDevice()
        {
            ShowSlateContent(false);
            EnableQRScanner(true);
        }

        private void EnterDeviceView()
        {
            _inspectionDeviceView.SetDetails(_currentDevice, !string.IsNullOrEmpty(_currentInspection.rejectTime));
            SetView(ViewType.Device);
        }

        public void RequestPhotoCapture(Action<string> base64PhotoResult)
            => NotifyCapture?.Invoke(base64PhotoResult);

        public Color PositiveColor => GetColorByStatus("c-processed");
        public Color NegativeColor => GetColorByStatus("c-pending");
        public Color NeuralColor => GetColorByStatus("c-processing");
        public Color DefaultColor => GetColorByStatus("Default");

        public Color GetColorByStatus(string status)
        {
            var statusColors = _statusColors.FirstOrDefault(statusColor => statusColor.StatusLabel.Contains(status));
            return statusColors?.Color ?? Color.white;
        }

        public void ShowDialog(string title, string message = null, Action confirmAction = null, Action cancelAction = null)
            => _dialogPoolHandler.EnqueueDialog(title, message, confirmAction, cancelAction);
    }
}
