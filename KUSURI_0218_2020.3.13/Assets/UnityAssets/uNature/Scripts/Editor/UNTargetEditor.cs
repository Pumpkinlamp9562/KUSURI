using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uNature.Core.Targets
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UNTarget), false)]
    public class UNTargetEditor : UnityEditor.Editor
    {
        UNTarget script;

        MonoScript m_PoolType;
        MonoScript PoolType
        {
            get { return m_PoolType; }
            set
            {
                if(value != PoolType)
                {
                    m_PoolType = value;
                }
            }
        }

        SerializedProperty m_PoolAmount;

        public override void OnInspectorGUI()
        {
            /*
            if (script == null || m_PoolAmount == null)
            {
                script = (UNTarget)target;

                if (script.PoolItemClass != null)
                {
                    PoolType = MonoScript.FromMonoBehaviour(script.PoolItemClass);
                }

                m_PoolAmount = serializedObject.FindProperty("PoolAmount");
            }

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(15);

            EditorGUILayout.BeginVertical();

            m_PoolShow = EditorGUILayout.Foldout(m_PoolShow, "Pool Settings");

            if (m_PoolShow)
            {
                EditorGUILayout.PropertyField(m_PoolAmount, new GUIContent("Pool Amount :", "How many Pool items will be created to each, USED tree prototype"));

                PoolType = (MonoScript)EditorGUILayout.ObjectField(new GUIContent("Pool Item Type :", "What would be the type of the created Pool item ?"), PoolType, typeof(MonoScript), false);

                if (GUILayout.Button("Generate Pool"))
                {
                    UNTarget currentTarget;

                    for (int i = 0; i < targets.Length; i++)
                    {
                        currentTarget = targets[i] as UNTarget;

                        currentTarget.CreatePool(PoolType == null ? null : PoolType.GetClass());
                    }
                }
            }
            */
        }
    }
}