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
        // �C�� Timeline ������d��
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

            // �p��C�� Timeline ������d��
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

            // �X�֩Ҧ��������q��A�ÿz��ŦX���󪺨ƥ�
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

            // �X�֩Ҧ��������q��A�ÿz��ŦX���󪺨ƥ�
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

            // �X�֩Ҧ��������q��A�ÿz��ŦX���󪺨ƥ�
            return source
                .Where(eventData => eventData.Date.StartsWith(monthFormat) && eventData.Status == "Pending" || IsRepairTaskUnDone(eventData, monthFormat))
                .ToList();
        }

        private bool IsRepairTaskUnDone(DeviceEventDto eventData, string dateFormatMatch)
        {
            // �ˬd eventData �O�_�� null
            if (eventData == null || string.IsNullOrEmpty(eventData.RecordSn))
                return false;

            // ���h��F���ˬd�O�_�ŦX R �}�Y�åB���ۥ��O�Ʀr
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

        // �L�Ѻc�y�禡�A�Ω� JSON �ϧǦC��
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
            // �p�G���ަb RepairOrders �d��
            if (index < RepairOrders.Count)
            {
                return RepairOrders[index];
            }

            // ��h RepairOrders ���ƶq�A�ˬd�O�_�b InspOrders �d��
            index -= RepairOrders.Count;
            if (index < InspOrders.Count)
            {
                return InspOrders[index];
            }

            // ��h InspOrders ���ƶq�A�ˬd�O�_�b WorkOrders �d��
            index -= InspOrders.Count;
            if (index < WorkOrders.Count)
            {
                return WorkOrders[index];
            }

            // ��h WorkOrders ���ƶq�A�ˬd�O�_�b Alarms �d��
            index -= WorkOrders.Count;
            if (index < Alarms.Count)
            {
                return Alarms[index];
            }

            // �p�G���޶W�X�d��A��^ null �ήھڻݨD�ߥX���`
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

