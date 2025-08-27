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
            // �ϥΥ��h��F���ǰt [ ] �������e
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
            sb.AppendLine($"<align=left><color=\"yellow\">�򥻸��<color=\"white\">");
            sb.AppendLine(Format("���q�W��", Company));
            sb.AppendLine(Format("�a��", Area));
            sb.AppendLine(Format("�ؿv�W", Building));
            sb.AppendLine(Format("�Ӽh", Floor));
            sb.AppendLine(Format("�Ŷ��s��", Space));
            sb.AppendLine(Format("�t��", System));
            sb.AppendLine(Format("�s��", SerialNumber));
            sb.AppendLine(Format("ID", ElementID));
            return sb.ToString();
        }
    }
}
