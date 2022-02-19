#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using uNature.Core.Settings;
using uNature.Core.FoliageClasses;

using System.Collections.Generic;

namespace uNature.Core.Utility
{
    public static class UNEditorUtility
    {
        #region ProgressBar
        static int _currentProgressIndex;
        public static int currentProgressIndex
        {
            get
            {
                return _currentProgressIndex;
            }
            set
            {
                _currentProgressIndex = value;

                if(_currentProgressIndex >= targetProgressIndex)
                {
                    SceneView.onSceneGUIDelegate -= OnSceneGUI; // disable the rendering
                    subscribedToSceneGUI = false;
                }
            }
        }
        public static int targetProgressIndex;

        public static string scrollbarText;
        static bool subscribedToSceneGUI;
        #endregion

        #if UNITY_EDITOR
        static GUIStyle _boldedBox;
        static GUIStyle boldedBox
        {
            get
            {
                if(_boldedBox == null)
                {
                    _boldedBox = EditorStyles.helpBox;
                    _boldedBox.fontStyle = FontStyle.Bold;
                }

                return _boldedBox;
            }
        }

        static GUISkin _customSkin;
        public static GUISkin customSkin
        {
            get
            {
                if(_customSkin == null)
                {
                    _customSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(UNSettings.ProjectPath + "Editor Default Resources/uNature_EditorSkin.guiskin");
                }

                return _customSkin;
            }
        }

        static GUIStyle _flatButton;
        public static GUIStyle flatButton
        {
            get
            {
                if(_flatButton == null)
                {
                    _flatButton = new GUIStyle("Box");
                }

                return _flatButton;
            }
        }
        #endif


        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        /// <summary>
        /// Show a scroll bar on scene GUI that shows a progress on a certain task.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="targetProgressIndex"></param>
        public static void StartSceneScrollbar(string text, int targetProgressIndex)
        {
            currentProgressIndex = 0;
            UNEditorUtility.targetProgressIndex = targetProgressIndex;

            scrollbarText = text;

            if (!subscribedToSceneGUI)
            {
                SceneView.onSceneGUIDelegate += OnSceneGUI;
                subscribedToSceneGUI = true;
            }
        }

        /// <summary>
        /// Called when rendering scene GUI.
        /// </summary>
        private static void OnSceneGUI(SceneView sceneview)
        {
            if (currentProgressIndex != targetProgressIndex)
            {
                Handles.BeginGUI();
                EditorGUI.ProgressBar(new Rect(Screen.width - 250, Screen.height - 75, 200, 20), (float)currentProgressIndex / targetProgressIndex, scrollbarText);
                Handles.EndGUI();
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Draw a help box
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public static bool DrawHelpBox(string title, string description, bool drawEnabled, bool enabledVariable)
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical(title, boldedBox);

            GUILayout.Space(15);

            GUILayout.Label(description);

            if (drawEnabled)
            {
                GUILayout.Space(15);

                float tempFieldWidth = EditorGUIUtility.labelWidth;

                EditorGUIUtility.labelWidth = 150;
                enabledVariable = EditorGUILayout.Toggle("Enabled : ", enabledVariable);
                EditorGUIUtility.labelWidth = tempFieldWidth;
            }

            GUILayout.EndVertical();

            GUILayout.Space(5);

            return enabledVariable;
        }
        #endif

        #if UNITY_EDITOR

        #region CustomTypeSelector

        public static T DrawSelectionBox<T>(GUIContent content, List<SelectionBoxItems<T>> values, SelectionBoxItems<T> value)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(content);

            if (GUILayout.Button("Selected Item : " + value.itemName, EditorStyles.objectField))
            {
                value = SelectionBoxWindow.instance.Draw(values, value);
                GUI.changed = true;
            }
            else
            {
                if (SelectionBoxWindow.item != null)
                {
                    value = SelectionBoxWindow.item as SelectionBoxItems<T>;
                    GUI.changed = true;
                }
            }

            GUILayout.EndHorizontal();

            return value == null ? default(T) : value.item;
        }

        #endregion

        #region PrototypesSelector
        public static List<T> DrawPrototypesSelector<T>(List<T> items, List<T> selectedItems, bool controlClicked, ref Vector2 scrollPos, float height, bool replacable, bool replaceAvailable, GenericMenu.MenuFunction2 OnReplace) where T : BasePrototypeItem
        {
            bool oldEnabled = GUI.enabled;

            scrollPos = GUILayout.BeginScrollView(scrollPos, "Box", GUILayout.Height(height));

            Event current = Event.current;
            Vector2 mousePosition = current.mousePosition;

            GUILayout.BeginHorizontal();

            if (items.Count > 0)
            {
                GUISkin defaultSkin = GUI.skin;

                GUI.skin = customSkin;

                for (int i = 0; i < items.Count; i++)
                {
                    if(!items[i].chooseableOnDisabled)
                    {
                        GUI.enabled = items[i].isEnabled && oldEnabled;
                    }

                    if (DrawHighlitableButton(items[i].preview, selectedItems.Contains(items[i]), GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        if (selectedItems.Contains(items[i]))
                        {
                            if (selectedItems.Count > 1 && !controlClicked)
                            {
                                selectedItems.Clear();
                                selectedItems.Add(items[i]);
                            }
                            else
                            {
                                selectedItems.Remove(items[i]);
                            }
                        }
                        else
                        {
                            if (controlClicked)
                            {
                                selectedItems.Add(items[i]);
                            }
                            else
                            {
                                selectedItems.Clear();
                                selectedItems.Add(items[i]);
                            }
                        }
                    }

                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    if (lastRect.Contains(mousePosition) && current.button == 1 && selectedItems.Count == 1)
                    {
                        // Now create the menu, add items and show it
                        GenericMenu menu = new GenericMenu();

                        if (replaceAvailable)
                        {
                            menu.AddItem(new GUIContent("Replace"), false, OnReplace, items[i]);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Replace"));
                        }

                        menu.ShowAsContext();
                    }

                    if (!items[i].chooseableOnDisabled)
                    {
                        GUI.enabled = oldEnabled;
                    }
                    else
                    {
                        if (!items[i].isEnabled)
                        {
                            GUI.color = Color.gray;
                            GUI.Label(new Rect(lastRect.x + 30, lastRect.y + 4, 16, 16), UNStandaloneUtility.GetUIIcon("Disabled"));
                            GUI.color = Color.white;
                        }
                    }
                }

                GUI.skin = defaultSkin;
            }
            else
            {
                GUILayout.Label("No Items Found!", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            return selectedItems;
        }

        public static bool DrawHighlitableButton(Texture2D texture, bool highlighted, params GUILayoutOption[] options)
        {
            bool pressed = false;

            GUISkin defaultSkin = GUI.skin;

            GUI.skin = customSkin;

            Event evnt = Event.current;

            if(GUILayout.Button(texture, highlighted ? "Highlight" : "Box", options) && evnt.button == 0)
            {
                pressed = true;
            }

            GUI.skin = defaultSkin;

            return pressed;
        }
        #endregion

        public static void DrawGUIHeader(string header, float spacing = 5)
        {
            GUILayout.Space(spacing);

            GUILayout.Label(header, EditorStyles.boldLabel);

            GUILayout.Space(spacing);
        }

        public static Vector2 MinMaxSlider(GUIContent content, float minValue, float maxValue, float minLimit, float maxLimit)
        {
            Vector2 value = new Vector2(minValue, maxValue);

            GUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(content, ref value.x, ref value.y, minLimit, maxLimit);

            GUILayout.Space(2);

            value.x = (float)System.Math.Round(EditorGUILayout.FloatField("", value.x, boldedBox, GUILayout.MaxWidth(45)), 2); // minimum value

            GUILayout.Space(2);

            value.y = (float)System.Math.Round(EditorGUILayout.FloatField("", value.y, boldedBox, GUILayout.MaxWidth(45)), 2); // maximum value

            GUILayout.EndHorizontal();

            return value;
        }

        /// <summary>
        /// Draw an GUI preview of a grid.
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="square"></param>
        public static void DrawGridPreview(int resolution, Texture2D square, int width, int height)
        {
            GUILayout.BeginVertical();
            for (int z = 0; z < resolution; z++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < resolution; x++)
                {
                    GUILayout.Label(square, GUILayout.Width(width), GUILayout.Height(height));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        #endif
    }

    #if UNITY_EDITOR
    public class SelectionBoxWindow : EditorWindow
    {
        static SelectionBoxWindow _instance;
        public static SelectionBoxWindow instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = GetWindow<SelectionBoxWindow>();
                    _instance.ShowPopup();

                    _instance.maxSize = new Vector2(300, 300);
                    _instance.minSize = _instance.maxSize;
                }

                return _instance;
            }
        }

        static List<BaseSelectionBoxItem> items;
        public static BaseSelectionBoxItem item = null;

        System.DateTime lastTime = System.DateTime.Now;
        bool clickDone
        {
            get
            {
                return (System.DateTime.Now - lastTime).TotalMilliseconds < 200;
            }
        }

        string searchBox = "";
        Vector2 scrollPos;

        public SelectionBoxItems<T> Draw<T>(List<SelectionBoxItems<T>> values, SelectionBoxItems<T> value)
        {
            items = new List<BaseSelectionBoxItem>();

            for(int i = 0; i < values.Count; i++)
            {
                items.Add(values[i]);
            }

            item = value;

            return item as SelectionBoxItems<T>;
        }

        void OnEnable()
        {
            scrollPos = Vector2.zero;
        }

        void OnDisable()
        {
            if(!Application.isPlaying)
            {
                #if UNITY_EDITOR
                #if UNITY_5_3_OR_NEWER
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                #else
                UnityEditor.EditorApplication.SaveScene();
                #endif
                #endif
            }
        }

        void OnGUI()
        {
            if (items == null)
            {
                Close();
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "Box");
            GUILayout.BeginVertical();

            searchBox = EditorGUILayout.TextField("", searchBox, GUI.skin.FindStyle("ToolbarSeachTextField"));

            GUILayout.Space(5);

            BaseSelectionBoxItem currentItem;
            for(int i = 0; i < items.Count; i++)
            {
                currentItem = items[i];

                if (searchBox == "" || currentItem.itemName == "None" || currentItem.itemName.ToLower().Contains(searchBox.ToLower()))
                {
                    GUI.backgroundColor = item == currentItem ? Color.gray : Color.white;
                    if (GUILayout.Button(currentItem.itemName, EditorStyles.objectFieldThumb))
                    {
                        if (clickDone)
                        {
                            Close();
                        }

                        lastTime = System.DateTime.Now;
                        item = currentItem;
                    }
                    GUI.backgroundColor = Color.white;
                }
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
#endif

                /// <summary>
                /// Selection box items class which is used when openning the custom selection box.
                /// </summary>
        public class SelectionBoxItems<T> : BaseSelectionBoxItem
    {
        public T item;

        public SelectionBoxItems(string _name, T _item)
        {
            base.itemName = _name;
            this.item = _item;
        }
    }

    public class BaseSelectionBoxItem
    {
        public string itemName;
    }
}

#endif