using MixedReality.Toolkit.UX.Experimental;
using Oversight.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Extensions;
using UnityEngine;

namespace Oversight.Clipping
{
    public enum ClippingImageProcess
    {
        ImageSelector,
        ImageAnchor
    }

    public class ClippingImageSlate : MonoBehaviour
    {
        [SerializeField] private ServiceManager _service;
        [SerializeField] private ClippingWorkbench _clippingWorkbench;
        [SerializeField] private VirtualizedScrollRectList _imageList;

        private List<ImageSelectorDtos> _imagesInStorage;
        private ImageSelectorDtos _selectedImageInfo;

        private void Start()
        {
            _service.GetClippingCompareImages(images =>
            {
                _imagesInStorage = images;
                _imageList.SetItemCount(images.Count);

                Debug.Log("[ClippingImageSlate] Image Count " + images.Count);
                if (_imageList.isActiveAndEnabled)
                    _imageList.ResetLayout();
            });

            _imageList.OnVisible += OnImageSelectorListItemVisible;
        }

        public void OnImageSelectorListItemVisible(GameObject target, int index)
        {
            PhotoSelectorListItem item = target.GetComponent<PhotoSelectorListItem>();
            int cacheIndex = index;

            item.SetContent(_imagesInStorage[index]);
            item.SetConfirmAction(() =>
            {
                _selectedImageInfo = _imagesInStorage[cacheIndex];

                BoxSide imageApplySide = _selectedImageInfo.GetBoxSide();
                _clippingWorkbench.ApplyImageToSide(imageApplySide, _selectedImageInfo.ImageBase64);

                this.gameObject.SetActive(false);

                Debug.Log($"[ClippingImageSlate] Image {_selectedImageInfo.ImageName} selected");
            });
        }
    }
}
