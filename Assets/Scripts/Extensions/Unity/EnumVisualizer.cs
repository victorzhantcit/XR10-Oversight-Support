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
            public T EnumValue; // 枚舉值
            public GameObject[] VisualObjects; // 對應的物件
        }

        [SerializeField]
        private List<EnumVisualMapping> _visualMappings = new List<EnumVisualMapping>();

        public List<EnumVisualMapping> VisualMappings => _visualMappings;

        private T _currentEnumValue;

        /// <summary>
        /// 獲取目前的狀態
        /// </summary>
        public T CurrentEnumValue => _currentEnumValue;

        /// <summary>
        /// 初始化到預設值
        /// </summary>
        [SerializeField]
        private T _defaultEnumValue;

        protected virtual void Start()
        {
            // 初始化到預設值
            SetEnumValue(_defaultEnumValue);
        }

        /// <summary>
        /// 切換到指定的枚舉值
        /// </summary>
        public virtual EnumVisualMapping SetEnumValue(T enumValue, bool enable = true, bool hideOthers = true)
        {
            // 禁用所有物件
            if (hideOthers)
                for (int i = 0; i < _visualMappings.Count; i++)
                    ActivateVisualObjects(_visualMappings[i].VisualObjects, false);

            // 查找與當前枚舉值相關的物件並啟用
            var selectedMapping = _visualMappings.FirstOrDefault(mapping => Equals(mapping.EnumValue, enumValue));
            if (selectedMapping != null)
                ActivateVisualObjects(selectedMapping.VisualObjects, enable);

            // 更新當前枚舉值
            _currentEnumValue = enumValue;

            return selectedMapping;
        }

        /// <summary>
        /// 禁用所有註冊的物件
        /// </summary>
        public void ActivateAll(bool enable)
        {
            for (int i = 0; i < _visualMappings.Count; i++)
                ActivateVisualObjects(_visualMappings[i].VisualObjects, enable);
        }

        /// <summary>
        /// 啟用或禁用對應的物件
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
