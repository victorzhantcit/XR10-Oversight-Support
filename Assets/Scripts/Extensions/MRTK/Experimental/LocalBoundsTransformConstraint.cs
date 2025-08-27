using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit;
using UnityEngine;

namespace MRTK.Extensions.Experimental
{
    public class SelfBoundsScaleConstraint : TransformConstraint
    {
        public Bounds initialLocalBounds;
        private float epsilon = 0.5f;

        private Vector3? lastCorrectedScale = null;

        public override TransformFlags ConstraintType => TransformFlags.Scale;

        public override void Setup(MixedRealityTransform worldPose)
        {
            base.Setup(worldPose);
            lastCorrectedScale = null;
            initialLocalBounds = new Bounds(this.transform.localPosition, this.transform.localScale);
        }

        public override void ApplyConstraint(ref MixedRealityTransform transformToConstrain)
        {
            Vector3 desiredScale = transformToConstrain.Scale;
            bool isInside = IsInsideBound(this.transform, initialLocalBounds.center, initialLocalBounds.extents);

            bool isExpanding = desiredScale.x > this.transform.localScale.x ||
                               desiredScale.y > this.transform.localScale.y ||
                               desiredScale.z > this.transform.localScale.z;

            if (!isInside && isExpanding)
            {
                // 如果這是第一次或使用者試圖往更大拉，才重新校正
                if (lastCorrectedScale == null || desiredScale.sqrMagnitude > lastCorrectedScale.Value.sqrMagnitude + 0.0001f)
                {
                    Vector3 correctedScale = GetCorrectedScaleWithinBounds(this.transform, desiredScale, initialLocalBounds);
                    transformToConstrain.Scale = correctedScale;
                    lastCorrectedScale = correctedScale;
                    Debug.Log("[SelfBoundsScaleConstraint] Scale corrected once.");
                }
                else
                {
                    // 保持之前校正的 scale，不再跳動
                    transformToConstrain.Scale = lastCorrectedScale.Value;
                }
            }
            else
            {
                // 合法縮小或在邊界內，重置狀態
                lastCorrectedScale = null;
                transformToConstrain.Scale = desiredScale;
            }
        }

        private Vector3 GetCorrectedScaleWithinBounds(Transform target, Vector3 desiredScale, Bounds bounds)
        {
            Vector3 corrected = desiredScale;
            Vector3 center = target.localPosition;
            Vector3 boundMin = bounds.min;
            Vector3 boundMax = bounds.max;

            float maxX = Mathf.Min(boundMax.x - center.x, center.x - boundMin.x) * 2f;
            float maxY = Mathf.Min(boundMax.y - center.y, center.y - boundMin.y) * 2f;
            float maxZ = Mathf.Min(boundMax.z - center.z, center.z - boundMin.z) * 2f;

            float bias = 0.01f;

            if (corrected.x > maxX) corrected.x = maxX - bias;
            if (corrected.y > maxY) corrected.y = maxY - bias;
            if (corrected.z > maxZ) corrected.z = maxZ - bias;

            return corrected;
        }

        private bool IsInsideBound(Transform target, Vector3 boundCenter, Vector3 boundExtents)
        {
            Vector3 halfScale = target.localScale * 0.5f;
            Vector3 min = target.localPosition - halfScale;
            Vector3 max = target.localPosition + halfScale;

            Vector3 boundMin = boundCenter - boundExtents;
            Vector3 boundMax = boundCenter + boundExtents;

            return (min.x >= boundMin.x - epsilon && max.x <= boundMax.x + epsilon) &&
                   (min.y >= boundMin.y - epsilon && max.y <= boundMax.y + epsilon) &&
                   (min.z >= boundMin.z - epsilon && max.z <= boundMax.z + epsilon);
        }
    }
}
