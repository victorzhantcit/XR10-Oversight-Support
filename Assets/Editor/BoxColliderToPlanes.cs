using UnityEditor;
using UnityEngine;

public class BoxColliderToPlanes : MonoBehaviour
{
    [MenuItem("Tools/Generate Box Colliders for Box Collider Faces")]
    public static void GenerateBoxColliders()
    {
        // 獲取當前選中的物件
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("請選擇一個帶有 Box Collider 的物件！");
            return;
        }

        // 檢查是否有 Box Collider
        BoxCollider boxCollider = selectedObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("選中的物件沒有 Box Collider！");
            return;
        }

        // 檢查選中物件的全域縮放
        if (selectedObject.transform.lossyScale != Vector3.one)
        {
            Debug.LogError("選中的物件的全域縮放不是 (1, 1, 1)。請調整縮放為 1 再繼續執行。");
            return;
        }

        // 創建一個父物件來存放生成的 Box Collider
        GameObject collidersParent = new GameObject("BoxColliderFaces");
        collidersParent.transform.SetParent(selectedObject.transform);
        collidersParent.transform.localPosition = Vector3.zero;
        collidersParent.transform.localRotation = Quaternion.identity;

        // 定義六個面的方向和位置
        Vector3[] normals = {
            Vector3.up,    // Top
            Vector3.down,  // Bottom
            Vector3.forward,  // Front
            Vector3.back,   // Back
            Vector3.left,  // Left
            Vector3.right  // Right
        };

        string[] faceNames = { "Top", "Bottom", "Front", "Back", "Left", "Right" };

        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;

        // 遍歷六個面，創建 Box Collider
        for (int i = 0; i < normals.Length; i++)
        {
            // 創建一個空的子物件
            GameObject face = new GameObject(faceNames[i] + " Collider");
            face.transform.SetParent(collidersParent.transform);

            // 計算位置
            Vector3 facePosition = center + Vector3.Scale(normals[i], size / 2);
            face.transform.localPosition = facePosition;
            face.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // 添加 Box Collider 並調整大小
            BoxCollider faceCollider = face.AddComponent<BoxCollider>();
            if (normals[i] == Vector3.up || normals[i] == Vector3.down)
            {
                faceCollider.size = new Vector3(size.x, 0.01f, size.z);
            }
            else if (normals[i] == Vector3.forward || normals[i] == Vector3.back)
            {
                faceCollider.size = new Vector3(size.x, size.y, 0.01f);
            }
            else if (normals[i] == Vector3.left || normals[i] == Vector3.right)
            {
                faceCollider.size = new Vector3(0.01f, size.y, size.z);
            }
        }

        Debug.Log("完成！已為 Box Collider 的六個面生成單純的 Box Collider。");
    }
}
