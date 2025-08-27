using UnityEditor;
using UnityEngine;

public class ClippingBoxShaderGUI : ShaderGUI
{
    public enum RenderingMode { Opaque = 0, Cutout = 1, Fade = 2, Transparent = 3 }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material material = materialEditor.target as Material;

        MaterialProperty modeProp = FindProperty("_Mode", props);
        RenderingMode mode = (RenderingMode)(int)modeProp.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (RenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", mode);
        if (EditorGUI.EndChangeCheck())
        {
            modeProp.floatValue = (float)mode;
            SetupMaterialWithRenderingMode(material, mode);
        }

        materialEditor.ShaderProperty(FindProperty("_Color", props), "Color");
        materialEditor.TexturePropertySingleLine(new GUIContent("MainTex"), FindProperty("_MainTex", props));
        materialEditor.ShaderProperty(FindProperty("_FadeAlpha", props), "Fade Alpha");
        materialEditor.ShaderProperty(FindProperty("_ClipBoxSide", props), "Clip Box Side");

        EditorGUILayout.Space();
        base.OnGUI(materialEditor, props);
    }

    public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(mat, oldShader, newShader);
        if (mat.HasProperty("_Mode"))
        {
            SetupMaterialWithRenderingMode(mat, (RenderingMode)(int)mat.GetFloat("_Mode"));
        }
    }

    public static void SetupMaterialWithRenderingMode(Material material, RenderingMode mode)
    {
        switch (mode)
        {
            case RenderingMode.Opaque:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                break;

            case RenderingMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;

            case RenderingMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 50;
                break;

            case RenderingMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 50;
                break;
        }
    }
}
