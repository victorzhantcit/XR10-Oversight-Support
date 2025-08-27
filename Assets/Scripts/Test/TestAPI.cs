using AutodeskPlatformService.Core;
using Newtonsoft.Json;
using Oversight.Core;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Extensions;
using UnityEngine;

public class TestAPI : MonoBehaviour
{
    //public string API;
    //public string Content;
    //public TMP_Text Result;
    //public TMP_Text Ping;

    //[SerializeField] private ServiceIBMS _server;
    //private AutoDeskAPI _autoDeskAPI;

    //private void Start()
    //{
    //    Ping.text = "0";
    //    string datetime = "2024-12-09T16:41:41";
    //    string dayTimePattern = @"\d{2}:\d{2}:\d{2}";

    //    Match match = Regex.Match(datetime, dayTimePattern);

    //    Debug.Log(match.Value);
    //}

    //private void Update()
    //{
    //    Ping.text = $"{int.Parse(Ping.text) + 1}";
    //}

    //public void GetAPSData()
    //{
    //    _server.GetAPSModelData(apsModelInfos =>
    //    {
    //        if (apsModelInfos == null)
    //            Debug.Log($"Error when load APS model info file");
    //        else
    //        {
    //            foreach (var kvp in apsModelInfos)
    //            {
    //                Debug.Log($"Key: {kvp.Key}, {PrintDetails(kvp.Value)}");
    //            }
    //            Debug.Log(_apsModelInfos["4743480"].Name);
    //        }
    //    });
    //}

    //public void GetDeviceLifecycle()
    //{
    //    _ = GetDeviceLifecycleAsync(data =>
    //    {
    //        DateTime targetDateTime = new DateTime(2024, 12, 6);
    //        var list = data.WorkOrdersMaintenances.GetMaintenanceByDate(targetDateTime);

    //        Debug.Log($"{targetDateTime:yyyy-MM-dd} WorkOrders Daily Tasks: {list.Count}");
    //    });
    //}

    //private async Task GetDeviceLifecycleAsync(Action<DeviceEventDto> callback)
    //{
    //    try
    //    {
    //        DeviceLifecycleRequestDto request = JsonConvert.DeserializeObject<DeviceLifecycleRequestDto>(Content);
    //        await _server.GetDeviceLifecycleAsync(request, responseData =>
    //        {
    //            DeviceEventDto deviceLifecycle = new DeviceEventDto();
    //            deviceLifecycle.Init(
    //                responseData,
    //                TimeConvert.ToDateTime(request.StartTime),
    //                TimeConvert.ToDateTime(request.EndTime)
    //            );
    //            callback?.Invoke(deviceLifecycle);
    //        });

    //        Debug.Log("GetDeviceLifecycle Success!");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogWarning($"Failed to get device lifecycle:\n{ex}");
    //        callback?.Invoke(null);
    //        return;
    //    }
    //}

    /// <summary>
    /// 遞迴打印對象的所有屬性和值
    /// </summary>
    /// <param name = "obj" > 要打印的對象 </ param >
    /// < param name="indentLevel">縮排級別</param>
    //private string PrintDetails(object obj)
    //{
    //    if (obj == null)
    //    {
    //        return "null\n";
    //    }

    //    var result = new System.Text.StringBuilder();
    //    var queue = new Queue<(object currentObject, int level, string parentName)>(); // 使用隊列模擬層次結構
    //    queue.Enqueue((obj, 0, ""));

    //    while (queue.Count > 0)
    //    {
    //        var (currentObject, level, parentName) = queue.Dequeue();

    //        if (currentObject == null)
    //        {
    //            result.AppendLine($"{new string(' ', level * 2)}{parentName}: null");
    //            continue;
    //        }

    //        var type = currentObject.GetType();
    //        if (type.IsPrimitive || currentObject is string) // 基本類型和字符串直接打印
    //        {
    //            result.AppendLine($"{new string(' ', level * 2)}{parentName}: {currentObject}");
    //            continue;
    //        }

    //        遍歷屬性
    //        foreach (var property in type.GetProperties())
    //        {
    //            try
    //            {
    //                var value = property.GetValue(currentObject, null);
    //                if (value == null)
    //                {
    //                    result.AppendLine($"{new string(' ', level * 2)}{property.Name}: null");
    //                }
    //                else if (property.PropertyType.IsPrimitive || value is string) // 基本類型直接打印
    //                {
    //                    result.AppendLine($"{new string(' ', level * 2)}{property.Name}: {value}");
    //                }
    //                else // 對於非基本類型，加入隊列進一步處理
    //                {
    //                    result.AppendLine($"{new string(' ', level * 2)}{property.Name}:");
    //                    queue.Enqueue((value, level + 1, property.Name));
    //                }
    //            }
    //            catch (System.Exception ex)
    //            {
    //                result.AppendLine($"{new string(' ', level * 2)}{property.Name}: [Error: {ex.Message}]");
    //            }
    //        }
    //    }

    //    return result.ToString();
    //}

    //public void Post()
    //{
    //    PostAsync();
    //}

    //private async void PostAsync()
    //{
    //    var response = await APIHelper.SendJsonRequestAsync<string>(
    //        API,
    //        APIHelper.HttpMethod.POST,
    //        data: Content
    //    );

    //    if (response.IsSuccess)
    //    {
    //        Result.text = $"{API}\n" +
    //            $"{Content}\n" +
    //            $"----------------\n" +
    //            $"Result\n{PrintJson(response.Data)}";
    //    }
    //    else
    //    {
    //        Result.text = $"{API}\n" +
    //            $"{Content}\n" +
    //            $"----------------\n" +
    //            $"Result\n Error (See in console)";
    //        Debug.LogWarning($"Error fetching device info: {response.ErrorMessage}");
    //    }
    //}

    //private string PrintJson(string jsonString)
    //{
    //    var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
    //    StringBuilder stringBuilder = new StringBuilder();
    //    foreach (var keyValuePair in jsonObject)
    //    {
    //        if (keyValuePair.Value is Newtonsoft.Json.Linq.JArray)
    //        {
    //            stringBuilder.Append($"{keyValuePair.Key}:");
    //            foreach (var item in (Newtonsoft.Json.Linq.JArray)keyValuePair.Value)
    //            {
    //                stringBuilder.Append($"- {item}\n");
    //            }
    //        }
    //        else
    //        {
    //            stringBuilder.Append($"{keyValuePair.Key}: {keyValuePair.Value}\n");
    //        }
    //    }

    //    return stringBuilder.ToString();
    //}

    //public void Get()
    //{
    //    GetAsync();
    //}

    //public void Get()
    //{
    //    _ = APIHelper.SendRequest<string>(
    //        API,
    //        APIHelper.HttpMethod.GET,
    //        jsonData: null, // GET 請求不需要 JSON 資料
    //        token: null,    // 不需要授權 Token
    //        onSuccess: response =>
    //        {
    //            if (response != null)
    //            {
    //                Result.text = $"{API}\n" +
    //                    $"{Content}\n" +
    //                    $"----------------\n" +
    //                    $"Result\n{PrintJson(response)}";
    //            }
    //            else
    //            {
    //                Result.text = $"{API}\n" +
    //                    $"{Content}\n" +
    //                    $"----------------\n" +
    //                    $"Result\n Error (See in console)";
    //            }
    //        },
    //        onError: error =>
    //        {
    //            Debug.LogWarning($"Error fetching device info: {error}");
    //        }
    //    );
    //}
}
