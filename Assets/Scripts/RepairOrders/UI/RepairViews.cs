using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using MRTK.Extensions;
using Oversight.Core;
using Oversight.Dtos;
using Oversight.UI;
using Oversight.Utils;
using RepairOrder.Dtos;
using RepairOrder.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace RepairOrder.Core
{
    public class RepairViews : MonoBehaviour
    {
        public delegate void GoToPage(BIMInfoPage page);
        public delegate void OnAddPhotoClicked(Action<string> callback);
        public event GoToPage NotifyChangePage;
        public event OnAddPhotoClicked NotifyCapture;
        private DialogPoolHandler _dialogPoolHandler;
        private ServiceManager _service;
        private BuildingViewer _buildingViewer;
        private CanvasInputFieldDialog _inputFieldDialog;

        private void Start()
        {
            _repairOrderList.OnVisible = OnRepairOrderListVisible;
            _preRepairPhotoList.OnVisible = OnPreRepairPhotoListVisible;
            _repairedPhotoList.OnVisible = OnRepairedPhotoListVisible;
            _addRepairPhotoList.OnVisible = OnAddRepairPhotoVisible;
            UpdateRepairOrders();
        }

        public void Initialize(
            DialogPoolHandler dialogPool, ServiceManager service, 
            BuildingViewer buildingViewer, CanvasInputFieldDialog canvasInputFieldDialog)
        {
            _dialogPoolHandler = dialogPool;
            _service = service;
            _buildingViewer = buildingViewer;
            _inputFieldDialog = canvasInputFieldDialog;
        }

        #region Repair Orders View
        [Header("Repair Orders UI")]
        [SerializeField] private VirtualizedScrollRectList _repairOrderList;
        [SerializeField] private ToggleCollection _repairsToggleCollection;
        [SerializeField] private TMP_Text _notUploadLabel;
        [SerializeField] private PressableButton _updateNotUploadButton;
        [SerializeField] private Image _updateLoadingIcon;
        private List<RepairOrderDto> _repairOrders = new List<RepairOrderDto>();
        private List<RepairOrderUploadDto> _notUploadOrders = new List<RepairOrderUploadDto>();
        private Dictionary<string, RepairOrderInfoDto> _repairOrderInfos = new Dictionary<string, RepairOrderInfoDto>();
        private Dictionary<string, string> _photoMap = new Dictionary<string, string>();
        private bool _showNotUploadRepairs = false;

        public void SwitchToGeneralRepairs() 
        {
            if (this.isActiveAndEnabled)
                ResetLayoutRepairList();
        }
        
        public void SwitchToNotUploadRepairs()
        {
            if (_notUploadOrders.Count == 0)
            {
                _dialogPoolHandler.EnqueueDialog(
                    title: "沒有待上傳的報修紀錄！",
                    confirmAction: () => _repairsToggleCollection.SetSelection(0, true)); // 回到報修紀錄，強制更新UI
                return;
            }
            ResetLayoutRepairList(true);
        }

        public void GoToGeneralRepairView() => ResetLayoutRepairList();

        private void ResetLayoutRepairList(bool showNotUpload = false)
        {
            int focusListCount = showNotUpload ? _notUploadOrders.Count : _repairOrders.Count;

            if (_repairsToggleCollection != null && _repairsToggleCollection.Toggles.Count < 2)
                _repairsToggleCollection.SetSelection(showNotUpload ? 1 : 0); // 非強制更新UI，僅在不相同狀態時執行
            _showNotUploadRepairs = showNotUpload;
            _repairOrderList.SetItemCount(0);
            _repairOrderList.SetItemCount(focusListCount);
            _notUploadLabel.text = $"待上傳（{_notUploadOrders.Count}）";
            if (_repairOrderList.isActiveAndEnabled)
                _repairOrderList.ResetLayout();
        }

        private void OnRepairOrderListVisible(GameObject target, int index)
        {
            RepairOrderListItem item = target.GetComponent<RepairOrderListItem>();

            if (!_showNotUploadRepairs)
            {
                RepairOrderDto repairInfo = _repairOrders[index];
                string recordSn = repairInfo.RecordSn;
                item.SetContent(repairInfo);
                item.SetButtonClickEvent(() =>
                {
                    UpdateRepairDetailView(_repairOrderInfos[recordSn]);
                });
            }
            else
            {
                RepairOrderUploadDto waitForUploadOrder = _notUploadOrders[index];
                int cacheIndex = index;
                RepairOrderDto formatToRepairOrder = new RepairOrderDto(waitForUploadOrder);
                item.SetContent(formatToRepairOrder);
                item.SetButtonClickEvent(() =>
                {
                    UpdateAddRepairInfo(waitForUploadOrder);
                });
            }
        }

        public void OnUpdateRepairOrdersClicked() => UpdateRepairOrders(true);

        public void UpdateRepairOrders(bool onlyGetFromServer = false)
        {
            _updateLoadingIcon.gameObject.SetActive(true);
            _service.LoadRepairOrders(_buildingViewer.BuildingCode, repairOrders =>
            {
                if (repairOrders != null)
                {
                    _repairOrders = repairOrders;
                    if (onlyGetFromServer)
                        _dialogPoolHandler.EnqueueDialog("報修紀錄已更新！");
                    ResetLayoutRepairList();
                    UpdateRepairInfos();
                }
                else
                {
                    _dialogPoolHandler.EnqueueDialog("取得報修紀錄失敗", "請檢查網路或連繫管理員");
                    Debug.LogWarning("No repair orders...");
                }

            }, onlyGetFromServer);
        }

        private async void UpdateRepairInfos()
        {
            var localRepairInfos = await _service.GetLocalRepairInfosAsync();


            if (localRepairInfos != null)
            {
                _repairOrderInfos = localRepairInfos;
            }
            else
            {
                localRepairInfos = new Dictionary<string, RepairOrderInfoDto>();
            }

            int asyncProcess = 0;
            for (int i = 0; i < _repairOrders.Count; i++)
            {
                int cacheIndex = i;
                string recordSn = _repairOrders[cacheIndex].RecordSn;
                _service.LoadRepairOrderInfo(_buildingViewer.BuildingCode, recordSn, repairInfo =>
                {
                    if (repairInfo != null && repairInfo.Count != 0)
                        localRepairInfos[recordSn] = repairInfo[0];

                    asyncProcess += 1;
                    // 判斷是否完成所有的讀取
                    if (asyncProcess == _repairOrders.Count)
                    {
                        _repairOrderInfos = localRepairInfos;
                        _service.SaveRepairInfos(localRepairInfos);
                        UpdatePhotoMap();
                    }
                });
            }
        }

        private async void UpdatePhotoMap()
        {
            var photoMap = await _service.GetLocalRepairPhotoMapAsync();

            if (photoMap != null)
            {
                _photoMap = photoMap;
            }

            if (_repairOrders == null)
                return;

            List<string> queryPhotoSns = new List<string>();
            foreach (var kvp in _repairOrderInfos)
            {
                string recordSn = kvp.Key;
                RepairOrderInfoDto repairInfo = kvp.Value;
                string photoSns = repairInfo.RepairOrder.PhotoSns;
                string rPhotoSns = repairInfo.RepairOrder.RphotoSns;
                if (!string.IsNullOrEmpty(photoSns))
                    queryPhotoSns.AddRange(photoSns.Split(','));

                if (!string.IsNullOrEmpty(rPhotoSns))
                    queryPhotoSns.AddRange(rPhotoSns.Split(','));
            }

            for (int i = 0; i < queryPhotoSns.Count; i++)
            {
                string requestPhotoSn = queryPhotoSns[i];
                if (_photoMap.ContainsKey(requestPhotoSn)) continue; // 已存在

                var photoResponse = await _service.GetServerPhotoAsync(requestPhotoSn);
                _photoMap[requestPhotoSn] = photoResponse;

            }
            _service.SaveRepairPhotoMap(_photoMap);
            CheckNotUploadOrders();
        }

        private async void CheckNotUploadOrders()
        {
            List<RepairOrderUploadDto> notUploadOrders = await _service.GetNotUploadRepairsAsync();
            _notUploadOrders = notUploadOrders ?? new List<RepairOrderUploadDto>();
            _notUploadLabel.text = $"待上傳（{_notUploadOrders.Count}）";
            ResetLayoutRepairList();
            _updateLoadingIcon.gameObject.SetActive(false);
        }
        #endregion

        #region Repair Detail View
        [Header("Repair Detail UI")]
        [SerializeField] private TMP_Text _repairDetail;
        [SerializeField] private TMP_Text _repairStatusLabel;
        [SerializeField] private Image _repairStatusImage;
        [SerializeField] private VirtualizedScrollRectList _preRepairPhotoList;
        [SerializeField] private VirtualizedScrollRectList _repairedPhotoList;
        private RepairOrderInfoDto _currentRepair;

        private List<string> _prePhotoSources;
        private List<string> _afPhotoSources;

        public void ResetLayoutPhotoLists()
        {
            _preRepairPhotoList.SetItemCount(0);
            _repairedPhotoList.SetItemCount(0);
            _preRepairPhotoList.SetItemCount(_prePhotoSources.Count);
            _repairedPhotoList.SetItemCount(_afPhotoSources.Count);
            if (_preRepairPhotoList.isActiveAndEnabled)
                _preRepairPhotoList.ResetLayout();
            if (_repairedPhotoList.isActiveAndEnabled)
                _repairedPhotoList.ResetLayout();
        }

        public void OnPreRepairPhotoListVisible(GameObject target, int index)
        {
            PhotoListItem item = target.GetComponent<PhotoListItem>();
            int cacheIndex = index;
            item.SetContent(_prePhotoSources[index], index, false);
        }

        public void OnRepairedPhotoListVisible(GameObject target, int index)
        {
            PhotoListItem item = target.GetComponent<PhotoListItem>();
            int cacheIndex = index;
            item.SetContent(_afPhotoSources[index], index, false);
        }

        private void UpdateRepairDetailView(RepairOrderInfoDto repairInfo)
        {
            if (repairInfo == null)
            {
                _dialogPoolHandler.EnqueueDialog("紀錄詳情遺失，請更新紀錄！");
                return;
            }

            LabelColorSet statusConverted = StatusColorConvert.GetLabelColorSet(repairInfo.RepairOrder.Status);

            _currentRepair = repairInfo;
            _repairDetail.text = repairInfo.GetDetailFormatString();
            _repairStatusLabel.text = statusConverted.Label.Text_zh;
            _repairStatusLabel.color = statusConverted.Colors.TextColor;
            _repairStatusImage.color = statusConverted.Colors.BaseColor;

            _prePhotoSources = GetPhotoInfo(_currentRepair.RepairOrder.PhotoSns);
            _afPhotoSources = GetPhotoInfo(_currentRepair.RepairOrder.RphotoSns);

            ResetLayoutPhotoLists();
            NotifyChangePage?.Invoke(BIMInfoPage.RepairDetail);
        }

        private List<string> GetPhotoInfo(string photoSns)
        {
            List<string> photoInfos = new List<string>();

            if (string.IsNullOrEmpty(photoSns))
                return photoInfos;

            string[] parsedPhotoSns = photoSns.Split(",");
            for (int i = 0; i < parsedPhotoSns.Length; i++)
            {
                _photoMap.TryGetValue(parsedPhotoSns[i], out string photoBase64);
                if (photoBase64 != null)
                    photoInfos.Add(photoBase64);
            }

            return photoInfos;
        }
        #endregion

        #region Add Repair
        [Header("Add Repair UI")]
        [SerializeField] private TMP_Text _addRepairBasicLabel;
        [SerializeField] private TMP_Text _addIssueLabel;
        [SerializeField] private VirtualizedScrollRectList _addRepairPhotoList;
        [SerializeField] private PressableButton[] _interactableButtons;
        private RepairOrderUploadDto _cacheAddRepair;
        private NoteDto _referenceNote;

        private void OnAddRepairPhotoVisible(GameObject target, int index)
        {
            string photoBase64 = _cacheAddRepair.PhotoList[index];
            PhotoListItem item = target.GetComponent<PhotoListItem>();
            int cacheIndex = index;

            item.SetContent(photoBase64, index, !_showNotUploadRepairs);
            item.SetRemoveAction(() =>
            {
                _cacheAddRepair.RemovePhoto(cacheIndex);
                ResetLayoutAddRepairPhotos();
            });
        }

        public void OnCaptureRepairPhotoClicked()
        {
            NotifyCapture?.Invoke(base64Photo =>
            {
                _cacheAddRepair.AddPhoto(base64Photo);
                ResetLayoutAddRepairPhotos();
            });
        }

        // 從筆記轉報修單所使用
        public void UpdateAddRepairInfo(NoteDto noteDto, string deviceType, string deviceCode)
        {
            _showNotUploadRepairs = false;
            _referenceNote = noteDto;
            _cacheAddRepair = new RepairOrderUploadDto(noteDto, deviceType, deviceCode);
            _addRepairBasicLabel.text = $"<size=9>筆記轉報修</size>\n" + _cacheAddRepair.GetInfoFormatString();
            _addIssueLabel.text = FormatInfo(
                "異常描述", 
                string.IsNullOrEmpty(_cacheAddRepair.Issue) ? "點擊添加異常描述" : _cacheAddRepair.Issue
            );

            for (int i = 0; i < _interactableButtons.Length; i++) 
                _interactableButtons[i].enabled = true;

            NotifyChangePage(BIMInfoPage.AddRepair);
            ResetLayoutAddRepairPhotos();
        }

        // 待上傳報修單檢視內容所使用
        public void UpdateAddRepairInfo(RepairOrderUploadDto uploadDto)
        {
            _showNotUploadRepairs = true;
            _cacheAddRepair = uploadDto;
            _addRepairBasicLabel.text = $"<size=9>待上傳報修單</size>\n" + uploadDto.GetInfoFormatString(); 
            _addIssueLabel.text = FormatInfo("異常描述", uploadDto.Issue);

            for (int i = 0; i < _interactableButtons.Length; i++) 
                _interactableButtons[i].enabled = false;

            NotifyChangePage(BIMInfoPage.AddRepair);
            ResetLayoutAddRepairPhotos();
        }

        private void ResetLayoutAddRepairPhotos()
        {
            _addRepairPhotoList.SetItemCount(0);
            _addRepairPhotoList.SetItemCount(_cacheAddRepair.PhotoList.Count);
            _addRepairPhotoList.ResetLayout();
        }

        public void OnEditDescriptionClicked()
        {
            _inputFieldDialog.Setup("輸入說明", _cacheAddRepair.Issue, submitText =>
            {
                _addIssueLabel.text = FormatInfo(
                    "異常描述",
                    string.IsNullOrEmpty(submitText) ? "點擊添加異常描述" : submitText
                );
                _cacheAddRepair.Issue = submitText;
                _referenceNote.Description = submitText;
            });
        }

        private string FormatInfo(string key, string value) => $"{key}<indent=40><color=grey>{value}</indent></color>";

        public void OnUploadRepairOrderClicked()
        {
            if (string.IsNullOrEmpty(_cacheAddRepair.Issue))
            {
                _dialogPoolHandler.EnqueueDialog("請輸入異常描述！");
                return;
            }

            if (_cacheAddRepair.PhotoList.Count == 0)
            {
                _dialogPoolHandler.EnqueueDialog("圖片不得為空！");
                return;
            }

            _service.AddRepairOrder(_cacheAddRepair, HandleRepairOrderUpload);
        }

        private void HandleRepairOrderUpload(RepairOrderUploadResponseDto uploadResponse)
        {
            NotifyChangePage(BIMInfoPage.RepairOrders);

            if (uploadResponse != null)
            {
                UpdateRepairOrders();
                _dialogPoolHandler.EnqueueDialog($"上傳成功！單號：{uploadResponse.RecordSn}");
            }
            else
            {
                _notUploadOrders.Add(_cacheAddRepair);
                _service.SaveNotUploadRepairs(_notUploadOrders);
                _repairsToggleCollection.SetSelection(1, true); // 強制前往待上傳清單頁面
                ResetLayoutRepairList(true);
                _dialogPoolHandler.EnqueueDialog("上傳失敗，已加入待上傳清單");
            }
        }

        // 上傳"等待上傳"的報修單的點擊事件
        public void OnUpdateNotUploadsClicked()
        {
            _updateNotUploadButton.enabled = false;
            _updateLoadingIcon.gameObject.SetActive(true);
            UpdateNotUploadsAsync();
        }

        private async void UpdateNotUploadsAsync()
        {
            List<int> successsOrdersIndex = new List<int>();

            for (int i = 0; i < _notUploadOrders.Count; i++)
            {
                RepairOrderUploadDto order = _notUploadOrders[i];
                var response = await _service.AddRepairOrder(order);

                if (response != null)
                    successsOrdersIndex.Add(i);
            }

            HandleFinishedNotUploads(successsOrdersIndex);
        }

        private void HandleFinishedNotUploads(List<int> successOrdersIndex)
        {
            if (_notUploadOrders == null || successOrdersIndex == null || successOrdersIndex.Count == 0)
            {
                _dialogPoolHandler.EnqueueDialog($"沒有報修單被上傳！", "請檢查網路或連繫管理員");
                _updateNotUploadButton.enabled = true;
                _updateLoadingIcon.gameObject.SetActive(false);
                return;
            }

            // 按降序排序，避免索引移除後的偏移問題
            successOrdersIndex.Sort((a, b) => b.CompareTo(a));
            if (successOrdersIndex.Count != _notUploadOrders.Count)
                _dialogPoolHandler.EnqueueDialog($"部分報修上傳失敗！");

            for (int i = 0; i < successOrdersIndex.Count; i++)
            {
                int removeIndex = successOrdersIndex[i];
                if (removeIndex >= 0 && removeIndex < _notUploadOrders.Count)
                    _notUploadOrders.RemoveAt(removeIndex);
                else
                    Debug.LogWarning($"Invalid index: {removeIndex}");
            }

            _service.SaveNotUploadRepairs(_notUploadOrders);
            _updateNotUploadButton.enabled = true;
            _updateLoadingIcon.gameObject.SetActive(false);
            _dialogPoolHandler.EnqueueDialog($"完成補上傳，總計 {successOrdersIndex.Count} 項", confirmAction: () => UpdateRepairOrders());
        }

        #endregion
        private void DebugLog(string message)
        {
            Debug.Log("[Repair View] " + message);
        }
    }

}

