using UnityEngine;
using UnityEditor;

namespace VolumetricLights
{

    [CustomEditor(typeof(VolumetricLightsRenderFeature))]
    public class RenderFeatureEditor : Editor
    {

        SerializedProperty renderPassEvent;
        SerializedProperty blendMode, brightness;
        SerializedProperty blurPasses, blurDownscaling, blurSpread;

        private void OnEnable()
        {
            renderPassEvent = serializedObject.FindProperty("renderPassEvent");
            blendMode = serializedObject.FindProperty("blendMode");
            brightness = serializedObject.FindProperty("brightness");
            blurPasses = serializedObject.FindProperty("blurPasses");
            blurDownscaling = serializedObject.FindProperty("blurDownscaling");
            blurSpread = serializedObject.FindProperty("blurSpread");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(renderPassEvent);
            EditorGUILayout.PropertyField(blendMode);
            EditorGUILayout.PropertyField(brightness);
            EditorGUILayout.PropertyField(blurPasses);
            if (blurPasses.intValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(blurDownscaling, new GUIContent("Downscaling"));
                EditorGUILayout.PropertyField(blurSpread);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();

        }

    }
}