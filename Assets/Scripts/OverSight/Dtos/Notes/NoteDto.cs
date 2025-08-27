using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Oversight.Dtos
{
    public class NoteDto
    {
        public string Id = string.Empty;
        public string Time = string.Empty;
        public string Location = string.Empty;
        public string CodeOfDevice { get; set; } = string.Empty; // 設備的Code
        public string Description { get; set; } = string.Empty; // 說明、描述
        public List<string> PhotoBase64 = new List<string>();

        public string BuildingCode = string.Empty;
        public string BuildingName = string.Empty;
        public string Issuer = string.Empty;
        public bool WaitForUploadToRepair = false;
        public DateTime CreateTime;

        [JsonConstructor] public NoteDto() { }

        public NoteDto(string buildingCode, string buildingName, string location, string issuer)
        {
            BuildingCode = buildingCode;
            BuildingName = buildingName;
            Location = location;
            Issuer = issuer;

            CreateTime = DateTime.Now;
            Time = CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            Id = $"N{CreateTime.ToString("yyyyMMddHHmmss")}";
        }

        public void AddPhoto(string photo)
        {
            PhotoBase64.Add(photo);
        }
    }

}
