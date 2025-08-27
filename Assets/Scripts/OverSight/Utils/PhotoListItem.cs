using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Oversight.Utils
{
    public class PhotoListItem : VirtualListItem<string>
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private PressableButton _removeImageButton;

        private string _photoSns;
        Action _removeImageAction = null;

        public override void SetContent(string photoBase64, int index, bool interactable)
        {
            _removeImageButton.gameObject.SetActive(interactable);
            _rawImage.texture = DecodeBase64Photo(photoBase64);

            ActivateLoadingIcon(true);
            LoadPhoto(photoBase64);
        }

        public void SetRemoveAction(Action editAction)
        {
            _removeImageAction = editAction;
        }


        // 異步加載並顯示圖片的方法
        private void LoadPhoto(string base64Photo)
        {
            ActivateLoadingIcon(false);

            if (string.IsNullOrEmpty(base64Photo))
            {
                Debug.LogWarning("圖片加載失敗或找不到圖片 (base64Photo == null || string.Empty)");
                return;
            }

            Texture2D texture = DecodeBase64Photo(base64Photo);

            if (texture == null)
            {
                Debug.LogWarning("圖片加載失敗或找不到圖片");
                return;
            }

            AspectRatioFitter fitter = _rawImage.GetComponent<AspectRatioFitter>();

            fitter.aspectRatio = (float)texture.width / texture.height;
            _rawImage.texture = texture;
        }

        private Texture2D DecodeBase64Photo(string photoBase64)
        {
            if (string.IsNullOrEmpty(photoBase64))
                return null;

            Texture2D texture = new Texture2D(2, 2);
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(photoBase64);
                if (texture.LoadImage(imageBytes)) return texture;
                else return null;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        private void ActivateLoadingIcon(bool enable) => _rawImage.transform.GetChild(0).gameObject.SetActive(enable);
        public void OnRemoveButtonClicked() => _removeImageAction?.Invoke();
    }
}
