using Inspection.Dtos;
using Inspection.Utils;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inspection.Core
{
    public class InspectionInfoView : MonoBehaviour
    {
        public delegate void DeviceClicked(int index);
        public event DeviceClicked OnDeviceItemClicked;

        [SerializeField] private InspectionRootView _inspectionRootUI;

        [Header("UI")]
        [SerializeField] private RectTransform _inspectionResponsiveLayout;
        [SerializeField] private TMP_Text _IdLabel, _nameLabel, _placeLabel, _typeLabel, _statusLabel;
        [SerializeField] private TMP_Text _scheduledDateLabel, _startTimeLabel, _submittedTimeLabel, _completedTimeLabel, _rejectTimeLabel, _commentLabel;
        [SerializeField] private RectTransform _scheduledDateDisplay, _startTimeDisplay, _submittedTimeDisplay, _completedTimeDisplay, _rejectTimeDisplay, _commentDisplay;
        [SerializeField] private Image _statusBackImage;
        [SerializeField] private PressableButton _resubmitButton;
        [SerializeField] private VirtualizedScrollRectList _deviceList;

        private InspectionDto _inspection;
        private OrderDeviceDto _orderDevice;
        private float savedScrollPosition = 0f;

        void Start()
        {
            _deviceList.OnVisible += OnDeviceItemVisible;
        }

        public void SetVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
            if (visible) RebuildLayout();
            else savedScrollPosition = _deviceList.Scroll;
        }

        private void OnDeviceItemVisible(GameObject gameObject, int index)
        {
            if (index < 0 || index >= _inspection.orderDevices.Count) return;

            DeviceListItem item = gameObject.GetComponent<DeviceListItem>();
            OrderDeviceDto targetData = _inspection.orderDevices[index];

            item.SetContent(targetData);
            item.SetColor(_inspectionRootUI.GetColorByStatus(targetData.status));
            item.SetEditAction(() => OnDeviceItemClicked?.Invoke(index));
        }

        public void SetInfo(InspectionDto inspection)
        {
            _inspection = inspection;
            _IdLabel.text = $"{inspection.recordSn}";
            _nameLabel.text = inspection.description;
            _placeLabel.text = inspection.buildingName;
            _typeLabel.text = inspection.translatedOrderType;
            UpdateInfoViewStatusAndDate();
            _deviceList.SetItemCount(inspection.orderDevices.Count);
            savedScrollPosition = 0f;
        }

        public void UpdateInfoViewStatusAndDate()
        {
            UpdateStatusAndBackground();
            UpdateLabels();
        }

        private void UpdateStatusAndBackground()
        {
            _statusLabel.text = _inspection.translatedStatus ?? "";
            _statusBackImage.color = _inspectionRootUI.GetColorByStatus(_inspection.status);

            bool isRejectedAndNotSubmitted = _inspection.IsRejected || (_inspection.IsProcessing && _inspection.rejectTime != null);

            _resubmitButton.gameObject.SetActive(isRejectedAndNotSubmitted);
        }

        private void UpdateLabels()
        {
            SetLabel(_scheduledDateLabel, _scheduledDateDisplay, _inspection.scheduledDate);
            SetLabel(_startTimeLabel, _startTimeDisplay, _inspection.startTime);
            SetLabel(_submittedTimeLabel, _submittedTimeDisplay, _inspection.submitTime);
            SetLabel(_completedTimeLabel, _completedTimeDisplay, _inspection.completeTime);
            SetLabel(_rejectTimeLabel, _rejectTimeDisplay, _inspection.rejectTime);
            SetLabel(_commentLabel, _commentDisplay, _inspection.comment);
        }

        private void SetLabel(TMP_Text label, RectTransform displayObject, string value)
        {
            bool hasValue = !string.IsNullOrEmpty(value);
            displayObject?.gameObject.SetActive(hasValue);
            label.text = value ?? ""; // 防止 null 傳入
        }

        public void OnReuploadRejectedButtonClicked() => _inspectionRootUI.ReuploadRejectedButtonClicked();

        private void RebuildLayout()
        {
            //yield return null; // 等待一幀
            LayoutRebuilder.ForceRebuildLayoutImmediate(_inspectionResponsiveLayout);
            _deviceList.ResetLayout();
            _deviceList.Scroll = savedScrollPosition;
        }
    }
}
