using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using System.Diagnostics;
using uNature.Core.Sectors;
using uNature.Core.Utility;
using Debug = UnityEngine.Debug;

namespace uNature.Core.FoliageClasses
{
    public delegate void OnFoliageManagerAssigned(FoliageCore_MainManager instance);

    [ExecuteInEditMode]
    public sealed class FoliageCore_MainManager : FoliageMeshManager
    {
        public const int FOLIAGE_MAIN_AREA_RADIUS = 10240; // -10240 -> 10240 (x & y) [20,480 * 20,000 = 419,430 units] 
        internal const int FOLIAGE_MAIN_AREA_RESOLUTION = 40; // 40 res -> 10240 * 2 = 20480 / 40 = 512. 

        public const int FOLIAGE_INSTANCE_AREA_SIZE = (FOLIAGE_MAIN_AREA_RADIUS * 2) / FOLIAGE_MAIN_AREA_RESOLUTION; // 512
        internal const int FOLIAGE_INSTANCE_AREA_BOUNDS = FOLIAGE_INSTANCE_AREA_SIZE * FOLIAGE_INSTANCE_AREA_SIZE; // 512 * 512

        #region Static Values
        internal static Vector3 FOLIAGE_MAIN_AREA_SECTOR_SIZE = new Vector3(FOLIAGE_MAIN_AREA_RADIUS * 2, 0, FOLIAGE_MAIN_AREA_RADIUS * 2);
        internal static Vector3 FOLIAGE_MAIN_AREA_BOUNDS_MIN = new Vector3(0, 0, 0);
        internal static Vector3 FOLIAGE_MAIN_AREA_BOUNDS_MAX = new Vector3(FOLIAGE_MAIN_AREA_RADIUS, FOLIAGE_MAIN_AREA_RADIUS, FOLIAGE_MAIN_AREA_RADIUS);
        internal static Bounds FOLIAGE_MAIN_AREA_BOUNDS = new Bounds(FOLIAGE_MAIN_AREA_BOUNDS_MIN, FOLIAGE_MAIN_AREA_BOUNDS_MAX * 2);

        internal static Vector3 FOLIAGE_INSTANCE_AREA_BOUNDS_MIN = new Vector3(0, 0, 0);
        internal static Vector3 FOLIAGE_INSTANCE_AREA_BOUNDS_MAX = new Vector3(FOLIAGE_INSTANCE_AREA_SIZE, FOLIAGE_INSTANCE_AREA_SIZE, FOLIAGE_INSTANCE_AREA_SIZE);

        private static int WARM_UP_LAST_FRAME = -1;

        /// <summary>
        /// Check if certain world cords are out of bounds.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        internal static bool CheckCordsOutOfBounds(float x, float z, float sizeX, float sizeZ)
        {
            bool outOfBounds = x < -FOLIAGE_MAIN_AREA_RADIUS || z < -FOLIAGE_MAIN_AREA_RADIUS
                || (x + sizeX) > FOLIAGE_MAIN_AREA_RADIUS || (z + sizeZ) > FOLIAGE_MAIN_AREA_RADIUS;

            return outOfBounds;
        }

        internal static List<UNMap> FOLIAGE_MAPS_WAITING_FOR_SAVE = new List<UNMap>();

        public static event OnFoliageManagerAssigned OnFoliageManagerAssignedEvent;

        private static FoliageCore_MainManager _instance;
        public static FoliageCore_MainManager instance
        {
            get
            {
                return _instance;
            }
            private set
            {
                _instance = value;

                if (_instance != value)
                {
                    if (OnFoliageManagerAssignedEvent != null)
                    {
                        OnFoliageManagerAssignedEvent(value);
                    }
                }
            }
        }
        #endregion

        #region Global Values
        [SerializeField]
        private string _guid = null;
        public string guid
        {
            get
            {
                if (_guid == null)
                {
                    _guid = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
#endif
                }

                return _guid;
            }
        }

        [SerializeField]
        private bool _enabled = true;
        public new bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        [SerializeField]
        float _density = 1;
        public float density
        {
            get
            {
                return _density;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);

                if (_density != value)
                {
                    _density = value;

                    FoliageDB.instance.UpdateShaderGeneralSettings();
                    FoliageMeshManager.MarkDensitiesDirty();
                }
            }
        }

        [SerializeField]
        bool _useColorMaps = false;
        public bool useColorsMaps
        {
            get { return _useColorMaps; }
            set
            {
                if (_useColorMaps == value) return;

                _useColorMaps = value;

                for (var i = 0; i < sector.chunks.Count; i++)
                {
                    var chunk = sector.chunks[i] as FoliageCore_Chunk;
                    if (chunk == null || !chunk.isFoliageInstanceAttached || chunk.attachedFoliageInstance.colorMap == null) continue;

                    if(!value && chunk.attachedFoliageInstance != null) DestroyImmediate(chunk.attachedFoliageInstance.colorMap);
                }
            }
        }

        [SerializeField]
        int _globalFadeDistance = 100;
        public int globalFadeDistance
        {
            get
            {
                return _globalFadeDistance;
            }
            set
            {
                if (_globalFadeDistance != value)
                {
                    _globalFadeDistance = value;

                    GenerateFoliageMeshInstances();
                    RegenerateQueueInstances();

                    FoliageDB.instance.UpdateShaderGeneralSettings();
                }
            }
        } 

        [SerializeField]
        private int _FoliageGenerationLayerMask = 1;
        public int FoliageGenerationLayerMask
        {
            get
            {
                return _FoliageGenerationLayerMask;
            }
            set
            {
                if (_FoliageGenerationLayerMask != value)
                {
                    _FoliageGenerationLayerMask = value;

#if UNITY_EDITOR
                    if (!Application.isPlaying && UnityEditor.EditorUtility.DisplayDialog("Generation Masks", "Apply generation masks changes to manager instances?", "Yes", "No"))
                    {
                        FoliageWorldMaps.ReGenerateGlobally();
                        Debug.Log("uNature: All Of The World Maps Have Been Updated!");
                    }
#endif
                }
            }
        }

        [SerializeField]
        private Color _foliageGlobalTint = Color.white;
        public Color foliageGlobalTint
        {
            get { return _foliageGlobalTint; }
            set
            {
                if(_foliageGlobalTint != value)
                {
                    _foliageGlobalTint = value;

                    FoliageDB.instance.UpdateShaderGeneralSettings();
                }
            }
        }

        public bool useQualitySettingsShadowDistance = false;
        public float foliageShadowDistance = 100;
        #endregion

        #region Sectors
        [SerializeField]
        private FoliageCore_Sector _sector = null;
        public FoliageCore_Sector sector
        {
            get
            {
                if (_sector == null)
                {
                    _sector = Sector.GenerateSector<FoliageCore_Sector, FoliageCore_Chunk>(transform, FOLIAGE_MAIN_AREA_SECTOR_SIZE, _sector, FOLIAGE_MAIN_AREA_RESOLUTION);
                }

                return _sector;
            }
        }

        /// <summary>
        /// Get chunk from bounds.
        /// 
        /// [REMOVE MAIN MANAGER POSITION FROM CORDS!!]
        /// for example:
        /// cordX = transform.position.x - FoliageCore_MainManager.instance.transform.position.x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int GetChunkID(float x, float z)
        {
            return Mathf.FloorToInt(x / FOLIAGE_INSTANCE_AREA_SIZE) + Mathf.FloorToInt(z / FOLIAGE_INSTANCE_AREA_SIZE) * FOLIAGE_MAIN_AREA_RESOLUTION;
        }

        /// <summary>
        /// Check if the chunk id is in range
        /// </summary>
        /// <param name="chunkID"></param>
        /// <returns></returns>
        public bool CheckChunkInBounds(int chunkID)
        {
            return chunkID >= 0 && chunkID < (FOLIAGE_MAIN_AREA_RESOLUTION * FOLIAGE_MAIN_AREA_RESOLUTION);
        }
        #endregion

        #region Constructors
        protected override void Awake()
        {
            base.Awake();

            Threading.UNThreadManager.InitializeIfNotAvailable();

            #if UN_MapMagic
            // create map data to save spikes in the future
            //FoliageGrassMap.CreateMapData(FOLIAGE_INSTANCE_AREA_SIZE);
            #endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            instance = this;

            if (sector == null) { } // call this to force the sector to be created if not available.

            FoliagePrototype.OnFoliageEnabledStateChangedEvent += OnFoliagePrototypeChanged;

            FoliageDB.instance.UpdateShaderGeneralSettings();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            FoliagePrototype.OnFoliageEnabledStateChangedEvent -= OnFoliagePrototypeChanged;
        }

        private void OnDestroy()
        {
            if(_instance == this)
            {
                _instance = null;
            }
        }

        private void OnValidate()
        {
            var prototypes = FoliageDB.unSortedPrototypes;
            for (var i = 0; i < prototypes.Count; i++)
            {
                var prototype = prototypes[i];
                if (prototype == null) continue;

                prototype.UpdateKeywords();
                prototype.UpdateLODs();
                prototype.UpdateManagerInformation();
            }
        }

        /// <summary>
        /// Called when the enable state of a prototype is changed
        /// </summary>
        /// <param name="changedPrototype"></param>
        /// <param name="value"></param>
        private void OnFoliagePrototypeChanged(FoliagePrototype changedPrototype, bool value)
        {
            if (value)
            {
                foreach (var meshInstances in prototypeMeshInstances)
                {
                    GenerateFoliageMeshInstanceForIndex(changedPrototype.id, meshInstances.Key);
                }

                WarmUpGrassMaps();
            }
            else
            {
                DestroyMeshInstance(changedPrototype.id);
            }

            RegenerateQueueInstances();
        }

        protected override void Update()
        {
            if (enabled)
            {
                base.Update();
            }
        }
        #endregion

        #region Terrain -> uNature
        /// <summary>
        /// Copy the terrain's details and use it with the custom Foliage system.
        /// </summary>
        /// <param name="terrain"></param>
        public void InsertFoliageFromTerrain(Terrain terrain, bool removeUnityGrass = true)
        {
            var terrainData = terrain.terrainData;
            var terrainPositionX = terrain.transform.position.x;
            var terrainPositionZ = terrain.transform.position.z;
            var terrainSize = terrainData.size.x;

            var sector = this.sector;
            var resolution = terrainData.detailWidth;

            var terrainConvertionRatio = terrainSize / resolution;
            var terrainAdjuster = terrainConvertionRatio > 1 ? terrainConvertionRatio : 1f;

            var details = UNStandaloneUtility.GetTerrainDetails(terrainData);

            var prototypes = UNStandaloneUtility.AddPrototypesIfDontExist(terrainData.detailPrototypes);
            WarmUpGrassMaps(true);

            var managerInstancePositionX = transform.position.x;
            var managerInstancePositionZ = transform.position.z;

            Debug.Log("Copying from terrain. \nSettings: " + terrainAdjuster + " " + terrainConvertionRatio + " " + resolution);

            try
            {
                for (var x = 0; x < resolution; x++)
                {
                    for (var z = 0; z < resolution; z++)
                    {
                        var initWorldX = x * terrainConvertionRatio + terrainPositionX;
                        var initWorldZ = z * terrainConvertionRatio + terrainPositionZ;

                        for (var worldX = initWorldX; worldX < initWorldX + terrainAdjuster; worldX++)
                        {
                            for (var worldZ = initWorldZ; worldZ < initWorldZ + terrainAdjuster; worldZ++)
                            {
                                var mChunk = sector.foliageChunks[GetChunkID(worldX - managerInstancePositionX, worldZ - managerInstancePositionZ)];

                                var mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                                var localX = mInstance.TransformCord(worldX, mInstance.transform.position.x);
                                var localZ = mInstance.TransformCord(worldZ, mInstance.transform.position.z);

                                var mInstanceIndex = localX + localZ * mInstance.foliageAreaResolutionIntegral;
                                var grassMap = mInstance.grassMap;

                                for (var prototypeIndex = 0; prototypeIndex < prototypes.Length; prototypeIndex++)
                                {
                                    var prototype = prototypes[prototypeIndex].id;
                                    var currentDensity = grassMap.GetPrototypeDensity(localX, localZ, prototype);

                                    var density = (byte) details[prototypeIndex][z, x];
                                    if (density <= 0 && currentDensity == 0) continue;

                                    grassMap.SetDensity(mInstanceIndex, prototype, density);
                                    grassMap.SetPixels32Delayed();
                                }
                            }
                        }

                        // return density back to 0 on terrain
                        for (int prototypeIndex = 0; prototypeIndex < prototypes.Length; prototypeIndex++)
                        {
                            details[prototypeIndex][z, x] = 0;
                        }
                    }
                }

                if (removeUnityGrass)
                {
                    for (var prototypeIndex = 0; prototypeIndex < prototypes.Length; prototypeIndex++)
                    {
                        terrainData.SetDetailLayer(0, 0, prototypeIndex, details[prototypeIndex]);
                    }
                }

                SaveDelayedMaps();

                RegenerateQueueInstances();

                if (!FoliageGenerationLayerMask.isBitMasked(terrain.gameObject.layer)) // if terrain layer isn't in the generation mask
                {
                    #if UNITY_EDITOR
                    if (EditorUtility.DisplayDialog("Add Layer", "Terrain layer isn't included in the heights layers. That means that it will ignore the height mask of the terrain. \nWould you like uNature to automatically add it for you?", "Yes", "No"))
                    {
                        FoliageGenerationLayerMask |= (1 << terrain.gameObject.layer);
                    }
                    #endif
                }
            }
            catch(System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Update the heights on a terrain
        /// </summary>
        /// <param name="terrain"></param>
        public void UpdateHeightsOnTerrain(Terrain terrain)
        {
            UpdateHeights((int)terrain.transform.position.x, (int)terrain.transform.position.z, (int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z);
        }
        #endregion

        #region Foliage Methods
        /// <summary>
        /// Update Heights On Cords
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldZ"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleZ"></param>
        public void UpdateHeights(int worldX, int worldZ, int scaleX, int scaleZ)
        {
            var position = transform.position;
            for (var xIndex = worldX; xIndex < worldX + scaleX; xIndex++)
            {
                for (var zIndex = worldZ; zIndex < worldZ + scaleZ; zIndex++)
                {
                    var mChunkIndex = GetChunkID(xIndex - position.x, zIndex - position.z);

                    var mChunk = sector.foliageChunks[mChunkIndex];

                    if (!mChunk.isFoliageInstanceAttached) continue;

                    var mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                    var interpolatedX = mInstance.TransformCord(xIndex, mInstance.pos.x);
                    var interpolatedZ = mInstance.TransformCord(zIndex, mInstance.pos.z);

                    var interpolatedSize = mInstance.TransformCord(1, 0);

                    if (interpolatedSize == 0)
                        interpolatedSize = 1;

                    mInstance.worldMaps.UpdateHeightsAndNormals(interpolatedX, interpolatedZ, interpolatedSize, interpolatedSize, false);
                    mInstance.worldMaps.SetPixels32Delayed();
                }
            }
        }

        /// <summary>
        /// Set detail layer in world cords
        /// </summary>
        /// <param name="worldX">WORLD CORDS!!</param>
        /// <param name="worldZ">WORLD CORDS!!</param>
        /// <param name="sizeX">WORLD CORDS!!</param>
        /// <param name="sizeZ">WORLD CORDS!!</param>
        /// <param name="prototypeIndex">prototype.id</param>
        public byte[,] GetDetailLayer(int worldX, int worldZ, int sizeX, int sizeZ, byte prototypeIndex)
        {
            if (sector == null) return null;

            var densities = new byte[sizeX, sizeZ];
            var pos = transform.position;

            var endX = worldX + sizeX;
            var endZ = worldZ + sizeZ;

            for (var x = worldX; x < endX; x++)
            {
                for (var z = worldZ; z < endZ; z++)
                {
                    var mChunkIndex = GetChunkID(x - pos.x, z - pos.z);

                    if (!CheckChunkInBounds(mChunkIndex)) continue; // if position is out of bounds continue to the next position [very rare]

                    var mChunk = _sector.foliageChunks[mChunkIndex];

                    var mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                    var interpolatedX = mInstance.TransformCord(x, mInstance.transform.position.x);
                    var interpolatedZ = mInstance.TransformCord(z, mInstance.transform.position.z);

                    var grassMap = mInstance.grassMap;

                    densities[x - worldX, z - worldZ] =
                        grassMap.GetPrototypeDensity(interpolatedX, interpolatedZ, prototypeIndex);
                }
            }

            return densities;
        }

        /// <summary>
        /// Set detail layer in world cords
        /// </summary>
        /// <param name="worldX">WORLD CORDS!!</param>
        /// <param name="worldZ">WORLD CORDS!!</param>
        /// <param name="sizeX">WORLD CORDS!!</param>
        /// <param name="sizeZ">WORLD CORDS!!</param>
        /// <param name="prototypeIndex">prototype.id</param>
        /// <param name="densities">the density in bytes from 0 -> 15</param>
        public void SetDetailLayer(int worldX, int worldZ, byte[,] densities, byte prototypeIndex)
        {
            if (sector == null) return;

            var position = transform.position;

            var sizeX = densities.GetLength(0);
            var sizeZ = densities.GetLength(1);

            if (densities.GetLength(0) != sizeX || densities.GetLength(1) != sizeZ)
            {
                Debug.LogError("uNature: Densities out of range!!");

                return;
            }

            var endX = worldX + sizeX;
            var endZ = worldZ + sizeZ;

            for (var x = worldX; x < endX; x++)
            {
                for (var z = worldZ; z < endZ; z++)
                {
                    var mChunkIndex = GetChunkID(x - position.x, z - position.z);

                    if (!CheckChunkInBounds(mChunkIndex)) continue; // if position is out of bounds continue to the next position [very rare]

                    var mChunk = _sector.foliageChunks[mChunkIndex];

                    var mInstance = mChunk.GetOrCreateFoliageManagerInstance();
                    var grassMap = mInstance.grassMap;

                    var interpolatedX = mInstance.TransformCord(x, mInstance.transform.position.x);
                    var interpolatedZ = mInstance.TransformCord(z, mInstance.transform.position.z);
                    var index = interpolatedX + interpolatedZ * grassMap.mapWidth;

                    grassMap.SetDensity(index, prototypeIndex, densities[x - worldX, z - worldZ]);
                    grassMap.SetPixels32Delayed();
                }
            }

            SaveDelayedMaps();
            RegenerateQueueInstances();
        }

        /// <summary>
        /// Discard 
        /// </summary>
        public void DiscardEmptyManagerInstances()
        {
            for (var i = 0; i < sector.foliageChunks.Count; i++)
            {
                var mInstance = sector.foliageChunks[i].attachedFoliageInstance;

                if(mInstance != null && mInstance.IsEmpty)
                {
                    FoliageManagerInstance.CleanUp(mInstance);
                }
            }
        }
        #endregion

        #region Statics
        internal static Dictionary<int, GPUMesh> GetPrototypeMeshInstances(FoliageResolutions resolution)
        {
            if (!prototypeMeshInstances.ContainsKey(resolution))
            {
                GenerateFoliageMeshInstances(resolution);
            }

            return prototypeMeshInstances[resolution];
        }

        /// <summary>
        /// Save maps that have been marked as delayed (waiting for update)
        /// </summary>
        public static void SaveDelayedMaps()
        {
            for (int i = 0; i < FOLIAGE_MAPS_WAITING_FOR_SAVE.Count; i++)
            {
                FOLIAGE_MAPS_WAITING_FOR_SAVE[i].ApplySetPixelsDelayed();
            }
            FOLIAGE_MAPS_WAITING_FOR_SAVE.Clear();
        }

        /// <summary>
        /// Update the existing grass maps
        /// </summary>
        /// <param name="prototype"></param>
        public static void WarmUpGrassMaps(bool AdvancedPoppulation_MeshInstances_MapPixels = false)
        {
            if (instance == null) return;

            var chunks = instance.sector.foliageChunks;
            FoliageCore_Chunk chunk;

            FoliageManagerInstance mInstance;

            for (int i = 0; i < chunks.Count; i++)
            {
                chunk = chunks[i];

                if (!chunk.isFoliageInstanceAttached) continue; // if no manager instance attached then there's nothing to remove!.

                mInstance = chunk.GetOrCreateFoliageManagerInstance();

                mInstance.UpdateGrassMapsForMaterials(AdvancedPoppulation_MeshInstances_MapPixels);

                if (AdvancedPoppulation_MeshInstances_MapPixels && mInstance.meshInstances == null) { } // warm up mesh instances if enabled.
            }
        }

        /// <summary>
        /// Update the existing grass maps
        /// </summary>
        /// <param name="prototype"></param>
        public static void WarmUpGrassMaps(FoliageCore_Chunk[] specificChunks, bool AdvancedPoppulation_MeshInstances_MapPixels = false)
        {
            if (instance == null) return;

            FoliageCore_Chunk chunk;

            FoliageManagerInstance mInstance;

            int currentFrame = Time.frameCount;

            if (currentFrame != WARM_UP_LAST_FRAME)
            {
                for (int i = 0; i < specificChunks.Length; i++)
                {
                    chunk = specificChunks[i];

                    if (chunk == null || !chunk.isFoliageInstanceAttached) continue; // if no manager instance attached then there's nothing to remove!.

                    mInstance = chunk.GetOrCreateFoliageManagerInstance();

                    mInstance.UpdateGrassMapsForMaterials(AdvancedPoppulation_MeshInstances_MapPixels);

                    if(AdvancedPoppulation_MeshInstances_MapPixels && mInstance.meshInstances == null) { } // warm up mesh instances if enabled.
                }

                WARM_UP_LAST_FRAME = currentFrame;
            }
        }

        /// <summary>
        /// Reset the existing grass maps
        /// </summary>
        /// <param name="prototype"></param>
        public static void ResetGrassMap(List<FoliagePrototype> prototypes)
        {
            if (instance == null) return;

            var chunks = instance.sector.foliageChunks;

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                if (!chunk.isFoliageInstanceAttached) continue; // if no manager instance attached then there's nothing to remove!.

                var mInstance = chunk.GetOrCreateFoliageManagerInstance();
                mInstance.grassMap.ResetDensity();
            }
        }

        /// <summary>
        /// Create an instance if not created
        /// </summary>
        public static void InitializeAndCreateIfNotFound()
        {
            if (instance != null) return;

            var instanceGameObject = new GameObject("Foliage Main Manager [DESTROY ONLY FROM FOLIAGE MANAGER WINDOW!!]");

            instanceGameObject.transform.position =
                new Vector3(-FOLIAGE_MAIN_AREA_RADIUS, -500, -FOLIAGE_MAIN_AREA_RADIUS);

            instanceGameObject.AddComponent<FoliageCore_MainManager>();

            #if UNITY_EDITOR
            #if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            #else
            UnityEditor.EditorApplication.MarkSceneDirty();
            #endif
            #endif
        }

        /// <summary>
        /// Destroy this manager instance and clean up the data.
        /// </summary>
        public static void DestroyManager()
        {
            FoliageCore_Chunk mChunk;
            FoliageManagerInstance mInstance;

            try
            {
                for (int i = 0; i < instance.sector.foliageChunks.Count; i++)
                {
                    mChunk = instance.sector.foliageChunks[i];

                    if (mChunk.isFoliageInstanceAttached)
                    {
                        mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                        FoliageManagerInstance.CleanUp(mInstance);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error occured while cleaning manager instances. \n" + ex.ToString());
            }

            GameObject.DestroyImmediate(instance.sector.gameObject);
            GameObject.DestroyImmediate(instance.gameObject);

            _instance = null;

            #if UNITY_EDITOR
            #if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            #else
            UnityEditor.EditorApplication.MarkSceneDirty();
            #endif
            #endif
        }
        #endregion
    }

    public enum FoliageResolutions
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }
}
