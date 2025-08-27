using Newtonsoft.Json;
using Oversight.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Extensions;
using User.Core;

namespace RepairOrder.Dtos
{
    [Serializable]
    public class RepairOrderUploadDto
    {
        public string BuildingCode = string.Empty;
        public string DeviceType = string.Empty;
        public string DeviceCode = string.Empty;
        public string DeviceDescription = string.Empty;
        public string Issuer = string.Empty;
        public string Department = string.Empty;
        public string Tel = string.Empty;
        public string Issue = string.Empty;
        public List<string> PhotoList = new List<string>();

        public string BuildingName;
        public string CreateTime;
        private const string OTHER_TYPE = "Other";

        [JsonConstructor] public RepairOrderUploadDto() { }

        /// <summary>
        /// 其他類 <seealso cref="deviceType"/> 設為 null
        /// </summary>
        /// <param name="note">以筆記轉工單的 reference</param>
        /// <param name="deviceType">其他類以 null 表示</param>
        public RepairOrderUploadDto(NoteDto note, string deviceType, string deviceCode)
        {
            BuildingCode = note.BuildingCode;
            DeviceType = !string.IsNullOrEmpty(deviceType) ? deviceType : OTHER_TYPE;
            if (string.IsNullOrEmpty(note.CodeOfDevice) || string.IsNullOrEmpty(deviceType))
                DeviceDescription = $"{note.Location} "; // 其他類或設備未明確定義
            DeviceDescription += note.CodeOfDevice;
            DeviceCode = !string.IsNullOrEmpty(deviceCode) ? deviceCode : string.Empty;
            Issuer = SecureDataManager.GetLoggedInUserName();
            Issue = note.Description;
            PhotoList = note.PhotoBase64;
            BuildingName = note.BuildingName;
            CreateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public string GetInfoFormatString()
        {
            string Format(string key, string value) => $"{key}<indent=40><color=grey>{value}</indent></color>";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Format("路段", BuildingName));
            sb.AppendLine(Format("報修時間", TimeConvert.ToFormattedString(DateTime.Now, "yyyy-MM-dd hh:mm:ss")));
            sb.AppendLine(Format("報修人員", Issuer));
            sb.AppendLine(Format("系統", DeviceType == OTHER_TYPE ? "其他" : DeviceType));
            sb.AppendLine(Format(DeviceType == OTHER_TYPE ? "報修地點" : "設備名稱", DeviceDescription));

            return sb.ToString();
        }

        public void AddPhoto(string photoBase64)
        {
            PhotoList.Add(photoBase64);
        }

        public void RemovePhoto(int indexOfPhoto)
        {
            PhotoList.RemoveAt(indexOfPhoto);
        }
    }
}
