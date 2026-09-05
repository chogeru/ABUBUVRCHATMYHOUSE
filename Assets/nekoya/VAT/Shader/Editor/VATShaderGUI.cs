using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared material inspector for the nekoya VAT shader family.
/// Changing Rendering Mode switches the shader while preserving properties
/// whose names are shared by the three shaders.
/// </summary>
public sealed class VATShaderGUI : ShaderGUI
{
    private const string OpaqueShaderName = "nekoya/VAT";
    private const string CutoutShaderName = "nekoya/VAT_Cutout";
    private const string TransparentShaderName = "nekoya/VAT_Transparent";

    private enum RenderingMode
    {
        Opaque,
        Cutout,
        Transparent
    }

    private static bool showMotion = true;
    private static bool showLighting = true;
    private static bool showAdvanced;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;
        if (material == null)
        {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("nekoya VAT Shader", EditorStyles.boldLabel);

        if (!TryGetRenderingMode(material.shader, out RenderingMode currentMode))
        {
            EditorGUILayout.HelpBox(
                "This inspector supports nekoya/VAT, nekoya/VAT_Cutout and nekoya/VAT_Transparent.",
                MessageType.Warning);
            materialEditor.PropertiesDefaultGUI(properties);
            return;
        }

        DrawRenderingMode(materialEditor, currentMode);

        MaterialProperty posTexture = FindProperty("_PosTexture", properties);
        MaterialProperty mainTexture = FindProperty("_MainTex", properties);
        MaterialProperty color = FindProperty("_Color", properties);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VAT Data", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(
            new GUIContent("Position Texture", "Position, packed normal and VAT header data."),
            posTexture);

        EditorGUILayout.Space();
        showMotion = EditorGUILayout.BeginFoldoutHeaderGroup(showMotion, "Motion");
        if (showMotion)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_IsFluid", "Use Vertex ID", "Calculate VAT UV from SV_VertexID instead of TEXCOORD1.");
            DrawProperty(materialEditor, properties, "_Motion", "Motion", "Animation frame. Used when Time Motion is disabled.");
            DrawProperty(materialEditor, properties, "_IsLerp", "Interpolate Frames", "Interpolate position and normal between adjacent frames.");
            DrawProperty(materialEditor, properties, "_IsRand", "Random Start", "Offset the animation using the object's world position.");
            DrawProperty(materialEditor, properties, "_TimeMotion", "Time Motion", "Drive Motion using Unity time.");
            DrawProperty(materialEditor, properties, "_FPS", "FPS", "Playback speed when Time Motion is enabled.");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        showLighting = EditorGUILayout.BeginFoldoutHeaderGroup(showLighting, "Surface & Lighting");
        if (showLighting)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "Color (RGB) and alpha (A)."), mainTexture, color);
            materialEditor.TextureScaleOffsetProperty(mainTexture);

            if (currentMode == RenderingMode.Cutout)
            {
                DrawProperty(materialEditor, properties, "_Cutoff", "Alpha Cutoff");
            }

            DrawProperty(materialEditor, properties, "_IsShadow", "Receive Shadow", "Apply received realtime shadow to the base pass.");
            DrawProperty(materialEditor, properties, "_IsRimLight", "Rim Light");

            MaterialProperty rimLight = FindProperty("_IsRimLight", properties);
            using (new EditorGUI.DisabledScope(rimLight.hasMixedValue || rimLight.floatValue < 0.5f))
            {
                EditorGUI.indentLevel++;
                DrawProperty(materialEditor, properties, "_RimLightColor", "Rim Color");
                DrawProperty(materialEditor, properties, "_RimLightPower", "Rim Power");
                EditorGUI.indentLevel--;
            }

            DrawProperty(materialEditor, properties, "_AmbientColor", "Ambient Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        showAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvanced, "Advanced");
        if (showAdvanced)
        {
            EditorGUI.indentLevel++;
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private static void DrawRenderingMode(MaterialEditor materialEditor, RenderingMode currentMode)
    {
        bool hasMixedMode = false;
        foreach (Object target in materialEditor.targets)
        {
            Material targetMaterial = target as Material;
            if (targetMaterial == null ||
                !TryGetRenderingMode(targetMaterial.shader, out RenderingMode targetMode) ||
                targetMode != currentMode)
            {
                hasMixedMode = true;
                break;
            }
        }

        EditorGUI.showMixedValue = hasMixedMode;
        EditorGUI.BeginChangeCheck();
        RenderingMode newMode = (RenderingMode)EditorGUILayout.EnumPopup(
            new GUIContent("Rendering Mode", "Switches the shader, render queue and render type."),
            currentMode);
        EditorGUI.showMixedValue = false;

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Shader newShader = Shader.Find(GetShaderName(newMode));
        if (newShader == null)
        {
            EditorGUILayout.HelpBox("The selected VAT shader could not be found.", MessageType.Error);
            return;
        }

        materialEditor.RegisterPropertyChangeUndo("VAT Rendering Mode");
        foreach (Object target in materialEditor.targets)
        {
            Material targetMaterial = target as Material;
            if (targetMaterial == null)
            {
                continue;
            }

            targetMaterial.shader = newShader;
            targetMaterial.renderQueue = -1;
            EditorUtility.SetDirty(targetMaterial);
        }

        // MaterialProperty instances belong to the previous shader. Rebuild the
        // inspector immediately after switching, even though all current VAT
        // shaders intentionally share their main property names.
        GUIUtility.ExitGUI();
    }

    private static void DrawProperty(
        MaterialEditor materialEditor,
        MaterialProperty[] properties,
        string propertyName,
        string label,
        string tooltip = null)
    {
        MaterialProperty property = FindProperty(propertyName, properties, false);
        if (property != null)
        {
            materialEditor.ShaderProperty(property, new GUIContent(label, tooltip));
        }
    }

    private static bool TryGetRenderingMode(Shader shader, out RenderingMode mode)
    {
        string shaderName = shader != null ? shader.name : string.Empty;
        switch (shaderName)
        {
            case OpaqueShaderName:
                mode = RenderingMode.Opaque;
                return true;
            case CutoutShaderName:
                mode = RenderingMode.Cutout;
                return true;
            case TransparentShaderName:
                mode = RenderingMode.Transparent;
                return true;
            default:
                mode = RenderingMode.Opaque;
                return false;
        }
    }

    private static string GetShaderName(RenderingMode mode)
    {
        switch (mode)
        {
            case RenderingMode.Cutout:
                return CutoutShaderName;
            case RenderingMode.Transparent:
                return TransparentShaderName;
            default:
                return OpaqueShaderName;
        }
    }
}
