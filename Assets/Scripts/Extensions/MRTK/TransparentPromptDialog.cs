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

        private void Start() => ShowBeforePalmUp("�ݦV��x�H�}�ҥ\����");

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

            // �T�O���|���ƭq�\
            _handConstraintPalmUp.OnFirstHandDetected.RemoveListener(OnPalmUpDetected);

            // ���𰻴�
            StartCoroutine(DelayDetectPalmUp());
        }

        private IEnumerator DelayDetectPalmUp()
        {
            yield return new WaitForSeconds(1f);

            // �T�O���󤴵M�ҥ�
            if (!gameObject.activeSelf) yield break;

            // �}�l�����ⳡ���A
            _handConstraintPalmUp.OnFirstHandDetected.AddListener(OnPalmUpDetected);
        }

        public void OnPalmUpDetected()
        {
            // �����ƥ��ť�A�����Ĳ�o
            _handConstraintPalmUp.OnFirstHandDetected.RemoveListener(OnPalmUpDetected);

            // ���ô��ܨð���^��
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
