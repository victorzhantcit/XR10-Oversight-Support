using System.Globalization;
using System;
using System.Collections.Generic;
using MRTK.Extensions;
using Unity.Extensions;

namespace Oversight.Dtos
{
    /// <summary>
    /// 此 DTO 目前處理的是 Mock 後端回應的格式，尚未接上實際後端回傳資料
    /// </summary>
    public class DeviceInfoDto : IVirtualList
    {
        public string BuildingName { get; set; }
        public string BuildingCode { get; set; }
        public string DeviceCode { get; set; }
        public string Code { get; set; }
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string COBieComponentAssetIdentifier { get; set; }
        public string COBieComponentDescription { get; set; }
        public string COBieComponentInstallationDate { get; set; }
        public string COBieComponentTagName { get; set; }
        public string COBieComponentWarrantyDurationPart { get; set; }
        public string COBieComponentWarrantyDurationUnit { get; set; }
        public string COBieComponentWarrantyGuarantorLabor { get; set; }
        public string COBieComponentWarrantyStartDate { get; set; }
        public string COBieComponentSerialNumber { get; set; }
        public string COBieFloorName { get; set; }
        public string COBieSystemCategory { get; set; }
        public string COBieTypeExpectedLife { get; set; }
        public string COBieTypeName { get; set; }
        public string COBieTypeReplacementCost { get; set; }
        public string COBieTypeAccessibilityPerformance { get; set; }
        public string OtherDocumentInspection { get; set; }

        public List<KeyValue> ToVirtualList()
        {
            var keyValues = new List<KeyValue>();

            keyValues.Add(new KeyValue("維護資訊", string.Empty));
            AddKeyValue(keyValues, "設備名稱", Description);
            AddKeyValue(keyValues, "設備編碼", DeviceCode);
            AddKeyValue(keyValues, "設備簡碼", COBieComponentDescription);
            AddKeyValue(keyValues, "設備類別", COBieTypeName);
            AddKeyValue(keyValues, "樓層", COBieFloorName);
            AddKeyValue(keyValues, "系統", COBieSystemCategory);
            AddKeyValue(keyValues, "模型編碼", DeviceId);

            keyValues.Add(new KeyValue("資產資訊", string.Empty));
            AddKeyValue(keyValues, "產品序號", COBieComponentSerialNumber);
            AddKeyValue(keyValues, "資產辨別碼", COBieComponentAssetIdentifier);
            AddKeyValue(keyValues, "設備售價", COBieTypeReplacementCost);
            AddKeyValue(keyValues, "使用年限", COBieTypeExpectedLife);
            AddKeyValue(keyValues, "無障礙功能", COBieTypeAccessibilityPerformance);
            AddKeyValue(keyValues, "安裝日期", COBieComponentInstallationDate);

            keyValues.Add(new KeyValue("保固相關", string.Empty));
            AddKeyValue(keyValues, "保固開始", COBieComponentWarrantyStartDate);
            AddKeyValue(keyValues, "保固時間", $"{COBieComponentWarrantyDurationPart} {COBieComponentWarrantyDurationUnit}");
            AddKeyValue(keyValues, "保固結束", CalculateWarrantyEndDate());
            AddKeyValue(keyValues, "保固廠商", COBieComponentWarrantyGuarantorLabor);

            return keyValues;
        }

        private void AddKeyValue(List<KeyValue> keyValues, string key, string value)
        {
            keyValues.Add(new KeyValue(key, string.IsNullOrEmpty(value) ? "--" : value));
        }


        private string CalculateWarrantyEndDate()
        {
            // 起始日期解析錯誤
            if (!DateTime.TryParseExact(COBieComponentWarrantyStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var warrantyStartDate))
                return "--";

            // 保固時間是非有效數字
            if (!int.TryParse(COBieComponentWarrantyDurationPart, out var duration))
                return "--";

            // 根據單位進行計算
            DateTime warrantyEndDate;
            switch (COBieComponentWarrantyDurationUnit.Trim().ToLower())
            {
                case "年":
                    warrantyEndDate = warrantyStartDate.AddYears(duration);
                    break;
                case "月":
                    warrantyEndDate = warrantyStartDate.AddMonths(duration);
                    break;
                case "日":
                    warrantyEndDate = warrantyStartDate.AddDays(duration);
                    break;
                default: // Unsupported warranty duration unit: {durationUnit}
                    return "--";
            }

            return warrantyEndDate.ToString("yyyy-MM-dd");
        }
    }
}
