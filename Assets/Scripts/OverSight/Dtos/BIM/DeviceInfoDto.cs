using System.Globalization;
using System;
using System.Collections.Generic;
using MRTK.Extensions;
using Unity.Extensions;

namespace Oversight.Dtos
{
    /// <summary>
    /// �� DTO �ثe�B�z���O Mock ��ݦ^�����榡�A�|�����W��ګ�ݦ^�Ǹ��
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

            keyValues.Add(new KeyValue("���@��T", string.Empty));
            AddKeyValue(keyValues, "�]�ƦW��", Description);
            AddKeyValue(keyValues, "�]�ƽs�X", DeviceCode);
            AddKeyValue(keyValues, "�]��²�X", COBieComponentDescription);
            AddKeyValue(keyValues, "�]�����O", COBieTypeName);
            AddKeyValue(keyValues, "�Ӽh", COBieFloorName);
            AddKeyValue(keyValues, "�t��", COBieSystemCategory);
            AddKeyValue(keyValues, "�ҫ��s�X", DeviceId);

            keyValues.Add(new KeyValue("�겣��T", string.Empty));
            AddKeyValue(keyValues, "���~�Ǹ�", COBieComponentSerialNumber);
            AddKeyValue(keyValues, "�겣��O�X", COBieComponentAssetIdentifier);
            AddKeyValue(keyValues, "�]�ư��", COBieTypeReplacementCost);
            AddKeyValue(keyValues, "�ϥΦ~��", COBieTypeExpectedLife);
            AddKeyValue(keyValues, "�L��ê�\��", COBieTypeAccessibilityPerformance);
            AddKeyValue(keyValues, "�w�ˤ��", COBieComponentInstallationDate);

            keyValues.Add(new KeyValue("�O�T����", string.Empty));
            AddKeyValue(keyValues, "�O�T�}�l", COBieComponentWarrantyStartDate);
            AddKeyValue(keyValues, "�O�T�ɶ�", $"{COBieComponentWarrantyDurationPart} {COBieComponentWarrantyDurationUnit}");
            AddKeyValue(keyValues, "�O�T����", CalculateWarrantyEndDate());
            AddKeyValue(keyValues, "�O�T�t��", COBieComponentWarrantyGuarantorLabor);

            return keyValues;
        }

        private void AddKeyValue(List<KeyValue> keyValues, string key, string value)
        {
            keyValues.Add(new KeyValue(key, string.IsNullOrEmpty(value) ? "--" : value));
        }


        private string CalculateWarrantyEndDate()
        {
            // �_�l����ѪR���~
            if (!DateTime.TryParseExact(COBieComponentWarrantyStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var warrantyStartDate))
                return "--";

            // �O�T�ɶ��O�D���ļƦr
            if (!int.TryParse(COBieComponentWarrantyDurationPart, out var duration))
                return "--";

            // �ھڳ��i��p��
            DateTime warrantyEndDate;
            switch (COBieComponentWarrantyDurationUnit.Trim().ToLower())
            {
                case "�~":
                    warrantyEndDate = warrantyStartDate.AddYears(duration);
                    break;
                case "��":
                    warrantyEndDate = warrantyStartDate.AddMonths(duration);
                    break;
                case "��":
                    warrantyEndDate = warrantyStartDate.AddDays(duration);
                    break;
                default: // Unsupported warranty duration unit: {durationUnit}
                    return "--";
            }

            return warrantyEndDate.ToString("yyyy-MM-dd");
        }
    }
}
