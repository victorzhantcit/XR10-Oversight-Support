using UnityEditor;
using UnityEngine;
using System.IO;
using Microsoft.MixedReality.GraphicsTools;

[CustomEditor(typeof(ClippingPlane))]
public class AssignMaterialsToClippingPlaneEditor : Editor
{
    private string folderPath; // �s���Ƨ������|

    public override void OnInspectorGUI()
    {
        ClippingPlane clippingBox = (ClippingPlane)target;

        // ��ܸ�Ƨ���ܫ��s
        if (GUILayout.Button("��ܸ�Ƨ�"))
        {
            folderPath = EditorUtility.OpenFolderPanel("��ܧ����Ƨ�", "Assets", "");
        }

        // ��ܥثe��ܪ���Ƨ����|
        if (!string.IsNullOrEmpty(folderPath))
        {
            GUILayout.Label($"��ܪ���Ƨ�: {folderPath}");
        }

        // �[�J���誺���s
        if (GUILayout.Button("�N��Ƨ���������K�[�� ClippingPlane"))
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                AssignMaterialsToClippingBox(clippingBox, folderPath);
            }
            else
            {
                Debug.LogWarning("�Х���ܸ�Ƨ��I");
            }
        }

        // �~����ܭ즳�� Inspector
        DrawDefaultInspector();
    }

    private void AssignMaterialsToClippingBox(ClippingPlane clippingBox, string folderPath)
    {
        // �T�O��Ƨ����|�O�۹��M�ת����|
        string relativePath = folderPath.Replace(Application.dataPath, "Assets");

        // ����Ƨ������Ҧ�����
        string[] materialPaths = Directory.GetFiles(relativePath, "*.mat", SearchOption.AllDirectories);

        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material != null)
            {
                clippingBox.AddMaterial(material);
                Debug.Log($"�w�K�[����: {material.name}");
            }
        }

        // �O�s���
        EditorUtility.SetDirty(clippingBox);
        Debug.Log("�Ҧ�����w���\�K�[�� ClippingPlane�I");
    }
}
