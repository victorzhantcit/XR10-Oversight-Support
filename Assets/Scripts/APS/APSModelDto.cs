using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using MRTK.Extensions;
using Unity.Extensions;

namespace AutodeskPlatformService.Dtos
{
    public class APSDataDto
    {
        [JsonProperty("data")]
        public APSData Data { get; set; }
    }

    public class APSData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("collection")]
        public List<APSModelInfo> Collection { get; set; }
    }

    public class APSModelInfo : IVirtualList
    {
        [JsonProperty("objectid")]
        public int ObjectId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        // Properties �ܬ��r�嵲�c
        [JsonProperty("properties")]
        public Dictionary<string, Dictionary<string, object>> Properties { get; set; }

        // ���ѵ��M��ϥ�
        public List<KeyValue> ToVirtualList()
        {
            List<KeyValue> keyValues = new List<KeyValue>();
            string[] defaultProperties = new string[] { "�ѧO���", "�ؤo", "����", "���Ƥι���" };
            string[] ignoreProperties = new string[] { "��L", "��r" };

            // �u���B�z defaultKeySeries ������
            foreach (var property in defaultProperties)
            {
                if (Properties.TryGetValue(property, out var value) && !ignoreProperties.Contains(property))
                    AppendSection(keyValues, property, value);
            }

            // �B�z��L��]�ư� defaultKeySeries �M ignoreKeys ������^
            var remainingProperties = Properties.Keys
                .Where(key => !defaultProperties.Contains(key) && !ignoreProperties.Contains(key));

            foreach (var property in remainingProperties)
                AppendSection(keyValues, property, Properties[property]);

            return keyValues;
        }

        // �N�ݩʥH�r�����
        private void AppendSection(List<KeyValue> keyValues, string property, Dictionary<string, object> sectionData)
        {
            keyValues.Add(new KeyValue(property, string.Empty)); // �K�[�q�����D

            bool noData = true;
            foreach (var kv in sectionData)
            {
                if (kv.Value == null) continue; // ���L�Ȭ� null ������

                string autoParseValue = kv.Value switch
                {
                    string strValue when !string.IsNullOrEmpty(strValue) => strValue, // ��@�r��B������
                    IEnumerable<string> strArray => string.Join(", ", strArray), // �r��}�C�A�r�����j
                    _ => null // ��L���������A�����L�ļƾ�
                };

                if (string.IsNullOrEmpty(autoParseValue)) continue; // ���L�Ȭ��Ū�����

                noData = false;
                keyValues.Add(new KeyValue(kv.Key, autoParseValue));
            }

            if (noData)
                keyValues.Add(new KeyValue(string.Empty, "�L���"));
        }

    }

    //public class APSModelProperties : IParseString
    //{
    //    [JsonProperty("����")]
    //    public Dictionary<string, object> Constraints { get; set; }

    //    [JsonProperty("���q")]
    //    public Dictionary<string, object> Phases { get; set; }

    //    [JsonProperty("���c")]
    //    public Dictionary<string, object> Structure { get; set; }

    //    [JsonProperty("�ؤo")]
    //    public Dictionary<string, object> Dimensions { get; set; }

    //    [JsonProperty("�ѧO���")]
    //    public Dictionary<string, object> IdentificationData { get; set; }

    //    [JsonProperty("��L")]
    //    public APSDeviceData Others { get; set; }

    //    [JsonProperty("���Ƥι���")]
    //    public Dictionary<string, object> MaterialsAndFinish { get; set; }

    //    public string ParseString()
    //    {
    //        // �ϥ� StringBuilder �Ӱ��ī����r�Ŧ�
    //        var sb = new System.Text.StringBuilder();
    //        AppendSection(sb, "�ѧO���", IdentificationData);
    //        AppendSection(sb, "���q", IdentificationData);
    //        AppendSection(sb, "�ؤo", Dimensions);
    //        AppendSection(sb, "����", Constraints);
    //        AppendSection(sb, "����", MaterialsAndFinish);

    //        return sb.ToString();
    //    }

    //    private void AppendSection(System.Text.StringBuilder sb, string sectionName, Dictionary<string, object> sectionData)
    //    {
    //        sb.AppendLine($"<align=left><color=\"yellow\">{sectionName}<color=\"white\">"); // �K�[�q�����D

    //        if (sectionData?.Count > 0)
    //        {
    //            foreach (var kv in sectionData)
    //            {
    //                string value = kv.Value switch
    //                {
    //                    string strValue => strValue, // ��@�r��
    //                    IEnumerable<string> strArray => string.Join(", ", strArray), // �r��}�C�A�r�����j
    //                    _ => "�����榡" // ��L��������
    //                };

    //                if (string.IsNullOrEmpty(value)) continue;

    //                sb.AppendLine($"\t<align=left>{kv.Key}<line-height=0>\t\t\n" +
    //                    $"<align=right>{value}<line-height=1.5em>");
    //            }
    //        }
    //        else
    //        {
    //            sb.AppendLine("\t�L�ƾ�");
    //        }
    //    }
    //}

    public class APSDeviceData
    {
        [JsonProperty("00DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("01���q�W��")]
        public string CompanyName { get; set; }

        [JsonProperty("02�a��")]
        public string Region { get; set; }

        [JsonProperty("03�ت��W")]
        public string BuildingName { get; set; }

        [JsonProperty("04�Ӽh")]
        public string Floor { get; set; }

        [JsonProperty("05�Ŷ��s��")]
        public string SpaceNumber { get; set; }

        [JsonProperty("06�t��")]
        public string System { get; set; }

        [JsonProperty("08�]�ƦW��")]
        public string DeviceName { get; set; }

        [JsonProperty("09�s��")]
        public string Number { get; set; }

        [JsonProperty("10ID")]
        public string Id { get; set; }
    }
}
