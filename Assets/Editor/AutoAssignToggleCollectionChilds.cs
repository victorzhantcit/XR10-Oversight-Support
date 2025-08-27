using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit; // 記得引用 MRTK 命名空間

[CustomEditor(typeof(ToggleCollection))]
public class ToggleCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ToggleCollection toggleCollection = (ToggleCollection)target;

        if (GUILayout.Button("Overrides Toggles by ChildObject holds StatefulInteractable"))
        {
            Undo.RecordObject(toggleCollection, "Update Toggle List");

            var interactables = toggleCollection.GetComponentsInChildren<StatefulInteractable>(true);
            var toggleList = new List<StatefulInteractable>();

            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    toggleList.Add(interactable);
                }
            }

            toggleCollection.Toggles = toggleList;
            EditorUtility.SetDirty(toggleCollection);

            Debug.Log($"Total apply {toggleList.Count} StatefulInteractables to ToggleCollection。");
        }
    }
}
