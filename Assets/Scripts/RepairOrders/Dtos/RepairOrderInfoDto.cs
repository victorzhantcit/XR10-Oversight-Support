using Newtonsoft.Json;
using System;
using System.Text;

namespace RepairOrder.Dtos
{
    [Serializable]
    public class RepairOrderInfoDto
    {
        [JsonProperty("repairOrder")]
        public RepairOrderDetail RepairOrder;

        [JsonProperty("buildingName")]
        public string BuildingName;

        [JsonProperty("repairRecord")]
        public RepairRecordData RepairRecord;

        public string GetDetailFormatString()
        {
            StringBuilder sb = new StringBuilder();
            string descriptionTag = RepairOrder.DeviceType == "Other" ? "報修地點" : "設備名稱";

            sb.AppendLine(Format("報修單號", RepairOrder.RecordSn));
            sb.AppendLine("進度"); // 非純字樣設計 值由其他地方實現
            sb.AppendLine(Format("路段", BuildingName));
            sb.AppendLine(Format("報修時間", GetTimeFormat(RepairOrder.CreateTime)));
            sb.AppendLine(Format("報修人員", RepairOrder.IssuerName));
            sb.AppendLine(Format("連絡電話", RepairOrder.Tel));
            sb.AppendLine(Format("報修單位", RepairOrder.Department));
            sb.AppendLine(Format("報修異常類別", RepairOrder.DeviceType));
            sb.AppendLine(Format(descriptionTag, RepairOrder.DeviceDescription));
            sb.AppendLine(Format("異常描述", RepairOrder.Issue));
            sb.AppendLine(Format("完成時間", GetTimeFormat(RepairOrder.CompleteTime)));
            sb.AppendLine(Format("處理方式", RepairOrder.Reply));
            return sb.ToString();
        }

        private string GetTimeFormat(string timeFormat)
        {
            if (DateTime.TryParse(timeFormat, out DateTime parsedDateTime))
                return parsedDateTime.ToString("yyyy-MM-dd hh:mm:ss");
            else
                return null;
        }
            

        private string Format(string key, string value) 
            => $"{key}<indent=60>{(!string.IsNullOrEmpty(value) ? value : "--")}</indent>";

        public class RepairOrderDetail
        {
            [JsonProperty("sn")]
            public int Sn;

            [JsonProperty("buildingCode")]
            public string BuildingCode;

            [JsonProperty("recordSn")]
            public string RecordSn;

            [JsonProperty("deviceType")]
            public string DeviceType;

            [JsonProperty("deviceCode")]
            public string DeviceCode;

            [JsonProperty("code")]
            public string Code;

            [JsonProperty("deviceDescription")]
            public string DeviceDescription;

            [JsonProperty("issuer")]
            public string Issuer;

            [JsonProperty("issuerName")]
            public string IssuerName;

            [JsonProperty("department")]
            public string Department;

            [JsonProperty("tel")]
            public string Tel;

            [JsonProperty("email")]
            public string Email;

            [JsonProperty("createTime")]
            public string CreateTime;

            [JsonProperty("photoSns")]
            public string PhotoSns;

            [JsonProperty("issue")]
            public string Issue;

            [JsonProperty("status")]
            public string Status;

            [JsonProperty("scheduledDate")]
            public string ScheduledDate;

            [JsonProperty("staff")]
            public string Staff;

            [JsonProperty("reply")]
            public string Reply;

            [JsonProperty("replyTime")]
            public string ReplyTime;

            [JsonProperty("workSn")]
            public string WorkSn;

            [JsonProperty("rphotoSns")]
            public string RphotoSns;

            [JsonProperty("signPhoto")]
            public string SignPhoto;

            [JsonProperty("dutyUnit")]
            public string DutyUnit;

            [JsonProperty("payUnit")]
            public string PayUnit;

            [JsonProperty("payAmount")]
            public string PayAmount;

            [JsonProperty("processTime")]
            public string ProcessTime;

            [JsonProperty("completeTime")]
            public string CompleteTime;
        }

        public class RepairRecordData
        {
            [JsonProperty("sn")]
            public int Sn;

            [JsonProperty("buildingCode")]
            public string BuildingCode;

            [JsonProperty("recordSn")]
            public string RecordSn;

            [JsonProperty("deviceCode")]
            public string DeviceCode;

            [JsonProperty("staff")]
            public string Staff;

            [JsonProperty("reply")]
            public string Reply;

            [JsonProperty("scheduledDate")]
            public string ScheduledDate;

            [JsonProperty("photoSns")]
            public string PhotoSns;

            [JsonProperty("replyTime")]
            public string ReplyTime;

            [JsonProperty("status")]
            public string Status;

            [JsonProperty("signPhoto")]
            public string SignPhoto;
        }
    }
}