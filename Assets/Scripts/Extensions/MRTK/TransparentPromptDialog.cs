using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MRTK.Extensions
{
    public class TransparentPromptDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _overlayHint;
        [SerializeField] private Image _overlayQrHint;
        [SerializeField] private PressableButton _cancelScanButton; 
        [SerializeField] private HandConstraintPalmUp _handConstraintPalmUp;
        private Action _palmUpCallback = null;
        private Action _cancelAction = null;

        private void Start() => ShowBeforePalmUp("看向手掌以開啟功能選單");

        public void Setup(bool activate, string message = "", bool isQrHint = false, Action cancelAction = null)
        {
            this.gameObject.SetActive(activate);
            _overlayHint.text = message;
            _overlayQrHint.enabled = isQrHint;
            _cancelScanButton.gameObject.SetActive(cancelAction != null);
            _cancelAction = _cancelAction = cancelAction ?? (() => Debug.Log("Cancel button clicked, but no action defined."));
        }

        public void Test()
        {
            ShowBeforePalmUp("Test");
        }

        public void ShowBeforePalmUp(string message, Action onPalmUp = null)
        {
            Setup(true, message);
            _palmUpCallback = onPalmUp;

            // 確保不會重複訂閱
            _handConstraintPalmUp.OnFirstHandDetected.RemoveListener(OnPalmUpDetected);

            // 延遲偵測
            StartCoroutine(DelayDetectPalmUp());
        }

        private IEnumerator DelayDetectPalmUp()
        {
            yield return new WaitForSeconds(1f);

            // 確保物件仍然啟用
            if (!gameObject.activeSelf) yield break;

            // 開始偵測手部狀態
            _handConstraintPalmUp.OnFirstHandDetected.AddListener(OnPalmUpDetected);
        }

        public void OnPalmUpDetected()
        {
            // 移除事件監聽，防止重複觸發
            _handConstraintPalmUp.OnFirstHandDetected.RemoveListener(OnPalmUpDetected);

            // 隱藏提示並執行回調
            Setup(false);
            _palmUpCallback?.Invoke();
        }


        public void Activate(bool activate)
        {
            Setup(activate, "...");
        }

        public void SetText(string text)
        {
            _overlayHint.text = text;
        }

        public void SetHintColor(Color color) => _overlayHint.color = color;

        public void DelayDeactivate()
        {
            if (!gameObject.activeSelf) return;

            StartCoroutine(DeactivateInSeconds(3f));
        }

        private IEnumerator DeactivateInSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Setup(false);
        }

        public void OnCancelButtonClicked() => _cancelAction?.Invoke();
    }
}
