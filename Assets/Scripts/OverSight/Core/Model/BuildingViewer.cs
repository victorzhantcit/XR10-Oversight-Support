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
        // �榡: �j�ӦW��_�Ӽh
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
        ALL_FS, // �ϥΪ̡BSlider���ݭn�Ҽ{��o�ӭȡA�{�����I�s
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
        [SerializeField] private Transform _floorLabelParent; // �Ӽh����r���X�A�l���󶶧ǥ��ݻP enum RGFloor �@�P
        [SerializeField] private Transform _systemLabelParent; // �t�Ϊ���r���X�A�l���󶶧ǥ��ݻP enum RGSystem �@�P
        //private Vector3 initialCameraToObjectVector; // �۾��P���󪺪�l�۹�V�q
        //private Quaternion initialCameraRotation;   // �۾�����l����
        //private Transform cameraTransform;          // �۾��� Transform
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

        // �ҫ�
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
            // �D���Ӽh�A������s��@�Ӽh
            if (floor != RGFloor.ALL)
            {
                var floorInfo = base.SetEnumValue(floor);
                ShowFloorComponent(floorInfo, _selectedSystem);
                return;
            }

            // ���Ӽh�AALL/AR�t��
            if (_selectedSystem == BuildingSystem.ALL || _selectedSystem == BuildingSystem.AR)
            {
                ShowWholeBuilding(_selectedSystem == BuildingSystem.ALL);
                return;
            }

            // ���Ӽh�ATris�L�j �W������H�X��B�z�᪺�ҫ�
            if (_allFloorReplaceMap.ContainsKey(_selectedSystem))
            {
                base.SetEnumValue(_allFloorReplaceMap[_selectedSystem]);
                return;
            }

            // ���Ӽh�A��l���Ӽh�v�@�ҥ�
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

            // �T�O�Ȭ���ƨ��ˬd�X�k�d��
            int floorIndex = Mathf.RoundToInt(eventData.NewValue); // �ର���

            // �ˬd floorIndex �O�_�b�T�|�d��
            if (Enum.IsDefined(typeof(RGFloor), floorIndex))
            {
                RGFloor floor = (RGFloor)floorIndex;
                HighLightSliderLabel(_floorLabels, floorIndex, floor == RGFloor.ALL);
                ShowFloor(floor);
                //Debug.Log($"��e�Ӽh: {floor}");
            }
            else
            {
                Debug.LogWarning($"�L�Ī��Ӽh����: {floorIndex}");
            }
        }

        public void OnRGSystemSliderUpdate(SliderEventData eventData)
        {
            if (!_init) return;

            // ��l�ƨt�μ��Ҷ��X
            if (_systemLabels == null)
                _systemLabels = _systemLabelParent.GetComponentsInChildren<TMP_Text>();

            // �T�O�ưʱ��Ȭ���ơA���ഫ���t�ί���
            int systemIndex = Mathf.RoundToInt(eventData.NewValue);

            // �ˬd���ެO�_�X�k�A�L�į��ު�����^
            if (!Enum.IsDefined(typeof(BuildingSystem), systemIndex))
            {
                Debug.LogWarning($"�L�Ī��t�ί���: {systemIndex}");
                return;
            }

            // ��s�襤���t��UI
            BuildingSystem selectedSystem = (BuildingSystem)systemIndex;
            HighLightSliderLabel(_systemLabels, systemIndex, selectedSystem == BuildingSystem.ALL);
            _selectedSystem = selectedSystem;

            // ���O���Ӽh�A������s��@�Ӽh�t��
            if (_selectedFloor != RGFloor.ALL)
            {
                _floorViewer.SetEnumValue(selectedSystem);
                return;
            }

            // ���Ӽh�AALL/AR�t��
            if (selectedSystem == BuildingSystem.ALL || selectedSystem == BuildingSystem.AR)
            {
                ShowWholeBuilding(selectedSystem == BuildingSystem.ALL);
                return;
            }

            // ���Ӽh�ATris�L�j �W������H�X��B�z�᪺�ҫ�
            if (_allFloorReplaceMap.ContainsKey(_selectedSystem))
            {
                base.SetEnumValue(_allFloorReplaceMap[_selectedSystem]);
                return;
            }

            // ���Ӽh�AMEP�t��(AC/EE/WE/PS/FS)
            base.ActivateAll(false);
            foreach (RGFloor targetFloor in Enum.GetValues(typeof(RGFloor)).Cast<RGFloor>())
            {
                if (targetFloor == RGFloor.ALL || IsTypeAllFloorSystem(targetFloor)) continue;

                // ����Ӽh��T����ܹ������t�Τ���
                var floorInfo = base.SetEnumValue(targetFloor, true, false);
                ShowFloorComponent(floorInfo, _selectedSystem);
            }

            //Debug.Log($"��e�t��: {selectedSystem}");
        }

        private void HighLightSliderLabel(TMP_Text[] labels, int floorIndex, bool highlightAll = false)
        {
            for (int i = 0; i < labels.Length; i++)
                labels[i].color = (i == floorIndex || highlightAll) ? Color.white : Color.gray;
        }

        /// <summary>
        /// �ˬd Floor ���]�m (���� FloorViwer script �b�Ĥ@�� GameObject �W)
        /// </summary>
        /// <param name="floorInfo"></param>
        /// <returns>�O�_�����T�]�m FloorViewer�A�Y���|�N FloorViewer �^�Ǧ�<see cref="_floorViewer"/></returns>
        private bool ValidateFloor(EnumVisualMapping floorInfo)
        {
            // �ˬd VisualObjects �O�_����
            if (floorInfo == null || floorInfo.VisualObjects == null || floorInfo.VisualObjects.Length == 0)
            {
                Debug.LogError($"No visual objects found for floor {_selectedFloor}.");
                return false;
            }

            // ���ը��o FloorViewer �ե�
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
            // �]�m QR code �������Ӽh
            string floor = anchorTag.Split('/')[1];
            RGFloor enumFloor = MatchStringToEnum(floor);
            _floorSlider.Value = (int)enumFloor;
            _systemSlider.Value = (int)BuildingSystem.ALL;

            // �ˬd QR code �������w���I�O�_�s�b
            QRAnchorDto qrAnchorInfo = _anchorPoses.FirstOrDefault(info => info.Tag == anchorTag);
            Transform modelAnchorTransform;
            if (qrAnchorInfo == null) return false;
            modelAnchorTransform = qrAnchorInfo.QRPose;

            // �����Y��G���ҫ�����Ҫ�l�� (1:1:1)
            this.transform.localScale = Vector3.one;

            // ���α���G�O���ҫ�������P pose.rotation �@�P
            Vector3 poseForwardVector = pose.rotation * Vector3.forward;
            Quaternion adjustedRotation = Quaternion.LookRotation(poseForwardVector);
            this.transform.rotation = adjustedRotation * Quaternion.Inverse(modelAnchorTransform.localRotation);

            // ���Φ첾�G�O���ҫ����첾�P�]�m����������w���I�@�P
            this.transform.position = pose.position;
            this.transform.position -= modelAnchorTransform.position - this.transform.position;

            // ��s�ҫ���ܡBUI�M���z
            this.gameObject.SetActive(true);
            _lockManipulatorButton.ForceSetToggled(true);
            Physics.SyncTransforms();

            //Debug.Log($"�ҫ����I�w�ھڱ��y�� pose ����A�s���ڪ����m: {this.transform.position}, ����: {this.transform.rotation}");
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
        /// ��w�ҫ����Ҧ����ʤ覡��Ĳ�o�禡�A��<seealso cref="PressableButton"/>�q�\
        /// </summary>
        /// <param name="isLocked">�O�_��w�ҫ�</param>
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
        /// �ֳt�]�w�ҫ��Y��Ĳ�o�禡�A��<seealso cref="PressableButton"/>�q�\
        /// </summary>
        /// <param name="scale">�Y�񪺤��<seealso cref="Transform.localScale"/></param>
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
            //    Debug.LogError("�۾����]�m�I");
            //    return;
            //}

            //// 2. �p��۾���e���¦V�t��
            //Quaternion currentCameraRotation = cameraTransform.rotation;
            //Quaternion rotationDifference = Quaternion.Inverse(initialCameraRotation) * currentCameraRotation;

            //// 3. ��_�۹��m�G�����l�V�q�H�A�t�۾���e�¦V
            //Vector3 adjustedPosition = cameraTransform.position + rotationDifference * initialCameraToObjectVector;

            //_lockManipulatorButton.ForceSetToggled(false);
            //// 4. ��s���󪺦�m
            //this.transform.position = adjustedPosition;

            //Debug.Log("���󪺦�m�w��_��P�۾�����l�۹��m�M�¦V�@�P");
            //this.transform.localScale = Vector3.one * _initScale;
            //UpdateModelScaleDisplay();
        }

        public void UpdateModelScaleDisplay()
        {
            _currentScaleLabel.text = $"�Y�� 1:{(1 / transform.localScale.x):F0}";
            //Debug.Log($"�Y�� 1:{(1 / transform.localScale.x):F0}");
        }
    }

}
