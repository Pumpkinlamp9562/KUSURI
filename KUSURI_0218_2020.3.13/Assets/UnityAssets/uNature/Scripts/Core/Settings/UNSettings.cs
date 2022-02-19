using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using uNature.Wrappers.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uNature.Core.Settings
{
    /// <summary>
    /// A class which handles certain settings of aspects in UN
    /// </summary>
    public class UNSettings : ScriptableObject
    {
        /// <summary>
        /// The version number of this package.
        /// </summary>
        public const string ProjectVersion = "2.2";

        /// <summary>
        /// The file name which will be created for this settings file.
        /// </summary>
        public const string fileName = "UNSettings";

        /// <summary>
        /// Project name (UN folder name).
        /// </summary>
        public const string ProjectName = "uNature";
        /// <summary>
        /// The found path to the project directory (based on the name provided on ProjectName).
        /// </summary>
        public static string ProjectPath
        {
            get
            {

                #if UNITY_EDITOR

                var directories = Directory.GetDirectories(@"Assets", ProjectName, SearchOption.AllDirectories);

                for (int i = 0; i < directories.Length; i++)
                {
                    if (directories[i].Contains(ProjectName))
                        return directories[i] + "/";
                }

                #endif

                return "";
            }
        }

        /// <summary>
        /// The instance to the UNSettings file.
        /// </summary>
        static UNSettings _instance;
        public static UNSettings instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<UNSettings>(fileName);

                    if(_instance == null)
                    {
                        _instance = CreateInstance<UNSettings>();

                        #if UNITY_EDITOR
                        AssetDatabase.CreateAsset(_instance, ProjectPath + "Resources/" + fileName + ".asset");
                        AssetDatabase.SaveAssets();
                        #endif
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Get current used skin.
        /// </summary>
        public static UNEditorSkin EditorSkin
        {
            get
            {
                #if UNITY_EDITOR
                if (instance.UN_GUI_SKIN == UNEditorSkin.Automatic)
                    return EditorGUIUtility.isProSkin ? UNEditorSkin.ProSkin : UNEditorSkin.LightSkin;
                else
                    return instance.UN_GUI_SKIN;
                #else
                return UNEditorSkin.Automatic;
                #endif
            }
        }


        [UNSetting(UNSettingCategories.Terrain, "Tree Instances Respawns: ", "Will tree instances respawn after being destroyed ?")]
        public bool UN_TreeInstancesRespawnsEnabled = false;

        [UNSetting(UNSettingCategories.Networking, "Server Authoritative: ", "Will only the server handle the tree instances managment ?")]
        public bool UN_Networking_Auth = true;

        [UNSetting(UNSettingCategories.Terrain, "Tree Instances Respawn: ", "How much time will it take the tree instances to respawn ? (in minutes), please note that this only affects trees that dont have the harvestable component on their prefab.")]
        public float UN_TreeInstancesRespawnsTime = 1;

        [UNSetting(UNSettingCategories.Threading, "Thread Enabled: ", "Will the system use multi-threading to remove overall from main thread?")]
        public bool UN_Threading_Enabled = true;

        [UNSetting(UNSettingCategories.Threading, "Thread Workers: ", "The amount of workers that the system will use for the multi threading processes.")]
        public Threading.uNature_Thread_Workers UN_Threading_WorkersCount = Threading.uNature_Thread_Workers.One_Worker;

        [UNSetting(UNSettingCategories.General, "Debug Mode Enabled: ", "Is the debug mode enabled ? (Editor Only)")]
        public bool UN_Debugging_Enabled = false;

        [UNSetting(UNSettingCategories.General, "Console Debug Mode Enabled: ", "Is the console debug mode enabled ? (Editor Only)")]
        public bool UN_Console_Debugging_Enabled = false;

        [UNSetting(UNSettingCategories.Grass, "Disable Culling On Grass, Mainly used for recording 360 videos.")]
        public bool UN_Foliage_Disable_Culling = false;

        [UNSetting(UNSettingCategories.General, "Editor Skin", "Leave as Automatic in order to fetch skin color from the unity editor skin.")]
        public UNEditorSkin UN_GUI_SKIN = UNEditorSkin.Automatic;

        /// <summary>
        /// Reset the settings to the default state.
        /// </summary>
        public void ResetDefaults()
        {
            if (instance == null) return;

            _instance = null;

#if UNITY_EDITOR

            AssetDatabase.DeleteAsset(ProjectPath + "Resources/" + fileName + ".asset");
            AssetDatabase.SaveAssets();

#endif
        }

        /// <summary>
        /// Log a message on the uNature debug mode.
        /// </summary>
        /// <param name="context"></param>
        public static void Log(string context)
        {
            if (UNSettings.instance.UN_Console_Debugging_Enabled)
            {
                Debug.Log("<b>uNature Debug Mode : " + context + "</b>");
            }
        }
    }

    /// <summary>
    /// The categories of the settings which will be used on the editor.
    /// </summary>
    public enum UNSettingCategories
    {
        Terrain,
        General,
        Networking,
        Interaction,
        Threading,
        Grass,
    }

    /// <summary>
    /// Editor skin used in the GUI.
    /// [Leave as automatic in order to get skin type from unity's skin]
    /// </summary>
    public enum UNEditorSkin
    {
        Automatic,
        LightSkin,
        ProSkin
    }

    /// <summary>
    /// The class of a category which handles keeping hold of all of the categories and makes all of the reflection needed.
    /// </summary>
    public class UNSettingCategory
    {
        static List<UNSettingCategory> _categories;
        public static List<UNSettingCategory> categories
        {
            get
            {
                if(_categories == null)
                {
                    _categories = new List<UNSettingCategory>();

                    List<FieldInfo> fitFields = new List<FieldInfo>();

                    FieldInfo[] fields = typeof(UNSettings).GetFields();

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i].GetCustomAttributes(true).Select(x => x as UNSettingAttribute).Count() > 0)
                        {
                            fitFields.Add(fields[i]);
                        }
                    }

                    FieldInfo field;
                    UNSettingAttribute attribute;
                    UNSettingCategory category;
                    for(int i = 0; i < fitFields.Count; i++)
                    {
                        field = fitFields[i];

                        attribute = (UNSettingAttribute)field.GetCustomAttributes(true).FirstOrDefault(x => (x as UNSettingAttribute) != null);
                        category = GetCategory(attribute.category);

                        if(category == null)
                        {
                            category = new UNSettingCategory(attribute.category);
                            categories.Add(category);
                        }

                        category.attributes.Add(attribute);
                        category.fields.Add(field);
                    }
                }

                return _categories;
            }
        }

        public bool show;

        public UNSettingCategories type;
        public List<UNSettingAttribute> attributes = new List<UNSettingAttribute>();
        public List<FieldInfo> fields = new List<FieldInfo>();

        public UNSettingCategory(UNSettingCategories category)
        {
            this.type = category;
        }

        public static UNSettingCategory GetCategory(UNSettingCategories category)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].type == category)
                    return categories[i];
            }

            return null;
        }
    }

    /// <summary>
    /// The attribute of each setting which handles the drawing of the setting (generically).
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple=false)]
    public class UNSettingAttribute : System.Attribute
    {
        public UNSettingCategories category;
        public string name;
        public string desc;

        GUIContent _content;
        GUIContent content
        {
            get
            {
                if(_content == null)
                {
                    _content = desc == "" ? new GUIContent(name) : new GUIContent(name, desc);
                }

                return _content;
            }
        }

        public UNSettingAttribute(UNSettingCategories category, string name)
        {
            this.category = category;
            this.name     = name;
        }

        public UNSettingAttribute(UNSettingCategories category, string name, string desc)
        {
            this.category = category;
            this.name = name;
            this.desc = desc;
        }

        public object Draw(object instance)
        {
#if UNITY_EDITOR
            System.Type type = instance.GetType();

            if (CheckType(type, typeof(string)))
                EditorGUILayout.LabelField(content, (string)instance);
            else if (CheckType(type, typeof(int)))
                return EditorGUILayout.IntField(content, (int)instance);
            else if (CheckType(type, typeof(float)))
                return EditorGUILayout.FloatField(content, (float)instance);
            else if (CheckType(type, typeof(bool)))
                return EditorGUILayout.Toggle(content, (bool)instance);
            else if (CheckType(type, typeof(System.Enum)))
                return (object)EditorGUILayout.EnumPopup(content, (System.Enum)instance);
            else if (CheckType(type, typeof(UNSetting)))
                ((UNSetting)instance).DrawGUI();
#endif

            return null;
        }

        bool CheckType(System.Type type, System.Type target)
        {
                return type == target || type.BaseType == target;
        }

    }

}
