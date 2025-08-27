using TMPro;
using Unity.Extensions;
using UnityEngine;

namespace MRTK.Extensions
{
    public class KeyValueListItem : VirtualListItem<KeyValue>
    {
        [SerializeField] private TMP_Text _key;
        [SerializeField] private TMP_Text _value;

        public override void SetContent(KeyValue data, int index = -1, bool interactable = true)
        {
            if (string.IsNullOrEmpty(data.Value))
            {
                _key.color = Color.yellow;
                _key.margin = Vector4.zero;
                _key.text = data.Key;
                _value.text = string.Empty;
            }
            else
            {
                _key.color = Color.white;
                _key.margin = new Vector4(5f, 0f, 0f, 0f);
                _key.text = data.Key;
                _value.text = data.Value;
            }
        }
    }
}
