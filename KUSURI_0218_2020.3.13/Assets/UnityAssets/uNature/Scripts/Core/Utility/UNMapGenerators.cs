using UnityEngine;
using System.Collections.Generic;

using uNature.Core.FoliageClasses;
using uNature.Core.Settings;
using uNature.Wrappers.Linq;
using System;

namespace uNature.Core.Utility
{
    public static class UNMapGenerators
    {
        private static Dictionary<int, Color32[]> colorCache = new Dictionary<int, Color32[]>();

        public static Texture2D GenerateColorMap(float x, float z, int size, FoliageManagerInstance mInstance, bool renderTexture = true)
        {
            Texture2D screenShot = null;

            if (!renderTexture)
            {
                screenShot = new Texture2D(size, size, TextureFormat.RGB24, false);
                return screenShot;
            }

            bool brushProjectorEnabled = UNBrushUtility.projector.enabled;

            UNBrushUtility.projector.enabled = false;

#if UNITY_EDITOR
            UNEditorUtility.StartSceneScrollbar("Calculating Color Map", 1);
#endif

            GameObject go = new GameObject("Temp_uNature_ColorMap_Generator");
            Camera camera = go.AddComponent<Camera>();

            float shadowDistance = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = 0;

            List<Terrain> dirtyTerrains = new List<Terrain>();
            List<Terrain.MaterialType> materialTypes = new List<Terrain.MaterialType>();

            var terrains = GameObject.FindObjectsOfType<Terrain>();
            Terrain terrain;

            for (int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];

                materialTypes.Add(terrain.materialType);
                terrain.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;

                if (terrain.drawTreesAndFoliage)
                {
                    terrain.drawTreesAndFoliage = false;
                    dirtyTerrains.Add(terrain);
                }
            }

            try
            {
                go.transform.position = new Vector3(x + size / 2, size, z + size / 2);

                camera.aspect = 1;
                camera.farClipPlane = size + 500;
                camera.fieldOfView = 53;

                go.transform.LookAt(new Vector3(x + size / 2, 0, z + size / 2));

                RenderTexture rt = new RenderTexture(size, size, 24);
                camera.targetTexture = rt;
                screenShot = new Texture2D(size, size, TextureFormat.RGB24, false);
                camera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                camera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors

                byte[] bytes = screenShot.EncodeToPNG();

                GameObject.DestroyImmediate(rt);
                GameObject.DestroyImmediate(camera);

                GameObject.DestroyImmediate(screenShot);

                screenShot =
                    new Texture2D(size, size, TextureFormat.RGB24, false, true)
                    {
                        name = "Color Map Texture_Chunk: " + mInstance.pos
                    };
                screenShot.LoadImage(bytes);

                /*
                if (Application.isPlaying) // if on runtime, create a fake instance
                {
                    screenShot = new Texture2D(size, size, TextureFormat.RGB24, false, true);
                    screenShot.LoadImage(bytes);
                }
                else //if on editor time compress
                {
                    screenShot = CreateAndSaveTexture(GetColorMapPath(mInstance), bytes, size, TextureFormat.RGB24);
                }
                */

                GameObject.DestroyImmediate(go);

#if UNITY_EDITOR
                UNEditorUtility.currentProgressIndex = 1;
#endif
            }
            catch (System.Exception ex)
            {
                QualitySettings.shadowDistance = shadowDistance;

                UNBrushUtility.projector.enabled = brushProjectorEnabled; // restore brush settings

                for (int i = 0; i < materialTypes.Count; i++)
                {
                    terrains[i].materialType = materialTypes[i];
                }

                for (int i = 0; i < dirtyTerrains.Count; i++)
                {
                    dirtyTerrains[i].drawTreesAndFoliage = true;
                }

                Debug.Log(ex.ToString());

                return null;
            }

            QualitySettings.shadowDistance = shadowDistance; // restore shadows
            UNBrushUtility.projector.enabled = brushProjectorEnabled; // restore brush settings

            for (int i = 0; i < materialTypes.Count; i++)
            {
                terrains[i].materialType = materialTypes[i];
            }

            for (int i = 0; i < dirtyTerrains.Count; i++)
            {
                dirtyTerrains[i].drawTreesAndFoliage = true;
            }

            return screenShot;
        }

        /// <summary>
        /// This will generate the texture itself instead of the whole FoliageGrassMap like the "CreateGrassMap" method.
        /// </summary>
        /// <param name="prototypeIndex"></param>
        /// <param name="mInstance"></param>
        /// <returns></returns>
        public static Texture2D GenerateGrassMap(FoliageManagerInstance mInstance)
        {
            int size = mInstance.foliageAreaResolutionIntegral;

            Texture2D map;

            try
            {
                map = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
                {
                    filterMode = FilterMode.Point,
                    name = "Grass Map Texture_Chunk: " + mInstance.pos
                };
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(mInstance);

#if UNITY_5_3_OR_NEWER
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#else
                UnityEditor.EditorApplication.MarkSceneDirty();
#endif
            }
#endif

            return map;
        }

        public static FoliageGrassMap CreateGrassMap(FoliageManagerInstance mInstance)
        {
            var size = mInstance.foliageAreaResolutionIntegral;
            var grassMap = new FoliageGrassMap(GenerateGrassMap(mInstance), PoolColors(size), mInstance);

            return grassMap;
        }

        public static string GetColorMapPath(FoliageManagerInstance mInstance)
        {
            return GetMapPath("Color", mInstance.guid, "");
        }

        public static void DisposeMap(Texture2D map, string path)
        {
            if (map != null)
            {
                UnityEngine.Object.DestroyImmediate(map);

                /*
                #if UNITY_EDITOR
                if(!Application.isPlaying)
                {
                    UnityEditor.AssetDatabase.DeleteAsset(UNSettings.ProjectPath + "Resources/" + path + ".png");
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                #endif
                */
            }
        }

        public static void SaveGrassMaps(FoliageManagerInstance mInstance)
        {
            mInstance.grassMap.Save();
        }

        public static FoliageWorldMaps GenerateWorldMaps(FoliageManagerInstance mInstance, bool updateHeights = true)
        {
            if (mInstance == null) return null;

            int mapResolution = mInstance.foliageAreaResolutionIntegral;

            FoliageWorldMaps worldsInformation;

            try
            {
                var heightMapTexture =
                    new Texture2D(mapResolution, mapResolution, TextureFormat.RGB24, false, true)
                    {
                        filterMode = FilterMode.Point,
                        name = "Height Map Texture_Chunk: " + mInstance.pos
                    };

                worldsInformation = new FoliageWorldMaps(heightMapTexture, mInstance, updateHeights);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(mInstance);

#if UNITY_5_3_OR_NEWER
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#else
                UnityEditor.EditorApplication.MarkSceneDirty();
#endif
            }
#endif

            return worldsInformation;
        }

        internal static Color32[] PoolColors(int mapResolution)
        {
            Color32[] cache;

            if (!colorCache.TryGetValue(mapResolution, out cache))
            {
                cache = new Color32[mapResolution * mapResolution];

                colorCache.Add(mapResolution, cache);
            }

            return cache;
        }

        public static void SaveMap(UNMap map, string path)
        {
            //CreateAndSaveTexture(path, map.EncodeToPNG(), map.mapWidth, map.map.format);
        }

        /// <summary>
        /// Get a hit for the maps (included with the map mask which is specified on the Foliage manager)
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static bool GetMapHit(Ray ray, out RaycastHit hit, float maxDistance)
        {
            var hits = Physics.RaycastAll(ray, maxDistance, FoliageCore_MainManager.instance.FoliageGenerationLayerMask);

            System.Array.Sort<RaycastHit>(hits, delegate (RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            });

            bool lengthBiggerThan0 = hits.Length > 0;

            hit = lengthBiggerThan0 ? hits[0] : new RaycastHit();

            return lengthBiggerThan0;
        }

        /// <summary>
        /// Get a hit for the maps (included with the map mask which is specified on the Foliage manager)
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static bool GetMapHit(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance)
        {
            return GetMapHit(new Ray(origin, direction), out hit, maxDistance);
        }

        private static RaycastHit tempHit;
        /// <summary>
        /// Get a hit for the maps (included with the map mask which is specified on the Foliage manager)
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static float GetHeightFast(Vector3 origin, Vector3 direction, int layerMask)
        {
            Physics.Raycast(origin, direction, out tempHit, Mathf.Infinity, layerMask);

            return tempHit.point.y;
        }

        /// <summary>
        /// Create and save a map texture.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void CreateAndSaveTexture(string resourcesPath, byte[] bytes, int size, TextureFormat textureFormat)
        {
            /*
            #if UNITY_EDITOR
            string projectPath = UNSettings.ProjectPath + "Resources/" + resourcesPath + ".png";

            System.IO.File.WriteAllBytes(projectPath, bytes);

            if (!Application.isPlaying)
            {
                UnityEditor.AssetDatabase.Refresh();
            }

            var textureImporter = UnityEditor.AssetImporter.GetAtPath(projectPath) as UnityEditor.TextureImporter;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.npotScale = UnityEditor.TextureImporterNPOTScale.None;
            textureImporter.isReadable = true;
            textureImporter.maxTextureSize = 512;

            textureImporter.SaveAndReimport();

            UnityEditor.AssetDatabase.SaveAssets();

            Texture2D decompressedMap = new Texture2D(size, size, textureFormat, false, true);
            decompressedMap.LoadImage(bytes);

            return decompressedMap;
            #endif
            */
        }

        /// <summary>
        /// Get map path.
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="guid"></param>
        /// <param name="identifier"></param>
        public static string GetMapPath(string mapName, string guid, string identifier)
        {
            return string.Format("Maps/{0}_{1}_{2}", mapName, guid, identifier);
        }
    }

    /// <summary>
    /// The abstract Map class.
    /// </summary>
    public abstract class UNMap
    {
        [SerializeField]
        protected Texture2D _map;
        public Texture2D map
        {
            get
            {
                return _map;
            }
            set
            {
                if (_map != value)
                {
                    _map = value;

                    _mapPixels = map == null ? null : map.GetPixels32();
                }
            }
        }

        /// <summary>
        /// Fast access to _mapPixels internally.
        /// </summary>
        [NonSerialized]
        private Color32[] _mapPixels;
        public Color32[] mapPixels
        {
            get
            {
                if (_mapPixels == null)
                {
                    _mapPixels = map.GetPixels32();
                }

                return _mapPixels;
            }
            internal set
            {
                _mapPixels = value;
            }
        }

        [SerializeField]
        private int _mapWidth;
        public int mapWidth
        {
            get
            {
                return _mapWidth;
            }
        }

        [System.NonSerialized]
        bool _dirty = false;
        public bool dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                if (_dirty != value)
                {
                    _dirty = value;

#if UNITY_EDITOR
                    if (!Application.isPlaying && map != null && value)
                    {
                        UnityEditor.EditorUtility.SetDirty(map);
                    }
#endif

                    OnDirty(value);
                }
            }
        }

        [System.NonSerialized]
        bool _saveDelayed = false;
        private bool saveDelayed
        {
            get
            {
                return _saveDelayed;
            }
            set
            {
                if (_saveDelayed != value)
                {
                    _saveDelayed = value;

                    if (value && !FoliageCore_MainManager.FOLIAGE_MAPS_WAITING_FOR_SAVE.Contains(this))
                    {
                        FoliageCore_MainManager.FOLIAGE_MAPS_WAITING_FOR_SAVE.Add(this);
                    }
                }
            }
        }

        [SerializeField]
        FoliageManagerInstance _mInstance;
        public FoliageManagerInstance mInstance
        {
            get
            {
                return _mInstance;
            }
        }

        protected UNMap()
        {
        }

        protected UNMap(Texture2D texture, Color32[] pixels, FoliageManagerInstance mInstance)
        {
            _map = texture;
            _mInstance = mInstance;

            Apply(pixels);
        }

        public void Apply(Color32[] pixels)
        {
            _mapPixels = pixels;
            _mapWidth = _map.width;

            map.Apply();

            dirty = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(map);
#endif
        }

        protected virtual void OnDirty(bool value)
        {

        }

        public void SetPixels32()
        {
            SetPixels32(mapPixels);
        }

        public void SetPixels32(Color32[] pixels)
        {
            SetPixels32(pixels, true);
        }

        public void SetPixelsNoApply()
        {
            SetPixels32(mapPixels, false);
        }

        public void SetPixels32Delayed()
        {
            _dirty = true;
            saveDelayed = true;
        }

        internal void ApplySetPixelsDelayed()
        {
            if (saveDelayed)
            {
                saveDelayed = false;

                SetPixels32();
            }
        }

        private void SetPixels32(Color32[] pixels, bool apply)
        {
            if (map == null) return;

            //Decompress();

            map.SetPixels32(pixels);
            if (apply)
            {
                Apply(pixels); // apply changes
            }

            //Compress();
        }

        /*
        protected void Compress()
        {
            map.Compress(true);

            map.filterMode = FilterMode.Point;
            map.alphaIsTransparency = true;
            map.wrapMode = TextureWrapMode.Clamp;
        }

        protected void Decompress()
        {
            map.Resize(map.width, map.height, textureFormat, false);
            map.SetPixels32(mapPixels);
        }
        */

        public void Resize(int size)
        {
            map.Resize(size, size);
            _mapWidth = size;
            _mapPixels = map.GetPixels32();

            Clear(true, Color.black);
        }

        public void Clear(bool autoApply, Color32 defaultColor)
        {
            int mapWidth = this.mapWidth;

            _mapPixels = UNMapGenerators.PoolColors(mapWidth);

            if (autoApply)
            {
                SetPixels32();
            }
        }

        public virtual void Dispose()
        {
            //
        }
    }

    /// <summary>
    /// Channels:
    ///
    /// R: Rnd-Range #1
    /// G: Rnd-Range #1
    /// B: Density
    /// A: Perlin Noise
    /// </summary>
    [System.Serializable]
    public class FoliageGrassMap : UNMap
    {
        public static string GetPath(FoliageManagerInstance mInstance)
        {
            return UNMapGenerators.GetMapPath("Detail", mInstance.guid, string.Empty);
        }

        public FoliageGrassMap(Texture2D texture, Color32[] pixels, FoliageManagerInstance mInstance) : base(texture, pixels, mInstance)
        {
        }

        public void ResetDensity()
        {
            for (var xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                for (var zIndex = 0; zIndex < mapWidth; zIndex++)
                {
                    mapPixels[xIndex + zIndex * mapWidth].r = 0;
                    mapPixels[xIndex + zIndex * mapWidth].b = 0;
                }
            }

            SetPixels32();
            Save();
        }

        /// <summary>
        /// Destroy this current grass map.
        /// </summary>
        public override void Dispose()
        {
            UNMapGenerators.DisposeMap(map, GetPath(mInstance));
        }

        /// <summary>
        /// Get density at normalized x & z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetPrototypeDensity(int x, int z, byte prototype)
        {
            try
            {
                var pixel = mapPixels[x + z * mapWidth];

                var channel = GetChannel(pixel, prototype, false);
                if (channel == 0)
                {
                    return pixel.g;
                }

                return channel == 1 ? pixel.a : (byte)0;
            }
            catch (Exception e)
            {
                Debug.LogError(map.width + " " + map.height + " " + " " + x + " " + z + " " + mapPixels.Length + " \n" + e);
                throw;
            }
        }

        /// <summary>
        /// Set density at normalized x & z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="prototypeIndex"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        public void SetDensity(int x, int z, byte prototypeIndex, byte density)
        {
            SetDensity(x + z * mapWidth, prototypeIndex, density);
        }

        /// <summary>
        /// Set density at normalized x & z
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prototypeIndex"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        public void SetDensity(int index, byte prototypeIndex, byte density)
        {
            var pixel = mapPixels[index];
            var channel = GetChannel(pixel, prototypeIndex, true);

            if (channel == 0)
            {
                pixel.r = prototypeIndex;
                pixel.g = density;
            }
            else if (channel == 1)
            {
                pixel.b = prototypeIndex;
                pixel.a = density;
            }

            mapPixels[index] = pixel;
        }

        /// <summary>
        /// Mark the densities as dirty.
        /// </summary>
        public void MarkDensitiesDirty()
        {
            FoliageMeshManager.MarkDensitiesDirty();
        }

        public void Save()
        {
            if (dirty)
            {
                SetPixels32();

                #if UNITY_EDITOR
                UNMapGenerators.SaveMap(this, GetPath(mInstance));
                #endif

                dirty = false;
            }
        }

        #region Static
        public static bool globalDirty
        {
            get
            {
                FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;
                FoliageCore_Chunk mChunk;
                FoliageManagerInstance mInstance;

                FoliageCore_Sector sector = mManager.sector;

                for (int j = 0; j < sector.foliageChunks.Count; j++)
                {
                    mChunk = sector.foliageChunks[j];

                    if (!mChunk.isFoliageInstanceAttached) continue;

                    mInstance = mChunk.GetOrCreateFoliageManagerInstance();
                    if (mInstance.grassMap.dirty) return true;
                }

                return false;
            }
        }

        public static void SaveAllMaps()
        {
            FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;

            for (int j = 0; j < mManager.sector.foliageChunks.Count; j++)
            {
                var mChunk = mManager.sector.foliageChunks[j];

                if (!mChunk.isFoliageInstanceAttached) continue;

                var mInstance = mChunk.GetOrCreateFoliageManagerInstance();
                mInstance.grassMap.Save();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="index"></param>
        /// <param name="searchNew">Is Searching for an empty slot?</param>
        /// <returns></returns>
        private static byte GetChannel(Color32 pixel, byte index, bool searchNew)
        {
            if (pixel.r == index || searchNew && pixel.r == 0) return 0;
            if (pixel.b == index || searchNew && pixel.b == 0) return 1;

            return (byte)(searchNew ? 0 : 2);
        }
        #endregion
    }

    /// <summary>
    /// Channels:
    /// R: Heights Channel #1
    /// G: Heights Channel #2
    /// B: Noise
    /// </summary>
    [System.Serializable]
    public class FoliageHeightMap : UNMap
    {
        public static string GetPath(FoliageManagerInstance mInstance)
        {
            return UNMapGenerators.GetMapPath("Height", mInstance.guid, "0");
        }

        public FoliageHeightMap(Texture2D texture, FoliageManagerInstance mInstance) : base(texture, texture.GetPixels32(), mInstance)
        {
        }

        public void Save()
        {
            if (dirty)
            {
#if UNITY_EDITOR
                UNMapGenerators.SaveMap(this, GetPath(mInstance));
#endif

                dirty = false;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            UNMapGenerators.DisposeMap(map, GetPath(mInstance));
        }

        /// <summary>
        /// Set height fast in a certain index to a world height
        /// Index = x + z * res
        /// </summary>
        public void SetHeightFast(int index, Color32 colorInstance, byte h1, byte h2)
        {
            colorInstance.r = h1;
            colorInstance.g = h2;
            colorInstance.b = (byte)UNMath.RANDOM.GetRndRange(0, 255);

            mapPixels[index] = colorInstance;
        }
    }

    [System.Serializable]
    public class FoliageWorldMaps
    {
        public const int HEIGHT_RESOLUTION = 2048;

        [SerializeField]
        private FoliageHeightMap _heightMap;
        public FoliageHeightMap heightMap
        {
            get
            {
                if (_heightMap == null || _heightMap.map == null)
                {
                    UNMapGenerators.GenerateWorldMaps(mInstance);
                    UpdateHeightsAndNormals(true);
                }

                return _heightMap;
            }
            internal set
            {
                _heightMap = value;
            }
        }

        public FoliageHeightMap heightMapFast
        {
            get
            {
                return _heightMap;
            }
        }

        [SerializeField]
        private FoliageManagerInstance mInstance;

        public bool dirty
        {
            get
            {
                return heightMap.dirty;
            }
        }

        public FoliageWorldMaps(Texture2D heightMap, FoliageManagerInstance mInstance, bool updateHeights)
        {
            this.heightMap = new FoliageHeightMap(heightMap, mInstance);
            this.mInstance = mInstance;

            if (updateHeights)
            {
                UpdateHeightsAndNormals(true);
            }

            Save();
        }

        /// <summary>
        /// Normalize a world height into a converted height.
        /// </summary>
        /// <param name="worldHeight"></param>
        /// <returns></returns>
        public static Vector2b NormalizeHeight(float worldHeight)
        {
            return NormalizeHeight(worldHeight, FoliageCore_MainManager.instance);
        }

        /// <summary>
        /// Normalize a world height into a converted height.
        /// </summary>
        /// <param name="worldHeight"></param>
        /// <returns></returns>
        public static Vector2b NormalizeHeight(float worldHeight, FoliageCore_MainManager manager)
        {
            Vector2b heights;

            float h = ((worldHeight - manager._threadPosition.y) / HEIGHT_RESOLUTION) * 65535;
            heights.x = (byte)(h / 256);
            heights.y = (byte)(h - (heights.x * 256));

            return heights;
        }

        /// <summary>
        /// Update height on a certain range.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeZ"></param>
        public void UpdateHeightsAndNormals(float x, float z, int sizeX, int sizeZ, bool save)
        {
            var mapWidth = mInstance.foliageAreaResolutionIntegral;
            var resMultiplier = mInstance.transformCordsMultiplier;

            x = Mathf.Clamp(x, 0, mapWidth);
            z = Mathf.Clamp(z, 0, mapWidth);

            var targetX = Mathf.Clamp(x + sizeX, 0, mapWidth);
            var targetZ = Mathf.Clamp(z + sizeZ, 0, mapWidth);

            var managerTransform = mInstance.transform;

            var rayPos = new Vector3(0, managerTransform.position.y + 10000, 0);
            var vDown = Vector3.down;

            var heightMap = this.heightMap;
            var emptyHeight = NormalizeHeight(0);

            var emptyColor = new Color32();
            for (var xIndex = x; xIndex < targetX; xIndex++)
            {
                for (var zIndex = z; zIndex < targetZ; zIndex++)
                {
                    rayPos.x = xIndex * resMultiplier + managerTransform.position.x; // transform to world cords
                    rayPos.z = zIndex * resMultiplier + managerTransform.position.z; // transform to world cords

                    RaycastHit hit;

                    Vector2b heights;

                    if (UNMapGenerators.GetMapHit(rayPos, vDown, out hit, Mathf.Infinity))
                    {
                        heights = NormalizeHeight(hit.point.y);
                    }
                    else
                    {
                        heights = emptyHeight;
                    }

                    var index = (int)xIndex + (int)zIndex * mapWidth;

                    heightMap.SetHeightFast(index, emptyColor, heights.x, heights.y);
                }
            }

            if (save)
            {
                heightMap.SetPixels32();
            }

            heightMap.dirty = true;
        }

        /// <summary>
        /// Update the heights and normals all over the map.
        /// </summary>
        /// <param name="save"></param>
        public void UpdateHeightsAndNormals(bool save)
        {
            UpdateHeightsAndNormals(0, 0, mInstance.foliageAreaResolutionIntegral, mInstance.foliageAreaResolutionIntegral, save);
        }

        /// <summary>
        /// Update the heights and normals all over the map by a terrain.
        /// </summary>
        /// <param name="save"></param>
        public void UpdateHeightsAndNormals(Terrain terrain)
        {
            int size = mInstance.foliageAreaResolutionIntegral;

            float adjusterX = mInstance.pos.x;
            float adjusterZ = mInstance.pos.z;

            var heights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

            float xIndex;
            float zIndex;

            int terrainSizeX = (int)terrain.terrainData.size.x;
            int terrainSizeZ = (int)terrain.terrainData.size.z;

            float h1;
            float h2;
            float h3;
            float h4;

            float v1;
            float v2;
            float v3;
            float v4;

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    xIndex = (int)(x + adjusterX - terrain.transform.position.x);
                    zIndex = (int)(z + adjusterZ - terrain.transform.position.z);

                    xIndex = Mathf.Clamp(xIndex, 0, terrainSizeX);
                    zIndex = Mathf.Clamp(zIndex, 0, terrainSizeZ);

                    xIndex = ((xIndex / terrainSizeX) * (terrain.terrainData.heightmapResolution - 1));
                    zIndex = ((zIndex / terrainSizeZ) * (terrain.terrainData.heightmapResolution - 1));

                    h1 = heights[Mathf.FloorToInt(zIndex), Mathf.FloorToInt(xIndex)];
                    h2 = heights[Mathf.CeilToInt(zIndex), Mathf.FloorToInt(xIndex)];
                    h3 = heights[Mathf.FloorToInt(zIndex), Mathf.CeilToInt(xIndex)];
                    h4 = heights[Mathf.CeilToInt(zIndex), Mathf.CeilToInt(xIndex)];

                    v1 = h1 * (Mathf.CeilToInt(xIndex) - xIndex) * (Mathf.CeilToInt(zIndex) - zIndex);
                    v2 = h2 * (xIndex - Mathf.FloorToInt(xIndex)) * (Mathf.CeilToInt(zIndex) - zIndex);
                    v3 = h3 * (Mathf.CeilToInt(xIndex) - xIndex) * (zIndex - Mathf.FloorToInt(zIndex));
                    v4 = h4 * (xIndex - Mathf.FloorToInt(xIndex)) * (zIndex - Mathf.FloorToInt(zIndex));

                    UpdateHeightAndNormal(x + z * size, (v1 + v2 + v3 + v4) * terrain.terrainData.size.y);
                }
            }

            heightMap.SetPixels32();
        }

        /// <summary>
        /// Update height
        /// </summary>
        /// <param name="worldMap"></param>
        public void UpdateHeightAndNormal(int index, float height)
        {
            Vector2b heights = NormalizeHeight(height);

            heightMap.mapPixels[index].r = heights.x;
            heightMap.mapPixels[index].g = heights.y;

            heightMap.dirty = true;
        }

        /// <summary>
        /// Update height and normal
        /// </summary>
        /// <param name="worldMap"></param>
        public void UpdateHeightAndNormal(int index, float height, Vector3 normal)
        {
            UpdateHeightAndNormal(index, height);
        }

        /// <summary>
        /// Get an height on the height map.
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public float GetHeight(int index)
        {
            Color32 pixel = heightMap.mapPixels[index];

            return (((pixel.r * 256f) + pixel.g) / 65535f) * HEIGHT_RESOLUTION;
        }

        /// <summary>
        /// Assign height to an index fast
        /// </summary>
        /// <param name="index"></param>
        /// <param name="colorInstance">An empty color instance so it doesn't allocate new ones.</param>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        public void SetHeightFast(int index, Color32 colorInstance, byte h1, byte h2)
        {
            _heightMap.SetHeightFast(index, colorInstance, h1, h2);
        }

        /// <summary>
        /// Save the world maps
        /// </summary>
        public void Save()
        {
            heightMap.Save();
        }

        /// <summary>
        /// Set pixels on the world maps, delayed.
        /// </summary>
        public void SetPixels32Delayed()
        {
            heightMap.SetPixels32Delayed();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return heightMap == null;

            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region Static
        public static bool globalDirty
        {
            get
            {
                FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;
                FoliageCore_Chunk mChunk;
                FoliageManagerInstance mInstance;

                FoliageCore_Sector sector = mManager.sector;

                for (int j = 0; j < sector.foliageChunks.Count; j++)
                {
                    mChunk = sector.foliageChunks[j];

                    if (!mChunk.isFoliageInstanceAttached) continue;

                    mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                    if (mInstance.worldMaps.dirty) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Regenerate all of the world maps.
        /// </summary>
        public static void ReGenerateGlobally()
        {
            FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;
            FoliageCore_Chunk mChunk;
            FoliageManagerInstance mInstance;

            for (int j = 0; j < mManager.sector.foliageChunks.Count; j++)
            {
                mChunk = mManager.sector.foliageChunks[j];

                if (!mChunk.isFoliageInstanceAttached) continue;

                mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                mInstance.worldMaps.UpdateHeightsAndNormals(true);
            }
        }

        /// <summary>
        /// Save all of the world maps.
        /// </summary>
        public static void SaveAllMaps()
        {
            FoliageCore_MainManager mManager = FoliageCore_MainManager.instance;
            FoliageCore_Chunk mChunk;
            FoliageManagerInstance mInstance;

            for (int j = 0; j < mManager.sector.foliageChunks.Count; j++)
            {
                mChunk = mManager.sector.foliageChunks[j];

                if (!mChunk.isFoliageInstanceAttached) continue;

                mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                mInstance.worldMaps.Save();
            }
        }

        public static void ApplyAreaSizeChange(FoliageManagerInstance mInstance)
        {
            mInstance.worldMaps.heightMap.Resize(mInstance.foliageAreaResolutionIntegral);

            UNSettings.Log("World maps updated succesfully after modifying resolution.");
        }
        #endregion
    }
}