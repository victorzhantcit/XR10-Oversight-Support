using AutodeskPlatformService.Dtos;
using Newtonsoft.Json;
using Oversight.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace AutodeskPlatformService.Core
{
    public class AutoDeskAPI : MonoBehaviour
    {
        [SerializeField] private string Urn = string.Empty;
        [SerializeField] private string ModelGuid = string.Empty;
        [SerializeField] private string LoginInfo = string.Empty;

        private readonly string APS_INFO_FILE = "APS_DT_RG.json";
        private string AuthUrl = "https://developer.api.autodesk.com/authentication/v2/token";
        private string PropertyUrl = "https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/metadata/{modelGuid}/properties?forceget=true";
        private string _cacheToken = string.Empty;

        public void GetAPSModelInfoAsync(Action<Dictionary<string, APSModelInfo>> response)
        {
            APIHelper.LoadDataAsync(
                onDataLoaded: response,
                loadLocalData: GetLocalModelDataAsync,
                loadServerData: GetServerModelInfoAsync,
                saveData: SaveModelInfoData
            );
        }

        public void SaveModelInfoData(Dictionary<string, APSModelInfo> modelData)
            => LocalFileSystem.SaveData(modelData, APS_INFO_FILE);

        public async Task<Dictionary<string, APSModelInfo>> GetLocalModelDataAsync()
            => await LocalFileSystem.GetLocalDataAsync<Dictionary<string, APSModelInfo>>(APS_INFO_FILE);

        public async Task<Dictionary<string, APSModelInfo>> GetServerModelInfoAsync()
        {
            DebugLog("Processing APS model data with server...");
            string resultString = await SendRequestModelDataAsync();
            if (resultString == null)
            {
                DebugLogWarning("Model data is null or invalid.");
                return null;
            }

            APSDataDto modelData = await Task.Run(() => JsonConvert.DeserializeObject<APSDataDto>(resultString));
            if (modelData == null || modelData.Data?.Collection == null)
            {
                DebugLogWarning("Model data is null or invalid.");
                return null;
            }

            // 資料格式正確 開始解析為字典並返回 (以elementID快速查詢)
            return await Task.Run(() =>
            {
                Regex NameRegex = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);
                Dictionary<string, APSModelInfo> APSElementIDMap = modelData.Data.Collection
                            .Where(x => !string.IsNullOrEmpty(x.Name) && NameRegex.IsMatch(x.Name))
                            .GroupBy(x => NameRegex.Match(x.Name).Groups[1].Value)
                            .ToDictionary(g => g.Key, g => g.First());

                DebugLog("Load APS model data from server.");
                return APSElementIDMap;
            });
        }

        private async Task<string> SendRequestModelDataAsync(bool hasRetried = false)
        {
            // 編碼 URN
            string urnBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Urn));
            string formattedPropertyUrl = PropertyUrl.Replace("{urn}", urnBase64).Replace("{modelGuid}", ModelGuid);

            using (UnityWebRequest request = UnityWebRequest.Get(formattedPropertyUrl))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_cacheToken}");

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                    return request.downloadHandler.text; // 返回成功的結果
                else
                {
                    // 當 Token 過期或權限不足時
                    if (!hasRetried && (request.responseCode == 401 || request.responseCode == 403))
                    {
                        // 重新獲取 Token 並重試
                        DebugLog("RefreshToken");
                        _cacheToken = await GetAccessTokenAsync();
                        return await SendRequestModelDataAsync(true); // 只重試一次
                    }
                    
                    // 詳細錯誤信息
                    DebugLogWarning($"Failed to get model data: {request.responseCode} {request.error} - Details: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // 設置 Basic Authorization Header
            string authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(LoginInfo));
            string postData = "grant_type=client_credentials&scope=data:read";

            using (UnityWebRequest request = new UnityWebRequest(AuthUrl, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                request.SetRequestHeader("Authorization", $"Basic {authInfo}");

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    APSAuthDto auth = JsonConvert.DeserializeObject<APSAuthDto>(request.downloadHandler.text);
                    _cacheToken = auth.AccessToken;
                    DebugLog("Get Access Token Successful");
                    return auth.AccessToken; // Token JSON，例如 { "access_token": "your_token", "expires_in": 3599 }
                }
                else
                {
                    DebugLog("Failed to get token: {request.responseCode} {request.error}");
                    throw new Exception($"Failed to get token: {request.responseCode} {request.error}");
                }
            }
        }

        private void DebugLogWarning(string message) => DebugLogBase(message, true);
        private void DebugLog(string message) => DebugLogBase(message, false);

        private void DebugLogBase(string message, bool isWarning = false)
        {
            string finalMessage = "[APS] " + message;
            if (!isWarning) Debug.Log(finalMessage);
            else Debug.LogWarning(finalMessage);
        }
    }
}