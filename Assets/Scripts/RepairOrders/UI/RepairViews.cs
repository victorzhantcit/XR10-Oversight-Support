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
                    title: "�S���ݤW�Ǫ����׬����I",
                    confirmAction: () => _repairsToggleCollection.SetSelection(0, true)); // �^����׬����A�j���sUI
                return;
            }
            ResetLayoutRepairList(true);
        }

        public void GoToGeneralRepairView() => ResetLayoutRepairList();

        private void ResetLayoutRepairList(bool showNotUpload = false)
        {
            int focusListCount = showNotUpload ? _notUploadOrders.Count : _repairOrders.Count;

            if (_repairsToggleCollection != null && _repairsToggleCollection.Toggles.Count < 2)
                _repairsToggleCollection.SetSelection(showNotUpload ? 1 : 0); // �D�j���sUI�A�Ȧb���ۦP���A�ɰ���
            _showNotUploadRepairs = showNotUpload;
            _repairOrderList.SetItemCount(0);
            _repairOrderList.SetItemCount(focusListCount);
            _notUploadLabel.text = $"�ݤW�ǡ]{_notUploadOrders.Count}�^";
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
                        _dialogPoolHandler.EnqueueDialog("���׬����w��s�I");
                    ResetLayoutRepairList();
                    UpdateRepairInfos();
                }
                else
                {
                    _dialogPoolHandler.EnqueueDialog("���o���׬�������", "���ˬd�����γsô�޲z��");
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
                    // �P�_�O�_�����Ҧ���Ū��
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
                if (_photoMap.ContainsKey(requestPhotoSn)) continue; // �w�s�b

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
            _notUploadLabel.text = $"�ݤW�ǡ]{_notUploadOrders.Count}�^";
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
                _dialogPoolHandler.EnqueueDialog("�����Ա��򥢡A�Ч�s�����I");
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

        // �q���O����׳�Ҩϥ�
        public void UpdateAddRepairInfo(NoteDto noteDto, string deviceType, string deviceCode)
        {
            _showNotUploadRepairs = false;
            _referenceNote = noteDto;
            _cacheAddRepair = new RepairOrderUploadDto(noteDto, deviceType, deviceCode);
            _addRepairBasicLabel.text = $"<size=9>���O�����</size>\n" + _cacheAddRepair.GetInfoFormatString();
            _addIssueLabel.text = FormatInfo(
                "���`�y�z", 
                string.IsNullOrEmpty(_cacheAddRepair.Issue) ? "�I���K�[���`�y�z" : _cacheAddRepair.Issue
            );

            for (int i = 0; i < _interactableButtons.Length; i++) 
                _interactableButtons[i].enabled = true;

            NotifyChangePage(BIMInfoPage.AddRepair);
            ResetLayoutAddRepairPhotos();
        }

        // �ݤW�ǳ��׳��˵����e�Ҩϥ�
        public void UpdateAddRepairInfo(RepairOrderUploadDto uploadDto)
        {
            _showNotUploadRepairs = true;
            _cacheAddRepair = uploadDto;
            _addRepairBasicLabel.text = $"<size=9>�ݤW�ǳ��׳�</size>\n" + uploadDto.GetInfoFormatString(); 
            _addIssueLabel.text = FormatInfo("���`�y�z", uploadDto.Issue);

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
            _inputFieldDialog.Setup("��J����", _cacheAddRepair.Issue, submitText =>
            {
                _addIssueLabel.text = FormatInfo(
                    "���`�y�z",
                    string.IsNullOrEmpty(submitText) ? "�I���K�[���`�y�z" : submitText
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
                _dialogPoolHandler.EnqueueDialog("�п�J���`�y�z�I");
                return;
            }

            if (_cacheAddRepair.PhotoList.Count == 0)
            {
                _dialogPoolHandler.EnqueueDialog("�Ϥ����o���šI");
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
                _dialogPoolHandler.EnqueueDialog($"�W�Ǧ��\�I�渹�G{uploadResponse.RecordSn}");
            }
            else
            {
                _notUploadOrders.Add(_cacheAddRepair);
                _service.SaveNotUploadRepairs(_notUploadOrders);
                _repairsToggleCollection.SetSelection(1, true); // �j��e���ݤW�ǲM�歶��
                ResetLayoutRepairList(true);
                _dialogPoolHandler.EnqueueDialog("�W�ǥ��ѡA�w�[�J�ݤW�ǲM��");
            }
        }

        // �W��"���ݤW��"�����׳檺�I���ƥ�
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
                _dialogPoolHandler.EnqueueDialog($"�S�����׳�Q�W�ǡI", "���ˬd�����γsô�޲z��");
                _updateNotUploadButton.enabled = true;
                _updateLoadingIcon.gameObject.SetActive(false);
                return;
            }

            // �����ǱƧǡA�קK���޲����᪺�������D
            successOrdersIndex.Sort((a, b) => b.CompareTo(a));
            if (successOrdersIndex.Count != _notUploadOrders.Count)
                _dialogPoolHandler.EnqueueDialog($"�������פW�ǥ��ѡI");

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
            _dialogPoolHandler.EnqueueDialog($"�����ɤW�ǡA�`�p {successOrdersIndex.Count} ��", confirmAction: () => UpdateRepairOrders());
        }

        #endregion
        private void DebugLog(string message)
        {
            Debug.Log("[Repair View] " + message);
        }
    }

}

