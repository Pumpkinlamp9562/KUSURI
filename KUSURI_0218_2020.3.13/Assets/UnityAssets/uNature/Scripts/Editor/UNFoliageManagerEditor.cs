using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uNature.Core.FoliageClasses
{
    [CustomEditor(typeof(FoliageCore_MainManager))]
    public class UNFoliageManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Editing availabe from the Foliage manager window!"); 
        }
    }
}
