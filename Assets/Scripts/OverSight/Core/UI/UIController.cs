using AutodeskPlatformService.Dtos;
using MRTK.Extensions;
using Newtonsoft.Json;
using Oversight.Dtos;
using Oversight.Raycast;
using Oversight.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using User.Core;

namespace Oversight.Core
{
    public class UIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ServiceManager _service;
        [SerializeField] private HandRaycastManager _handRaycastManager;

        [Header("UI")]
        [SerializeField] private HandMenuStateVisualizer _handMenuVisualizer;
        [SerializeField] private LoginSlate _loginSlate;
        [SerializeField] private InfoSlate _infoSlate;
        [SerializeField] private DialogPoolHandler _dialogPoolHandler;
        [SerializeField] private TransparentPromptDialog _promptDialog;

        [Header("QR Code")]
        [SerializeField] private QRCodeDetector _qrCodeDetector;

        [Header("Building")]
        [SerializeField] private BuildingViewer _rGManager;
        [SerializeField] private Transform _clippingWorkbench;

        [Header("Model fine-turning")]
        [SerializeField] private BoundControlHandler _boundCtrlHandler;

        private List<DeviceInfoDto> _deviceInfos = null;
        private Dictionary<string, APSModelInfo> _apsModelInfos;
        private bool _selectForNote = false;

        // Start is called before the first frame updateToS
        private void Start()
        {
            _loginSlate.Init(_service);
            _loginSlate.OnLoginSuccess += HandleUserLogin;
        }

        private void OnDestroy()
        {
            if (_loginSlate != null)
                _loginSlate.OnLoginSuccess -= HandleUserLogin;
        }

        private void HandleUserLogin()
        {
            _handRaycastManager.RequestAnchorObject(_rGManager.transform);
            PrepareDatas();
        }

        private void PrepareDatas()
        {
            _service.GetAPSModelData(apsModelData =>
            {
                if (apsModelData != null)
                    _apsModelInfos = apsModelData;
                else
                    _dialogPoolHandler.EnqueueDialog("APS�ɮ׸��J���ѡA���ˬd�����γsô�޲z��");
            });

            _service.LoadDeviceInfos(deviceInfos =>
            {
                if (deviceInfos != null)
                    _deviceInfos = deviceInfos;
                else
                    _dialogPoolHandler.EnqueueDialog("�]�Ƹ�T������ѡA���ˬd�����γsô�޲z��");

                _infoSlate.Init(_service, _rGManager, _deviceInfos);
            });
        }

        public APSModelInfo GetAPSModelInfo(string elementID)
        {
            if (_apsModelInfos == null || !_apsModelInfos.ContainsKey(elementID))
            {
                _dialogPoolHandler.EnqueueDialog("APS��Ƹ��J����");
                return null;
            }
            return _apsModelInfos[elementID];
        }

        // ShowBIMInfo(hitObject.name, false);
        public void ShowBIMInfo(string sourceName, bool onlyID)
        {
            BIMDataDto selectedBIM = new BIMDataDto(sourceName, onlyID);
            var device = _deviceInfos.FirstOrDefault(device => device.DeviceId == selectedBIM.DeviceID);
            //Debug.Log(selectedBIM.DeviceID);

            if (_selectForNote)
            {
                string deviceDescription = selectedBIM.DeviceName;

                if (device != null) deviceDescription = device.Code;
                _infoSlate.UpdateNoteDevice(deviceDescription);
                _infoSlate.GoToNoteDetail();
                _selectForNote = false;
                _promptDialog.Setup(false);
                return;
            }

            if (device != null)
                _infoSlate.ShowBIM(selectedBIM, device, device.DeviceCode);
            else
            {
                APSModelInfo apsModel = GetAPSModelInfo(selectedBIM.ElementID);
                _infoSlate.ShowBIM(selectedBIM, apsModel);
            }
            _infoSlate.GoToBIM();
        }

        // Note: Hand menu ScanQR Button Clicked Event => Call EnableQRScanner(true)
        public void EnableQRScanner(bool enabled)
        {
            string hintText = enabled ? $"���y�]��/�Ŷ� QR Code" : string.Empty;
            _promptDialog.Setup(enabled, hintText, enabled, () => EnableQRScanner(false));
            if (enabled)
                _qrCodeDetector.OnQrCodeDetected += HandleQRCodeDetected;
            else
                _qrCodeDetector.OnQrCodeDetected -= HandleQRCodeDetected;
            _qrCodeDetector.EnableArMarker(enabled);
        }

        public void TestModelPoseFineTurning()
        {
            HandleQRCodeDetected("\"DT_TPE_RG/1F/Garden\"", new Pose(new Vector3(0f,0f, 0f), new Quaternion(0f,0f, 0f, 0f)));
        }

        private void HandleQRCodeDetected(string qrContent, Pose qrPose)
        {
            EnableQRScanner(false);

            try
            {
                if (qrContent.StartsWith(_service.IBMS_PLATFORM_SERVER) || qrContent.StartsWith("http"))
                    HandleDeviceQRScaning(qrContent);
                else
                    HandleSpaceAnchor(qrContent, qrPose);
            }
            catch (Exception ex)
            {
                HandleAnchorModelFailed();
                Debug.LogException(ex);
            }
        }

        private void HandleDeviceQRScaning(string qrContent)
        {
            // �ϥΥ��h��F������ DeviceCode ���� (�Ԩ�QRCode�榡)
            string pattern = "\"DeviceCode\":\"(.*?)\"";
            Match match = Regex.Match(qrContent, pattern);

            if (match.Success)
            {
                string deviceCodeValue = match.Groups[1].Value; // �����A���������e
                Debug.Log($"DeviceCode ���ȬO: {deviceCodeValue}");
                DeviceInfoDto deviceInfo = _deviceInfos.Find(x => x.DeviceCode == deviceCodeValue);
                if (deviceInfo != null) ShowBIMInfo(deviceInfo.DeviceId, true);
                else _dialogPoolHandler.EnqueueDialog("����]��ID���ѡA���ˬd����");
            }
            else
            {
                _dialogPoolHandler.EnqueueDialog("�o���O�]��QR Code");
            }
        }

        private void HandleSpaceAnchor(string qrContent, Pose qrPose)
        {
            string qrTagName = JsonConvert.DeserializeObject<string>(qrContent);
            bool isModelAnchored = _rGManager.AnchorModelToPose(qrPose, qrTagName);

            if (!isModelAnchored)
            {
                HandleAnchorModelFailed();
            }
            else
            {
                _dialogPoolHandler.EnqueueDialog(
                    title: "�w�ͦ��ҫ�",
                    message: "�d�ݵ��M�|�Ϊp�A�O�_�ݭn�L�աH",
                    cancelAction: () => Debug.Log("Cancel model fine-tuning."),
                    confirmAction: () =>
                    {
                        _boundCtrlHandler.AddBoundControl(null, qrPose, _rGManager.transform);
                        _handMenuVisualizer.GoToPoseFineTurning();
                    }
                );
            }
        }

        private void HandleAnchorModelFailed()
        {
            _dialogPoolHandler.EnqueueDialog("�L�Ī��ҫ��w��QR Code");
        }

        public void ResetRGModelPose()
        {
            _dialogPoolHandler.EnqueueDialog(title: "�O�_���m�ҫ���m�H", confirmAction: () => _handRaycastManager.RequestAnchorObject(_clippingWorkbench.transform));
        }

        public void SelectBIMForNote()
        {
            _selectForNote = true;
            _promptDialog.Setup(true, "�I���������MEP�γ]��", false, () =>
            {
                _selectForNote = false;
                _promptDialog.Setup(false);
            });
        }

        public void SignOutAccount()
        {
            _dialogPoolHandler.EnqueueDialog(
                title: "�T�w�n�X�H", 
                confirmAction: _handMenuVisualizer.GoToSignOut, 
                cancelAction: () => Debug.Log("Cancel signout")
            );
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
