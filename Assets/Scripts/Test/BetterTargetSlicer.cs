using UnityEngine;

public class BetterTargetSlicer : MonoBehaviour
{
    public Shader slicerShader; // 自定義裁切 Shader
    public GameObject targetObject; // 需要裁切的目標物件
    public float sliceHeight = 0.0f; // 剖切高度
    public Color sliceColor = Color.green; // 剖切面顏色

    void Start()
    {
        // 動態替換目標物件的材質
        ApplySlicerShader(targetObject);
    }

    void Update()
    {
        // 更新全局裁切參數
        Shader.SetGlobalFloat("_GlobalSliceValue", sliceHeight);
        Shader.SetGlobalColor("_SliceColor", sliceColor);

        // 控制剖切高度 (上下鍵控制)
        if (Input.GetKey(KeyCode.UpArrow))
            sliceHeight += Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow))
            sliceHeight -= Time.deltaTime;

        // 更新物件顯示狀態
        UpdateObjectVisibility(targetObject);
    }

    private void ApplySlicerShader(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Target object is not assigned!");
            return;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 獲取該 Renderer 的所有材質
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;

                // 創建基於裁切 Shader 的新材質
                Material slicerMaterial = new Material(slicerShader);

                // 繼承原材質的屬性
                slicerMaterial.CopyPropertiesFromMaterial(materials[i]);

                // 如果原材質有顏色屬性，繼承顏色
                if (materials[i].HasProperty("_Color"))
                {
                    Color originalColor = materials[i].GetColor("_Color");
                    slicerMaterial.SetColor("_Color", originalColor);
                }

                // 設置剖面顏色
                slicerMaterial.SetColor("_SliceColor", sliceColor);

                // 替換材質
                materials[i] = slicerMaterial;
            }

            // 更新 Renderer 的材質
            renderer.sharedMaterials = materials;
        }
    }

    private void UpdateObjectVisibility(GameObject obj)
    {
        if (obj == null)
            return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 計算物件的世界邊界
            Bounds bounds = renderer.bounds;

            // 判斷物件是否在剖切高度以上
            bool isVisible = bounds.max.y >= sliceHeight;

            // 啟用或禁用物件
            renderer.gameObject.SetActive(isVisible);
        }
    }
}
