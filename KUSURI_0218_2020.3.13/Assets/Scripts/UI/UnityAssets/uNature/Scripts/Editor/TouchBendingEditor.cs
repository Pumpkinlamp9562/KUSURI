using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uNature.Core.FoliageClasses
{
    [CustomEditor(typeof(TouchBending))]
    public class TouchBendingEditor : UnityEditor.Editor
    {
        TouchBending touchBending;

        private void OnEnable()
        {
            touchBending = target as TouchBending;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(touchBending, "Touch Bending modifications");

            touchBending.radius = EditorGUILayout.FloatField(new GUIContent("Radius: ", "The radius of the touch bending, the higher it is, the more grass in the area will be bended."), touchBending.radius);
            touchBending.seekingRange = EditorGUILayout.FloatField(new GUIContent("Seeking Range: ", "This is the range from the cameras that the touch bending will be calculated. The lower it is the less range it will have but the ability to use more touch bending instances."), touchBending.seekingRange);
            touchBending.simulateOnEditorTime = EditorGUILayout.Toggle("Simulate Touch Bending On Editor Time: ", touchBending.simulateOnEditorTime);

            if (touchBending.simulate)
            {
                if (touchBending.id == -1)
                {
                    EditorGUILayout.HelpBox("ID isn't assigned, most likely because there are more than the allowed instances of touch bending objects around the player.", MessageType.Error);
                }

                if (!touchBending.inBounds)
                {
                    EditorGUILayout.HelpBox("Touch bending instance out of bounds, no touch bending will be apllied. Get it closer to a UNSeeker/ increase seeking range", MessageType.Warning);
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                Undo.FlushUndoRecordObjects();
            }
        }
    }
}
