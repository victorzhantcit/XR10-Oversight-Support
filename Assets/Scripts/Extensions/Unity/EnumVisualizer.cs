using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Extensions
{
    public abstract class EnumStateVisualizer<T> : MonoBehaviour where T : Enum
    {
        [Serializable]
        public class EnumVisualMapping
        {
            public T EnumValue; // �T�|��
            public GameObject[] VisualObjects; // ����������
        }

        [SerializeField]
        private List<EnumVisualMapping> _visualMappings = new List<EnumVisualMapping>();

        public List<EnumVisualMapping> VisualMappings => _visualMappings;

        private T _currentEnumValue;

        /// <summary>
        /// ����ثe�����A
        /// </summary>
        public T CurrentEnumValue => _currentEnumValue;

        /// <summary>
        /// ��l�ƨ�w�]��
        /// </summary>
        [SerializeField]
        private T _defaultEnumValue;

        protected virtual void Start()
        {
            // ��l�ƨ�w�]��
            SetEnumValue(_defaultEnumValue);
        }

        /// <summary>
        /// ��������w���T�|��
        /// </summary>
        public virtual EnumVisualMapping SetEnumValue(T enumValue, bool enable = true, bool hideOthers = true)
        {
            // �T�ΩҦ�����
            if (hideOthers)
                for (int i = 0; i < _visualMappings.Count; i++)
                    ActivateVisualObjects(_visualMappings[i].VisualObjects, false);

            // �d��P��e�T�|�Ȭ���������ñҥ�
            var selectedMapping = _visualMappings.FirstOrDefault(mapping => Equals(mapping.EnumValue, enumValue));
            if (selectedMapping != null)
                ActivateVisualObjects(selectedMapping.VisualObjects, enable);

            // ��s��e�T�|��
            _currentEnumValue = enumValue;

            return selectedMapping;
        }

        /// <summary>
        /// �T�ΩҦ����U������
        /// </summary>
        public void ActivateAll(bool enable)
        {
            for (int i = 0; i < _visualMappings.Count; i++)
                ActivateVisualObjects(_visualMappings[i].VisualObjects, enable);
        }

        /// <summary>
        /// �ҥΩθT�ι���������
        /// </summary>
        protected void ActivateVisualObjects(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj != null && obj.activeSelf != active)
                    obj.SetActive(active);
            }

        }
    }
}
