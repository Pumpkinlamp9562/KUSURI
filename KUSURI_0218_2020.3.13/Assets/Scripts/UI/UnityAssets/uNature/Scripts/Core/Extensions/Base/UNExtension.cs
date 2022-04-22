using UnityEngine;

using System.IO;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

using System.Reflection;

namespace uNature.Core.Extensions
{

    /// <summary>
    /// A uConstruct extension that will allow other 3d party systems to work with uConstruct.
    /// </summary>
    public class UNExtension
    {
        /// <summary>
        /// The asset name (for example TreesManagerSystem).
        /// </summary>
        public virtual string AssetName
        {
            get { return ""; }
        }

        /// <summary>
        /// The asset description ( for example : 
        /// 
        /// An asset used for optimizing terrain & game world.
        /// Features :
        /// ....
        /// 
        /// </summary>
        public virtual string AssetDescription
        {
            get { return ""; }
        }

        /// <summary>
        /// Is this asset featured?
        /// </summary>
        public virtual bool Featured
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Asset logo name that will be searched on the project.
        /// 
        /// For example:
        /// uConstructLogo
        /// 
        /// </summary>
        public virtual string AssetLogoName
        {
            get { return ""; }
        }

        /// <summary>
        /// The asset publisher name (for example EEProductions).
        /// </summary>
        public virtual string PublisherName
        {
            get { return ""; }
        }

        /// <summary>
        /// Asset extension documentation name.
        /// </summary>
        public virtual string AssetDocumentationName
        {
            get { return ""; }
        }

        /// <summary>
        /// Asset extension asset store adress - (For exmaple - https://www.assetstore.unity3d.com/en/#!/content/43129).
        /// </summary>
        public virtual string AssetStoreAdress
        {
            get { return ""; }
        }

        /// <summary>
        /// The namespace that will be added to the defines when the extension is activated.
        /// </summary>
        public virtual string AssetNameSpace
        {
            get { return AssetName + "_EXTENSION"; }
        }

        /// <summary>
        /// Default means that this asset doesnt require it to be enabled,
        /// that means that its working with uConstruct out of the box.
        /// </summary>
        public virtual bool IsDefault
        {
            get { return false; }
        }

        /// <summary>
        /// Last checked symbols.
        /// </summary>
        string symbols = "NONE";

        /// <summary>
        /// Last checked logo.
        /// </summary>
        Texture logo;

        /// <summary>
        /// Is the extension activated currently?
        /// </summary>
        public bool isActivated
        {
            get
            {
                #if UNITY_EDITOR
                if (symbols == "NONE")
                    symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);

                return symbols.Contains(AssetNameSpace) || IsDefault;
                #else
                return false;
                #endif
            }
            set
            {
                #if UNITY_EDITOR
                if (value && !isActivated)
                {
                    UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup, symbols + ";" + AssetNameSpace);
                    symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
                }
                else if (!value && isActivated)
                {
                    symbols = symbols.Replace(AssetNameSpace, "");
                    UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }
                #endif
            }
        }

        /// <summary>
        /// Is the extension viewed?
        /// </summary>
        public bool isViewed;

        /// <summary>
        /// Loaded methods that are created to give tools to people who activated the extension.
        /// </summary>
        public List<MethodInfo> HelperMethods;

        /// <summary>
        /// Open the documentation of the extension
        /// <param name="instance">Extension instance</param>
        /// </summary>
        public static void OpenDocs(UNExtension instance)
        {
            #if UNITY_EDITOR
            if (instance.AssetDocumentationName == "") return;

            var files = Directory.GetFiles(@"Assets", instance.AssetDocumentationName, SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(instance.AssetDocumentationName))
                {
                    if (File.Exists(files[i]))
                    {
                        System.Diagnostics.Process.Start(files[i]);
                        return;
                    }
                }
            }
            #endif
        }

        /// <summary>
        /// Open the asset store page of the extension
        /// <param name="instance">Extension instance</param>
        /// </summary>
        public static void OpenAssetStore(UNExtension instance)
        {
            #if UNITY_EDITOR
            if (instance.AssetStoreAdress == "") return;

            Application.OpenURL(instance.AssetStoreAdress);
            #endif
        }

        /// <summary>
        /// Get the extension logo
        /// </summary>
        /// <param name="instance">Extension instance</param>
        public static Texture GetLogo(UNExtension instance)
        {
            if (instance.AssetLogoName == "") return null;

            #if UNITY_EDITOR

            if (instance.logo == null)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets(instance.AssetLogoName);

                string path;

                for (int i = 0; i < guids.Length; i++)
                {
                    path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);

                    if (path.Contains(instance.AssetLogoName))
                    {
                        instance.logo = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
                        break;
                    }
                }
            }

            return instance.logo;

            #else

            return null;

            #endif
        }

        /// <summary>
        /// Load helper methods from an instance.
        /// </summary>
        /// <param name="instance">Extension instance</param>
        public static void LoadMethods(UNExtension instance, Type type)
        {
            #if UNITY_EDITOR
            instance.HelperMethods = new List<MethodInfo>();

            MethodInfo[] methods = type.GetMethods();
            MethodInfo method;

            for (int i = 0; i < methods.Length; i++)
            {
                method = methods[i];

                if (method.GetCustomAttributes(true).Select(x => (MethodHelperAttribute)x).Count() > 0)
                    instance.HelperMethods.Add(method);
            }

            #endif
        }

        #region Buttons-Methods
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MethodHelperAttribute : Attribute
    {
    }
}
