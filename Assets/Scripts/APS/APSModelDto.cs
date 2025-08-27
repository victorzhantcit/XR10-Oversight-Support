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

        // Properties 變為字典結構
        [JsonProperty("properties")]
        public Dictionary<string, Dictionary<string, object>> Properties { get; set; }

        // 提供給清單使用
        public List<KeyValue> ToVirtualList()
        {
            List<KeyValue> keyValues = new List<KeyValue>();
            string[] defaultProperties = new string[] { "識別資料", "尺寸", "約束", "材料及飾面" };
            string[] ignoreProperties = new string[] { "其他", "文字" };

            // 優先處理 defaultKeySeries 中的鍵
            foreach (var property in defaultProperties)
            {
                if (Properties.TryGetValue(property, out var value) && !ignoreProperties.Contains(property))
                    AppendSection(keyValues, property, value);
            }

            // 處理其他鍵（排除 defaultKeySeries 和 ignoreKeys 中的鍵）
            var remainingProperties = Properties.Keys
                .Where(key => !defaultProperties.Contains(key) && !ignoreProperties.Contains(key));

            foreach (var property in remainingProperties)
                AppendSection(keyValues, property, Properties[property]);

            return keyValues;
        }

        // 將屬性以字串拼接
        private void AppendSection(List<KeyValue> keyValues, string property, Dictionary<string, object> sectionData)
        {
            keyValues.Add(new KeyValue(property, string.Empty)); // 添加段落標題

            bool noData = true;
            foreach (var kv in sectionData)
            {
                if (kv.Value == null) continue; // 跳過值為 null 的項目

                string autoParseValue = kv.Value switch
                {
                    string strValue when !string.IsNullOrEmpty(strValue) => strValue, // 單一字串且不為空
                    IEnumerable<string> strArray => string.Join(", ", strArray), // 字串陣列，逗號分隔
                    _ => null // 其他未知類型，視為無效數據
                };

                if (string.IsNullOrEmpty(autoParseValue)) continue; // 跳過值為空的項目

                noData = false;
                keyValues.Add(new KeyValue(kv.Key, autoParseValue));
            }

            if (noData)
                keyValues.Add(new KeyValue(string.Empty, "無資料"));
        }

    }

    //public class APSModelProperties : IParseString
    //{
    //    [JsonProperty("約束")]
    //    public Dictionary<string, object> Constraints { get; set; }

    //    [JsonProperty("階段")]
    //    public Dictionary<string, object> Phases { get; set; }

    //    [JsonProperty("結構")]
    //    public Dictionary<string, object> Structure { get; set; }

    //    [JsonProperty("尺寸")]
    //    public Dictionary<string, object> Dimensions { get; set; }

    //    [JsonProperty("識別資料")]
    //    public Dictionary<string, object> IdentificationData { get; set; }

    //    [JsonProperty("其他")]
    //    public APSDeviceData Others { get; set; }

    //    [JsonProperty("材料及飾面")]
    //    public Dictionary<string, object> MaterialsAndFinish { get; set; }

    //    public string ParseString()
    //    {
    //        // 使用 StringBuilder 來高效拼接字符串
    //        var sb = new System.Text.StringBuilder();
    //        AppendSection(sb, "識別資料", IdentificationData);
    //        AppendSection(sb, "階段", IdentificationData);
    //        AppendSection(sb, "尺寸", Dimensions);
    //        AppendSection(sb, "約束", Constraints);
    //        AppendSection(sb, "材料", MaterialsAndFinish);

    //        return sb.ToString();
    //    }

    //    private void AppendSection(System.Text.StringBuilder sb, string sectionName, Dictionary<string, object> sectionData)
    //    {
    //        sb.AppendLine($"<align=left><color=\"yellow\">{sectionName}<color=\"white\">"); // 添加段落標題

    //        if (sectionData?.Count > 0)
    //        {
    //            foreach (var kv in sectionData)
    //            {
    //                string value = kv.Value switch
    //                {
    //                    string strValue => strValue, // 單一字串
    //                    IEnumerable<string> strArray => string.Join(", ", strArray), // 字串陣列，逗號分隔
    //                    _ => "未知格式" // 其他未知類型
    //                };

    //                if (string.IsNullOrEmpty(value)) continue;

    //                sb.AppendLine($"\t<align=left>{kv.Key}<line-height=0>\t\t\n" +
    //                    $"<align=right>{value}<line-height=1.5em>");
    //            }
    //        }
    //        else
    //        {
    //            sb.AppendLine("\t無數據");
    //        }
    //    }
    //}

    public class APSDeviceData
    {
        [JsonProperty("00DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("01公司名稱")]
        public string CompanyName { get; set; }

        [JsonProperty("02地區")]
        public string Region { get; set; }

        [JsonProperty("03建物名")]
        public string BuildingName { get; set; }

        [JsonProperty("04樓層")]
        public string Floor { get; set; }

        [JsonProperty("05空間編號")]
        public string SpaceNumber { get; set; }

        [JsonProperty("06系統")]
        public string System { get; set; }

        [JsonProperty("08設備名稱")]
        public string DeviceName { get; set; }

        [JsonProperty("09編號")]
        public string Number { get; set; }

        [JsonProperty("10ID")]
        public string Id { get; set; }
    }
}
