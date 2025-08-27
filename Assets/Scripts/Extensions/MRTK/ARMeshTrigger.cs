using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MRTK.Extensions
{
    [RequireComponent(typeof(ARMeshManager))]
    public class ARMeshTrigger : MonoBehaviour
    {
        private ARMeshManager _arMeshManager;
        private GameObject _arMeshGroup = null;

        private void Start()
        {
            _arMeshManager = GetComponent<ARMeshManager>();
            _arMeshManager.enabled = false;
        }

        public void ActivateArMesh(bool isActive)
        {
            // 取得 AR Mesh Manager 產生網格的集合
            if (_arMeshGroup == null)
            {
                Transform cameraOffset = _arMeshManager.transform.parent;
                _arMeshGroup = cameraOffset.GetChild(cameraOffset.childCount - 1).gameObject;
                //Debug.Log(_arMeshGroup.name);
            }

            // 啟用/關閉 AR Mesh 偵測與顯示
            _arMeshManager.enabled = isActive;
            _arMeshGroup.SetActive(isActive);
        }
    }
}

