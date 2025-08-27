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
            string descriptionTag = RepairOrder.DeviceType == "Other" ? "���צa�I" : "�]�ƦW��";

            sb.AppendLine(Format("���׳渹", RepairOrder.RecordSn));
            sb.AppendLine("�i��"); // �D�¦r�˳]�p �ȥѨ�L�a���{
            sb.AppendLine(Format("���q", BuildingName));
            sb.AppendLine(Format("���׮ɶ�", GetTimeFormat(RepairOrder.CreateTime)));
            sb.AppendLine(Format("���פH��", RepairOrder.IssuerName));
            sb.AppendLine(Format("�s���q��", RepairOrder.Tel));
            sb.AppendLine(Format("���׳��", RepairOrder.Department));
            sb.AppendLine(Format("���ײ��`���O", RepairOrder.DeviceType));
            sb.AppendLine(Format(descriptionTag, RepairOrder.DeviceDescription));
            sb.AppendLine(Format("���`�y�z", RepairOrder.Issue));
            sb.AppendLine(Format("�����ɶ�", GetTimeFormat(RepairOrder.CompleteTime)));
            sb.AppendLine(Format("�B�z�覡", RepairOrder.Reply));
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