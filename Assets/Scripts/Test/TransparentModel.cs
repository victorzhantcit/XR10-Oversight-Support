using UnityEngine;

public class TransparentModel : MonoBehaviour
{
    private MaterialPropertyBlock _propertyBlock;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void SetTransparency(float alpha)
    {
        if (_renderer != null)
        {
            // 獲取當前的 MaterialPropertyBlock
            _renderer.GetPropertyBlock(_propertyBlock);

            // 調整顏色的透明度
            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                if (_renderer.materials[i].HasProperty("_Color"))
                {
                    Color color = _renderer.materials[i].GetColor("_Color");
                    color.a = alpha;
                    _propertyBlock.SetColor("_Color", color);

                    // 為當前材質應用修改
                    _renderer.SetPropertyBlock(_propertyBlock, i);
                }
            }
        }
    }

    public void RestoreTransparency()
    {
        if (_renderer != null)
        {
            // 移除覆蓋的 MaterialPropertyBlock
            _renderer.SetPropertyBlock(null);
        }
    }
}
