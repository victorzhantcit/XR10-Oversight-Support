using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CountChildObjectsWithKeyword : Editor
{
    private static KeywordConfig keywordConfig;

    [MenuItem("GameObject/Count Child Objects With Keyword", false, 0)]
    private static void CountChildObjects()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("Please select a GameObject in the Hierarchy.");
            return;
        }

        if (keywordConfig == null)
        {
            keywordConfig = Resources.Load<KeywordConfig>("KeywordConfig");
            if (keywordConfig == null)
            {
                Debug.LogError("KeywordConfig not found. Please create a KeywordConfig ScriptableObject in Resources folder.");
                return;
            }
        }

        string keyword = keywordConfig.keyword;
        List<string> matchedObjectNames = new List<string>();
        int count = CountChildObjectsRecursively(selectedObject.transform, keyword, matchedObjectNames);

        // 將匹配到的物件名稱儲存到 KeywordConfig
        keywordConfig.matchedObjects = matchedObjectNames;
        EditorUtility.SetDirty(keywordConfig);

        // 顯示結果為跳出視窗，提示前往配置檔案路徑
        if (count > 0)
        {
            string assetPath = AssetDatabase.GetAssetPath(keywordConfig);
            EditorUtility.DisplayDialog("Keyword Search Result",
                $"The number of child objects containing the keyword '{keyword}' is: {count}\n\nPlease check the config file at:\n{assetPath}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Keyword Search Result",
                $"The number of child objects containing the keyword '{keyword}' is: {count}", "OK");
        }
    }

    private static int CountChildObjectsRecursively(Transform parent, string keyword, List<string> matchedObjectNames)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.name.Contains(keyword))
            {
                count++;
                matchedObjectNames.Add(child.name);
            }

            count += CountChildObjectsRecursively(child, keyword, matchedObjectNames);
        }

        return count;
    }
}
