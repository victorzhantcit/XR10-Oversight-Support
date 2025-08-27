//using Inspection.Utils;
//using Inspection.Dtos;
//using Newtonsoft.Json;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks;
//using User.Dtos;
//using User.Core;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace Inspection.Core
//{
//    public class InspectionDataCenter : MonoBehaviour
//    {
//        public delegate void OnOrderDownloaded(bool success, List<InspectionDto> inspectionList = null);
//        public event OnOrderDownloaded OrderDownloaded;
//        public delegate void OnImageLoaded(string result, Texture2D imageTexture);
//        public delegate void OnDeviceSubmitted(OrderDeviceSubmitResponseDto response);

//        public string API_URL = "https://ibms.tcitech.com.tw:44320";
//        private string LOGIN_URL => $"{API_URL}/api/Auth/Login";
//        private string ORDERS_URL => API_URL + "/api/Eqpt/Orders";
//        private string SUBMIT_DEVICE_URL => $"{API_URL}/api/Eqpt/SubmitInspDevice";
//        private string PHOTO_URL => $"{API_URL}/api/Eqpt/GetPhoto";
//        private string START_INSPECTION_URL => $"{API_URL}/api/Eqpt/StartInsp";
//        private string UPDATE_DEVICE_URL => $"{API_URL}/api/Eqpt/UpdateInspDevice";
//        private string SUBMIT_ORDER_URL => $"{API_URL}/api/Eqpt/SubmitInspOrder";

//        private readonly string SAVE_FOLDER = "Data/Inspection";
//        private readonly string INSPECTION_ORDER_FILENAME = "InspectionList.json";
//        private readonly string APP_DATA_FILENAME = "InspectionPhotos.json";
//        private readonly string WAIT_UPLOAD_FILENAME = "WaitForUpload.json";

//        private readonly Queue<string> _saveFileNameQueue = new Queue<string>();
//        private readonly Queue<string> _saveDataQueue = new Queue<string>();

//        private bool _isSavingInspection = false;
//        private InspectionPhotosStorage _photoMap;
//        private Queue<OfflineUploadDto> _waitForUploadQueue = new Queue<OfflineUploadDto>();

//        private bool _isInitialized = false;
//        public bool IsInitialized => _isInitialized;

//        private string _token;

//        // Singleton 實例
//        public static InspectionDataCenter Instance { get; private set; }

//        private void Awake()
//        {
//            // 設置單例實例
//            if (Instance == null)
//                Instance = this;
//            else
//                Destroy(gameObject);
//        }

//        private void Start()
//        {
//            _photoMap = LoadLocalPhotoMap();
//        }

//        public string GetSavingFilePath(string filename)
//        {
//            string folderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

//            if (!Directory.Exists(folderPath))
//                Directory.CreateDirectory(folderPath);

//            return Path.Combine(folderPath, filename);
//        }

//        public InspectionPhotosStorage LoadLocalPhotoMap()
//        {
//            string path = GetSavingFilePath(APP_DATA_FILENAME);

//            if (File.Exists(path))
//            {
//                try
//                {
//                    string json = File.ReadAllText(path); // 從文件中讀取 JSON 字符串

//                    return JsonConvert.DeserializeObject<InspectionPhotosStorage>(json);
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogException(ex);
//                }

//            }

//            // 如果文件不存在，返回一個初始化的AppData
//            InspectionPhotosStorage defaultData = new InspectionPhotosStorage() { PhotoHashMap = new Dictionary<string, string>(), LastPhotoSns = 0};
//            SaveAppDatas(defaultData);
//            return defaultData;
//        }

//        public List<InspectionDto> LoadLocalInspectionList()
//        {
//            string path = GetSavingFilePath(INSPECTION_ORDER_FILENAME);

//            if (File.Exists(path))
//            {
//                try
//                {
//                    string json = File.ReadAllText(path); // 從文件中讀取 JSON 字符串
//                    var recordList = JsonConvert.DeserializeObject<List<InspectionDto>>(json);

//                    return recordList;
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogException(ex);
//                }
//            }
//            else
//                DebugLogWarning($"File not found at path: {path}. Returning default data.");

//            List<InspectionDto> defaultData = new List<InspectionDto>();
//            SaveInspectionDatas(defaultData);
//            return defaultData; // 如果文件不存在，返回一個空的 List
//        }

//        public Queue<OfflineUploadDto> LoadWaitForUploadQueue()
//        {
//            string path = GetSavingFilePath(WAIT_UPLOAD_FILENAME);

//            if (File.Exists(path))
//            {
//                try
//                {
//                    string json = File.ReadAllText(path); // 從文件中讀取 JSON 字符串
//                    List<OfflineUploadDto> storage = JsonConvert.DeserializeObject<List<OfflineUploadDto>>(json) ?? new List<OfflineUploadDto>();
//                    //DebugLog("storage OfflineUploadDto count " + storage.Count);
//                    return new Queue<OfflineUploadDto>(storage);
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogError($"Failed to load queue from path: {path}. Exception: {ex.Message}");
//                }
//            }
//            else
//                DebugLogWarning($"File not found at path: {path}. Returning default data.");

//            Queue<OfflineUploadDto> defaultData = new Queue<OfflineUploadDto>();
//            SaveWaitForUploadDatas(defaultData);
//            return defaultData;
//        }

//        public void SaveInspectionDatas(List<InspectionDto> inspectionDatas) => QueueSaveData(inspectionDatas, INSPECTION_ORDER_FILENAME);
//        public void SaveAppDatas(InspectionPhotosStorage appDatas) => QueueSaveData(appDatas, APP_DATA_FILENAME);
//        public void SaveWaitForUploadDatas(Queue<OfflineUploadDto> waitForUploadDatas) => QueueSaveData(waitForUploadDatas, WAIT_UPLOAD_FILENAME);

//        public void AddWaitForUpload(string url, string data)
//        {
//            _waitForUploadQueue.Enqueue(new OfflineUploadDto { Url = url, Data = data });
//            SaveWaitForUploadDatas(_waitForUploadQueue);
//        }

//        // 將儲存請求加入到佇列中
//        private void QueueSaveData(object inspectionDatas, string filename)
//        {
//            string dataJsonString = JsonConvert.SerializeObject(inspectionDatas);

//            _saveFileNameQueue.Enqueue(filename);
//            _saveDataQueue.Enqueue(dataJsonString);
//            ProcessQueue();
//        }

//        // 處理保存佇列
//        private async void ProcessQueue()
//        {
//            if (_isSavingInspection || _saveDataQueue.Count == 0)
//                return;

//            _isSavingInspection = true;

//            while (_saveDataQueue.Count > 0)
//            {
//                string data = _saveDataQueue.Dequeue();
//                string filename = _saveFileNameQueue.Dequeue();

//                await SaveDataAsync(data, filename);
//            }

//            _isSavingInspection = false;
//        }

//        private async Task SaveDataAsync(string data, string filename)
//        {
//            string path = GetSavingFilePath(filename);

//            using (StreamWriter writer = new StreamWriter(path))
//            {
//                await writer.WriteAsync(data);
//            }

//            DebugLog("Cache data saved: " + filename);
//        }

//        // 通用的 POST 請求函式
//        /// <summary>
//        /// Start -> PostRequest -> SendWebRequest -> Check Response ->
//        /// |-----------------Success(callback with result)----------------------|
//        /// |-----------------Failure: 401/403(callback with result)-------------| 
//        ///   -> Call RefreshAuthToken -> SendWebRequest for Token -> Check Response
//        /// |------Success: Update Token, Call PostRequest again(once)---------|
//        /// |------Failure: callback(null) and end the coroutine---------------|
//        /// </summary>
//        public IEnumerator PostRequest(string url, string jsonData, System.Action<string> callback = null, bool hasRefrreshedToken = false)
//        {
//            // 將 JSON 資料轉換為 byte[]，並將 url 附加到請求的 UnityWebRequest POST body raw 中
//            UnityWebRequest request = new UnityWebRequest(url, "POST");
//            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
//            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            request.downloadHandler = new DownloadHandlerBuffer();
//            //DebugLog($"Post request to url: {url}, jsonData: {jsonData}");

//            // 設置 request header
//            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

//            yield return new WaitUntil(() => _photoMap != null);
//            if (!string.IsNullOrEmpty(_token))
//                request.SetRequestHeader("Authorization", "Bearer " + _token);

//            yield return request.SendWebRequest();

//            // 檢查是否發生錯誤
//            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//            {
//                // 如果是 Auth 過期（401 或 403），並且尚未嘗試刷新 Token
//                if ((request.responseCode == 401 || request.responseCode == 403) && !hasRefrreshedToken)
//                {
//                    DebugLogWarning("Auth token expired. Attempting to refresh token...");

//                    // 嘗試刷新 Token 並重試請求
//                    yield return StartCoroutine(RefreshAuthToken(success =>
//                    {
//                        if (success) // 成功刷新 Token，重新發送請求被中斷的前一個請求
//                            StartCoroutine(PostRequest(url, jsonData, callback, true));
//                        else
//                            callback?.Invoke(null); // 刷新失敗，通知回調失敗
//                    }));

//                    yield break;
//                }
//                if (request.responseCode == 400)
//                    Debug.LogError($"Error: {request.error}\nResponse Code: {request.responseCode}\nResponse: {request.downloadHandler.text}");
//                // 直接結束並返回 null，表示請求失敗
//                callback?.Invoke(null);
//            }
//            else
//            {
//                // 請求成功，回傳伺服器的回應內容
//                string responseText = request.downloadHandler.text;
//                DebugLog("Server Response!"/* + responseText*/);
//                callback?.Invoke(responseText);
//            }
//        }

//        public bool IsNetworkAvailable() => Application.internetReachability != NetworkReachability.NotReachable;

//        // 刷新 Token
//        private IEnumerator RefreshAuthToken(Action<bool> onSuccess)
//        {
//            UserData loginData = SecureDataManager.LoadLoggedInData();
//            string loginDataString = JsonConvert.SerializeObject(loginData);

//            if (!IsNetworkAvailable())
//            {
//                onSuccess?.Invoke(false);  // 網絡不可用，立即失敗
//                yield break;
//            }

//            // 刷新 Token 請求
//            yield return StartCoroutine(PostRequest(LOGIN_URL, loginDataString, (loginResponse) =>
//            {
//                if (!string.IsNullOrEmpty(loginResponse))
//                {
//                    // 解析並儲存新的 Auth Token
//                    var loginInfo = JsonConvert.DeserializeObject<UserLoginResponseDto>(loginResponse);

//                    _token = loginInfo.Token;
//                    SaveAppDatas(_photoMap);

//                    DebugLog("Token refreshed successfully.");
//                    onSuccess?.Invoke(true);  // 成功時調用回調
//                }
//                else
//                {
//                    Debug.LogError("Failed to refresh token.");
//                    onSuccess?.Invoke(false);  // 失敗時調用回調
//                }
//            }));
//        }

//        // API: /api/Eqpt/Orders
//        public void DownloadInspectionListOnline()
//        {
//            StartCoroutine(PostRequest(ORDERS_URL, BuildOrdersQuery(), HandleOrdersResponse));
//        }

//        private string BuildOrdersQuery()
//        {
//            int timeSpanDays = 360;
//            DateTime currentDate = DateTime.Now;
//            DateTime startDate = currentDate.AddDays(-timeSpanDays);
//            DateTime endDate = currentDate.AddDays(timeSpanDays);
//            return $"\"{{'Kind':'I','StartDate':'{startDate:yyyy-MM-dd}','EndDate':'{endDate:yyyy-MM-dd}'}}\"";
//        }

//        // 下載完畢後會回傳給事件 OrderDownloaded
//        private void HandleOrdersResponse(string inspectionListJson)
//        {
//            if (inspectionListJson == null)
//            {
//                OrderDownloaded?.Invoke(false);
//                return;
//            }

//            try
//            {
//                var inspectionList = JsonConvert.DeserializeObject<List<InspectionDto>>(inspectionListJson);

//                for (int i = 0; i < inspectionList.Count; i++)
//                    ParseAndLoadPhoto(inspectionList[i].photoSns);

//                SaveInspectionDatas(inspectionList);
//                OrderDownloaded?.Invoke(true, inspectionList);
//            }
//            catch (Exception ex)
//            {
//                OrderDownloaded?.Invoke(false);
//                DebugLogWarning(ex.Message);
//            }
//        }

//        // API: /api/Eqpt/GetPhoto
//        private void ParseAndLoadPhoto(string photoSnsString)
//        {
//            if (string.IsNullOrEmpty(photoSnsString) || _photoMap.PhotoHashMap.ContainsKey(photoSnsString))
//                return;

//            string[] photoSnsArray = photoSnsString.Split(',');

//            for (int i = 0; i < photoSnsArray.Length; i++)
//            {
//                string cachePhotoSns = photoSnsArray[i];

//                if (_photoMap.ContainsPhotoSns(cachePhotoSns))
//                    continue;

//                StartCoroutine(PostRequest(PHOTO_URL, cachePhotoSns, (photoBase64) => HandlePhotoDownloaded(cachePhotoSns, photoBase64)));
//            }
//        }

//        private void HandlePhotoDownloaded(string photoSns, string photoBase64)
//        {
//            // 圖片字串為空或已存在於本地資料則不處理
//            if (!(string.IsNullOrEmpty(photoBase64) || _photoMap.ContainsPhotoSns(photoSns)))
//            {
//                _photoMap.AddPhoto(photoSns, photoBase64);
//                SaveAppDatas(_photoMap);
//            }
//        }

//        public Texture2D GetPhotoTexture(string photoSns)
//        {
//            string photoBase64 = _photoMap.GetPhotoData(photoSns);

//            if (string.IsNullOrEmpty(photoBase64))
//                return null;

//            byte[] imageBytes = Convert.FromBase64String(photoBase64);
//            Texture2D texture = new Texture2D(2, 2);

//            if (texture.LoadImage(imageBytes))
//                return texture;

//            return null;
//        }

//        public string GetPhotoBase64(string sns)
//        {
//            if (_photoMap.PhotoHashMap.ContainsKey(sns))
//                return _photoMap.PhotoHashMap[sns];

//            return null;
//        }

//        public string AddPhoto(string photoBase64)
//        {
//            string newLocalSns = _photoMap.SavePhotoAndReturnSns(photoBase64);

//            SaveAppDatas(_photoMap);
//            return newLocalSns;
//        }

//        public void UpdatePhotoSns(string oldSns, string newSns)
//        {
//            _photoMap.UpdatePhotoSns(oldSns, newSns);

//            SaveAppDatas(_photoMap);
//        }

//        public void RemovePhoto(string sns)
//        {
//            _photoMap.RemovePhotoBySns(sns);

//            SaveAppDatas(_photoMap);
//        }

//        // API: /api/Eqpt/StartInsp
//        public void PostStartInspectionData(string startInspectData, Action<string> callback)
//        {
//            if (!IsNetworkAvailable())
//            {
//                callback?.Invoke(null);
//                DebugLogWarning($"Network unavailable. Adding StartInsp data to upload queue: {startInspectData}");
//                AddWaitForUpload(START_INSPECTION_URL, startInspectData);
//                return;
//            }
//            StartCoroutine(PostRequest(START_INSPECTION_URL, startInspectData, (dateString) => callback?.Invoke(dateString)));
//        }

//        // API: /api/Eqpt/UpdateInspDevice
//        public void PostUpdateInspectionData(string updateInspectData, Action<string> callback)
//        {
//            if (!IsNetworkAvailable())
//            {
//                callback?.Invoke(null);
//                DebugLogWarning($"Network unavailable. Adding UpdateInspDevice data to upload queue: {updateInspectData}");
//                AddWaitForUpload(UPDATE_DEVICE_URL, updateInspectData);
//                return;
//            }
//            StartCoroutine(PostRequest(UPDATE_DEVICE_URL, updateInspectData, (dateString) => callback?.Invoke(dateString)));
//        }

//        // API: /api/Eqpt/SubmitInspDevice
//        public void PostDeviceInspectResult(string deviceData, OnDeviceSubmitted callback)
//        {
//            if (!IsNetworkAvailable())
//            {
//                callback?.Invoke(null);
//                DebugLogWarning($"Network unavailable. Adding SubmitInspDevice data to upload queue: {deviceData}");
//                AddWaitForUpload(SUBMIT_DEVICE_URL, deviceData);
//                return;
//            }
//            StartCoroutine(PostRequest(SUBMIT_DEVICE_URL, deviceData, (result) => HandleDeviceSubmitted(result, callback)));
//        }

//        // API: /api/Eqpt/SubmitInspOrder
//        public void PostOrderInspectResult(string orderData, OnDeviceSubmitted callback)
//        {
//            if (!IsNetworkAvailable())
//            {
//                callback?.Invoke(null);
//                DebugLogWarning($"Network unavailable. Adding SubmitInspOrder data to upload queue: {orderData}");
//                AddWaitForUpload(SUBMIT_ORDER_URL, orderData);
//                return;
//            }
//            // SubmitInspOrder 回傳的資料跟 SubmitInspDevice 一樣 只有PhotoSns是空值
//            StartCoroutine(PostRequest(SUBMIT_ORDER_URL, orderData, (result) => HandleDeviceSubmitted(result, callback)));
//        }

//        private void HandleDeviceSubmitted(string result, OnDeviceSubmitted callback)
//        {
//            if (result == null)
//            {
//                callback?.Invoke(null);
//                return;
//            }

//            try
//            {
//                var deviceSubmitResult = JsonConvert.DeserializeObject<OrderDeviceSubmitResponseDto>(result);
//                DebugLog($"DeviceSubmitted!"/* + result*/);
//                callback?.Invoke(deviceSubmitResult);
//            }
//            catch (Exception ex)
//            {
//                callback?.Invoke(null);
//                DebugLogWarning(ex.Message);
//            }
//        }

//        private void DebugLogWarning(string message) => DebugLogBase(message, true);
//        private void DebugLog(string message) => DebugLogBase(message, false);
//        private void DebugLogBase(string message, bool isWarning = false)
//        {
//            string finalMessage = "[Inspection Data Center] " + message;
//            if (!isWarning) Debug.Log(finalMessage);
//            else Debug.LogWarning(finalMessage);
//        }
//    }
//}
