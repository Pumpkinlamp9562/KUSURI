using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Sectors;
using uNature.Core.Utility;
using uNature.Core.Settings;
using uNature.Core.Threading;
using uNature.Wrappers.Linq;

namespace uNature.Core.FoliageClasses
{
    [ExecuteInEditMode]
    public sealed class FoliageManagerInstance : Threading.ThreadItem
    {
        public const int AREA_SIZE = FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE;

        static List<FoliageManagerInstance> _instances = null;
        public static List<FoliageManagerInstance> instances
        {
            get
            {
                if (_instances == null)
                {
                    _instances = GameObject.FindObjectsOfType<FoliageManagerInstance>().ToList();
                }

                return _instances;
            }
        }

        #region Variables
        [SerializeField]
        private string _guid;
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
            get { return _enabled; }
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                FoliageMeshManager.RegenerateQueueInstances();
            }
        }

        [System.NonSerialized]
        private Dictionary<int, GPUMesh> _meshInstances;
        public Dictionary<int, GPUMesh> meshInstances
        {
            get
            {
                if (_meshInstances == null)
                {
                    PoolMeshInstances();
                }

                return _meshInstances;
            }
        }

        /// <summary>
        /// Fast access to _pos internally.
        /// </summary>
        [SerializeField]
        internal Vector3 _pos = -Vector3.one;
        public Vector3 pos
        {
            get
            {
                return _pos;
            }
        }

        /// <summary>
        /// Is this manager instance empty? (no grass drawn on it)
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                var res = foliageAreaResolutionIntegral;
                var prototypes = FoliageDB.unSortedPrototypes;

                for (var x = 0; x < res; x++)
                {
                    for (var z = 0; z < res; z++)
                    {
                        for (var i = 0; i < prototypes.Count; i++)
                        {
                            if (grassMap.GetPrototypeDensity(x, z, (byte)prototypes[i].id) != 0) // if this unit isn't empty, return false.
                                return false;
                        }
                    }
                }

                return true; // if NONE of the grass maps weren't empty, then we are indeed empty.
            }
        }
        #endregion

        #region Maps_Sizes & Resolutions
        [SerializeField]
        FoliageResolutions _foliageAreaResolution = FoliageResolutions._512;
        public FoliageResolutions foliageAreaResolution
        {
            get
            {
                return _foliageAreaResolution;
            }
            set
            {
                if (_foliageAreaResolution != value)
                {
                    _foliageAreaResolution = value;
                    _foliageAreaResolutionIntegral = (int)value;

                    _transformCordsMultiplier = -1;

                    UpdateResolutionChange();
                }
            }
        }

        int _foliageAreaResolutionIntegral = -1;
        public int foliageAreaResolutionIntegral
        {
            get
            {
                if (_foliageAreaResolutionIntegral == -1)
                {
                    _foliageAreaResolutionIntegral = (int)foliageAreaResolution;
                }

                return _foliageAreaResolutionIntegral;
            }
        }

        [SerializeField]
        float _transformCordsMultiplier = -1;
        public float transformCordsMultiplier
        {
            get
            {
                if (_transformCordsMultiplier == -1)
                {
                    _transformCordsMultiplier = (float)AREA_SIZE / foliageAreaResolutionIntegral;
                }

                return _transformCordsMultiplier;
            }
            set
            {
                _transformCordsMultiplier = value;
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
                _FoliageGenerationLayerMask = value;
            }
        }
        #endregion

        #region Maps_Textures
        [SerializeField]
        private Texture2D _colorMap;
        public Texture2D colorMap
        {
            get
            {
                if (_colorMap == null && FoliageCore_MainManager.instance.useColorsMaps)
                {
                    colorMap = UNMapGenerators.GenerateColorMap(transform.position.x, transform.position.z, AREA_SIZE, this);
                }

                return _colorMap;
            }
            set
            {
                if (_colorMap != value)
                {
                    _colorMap = value;
                }
            }
        }

        [SerializeField]
        internal FoliageWorldMaps _worldMaps;
        public FoliageWorldMaps worldMaps
        {
            get
            {
                if (_worldMaps == null)
                {
                    _worldMaps = UNMapGenerators.GenerateWorldMaps(this);
                }

                return _worldMaps;
            }
            set
            {
                _worldMaps = value;
            }
        }

        [SerializeField]
        private FoliageGrassMap _grassMap;
        public FoliageGrassMap grassMap
        {
            get
            {
                if(_grassMap == null)
                {
                    _grassMap = UNMapGenerators.CreateGrassMap(this);
                }

                return _grassMap;
            }
        }
        #endregion

        #region Grid-Management
        [SerializeField]
        FoliageCore_Chunk _attachedTo;
        public FoliageCore_Chunk attachedTo
        {
            get
            {
                return _attachedTo;
            }
        }
        #endregion

        #region Timestamp Checks
        private int lastGrassMapsFetchingFrame;
        private bool fetchGrassMaps
        {
            get
            {
                bool fetch = lastGrassMapsFetchingFrame != Time.frameCount;
                lastGrassMapsFetchingFrame = Time.frameCount;

                return fetch;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a manager instance
        /// </summary>
        /// <param name="attachedTo">what chunk is it attached to?</param>
        /// <param name="fastMode">if marked as true it won't create a color map and won't poppulate the height map</param>
        /// <returns></returns>
        internal static FoliageManagerInstance CreateInstance(FoliageCore_Chunk attachedTo, bool poppulateHeights = true)
        {
            GameObject go = new GameObject("Foliage Manager Instance");
            go.transform.SetParent(attachedTo.transform);

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            FoliageManagerInstance mInstance = go.AddComponent<FoliageManagerInstance>();
            mInstance._attachedTo = attachedTo;
            mInstance._pos = attachedTo.transform.position;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("uNature", "Creating Manager Instance - Color Map", 0f);
            }
#endif

            if(mInstance.colorMap == null) { }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("uNature", "Creating Manager Instance - Grass Maps", 0.33f);
            }
#endif

            if (mInstance.grassMap == null) { };

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("uNature", "Creating Manager Instance - World Maps", 0.67f);
            }
#endif

            mInstance._worldMaps = UNMapGenerators.GenerateWorldMaps(mInstance, poppulateHeights);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
#endif

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
#if UNITY_5_3_OR_NEWER
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#else
                UnityEditor.EditorApplication.MarkSceneDirty();
#endif
            }
#endif

            FoliageCore_MainManager.RegenerateQueueInstances();

            return mInstance;
        }
        public static void CleanUp(FoliageManagerInstance mInstance)
        {
            // ----- Delete Grass Maps ----- //

            FoliagePrototype prototype;

            if (mInstance._grassMap != null)
            {
                mInstance._grassMap.Dispose();
            }

            // ----- Delete World Maps ----- //

            mInstance._worldMaps.heightMap.Dispose();

            // ----- Delete Color Map ----- //
            UNMapGenerators.DisposeMap(mInstance._colorMap, UNMapGenerators.GetColorMapPath(mInstance));

            // Free up memory
            mInstance._worldMaps = null;
            mInstance._colorMap = null;
            mInstance._grassMap = null;

            Resources.UnloadUnusedAssets();

            GameObject.DestroyImmediate(mInstance.gameObject);

            // Regenerate queue to avoid using destroyed instances
            FoliageMeshManager.RegenerateQueueInstances();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!instances.Contains(this))
            {
                instances.Add(this);
            }

            _pos = transform.position;
        }

        private void Start()
        {
            if (colorMap == null) { } // force creation
            if (worldMaps == null) { } // force creation
            if (grassMap == null) { } // force creation
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (instances.Contains(this))
            {
                instances.Remove(this);
            }
        }

        #endregion

        #region Size And Transform Changes Updates
        private void UpdateResolutionChange()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayProgressBar("uNature Foliage", "uNature is recalculating the new Foliage area resolution \nThat might take awhile...", 0.2f);
#endif

            // update world map
            worldMaps = UNMapGenerators.GenerateWorldMaps(this);

            // update & clean grass maps 
            DisposeExistingGrassMaps();

            _grassMap = null;
            UpdateGrassMapsForMaterials(false);

            PoolMeshInstances();

            UNSettings.Log("uNature finished updating the area resolution successfully!! New resolution : " + _foliageAreaResolutionIntegral);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        private void DisposeExistingGrassMaps()
        {
            grassMap.Dispose();
        }

        private void PoolMeshInstances()
        {
            if (FoliageCore_MainManager.instance == null) return;

            _meshInstances = FoliageCore_MainManager.GetPrototypeMeshInstances(foliageAreaResolution);
        }

        public void UpdateMaterialBlock(MaterialPropertyBlock mBlock)
        {
            // property blocks. (Unique to each manager instance).
            if (colorMap != null)
            {
                mBlock.SetTexture("_ColorMap", colorMap);
            }

            mBlock.SetTexture("_HeightMap_WORLD", worldMaps.heightMap.map);

            mBlock.SetFloat("_FoliageAreaResolution", foliageAreaResolutionIntegral);
            mBlock.SetVector("_FoliageAreaPosition", transform.position);
        }

        internal void UpdateGrassMapsForMaterials(bool poppulateMapPixels)
        {
            if (grassMap == null) return;

            if (poppulateMapPixels && grassMap.mapPixels == null) // force map pixels generation
            {
            }
        }
        #endregion

        #region Foliage Modifications Methods
        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int TransformCord(float x, float removeOffset)
        {
            return (int)TransformCordFloat(x, removeOffset);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float TransformCordFloat(float x, float removeOffset)
        {
            return TransformCordCustomFloat(x, removeOffset, transformCordsMultiplier);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int TransformCordCustom(float x, float removeOffset, float multiplier)
        {
            return (int)TransformCordCustomFloat(x, removeOffset, multiplier);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float TransformCordCustomFloat(float x, float removeOffset, float multiplier)
        {
            return (x - removeOffset) / multiplier;
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int InverseCord(float x, float addOffset)
        {
            return (int)InverseCordFloat(x, addOffset);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float InverseCordFloat(float x, float addOffset)
        {
            return InverseCordCustomFloat(x, addOffset, transformCordsMultiplier);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int InverseCordCustom(float x, float addOffset, float multiplier)
        {
            return (int)InverseCordCustomFloat(x, addOffset, multiplier);
        }

        /// <summary>
        /// Transform 1 cord
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float InverseCordCustomFloat(float x, float addOffset, float multiplier)
        {
            return (x * multiplier) + addOffset;
        }
        #endregion
    }
}
