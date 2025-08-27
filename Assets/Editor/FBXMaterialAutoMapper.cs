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
        GUILayout.Label("�۰ʸɥ������� FBX ����", EditorStyles.boldLabel);

        GUILayout.Space(10);
        GUILayout.Label("�즲�����Ƨ���U��ؤ�:");
        materialFolder = EditorGUILayout.ObjectField(materialFolder, typeof(Object), false);

        GUILayout.Space(10);
        GUILayout.Label("�즲 FBX ��Ƨ���U��ؤ�:");
        fbxFolder = EditorGUILayout.ObjectField(fbxFolder, typeof(Object), false);

        GUILayout.Space(20);

        if (GUILayout.Button("�}�l�״_������"))
        {
            if (materialFolder == null || fbxFolder == null)
            {
                Debug.LogError("�нT�O�w��ܧ����Ƨ��M FBX ��Ƨ��I");
                return;
            }

            string materialFolderPath = AssetDatabase.GetAssetPath(materialFolder);
            string fbxFolderPath = AssetDatabase.GetAssetPath(fbxFolder);

            if (!Directory.Exists(materialFolderPath) || !Directory.Exists(fbxFolderPath))
            {
                Debug.LogError("�нT�O�즲���O��Ƨ��I");
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
            Debug.LogWarning("�����Ƨ����S�����������I");
            return;
        }

        string[] fbxPaths = Directory.GetFiles(fbxFolderPath, "*.fbx", SearchOption.AllDirectories);

        foreach (var fbxPath in fbxPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

            if (importer == null)
            {
                Debug.LogWarning($"�L�k���J FBX �ҫ�: {fbxPath}");
                continue;
            }

            // �Ȯɳ]�m�����ɤJ����A�i�歫�s�M�g
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
                        Debug.Log($"�ɤW����: {materialName} -> {matchedMaterial.name} ({fbxPath})");
                        hasChanged = true;
                    }
                    else
                    {
                        Debug.LogWarning($"�䤣��ǰt������: {materialName}");
                    }
                }
            }

            if (hasChanged)
            {
                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                Debug.Log($"�w��s�����Χ����: {fbxPath}");
            }

            // ��s Material Creation Mode �� ImportViaMaterialDescription
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
            Debug.Log($"�w�N Material Creation Mode �]�m�� ImportViaMaterialDescription: {fbxPath}");
        }

        Debug.Log("�Ҧ� FBX �򥢧���ɥ������Χ����I");
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
                Debug.LogWarning($"�L�k���J����: {materialPaths[i]}");
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