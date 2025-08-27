using UnityEditor;
using UnityEngine;

public class BoxColliderToPlanes : MonoBehaviour
{
    [MenuItem("Tools/Generate Box Colliders for Box Collider Faces")]
    public static void GenerateBoxColliders()
    {
        // �����e�襤������
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("�п�ܤ@�ӱa�� Box Collider ������I");
            return;
        }

        // �ˬd�O�_�� Box Collider
        BoxCollider boxCollider = selectedObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("�襤������S�� Box Collider�I");
            return;
        }

        // �ˬd�襤���󪺥����Y��
        if (selectedObject.transform.lossyScale != Vector3.one)
        {
            Debug.LogError("�襤�����󪺥����Y�񤣬O (1, 1, 1)�C�нվ��Y�� 1 �A�~�����C");
            return;
        }

        // �Ыؤ@�Ӥ�����Ӧs��ͦ��� Box Collider
        GameObject collidersParent = new GameObject("BoxColliderFaces");
        collidersParent.transform.SetParent(selectedObject.transform);
        collidersParent.transform.localPosition = Vector3.zero;
        collidersParent.transform.localRotation = Quaternion.identity;

        // �w�q���ӭ�����V�M��m
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

        // �M�����ӭ��A�Ы� Box Collider
        for (int i = 0; i < normals.Length; i++)
        {
            // �Ыؤ@�ӪŪ��l����
            GameObject face = new GameObject(faceNames[i] + " Collider");
            face.transform.SetParent(collidersParent.transform);

            // �p���m
            Vector3 facePosition = center + Vector3.Scale(normals[i], size / 2);
            face.transform.localPosition = facePosition;
            face.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // �K�[ Box Collider �ýվ�j�p
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

        Debug.Log("�����I�w�� Box Collider �����ӭ��ͦ���ª� Box Collider�C");
    }
}
