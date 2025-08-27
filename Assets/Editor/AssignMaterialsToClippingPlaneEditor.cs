using UnityEditor;
using UnityEngine;
using System.IO;
using Microsoft.MixedReality.GraphicsTools;

[CustomEditor(typeof(ClippingPlane))]
public class AssignMaterialsToClippingPlaneEditor : Editor
{
    private string folderPath; // 存放資料夾的路徑

    public override void OnInspectorGUI()
    {
        ClippingPlane clippingBox = (ClippingPlane)target;

        // 顯示資料夾選擇按鈕
        if (GUILayout.Button("選擇資料夾"))
        {
            folderPath = EditorUtility.OpenFolderPanel("選擇材質資料夾", "Assets", "");
        }

        // 顯示目前選擇的資料夾路徑
        if (!string.IsNullOrEmpty(folderPath))
        {
            GUILayout.Label($"選擇的資料夾: {folderPath}");
        }

        // 加入材質的按鈕
        if (GUILayout.Button("將資料夾內的材質添加到 ClippingPlane"))
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                AssignMaterialsToClippingBox(clippingBox, folderPath);
            }
            else
            {
                Debug.LogWarning("請先選擇資料夾！");
            }
        }

        // 繼續顯示原有的 Inspector
        DrawDefaultInspector();
    }

    private void AssignMaterialsToClippingBox(ClippingPlane clippingBox, string folderPath)
    {
        // 確保資料夾路徑是相對於專案的路徑
        string relativePath = folderPath.Replace(Application.dataPath, "Assets");

        // 找到資料夾內的所有材質
        string[] materialPaths = Directory.GetFiles(relativePath, "*.mat", SearchOption.AllDirectories);

        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material != null)
            {
                clippingBox.AddMaterial(material);
                Debug.Log($"已添加材質: {material.name}");
            }
        }

        // 保存更改
        EditorUtility.SetDirty(clippingBox);
        Debug.Log("所有材質已成功添加到 ClippingPlane！");
    }
}
