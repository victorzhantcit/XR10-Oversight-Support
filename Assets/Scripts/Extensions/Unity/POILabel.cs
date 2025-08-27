using TMPro;
using UnityEngine;

namespace Unity.Extensions
{
    using UnityEngine;
    using TMPro;

    public class POILabel : MonoBehaviour
    {
        [SerializeField] private Transform targetPoint; // 目標參考點

        [SerializeField] private float baseDistance = 10f; // 基準距離
        [SerializeField] private float baseScale = 1f;     // 基準縮放大小
        [SerializeField] private float verticalOffset = 0.5f; // **新增：標籤的垂直偏移量**

        private void LateUpdate()
        {
            if (targetPoint != null && targetPoint.gameObject.activeSelf)
            {
                // **在原始位置的基礎上增加 Y 軸偏移**
                Vector3 adjustedPosition = targetPoint.position + Vector3.up * verticalOffset;
                transform.position = adjustedPosition;

                // 讓標籤永遠朝向玩家
                Vector3 faceCamera = 2 * adjustedPosition - Camera.main.transform.position;
                transform.LookAt(faceCamera);

                // 讓視覺下的縮放在任何距離呈現大小一致
                float distance = Vector3.Distance(Camera.main.transform.position, adjustedPosition);
                float scaleMultiplier = distance / baseDistance;
                transform.localScale = Vector3.one * baseScale * scaleMultiplier;
            }
        }
    }


}
