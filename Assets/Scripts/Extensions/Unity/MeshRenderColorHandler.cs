using UnityEngine;

namespace Unity.Extensions
{
    public class MeshRenderColorHandler : MonoBehaviour
    {
        public void OnComponentHoverEntered(MeshRenderer meshRenderer)
        {
            Color baseColor = meshRenderer.material.color;
            meshRenderer.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        }

        public void OnComponentHoverExited(MeshRenderer meshRenderer)
        {
            Color baseColor = meshRenderer.material.color;
            meshRenderer.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f);
        }
    }
}
