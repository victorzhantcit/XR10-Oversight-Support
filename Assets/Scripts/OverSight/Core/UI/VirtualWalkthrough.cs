using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using Oversight.Core;
using System.Collections;
using UnityEngine;

public class VirtualWalkthrough : MonoBehaviour
{
    [SerializeField] private BuildingViewer _buildingViewer;
    [SerializeField] private HandConstraintPalmUp _handConstraintPalmUp;
    private float _moveSpeed = 1f;

    private Coroutine _movingCoroutine = null;
    private bool _previousLockState;

    /// <summary>
    /// 鎖定模型位置後開始虛擬漫遊，此函式目前僅綁定於按鈕上
    /// </summary>
    public void StartVirtualWalking()
    {
        if (_movingCoroutine == null)
        {
            // 訂閱手掌偵測事件
            if (_handConstraintPalmUp != null)
                _handConstraintPalmUp.OnFirstHandDetected.AddListener(OnPalmUpDetected);

            gameObject.SetActive(true);
            _movingCoroutine = StartCoroutine(MovingFoward());
            _previousLockState = _buildingViewer.IsManipulatorLocked;
            _buildingViewer.LockModelPose(true);
        }
    }

    public void StopVirtualWalking()
    {
        if (_movingCoroutine != null)
        {
            // 解除手掌偵測事件
            if (_handConstraintPalmUp != null)
                _handConstraintPalmUp.OnFirstHandDetected.RemoveListener(OnPalmUpDetected);

            StopCoroutine(_movingCoroutine);
            _movingCoroutine = null;
            _buildingViewer.LockModelPose(_previousLockState);
            gameObject.SetActive(false);
        }
    }

    private void OnPalmUpDetected()
    {
        StopVirtualWalking();
    }

    public void OnWalkSpeedSliderUpdate(SliderEventData eventData)
        => _moveSpeed = eventData.NewValue;

    private IEnumerator MovingFoward()
    {
        if (Camera.main == null)
        {
            Debug.LogError("主攝影機未找到，停止虛擬漫遊。");
            StopVirtualWalking();
            yield break;
        }

        Transform cameraOffset = Camera.main.transform.parent;
        while (true)
        {
            Vector3 forwardDirection = Camera.main.transform.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();

            cameraOffset.position += forwardDirection * _moveSpeed * Time.deltaTime;

            yield return null; // 每幀更新一次
        }
    }
}
