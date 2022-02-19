using UnityEditor;

namespace VolumetricLights {

    [CustomEditor(typeof(VolumetricLightAnimation))]
    public partial class VolumetricLightAnimationEditor : Editor {

        public override void OnInspectorGUI() {

            EditorGUILayout.HelpBox("This component is obsolete and will be removed in future versions of Volumetric Light. Instead, animate Volumetric Light component properties directly.", MessageType.Warning);
        }
    }

}