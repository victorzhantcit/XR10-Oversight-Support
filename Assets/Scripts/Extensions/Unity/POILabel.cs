using TMPro;
using UnityEngine;

namespace Unity.Extensions
{
    using UnityEngine;
    using TMPro;

    public class POILabel : MonoBehaviour
    {
        [SerializeField] private Transform targetPoint; // �ؼаѦ��I

        [SerializeField] private float baseDistance = 10f; // ��ǶZ��
        [SerializeField] private float baseScale = 1f;     // ����Y��j�p
        [SerializeField] private float verticalOffset = 0.5f; // **�s�W�G���Ҫ����������q**

        private void LateUpdate()
        {
            if (targetPoint != null && targetPoint.gameObject.activeSelf)
            {
                // **�b��l��m����¦�W�W�[ Y �b����**
                Vector3 adjustedPosition = targetPoint.position + Vector3.up * verticalOffset;
                transform.position = adjustedPosition;

                // �����ҥû��¦V���a
                Vector3 faceCamera = 2 * adjustedPosition - Camera.main.transform.position;
                transform.LookAt(faceCamera);

                // ����ı�U���Y��b����Z���e�{�j�p�@�P
                float distance = Vector3.Distance(Camera.main.transform.position, adjustedPosition);
                float scaleMultiplier = distance / baseDistance;
                transform.localScale = Vector3.one * baseScale * scaleMultiplier;
            }
        }
    }


}
