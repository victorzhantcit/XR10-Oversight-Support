using FMSolution.FMETP;
using Inspection.Core;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using MRTK.Extensions;
using Oversight.Core;
using Oversight.Dtos;
using Oversight.Utils;
using RepairOrder.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Extensions;
using Unity.VisualScripting;
using UnityEngine;
using User.Core;

#if WINDOWS_UWP
using Windows.Networking.Connectivity; // UWP 連線偵測使用
#endif

namespace Oversight.UI
{
    public enum BIMInfoPage
    {
        NoteList, // Visual Page Begin
        InspOrders,
        RepairOrders,
        BIM,
        Maintenance, // Visual Page End
        NoteDetail,
        RepairDetail,
        AddRepair
    }

    public class InfoSlate : EnumStateVisualizer<BIMInfoPage>
    {
        [Header("Dependencies")]
        [SerializeField] private DialogPoolHandler _dialogPoolHandler;
        [SerializeField] private TransparentPromptDialog _promptDialog;
        [SerializeField] private CanvasInputFieldDialog _inputFieldDialog;
        [SerializeField] private PhotoCaptureFMETP _photoCapture;
        [SerializeField] private ImagePostProcessing _imagePostProcessing;
        private ServiceManager _service;
        private BuildingViewer _targetBuilding;
        private List<DeviceInfoDto> _deviceInfos;

        [Header("Common UI")]
        [SerializeField] private ToggleCollection _pageButtonCollection;

        #region Initialize
        private new void Start() => Initialize();

        private void OnDestroy() => UnregisterEvents();

        private void OnEnable() => StartNetworkDetection();

        private void OnDisable() => StopNetworkDetection();

        private void Initialize()
        {
            RegisterEvents();
            StartCoroutine(WaitForVirtualListThenInit());
        }

        private void RegisterEvents()
        {
            _noteList.OnVisible = OnNoteListVisible;
            _bIMPropertyList.OnVisible = OnPropertyListVisible;
            _notePhotoList.OnVisible = OnPhotoListVisible;
            _repairView.NotifyChangePage += GoToRepairPage;
            _repairView.NotifyCapture += HandleAddPhotoEvent;
            _inspectionView.NotifyCapture += HandleAddPhotoEvent;
        }

        private void UnregisterEvents()
        {
            if (_inspectionView != null)
                _inspectionView.NotifyCapture += HandleAddPhotoEvent;

            if (_repairView != null)
            {
                _repairView.NotifyChangePage += GoToRepairPage;
                _repairView.NotifyCapture += HandleAddPhotoEvent;
            }
        }

        // Called by higher-level UI control scripts
        public void Init(ServiceManager service, BuildingViewer buildingViewer, List<DeviceInfoDto> deviceInfos)
        {
            _service = service;
            _targetBuilding = buildingViewer;
            _deviceInfos = deviceInfos;

            DebugLog("Start Get Device Lifecycle Async");
            _service.GetDeviceLifecycleAsync(data => {
                _lifecycleManager = new DeviceLifecycleManager(
                    repairOrders: data.RepairOrders,
                    inspOrders: data.InspOrders,
                    workOrders: data.WorkOrders,
                    alarms: data.Alarms,
                    startTime: new DateTime(2023, 1, 10),
                    endTime: new DateTime(2025, 1, 10)
                );

                // Initialize maintenance view when data received
                _maintView.Initialize(_lifecycleManager, _dialogPoolHandler);
                DebugLog("Got Device Lifecycle!");
            });

            DebugLog("Start get notes on disk ...");
            _service.GetLocalNotes(notes =>
            {
                if (notes != null)
                {
                    _notes = notes;
                    GoToNoteListView();
                    DebugLog("Got notes!");
                }
                else
                    Debug.LogWarning("No notes...");
            });

            // Initialize and update repair order view
            _repairView.Initialize(_dialogPoolHandler, service, buildingViewer, _inputFieldDialog);
        }

        private IEnumerator WaitForVirtualListThenInit()
        {
            GetComponent<Follow>().enabled = false;
            yield return new WaitForSeconds(1f);
            GoToNoteListView();
            gameObject.SetActive(false);
            GetComponent<Follow>().enabled = true; 
        }

        public void HandleAddPhotoEvent(Action<string> callback)
        {
            _photoCapture.CapturePhoto(capturedTexture =>
            {
                _imagePostProcessing.ProcessImage(capturedTexture, withHologramTexture =>
                {
                    if (withHologramTexture == null) return;
                    byte[] imageBtyesData = withHologramTexture.EncodeToPNG();
                    string base64Photo = Convert.ToBase64String(imageBtyesData);
                    callback?.Invoke(base64Photo);
                });
            });
        }

        public void GoToPage(BIMInfoPage page, Action<BIMInfoPage> initPageAction = null)
        {
            gameObject.SetActive(true);

            BIMInfoPage previousPage = base.CurrentEnumValue;
            int pageIndex = (int)page;
            if (pageIndex >= 0 && pageIndex < _pageButtonCollection.Toggles.Count)
                _pageButtonCollection.SetSelection(pageIndex);
            base.SetEnumValue(page);

            initPageAction?.Invoke(previousPage);
        }

        public void GoToRepairPage(BIMInfoPage page)
        {
            if (page == BIMInfoPage.RepairOrders || page == BIMInfoPage.RepairDetail || page == BIMInfoPage.AddRepair)
                GoToPage(page);
        }
        #endregion

        #region NoteList View
        [Header("Note List UI")]
        [SerializeField] private VirtualizedScrollRectList _noteList;
        private List<NoteDto> _notes;

        public void GoToNoteListView() => GoToPage(BIMInfoPage.NoteList, _ =>
        {
            // Initialize page UI
            if (_notes == null) return;
            _noteList.SetItemCount(_notes.Count);
            _noteList.ResetLayout();
        });
        
        public void OnNoteListVisible(GameObject target, int index)
        {
            NoteListItem item = target.GetComponent<NoteListItem>();
            int cacheIndex = index;
            item.SetContent(_notes[index]);
            item.SetEditAction(() =>
            {
                UpdateNoteDetailView(_notes[cacheIndex]);
                DebugLog("note list index: " + cacheIndex);
            });
            item.SetTurnToRepairAction(() =>
            {
                DeviceInfoDto deviceInfo = _deviceInfos.FirstOrDefault(x => x.Code == _notes[cacheIndex].CodeOfDevice);
                _repairView.UpdateAddRepairInfo(_notes[cacheIndex], deviceInfo?.Type, deviceInfo?.DeviceCode);
                GoToPage(BIMInfoPage.AddRepair);
            });
        }

        public void OnCreateNewNoteClicked()
        {
            _currentNote = new NoteDto(
                _targetBuilding.BuildingCode,
                _targetBuilding.BuildingName,
                $"{_targetBuilding.BuildingName} {_targetBuilding.SelectedFloor}",
                SecureDataManager.GetLoggedInUserName()
            );
            UpdateNoteDetailView(_currentNote);
        }

        public void OnClearNotesClicked()
        {
            _dialogPoolHandler.EnqueueDialog(
                title: "確定刪除所有筆記？",
                confirmAction: () =>
                {
                    _notes.Clear();
                    _service.SaveNotesData(_notes);
                    GoToNoteListView();
                },
                cancelAction: () => DebugLog("Cancel Delete All Note")
            );
        }
        #endregion

        #region Note Detail View
        [Header("Note Detail UI")]
        [SerializeField] private TMP_Text _noteBasicInfo;
        [SerializeField] private TMP_Text _noteDevice;
        [SerializeField] private TMP_Text _noteDescription;
        [SerializeField] private VirtualizedScrollRectList _notePhotoList;
        private NoteDto _currentNote;

        public void GoToNoteDetail() => GoToPage(BIMInfoPage.NoteDetail, previousView =>
        {
            // Initialize page UI
            if (_currentNote == null)
            {
                _dialogPoolHandler.EnqueueDialog("請從筆記頁面新增/選擇筆記");
                GoToPage(previousView);
                return;
            }

            _notePhotoList.SetItemCount(_currentNote.PhotoBase64.Count);
            _notePhotoList.ResetLayout();
        });

        public void OnPhotoListVisible(GameObject target, int index)
        {
            PhotoListItem item = target.GetComponent<PhotoListItem>();
            int cacheIndex = index;
            item.SetContent(_currentNote.PhotoBase64[index], index, true);
            item.SetRemoveAction(() => 
            {
                _currentNote.PhotoBase64.RemoveAt(cacheIndex);
                _notePhotoList.SetItemCount(_currentNote.PhotoBase64.Count);
                _notePhotoList.ResetLayout();
            });
        }

        public void UpdateNoteDetailView(NoteDto note)
        {
            _currentNote = note;
            _noteBasicInfo.text = $"<size=9>筆記內容</size>\n" +
                $"時間<indent=40>{note.Time}</indent>\n" +
                $"地點<indent=40>{note.Location}</indent>";
            _noteDevice.text = $"設備<indent=40>{(string.IsNullOrEmpty(note.CodeOfDevice) ? "未選擇" : note.CodeOfDevice)}</indent>";
            _noteDescription.text = $"說明<indent=40>{(string.IsNullOrEmpty(note.Description) ? "點擊添加說明" : note.Description)}</indent>";
            GoToNoteDetail();
        }

        public void UpdateNoteDevice(string codeOfDevice)
        {
            _currentNote.CodeOfDevice = codeOfDevice;
            _noteDevice.text = $"設備<indent=40>{(string.IsNullOrEmpty(codeOfDevice) ? "未選擇" : codeOfDevice)}</indent>";
        }

        public void OnSaveNoteClicked()
        {
            NoteDto existedNote = _notes.FirstOrDefault(note => note.Id == _currentNote.Id);

            if (existedNote != null)
                _dialogPoolHandler.EnqueueDialog("已更新筆記", $"編號:{_currentNote.Id}");
            else
            {
                _notes.Add(_currentNote);
                _dialogPoolHandler.EnqueueDialog("已新增筆記", $"編號:{_currentNote.Id}");
            }

            _service.SaveNotesData(_notes);
            GoToNoteListView();
        }

        public void OnAddNotePhotoClicked()
        {
            HandleAddPhotoEvent(base64Photo =>
            {
                _currentNote.AddPhoto(base64Photo);
                _notePhotoList.SetItemCount(_currentNote.PhotoBase64.Count);
                _notePhotoList.ResetLayout();
            });
        }

        public void OnEditDescriptionClicked()
        {
            _inputFieldDialog.Setup("輸入說明", _currentNote.Description, submitText =>
            {
                string displayText = string.IsNullOrEmpty(submitText) ? "點擊添加說明" : _currentNote.Description;
                _currentNote.Description = submitText;
                _noteDescription.text = $"說明<indent=40>{_currentNote.Description}</indent>";
            });
        }

        public void OnRemoveNoteClicked()
        {
            _dialogPoolHandler.EnqueueDialog(
                title: "確定刪除當前筆記？",
                confirmAction: () => {
                    int currentNoteIndex = _notes.FindIndex(note => note.Id == _currentNote.Id);
                    DebugLog($"{_notes[currentNoteIndex].Id}, {_currentNote.Id}, index:{currentNoteIndex}");
                    _notes.RemoveAt(currentNoteIndex);
                    _service.SaveNotesData(_notes);
                    GoToNoteListView();
                },
                cancelAction: () => DebugLog("Cancel Delete Note")
            );
        }
        #endregion

        #region Repair View (Orders, Detail, Add)
        [Header("Repair UI")]

        [SerializeField] private RepairViews _repairView;

        public void GoToRepairOrdersView() 
            => GoToPage(BIMInfoPage.RepairOrders, previousView => _repairView.GoToGeneralRepairView());

        public void GoToRepairDetail() 
            => GoToPage(BIMInfoPage.RepairDetail, previousView => _repairView.ResetLayoutPhotoLists());
        #endregion

        #region BIM Info View
        [Header("BIM UI")]
        [SerializeField] private TMP_Text _bIMDeviceName;
        [SerializeField] private TMP_Text _bIMBasicInfo;
        [SerializeField] private VirtualizedScrollRectList _bIMPropertyList;
        private Action _onDeviceLifecycleClicked = null;
        private List<KeyValue> _bIMProperties = new List<KeyValue>();
        private BIMDataDto _currentBIM;
        private string _currentDeviceCode;

        public void GoToBIM() => GoToPage(BIMInfoPage.BIM, previousView =>
        {
            // Initialize page UI
            if (_currentBIM == null)
            {
                _dialogPoolHandler.EnqueueDialog("無選取的模型！", "請於手選單切換資訊捏取模式後，捏取模型取得資訊");
                GoToPage(previousView);
                return;
            }
            _bIMPropertyList.SetItemCount(0);
            _bIMPropertyList.SetItemCount(_bIMProperties.Count);
            _bIMPropertyList.ResetLayout();
        });

        public void OnPropertyListVisible(GameObject target, int index)
        {
            KeyValueListItem item = target.GetComponent<KeyValueListItem>();
            item.SetContent(_bIMProperties[index]);
        }

        public void ShowBIM(BIMDataDto bimData, IVirtualList detail, string deviceCode = "")
        {
            _currentBIM = bimData;
            _currentDeviceCode = deviceCode;

            _bIMDeviceName.text = bimData.DeviceName;
            _bIMBasicInfo.text = bimData.ParseString();
            _onDeviceLifecycleClicked = () =>
            {
                if (!string.IsNullOrEmpty(deviceCode)) GoToMaintenance();
                else _dialogPoolHandler.EnqueueDialog("此設備無日曆紀錄！");
            };

            if (detail != null)
                _bIMProperties = detail.ToVirtualList();
            else
            {
                _bIMProperties.Clear();
                _bIMProperties.Add(new KeyValue("詳細資料載入失敗 ...", null));
            }
            GoToBIM();
        }

        public void OnShowLifecycleClicked()
        {
            _onDeviceLifecycleClicked?.Invoke();
        }
        #endregion

        #region Maintenance View
        // Maintenance View 容納資訊較多，由低階的UI Handler處理詳細邏輯
        [Header("Maintenance UI")]
        [SerializeField] private DeviceMaintenanceView _maintView;
        private DeviceLifecycleManager _lifecycleManager = null;

        public void GoToMaintenance() => GoToPage(BIMInfoPage.Maintenance, previousView =>
        {
            //// Initialize page UI
            if (_currentBIM == null)
            {
                _dialogPoolHandler.EnqueueDialog("未選擇設備！");
                GoToPage(previousView);
                return;
            }

            if (string.IsNullOrEmpty(_currentDeviceCode))
            {
                _dialogPoolHandler.EnqueueDialog("查無紀錄！");
                GoToPage(previousView);
                return;
            }

            _maintView.InitPage();
            _maintView.UpdateView(_currentBIM.DeviceName, _currentDeviceCode);
            // Test Only
            //_currentDeviceCode = "DELTA/TP/RK2/A/B1F/EE/RK2_GP_1";
            //_currentDeviceCode = "DELTA/TP/RK2/A/1F/HVAC/AHU_01F01";
            //_maintView.UpdateView(_currentDeviceCode, _currentDeviceCode);
        });
        #endregion

        #region Inspection View
        [Header("Inspection View")]
        [SerializeField] private InspectionRootView _inspectionView;

        public void GoToInspectionView() => GoToPage(BIMInfoPage.InspOrders, _ =>
        {
            // Initialize page UI
            _inspectionView.RefreshCurrentPage();
        });

        #endregion

        #region Network quality Display

        [Header("Network Quality Display")]
        [SerializeField] private TMP_Text _qualityText;
        [SerializeField] private SpriteRenderer _qualityIcon;
        [SerializeField] Sprite[] _wifiStatusSprites; // 訊號圖示表示由小到大

        private UnityEngine.Ping _ping;
        private Coroutine _networkChecking = null;

        private void StartNetworkDetection()
        {
            if (_networkChecking == null && _service != null)
                _networkChecking = StartCoroutine(NetworkDetectingLoop());
        }

        private void StopNetworkDetection()
        {
            if (_networkChecking != null)
            {
                StopCoroutine(_networkChecking);
                _networkChecking = null;
            }
        }

        private IEnumerator NetworkDetectingLoop()
        {
            while (true)
            {
                yield return CheckNetworkQuality();
                yield return new WaitForSeconds(5f);
            }
        }

        private IEnumerator CheckNetworkQuality()
        {
#if WINDOWS_UWP
        // 在 HoloLens 2（UWP）上使用 Windows 網路 API 檢測網路狀態
        bool isConnected = false;
        int signalStrength = 0;

        ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
        if (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
        {
            isConnected = true;
            signalStrength = GetSignalStrength(profile); // 取得網路訊號強度
        }

        // 顯示網路強度（UWP 的情况）
        DisplayNetworkQuality(isConnected, signalStrength);
#else
            // 在編輯器或非 UWP 環境中使用 Ping 檢測網路強度
            if (!_service.IsNetworkAvailable)
            {
                DisplayUnityNetworkQuality(false, 0);
                yield break;
            }

            _ping = new UnityEngine.Ping("8.8.8.8"); // 開始 Ping Google DNS 伺服器
            while (!_ping.isDone)
            {
                yield return null; // 等待 Ping 結果
            }

            int pingTime = _ping.time; // 取得 Ping 的時間
            DisplayUnityNetworkQuality(true, pingTime);
#endif
            yield break; // 無論任何平台 最後都要終止線程
        }

#if WINDOWS_UWP
    // 取得訊號強度的簡單模擬，使用網路狀態列的值(0-5)
    private int GetSignalStrength(ConnectionProfile profile)
    {
        var signalBars = profile.GetSignalBars();
        if (signalBars.HasValue)
        {
            return signalBars.Value;
        }
        return 0; // 沒有訊號時返回 0
    }
#endif

        // 在非 UWP 環境中透過 Ping 時間，轉換數值後呼叫顯示網路品質方法
        private void DisplayUnityNetworkQuality(bool isConnect, int pingTime)
        {
            // 以標準網路強度，尋找適合的網路狀態圖示索引 0ms / 50ms / 100ms
            int signalStrength = 5 - Mathf.RoundToInt((pingTime / 200f) * 5);
            DisplayNetworkQuality(isConnect, signalStrength);
        }

        // 顯示網路品質方法，再恢復網路時檢查"等待上傳"的數量
        private void DisplayNetworkQuality(bool isConnected, int signalStrength)
        {
            //if (!_previousConnection && isConnected)
            //    StartCoroutine(ReconnectNotUploadedCheck(_dataCenter.LoadWaitForUploadQueue()));

            //_previousConnection = isConnected;
            //Debug.Log($"Network available: {isConnected}, signal: {signalStrength}");

            int spriteIndex = Mathf.Clamp(signalStrength, 1, _wifiStatusSprites.Length - 1);
            _qualityIcon.sprite = (isConnected) ? _wifiStatusSprites[spriteIndex] : _wifiStatusSprites[0]; // 無網路連線狀態
            _qualityIcon.color = (isConnected) ? Color.white : Color.yellow;
            _qualityText.color = _qualityIcon.color;
            _qualityText.text = (isConnected) ? "Wifi 訊號" : "無訊號";
        }
        #endregion

        private void DebugLog(string message)
        {
            //Debug.Log("[BIMInfoSlate] " + message);
        }
    }
}
