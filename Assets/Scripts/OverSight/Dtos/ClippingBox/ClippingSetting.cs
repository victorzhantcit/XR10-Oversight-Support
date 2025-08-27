using MixedReality.Toolkit.UX;
using System;
using UnityEngine;

namespace Oversight.Clipping
{
    [Serializable]
    public class ClippingSetting
    {
        //public ClippingFloor Floor;
        //public ClippingSection Section;

        public PressableButton TriggerButton;
        public MeshRenderer ClippingBoxRenderer;

        private ClippingTransform _clippingTransform;

        // 提供一個註冊 listener 的方法，但不處理外部邏輯
        public void RegisterTrigger(Action<ClippingTransform> onTriggered)
        {
            UnregisterTrigger();
            _clippingTransform = GetBounds();
            TriggerButton.OnClicked.AddListener(() =>
            {
                onTriggered?.Invoke(_clippingTransform);
            });
        }

        public void UnregisterTrigger()
        {
            TriggerButton.OnClicked.RemoveAllListeners();
        }

        public ClippingTransform GetBounds()
        {
            Transform t = ClippingBoxRenderer.transform;
            Bounds worldBounds = ClippingBoxRenderer.bounds;

            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;

            Vector3[] corners = new Vector3[8];
            corners[0] = t.InverseTransformPoint(center + new Vector3(+extents.x, +extents.y, +extents.z));
            corners[1] = t.InverseTransformPoint(center + new Vector3(+extents.x, +extents.y, -extents.z));
            corners[2] = t.InverseTransformPoint(center + new Vector3(+extents.x, -extents.y, +extents.z));
            corners[3] = t.InverseTransformPoint(center + new Vector3(+extents.x, -extents.y, -extents.z));
            corners[4] = t.InverseTransformPoint(center + new Vector3(-extents.x, +extents.y, +extents.z));
            corners[5] = t.InverseTransformPoint(center + new Vector3(-extents.x, +extents.y, -extents.z));
            corners[6] = t.InverseTransformPoint(center + new Vector3(-extents.x, -extents.y, +extents.z));
            corners[7] = t.InverseTransformPoint(center + new Vector3(-extents.x, -extents.y, -extents.z));

            Bounds localBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Length; i++)
            {
                localBounds.Encapsulate(corners[i]);
            }

            Vector3 localScale = ClippingBoxRenderer.transform.localScale;

            // 防止除以 0（很少見但保險）
            Vector3 safeLossyScale = new Vector3(
                Mathf.Approximately(localScale.x, 0f) ? 1f : localScale.x,
                Mathf.Approximately(localScale.y, 0f) ? 1f : localScale.y,
                Mathf.Approximately(localScale.z, 0f) ? 1f : localScale.z
            );

            // 如果你有 worldSize，可以這樣反推回 localSize
            Vector3 approxLocalSize = localBounds.size;
            approxLocalSize = new Vector3(
                approxLocalSize.x * safeLossyScale.x,
                approxLocalSize.y * safeLossyScale.y,
                approxLocalSize.z * safeLossyScale.z
            );

            //Debug.Log($"[Renderer: {ClippingBoxRenderer.name}] Local Bounds Approximated (based on localScale):\n" +
            //          $"Center: {ClippingBoxRenderer.transform.localPosition}\tSize: {approxLocalSize}");

            return new ClippingTransform(ClippingBoxRenderer.transform.localPosition, approxLocalSize);
        }
    }

    public class ClippingTransform
    {
        public Vector3 Position;
        public Vector3 Scale;

        public ClippingTransform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
        }
    }

    public enum ClippingFloor
    {
        // 格式: 大樓名稱_樓層
        None,
        RG_BBF,
        RG_B3F,
        RG_B2F,
        RG_B1F,
        RG_1F,
        RG_2F,
        RG_3F,
        RG_4F,
        RG_5F,
        RG_6F,
        RG_7F,
        RG_8F,
        RG_9F,
        RG_10F,
        RG_R1F,
        RG_R2F,
        ALL
    }

    public enum ClippingSection
    {
        None,
        Sec_A,
        Sec_B,
        Sec_C,
        Sec_D,
        Sec_E,
        Sec_F,
        Sec_G,
        Sec_H,
        Sec_I,
        Sec_J,
        Sec_K,
        Sec_L,
        Sec_M,
        Sec_N,
        Sec_O,
        ALL
    }
}
