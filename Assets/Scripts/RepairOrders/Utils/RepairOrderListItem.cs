using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using Oversight.Utils;
using RepairOrder.Dtos;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RepairOrder.Utils
{
    public class RepairOrderListItem : VirtualListItem<RepairOrderDto>
    {
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private Image _statusBG;
        [SerializeField] private TMP_Text _recordSnLabel;
        [SerializeField] private TMP_Text _baseInfoLabel;
        [SerializeField] private TMP_Text _issueLabel;
        [SerializeField] private PressableButton _editButton;
        private Action _action = null;

        public override void SetContent(RepairOrderDto data, int index = -1, bool interactable = true)
        {
            LabelColorSet statusColorSet = StatusColorConvert.GetLabelColorSet(data.Status);
            string deviceTypeFormat = data.DeviceType == "Other" ? "其他" : data.DeviceType;
            _statusLabel.text = statusColorSet.Label.Text_zh;
            _statusLabel.color = statusColorSet.Colors.TextColor;
            _statusBG.color = statusColorSet.Colors.BaseColor;
            _recordSnLabel.text = data.RecordSn;
            if (!string.IsNullOrEmpty(data.CreateTime))
                _baseInfoLabel.text = GetTimeFormat(data.CreateTime);
            if (!string.IsNullOrEmpty(data.CompleteTime))
                _baseInfoLabel.text += $" / {GetTimeFormat(data.CompleteTime)}";
            _baseInfoLabel.text += $"\n{data.BuildingName} 異常類別 {deviceTypeFormat}";
            _issueLabel.text = data.Issue;
        }

        public void SetButtonClickEvent(Action action) => _action = action;

        public void OnButtonClicked() => _action?.Invoke();

        private string GetTimeFormat(string time) => Regex.Match(time, @"\d{4}-\d{2}-\d{2}").Value;
    }
}

