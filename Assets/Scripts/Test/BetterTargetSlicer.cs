using UnityEngine;

public class BetterTargetSlicer : MonoBehaviour
{
    public Shader slicerShader; // �۩w�q���� Shader
    public GameObject targetObject; // �ݭn�������ؼЪ���
    public float sliceHeight = 0.0f; // �������
    public Color sliceColor = Color.green; // ������C��

    void Start()
    {
        // �ʺA�����ؼЪ��󪺧���
        ApplySlicerShader(targetObject);
    }

    void Update()
    {
        // ��s���������Ѽ�
        Shader.SetGlobalFloat("_GlobalSliceValue", sliceHeight);
        Shader.SetGlobalColor("_SliceColor", sliceColor);

        // ���������� (�W�U�䱱��)
        if (Input.GetKey(KeyCode.UpArrow))
            sliceHeight += Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow))
            sliceHeight -= Time.deltaTime;

        // ��s������ܪ��A
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
            // ����� Renderer ���Ҧ�����
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;

                // �Ыذ����� Shader ���s����
                Material slicerMaterial = new Material(slicerShader);

                // �~�ӭ���誺�ݩ�
                slicerMaterial.CopyPropertiesFromMaterial(materials[i]);

                // �p�G����観�C���ݩʡA�~���C��
                if (materials[i].HasProperty("_Color"))
                {
                    Color originalColor = materials[i].GetColor("_Color");
                    slicerMaterial.SetColor("_Color", originalColor);
                }

                // �]�m�孱�C��
                slicerMaterial.SetColor("_SliceColor", sliceColor);

                // ��������
                materials[i] = slicerMaterial;
            }

            // ��s Renderer ������
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
            // �p�⪫�󪺥@�����
            Bounds bounds = renderer.bounds;

            // �P�_����O�_�b������ץH�W
            bool isVisible = bounds.max.y >= sliceHeight;

            // �ҥΩθT�Ϊ���
            renderer.gameObject.SetActive(isVisible);
        }
    }
}
