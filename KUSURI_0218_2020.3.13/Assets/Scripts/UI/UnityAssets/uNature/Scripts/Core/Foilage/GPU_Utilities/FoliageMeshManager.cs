using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Utility;
using uNature.Core.Targets;
using uNature.Core.FoliageClasses.Interactions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uNature.Core.FoliageClasses
{
    public class FoliageMeshManager : UNTarget
    {
        #region PropertyIDs
        private static int _property_id_worldposition = -1;
        public static int PROPERTY_ID_WORLDPOSITION
        {
            get
            {
                if(_property_id_worldposition == -1)
                {
                    _property_id_worldposition = Shader.PropertyToID("_WorldPosition");
                }

                return _property_id_worldposition;
            }
        }

        private static int _property_id_grassmap = -1;
        public static int PROPERTY_ID_GRASSMAP
        {
            get
            {
                if (_property_id_grassmap == -1)
                {
                    _property_id_grassmap = Shader.PropertyToID("_GrassMap");
                }

                return _property_id_grassmap;
            }
        }

        private static int _property_id_prototype = -1;
        public static int PROPERTY_ID_PROTOTYPE
        {
            get
            {
                if (_property_id_prototype == -1)
                {
                    _property_id_prototype = Shader.PropertyToID("_PrototypeID");
                }

                return _property_id_prototype;
            }
        }

        private static int _property_id_foliage_interaction_touch_bending_objects = -1;
        public static int PROPERTY_ID_FOLIAGE_INTERACTION_TOUCH_BENDING_OBJECTS
        {
            get
            {
                if(_property_id_foliage_interaction_touch_bending_objects == -1)
                {
                    _property_id_foliage_interaction_touch_bending_objects = Shader.PropertyToID("_InteractionTouchBendedInstances");
                }

                return _property_id_foliage_interaction_touch_bending_objects;
            }
        }
        #endregion

        #if UNITY_EDITOR
        [System.NonSerialized]
        RenderingQueue_InteractionReceiver _editorQueueInstance = null;
        internal RenderingQueue_InteractionReceiver editorQueueInstance
        {
            get
            {
                if(_editorQueueInstance == null)
                {
                    _editorQueueInstance = new RenderingQueue_InteractionReceiver();

                    SceneView sView = SceneView.lastActiveSceneView;

                    if (sView == null) return null;

                    _editorQueueInstance.transform = sView.camera.transform;
                    _editorQueueInstance.camera = sView.camera;
                }

                // look for destroyed scene view.

                try
                {
                    if (_editorQueueInstance.transform.position.x == 0) { }
                }
                catch
                {
                    _editorQueueInstance = null;
                    return editorQueueInstance;
                }

                return _editorQueueInstance;
            }
            set
            {
                _editorQueueInstance = value;
            }
        }

        protected bool RENDERING_editorDrawCalled;
        protected Vector3 RENDERING_lastEditorCameraPosition = Vector3.zero;
        #endif

        #region Rendering Variables
        static Dictionary<FoliageResolutions, Dictionary<int, GPUMesh>> _prototypeMeshInstances = null;
        public static Dictionary<FoliageResolutions, Dictionary<int, GPUMesh>> prototypeMeshInstances
        {
            get
            {
                if (_prototypeMeshInstances == null)
                {
                    _prototypeMeshInstances = new Dictionary<FoliageResolutions, Dictionary<int, GPUMesh>>();
                }

                return _prototypeMeshInstances;
            }
        }

        [System.NonSerialized]
        MaterialPropertyBlock _propertyBlock = null;
        public MaterialPropertyBlock propertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }

                return _propertyBlock;
            }
        }
        #endregion

        #region Debug Variables
        public bool DEBUG_Window_Open = false;
        public bool DEBUG_Window_Minimized = false;
        
        [System.NonSerialized]
        private FoliagePrototype DEBUG_CHOSEN_PROTOTYPE = null;

        protected int _lastRenderedVertices;
        public int lastRenderedVertices
        {
            get
            {
                return _lastRenderedVertices;
            }
        }

        protected int _lastRenderedDrawCalls;
        public int lastRenderedDrawCalls
        {
            get
            {
                return _lastRenderedDrawCalls * 2;
            }
        }

        protected int _lastRenderedPrototypes;
        public int lastRenderedPrototypes
        {
            get
            {
                return _lastRenderedPrototypes; // include shadow pass
            }
        }

        Rect debugWindowRect = new Rect(Screen.width / 25, Screen.height / 15, 500, 400);

        FoliageCore_Chunk latestManagerChunk = null;

        Texture2D _closeIcon;
        Texture2D closeIcon
        {
            get
            {
                if (_closeIcon == null)
                {
                    _closeIcon = UNStandaloneUtility.GetUIIcon("Close");
                }

                return _closeIcon;
            }
        }

        Texture2D _showIcon;
        Texture2D showIcon
        {
            get
            {
                if (_showIcon == null)
                {
                    _showIcon = UNStandaloneUtility.GetUIIcon("Show");
                }

                return _showIcon;
            }
        }

        Texture2D _hideIcon;
        Texture2D hideIcon
        {
            get
            {
                if (_hideIcon == null)
                {
                    _hideIcon = UNStandaloneUtility.GetUIIcon("Hide");
                }

                return _hideIcon;
            }
        }
        #endregion

        #region Constructors
        protected override void OnEnable()
        {
            base.OnEnable();

            #if UNITY_EDITOR
            Camera.onPostRender += OnGlobalPostRender;
            #endif

            #if UNITY_EDITOR
            // this method will force an update call on the editor, so the chunks can be updated on the editor.
            EditorUtility.SetDirty(this);
            #endif
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            DestroyMeshInstances();

            #if UNITY_EDITOR
            Camera.onPostRender -= OnGlobalPostRender;
            #endif
        }
        #endregion

        #region Mesh Instances Methods
        /// <summary>
        /// Generate new mesh instances
        /// </summary>
        /// <param name="areaSize"></param>
        public static void GenerateFoliageMeshInstances()
        {
            foreach (var meshInstances in prototypeMeshInstances)
            {
                if (meshInstances.Value.Count > 0)
                {
                    for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                    {
                        if (meshInstances.Value.ContainsKey(FoliageDB.unSortedPrototypes[i].id))
                        {
                            meshInstances.Value[FoliageDB.unSortedPrototypes[i].id].Destroy();
                        }
                    }

                    meshInstances.Value.Clear();
                }

                if (FoliageCore_MainManager.instance.enabled)
                {
                    for (int prototypeIndex = 0; prototypeIndex < FoliageDB.unSortedPrototypes.Count; prototypeIndex++)
                    {
                        if (FoliageDB.unSortedPrototypes[prototypeIndex].enabled)
                        {
                            GenerateFoliageMeshInstanceForIndex(FoliageDB.unSortedPrototypes[prototypeIndex].id, meshInstances.Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate new mesh instances
        /// </summary>
        /// <param name="areaSize"></param>
        public static void GenerateFoliageMeshInstances(FoliageResolutions resolution)
        {
            if (!prototypeMeshInstances.ContainsKey(resolution))
            {
                prototypeMeshInstances.Add(resolution, new Dictionary<int, GPUMesh>());
            }

            var meshInstances = prototypeMeshInstances[resolution];

            if (meshInstances.Count > 0)
            {
                for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                {
                    if (meshInstances.ContainsKey(FoliageDB.unSortedPrototypes[i].id))
                    {
                        meshInstances[FoliageDB.unSortedPrototypes[i].id].Destroy();
                    }
                }

                meshInstances.Clear();
            }

            if (FoliageCore_MainManager.instance.enabled)
            {
                for (int prototypeIndex = 0; prototypeIndex < FoliageDB.unSortedPrototypes.Count; prototypeIndex++)
                {
                    if (FoliageDB.unSortedPrototypes[prototypeIndex].enabled)
                    {
                        GenerateFoliageMeshInstanceForIndex(FoliageDB.unSortedPrototypes[prototypeIndex].id, resolution);
                    }
                }
            }
        }

        /// <summary>
        /// Generate new mesh instances
        /// </summary>
        /// <param name="areaSize"></param>
        public static void GenerateFoliageMeshInstances(int prototypeID)
        {
            foreach (var meshInstances in prototypeMeshInstances)
            {
                if (FoliageCore_MainManager.instance.enabled)
                {
                    if (FoliageDB.sortedPrototypes[prototypeID].enabled)
                    {
                        GenerateFoliageMeshInstanceForIndex(prototypeID, meshInstances.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Create Foliage mesh instances for a certain index and foliage size.
        /// </summary>
        /// <param name="meshInstances"></param>
        /// <param name="prototypeIndex"></param>
        public static void GenerateFoliageMeshInstanceForIndex(int prototypeIndex, FoliageResolutions resolution)
        {
            Dictionary<int, GPUMesh> meshInstances = prototypeMeshInstances[resolution];

            FoliagePrototype prototype = FoliageDB.sortedPrototypes[prototypeIndex];

            if (!FoliageCore_MainManager.instance.enabled) return;

            bool prototypeMeshExists = prototypeMeshInstances != null && meshInstances.ContainsKey(prototypeIndex);

            if (prototypeMeshExists)
            {
                meshInstances[prototypeIndex].Destroy();
                meshInstances.Remove(prototypeIndex);
            }

            Mesh[,,] meshes = null;
            List<byte> densities = prototype.densitiesLODs;
            FoliageMeshInstancesGroup mGroup = new FoliageMeshInstancesGroup();
                
            meshes = FoliageMeshInstance.CreateFoliageInstances(prototypeIndex, densities, out mGroup, resolution);

            meshInstances.Add(prototypeIndex, new GPUMesh(meshes, mGroup, prototypeIndex, resolution));
        }

        /// <summary>
        /// Destroy the current mesh instances.
        /// </summary>
        protected static void DestroyMeshInstances()
        {
            if (prototypeMeshInstances.Count == 0) return;

            FoliagePrototype prototype;

            foreach (var meshInstances in prototypeMeshInstances.Values)
            {
                for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                {
                    prototype = FoliageDB.unSortedPrototypes[i];

                    if (meshInstances != null && meshInstances.ContainsKey(prototype.id))
                    {
                        meshInstances[prototype.id].Destroy();
                    }
                }

                meshInstances.Clear();
            }
            _prototypeMeshInstances = null;
        }

        /// <summary>
        /// Destroy a mesh instance
        /// </summary>
        /// <param name="prototypeID"></param>
        public static void DestroyMeshInstance(int prototypeID)
        {
            foreach (var meshInstances in prototypeMeshInstances.Values)
            {
                if (meshInstances != null && meshInstances.ContainsKey(prototypeID))
                {
                    meshInstances[prototypeID].Destroy();
                }

                meshInstances.Remove(prototypeID);
            }
        }

        /// <summary>
        /// Restart all of the queue instances.
        /// </summary>
        public static void RegenerateQueueInstances()
        {
            if (FoliageCore_MainManager.instance == null) return;

            if (Application.isPlaying)
            {
                for (var i = 0; i < FoliageReceiver.FReceivers.Count; i++)
                {
                    FoliageReceiver.FReceivers[i].queueInstance = null;
                }
            }

            #if UNITY_EDITOR
            FoliageCore_MainManager.instance.editorQueueInstance = null;
            #endif
        }

        /// <summary>
        /// Mark all of the densities as dirty
        /// </summary>
        public static void MarkDensitiesDirty()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < FoliageReceiver.FReceivers.Count; i++)
                {
                    FoliageReceiver.FReceivers[i].queueInstance.ResetDensity();
                }
            }

            #if UNITY_EDITOR
            var editorQueue = FoliageCore_MainManager.instance.editorQueueInstance;

            if (editorQueue == null) return; // no scene view visible.

            editorQueue.ResetDensity();
            #endif
        }

        /// <summary>
        /// Create a new mesh instace
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateNewMesh()
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;

            mesh.bounds = new Bounds(FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_BOUNDS_MIN, FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_BOUNDS_MAX);

            return mesh;
        }
        #endregion

        #region Rendering Methods

        public void OnGlobalPostRender(Camera camera)
        {
            #if UNITY_EDITOR
            // 
            // Editor camera, force scene updating.
            //
            if (!Application.isPlaying)
            {
                SceneView sView = SceneView.currentDrawingSceneView;
                if (sView != null && sView.camera == camera)
                {
                    if (Vector3.Distance(sView.camera.transform.position, RENDERING_lastEditorCameraPosition) > 5 || !RENDERING_editorDrawCalled)
                    {
                        // this method will force an update call on the editor, so the chunks can be updated on the editor.
                        EditorUtility.SetDirty(this);

                        RENDERING_lastEditorCameraPosition = sView.camera.transform.position;
                        RENDERING_editorDrawCalled = true;

                        return;
                    }
                }
            }
            #endif
        }

        protected override void Update()
        {
            base.Update();
            DrawEditorCameras();
        }

        private void DrawEditorCameras()
        {
            #if UNITY_EDITOR
            var sceneCamera = SceneView.lastActiveSceneView;

            if (sceneCamera == null) return;

            editorQueueInstance.CheckPositionChange();

            RenderingPipielineUtility.RenderQueue(editorQueueInstance, sceneCamera.camera);
            #endif
        }

        private void DEBUG_ResetValues()
        {
            _lastRenderedDrawCalls = 0;
            _lastRenderedVertices = 0;
            _lastRenderedPrototypes = 0;
        }
        #endregion

        #region DEBUG_UI Methods
        private void OnGUI()
        {
            DEBUG_DrawUI();
        }

        public void DEBUG_DrawUI()
        {
            FoliagePrototype prototype;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Settings.UNSettings.instance.UN_Console_Debugging_Enabled)
            {
                #if UNITY_EDITOR
                Handles.BeginGUI();
                #endif

                ClampDebugWindow();

                if (DEBUG_Window_Open)
                {
                    debugWindowRect = GUILayout.Window(3, debugWindowRect, (id) =>
                    {
                        if (GUI.Button(new Rect(580, 0.75f, 15, 15), closeIcon, "Label"))
                        {
                            DEBUG_Window_Open = false;
                        }

                        if (FoliageCore_MainManager.instance.enabled)
                        {
                            if (GUI.Button(new Rect(565, -1f, 15, 15), DEBUG_Window_Minimized ? "+" : "-", "Label"))
                            {
                                DEBUG_Window_Minimized = !DEBUG_Window_Minimized;
                            }

                            if (!DEBUG_Window_Minimized)
                            {
                                GUILayout.BeginHorizontal();

                                GUILayout.BeginVertical();

                                #region Global Settings
                                DebugGlobalSettings();
                                #endregion

                                #region Prototypes
                                GUILayout.Space(10);

                                GUILayout.Label("Prototypes Settings:", UNStandaloneUtility.boldLabel);

                                UNStandaloneUtility.BeginHorizontalOffset(25);

                                GUILayout.BeginHorizontal();
                                for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                                {
                                    GUILayout.Space(5);

                                    prototype = FoliageDB.unSortedPrototypes[i];

                                    GUI.color = DEBUG_CHOSEN_PROTOTYPE == prototype ? Color.white : Color.grey;

                                    if (GUILayout.Button(prototype.name, GUILayout.Width(100)))
                                    {
                                        if (DEBUG_CHOSEN_PROTOTYPE == prototype)
                                        {
                                            DEBUG_CHOSEN_PROTOTYPE = null;
                                        }
                                        else
                                        {
                                            DEBUG_CHOSEN_PROTOTYPE = prototype;
                                        }
                                    }

                                    GUI.color = Color.white;
                                }
                                GUILayout.EndHorizontal();

                                UNStandaloneUtility.EndHorizontalOffset();
                                #endregion

                                if(DEBUG_CHOSEN_PROTOTYPE != null)
                                {
                                    UNStandaloneUtility.BeginHorizontalOffset(25);
                                    DebugPrototypeInformation(DEBUG_CHOSEN_PROTOTYPE.id);
                                    UNStandaloneUtility.EndHorizontalOffset();
                                }

                                GUILayout.EndVertical();

                                GUILayout.Space(10);

                                DebugVisualSettings();

                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                GUILayout.Label("", GUILayout.Height(15));
                            }
                        }
                        else
                        {
                            GUILayout.Label("Foliage disabled. Enable it on the FoliageManager component in your scene.");
                        }

                        GUI.DragWindow();

                    }, "uNature Debug", "Window", GUILayout.Width(600), GUILayout.Height((DEBUG_Window_Minimized || !FoliageCore_MainManager.instance.enabled) ? 2 : 400));
                }

#if UNITY_EDITOR
                Handles.EndGUI();
#endif
            }

#endif
        }

        private void DebugGlobalSettings()
        {
            GUILayout.Space(20);

            GUILayout.Label(string.Format("Global Settings:    (Frame: {0})", Time.frameCount), UNStandaloneUtility.boldLabel);

            GUILayout.Space(5);

            UNStandaloneUtility.BeginHorizontalOffset(25);

            GUILayout.Label(string.Format("Current Grided-Position : {0} ({1})", latestManagerChunk == null ? "Out Of Bounds" : latestManagerChunk.transform.position.ToString(), latestManagerChunk == null ? "NaN" : latestManagerChunk.isFoliageInstanceAttached ? latestManagerChunk.GetOrCreateFoliageManagerInstance(true).foliageAreaResolutionIntegral.ToString() : "Not Poppulated"));
            GUILayout.Label("Foliage Shadow Distance: " + (FoliageCore_MainManager.instance.useQualitySettingsShadowDistance ? QualitySettings.shadowDistance : FoliageCore_MainManager.instance.foliageShadowDistance));
            GUILayout.Label("Foliage Foliage Density: " + FoliageCore_MainManager.instance.density);

            UNStandaloneUtility.EndHorizontalOffset();
        }

        private void DebugPrototypeInformation(int prototypeID)
        {
            FoliagePrototype prototype = FoliageDB.sortedPrototypes[prototypeID];

            GUILayout.Space(5);

            GUILayout.Label(string.Format("Current: {0} (ID: {1})", prototype.name, prototypeID), UNStandaloneUtility.boldLabel);

            GUILayout.Space(5);

            UNStandaloneUtility.BeginHorizontalOffset(25);

            GUILayout.Label("GPU Generated Density: " + prototype.maxGeneratedDensity);

            GUILayout.Label(string.Format("Width Noise: {0} ~ {1}", System.Math.Round(prototype.minimumWidth, 2), System.Math.Round(prototype.maximumWidth, 2)));
            GUILayout.Label(string.Format("Height Noise: {0} ~ {1}", System.Math.Round(prototype.minimumHeight, 2), System.Math.Round(prototype.maximumHeight, 2)));

            GUILayout.Space(5);

            GUILayout.Label("Wind:");

            UNStandaloneUtility.BeginHorizontalOffset(25);

            GUILayout.Label("Individual Wind: " + prototype.useCustomWind);
            GUILayout.Label("Wind Bending: " + (prototype.useCustomWind == false ? FoliageDB.instance.globalWindSettings.windBending : prototype.customWindSettings.windBending));
            GUILayout.Label("Wind Speed: " + (prototype.useCustomWind == false ? FoliageDB.instance.globalWindSettings.windSpeed : prototype.customWindSettings.windSpeed));

            UNStandaloneUtility.EndHorizontalOffset();

            UNStandaloneUtility.EndHorizontalOffset();
        }

        private void DebugVisualSettings()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(10);

            GUILayout.Label("Visual Settings:", UNStandaloneUtility.boldLabel);

            UNStandaloneUtility.BeginHorizontalOffset(25);

            GUILayout.Space(5);

            GUILayout.Label("Vertices: " + string.Format("{0:n0}", lastRenderedVertices));
            GUILayout.Label("Draw Calls: " + string.Format("{0:n0}", lastRenderedDrawCalls));
            GUILayout.Label("Prototypes Drawn: " + string.Format("{0:n0}", lastRenderedPrototypes));

            UNStandaloneUtility.EndHorizontalOffset();

            GUILayout.EndVertical();
        }

        private void ClampDebugWindow()
        {
            float halfWidth = debugWindowRect.width / 2;
            float halfHeight = debugWindowRect.height / 2;

            debugWindowRect.x = Mathf.Clamp(debugWindowRect.x, -halfWidth, Screen.width - halfWidth);
            debugWindowRect.y = Mathf.Clamp(debugWindowRect.y, -halfHeight, Screen.height - halfHeight);
        }
        #endregion
    }

    public enum FoliageGenerationRadius
    {
        _1x1 = 1,
        _3x3 = 3,
        _5x5 = 5
    }

    /// <summary>
    /// A class used to hold the gpu meshes
    /// </summary>
    public class GPUMesh
    {
        /// <summary>
        /// 1RD Dimension -> mesh lods index
        /// 2RD Dimension -> density lods index
        /// 3RD Dimension -> Mesh index
        /// </summary>
        public Mesh[,,] meshesCache = null;
        public int meshLODsCount
        {
            get
            {
                return meshesCache.GetLength(0);
            }
        }
        public int densityLODsCount
        {
            get
            {
                return meshesCache.GetLength(1);
            }
        }
        public int meshesCount
        {
            get
            {
                return meshesCache.GetLength(2);
            }
        }

        /// <summary>
        /// Dimension 1 : x chunk
        /// Dimension 2 : z chunk
        /// Dimension 3 : LOD index
        /// </summary>
        public FoliageMeshInstancesGroup LODMeshInstances = null;

        public GPUMesh(Mesh[,,] meshesCache, FoliageMeshInstancesGroup LODMeshInstances, int prototypeIndex, FoliageResolutions resolution)
        {
            this.meshesCache        = meshesCache;
            this.LODMeshInstances   = LODMeshInstances;
        }

        public void Destroy()
        {
            for(int i = 0; i < LODMeshInstances.Count; i++)
            {
                LODMeshInstances.meshInstances[i].Destroy();
            }

            Mesh mesh;

            for (int a = 0; a < meshesCache.GetLength(0); a++)
            {
                for (int b = 0; b < meshesCache.GetLength(1); b++)
                {
                    for (int c = 0; c < meshesCache.GetLength(2); c++)
                    {
                        mesh = meshesCache[a, b, c];

                        if (mesh == null) continue;

                        Object.DestroyImmediate(mesh);
                    }
                }
            }

            meshesCache = null;
            LODMeshInstances = null;
        }
        public int GetMesh(int density, FoliagePrototype prototype)
        {
            if (density == 0) return -1;
            var lodLevels = prototype.densitiesLODs;

            for (int i = lodLevels.Count - 1; i >= 0; i--)
            {
                if (density <= lodLevels[i])
                {
                    return i;
                }
            }

            return 0;
        }
    }

    public class FoliageMeshInstancesGroup
    {
        private List<FoliageMeshInstance> tempMeshInstances = new List<FoliageMeshInstance>();
        public FoliageMeshInstance[] meshInstances = null;

        private int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        public FoliageMeshInstancesGroup()
        {
            meshInstances = null;
            tempMeshInstances = new List<FoliageMeshInstance>();
            count = 0;
        }

        public void Add(FoliageMeshInstance instance)
        {
            tempMeshInstances.Add(instance);
            count = tempMeshInstances.Count;
        }

        public void Finish()
        {
            meshInstances = new FoliageMeshInstance[tempMeshInstances.Count];

            for(int i = 0; i < meshInstances.Length; i++)
            {
                meshInstances[i] = tempMeshInstances[i];
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < meshInstances.Length; i++)
            {
                meshInstances[i].Destroy();
            }
            tempMeshInstances.Clear();
            System.Array.Clear(meshInstances, 0, meshInstances.Length);

            count = 0;
        }
    }
}