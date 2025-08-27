using Oversight.Dtos;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Oversight.Utils
{
    public class TimeLineListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dateLabel;
        [SerializeField] private TimeLineNode _alarmNode;
        [SerializeField] private TimeLineNode _repairOrdersNode;
        [SerializeField] private TimeLineNode _inspOrdersNode;
        [SerializeField] private TimeLineNode _workOrdersNode;
        private Action _onClickEvent;

        public void SetDayAndWeekOfDay(int day, string weekInChinese) 
            => _dateLabel.text = $"{day:D2} <color=grey>¬P´Á{weekInChinese}";

        public void SetRepairNode(List<DeviceEventDto> events, MaintenanceStatus maintStatus)
            => _repairOrdersNode.SetPoint(events, maintStatus);

        public void SetInspNode(List<DeviceEventDto> events, MaintenanceStatus maintStatus)
            => _inspOrdersNode.SetPoint(events, maintStatus);

        public void SetWorkNode(List<DeviceEventDto> events, MaintenanceStatus maintStatus)
            => _workOrdersNode.SetPoint(events, maintStatus);

        public void SetAlarmNode(List<DeviceEventDto> events, MaintenanceStatus maintStatus)
            => _alarmNode.SetPoint(events, maintStatus);

        public void SetButtonClickEvent(Action eventAction) => _onClickEvent = eventAction;

        public void OnButtonClicked() => _onClickEvent?.Invoke();
    }

}
