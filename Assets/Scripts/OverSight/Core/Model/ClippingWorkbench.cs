using Microsoft.MixedReality.GraphicsTools;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oversight.Clipping
{
    public enum BoxSide
    {
        Bottom,
        Back
    }

    public class ClippingWorkbench : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] private Transform _modelOutline;
        [SerializeField] private BoundsControl _clippingBoundsControl;
        [SerializeField] private Collider _clippingBoundsCollider;
        [SerializeField] private ObjectManipulator _moveObjectManipulator;
        [SerializeField] private MeshRenderer _clippingBoxRenderer;

        [Header("SpriteRenderer")]
        [SerializeField] private SpriteRenderer _compareBottomSprite;
        [SerializeField] private SpriteRenderer _compareBackSprite;

        private Bounds _initialLocalBounds;

        [SerializeField] private List<ClippingSetting> _clippingSettings;

        private void Start() => Initialize();

        private void OnDestroy() => UnregisterClippingOptions();

        private void Initialize()
        {
            _initialLocalBounds = GetClippingBounds();
            RegisterClippingOptions();
            ResetClippingBox();
        }

        public void InitWorkbenchPosition()
        {
            Transform cameraTransform = Camera.main.transform;

            Vector3 forward = cameraTransform.forward;
            forward.y = 0; // 保持水平前方
            forward.Normalize();

            this.transform.position = cameraTransform.position + forward * 2.0f;
        }

        public Bounds GetClippingBounds()
            => new Bounds(_clippingBoundsControl.transform.localPosition, _clippingBoundsControl.transform.localScale);

        public Bounds GetOutlineBounds()
            => new Bounds(_modelOutline.transform.localPosition, _modelOutline.transform.localScale);

        public void RegisterClippingOptions()
        {
            for (int i = 0; i < _clippingSettings.Count; i++)
                _clippingSettings[i].RegisterTrigger(ApplyClippingSettingToBox);
        }

        public void UnregisterClippingOptions()
        {
            for (int i = 0; i < _clippingSettings.Count; i++)
                _clippingSettings[i].UnregisterTrigger();
        }

        public void ApplyClippingSettingToBox(ClippingTransform clippingTransform)
        {
            _clippingBoundsControl.transform.localPosition = clippingTransform.Position;
            _clippingBoundsControl.transform.localScale = clippingTransform.Scale;
        }

        public void ToggleMoveObjectManipulator()
        {
            _moveObjectManipulator.gameObject.SetActive(true);
            _clippingBoxRenderer.enabled = false;
            _clippingBoundsCollider.enabled = false;
            _modelOutline.gameObject.SetActive(false);
            _clippingBoundsControl.EnabledHandles = HandleType.None;
        }

        public void ClearSpriteRenderers()
        {
            _compareBottomSprite.sprite = null;
            _compareBackSprite.sprite = null;
        }

        public void ToggleClippingBox()
        {
            _moveObjectManipulator.gameObject.SetActive(false);
            _clippingBoxRenderer.enabled = true;
            _clippingBoundsCollider.enabled = true;
            _modelOutline.gameObject.SetActive(true);
            _clippingBoundsControl.EnabledHandles = HandleType.Scale;
        }

        public void ComfirmClipping()
        {
            ToggleMoveObjectManipulator();
        }

        public void ResetClippingBox()
        {
            ApplyClippingSettingToBox(new ClippingTransform(_initialLocalBounds.center, _initialLocalBounds.size));
            ClearSpriteRenderers();
        }

        public void ApplyImageToSide(BoxSide boxSide, string base64Image)
        {
            if (boxSide == BoxSide.Bottom)
                ApplyImageToClippingBottom(base64Image);
            else if (boxSide == BoxSide.Back)
                ApplyImageToClippingBack(base64Image);
        }

        // Bottom 所以只會變更Z值，其餘transform不動 該值應該偵測currentClippingBounds、modelOutlineBounds底部
        public void ApplyImageToClippingBottom(string base64Image = null)
        {
            Bounds currentClipping = GetClippingBounds();
            Vector3 compareLocalPosition = _compareBottomSprite.transform.localPosition;
            float offsetZ = _clippingBoundsControl.transform.localPosition.z - currentClipping.extents.z;
            Vector3 newCompareLocalPosition = new Vector3(compareLocalPosition.x, compareLocalPosition.y, offsetZ);

            _compareBottomSprite.transform.localPosition = newCompareLocalPosition;
            _compareBottomSprite.sprite = ConvertBase64ToSprite(base64Image);
            _compareBackSprite.sprite = null;

            Debug.Log($"CompareLocalPosition {compareLocalPosition}\nOffsetZ: {offsetZ}\nCompare NewLocalPosition {newCompareLocalPosition}");
        }

        public void ApplyImageToClippingBack(string base64Image)
        {
            Bounds currentClipping = GetClippingBounds();
            Vector3 compareLocalPosition = _compareBackSprite.transform.localPosition;
            float offsetY = _clippingBoundsControl.transform.localPosition.y + currentClipping.extents.y;
            Vector3 newCompareLocalPosition = new Vector3(compareLocalPosition.x, offsetY, compareLocalPosition.z);

            _compareBackSprite.transform.localPosition = newCompareLocalPosition;
            _compareBackSprite.sprite = ConvertBase64ToSprite(base64Image);
            _compareBottomSprite.sprite = null;

            Debug.Log($"CompareLocalPosition {compareLocalPosition}\nOffsetY: {offsetY}\nCompare NewLocalPosition {newCompareLocalPosition}");
        }

        private Sprite ConvertBase64ToSprite(string base64Image)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                texture.Apply();

                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f) // pivot 中心
                );
            }
            catch (Exception e)
            {
                Debug.LogError("Base64 轉 Sprite 失敗：" + e.Message);
                return null;
            }
        }
    }
}
