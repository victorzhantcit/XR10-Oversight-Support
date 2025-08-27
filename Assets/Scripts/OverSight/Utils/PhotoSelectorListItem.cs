using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oversight.Clipping
{
    public class PhotoSelectorListItem : VirtualListItem<ImageSelectorDtos>
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private TMP_Text _imageFileName;
        [SerializeField] private PressableButton _selectImageButton;

        Action _confirmAction = null;

        public override void SetContent(ImageSelectorDtos selector, int _ = 0, bool __ = true)
        {
            _imageFileName.text = selector.ImageName ?? string.Empty;
            ActivateLoadingIcon(true);
            LoadPhoto(selector.ImageBase64);
        }

        private void ActivateLoadingIcon(bool enable)
            => _rawImage.transform.GetChild(0).gameObject.SetActive(enable);

        public void SetConfirmAction(Action editAction)
        {
            _confirmAction = editAction;
        }

        public void OnConfirmButtonClicked() => _confirmAction?.Invoke();

        private void LoadPhoto(string base64Photo)
        {
            ActivateLoadingIcon(false);

            if (string.IsNullOrEmpty(base64Photo))
            {
                Debug.LogWarning("Image is null or empty");
                return;
            }

            Texture2D texture = DecodeBase64Photo(base64Photo);

            if (texture == null)
            {
                Debug.LogWarning("Decoding image failed!");
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
    }
}
