using MRTK.Extensions;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FMSolution.FMETP
{
    public class PhotoCaptureFMETP : MonoBehaviour
    {
        [SerializeField] private TransparentPromptDialog _dialog; // 提示框
        [SerializeField] private GameViewEncoder _gameViewEncoder; // FMETP 的 GameViewEncoder
                                                                   //[SerializeField] private RawImage _rawImage; // 用於顯示截圖的 UI

        public void CapturePhoto(Action<Texture2D> captureResponse)
        {
//#if UNITY_EDITOR
//            ApplyVirtualCaptureTexture(captureResponse);
//#else
            StartCoroutine(ApplyWebCamTexture(captureResponse));
//#endif
        }

        private IEnumerator ApplyWebCamTexture(Action<Texture2D> captureResponse)
        {
            // 確保 GameViewEncoder 啟用
            _gameViewEncoder.enabled = true;

            // 截圖倒計時
            int countdown = 3;
            while (countdown > 0)
            {
                _dialog.Setup(true, $"截圖倒計時...{countdown--}");
                yield return new WaitForSeconds(1f);
            }

            _dialog.Setup(false);

            // 等待 WebCamTexture 就緒
            yield return new WaitUntil(() =>
                _gameViewEncoder.WebcamTexture != null && _gameViewEncoder.WebcamTexture.isPlaying);

            // 保存為 Texture2D 並將結果傳回
            Texture2D capturedTexture = SavePhotoToTexture2D();
            if (capturedTexture != null)
            {
                //_rawImage.texture = capturedTexture; // 在 UI 顯示
                captureResponse?.Invoke(capturedTexture);
            }
            else
            {
                Debug.LogError("截圖失敗！");
                captureResponse?.Invoke(null);
            }

            // 停用 GameViewEncoder
            _gameViewEncoder.enabled = false;
        }

        private async void ApplyVirtualCaptureTexture(Action<Texture2D> captureResponse)
        {
            // 在 Unity Editor 中生成隨機圖片
            int textureWidth = 1080;
            int textureHeight = 720;
            int blockSize = 72; // 每個區塊的大小
            Texture2D randomTexture = new Texture2D(textureWidth, textureHeight);

            // 截圖倒計時
            int countdown = 3;
            while (countdown > 0)
            {
                _dialog.Setup(true, $"截圖倒計時...{countdown--}");
                await Task.Delay(1000);
            }

            _dialog.Setup(false);

            for (int y = 0; y < textureHeight; y += blockSize)
            {
                for (int x = 0; x < textureWidth; x += blockSize)
                {
                    // 為每個區塊生成一種隨機顏色
                    Color randomColor = new Color(Random.value, Random.value, Random.value);
                    for (int dy = 0; dy < blockSize; dy++)
                    {
                        for (int dx = 0; dx < blockSize; dx++)
                        {
                            if (x + dx < textureWidth && y + dy < textureHeight)
                            {
                                randomTexture.SetPixel(x + dx, y + dy, randomColor);
                            }
                        }
                    }
                }
            }

            randomTexture.Apply();

            captureResponse?.Invoke(randomTexture);
        }

        private Texture2D SavePhotoToTexture2D()
        {
            WebCamTexture webcamTexture = _gameViewEncoder.WebcamTexture;
            if (webcamTexture == null)
            {
                Debug.LogError("WebCamTexture 尚未初始化！");
                return null;
            }

            // 創建 Texture2D 並從 WebCamTexture 中獲取像素數據
            Texture2D photoTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
            photoTexture.SetPixels(webcamTexture.GetPixels());
            photoTexture.Apply();

            Debug.Log("Photo captured and saved as Texture2D.");
            return photoTexture;
        }
    }
}
