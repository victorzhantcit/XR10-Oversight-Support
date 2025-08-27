#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXMaterialAutoMapper : EditorWindow
{
    private Object materialFolder;
    private Object fbxFolder;

    [MenuItem("Tools/FBX Material Auto Mapper")]
    public static void ShowWindow()
    {
        GetWindow<FBXMaterialAutoMapper>("FBX Material Auto Mapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("自動補全並應用 FBX 材質", EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.Label("拖曳材質資料夾到下方框內:");
        materialFolder = EditorGUILayout.ObjectField(materialFolder, typeof(Object), false);

        GUILayout.Space(10);
        GUILayout.Label("拖曳 FBX 資料夾到下方框內:");
        fbxFolder = EditorGUILayout.ObjectField(fbxFolder, typeof(Object), false);

        GUILayout.Space(20);

        if (GUILayout.Button("開始修復並應用"))
        {
            if (materialFolder == null || fbxFolder == null)
            {
                Debug.LogError("請確保已選擇材質資料夾和 FBX 資料夾！");
                return;
            }

            string materialFolderPath = AssetDatabase.GetAssetPath(materialFolder);
            string fbxFolderPath = AssetDatabase.GetAssetPath(fbxFolder);

            if (!Directory.Exists(materialFolderPath) || !Directory.Exists(fbxFolderPath))
            {
                Debug.LogError("請確保拖曳的是資料夾！");
                return;
            }

            MapAndApplyMaterials(materialFolderPath, fbxFolderPath);
        }
    }

    private void MapAndApplyMaterials(string materialFolderPath, string fbxFolderPath)
    {
        Material[] allMaterials = LoadMaterialsFromFolder(materialFolderPath);
        if (allMaterials == null || allMaterials.Length == 0)
        {
            Debug.LogWarning("材質資料夾中沒有找到任何材質！");
            return;
        }

        string[] fbxPaths = Directory.GetFiles(fbxFolderPath, "*.fbx", SearchOption.AllDirectories);

        foreach (var fbxPath in fbxPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

            if (importer == null)
            {
                Debug.LogWarning($"無法載入 FBX 模型: {fbxPath}");
                continue;
            }

            // 暫時設置為不導入材質，進行重新映射
            importer.materialImportMode = ModelImporterMaterialImportMode.None;

            bool hasChanged = false;
            var remappedMaterials = importer.GetExternalObjectMap();

            foreach (var remap in remappedMaterials)
            {
                var materialName = remap.Key.name;
                var existingMaterial = remap.Value as Material;

                if (existingMaterial == null)
                {
                    Material matchedMaterial = FindMaterialByName(allMaterials, materialName);

                    if (matchedMaterial != null)
                    {
                        importer.AddRemap(remap.Key, matchedMaterial);
                        Debug.Log($"補上材質: {materialName} -> {matchedMaterial.name} ({fbxPath})");
                        hasChanged = true;
                    }
                    else
                    {
                        Debug.LogWarning($"找不到匹配的材質: {materialName}");
                    }
                }
            }

            if (hasChanged)
            {
                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                Debug.Log($"已更新並應用材質到: {fbxPath}");
            }

            // 更新 Material Creation Mode 為 ImportViaMaterialDescription
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
            Debug.Log($"已將 Material Creation Mode 設置為 ImportViaMaterialDescription: {fbxPath}");
        }

        Debug.Log("所有 FBX 遺失材質補全並應用完成！");
    }


    private Material[] LoadMaterialsFromFolder(string folderPath)
    {
        string[] materialPaths = Directory.GetFiles(folderPath, "*.mat", SearchOption.AllDirectories);
        if (materialPaths == null || materialPaths.Length == 0)
        {
            return null;
        }

        Material[] materials = new Material[materialPaths.Length];
        for (int i = 0; i < materialPaths.Length; i++)
        {
            materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPaths[i]);
            if (materials[i] == null)
            {
                Debug.LogWarning($"無法載入材質: {materialPaths[i]}");
            }
        }

        return materials;
    }

    private Material FindMaterialByName(Material[] materials, string name)
    {
        if (materials == null) return null;

        foreach (var mat in materials)
        {
            if (mat != null && mat.name == name)
            {
                return mat;
            }
        }
        return null;
    }
}
#endif