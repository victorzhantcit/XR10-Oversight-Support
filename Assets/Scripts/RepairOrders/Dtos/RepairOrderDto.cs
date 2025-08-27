using Newtonsoft.Json;
using System;

namespace RepairOrder.Dtos
{
    [Serializable]
    public class RepairOrderDto
    {
        [JsonProperty("sn")]
        public string Sn;
        [JsonProperty("city")]
        public string City;
        [JsonProperty("district")]
        public string District;
        [JsonProperty("recordSn")]
        public string RecordSn;
        [JsonProperty("createTime")]
        public string CreateTime;
        [JsonProperty("buildingCode")]
        public string BuildingCode;
        [JsonProperty("buildingName")]
        public string BuildingName;
        [JsonProperty("deviceType")]
        public string DeviceType;
        [JsonProperty("issue")]
        public string Issue;
        [JsonProperty("status")]
        public string Status;
        [JsonProperty("workSn")]
        public string WorkSn;
        [JsonProperty("completeTime")]
        public string CompleteTime;

        [JsonConstructor] public RepairOrderDto() { }

        public RepairOrderDto(RepairOrderUploadDto uploadDto)
        {
            Status = "NotUpload";
            RecordSn = string.Empty;
            BuildingCode = uploadDto.BuildingCode;
            BuildingName = uploadDto.BuildingName;
            DeviceType = uploadDto.DeviceType;
            Issue = uploadDto.Issue;
            CreateTime = uploadDto.CreateTime;
        }
    }
}
