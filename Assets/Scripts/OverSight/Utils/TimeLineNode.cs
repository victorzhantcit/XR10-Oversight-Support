using System.Collections.Generic;
using UnityEngine;
using Oversight.Dtos;
using UnityEngine.UI;
using TMPro;
using Unity.Extensions;

namespace Oversight.Utils
{
    public enum MaintenanceStatus
    {
        PendingBefore,
        InProgress,
        PendingAfter,
        SingleProcess
    }

    public enum TimeNodeState
    {
        None,
        FirstEvent,                
        InProgressIDLE,
        InProgressEvent,      
        LatestEvent,
        SingleEvent
    }

    public class TimeLineNode : EnumStateVisualizer<TimeNodeState>
    {
        //public Color BaseColor;
        [SerializeField] private Image _progressBeforeLine;
        [SerializeField] private Image _progressAfterLine;
        [SerializeField] private Image _eventPoint;
        [SerializeField] private TMP_Text _eventCount;

        // Hide base start
        private new void Start()
        {
            //_progressBeforeLine.color = BaseColor;
            //_progressAfterLine.color = BaseColor;
            //_eventPoint.color = BaseColor;
        }

        public void SetPoint(List<DeviceEventDto> dateEvents, MaintenanceStatus maintenanceStatus)
        {
            bool hasEvent = dateEvents != null && dateEvents.Count > 0;
            TimeNodeState state = TimeNodeState.None;

            // 有事件的狀態判斷
            if (hasEvent)
            {
                if (maintenanceStatus == MaintenanceStatus.PendingBefore)
                    state = TimeNodeState.FirstEvent;
                else if (maintenanceStatus == MaintenanceStatus.InProgress)
                    state = TimeNodeState.InProgressEvent;
                else if (maintenanceStatus == (MaintenanceStatus.SingleProcess))
                    state = TimeNodeState.SingleEvent;
                else if (maintenanceStatus == MaintenanceStatus.PendingAfter)
                    state = TimeNodeState.LatestEvent;
            }
            // 無事件的狀態判斷
            else if (!hasEvent && maintenanceStatus == MaintenanceStatus.InProgress)
            {
                state = TimeNodeState.InProgressIDLE;
            }

            _eventCount.text = dateEvents.Count.ToString();
            this.SetEnumValue(state);
        }

        public override EnumVisualMapping SetEnumValue(TimeNodeState enumValue, bool enable = true, bool hideOthers = true)
        {
            _progressBeforeLine.gameObject.SetActive(false);
            _progressAfterLine.gameObject.SetActive(false);
            _eventPoint.gameObject.SetActive(false);
            return base.SetEnumValue(enumValue, enable, false);
        }
    }
}
