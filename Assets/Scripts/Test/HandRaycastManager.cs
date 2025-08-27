using MRTK.Extensions;
using Oversight.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Pose = UnityEngine.Pose;

namespace Test
{
    [Serializable]
    public enum TestRaycastMode
    {
        Move,
        Adjust
    }

    public class HandRaycastManager : MonoBehaviour
    {
        public TestCesium _gameManager;
        [SerializeField] private HandRaycastHandler _raycastHandler;

        public TestRaycastMode _rayMode = TestRaycastMode.Move;

        // 定義模式對應的名稱
        private readonly Dictionary<TestRaycastMode, string> modeMessages = new Dictionary<TestRaycastMode, string>
        {
            { TestRaycastMode.Move, "移動" },
            { TestRaycastMode.Adjust, "微調" }
        };

        private void Start()
        {
            _raycastHandler.OnSelectEntered += HandleSelectEntered;
            _raycastHandler.OnSelectExited += HandleSelectExited;
            _raycastHandler.OnHoverEntered += HandleHoverEntered;
            _raycastHandler.OnHoverExited += HandleHoverExited;
        }

        private void OnDestroy()
        {
            if (_raycastHandler != null)
            {
                _raycastHandler.OnSelectEntered -= HandleSelectEntered;
                _raycastHandler.OnSelectExited -= HandleSelectExited;
                _raycastHandler.OnHoverEntered -= HandleHoverEntered;
                _raycastHandler.OnHoverExited -= HandleHoverExited;
            }
        }

        public void HandleHoverEntered(GameObject hitObject)
        {
            switch (_rayMode)
            {
                default:
                    break;
            }
        }

        public void HandleHoverExited(GameObject hitObject)
        {
            switch (_rayMode)
            {
                default:
                    break;
            }
        }

        public void HandleSelectEntered(GameObject hitObject, Pose pose)
        {
            switch(_rayMode)
            {
                default:
                    break;
            }
        }

        public void HandleSelectExited(GameObject hitObject, Pose pose)
        {
            Debug.Log($"{_rayMode.ToString()}");
            switch (_rayMode)
            {
                case TestRaycastMode.Adjust:
                    _gameManager.AddBoundControl(hitObject, pose);
                    break;
                default:
                    break;
            }
        }

        public void GoToNextMode()
        {
            int rayModeCount = Enum.GetValues(typeof(TestRaycastMode)).Length;
            SwitchToMode((TestRaycastMode)(((int)_rayMode + 1) % rayModeCount));
        }

        public void SwitchToMode(TestRaycastMode raycastMode)
        {
            // 計算下一個模式（輪迴）並通知訂閱者模式更換
            _rayMode = raycastMode;
            string modeName = modeMessages.TryGetValue(_rayMode, out string message) ? message : "未知";
            Debug.Log($"switch to {modeName}");
        }

    }
}

