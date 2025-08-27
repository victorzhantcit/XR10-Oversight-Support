using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Extensions;

namespace Oversight.Dtos
{
    public class DeviceLifecycleManager : BaseEventOrders
    {
        [JsonIgnore]
        public DateTime QueryStartTime { get; }
        [JsonIgnore]
        public DateTime QueryEndTime { get; }
        public DeviceLifecycleManager(
            List<DeviceEventDto> repairOrders, 
            List<DeviceEventDto> inspOrders, 
            List<DeviceEventDto> workOrders,
            List<DeviceEventDto> alarms,
            DateTime startTime,
            DateTime endTime) 
            : base(repairOrders, inspOrders, workOrders, alarms) 
        {
            QueryStartTime = startTime;
            QueryEndTime = endTime;
        }

        public DeviceEventOrders GetDeviceEventOrders(string deviceCode)
        {
            return new DeviceEventOrders(
                GetDeviceEventsBy(deviceCode, RepairOrders),
                GetDeviceEventsBy(deviceCode, InspOrders),
                GetDeviceEventsBy(deviceCode, WorkOrders),
                GetDeviceEventsBy(deviceCode, Alarms),
                deviceCode
            );
        }

        private List<DeviceEventDto> GetDeviceEventsBy(string deviceCode, IReadOnlyList<DeviceEventDto> source)
        {
            return source.Where(eventData => eventData.DeviceCode == deviceCode).ToList();
        }
    }

    public class DeviceEventOrders : BaseEventOrders
    {
        private string _deviceCode;
        public string DeviceCode => _deviceCode;
        // 每條 Timeline 的日期範圍
        public DateRange RepairDateRange { get; }
        public DateRange InspDateRange { get; }
        public DateRange WorkDateRange { get; }
        public DateRange AlarmDateRange { get; }

        public DeviceEventOrders(
            List<DeviceEventDto> repairOrders,
            List<DeviceEventDto> inspOrders,
            List<DeviceEventDto> workOrders,
            List<DeviceEventDto> alarms,
            string deviceCode
        ) : base(repairOrders, inspOrders, workOrders, alarms)
        {
            _deviceCode = deviceCode;

            // 計算每條 Timeline 的日期範圍
            RepairDateRange = CalculateDateRange(repairOrders);
            InspDateRange = CalculateDateRange(inspOrders);
            WorkDateRange = CalculateDateRange(workOrders);
            AlarmDateRange = CalculateDateRange(alarms);
        }

        private DateRange CalculateDateRange(List<DeviceEventDto> events)
        {
            if (events == null || !events.Any())
            {
                return new DateRange(DateTime.MinValue, DateTime.MinValue);
            }

            var dates = events.Select(e => DateTime.Parse(e.Date)).ToList();
            return new DateRange(dates.Min(), dates.Max());
        }

        public BaseEventOrders GetNodeData(DateTime date)
        {
            string dateFormat = TimeConvert.ToFormattedString(date);

            // 合併所有類型的訂單，並篩選符合條件的事件
            return new BaseEventOrders(
                repairOrders: GetNodeEvents(RepairOrders, dateFormat),
                inspOrders: GetNodeEvents(InspOrders, dateFormat),
                workOrders: GetNodeEvents(WorkOrders, dateFormat),
                alarms: GetNodeEvents(Alarms, dateFormat)
            );
        }

        private List<DeviceEventDto> GetNodeEvents(List<DeviceEventDto> source, string dateFormat)
        {
            return source.Where(eventData => eventData.Date == dateFormat).ToList();
        }

        public List<DeviceEventDto> GetDayEvents(DateTime date)
        {
            string dateFormat = TimeConvert.ToFormattedString(date);

            // 合併所有類型的訂單，並篩選符合條件的事件
            return RepairOrders
                .Concat(InspOrders)
                .Concat(WorkOrders)
                .Concat(Alarms)
                .Where(eventData => eventData.Date == dateFormat && eventData.Status != "Pending" && !IsRepairTaskUnDone(eventData, dateFormat))
                .ToList();
        }

        public BaseEventOrders GetMonthPending(DateTime date)
        {
            return new BaseEventOrders(
                repairOrders: GetMaintMonthPending(date, RepairOrders),
                inspOrders: GetMaintMonthPending(date, InspOrders),
                workOrders: GetMaintMonthPending(date, WorkOrders),
                alarms: GetMaintMonthPending(date, Alarms)
            );
        }

        private List<DeviceEventDto> GetMaintMonthPending(DateTime month, List<DeviceEventDto> source)
        {
            string monthFormat = TimeConvert.ToFormattedString(month, "yyyy-MM");

            // 合併所有類型的訂單，並篩選符合條件的事件
            return source
                .Where(eventData => eventData.Date.StartsWith(monthFormat) && eventData.Status == "Pending" || IsRepairTaskUnDone(eventData, monthFormat))
                .ToList();
        }

        private bool IsRepairTaskUnDone(DeviceEventDto eventData, string dateFormatMatch)
        {
            // 檢查 eventData 是否為 null
            if (eventData == null || string.IsNullOrEmpty(eventData.RecordSn))
                return false;

            // 正則表達式檢查是否符合 R 開頭並且接著全是數字
            string pattern = @"^R\d+$";
            return Regex.IsMatch(eventData.RecordSn, pattern) && eventData.Date.StartsWith(dateFormatMatch) && (eventData.Status == "Changed" || eventData.Status == "Pending");
        }
    }

    public class BaseEventOrders
    {
        [JsonProperty("repairOrders")]
        public List<DeviceEventDto> RepairOrders { get; }
        [JsonProperty("inpsOrders")]
        public List<DeviceEventDto> InspOrders { get; }
        [JsonProperty("workOrders")]
        public List<DeviceEventDto> WorkOrders { get; }
        [JsonProperty("alarms")]
        public List<DeviceEventDto> Alarms { get; }

        public int Count { get; }

        // 無參構造函式，用於 JSON 反序列化
        public BaseEventOrders()
        {
            RepairOrders = new List<DeviceEventDto>();
            InspOrders = new List<DeviceEventDto>();
            WorkOrders = new List<DeviceEventDto>();
            Alarms = new List<DeviceEventDto>();
        }
        
        public BaseEventOrders(
            List<DeviceEventDto> repairOrders,
            List<DeviceEventDto> inspOrders,
            List<DeviceEventDto> workOrders,
            List<DeviceEventDto> alarms)
        {
            RepairOrders = repairOrders ?? new List<DeviceEventDto>();
            InspOrders = inspOrders ?? new List<DeviceEventDto>();
            WorkOrders = workOrders ?? new List<DeviceEventDto>();
            Alarms = alarms ?? new List<DeviceEventDto>();
            Count = RepairOrders.Count + InspOrders.Count + WorkOrders.Count + Alarms.Count;
        }

        public DeviceEventDto GetEventByIndex(int index)
        {
            // 如果索引在 RepairOrders 範圍內
            if (index < RepairOrders.Count)
            {
                return RepairOrders[index];
            }

            // 減去 RepairOrders 的數量，檢查是否在 InspOrders 範圍內
            index -= RepairOrders.Count;
            if (index < InspOrders.Count)
            {
                return InspOrders[index];
            }

            // 減去 InspOrders 的數量，檢查是否在 WorkOrders 範圍內
            index -= InspOrders.Count;
            if (index < WorkOrders.Count)
            {
                return WorkOrders[index];
            }

            // 減去 WorkOrders 的數量，檢查是否在 Alarms 範圍內
            index -= WorkOrders.Count;
            if (index < Alarms.Count)
            {
                return Alarms[index];
            }

            // 如果索引超出範圍，返回 null 或根據需求拋出異常
            return null;
        }

        public List<DeviceEventDto> GetTaskEvents() 
        { 
            return InspOrders.Concat(WorkOrders).Concat(RepairOrders).ToList(); 
        }
    }


    public class DeviceEventDto
    {
        [JsonProperty("recordSn")]
        public string RecordSn;
        [JsonProperty("staff")]
        public string Staff;
        [JsonProperty("date")]
        public string Date;
        [JsonProperty("deviceCode")]
        public string DeviceCode;
        [JsonProperty("content")]
        public string Content;
        [JsonProperty("status")]
        public string Status;
    }

    public class DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }

    public class DeviceLifecycleRequestDto
    {
        public string BuildingCode;
        public string DeviceCode;
        public string StartTime;
        public string EndTime;

        public void Init(string deviceCode, DateTime startTime, DateTime endTime)
        {
            BuildingCode = "RG";
            DeviceCode = deviceCode;
            StartTime = TimeConvert.ToFormattedString(startTime);
            EndTime = TimeConvert.ToFormattedString(endTime);
        }

        public string Print()
        {
            return "Init DeviceLifecycleRequestDto:\n" +
                $"BuildingCode: {BuildingCode}\n" +
                $"DeviceCode: {DeviceCode}\n" +
                $"StartTime: {StartTime}\n" +
                $"EndTime: {EndTime}";
        }
    }
}

