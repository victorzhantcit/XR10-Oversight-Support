using Newtonsoft.Json;
using Oversight.Clipping;
using Oversight.Dtos;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Oversight.Core
{
    public class MockServiceIBMS
    {
        private string _mockDataFolder;

        public MockServiceIBMS()
        {
            // ���a�����ƾڪ��ؿ�
            _mockDataFolder = Path.Combine(Application.persistentDataPath, "Mock");

            // �T�O�ؿ��s�b
            if (!Directory.Exists(_mockDataFolder))
            {
                Directory.CreateDirectory(_mockDataFolder);
            }
        }

        // �����q���A������]�ƥͩR�P���ƾ�
        public async Task<DeviceLifecycleManager> GetDeviceLifecycleAsync(string buildingCode)
        {
            string filePath = Path.Combine(_mockDataFolder, $"{buildingCode}_DevicesLifecycle.json");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Mock data file not found: {filePath}");
                return null;
            }

            try
            {
                // Ū�� JSON ��󤺮e
                return await Task.Run(() =>
                {
                    string jsonContent = File.ReadAllText(filePath);
                    Debug.Log("Receive device lifecycle from mock server"/* + jsonContent*/);
                    // �N JSON �ഫ�� DTO
                    return JsonConvert.DeserializeObject<DeviceLifecycleManager>(jsonContent);
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading mock data file: {filePath}\n{ex.Message}");
                return null;
            }
        }

        public async Task<List<DeviceInfoDto>> GetDeviceInfoAsync(string buildingCode)
        {
            string filePath = Path.Combine(_mockDataFolder, $"{buildingCode}_DeviceInfo.json");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Mock data file not found: {filePath}");
                return null;
            }

            try
            {
                // Ū�� JSON ��󤺮e
                return await Task.Run(() =>
                {
                    string jsonContent = File.ReadAllText(filePath);
                    Debug.Log("Receive device info from mock server"/* + jsonContent*/);
                    // �N JSON �ഫ�� DTO
                    return JsonConvert.DeserializeObject<List<DeviceInfoDto>>(jsonContent);
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading mock data file: {filePath}\n{ex.Message}");
                return null;
            }
        }

        public async Task<List<ImageSelectorDtos>> GetClippingCompareImagesAsync(string buildingCode)
        {
            string filePath = Path.Combine(_mockDataFolder, $"{buildingCode}_ClippingCompareImage.json");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Mock data file not found: {filePath}");
                return null;
            }

            try
            {
                // Ū�� JSON ��󤺮e
                return await Task.Run(() =>
                {
                    string jsonContent = File.ReadAllText(filePath);
                    Debug.Log("Receive device info from mock server"/* + jsonContent*/);
                    // �N JSON �ഫ�� DTO
                    return JsonConvert.DeserializeObject<List<ImageSelectorDtos>>(jsonContent);
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading mock data file: {filePath}\n{ex.Message}");
                return null;
            }
        }
    }
}
