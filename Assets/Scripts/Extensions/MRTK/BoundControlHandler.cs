using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace MRTK.Extensions
{
    public class BoundControlHandler : MonoBehaviour
    {
        [SerializeField] private BoundsControl _boundControlTool;

        private Transform _targetTransform;
        private Vector3 initialModelPositionOffset;
        private Quaternion initialModelRotationOffset;
        private Vector3 initialBoundControlScale;
        private Vector3 initialModelScale;

        private void Update()
        {
            // 確保只有在 Bound Control 啟用時執行同步
            if (_boundControlTool != null && _boundControlTool.gameObject.activeSelf && _targetTransform != null)
            {
                // 計算縮放比例
                Vector3 scaleRatio = new Vector3(
                    _boundControlTool.transform.localScale.x / initialBoundControlScale.x,
                    _boundControlTool.transform.localScale.y / initialBoundControlScale.y,
                    _boundControlTool.transform.localScale.z / initialBoundControlScale.z
                );

                // 計算調整後的位置和旋轉
                Vector3 scaledOffset = Vector3.Scale(initialModelPositionOffset, scaleRatio);
                _targetTransform.position = _boundControlTool.transform.position + _boundControlTool.transform.rotation * scaledOffset;
                _targetTransform.rotation = _boundControlTool.transform.rotation * initialModelRotationOffset;

                // 更新縮放
                _targetTransform.localScale = new Vector3(
                    initialModelScale.x * scaleRatio.x,
                    initialModelScale.y * scaleRatio.y,
                    initialModelScale.z * scaleRatio.z
                );
            }
        }

        public void AddBoundControl(GameObject hitObject, Pose pose, Transform overrideModelTarget = null)
        {
            // 移除之前的 Bound Control
            RemoveBoundControl();

            // 設定 Bound Control 應用的目標物件
            _targetTransform = overrideModelTarget ?? hitObject.transform;

            // 設定 Bound Control 的位置和旋轉為固定的 Pose
            _boundControlTool.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _boundControlTool.gameObject.SetActive(true);

            // 記錄初始偏移：計算 _adjustModel 相對於 BCT 的偏移
            initialBoundControlScale = _boundControlTool.transform.localScale;
            initialModelScale = _targetTransform.localScale;
            initialModelPositionOffset = Quaternion.Inverse(_boundControlTool.transform.rotation) *
                                         (_targetTransform.position - _boundControlTool.transform.position);
            initialModelRotationOffset = Quaternion.Inverse(_boundControlTool.transform.rotation) * _targetTransform.rotation;

            DebugLog("Bound controlling: " + _targetTransform.name);
            DebugLog("Pose: " + pose.position + ", " + pose.rotation);
        }

        public void RemoveBoundControl()
        {
            _boundControlTool.gameObject.SetActive(false);
            _targetTransform = null;
            initialModelPositionOffset = Vector3.zero;
            initialModelRotationOffset = Quaternion.identity;
            initialBoundControlScale = Vector3.one;
            initialModelScale = Vector3.one;

            DebugLog("RemoveBoundControl");
        }

        private void DebugLog(string msg)
        {
            Debug.Log("[BoundControlHandler] " + msg);
        }
    }
}

