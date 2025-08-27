using TMPro;
using UnityEngine;

namespace Oversight.Raycast
{
    public class DistanceMeasurement : MonoBehaviour
    {
        public float LABEL_OFFSET = 0.001f;
        [SerializeField] private Transform _firstPointAnchor;
        [SerializeField] private Transform _secondPointAnchor;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private TMP_Text _distanceLabel;

        public bool HasFirstPoint { get; private set; } = false;
        public bool HasSecondPoint { get; private set; } = false;

        private void LateUpdate()
        {
            if (_distanceLabel.gameObject.activeSelf)
            {
                Vector3 faceCamera = 2 * _distanceLabel.transform.position - Camera.main.transform.position;
                _distanceLabel.transform.LookAt(faceCamera);
            }
        }

        public void SetFirstPoint(Pose pose)
        {
            SetPose(_firstPointAnchor, pose);
            HasFirstPoint = true;
        }

        public void SetSecondPoint(Pose pose)
        {
            Debug.Log("Set Second Point " + pose.position);
            SetPose(_secondPointAnchor, pose);
            HasSecondPoint = true;
            DrawDistance();
        }

        public void ClearFirstPoint()
        {
            HasFirstPoint = false;
            _firstPointAnchor.gameObject.SetActive(false);
            _lineRenderer.gameObject.SetActive(false);
            _distanceLabel.gameObject.SetActive(false);
        }

        public void ClearSecondPoint()
        {
            HasSecondPoint = false;
            _secondPointAnchor.gameObject.SetActive(false);
            _lineRenderer.gameObject.SetActive(false);
            _distanceLabel.gameObject.SetActive(false);
        }

        private void SetPose(Transform anchor, Pose pose)
        {
            anchor.position = pose.position;
            anchor.rotation = pose.rotation;
            anchor.gameObject.SetActive(true);
        }

        private void DrawDistance()
        {
            Debug.Log("DrawDistance");
            Vector3 firstPoint = _firstPointAnchor.position;
            Vector3 secondPoint = _secondPointAnchor.position;

            // 計算兩點之間的向量
            float distance = Vector3.Distance(firstPoint, secondPoint);
            Vector3 direction = (secondPoint - firstPoint).normalized;

            // 設置文字的旋轉，使其與線平行，確保文字的上向量與世界的上向量對齊
            Vector3 perpendicularDirection = Vector3.Cross(direction, Vector3.up).normalized;
            Quaternion rotation = Quaternion.LookRotation(perpendicularDirection, Vector3.up);

            // 計算文字顯示的位置，將其偏移到線的旁邊
            float labelOffset = 2f / 3f; // 偏移量，可根據需要調整
                                         //Vector3 textPosition = secondPoint + direction * labelOffset;
            Vector3 textPosition = Vector3.Lerp(firstPoint, secondPoint, labelOffset);
            Transform distanceTransform = _distanceLabel.transform;

            _lineRenderer.SetPosition(0, firstPoint);
            _lineRenderer.SetPosition(1, secondPoint);
            _lineRenderer.gameObject.SetActive(true);

            // 設置文字的位置
            distanceTransform.position = textPosition;

            //AdjustTextRotationTowardsCamera();

            _distanceLabel.text = distance.ToString("F2") + "m";
            _distanceLabel.gameObject.SetActive(true);

        }

        public void ResetState()
        {
            // 重置內部狀態
            HasFirstPoint = false;
            HasSecondPoint = false;

            // 隱藏 UI 與測量點
            _firstPointAnchor.gameObject.SetActive(false);
            _secondPointAnchor.gameObject.SetActive(false);
            _lineRenderer.gameObject.SetActive(false);
            _distanceLabel.gameObject.SetActive(false);

            // 清空 `LineRenderer`
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.zero);

            // 設置為非激活狀態，確保物件回收後不會影響場景
            gameObject.SetActive(false);
        }

    }
}