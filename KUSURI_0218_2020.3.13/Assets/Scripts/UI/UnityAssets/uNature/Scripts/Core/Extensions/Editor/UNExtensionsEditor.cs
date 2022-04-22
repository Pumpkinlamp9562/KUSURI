using UnityEngine;
using System.Collections;
using System.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using uNature.Core.Utility;

namespace uNature.Core.Extensions
{
    public class UNExtensionsEditor : EditorWindow
    {
        public const string UN_DEFINE = "UN_INSTALLED";

        static Texture2D _featuredIcon;
        public static Texture2D featuredIcon
        {
            get
            {
                if(_featuredIcon == null)
                {
                    _featuredIcon = UNStandaloneUtility.GetUIIcon("Star");
                }

                return _featuredIcon;
            }
        }

        static GUIStyle _featuredFoldoutStyle;
        public static GUIStyle featuredFoldoutStyle
        {
            get
            {
                if(_featuredFoldoutStyle == null)
                {
                    _featuredFoldoutStyle = new GUIStyle("Foldout");
                    _featuredFoldoutStyle.fontStyle = FontStyle.Bold;
                }

                return _featuredFoldoutStyle;
            }
        }
        [SerializeField]
        Dictionary<string, List<UNExtension>> extensions = new Dictionary<string, List<UNExtension>>();

        GUIStyle invisibleButtonStyle, boxStyle;

        Vector2 scrollPos;

        static bool isOpen
        {
            get { return Resources.FindObjectsOfTypeAll<UNExtensionsEditor>().Length != 0; }
        }

        [MenuItem("Window/uNature/Extensions", priority = -1)]
        public static void Open()
        {
            var instance = GetWindow<UNExtensionsEditor>("uNature Extensions Manager");
            instance.Init();
        }

        [UnityEditor.Callbacks.DidReloadScripts()]
        public static void HandleCompile()
        {
            if (isOpen)
            {
                Open();
            }

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if(!symbols.Contains(UN_DEFINE))
            {
                symbols += ";" + UN_DEFINE;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }
        
        void Init()
        {
            extensions = new Dictionary<string, List<UNExtension>>();
            System.Type[] types = Assembly.GetAssembly(typeof(UNExtension)).GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].BaseType != typeof(UNExtension) || types[i].IsAbstract) continue;

                UNExtension extension = (UNExtension)System.Activator.CreateInstance(types[i]);
                UNExtension.LoadMethods(extension, types[i]);

                if (!extensions.ContainsKey(extension.PublisherName))
                {
                    extensions.Add(extension.PublisherName, new List<UNExtension>());
                }

                extensions[extension.PublisherName].Add(extension);
            }
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
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var extensionGroup in extensions)
            {
                GUILayout.BeginVertical(extensionGroup.Key, boxStyle);

                GUILayout.Space(15);

                foreach (var extension in extensionGroup.Value)
                {
                    GUILayout.BeginHorizontal();
                    extension.isViewed = EditorGUILayout.Foldout(extension.isViewed, extension.AssetName, extension.Featured ? featuredFoldoutStyle : "Foldout");

                    if (extension.Featured)
                    {
                        GUILayout.Label(featuredIcon, GUILayout.Width(20), GUILayout.Height(20));
                    }

                    GUILayout.EndHorizontal();

                    if (extension.isViewed)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(15);

                        GUILayout.BeginVertical();

                        if (!extension.IsDefault)
                            extension.isActivated = EditorGUILayout.Toggle("Activated :", extension.isActivated);

                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();

                        Texture logo = UNExtension.GetLogo(extension);

                        if (logo != null)
                        {
                            GUILayout.Label(logo, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                        }
                        EditorGUILayout.LabelField(extension.AssetDescription, GUILayout.Height(logo == null ? 50 : logo.height)); // draw description

                        GUILayout.EndHorizontal();

                        if (extension.isActivated && extension.HelperMethods.Count > 0)
                        {
                            GUILayout.Space(15);

                            GUILayout.BeginVertical("Tools : ", boxStyle);

                            GUILayout.Space(25);

                            for (int i = 0; i < extension.HelperMethods.Count; i++)
                            {
                                GUILayout.Space(2);

                                if (GUILayout.Button(extension.HelperMethods[i].Name))
                                {
                                    extension.HelperMethods[i].Invoke(extension, null);
                                }
                            }

                            GUILayout.EndVertical();

                        }

                        GUILayout.Space(15);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Open Documentation", boxStyle))
                        {
                            UNExtension.OpenDocs(extension);
                        }
                        if (GUILayout.Button("Asset Store", boxStyle))
                        {
                            UNExtension.OpenAssetStore(extension);
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(5);
                }

                GUILayout.EndVertical();

                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}