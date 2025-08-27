using MRTK.Extensions;
using Oversight.Dtos;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayEventListItem : VirtualListItem<DeviceEventDto>
{
    [SerializeField] private TMP_Text _eventTypeLabel;
    [SerializeField] private TMP_Text _headerLabel;
    [SerializeField] private TMP_Text _conetentLabel;
    [SerializeField] private Image _itemBackPlate;
    [SerializeField] private Image _eventBackPlate;
    private Action _onClicked;
    private DeviceEventDto _dayEvent;

    public override void SetContent(DeviceEventDto dayEvent, int index = -1, bool interactable = true)
    {
        _dayEvent = dayEvent;
        SetEventType(dayEvent.Content, dayEvent.RecordSn);
        _conetentLabel.text = dayEvent.Content;
        _headerLabel.text = $"{dayEvent.RecordSn} {GetStatusName(dayEvent.Status)}\n{GetDayTime(dayEvent.DeviceCode)}";
    }

    private void SetEventType(string content, string title)
    {
        Color backPlateColor = Color.white;
        Color tagTextColor = Color.white;
        string eventType = string.Empty;

        if (content == "Alarm")
        {
            backPlateColor = new Color(1f, 0.2783019f, 0.2783019f); // 告警顏色
            eventType = "告警";
        }
        else
        {
            switch (title[0])
            {
                case 'R':
                    eventType = "報修";
                    backPlateColor = new Color(1f, 1f, 0.004716992f); // 報修顏色
                    tagTextColor = Color.black;
                    break;
                case 'I':
                    eventType = "巡檢";
                    backPlateColor = new Color(0.1921569f, 0.764706f, 0.7960785f); // 巡檢顏色
                    break;
                case 'W':
                    eventType = "工單";
                    backPlateColor = new Color(0.909804f, 0.2901961f, 0.6117647f); // 工單顏色
                    break;
                default:
                    break;
            }
        }
        _eventTypeLabel.text = eventType;
        _eventTypeLabel.color = tagTextColor;
        _headerLabel.color = backPlateColor;
        _itemBackPlate.color = backPlateColor;
        _eventBackPlate.color = backPlateColor;
    }

    private string GetStatusName(string statusCode)
    {
        return statusCode switch
        {
            "Pending" => "待處理",
            "Changed" => "轉工單",
            "Processing" => "處理中",
            "Submitted" => "完工上傳",
            "Done" => "已完成",
            "Approving" => "覆核中",
            "Reject" => "退件",
            "Completed" => "已完成",
            "NotUploaded" => "等待上傳",
            "Pause" => "已暫結",
            _ => string.Empty
        };
    }

    private string GetDayTime(string date)
    {
        string dayPattern = @"\d{2}:\d{2}:\d{2}";
        Match match = Regex.Match(date, dayPattern);

        if (match.Success) return match.Value;
        return string.Empty;
    }

    public void SetButtonClickEvent(Action action) => _onClicked = action;

    public void OnButtonClicked() => _onClicked?.Invoke();

    public string GetHeader()
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(_eventBackPlate.color)}>{_eventTypeLabel.text}</color> {_dayEvent.RecordSn}";
    } 

    public string GetParseDetail()
    {
        string Format(string label, string value)
            => $"<align=left>{label}<line-height=0>\n" +
            $"<align=right>{value}<line-height=1.5em>";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Format("時間", _dayEvent.Date));
        sb.AppendLine(Format("建立/執行人員", _dayEvent.Staff));
        sb.AppendLine(Format("狀態", GetStatusName(_dayEvent.Status)));
        sb.AppendLine($"<align=left>內容\t\t{_dayEvent.Content}");
        return sb.ToString();
    }
}
