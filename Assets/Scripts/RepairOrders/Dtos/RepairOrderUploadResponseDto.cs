using Newtonsoft.Json;

namespace RepairOrder.Dtos
{
    public class RepairOrderUploadResponseDto
    {
        [JsonProperty("sn")]
        public string Sn;
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
}
