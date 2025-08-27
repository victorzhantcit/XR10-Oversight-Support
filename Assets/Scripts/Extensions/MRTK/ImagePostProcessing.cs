using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImagePostProcessing : MonoBehaviour
{
    [Header("UI Components")]
    public RawImage realWorldImage; // 真實世界圖像的 UI
    public RawImage virtualImage;  // 虛擬圖像的 UI
    public MixedReality.Toolkit.UX.Slider slider; // MRTK.UX 的 Slider
    public PressableButton confirmButton; // 確認鍵

    [Header("Camera Settings")]
    public Camera virtualCamera; // 用於渲染虛擬圖像的相機
    public LayerMask ignoreRender;

    [Header("Slate")]
    public Transform slateToShow;

    private RenderTexture renderTexture;

    /// <summary>
    /// 初始化腳本
    /// </summary>
    private void Start()
    {
        // 初始化 RenderTexture
        virtualCamera.enabled = false;
        virtualCamera.cullingMask = ~ignoreRender.value;

        // 設置 Slider 事件
        slider.OnValueUpdated.AddListener(UpdateVirtualImageAlpha);

        // 設置確認按鈕事件
        confirmButton.OnClicked.AddListener(MergeImages);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.OnValueUpdated.RemoveListener(UpdateVirtualImageAlpha);

        if (confirmButton != null)
            confirmButton.OnClicked.RemoveListener(MergeImages);
    }


    /// <summary>
    /// 更新虛擬圖像的透明度
    /// </summary>
    /// <param name="value">Slider 的值 (0 ~ 1)</param>
    private void UpdateVirtualImageAlpha(SliderEventData eventData)
    {
        if (virtualImage != null)
        {
            Color color = virtualImage.color;
            color.a = eventData.NewValue;
            virtualImage.color = color;
        }
    }

    /// <summary>
    /// 外部接口：設置真實世界圖像
    /// </summary>
    /// <param name="image">真實世界圖像的 Texture2D</param>
    public void SetRealWorldImage(Texture2D image)
    {
        if (realWorldImage != null)
        {
            realWorldImage.texture = image;

            // 获取真实世界图像的尺寸
            int width = image.width;
            int height = image.height;

            // 创建新的 RenderTexture
            if (renderTexture != null)
            {
                renderTexture.Release();
            }
            renderTexture = new RenderTexture(width, height, 8);
            virtualCamera.targetTexture = renderTexture;
            virtualImage.texture = renderTexture;
        }
    }


    /// <summary>
    /// 合併兩張圖片並執行 Callback
    /// </summary>
    public void MergeImages()
    {
        if (realWorldImage.texture == null || virtualImage.texture == null)
        {
            Debug.LogError("無法合併圖片，因為其中一個圖像為空！");
            return;
        }

        Debug.Log($"realWorldImage: {realWorldImage.texture.width}x{realWorldImage.texture.height}\n virtualImage: {virtualImage.texture.width}x{virtualImage.texture.height}");

        Texture2D realWorldTexture = realWorldImage.texture as Texture2D;
        RenderTexture virtualRenderTexture = renderTexture;

        // 讀取 RenderTexture 到 Texture2D
        Texture2D virtualTexture = new Texture2D(realWorldImage.texture.width, realWorldImage.texture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = virtualRenderTexture;
        virtualTexture.ReadPixels(new Rect(0, 0, virtualRenderTexture.width, virtualRenderTexture.height), 0, 0);
        virtualTexture.Apply();
        RenderTexture.active = null;

        // 合併圖片
        float overlayOpacity = slider.Value; // MRTK.UX Slider 的值
        Texture2D combinedTexture = CombineTextures(realWorldTexture, virtualTexture, overlayOpacity);

        // 執行 Callback
        OnImagesMerged?.Invoke(combinedTexture);
        slateToShow.gameObject.SetActive(false);
    }

    /// <summary>
    /// 合併兩張圖片，中心對齊，並裁切超出範圍的部分
    /// </summary>
    /// <param name="background">底層圖片</param>
    /// <param name="overlay">頂層圖片</param>
    /// <param name="overlayOpacity">頂層圖片的透明度 (0 ~ 1)</param>
    /// <returns>合併後的 Texture2D</returns>
    private Texture2D CombineTextures(Texture2D background, Texture2D overlay, float overlayOpacity)
    {
        int resultWidth = background.width;
        int resultHeight = background.height;

        Texture2D result = new Texture2D(resultWidth, resultHeight, TextureFormat.RGBA32, false);

        // 计算顶层图片的偏移量，以中心对齐
        int overlayOffsetX = (resultWidth - overlay.width) / 2;
        int overlayOffsetY = (resultHeight - overlay.height) / 2;

        for (int y = 0; y < resultHeight; y++)
        {
            for (int x = 0; x < resultWidth; x++)
            {
                // 获取底层图片的颜色
                Color bgColor = background.GetPixel(x, y);

                // 获取顶层图片的颜色（如果在范围内）
                Color overlayColor = Color.clear;
                int overlayX = x - overlayOffsetX;
                int overlayY = y - overlayOffsetY;

                if (overlayX >= 0 && overlayX < overlay.width && overlayY >= 0 && overlayY < overlay.height)
                {
                    overlayColor = overlay.GetPixel(overlayX, overlayY);
                    overlayColor.a *= overlayOpacity; // 调整透明度
                }

                // 合并颜色
                Color finalColor = Color.Lerp(bgColor, overlayColor, overlayColor.a);
                result.SetPixel(x, y, finalColor);
            }
        }

        result.Apply();
        return result;
    }

    /// <summary>
    /// 外部接口：設置真實世界圖像並返回合併結果
    /// </summary>
    /// <param name="image">傳入的真實世界圖像</param>
    /// <param name="callback">處理完成後的回調</param>
    public void ProcessImage(Texture2D image, Action<Texture2D> callback)
    {
        StartCoroutine(CaputeSceneView());
        SetRealWorldImage(image);
        OnImagesMerged = callback;
    }

    private IEnumerator CaputeSceneView()
    {
        virtualCamera.enabled = true;
        yield return new WaitForFixedUpdate();
        virtualCamera.enabled = false;
        slateToShow.gameObject.SetActive(true);
    }

    // 回調事件
    private Action<Texture2D> OnImagesMerged;
}
