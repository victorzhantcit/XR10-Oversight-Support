using Microsoft.MixedReality.GraphicsTools;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Test
{
    public class TestCesium : MonoBehaviour
    {
        public HandRaycastManager _raycastManager;
        public BoundsControl _boundControlTool;
        public GameObject _adjustModel;
        public GameObject _overrideModelAnchor;

        private Vector3 initialModelPositionOffset;
        private Quaternion initialModelRotationOffset;
        private Vector3 initialBoundControlScale;
        private Vector3 initialModelScale;

        public ClippingBox clippingBox;

        private void Start()
        {
            //clippingBox.AddMaterial
        }

        private void Update()
        {
            // 確保只有在 Bound Control 啟用時執行同步
            if (_boundControlTool != null && _boundControlTool.gameObject.activeSelf && _adjustModel != null)
            {
                // 計算縮放比例
                Vector3 scaleRatio = new Vector3(
                    _boundControlTool.transform.localScale.x / initialBoundControlScale.x,
                    _boundControlTool.transform.localScale.y / initialBoundControlScale.y,
                    _boundControlTool.transform.localScale.z / initialBoundControlScale.z
                );

                // 計算調整後的位置和旋轉
                Vector3 scaledOffset = Vector3.Scale(initialModelPositionOffset, scaleRatio);
                _adjustModel.transform.position = _boundControlTool.transform.position + _boundControlTool.transform.rotation * scaledOffset;
                _adjustModel.transform.rotation = _boundControlTool.transform.rotation * initialModelRotationOffset;

                // 更新縮放
                _adjustModel.transform.localScale = new Vector3(
                    initialModelScale.x * scaleRatio.x,
                    initialModelScale.y * scaleRatio.y,
                    initialModelScale.z * scaleRatio.z
                );
            }
        }


        public void ToggleAdjustModel(bool enabled)
        {
            _raycastManager.GoToNextMode();
            if (enabled == false)
            {
                RemoveBoundControl();
                _raycastManager.SwitchToMode(TestRaycastMode.Move);
            }
            else
            {
                _raycastManager.SwitchToMode(TestRaycastMode.Adjust);
            }
        }

        public void AddBoundControl(GameObject hitObject, Pose pose)
        {
            // 移除之前的 Bound Control
            RemoveBoundControl();

            // 設定目標物件
            if (_overrideModelAnchor == null)
                _adjustModel = hitObject;
            else
                _adjustModel = _overrideModelAnchor;

            // 設定 Bound Control 的位置和旋轉為固定的 Pose
            _boundControlTool.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _boundControlTool.gameObject.SetActive(true);

            // 記錄初始偏移：計算 _adjustModel 相對於 BCT 的偏移
            initialBoundControlScale = _boundControlTool.transform.localScale;
            initialModelScale = _adjustModel.transform.localScale;
            initialModelPositionOffset = Quaternion.Inverse(_boundControlTool.transform.rotation) *
                                         (_adjustModel.transform.position - _boundControlTool.transform.position);
            initialModelRotationOffset = Quaternion.Inverse(_boundControlTool.transform.rotation) * _adjustModel.transform.rotation;
            _raycastManager.SwitchToMode(TestRaycastMode.Move);
            //Debug.Log("Bound controlling: " + hitObject.name);
            //Debug.Log("Pose: " + pose.position + ", " + pose.rotation);
        }

        public void RemoveBoundControl()
        {
            _boundControlTool.gameObject.SetActive(false);
            _adjustModel = null;
            initialModelPositionOffset = Vector3.zero;
            initialModelRotationOffset = Quaternion.identity;
            initialBoundControlScale = Vector3.one;
            initialModelScale = Vector3.one;

            Debug.Log("RemoveBoundControl");
        }
    }
}
