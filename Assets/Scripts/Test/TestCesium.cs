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
            // �T�O�u���b Bound Control �ҥήɰ���P�B
            if (_boundControlTool != null && _boundControlTool.gameObject.activeSelf && _adjustModel != null)
            {
                // �p���Y����
                Vector3 scaleRatio = new Vector3(
                    _boundControlTool.transform.localScale.x / initialBoundControlScale.x,
                    _boundControlTool.transform.localScale.y / initialBoundControlScale.y,
                    _boundControlTool.transform.localScale.z / initialBoundControlScale.z
                );

                // �p��վ�᪺��m�M����
                Vector3 scaledOffset = Vector3.Scale(initialModelPositionOffset, scaleRatio);
                _adjustModel.transform.position = _boundControlTool.transform.position + _boundControlTool.transform.rotation * scaledOffset;
                _adjustModel.transform.rotation = _boundControlTool.transform.rotation * initialModelRotationOffset;

                // ��s�Y��
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
            // �������e�� Bound Control
            RemoveBoundControl();

            // �]�w�ؼЪ���
            if (_overrideModelAnchor == null)
                _adjustModel = hitObject;
            else
                _adjustModel = _overrideModelAnchor;

            // �]�w Bound Control ����m�M���ର�T�w�� Pose
            _boundControlTool.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _boundControlTool.gameObject.SetActive(true);

            // �O����l�����G�p�� _adjustModel �۹�� BCT ������
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
