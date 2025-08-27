using MixedReality.Toolkit.UX;
using MRTK.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oversight.Dtos;

namespace Oversight.Utils
{
    public class NoteListItem : VirtualListItem<NoteDto>
    {
        [SerializeField] private Image _backPlateImage;
        [SerializeField] private TMP_Text _locationLabel;
        [SerializeField] private TMP_Text _timeLabel;
        [SerializeField] private RectTransform _pictureIcon;
        [SerializeField] private TMP_Text _pictureAmount;
        [SerializeField] private TMP_Text _deviceLabel;
        [SerializeField] private TMP_Text _descriptionLabel;
        [SerializeField] private PressableButton _editButton;
        [SerializeField] private PressableButton _turnToRepairButton;
        [SerializeField] private TMP_Text _turnToRepairLabel;
        private Action _editAction = null;
        private Action _toRepairAction = null;

        public override void SetContent(NoteDto note, int _ = -1, bool __ = false)
        {
            bool hasPicture = note.PhotoBase64.Count > 0;
            _pictureIcon.gameObject.SetActive(hasPicture);
            _pictureAmount.gameObject.SetActive(hasPicture);

            _locationLabel.text = note.Location;
            _timeLabel.text = note.Time;
            _pictureAmount.text = $"({note.PhotoBase64.Count})";
            _deviceLabel.text = string.IsNullOrEmpty(note.CodeOfDevice) ? "<color=\"grey\">無特定設備" : note.CodeOfDevice;
            _descriptionLabel.text = note.Description;
            _turnToRepairButton.enabled = !note.WaitForUploadToRepair;
            _turnToRepairLabel.text = note.WaitForUploadToRepair ? "<color=yellow>待報修上傳" : "轉報修單";
        }

        public void SetColor(Color color) => _backPlateImage.color = color;

        public void SetEditAction(Action editAction) => _editAction = editAction;

        public void OnEnterEditorClicked() => _editAction?.Invoke();

        public void SetTurnToRepairAction(Action toRepairAction) => _toRepairAction = toRepairAction;

        public void OnTurnToRepairClicked() => _toRepairAction?.Invoke();
    }
}
