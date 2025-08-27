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
            // �����e�� MaterialPropertyBlock
            _renderer.GetPropertyBlock(_propertyBlock);

            // �վ��C�⪺�z����
            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                if (_renderer.materials[i].HasProperty("_Color"))
                {
                    Color color = _renderer.materials[i].GetColor("_Color");
                    color.a = alpha;
                    _propertyBlock.SetColor("_Color", color);

                    // ����e�������έק�
                    _renderer.SetPropertyBlock(_propertyBlock, i);
                }
            }
        }
    }

    public void RestoreTransparency()
    {
        if (_renderer != null)
        {
            // �����л\�� MaterialPropertyBlock
            _renderer.SetPropertyBlock(null);
        }
    }
}
