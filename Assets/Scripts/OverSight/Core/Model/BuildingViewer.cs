using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using Oversight.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Extensions;
using UnityEngine;

namespace Oversight.Core
{
    [Serializable]
    public enum RGFloor
    {
        // 格式: 大樓名稱_樓層
        RG_BBF,
        RG_B3F,
        RG_B2F,
        RG_B1F,
        RG_1F,
        RG_2F,
        RG_3F,
        RG_4F,
        RG_5F,
        RG_6F,
        RG_7F,
        RG_8F,
        RG_9F,
        RG_10F,
        RG_R1F,
        RG_R2F,
        ALL,
        ALL_FS, // 使用者、Slider不需要考慮到這個值，程式內呼叫
        ALL_PS,
        ALL_AC
    }



    [Serializable]
    public class SliceData
    {
        public RGFloor Floor;
        public float CellingValue;
    }


    public class BuildingViewer : EnumStateVisualizer<RGFloor>
    {
        public string BuildingCode = string.Empty;
        public string BuildingName = string.Empty;
        public bool IsManipulatorLocked { get; private set; } = false;
        [SerializeField] private PressableButton _lockManipulatorButton;
        [SerializeField] private PressableButton _scalerSwitchButton;
        [SerializeField] private TMP_Text _currentScaleLabel;
        [SerializeField] private Slider _floorSlider;
        [SerializeField] private Slider _systemSlider;
        [SerializeField] private Transform _floorR2FCelling;
        [SerializeField] private Transform _floorLabelParent; // 樓層的文字集合，子物件順序必需與 enum RGFloor 一致
        [SerializeField] private Transform _systemLabelParent; // 系統的文字集合，子物件順序必需與 enum RGSystem 一致
        //private Vector3 initialCameraToObjectVector; // 相機與物件的初始相對向量
        //private Quaternion initialCameraRotation;   // 相機的初始旋轉
        //private Transform cameraTransform;          // 相機的 Transform
        private float _initScale;
        private MoveAxisConstraint _moveAxisConstraint;
        private RotationAxisConstraint _rotationAxisConstraint;
        private MinMaxScaleConstraint _minMaxScaleConstraint;
        private TMP_Text[] _systemLabels;
        private TMP_Text[] _floorLabels;
        private FloorViewer _floorViewer;
        private RGFloor _selectedFloor = RGFloor.ALL;
        public string SelectedFloor
            => (_selectedFloor != RGFloor.ALL) ? _selectedFloor.ToString().Split("_")[1] : string.Empty;
        private BuildingSystem _selectedSystem = BuildingSystem.ALL;
        private bool _init = false;
        private Dictionary<BuildingSystem, RGFloor> _allFloorReplaceMap = new Dictionary<BuildingSystem, RGFloor>()
        {
            { BuildingSystem.AC, RGFloor.ALL_AC },
            { BuildingSystem.PS, RGFloor.ALL_PS },
            { BuildingSystem.FS, RGFloor.ALL_FS },
        };

        // 模型
        [SerializeField] private List<QRAnchorDto> _anchorPoses;

        private new void Start() // hide base.Start()
        {
            _moveAxisConstraint = GetComponent<MoveAxisConstraint>();
            _rotationAxisConstraint = GetComponent<RotationAxisConstraint>();
            _minMaxScaleConstraint = GetComponent<MinMaxScaleConstraint>();
            _initScale = this.transform.localScale.x;
            _init = true;
            ShowFloor(RGFloor.ALL);
        }

        public void ShowFloor(RGFloor floor)
        {
            _selectedFloor = floor;
            // 非全樓層，直接更新單一樓層
            if (floor != RGFloor.ALL)
            {
                var floorInfo = base.SetEnumValue(floor);
                ShowFloorComponent(floorInfo, _selectedSystem);
                return;
            }

            // 全樓層，ALL/AR系統
            if (_selectedSystem == BuildingSystem.ALL || _selectedSystem == BuildingSystem.AR)
            {
                ShowWholeBuilding(_selectedSystem == BuildingSystem.ALL);
                return;
            }

            // 全樓層，Tris過大 獨立顯示黏合減面處理後的模型
            if (_allFloorReplaceMap.ContainsKey(_selectedSystem))
            {
                base.SetEnumValue(_allFloorReplaceMap[_selectedSystem]);
                return;
            }

            // 全樓層，其餘按樓層逐一啟用
            base.ActivateAll(false);
            foreach (RGFloor targetFloor in (RGFloor[])Enum.GetValues(typeof(RGFloor)))
            {
                if (targetFloor == RGFloor.ALL || IsTypeAllFloorSystem(targetFloor)) continue;
                var floorInfo = base.SetEnumValue(targetFloor, true, false);
                ShowFloorComponent(floorInfo, _selectedSystem);
            }
            Physics.SyncTransforms();
        }

        private bool IsTypeAllFloorSystem(RGFloor targetFloor)
        {
            foreach (var kvp in _allFloorReplaceMap)
                if (kvp.Value == targetFloor) return true;
            return false;
        }

        private void ShowWholeBuilding(bool includeOutsideMEPs = true)
        {
            base.SetEnumValue(RGFloor.ALL);
            if (includeOutsideMEPs)
            {
                var floorInfo = base.SetEnumValue(RGFloor.RG_R1F, true, false);
                ShowFloorComponent(floorInfo, BuildingSystem.AC);
            }
            else
                base.SetEnumValue(RGFloor.RG_R1F, false, false);
        }

        public void OnFloorSliderUpdate(SliderEventData eventData)
        {
            if (!_init) return;

            if (_floorLabels == null)
                _floorLabels = _floorLabelParent.GetComponentsInChildren<TMP_Text>();

            // 確保值為整數並檢查合法範圍
            int floorIndex = Mathf.RoundToInt(eventData.NewValue); // 轉為整數

            // 檢查 floorIndex 是否在枚舉範圍內
            if (Enum.IsDefined(typeof(RGFloor), floorIndex))
            {
                RGFloor floor = (RGFloor)floorIndex;
                HighLightSliderLabel(_floorLabels, floorIndex, floor == RGFloor.ALL);
                ShowFloor(floor);
                //Debug.Log($"當前樓層: {floor}");
            }
            else
            {
                Debug.LogWarning($"無效的樓層索引: {floorIndex}");
            }
        }

        public void OnRGSystemSliderUpdate(SliderEventData eventData)
        {
            if (!_init) return;

            // 初始化系統標籤集合
            if (_systemLabels == null)
                _systemLabels = _systemLabelParent.GetComponentsInChildren<TMP_Text>();

            // 確保滑動條值為整數，並轉換為系統索引
            int systemIndex = Mathf.RoundToInt(eventData.NewValue);

            // 檢查索引是否合法，無效索引直接返回
            if (!Enum.IsDefined(typeof(BuildingSystem), systemIndex))
            {
                Debug.LogWarning($"無效的系統索引: {systemIndex}");
                return;
            }

            // 更新選中的系統UI
            BuildingSystem selectedSystem = (BuildingSystem)systemIndex;
            HighLightSliderLabel(_systemLabels, systemIndex, selectedSystem == BuildingSystem.ALL);
            _selectedSystem = selectedSystem;

            // 不是全樓層，直接更新單一樓層系統
            if (_selectedFloor != RGFloor.ALL)
            {
                _floorViewer.SetEnumValue(selectedSystem);
                return;
            }

            // 全樓層，ALL/AR系統
            if (selectedSystem == BuildingSystem.ALL || selectedSystem == BuildingSystem.AR)
            {
                ShowWholeBuilding(selectedSystem == BuildingSystem.ALL);
                return;
            }

            // 全樓層，Tris過大 獨立顯示黏合減面處理後的模型
            if (_allFloorReplaceMap.ContainsKey(_selectedSystem))
            {
                base.SetEnumValue(_allFloorReplaceMap[_selectedSystem]);
                return;
            }

            // 全樓層，MEP系統(AC/EE/WE/PS/FS)
            base.ActivateAll(false);
            foreach (RGFloor targetFloor in Enum.GetValues(typeof(RGFloor)).Cast<RGFloor>())
            {
                if (targetFloor == RGFloor.ALL || IsTypeAllFloorSystem(targetFloor)) continue;

                // 獲取樓層資訊並顯示對應的系統元件
                var floorInfo = base.SetEnumValue(targetFloor, true, false);
                ShowFloorComponent(floorInfo, _selectedSystem);
            }

            //Debug.Log($"當前系統: {selectedSystem}");
        }

        private void HighLightSliderLabel(TMP_Text[] labels, int floorIndex, bool highlightAll = false)
        {
            for (int i = 0; i < labels.Length; i++)
                labels[i].color = (i == floorIndex || highlightAll) ? Color.white : Color.gray;
        }

        /// <summary>
        /// 檢查 Floor 的設置 (應有 FloorViwer script 在第一個 GameObject 上)
        /// </summary>
        /// <param name="floorInfo"></param>
        /// <returns>是否有正確設置 FloorViewer，若有會將 FloorViewer 回傳至<see cref="_floorViewer"/></returns>
        private bool ValidateFloor(EnumVisualMapping floorInfo)
        {
            // 檢查 VisualObjects 是否有效
            if (floorInfo == null || floorInfo.VisualObjects == null || floorInfo.VisualObjects.Length == 0)
            {
                Debug.LogError($"No visual objects found for floor {_selectedFloor}.");
                return false;
            }

            // 嘗試取得 FloorViewer 組件
            if (!floorInfo.VisualObjects[0].TryGetComponent(out _floorViewer))
            {
                Debug.LogError($"Cannot get FloorViewer component on the first visual object for floor {_selectedFloor}.");
                return false;
            }
            return true;
        }

        private void ShowFloorComponent(EnumVisualMapping visualMapping, BuildingSystem rGSystem)
        {
            if (ValidateFloor(visualMapping))
                _floorViewer.SetEnumValue(rGSystem);
        }

        public bool AnchorModelToPose(Pose pose, string anchorTag)
        {
            // 設置 QR code 對應的樓層
            string floor = anchorTag.Split('/')[1];
            RGFloor enumFloor = MatchStringToEnum(floor);
            _floorSlider.Value = (int)enumFloor;
            _systemSlider.Value = (int)BuildingSystem.ALL;

            // 檢查 QR code 對應的定位點是否存在
            QRAnchorDto qrAnchorInfo = _anchorPoses.FirstOrDefault(info => info.Tag == anchorTag);
            Transform modelAnchorTransform;
            if (qrAnchorInfo == null) return false;
            modelAnchorTransform = qrAnchorInfo.QRPose;

            // 應用縮放：讓模型的比例初始化 (1:1:1)
            this.transform.localScale = Vector3.one;

            // 應用旋轉：保持模型的旋轉與 pose.rotation 一致
            Vector3 poseForwardVector = pose.rotation * Vector3.forward;
            Quaternion adjustedRotation = Quaternion.LookRotation(poseForwardVector);
            this.transform.rotation = adjustedRotation * Quaternion.Inverse(modelAnchorTransform.localRotation);

            // 應用位移：保持模型的位移與設置於場景中的定位點一致
            this.transform.position = pose.position;
            this.transform.position -= modelAnchorTransform.position - this.transform.position;

            // 更新模型顯示、UI和物理
            this.gameObject.SetActive(true);
            _lockManipulatorButton.ForceSetToggled(true);
            Physics.SyncTransforms();

            //Debug.Log($"模型錨點已根據掃描的 pose 對齊，新的根物件位置: {this.transform.position}, 旋轉: {this.transform.rotation}");
            return true;
        }

        private RGFloor MatchStringToEnum(string floorString)
        {
            foreach (RGFloor enumFloor in Enum.GetValues(typeof(RGFloor)).Cast<RGFloor>())
            {
                if (enumFloor.ToString() == $"RG_{floorString}")
                    return enumFloor;
            }
            return RGFloor.ALL;
        }

        /// <summary>
        /// 鎖定模型的所有移動方式的觸發函式，由<seealso cref="PressableButton"/>訂閱
        /// </summary>
        /// <param name="isLocked">是否鎖定模型</param>
        public void LockModelPose(bool isLocked)
        {
            if (isLocked != _lockManipulatorButton.IsToggled)
                _lockManipulatorButton.ForceSetToggled(isLocked);
            Vector3 currentScale = Vector3.one * transform.localScale.x;
            AxisFlags constraintOnEveryAxis = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
            _moveAxisConstraint.ConstraintOnMovement = isLocked ? constraintOnEveryAxis : AxisFlags.None;
            _rotationAxisConstraint.ConstraintOnRotation = isLocked ? constraintOnEveryAxis : AxisFlags.XAxis | AxisFlags.ZAxis;
            _minMaxScaleConstraint.MinimumScale = currentScale;
            _minMaxScaleConstraint.MaximumScale = currentScale;
            _minMaxScaleConstraint.enabled = isLocked;
            IsManipulatorLocked = isLocked;
        }

        /// <summary>
        /// 快速設定模型縮放的觸發函式，由<seealso cref="PressableButton"/>訂閱
        /// </summary>
        /// <param name="scale">縮放的比例<seealso cref="Transform.localScale"/></param>
        public void SetModelScaler(float scale)
        {
            if (_lockManipulatorButton.IsToggled)
            {
                _scalerSwitchButton.ForceSetToggled(!_scalerSwitchButton);
                return;
            }
            this.transform.localScale = Vector3.one * scale;
            UpdateModelScaleDisplay();
        }

        public void ResetModelPose()
        {
            //if (cameraTransform == null)
            //{
            //    Debug.LogError("相機未設置！");
            //    return;
            //}

            //// 2. 計算相機當前的朝向差異
            //Quaternion currentCameraRotation = cameraTransform.rotation;
            //Quaternion rotationDifference = Quaternion.Inverse(initialCameraRotation) * currentCameraRotation;

            //// 3. 恢復相對位置：旋轉初始向量以適配相機當前朝向
            //Vector3 adjustedPosition = cameraTransform.position + rotationDifference * initialCameraToObjectVector;

            //_lockManipulatorButton.ForceSetToggled(false);
            //// 4. 更新物件的位置
            //this.transform.position = adjustedPosition;

            //Debug.Log("物件的位置已恢復到與相機的初始相對位置和朝向一致");
            //this.transform.localScale = Vector3.one * _initScale;
            //UpdateModelScaleDisplay();
        }

        public void UpdateModelScaleDisplay()
        {
            _currentScaleLabel.text = $"縮放 1:{(1 / transform.localScale.x):F0}";
            //Debug.Log($"縮放 1:{(1 / transform.localScale.x):F0}");
        }
    }

}
