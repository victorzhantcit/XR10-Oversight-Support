using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using MRTK.Extensions;
using Oversight.Dtos;
using Oversight.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Extensions;
using UnityEngine;

public class DeviceMaintenanceView : MonoBehaviour
{
    [SerializeField] private TMP_Text _lifecycleOwner;
    [SerializeField] private TMP_Text _currentTimeLineMonth;
    [SerializeField] private VirtualizedScrollRectList _timeLineList;
    [SerializeField] private Slider _timelineSlider;
    [SerializeField] private TMP_Text _dayEventListTitle;
    [SerializeField] private VirtualizedScrollRectList _dayEventList;
    [SerializeField] private VirtualizedScrollRectList _monthPendingList;
    [SerializeField] private ToggleCollection _monthPendingFilter;
    [SerializeField] private TMP_Text _monthPendingTotal;
    [SerializeField] private TMP_Text _monthPendingWorkOrder;
    [SerializeField] private TMP_Text _monthPendingInspOrder;
    [SerializeField] private TMP_Text _monthPendingRepair;

    private DeviceLifecycleManager _lifecycleManager = null;
    private DeviceEventOrders _deviceEventOrders;
    private List<DeviceEventDto> _dayEvents = new List<DeviceEventDto>();
    private BaseEventOrders _monthPendingEvents;
    private List<DeviceEventDto> _monthPendingTasks;
    private bool _isSliderControllTimeline = false;

    private DialogPoolHandler _dialogPool = null;
    private string _currentDeviceCode;
    private int _cacheMonth = -1;

    private string[] _weekInChinese = new string[] { "日", "一", "二", "三", "四", "五", "六" };

    private void Start()
    {
        _timeLineList.OnVisible = OnTimeLineListVisible;
        _dayEventList.OnVisible = OnDayEventListVisible;
        _monthPendingList.OnVisible = OnMonthPendingListVisible;
    }

    public void Initialize(DeviceLifecycleManager lifecycleManager, DialogPoolHandler dialogPool)
    {
        _lifecycleManager = lifecycleManager;
        _dialogPool = dialogPool;
    }

    public void InitPage()
    {
        _monthPendingTotal.text = "0";
        _monthPendingWorkOrder.text = "0";
        _monthPendingInspOrder.text = "0";
        _monthPendingRepair.text = "0";
        _monthPendingList.SetItemCount(0);
        _monthPendingList.ResetLayout();
        _dayEventList.SetItemCount(0);
        _dayEventList.ResetLayout();
    }

    public void UpdateView(string deviceName, string deviceCode)
    {
        _currentDeviceCode = deviceCode;
        _deviceEventOrders = _lifecycleManager.GetDeviceEventOrders(deviceCode);

        if (_deviceEventOrders == null)
        {
            _dialogPool.EnqueueDialog("查無紀錄!");
            return;
        }

        int maintenanceDays = (_lifecycleManager.QueryEndTime - _lifecycleManager.QueryStartTime).Days + 1;

        UpdateLifecycleMonth(_lifecycleManager.QueryStartTime);
        _lifecycleOwner.text = deviceName;
        _timeLineList.SetItemCount(maintenanceDays);
        _timeLineList.ResetLayout();
        //Debug.Log($"{_lifecycleManager.QueryEndTime} - {_lifecycleManager.QueryStartTime} : {maintenanceDays} Days");
    }

    private void UpdateLifecycleMonth(DateTime date) => _currentTimeLineMonth.text = $"設備紀錄 {date.Year}-{date.Month:D2}";

    public void OnTimeLineListVisible(GameObject target, int index)
    {
        DateTime eventDate = _lifecycleManager.QueryStartTime.AddDays(index);
        //Debug.Log($"{TimeConvert.ToFormattedString(eventDate)} visible");
        if (eventDate > _lifecycleManager.QueryEndTime || eventDate < _lifecycleManager.QueryStartTime)
        {
            Debug.LogError("Device lifecycle range out of bounds.");
            return;
        }

        BaseEventOrders dateEvents = _deviceEventOrders.GetNodeData(eventDate);
        TimeLineListItem item = target.GetComponent<TimeLineListItem>();
        int dateIndex = int.Parse(TimeConvert.ToFormattedString(eventDate, "yyyyMMdd"));

        UpdateLifecycleMonth(eventDate);
        item.SetDayAndWeekOfDay(eventDate.Day, _weekInChinese[(int)eventDate.DayOfWeek]);
        item.SetAlarmNode(dateEvents.Alarms, GetNodeMaintStatus(eventDate, _deviceEventOrders.AlarmDateRange));
        item.SetRepairNode(dateEvents.RepairOrders, GetNodeMaintStatus(eventDate, _deviceEventOrders.RepairDateRange));
        item.SetInspNode(dateEvents.InspOrders, GetNodeMaintStatus(eventDate, _deviceEventOrders.InspDateRange));
        item.SetWorkNode(dateEvents.WorkOrders, GetNodeMaintStatus(eventDate, _deviceEventOrders.WorkDateRange));
        item.SetButtonClickEvent(() => OnDateButtonClicked(eventDate));

        if (!_isSliderControllTimeline)
            _timelineSlider.Value = _timeLineList.Scroll / _timeLineList.MaxScroll * _timelineSlider.MaxValue;
    }

    private MaintenanceStatus GetNodeMaintStatus(DateTime eventDate, DateRange timeline)
    {
        if (timeline.Start == timeline.End)
            return MaintenanceStatus.SingleProcess;
        else if (eventDate > timeline.Start && eventDate < timeline.End)
            return MaintenanceStatus.InProgress;
        else if (eventDate == timeline.Start)
            return MaintenanceStatus.PendingBefore;
        else if (eventDate == timeline.End)
            return MaintenanceStatus.PendingAfter;
        return MaintenanceStatus.PendingAfter;
    }

    private void OnDateButtonClicked(DateTime eventDate)
    {
        _dayEvents = _deviceEventOrders.GetDayEvents(eventDate);
        int eventCount = _dayEvents.Count;
        string countDisplay = eventCount != 0 ? $"{_dayEvents.Count}" : "無";

        _dayEventListTitle.text = $"當日工作總覽 ({countDisplay})";
        _dayEventList.SetItemCount(_dayEvents.Count);
        _dayEventList.ResetLayout();

        if (_cacheMonth == eventDate.Month)
            return;

        _cacheMonth = eventDate.Month;
        _monthPendingEvents = _deviceEventOrders.GetMonthPending(eventDate);
        _monthPendingTotal.text = $"{_monthPendingEvents.Count}";
        _monthPendingWorkOrder.text = $"{_monthPendingEvents.WorkOrders.Count}";
        _monthPendingInspOrder.text = $"{_monthPendingEvents.InspOrders.Count}";
        _monthPendingRepair.text = $"{_monthPendingEvents.RepairOrders.Count}";
        _monthPendingFilter.SetSelection(0);
        _monthPendingTasks = _monthPendingEvents.GetTaskEvents();
        _monthPendingList.SetItemCount(_monthPendingTasks.Count);
        _monthPendingList.ResetLayout();
    }

    public void OnDayEventListVisible(GameObject target, int index)
    {
        DayEventListItem item = target.GetComponent<DayEventListItem>();
        DeviceEventDto dayEvent = _dayEvents[index];
        item.SetContent(dayEvent);
        item.SetButtonClickEvent(() =>
        {
            _dialogPool.EnqueueDialog(item.GetHeader(), item.GetParseDetail());
        });
    }

    public void OnMonthPendingListVisible(GameObject target, int index)
    {
        DayEventListItem item = target.GetComponent<DayEventListItem>();
        item.SetContent(_monthPendingTasks[index]);
        item.SetButtonClickEvent(() =>
        {
            _dialogPool.EnqueueDialog(item.GetHeader(), item.GetParseDetail());
        });
    }

    public void SwitchToAllPending() => SwitchPendingTarget(_monthPendingEvents?.GetTaskEvents());
    public void SwitchToInspPending() => SwitchPendingTarget(_monthPendingEvents?.InspOrders);
    public void SwitchToWorkerPending() => SwitchPendingTarget(_monthPendingEvents?.WorkOrders);
    public void SwitchToRepairPending() => SwitchPendingTarget(_monthPendingEvents?.RepairOrders);

    private void SwitchPendingTarget(List<DeviceEventDto> targetEvents)
    {
        if (targetEvents == null) return;

        _monthPendingTasks = targetEvents;
        _monthPendingList.SetItemCount(_monthPendingTasks.Count);
        _monthPendingList.ResetLayout();
    }

    private void OnMonthSliderUpdated(SliderEventData eventData)
    {
        if (_deviceEventOrders == null) return;

        int value = Mathf.RoundToInt(eventData.NewValue);
        DateTime jumpToMonth = TimeConvert.GetFirstDayOfMonth(_lifecycleManager.QueryStartTime.AddMonths(value));

        UpdateLifecycleMonth(jumpToMonth);
        _timeLineList.Scroll = (jumpToMonth.Date - _lifecycleManager.QueryStartTime).Days;
        //Debug.Log(_currentTimeLineMonth.text);
    }

    public void OnMonthSliderHoverEntered(Single time)
    {
        _isSliderControllTimeline = true;
        _timelineSlider.OnValueUpdated.RemoveListener(OnMonthSliderUpdated);
        _timelineSlider.OnValueUpdated.AddListener(OnMonthSliderUpdated);
    }

    public void OnMonthSliderHoverExited(Single time)
    {
        _timelineSlider.OnValueUpdated.RemoveListener(OnMonthSliderUpdated);
        _isSliderControllTimeline = false;
    }
}
