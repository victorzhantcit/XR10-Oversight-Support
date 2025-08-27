using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using Oversight.Core;
using System;
using Unity.Extensions;
using UnityEngine;
using Pose = UnityEngine.Pose;


namespace Oversight.Raycast
{
    [Serializable]
    public enum RaycastMode
    {
        Move,
        BIM,
        Measuring
    }

    public class HandRaycastManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private HandRaycastHandler _raycastHandler;
        [SerializeField] private UIController _uiController;
        [SerializeField] private TransparentPromptDialog _promptDialog;
        [SerializeField] private PressableButton _mrMeshToggle;

        public RaycastMode _rayMode = RaycastMode.Move;

        // BIM
        [Header("HighLight Object")]
        [SerializeField] private Transform _highlightCube;
        private float HighLightCubeAppend = 0.001f;

        // Measuring
        [Header("Measurement")]
        [SerializeField] private PressableButton _measureUndo;
        [SerializeField] private PressableButton _measureRedo;
        [SerializeField] private Transform _measureDisplay;
        [SerializeField] private DistanceMeasurement _measurePrefab;
        private DistanceMeasurement _currentMarker = null;
        private HistoryRecorder<MeasureRecordData> _historyRecorder;
        private ObjectPool<DistanceMeasurement> _measurementPool;

        // Anchor object to scene by MR Mesh
        private Transform _anchorTransform;
        private bool _anchorIgnoreRotation;

        private void Start()
        {
            _raycastHandler.OnSelectEntered += HandleSelectEntered;
            _raycastHandler.OnSelectExited += HandleSelectExited;
            _raycastHandler.OnHoverEntered += HandleHoverEntered;
            _raycastHandler.OnHoverExited += HandleHoverExited;

            // 初始化物件池
            _measurementPool = new ObjectPool<DistanceMeasurement>(_measurePrefab, _measureDisplay);

            // 初始化 HistoryRecorder
            _historyRecorder = new HistoryRecorder<MeasureRecordData>(
                undoAction: UndoMeasurementPoint,
                redoAction: RedoMeasurementPoint
            );

            // 初始化按鈕狀態
            UpdateMeasureButtons();
        }

        public void HandleHoverEntered(GameObject hitObject)
        {
            if (_rayMode == RaycastMode.BIM)
                SetCubeToMatchBoxCollider(hitObject.GetComponent<BoxCollider>(), _highlightCube);
        }

        public void HandleHoverExited(GameObject hitObject)
        {
            if (_rayMode == RaycastMode.BIM)
                _highlightCube.gameObject.SetActive(false);
        }

        public void HandleSelectEntered(GameObject hitObject, Pose pose)
        {
            if (_anchorTransform != null)
            {
                HandleAnchorObjectToPose(pose);
                return;
            }

            if (_rayMode == RaycastMode.Measuring)
                Measuring(pose);
        }

        public void HandleSelectExited(GameObject hitObject, Pose pose)
        {
            if (_rayMode == RaycastMode.BIM)
                _uiController.ShowBIMInfo(hitObject.name, false);
        }

        private void SetCubeToMatchBoxCollider(BoxCollider boxCollider, Transform cube)
        {
            if (boxCollider == null || cube == null)
            {
                Debug.LogWarning("BoxCollider or Cube is null.");
                return;
            }

            cube.SetParent(null);
            cube.position = boxCollider.bounds.center;
            cube.rotation = boxCollider.transform.rotation;
            cube.localScale = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) + Vector3.one * HighLightCubeAppend;
            cube.gameObject.SetActive(true);
        }

        public void SwitchToBIMMode() => SwitchToMode(RaycastMode.BIM);
        public void SwitchToMoveMode() => SwitchToMode(RaycastMode.Move);
        public void SwitchToMeasuringMode() => SwitchToMode(RaycastMode.Measuring);

        public void SwitchToMode(RaycastMode raycastMode)
        {
            _rayMode = raycastMode;
        }

        private void Measuring(Pose pose)
        {
            if (_currentMarker == null)
            {
                _currentMarker = _measurementPool.Get(); // 從物件池獲取
                _currentMarker.transform.SetParent(_measureDisplay);
                _currentMarker.SetFirstPoint(pose);

                _historyRecorder.AddToHistory(new MeasureRecordData(_currentMarker.transform, pose, true, _currentMarker)); // 記錄第一點
            }
            else
            {
                _currentMarker.SetSecondPoint(pose);

                _historyRecorder.AddToHistory(new MeasureRecordData(_currentMarker.transform, pose, false, _currentMarker)); // 記錄第二點
                _currentMarker = null;
            }

            UpdateMeasureButtons(); // 更新按鈕狀態
        }


        private void UndoMeasurementPoint(MeasureRecordData record)
        {
            var historyMarker = record.Anchor.GetComponent<DistanceMeasurement>();
            if (historyMarker == null) return;

            if (record.IsFirstPoint)
            {
                historyMarker.ClearFirstPoint();
                historyMarker.ResetState();
                _measurementPool.Release(historyMarker); // ⭐ 使用物件池回收
                _currentMarker = null; // 確保回收後不會引用已被釋放的物件
            }
            else
            {
                historyMarker.ClearSecondPoint();
                _currentMarker = historyMarker; // **撤銷第二點時，保留 `_currentMarker`**
            }

            //record.SavedMeasurement = historyMarker; // ⭐ 儲存物件，Redo 時復用
            UpdateMeasureButtons();
        }


        private void RedoMeasurementPoint(MeasureRecordData record)
        {
            if (record.IsFirstPoint)
            {
                var historyMarker = _measurementPool.Get();
                _currentMarker = historyMarker; // ⭐ 還原第一點時，讓 _currentMarker 指向該物件
                historyMarker.SetFirstPoint(record.Pose);
                historyMarker.gameObject.SetActive(true);
            }
            else
            {
                _currentMarker.SetSecondPoint(record.Pose);
                _currentMarker = null; // ⭐ 保留 _currentMarker，避免錯誤重置
            }

            UpdateMeasureButtons();
        }


        public void MeasuringUndo() => _historyRecorder.Undo();

        public void MeasuringRedo() => _historyRecorder.Redo();

        public void ClearAllMeasurements()
        {
            foreach (Transform measureComponent in _measureDisplay)
                Destroy(measureComponent.gameObject);

            _measurementPool.Clear();
            _historyRecorder.ClearHistory();

            UpdateMeasureButtons(); // 更新按鈕狀態
        }

        private void UpdateMeasureButtons()
        {
            _measureUndo.enabled = _historyRecorder.CanUndo;
            _measureRedo.enabled = _historyRecorder.CanRedo;
        }

        // 將物件以 MR Mesh 來定位於空間中
        public void RequestAnchorObject(Transform transform, bool ignoreRotation = false)
        {
#if UNITY_EDITOR
            transform.position = new Vector3(-0.02f, 1.5f, 1f);
#else
            if (transform == null)
            {
                Debug.LogWarning("RequestAnchorObject: Transform 為 null，無法進行定位。");
                return;
            }

            _mrMeshToggle.ForceSetToggled(true);
            _anchorTransform = transform;
            _anchorIgnoreRotation = ignoreRotation;

            _promptDialog.Setup(true, "點擊空間中的平面放置模型", cancelAction: () =>
            {
                _mrMeshToggle.ForceSetToggled(false);
                _anchorTransform = null;
                _promptDialog.Setup(false);
            });
#endif
        }

        private void HandleAnchorObjectToPose(Pose pose)
        {
            if (_anchorTransform != null)
            {
                // 獲取使用者的面向方向
                Vector3 userForward = Camera.main.transform.forward;
                userForward.y = 0; // 確保只考慮水平旋轉
                Quaternion lookRotation = Quaternion.LookRotation(userForward);

                // 根據 _anchorIgnoreRotation 決定是否設定旋轉
                if (_anchorIgnoreRotation)
                {
                    _anchorTransform.position = pose.position;
                }
                else
                {
                    // 只調整 Y 軸旋轉，保持其他軸的旋轉
                    Quaternion adjustedRotation = Quaternion.Euler(
                        _anchorTransform.rotation.eulerAngles.x,  // 保持原本的 X 軸旋轉
                        lookRotation.eulerAngles.y,              // 調整 Y 軸，使其面對使用者
                        _anchorTransform.rotation.eulerAngles.z   // 保持原本的 Z 軸旋轉
                    );

                    _anchorTransform.SetPositionAndRotation(pose.position, adjustedRotation);
                }
            }

            _promptDialog.Setup(false);
            _mrMeshToggle.ForceSetToggled(false);
            _anchorTransform = null;
        }


    }

    public class MeasureRecordData
    {
        public Transform Anchor { get; private set; }
        public Pose Pose { get; private set; }
        public bool IsFirstPoint { get; private set; }
        public DistanceMeasurement SavedMeasurement { get; set; } // ⭐ 儲存 Undo 時的測量物件

        public MeasureRecordData(Transform anchor, Pose pose, bool isFirstPoint, DistanceMeasurement savedMeasurement)
        {
            Anchor = anchor;
            Pose = pose;
            IsFirstPoint = isFirstPoint;
            SavedMeasurement = savedMeasurement;
        }
    }
}

