using UnityEngine;
using UnityEditor;
using uNature.Core.Utility;

namespace uNature.Core.Seekers
{
    [CustomEditor(typeof(UNSeeker))]
    public class UNSeekerEditor : UnityEditor.Editor
    {
        UNSeeker seeker;

        private void OnEnable()
        {
            seeker = target as UNSeeker;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(seeker, "Seeker modifications");

            #region Trees
            GUILayout.Label("Trees: ", EditorStyles.boldLabel);
            GUILayout.Space(1);

            UNStandaloneUtility.BeginHorizontalOffset(15);

            seeker.treesCheckDistance = EditorGUILayout.FloatField("Trees Update Distance: ", seeker.treesCheckDistance);
            seeker.seekingDistance = EditorGUILayout.FloatField("Trees Seeking Distance: ", seeker.seekingDistance);

            GUILayout.Space(5);

            seeker.attackTrees = EditorGUILayout.BeginToggleGroup("Attack Trees", seeker.attackTrees);

            GUILayout.Space(2);

            UNStandaloneUtility.BeginHorizontalOffset(25); //adjust more offset

            seeker.raycastMask = UNEditorUtility.LayerMaskField("Raycast Mask: ", seeker.raycastMask);
            seeker.raycastDistance = EditorGUILayout.FloatField("Raycast Distance: ", seeker.raycastDistance);

            UNStandaloneUtility.EndHorizontalOffset();

            EditorGUILayout.EndToggleGroup();

            UNStandaloneUtility.EndHorizontalOffset();

            GUILayout.Space(10);
            #endregion

            #region Grass
            GUILayout.Label("Grass: ", EditorStyles.boldLabel);
            GUILayout.Space(1);

            UNStandaloneUtility.BeginHorizontalOffset(15);

            seeker.isGrassReceiver = EditorGUILayout.BeginToggleGroup("Receive Grass", seeker.isGrassReceiver);

            GUILayout.Space(2);

            UNStandaloneUtility.BeginHorizontalOffset(25); //adjust more offset

            seeker.grassCheckDistance = EditorGUILayout.FloatField("Grass Update Distance: ", seeker.grassCheckDistance);

            UNStandaloneUtility.EndHorizontalOffset();

            EditorGUILayout.EndToggleGroup();

            UNStandaloneUtility.EndHorizontalOffset();

            GUILayout.Space(10);
            #endregion

            #region General
            GUILayout.Label("General: ", EditorStyles.boldLabel);
            GUILayout.Space(1);

            UNStandaloneUtility.BeginHorizontalOffset(15);

            seeker.playerCamera = EditorGUILayout.ObjectField("Rendering Camera: ", seeker.playerCamera, typeof(Camera), true) as Camera;

            UNStandaloneUtility.EndHorizontalOffset();
            #endregion

            if (GUI.changed)
            {
                EditorUtility.SetDirty(seeker);
                Undo.FlushUndoRecordObjects();
            }
        }
    }
}
