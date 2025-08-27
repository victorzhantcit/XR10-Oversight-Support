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
            // �T�O�u���b Bound Control �ҥήɰ���P�B
            if (_boundControlTool != null && _boundControlTool.gameObject.activeSelf && _targetTransform != null)
            {
                // �p���Y����
                Vector3 scaleRatio = new Vector3(
                    _boundControlTool.transform.localScale.x / initialBoundControlScale.x,
                    _boundControlTool.transform.localScale.y / initialBoundControlScale.y,
                    _boundControlTool.transform.localScale.z / initialBoundControlScale.z
                );

                // �p��վ�᪺��m�M����
                Vector3 scaledOffset = Vector3.Scale(initialModelPositionOffset, scaleRatio);
                _targetTransform.position = _boundControlTool.transform.position + _boundControlTool.transform.rotation * scaledOffset;
                _targetTransform.rotation = _boundControlTool.transform.rotation * initialModelRotationOffset;

                // ��s�Y��
                _targetTransform.localScale = new Vector3(
                    initialModelScale.x * scaleRatio.x,
                    initialModelScale.y * scaleRatio.y,
                    initialModelScale.z * scaleRatio.z
                );
            }
        }

        public void AddBoundControl(GameObject hitObject, Pose pose, Transform overrideModelTarget = null)
        {
            // �������e�� Bound Control
            RemoveBoundControl();

            // �]�w Bound Control ���Ϊ��ؼЪ���
            _targetTransform = overrideModelTarget ?? hitObject.transform;

            // �]�w Bound Control ����m�M���ର�T�w�� Pose
            _boundControlTool.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _boundControlTool.gameObject.SetActive(true);

            // �O����l�����G�p�� _adjustModel �۹�� BCT ������
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

