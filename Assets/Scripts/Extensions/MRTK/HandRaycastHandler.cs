using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MRTK.Extensions
{
    public class HandRaycastHandler : MonoBehaviour
    {
        public delegate void OnRaySelected(GameObject hitObject, Pose hitPose);
        public delegate void OnRayHover(GameObject hoverObject);
        public event OnRaySelected OnSelectEntered;
        public event OnRaySelected OnSelectExited;
        public event OnRayHover OnHoverEntered;
        public event OnRayHover OnHoverExited;

        public LayerMask modelLayer;
        public LayerMask spatialLayer;

        private GameObject _hitObject;
        private Pose _hitPose;
        private Coroutine _hoverCoroutine;

        public void OnRaySelectEntered(SelectEnterEventArgs args)
        {
            if (HitPoseDetect(args))
            {
                OnSelectEntered?.Invoke(_hitObject, _hitPose);
                //Debug.Log("OnSelectEntered");
            }
        }

        public void OnRaySelectExited(SelectExitEventArgs args)
        {
            if (HitPoseDetect(args))
            {
                OnSelectExited?.Invoke(_hitObject, _hitPose);
                //Debug.Log("OnRaySelectExited");
            }
        }

        public void OnRayHoverEntered(HoverEnterEventArgs args)
        {
            if (HitPoseDetect(args))
            {
                OnHoverEntered?.Invoke(_hitObject);
                //Debug.Log("OnRayHoverEntered");

                // 啟動持續 Hover 行為
                if (_hoverCoroutine != null)
                {
                    StopCoroutine(_hoverCoroutine);
                }
                _hoverCoroutine = StartCoroutine(OnRayHovering(args.interactorObject as XRRayInteractor));
            }
        }

        public void OnRayHoverExited(HoverExitEventArgs args)
        {
            if (_hitObject != null)
            {
                OnHoverExited?.Invoke(_hitObject);
                //Debug.Log("OnRayHoverExited");

                // 停止 Hover 行為
                if (_hoverCoroutine != null)
                {
                    StopCoroutine(_hoverCoroutine);
                    _hoverCoroutine = null;
                }

                _hitObject = null; // 清空當前命中對象
            }
        }

        private IEnumerator OnRayHovering(XRRayInteractor interactor)
        {
            while (true)
            {
                if (interactor != null && TryGetDynamicHit(interactor, out GameObject newHitObject, out Pose newHitPose))
                {
                    if (newHitObject != _hitObject)
                    {
                        // 通知 Hover 目標改變
                        if (_hitObject != null)
                        {
                            OnHoverExited?.Invoke(_hitObject);
                            //Debug.Log("OnHoverExited");
                        }

                        OnHoverEntered?.Invoke(newHitObject);
                        //Debug.Log("OnHoverEntered");
                        _hitObject = newHitObject;
                    }

                    // 更新 Pose
                    _hitPose = newHitPose;
                }
                else if (_hitObject != null)
                {
                    // 如果沒有命中任何物件但仍有舊物件，退出 Hover
                    OnHoverExited?.Invoke(_hitObject);
                    //Debug.Log("OnHoverExited");
                    _hitObject = null;
                }

                yield return null; // 等待下一幀
            }
        }

        private bool HitPoseDetect(BaseInteractionEventArgs args)
        {
            var interactor = args.interactorObject as XRRayInteractor;
            if (interactor == null)
            {
                Debug.LogWarning("Interactor is not an XRRayInteractor.");
                return false;
            }

            return TryGetDynamicHit(interactor, out _hitObject, out _hitPose);
        }

        private bool TryGetDynamicHit(XRRayInteractor interactor, out GameObject hitObject, out Pose hitPose)
        {
            hitObject = null;
            hitPose = default;

            if (interactor == null)
                return false;

            RaycastHit hitInfo;
            if (!interactor.TryGetCurrent3DRaycastHit(out hitInfo))
                return false;

            if (!IsVaildRaycastLayer(hitInfo))
                return false;

            hitObject = hitInfo.collider.gameObject;
            hitPose = new Pose(hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            return true;
        }

        private bool IsVaildRaycastLayer(RaycastHit hitInfo)
        {
            return IsInLayerMask(hitInfo.collider.gameObject, spatialLayer) || IsInLayerMask(hitInfo.collider.gameObject, modelLayer);
        }

        private bool IsInLayerMask(GameObject obj, LayerMask mask)
        {
            return ((mask.value & (1 << obj.layer)) != 0);
        }
    }
}
