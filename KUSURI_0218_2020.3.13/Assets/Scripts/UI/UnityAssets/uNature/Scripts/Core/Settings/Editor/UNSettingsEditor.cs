using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

namespace uNature.Core.Settings
{
    public class UNSettingsEditor : EditorWindow
    {
        public UNSettings _settings;
        public UNSettings settings
        {
            get
            {
                if(_settings == null)
                {
                    _settings = UNSettings.instance;
                }

                return _settings;
            }
        }

        GUIStyle invisibleButtonStyle;
        GUIStyle boxStyle;

        Vector2 scrollPos;

        [MenuItem("Window/uNature/Settings", priority=-1)]
        public static void Open()
        {
            var instance = GetWindow<UNSettingsEditor>("UNSettings");
            instance._settings = UNSettings.instance;
        }

        void OnGUI()
        {
            if (invisibleButtonStyle == null)
            {
                invisibleButtonStyle = new GUIStyle("Button");

                invisibleButtonStyle.normal.background = null;
                invisibleButtonStyle.focused.background = null;
                invisibleButtonStyle.hover.background = null;
                invisibleButtonStyle.active.background = null;
            }
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle("Box");

                boxStyle.normal.textColor = invisibleButtonStyle.normal.textColor;
                boxStyle.focused.textColor = invisibleButtonStyle.focused.textColor;
                boxStyle.hover.textColor = invisibleButtonStyle.hover.textColor;
                boxStyle.active.textColor = invisibleButtonStyle.active.textColor;

                boxStyle.fontStyle = FontStyle.Bold;
            }

            GUILayout.BeginVertical("uNature " + UNSettings.ProjectVersion, boxStyle);
            GUILayout.Space(15);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            UNSettingCategory category;
            object drawValue;

            for (int i = 0; i < UNSettingCategory.categories.Count; i++)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                category = UNSettingCategory.categories[i];

                category.show = EditorGUILayout.Foldout(category.show, "Show " + category.type.ToString() + " Settings");

                if (category.show)
                {
                    GUILayout.Space(15);

                    for (int b = 0; b < category.attributes.Count; b++)
                    {
                        drawValue = category.attributes[b].Draw(category.fields[b].GetValue(settings));

                        if (drawValue != null)
                        {
                            category.fields[b].SetValue(settings, drawValue);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Reset To Default"))
            {
                settings.ResetDefaults();
                _settings = null;

                GUILayout.EndVertical();
                return;
            }
         
            if(GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndVertical();
        }

    }
}