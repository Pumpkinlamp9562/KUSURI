using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using uNature.Core.Utility;
using uNature.Core.Threading;

namespace uNature.Core.FoliageClasses
{
    public class UNFoliageEditor : EditorWindow
    {
        GUIStyle invisibleButtonStyle;

        enum SelectableShaderTypes
        {
            Basic_Diffuse = 1,
            Advanced_AO_NORMALMAP = 2
        }

        #region PaintVariables
        [System.NonSerialized]
        CurrentPaintMethod paintMethod;

        [System.NonSerialized]
        int paintBrushSize = 15;
        [System.NonSerialized]
        byte paintDensity = 10;
        [System.NonSerialized]
        byte eraseDensity = 0;

        [System.NonSerialized]
        List<PaintBrush> chosenBrushes = new List<PaintBrush>();

        Vector2 brushesScrollPos;
        Vector2 splatsScrollPos;

        [System.NonSerialized]
        private bool createNewManagers = true;
        #endregion

        #region PrototypesVariables
        [System.NonSerialized]
        List<FoliagePrototype> chosenPrototypes = new List<FoliagePrototype>();

        SelectableShaderTypes currentPrototypeShader;

        Vector2 prototypesScrollPos;
        Vector2 prototypesEditDataPos;
        Vector2 globalSettingsPos;

        Vector3 lastBrushPosition;

        [System.NonSerialized]
        FoliagePrototype _currentPrototype;
        public FoliagePrototype currentPrototype
        {
            get
            {
                if(_currentPrototype == null)
                {
                    _currentPrototype = chosenPrototypes.Count > 0 ? chosenPrototypes[0] : null;
                }

                return _currentPrototype;
            }
            set
            {
                if(_currentPrototype != value)
                {
                    _currentPrototype = value;

                    if (value != null)
                    {
                        value.UpdateManagerInformation();

                        _prototypeFadeDistance = value.fadeDistance;
                    }
                }
            }
        }

        [System.NonSerialized]
        int _prototypeFadeDistance;
        int prototypeFadeDistance
        {
            get
            {
                if(_prototypeFadeDistance == 0)
                {
                    _prototypeFadeDistance = currentPrototype == null ? 0 : currentPrototype.fadeDistance;
                }

                return _prototypeFadeDistance;
            }
            set
            {
                value = Mathf.Clamp(value, 1, 500);

                _prototypeFadeDistance = value;
            }
        }

        [System.NonSerialized]
        int _globalFadeDistance = 0;
        int globalFadeDistance
        {
            get
            {
                if(_globalFadeDistance == 0)
                {
                    _globalFadeDistance = FoliageCore_MainManager.instance.globalFadeDistance;
                }

                return _globalFadeDistance;
            }
            set
            {
                _globalFadeDistance = value;
            }
        }
        #endregion

        #region Terrain Drawing
        [System.NonSerialized]
        private bool terrain_paint_features = false;

        [System.NonSerialized]
        private bool terrain_paint_averege_density = false;

        [System.NonSerialized]
        List<UN_TerrainTexturePrototype> chosenSplats = new List<UN_TerrainTexturePrototype>();
        #endregion

        Vector2 globalScrollPos;

        bool ctrlPressed = false;
        bool safeKeyPressed = false;

        [System.NonSerialized]
        string[] projectLayers;

        #region Foliage Chunks
        [System.NonSerialized]
        FoliageCore_Chunk selectedChunk = null;
        #endregion

        [MenuItem("Window/uNature/Foliage")]
        public static void OpenWindow()
        {
            GetWindow<UNFoliageEditor>("Foliage Manager");
        }

        void OnEnable()
        {
            projectLayers = null;

            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;

            if(FoliageCore_MainManager.instance != null)
            {
                globalFadeDistance = FoliageCore_MainManager.instance.globalFadeDistance;
            }
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        private void Update()
        {
            this.Repaint();

            CreateMissingLayers();
        }

        public void OnGUI()
        {
            if (FoliageCore_MainManager.instance == null)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Foliage Manager Not Found!!");

                if(GUILayout.Button("Create Foliage Manager"))
                {
                    FoliageCore_MainManager.InitializeAndCreateIfNotFound();
                    globalFadeDistance = FoliageCore_MainManager.instance.globalFadeDistance;
                }
                GUILayout.EndVertical();

                return;
            }

            globalScrollPos = EditorGUILayout.BeginScrollView(globalScrollPos);

            ctrlPressed = Event.current == null ? false : Event.current.control;
            safeKeyPressed = Event.current == null ? false : Event.current.alt;

            if (Event.current != null && Event.current.keyCode == KeyCode.Escape) // try to disable brush on GUI window
            {
                chosenBrushes.Clear();

                EditorUtility.SetDirty(FoliageDB.instance);
                EditorUtility.SetDirty(FoliageCore_MainManager.instance);
            }

            if (invisibleButtonStyle == null)
            {
                invisibleButtonStyle = new GUIStyle("Box");

                invisibleButtonStyle.normal.background = null;
                invisibleButtonStyle.focused.background = null;
                invisibleButtonStyle.hover.background = null;
                invisibleButtonStyle.active.background = null;
            }

            if (FoliageCore_MainManager.instance == null) return;
             
            FoliageCore_MainManager.instance.enabled = UNEditorUtility.DrawHelpBox(string.Format("Foliage Manager: (GUID : {0})", FoliageCore_MainManager.instance.guid), "Here you can manage and paint \nFoliage all over your scene!", true, FoliageCore_MainManager.instance.enabled); // add variable to edit.

            GUI.enabled = FoliageCore_MainManager.instance.enabled;

            if (!DrawUtilityWindow()) return; // if it returns false then it means we need to stop the gui

            GUILayout.Space(5);

            DrawPaintWindow();

            GUILayout.Space(5);

            DrawPrototypesWindow();

            GUILayout.Space(2);

            DrawPrototypesEditUI();

            GUILayout.Space(5);

            DrawGlobalSettingsUI();

            GUILayout.Space(5);

            DrawFoliageInstancesEditingUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(FoliageDB.instance);
                EditorUtility.SetDirty(FoliageCore_MainManager.instance);

                if (!Application.isPlaying)
                {
                    #if UNITY_5_3_OR_NEWER
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                    #else
                    UnityEditor.EditorApplication.MarkSceneDirty();
                    #endif
                }
            }

            EditorGUILayout.EndScrollView();

            GUI.enabled = true;
        }

        bool DrawUtilityWindow()
        {
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));

            UNEditorUtility.DrawGUIHeader("Utility", 0);

            GUILayout.Space(5);

            #region First Line
            GUILayout.BeginHorizontal(); // FIRST line

            if (GUILayout.Button("Destroy"))
            {
                if (EditorUtility.DisplayDialog("Deleting Manager", "Are you sure you want to delete this manager instance? \nThis action can not be undone!", "Yes", "Cancel"))
                {
                    FoliageCore_MainManager.DestroyManager();
                    return false;
                }
            }

            if(GUILayout.Button("Regenerate Heights"))
            {
                FoliageWorldMaps.ReGenerateGlobally();
            }

            if (GUILayout.Button("Reset Grass"))
            {
                if (EditorUtility.DisplayDialog("Grass Reset", "Are you sure that you want to reset all of your placed grass? this CAN NOT be undone.", "Yes", "No"))
                {
                    FoliageCore_MainManager.ResetGrassMap(FoliageDB.unSortedPrototypes);
                }
            }

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(2);

            #region Second Line
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Discard Empty Instances"))
            {
                FoliageCore_MainManager.instance.DiscardEmptyManagerInstances();
            }

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();

            return true;
        }

        void DrawPaintWindow()
        {
            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Paint Tools:", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reload Brushes"))
            {
                FoliageDB.instance.brushes = null;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box");

            string[] paintMethodsNames = Enum.GetNames(typeof(CurrentPaintMethod));
            CurrentPaintMethod currentType;

            for (int i = 0; i < paintMethodsNames.Length; i++)
            {
                currentType = (Enum.GetValues(typeof(CurrentPaintMethod)) as CurrentPaintMethod[])[i];

                if (UNEditorUtility.DrawHighlitableButton(UNStandaloneUtility.GetUIIcon(paintMethodsNames[i]), paintMethod == currentType, GUILayout.Width(40), GUILayout.Height(40)))
                {
                    paintMethod = currentType; // select the paint method.
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(3);

            GUILayout.BeginVertical("Box");

            GUILayout.Label("Paint Settings:", EditorStyles.boldLabel);

            GUILayout.Space(5);

            switch (paintMethod)
            {
                case CurrentPaintMethod.Normal_Paint:
                    DrawNormalBrush();
                    break;

                case CurrentPaintMethod.Spline_Paint:
                    DrawSplineBrush();
                    break;
            }

            GUILayout.Space(5);

            paintBrushSize = EditorGUILayout.IntSlider(new GUIContent("Paint Brush Size:", "The percentage from the brush that will be drawn"), paintBrushSize, 1, 100);
            paintDensity = (byte)EditorGUILayout.IntSlider(new GUIContent("Paint Brush Density:", "The density that will be painted"), paintDensity, 0, 20);
            eraseDensity = (byte)EditorGUILayout.IntSlider(new GUIContent("Erase Brush Density:", "The density which is going to be set on remove"), eraseDensity, 0, 20);
            createNewManagers = EditorGUILayout.Toggle(new GUIContent("Create New Instances If Not Created:", "Will uNature create new instances if not created"), createNewManagers);

            GUILayout.Space(10);

            terrain_paint_features = EditorGUILayout.BeginToggleGroup("Advanced Terrain Painting", terrain_paint_features);

            float selectionBoxHeight = UNBrushUtility.instance.splatPrototypes.Count > 0 ? 80 : 25;
            chosenSplats = UNEditorUtility.DrawPrototypesSelector<UN_TerrainTexturePrototype>(UNBrushUtility.instance.splatPrototypes, chosenSplats, ctrlPressed, ref splatsScrollPos, selectionBoxHeight, false, false, null);
            terrain_paint_averege_density = EditorGUILayout.Toggle("Lerped Density:", terrain_paint_averege_density);

            EditorGUILayout.EndToggleGroup();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    
        void DrawNormalBrush()
        {
            float selectionBoxHeight = FoliageDB.instance.brushes.Count > 0 ? 80 : 25;
            chosenBrushes = UNEditorUtility.DrawPrototypesSelector(FoliageDB.instance.brushes, chosenBrushes, false, ref brushesScrollPos, selectionBoxHeight, false, false, null);
        }

        void DrawSplineBrush()
        {
            GUILayout.Label("Spline Painting Is Not Yet Implemented!", EditorStyles.boldLabel);
        }

        void DrawPrototypesWindow()
        {
            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prototypes Management:", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (chosenPrototypes.Count > 0)
            {
                if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Width(15)))
                {
                    if (EditorUtility.DisplayDialog("uNature", "Are you sure you want to delete this prototype ? \nThis cannot be undone!", "Yes", "No"))
                    {
                        for (int i = 0; i < chosenPrototypes.Count; i++)
                        {
                            FoliageDB.instance.RemovePrototype(chosenPrototypes[i]);
                        }
                        chosenPrototypes.Clear();

                        return;
                    }
                }
                if (GUILayout.Button(new GUIContent("R", "Remove the prototype density from this foliage manager.")))
                {
                    if (EditorUtility.DisplayDialog("uNature", "Are you sure you want to remove this prototype's density ? \nThis cannot be undone!", "Yes", "No"))
                    {
                        for (int i = 0; i < chosenPrototypes.Count; i++)
                        {
                            FoliageCore_MainManager.ResetGrassMap(chosenPrototypes);
                        }
                    }
                }

                if (GUILayout.Button(new GUIContent("L", "Locate the material instance of this prototype")))
                {
                    UnityEngine.Object[] selectionTargets = new UnityEngine.Object[chosenPrototypes.Count];

                    for (int i = 0; i < selectionTargets.Length; i++)
                    {
                        selectionTargets[i] = chosenPrototypes[i].FoliageInstancedMeshData.mat;
                    }

                    Selection.objects = selectionTargets;
                }

            }

            GUILayout.EndHorizontal();

            float selectionBoxHeight = FoliageDB.unSortedPrototypes.Count > 0 ? 80 : 25;
            chosenPrototypes = UNEditorUtility.DrawPrototypesSelector(FoliageDB.unSortedPrototypes, chosenPrototypes, ctrlPressed, ref prototypesScrollPos, selectionBoxHeight, true, chosenPrototypes.Count == 1, OnReplaceCalled);

            var rect = GUILayoutUtility.GetLastRect();
            rect.width = Screen.width;
            rect.height = selectionBoxHeight;

            #region Drag and drop
            var evnt = Event.current;
            if (evnt.type == EventType.DragUpdated)
            {
                if (rect.Contains(evnt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
            }

            if (evnt.type == EventType.DragPerform)
            {
                if (rect.Contains(evnt.mousePosition))
                {
                    GameObject targetFoliagePrefab;
                    Texture2D targetFoliageTexture;
                    UnityEngine.Object targetGeneric;

                    bool exists = false;

                    DragAndDrop.AcceptDrag();
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        targetFoliagePrefab = DragAndDrop.objectReferences[i] as GameObject;
                        targetFoliageTexture = DragAndDrop.objectReferences[i] as Texture2D;

                        targetGeneric = targetFoliagePrefab == null ? (UnityEngine.Object)targetFoliageTexture : targetFoliagePrefab;

                        if (targetGeneric != null)
                        {
                            for (int b = 0; b < FoliageDB.unSortedPrototypes.Count; b++)
                            {
                                if ((targetFoliagePrefab != null && targetFoliagePrefab == FoliageDB.unSortedPrototypes[b].FoliageMesh) || (targetFoliageTexture != null && targetFoliageTexture == FoliageDB.unSortedPrototypes[b].FoliageTexture))
                                {
                                    Debug.LogWarning("Foliage : " + targetGeneric.name + " Already exists! Ignored!");

                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (exists)
                            continue;

                        if (targetFoliagePrefab != null)
                        {
                            FoliageDB.instance.AddPrototype(targetFoliagePrefab);
                        }
                        else if (targetFoliageTexture != null)
                        {
                            FoliageDB.instance.AddPrototype(targetFoliageTexture);
                        }
                    }
                }
            }
            #endregion

            GUILayout.EndVertical();
        }

        void OnReplaceCalled(object obj)
        {
            FoliagePrototype fPrototype = (FoliagePrototype)obj;

            if (fPrototype == null || chosenPrototypes.Count != 1 || chosenPrototypes[0] == fPrototype) return;

            FoliageDB.SwitchPrototypes(chosenPrototypes[0], fPrototype);

            chosenPrototypes.Clear();

            EditorUtility.SetDirty(FoliageDB.instance);

            Repaint();
        }

        void DrawPrototypesEditUI()
        {
            bool canEdit = chosenPrototypes.Count == 1;

            EditorGUILayout.BeginVertical("Box");

            if (chosenPrototypes.Count > 0 && canEdit) // render settings
            {
                currentPrototype = chosenPrototypes[0];

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Prototype: {0}({1})", currentPrototype.name, currentPrototype.id), EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                currentPrototype.enabled = GUILayout.Toggle(currentPrototype.enabled, "");

                GUILayout.EndHorizontal();

                GUI.enabled = currentPrototype.enabled && GUI.enabled;

                GUILayout.Space(10);

                if(currentPrototype.FoliageType == FoliageType.Texture)
                {
                    currentPrototype.FoliageTexture = (Texture2D)EditorGUILayout.ObjectField("Prototype: ", currentPrototype.FoliageTexture, typeof(Texture2D), false);
                }
                else
                {
                    currentPrototype.FoliageMesh = (GameObject)EditorGUILayout.ObjectField("Prototype: ", currentPrototype.FoliageMesh, typeof(GameObject), false);
                }

                GUILayout.Space(10);

                UNEditorUtility.DrawGUIHeader("Per-Blade Settings");

                currentPrototype.spread = EditorGUILayout.Slider(new GUIContent("Spread Noise:", "The randomized space between each Foliage"), currentPrototype.spread, 0, 2);

                Vector2 widthValues = UNEditorUtility.MinMaxSlider(new GUIContent("Width Noise:", ""), currentPrototype.minimumWidth, currentPrototype.maximumWidth, FoliagePrototype.SIZE_MIN_VALUE, FoliagePrototype.SIZE_MAX_VALUE);
                Vector2 heightValues = UNEditorUtility.MinMaxSlider(new GUIContent("Height Noise:", ""), currentPrototype.minimumHeight, currentPrototype.maximumHeight, FoliagePrototype.SIZE_MIN_VALUE, FoliagePrototype.SIZE_MAX_VALUE);

                currentPrototype.minimumWidth = widthValues.x;
                currentPrototype.maximumWidth = widthValues.y;

                currentPrototype.minimumHeight = heightValues.x;
                currentPrototype.maximumHeight = heightValues.y;

                UNEditorUtility.DrawGUIHeader("Shader Properties");

                currentPrototype.renderingLayer = EditorGUILayout.LayerField(new GUIContent("Rendering Layer: ", "The rendering layer"), currentPrototype.renderingLayer);
                currentPrototype.cutOff = EditorGUILayout.Slider("Material Cutoff", currentPrototype.cutOff, 0f, 1f);

                DrawShaderManagement(currentPrototype);

                currentPrototype.useCustomFadeDistance = EditorGUILayout.Toggle("Use Custom Fade Distance", currentPrototype.useCustomFadeDistance);

                bool tempEnabled = GUI.enabled;

                GUI.enabled = tempEnabled && currentPrototype.useCustomFadeDistance;

                GUILayout.BeginHorizontal();

                if (currentPrototype.useCustomFadeDistance)
                {
                    prototypeFadeDistance = EditorGUILayout.IntSlider("Fade Distance: ", prototypeFadeDistance, 10, 500);
                }
                else
                {
                    EditorGUILayout.IntSlider("Fade Distance: ", currentPrototype.GetRealPrototypeFadeDistance(), 10, 500);
                }

                GUI.enabled = GUI.enabled && prototypeFadeDistance != currentPrototype.fadeDistance;
                if (GUILayout.Button("Apply"))
                {
                    currentPrototype.fadeDistance = prototypeFadeDistance;
                }
                GUI.enabled = tempEnabled;
                GUILayout.EndHorizontal();

                UNEditorUtility.DrawGUIHeader("Touch Bending Settings");

                currentPrototype.touchBendingEnabled = GUILayout.Toggle(currentPrototype.touchBendingEnabled, "Touch Bending Enabled");
                currentPrototype.touchBendingStrength = EditorGUILayout.Slider("Touch Bending Strength", currentPrototype.touchBendingStrength, 0.01f, 5f);

                UNEditorUtility.DrawGUIHeader("Custom Wind Settings");

                currentPrototype.useCustomWind = EditorGUILayout.BeginToggleGroup(new GUIContent("Individual Wind", "Use Individual wind for this specific prototype (dont use the global settings)"), currentPrototype.useCustomWind);
                currentPrototype.customWindSettings.windSpeed = EditorGUILayout.Slider("Wind Speed:", currentPrototype.customWindSettings.windSpeed, 0f, 5f);
                currentPrototype.customWindSettings.windBending = EditorGUILayout.Slider("Wind Bending:", currentPrototype.customWindSettings.windBending, 0f, 3f);
                EditorGUILayout.EndToggleGroup();

                UNEditorUtility.DrawGUIHeader("Rendering Settings");

                currentPrototype.receiveShadows = EditorGUILayout.Toggle("Receive Shadows:", currentPrototype.receiveShadows);
                currentPrototype.castShadows = EditorGUILayout.Toggle("Cast Shadows:", currentPrototype.castShadows);
                currentPrototype.useColorMap = EditorGUILayout.Toggle("Use Color Map:", currentPrototype.useColorMap);
                currentPrototype.rotateNormals = EditorGUILayout.Toggle("Rotate Normals: ", currentPrototype.rotateNormals);

                bool overUsingDensity = currentPrototype.maxGeneratedDensity > currentPrototype.FoliageInstancedMeshData.MeshInstancesLimiter_Optimization_Clamp;

                GUI.color = overUsingDensity ? Color.red : Color.white;

                currentPrototype.maxGeneratedDensity = EditorGUILayout.IntSlider("Max Generatable Density:", currentPrototype.maxGeneratedDensity, 1, FoliagePrototype.MAX_GENERATION_DENSITY);

                GUI.color = Color.white;

                if(overUsingDensity)
                {
                    EditorGUILayout.HelpBox("Generated density is above the recommended density value! This might cause performance issues. Recommended density: " + currentPrototype.FoliageInstancedMeshData.MeshInstancesLimiter_Optimization_Clamp, MessageType.Warning);
                }

                #if !UNITY_5_5_OR_NEWER
                GUI.enabled = false;
                #endif

                currentPrototype.useInstancing = EditorGUILayout.Toggle("Use Instancing:", currentPrototype.useInstancing);

                GUI.enabled = tempEnabled;

                GUILayout.Space(10);

                currentPrototype.healthyColor = EditorGUILayout.ColorField("Healthy Color", currentPrototype.healthyColor);
                currentPrototype.dryColor = EditorGUILayout.ColorField("Dry Color", currentPrototype.dryColor);

                GUILayout.Space(10);

                currentPrototype.useLODs = EditorGUILayout.BeginToggleGroup(new GUIContent("Use LODs", "Use Level Of Detail on the Foliage."), currentPrototype.useLODs);
                if (currentPrototype.useLODs)
                {
                    currentPrototype.lods = UNLODUtility.DrawLODs(currentPrototype, currentPrototype.lods);
                }
                EditorGUILayout.EndToggleGroup();

                GUI.enabled = true;
            }
            else if (chosenPrototypes.Count > 1) // if bigger than one, lets disable editing (not supporting multi-editing)
            {
                GUILayout.Label("Multi-editing is not supported!, Please note \nthat you can still draw while multi-selecting prototypes.", EditorStyles.centeredGreyMiniLabel);
            }
            else // if zero, just write that no item is selected.
            {
                currentPrototype = null;

                GUILayout.Label("No prototype is selected, please choose one to continue!", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.EndVertical();
        }

        private void DrawShaderManagement(FoliagePrototype prototype)
        {
            var shaderType = prototype.FoliageInstancedMeshData.shaderType;

            if(shaderType == ShaderType.NaN || shaderType == ShaderType.Custom)
            {
                bool enabled = GUI.enabled;

                GUI.enabled = false;

                EditorGUILayout.EnumPopup("Shader: ", shaderType);

                GUI.enabled = enabled;

                return;
            }

            if(ShaderChanged(prototype))
            {
                currentPrototypeShader = (SelectableShaderTypes)((int)shaderType);
            }

            currentPrototypeShader = (SelectableShaderTypes)EditorGUILayout.EnumPopup("Shader: ", currentPrototypeShader);

            if (ShaderChanged(prototype))
            {
                //prototype.FoliageInstancedMeshData.mat.shader = Shader.Find(currentPrototypeShader == SelectableShaderTypes.Advanced_AO_NORMALMAP ? FoliagePrototype.SHADER_ADVANCED_NAME :
                //    FoliagePrototype.SHADER_BASIC_NAME);

                prototype.FoliageInstancedMeshData.mat.shader = Shader.Find(currentPrototypeShader == SelectableShaderTypes.Advanced_AO_NORMALMAP ? FoliagePrototype.SHADER_ADVANCED_NAME :
                FoliagePrototype.SHADER_BASIC_NAME);

                EditorUtility.SetDirty(prototype.FoliageInstancedMeshData.mat);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (Selection.activeObject == prototype.FoliageInstancedMeshData.mat)
                {
                    Selection.activeObject = null;
                }
            }
        }

        private bool ShaderChanged(FoliagePrototype prototype)
        {
            return (int)prototype.FoliageInstancedMeshData.shaderType != (int)currentPrototypeShader;
        }

        void DrawGlobalSettingsUI()
        {
            EditorGUILayout.BeginVertical("Box");

            #region Shadows Settings
            UNEditorUtility.DrawGUIHeader("Shadow Settings");

            FoliageCore_MainManager.instance.useQualitySettingsShadowDistance = EditorGUILayout.Toggle("Use Quality Settings Shadow Distance:", FoliageCore_MainManager.instance.useQualitySettingsShadowDistance);

            bool tempEnabled = GUI.enabled;

            GUI.enabled = FoliageCore_MainManager.instance.useQualitySettingsShadowDistance == false && tempEnabled;

            FoliageCore_MainManager.instance.foliageShadowDistance = EditorGUILayout.Slider("Foliage Shadow Distance :", FoliageCore_MainManager.instance.foliageShadowDistance, 0, QualitySettings.shadowDistance);

            GUI.enabled = tempEnabled;
            #endregion

            UNEditorUtility.DrawGUIHeader("Prototype Global Settings");

            FoliageCore_MainManager.instance.density = EditorGUILayout.Slider("Foliage Density:", FoliageCore_MainManager.instance.density, 0, 1);
            FoliageCore_MainManager.instance.foliageGlobalTint = EditorGUILayout.ColorField("Global Tint: ", FoliageCore_MainManager.instance.foliageGlobalTint);

            GUILayout.BeginHorizontal();
            globalFadeDistance = EditorGUILayout.IntSlider("Fade Distance: ", globalFadeDistance, 10, 500);

            tempEnabled = GUI.enabled;

            GUI.enabled = tempEnabled && globalFadeDistance != FoliageCore_MainManager.instance.globalFadeDistance;
            if (GUILayout.Button("Apply"))
            {
                FoliageCore_MainManager.instance.globalFadeDistance = globalFadeDistance;
            }
            GUI.enabled = tempEnabled;
            GUILayout.EndHorizontal();

            FoliageCore_MainManager.instance.FoliageGenerationLayerMask = EditorGUILayout.MaskField("Maps Generation Mask:", FoliageCore_MainManager.instance.FoliageGenerationLayerMask, GetLayerNames());
            FoliageCore_MainManager.instance.useColorsMaps = EditorGUILayout.Toggle("Use Color Maps:",
                FoliageCore_MainManager.instance.useColorsMaps);

            UNEditorUtility.DrawGUIHeader("Wind Settings");

            FoliageDB.instance.globalWindSettings.windSpeed = EditorGUILayout.Slider("Wind Speed:", FoliageDB.instance.globalWindSettings.windSpeed, 0f, 5f);
            FoliageDB.instance.globalWindSettings.windBending = EditorGUILayout.Slider("Wind Bending:", FoliageDB.instance.globalWindSettings.windBending, 0f, 3f);

            GUILayout.Space(10);

            GUILayout.EndVertical();
        }

        void DrawFoliageInstancesEditingUI()
        {
            EditorGUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Chunks Settings:", EditorStyles.boldLabel);

            if(selectedChunk != null && selectedChunk.isFoliageInstanceAttached)
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", GUILayout.Width(70), GUILayout.Height(20)))
                {
                    if (EditorUtility.DisplayDialog("Clearing Chunk", "Are you sure you want to clear this chunk? \nThis action can not be undone!", "Yes", "Cancel"))
                    {
                        var mInstance = selectedChunk.GetOrCreateFoliageManagerInstance();

                        FoliageManagerInstance.CleanUp(mInstance);

                        selectedChunk = null;

                        #if UNITY_EDITOR
                        #if UNITY_5_3_OR_NEWER
                        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                        #else
                        UnityEditor.EditorApplication.MarkSceneDirty();
                        #endif
                        #endif

                        return;
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (selectedChunk == null)
            {
                GUILayout.Label("Chunk cannot be found!");
            }
            else
            {
                if (!selectedChunk.isFoliageInstanceAttached)
                {
                    GUILayout.Label("Chunk doesn't have a manager attached!");
                }
                else
                {
                    FoliageManagerInstance mInstance = selectedChunk.GetOrCreateFoliageManagerInstance();

                    mInstance.enabled = EditorGUILayout.Toggle("Manager Instance Enabled: ", mInstance.enabled);
                    mInstance.foliageAreaResolution = (FoliageResolutions)EditorGUILayout.EnumPopup("Foliage Area Resolution", mInstance.foliageAreaResolution);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("World Maps Settings", EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Generate"))
                    {
                        mInstance.worldMaps.UpdateHeightsAndNormals(true);
                        mInstance.worldMaps.Save();
                    }

                    GUILayout.EndHorizontal();

                    mInstance.worldMaps.heightMap.map = (Texture2D)EditorGUILayout.ObjectField(string.Format("Height Map: ({0})", mInstance.foliageAreaResolutionIntegral), mInstance.worldMaps.heightMap.map, typeof(Texture2D), false);
                    mInstance.grassMap.map = (Texture2D)EditorGUILayout.ObjectField(string.Format("Grass Map: ({0})", mInstance.foliageAreaResolutionIntegral), mInstance.grassMap.map, typeof(Texture2D), false);

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color Maps Settings", EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Generate"))
                    {
                        mInstance.colorMap = UNMapGenerators.GenerateColorMap(mInstance.transform.position.x, mInstance.transform.position.z, FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE, mInstance);
                    }

                    GUILayout.EndHorizontal();

                    mInstance.colorMap = (Texture2D)EditorGUILayout.ObjectField(string.Format("Color Map: ({0})", FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE), mInstance.colorMap, typeof(Texture2D), false);

                }
            }

            EditorGUILayout.EndVertical();
        }

        void OnSceneGUI(SceneView sView)
        {
            if (FoliageCore_MainManager.instance == null) return;

            var current = Event.current;

            if (current != null)
            {
                if (Event.current.keyCode == KeyCode.Escape) // try to disable brush on Scene window
                {
                    chosenBrushes.Clear();

                    EditorUtility.SetDirty(FoliageDB.instance);
                    EditorUtility.SetDirty(FoliageCore_MainManager.instance);
                }

                var type = current.type;

                var ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                RaycastHit hit;

                #region Brushes
                if (chosenBrushes.Count > 0 && chosenPrototypes.Count > 0 && !safeKeyPressed)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));    //Disable scene view mouse selection

                    //UNBrushUtility.instance.DrawBrush(chosenBrushes[0].brushTexture, Color.cyan, ray.origin, Quaternion.FromToRotation(Vector3.forward, ray.direction), paintBrushSize);

                    bool hitTarget = Physics.Raycast(ray, out hit, Mathf.Infinity);
                    if (hitTarget)
                    {
                        UNBrushUtility.instance.DrawBrush(chosenBrushes[0].brushTexture, Color.cyan, hit.point, ray.origin.y, paintBrushSize);

                        if ((type == EventType.MouseDrag || type == EventType.MouseDown) && current.button == 0)
                        {
                            if (Vector3.Distance(lastBrushPosition, hit.point) > 1)
                            {
                                lastBrushPosition = hit.point;

                                PaintBrush(current.shift, new Vector2(hit.point.x, hit.point.z), chosenBrushes[0]);

                                Repaint();
                            }

                            if (type == EventType.KeyUp)
                            {
                                HandleUtility.Repaint();    //Enable scene view mouse selection
                            }
                        }
                    }
                }
                #endregion

                #region Chunks Selection
                if (type == EventType.MouseDown && current.button == 0)
                {
                    RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                    for (var i = 0; i < hits.Length; i++)
                    {
                        hit = hits[i];

                        var mChunk = hit.transform.GetComponent<FoliageCore_Chunk>();
                        if (mChunk == null) continue;

                        selectedChunk = mChunk;

                        if(selectedChunk.isFoliageInstanceAttached)
                        {
                            var pos = hits[0].point;

                            Debug.Log("Grass Map Debug: " + selectedChunk.GetOrCreateFoliageManagerInstance()
                                          .grassMap.mapPixels[
                                              (int)(pos.x - selectedChunk.transform.position.x) + (int)(pos.z -
                                                                                                        selectedChunk.transform.position.z) * FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE]);
                        }

                        return;
                    }
                }
            }
            #endregion

            #region DebugMode
            if (FoliageCore_MainManager.instance != null)
            {
                FoliageCore_MainManager.instance.DEBUG_DrawUI();
            }
            #endregion
        }

        private string[] GetLayerNames()
        {
            if (projectLayers == null)
            {
                SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty layers = layersManager.FindProperty("layers");
                projectLayers = new string[layers.arraySize];

                for (int i = 0; i < projectLayers.Length; i++)
                {
                    projectLayers[i] = layers.GetArrayElementAtIndex(i).stringValue;
                }
            }

            return projectLayers;
        }

        static void CreateMissingLayers()
        {
            SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layersManager.FindProperty("layers");

            string layerName = "uNature_Terrain";

            //check if layers exists
            bool terrainLayerExists = CheckIfLayerExists(layers, "uNature_Terrain");

            if (terrainLayerExists)
            {
                return;
            }

            if (!terrainLayerExists)
            {
                bool success = CreateLayer(layersManager, layers, layerName);

                if (!success)
                {
                    return;
                }
            }

            Debug.Log("Layers created succesfully.");
        }

        static bool CreateLayer(SerializedObject layersManager, SerializedProperty layers, string layerName)
        {
            var emptyIndex = GetEmptyLayerIndex(layers);
            if (emptyIndex == -1)
                return false;

            var layer = layers.GetArrayElementAtIndex(emptyIndex);
            if (layer == null) return true;

            layer.stringValue = layerName;
            layersManager.ApplyModifiedProperties();

            return true;
        }

        private static int GetEmptyLayerIndex(SerializedProperty layers)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == "")
                    return i;
            }

            return -1;
        }

        private static bool CheckIfLayerExists(SerializedProperty layers, string layer)
        {
            var found = false;

            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == layer.ToLower())
                    found = true;
            }

            return found;
        }

        void PaintBrush(bool isErase, Vector2 position, PaintBrush brush)
        {
            UNBrushUtility.PaintBrush(isErase, paintBrushSize, paintDensity, eraseDensity, createNewManagers, terrain_paint_features, chosenSplats, terrain_paint_averege_density, chosenPrototypes, position, brush);
        }
    }

    public enum CurrentPaintMethod
    {
        Normal_Paint,
        Spline_Paint,
    }
}
