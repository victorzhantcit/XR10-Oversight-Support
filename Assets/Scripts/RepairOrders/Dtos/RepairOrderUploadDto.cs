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
        /// ��L�� <seealso cref="deviceType"/> �]�� null
        /// </summary>
        /// <param name="note">�H���O��u�檺 reference</param>
        /// <param name="deviceType">��L���H null ���</param>
        public RepairOrderUploadDto(NoteDto note, string deviceType, string deviceCode)
        {
            BuildingCode = note.BuildingCode;
            DeviceType = !string.IsNullOrEmpty(deviceType) ? deviceType : OTHER_TYPE;
            if (string.IsNullOrEmpty(note.CodeOfDevice) || string.IsNullOrEmpty(deviceType))
                DeviceDescription = $"{note.Location} "; // ��L���γ]�ƥ����T�w�q
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

            sb.AppendLine(Format("���q", BuildingName));
            sb.AppendLine(Format("���׮ɶ�", TimeConvert.ToFormattedString(DateTime.Now, "yyyy-MM-dd hh:mm:ss")));
            sb.AppendLine(Format("���פH��", Issuer));
            sb.AppendLine(Format("�t��", DeviceType == OTHER_TYPE ? "��L" : DeviceType));
            sb.AppendLine(Format(DeviceType == OTHER_TYPE ? "���צa�I" : "�]�ƦW��", DeviceDescription));

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
