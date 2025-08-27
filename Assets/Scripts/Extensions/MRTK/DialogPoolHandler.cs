using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using UnityEngine;
using Unity.Extensions;

namespace MRTK.Extensions
{
    public class DialogPoolHandler : MonoBehaviour
    {
        [SerializeField] private Transform _dialogPoolParent;
        [SerializeField] private Dialog _dialog;
        private ObjectPool<Dialog> _dialogPool;

        private Queue<DialogRequest> _dialogQueue = new Queue<DialogRequest>();
        private bool _isDialogShowing = false;

        private void Start() => _dialogPool = new ObjectPool<Dialog>(_dialog, _dialogPoolParent);

        public void EnqueueDialog(string title, string message = null, Action confirmAction = null, Action cancelAction = null)
        {
            _dialogQueue.Enqueue(new DialogRequest(title, message, confirmAction, cancelAction));
            ShowNextDialog();
        }

        public void EnqueueConditionalDialog(string title, Func<bool> condition, Action confirmAction = null, bool isLoading = false)
        {
            _dialogQueue.Enqueue(new ConditionalDialogRequest(title, condition, confirmAction, isLoading));
            ShowNextDialog();
        }

        private void ShowNextDialog()
        {
            if (_isDialogShowing || _dialogQueue.Count == 0) return;

            _isDialogShowing = true;

            var request = _dialogQueue.Dequeue();

            if (request is ConditionalDialogRequest conditionalRequest)
            {
                ShowConditionalDialog(conditionalRequest);
            }
            else
            {
                ShowStandardDialog((DialogRequest)request);
            }
        }

        private void ShowStandardDialog(DialogRequest request)
        {
            Dialog dialog = _dialogPool.Get();
            dialog.Reset();
            dialog.SetHeader(request.Title);

            if (!string.IsNullOrEmpty(request.Message))
                dialog.SetBody(request.Message);

            SetDialogButtons(dialog, request.ConfirmAction, request.CancelAction);
            dialog.ShowAsync();
        }

        private void ShowConditionalDialog(ConditionalDialogRequest request)
        {
            Dialog dialog = _dialogPool.Get();
            dialog.Reset();
            dialog.SetHeader(request.Title);

            if (request.IsLoading)
            {
                dialog.SetBody("正在加載中...");
            }

            SetDialogButtons(dialog, request.ConfirmAction, null);
            dialog.ShowAsync();

            StartCoroutine(WaitForConditionAndClose(dialog, request.Condition));
        }

        private void SetDialogButtons(Dialog dialog, Action confirmAction, Action cancelAction)
        {
            dialog.SetPositive("確定", (args) =>
            {
                confirmAction?.Invoke();
                StartCoroutine(ReleaseDialogAfterDisabled(dialog));
            });

            if (cancelAction != null)
            {
                dialog.SetNegative("取消", (args) =>
                {
                    cancelAction.Invoke();
                    StartCoroutine(ReleaseDialogAfterDisabled(dialog));
                });
            }
        }

        private IEnumerator WaitForConditionAndClose(Dialog dialog, Func<bool> condition)
        {
            yield return new WaitUntil(condition); // 等待條件達成
            StartCoroutine(ReleaseDialogAfterDisabled(dialog));
        }

        private IEnumerator ReleaseDialogAfterDisabled(Dialog dialog)
        {
            yield return new WaitUntil(() => !dialog.isActiveAndEnabled);
            _dialogPool.Release(dialog);
            _isDialogShowing = false;
            ShowNextDialog(); // 顯示下一個對話框
        }

        private class DialogRequest
        {
            public string Title { get; }
            public string Message { get; }
            public Action ConfirmAction { get; }
            public Action CancelAction { get; }

            public DialogRequest(string title, string message, Action confirmAction, Action cancelAction)
            {
                Title = title;
                Message = message;
                ConfirmAction = confirmAction;
                CancelAction = cancelAction;
            }
        }

        private class ConditionalDialogRequest : DialogRequest
        {
            public Func<bool> Condition { get; }
            public bool IsLoading { get; }

            public ConditionalDialogRequest(string title, Func<bool> condition, Action confirmAction, bool isLoading)
                : base(title, null, confirmAction, null)
            {
                Condition = condition;
                IsLoading = isLoading;
            }
        }
    }
}
