using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshRenderer))]
public class RendererLocalBoundsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshRenderer renderer = (MeshRenderer)target;

        if (GUILayout.Button("Show Local Bounds"))
        {
            Transform t = renderer.transform;
            Bounds worldBounds = renderer.bounds;

            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;

            Vector3[] corners = new Vector3[8];
            corners[0] = t.InverseTransformPoint(center + new Vector3(+extents.x, +extents.y, +extents.z));
            corners[1] = t.InverseTransformPoint(center + new Vector3(+extents.x, +extents.y, -extents.z));
            corners[2] = t.InverseTransformPoint(center + new Vector3(+extents.x, -extents.y, +extents.z));
            corners[3] = t.InverseTransformPoint(center + new Vector3(+extents.x, -extents.y, -extents.z));
            corners[4] = t.InverseTransformPoint(center + new Vector3(-extents.x, +extents.y, +extents.z));
            corners[5] = t.InverseTransformPoint(center + new Vector3(-extents.x, +extents.y, -extents.z));
            corners[6] = t.InverseTransformPoint(center + new Vector3(-extents.x, -extents.y, +extents.z));
            corners[7] = t.InverseTransformPoint(center + new Vector3(-extents.x, -extents.y, -extents.z));

            Bounds localBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Length; i++)
            {
                localBounds.Encapsulate(corners[i]);
            }

            Vector3 localScale = renderer.transform.localScale;

            // 防止除以 0（很少見但保險）
            Vector3 safeLossyScale = new Vector3(
                Mathf.Approximately(localScale.x, 0f) ? 1f : localScale.x,
                Mathf.Approximately(localScale.y, 0f) ? 1f : localScale.y,
                Mathf.Approximately(localScale.z, 0f) ? 1f : localScale.z
            );

            // 如果你有 worldSize，可以這樣反推回 localSize
            Vector3 approxLocalSize = localBounds.size;
            approxLocalSize = new Vector3(
                approxLocalSize.x * safeLossyScale.x,
                approxLocalSize.y * safeLossyScale.y,
                approxLocalSize.z * safeLossyScale.z
            );

            Debug.Log($"[Renderer: {renderer.name}] Local Bounds Approximated (based on localScale):\n" +
                      $"Center: {renderer.transform.localPosition}\tSize: {approxLocalSize}");

        }
    }
}
