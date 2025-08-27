using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Oversight.Dtos
{
    public class BIMDataDto
    {
        public string GameObjectName { get; private set; }
        public string DeviceID { get; private set; }
        public string ElementID { get; private set; }
        public string Company => GetData(0);
        public string Area => GetData(1);
        public string Building => GetData(2);
        public string Floor => GetData(3);
        public string Space => GetData(4);
        public string System => GetData(5);
        public string DeviceName => GetData(6);
        public string SerialNumber => GetData(7);
        public string ID => GetData(8);

        private string[] _infoList = new string[10];

        public BIMDataDto(string objectName, bool onlyID = false)
        {
            if (onlyID)
            {
                DeviceID = objectName;
                _infoList = objectName.Split('+');
            }
            else
            {
                List<string> tags = ParseName(objectName);
                GameObjectName = objectName.Substring(0, objectName.IndexOf('['));
                ElementID = tags[0];
                DeviceID = tags[1];
                _infoList = tags[1].Split('+');
            }
        }

        private List<string> ParseName(string objectName)
        {
            // 使用正則表達式匹配 [ ] 內的內容
            Regex regex = new Regex(@"\[(.*?)\]");
            MatchCollection matches = regex.Matches(objectName);

            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    results.Add(match.Groups[1].Value);
                }
            }

            return results;
        }

        private string GetData(int index)
        {
            if (index >= 0 & index < _infoList.Length)
                return _infoList[index];
            return string.Empty;
        }

        // IParseString
        public string ParseString()
        {
            string Format(string label, string value) 
                => $"\t<align=left>{label}<line-height=0>\n" +
                $"<align=right>{value}<line-height=1.5em>";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<align=left><color=\"yellow\">基本資料<color=\"white\">");
            sb.AppendLine(Format("公司名稱", Company));
            sb.AppendLine(Format("地區", Area));
            sb.AppendLine(Format("建築名", Building));
            sb.AppendLine(Format("樓層", Floor));
            sb.AppendLine(Format("空間編號", Space));
            sb.AppendLine(Format("系統", System));
            sb.AppendLine(Format("編號", SerialNumber));
            sb.AppendLine(Format("ID", ElementID));
            return sb.ToString();
        }
    }
}
