using UnityEngine;
using uNature.Core.Utility;
using uNature.Core.Settings;

using System.Collections.Generic;

namespace uNature.Core.FoliageClasses
{
    public delegate void OnFoliageEnableChanged(FoliagePrototype changedPrototype, bool value);

    [System.Serializable]
    public sealed class FoliagePrototype : BasePrototypeItem
    {
        #region Static Variables
        public const string SHADER_BASIC_NAME = "uNature/FoliageShader_Basic";
        public const string SHADER_ADVANCED_NAME = "uNature/FoliageShader_Advanced";

        public const int MAX_GENERATION_DENSITY = 20;

        public const float SIZE_MIN_VALUE = 0.1f;
        public const float SIZE_MAX_VALUE = 5.0f;

        public static Color DEFAULT_HEALTHY_COLOR = new Color(33f / 255, 129f / 255, 25f / 255, 1);
        public static Color DEFAULT_DRY_COLOR = new Color(205f / 255, 188f / 255, 26f / 255, 1);

        private static GameObject _FoliageGameObject;
        internal static GameObject FoliageGameObject
        {
            get
            {
                if (_FoliageGameObject == null)
                {
                    _FoliageGameObject = Resources.Load<GameObject>("Foliage/DoubleSidedQuadCenterPivot");
                }

                return _FoliageGameObject;
            }
        }

        public static event OnFoliageEnableChanged OnFoliageEnabledStateChangedEvent;
        #endregion

        #region FoliageData
        public FoliageType FoliageType
        {
            get
            {
                return FoliageMesh != null ? FoliageType.Prefab : FoliageType.Texture;
            }
        }

        [SerializeField]
        GameObject _FoliageMesh;
        public GameObject FoliageMesh
        {
            get { return _FoliageMesh; }
            set
            {
                if (_FoliageMesh != value)
                {
                    _FoliageMesh = value;

                    GenerateInstantiatedMesh(healthyColor, dryColor);

                    FoliageMeshManager.RegenerateQueueInstances();
                    FoliageMeshManager.GenerateFoliageMeshInstances();
                }
            }
        }

        [SerializeField]
        Texture2D _FoliageTexture;
        public Texture2D FoliageTexture
        {
            get { return _FoliageTexture; }
            set
            {
                if(_FoliageTexture != value)
                {
                    _FoliageTexture = value;

                    GenerateInstantiatedMesh(healthyColor, dryColor);
                }
            }
        }

        /// <summary>
        /// Fast access to foliageID internally
        /// </summary>
        [SerializeField]
        internal byte _foliageID_INTERNAL;
        public byte id
        {
            get
            {
                return _foliageID_INTERNAL;
            }
            private set
            {
                _foliageID_INTERNAL = value;
            }
        }

        [SerializeField]
        float _spread = 1;
        public float spread
        {
            get
            {
                return _spread;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 2);

                if (value != _spread)
                {
                    _spread = value;
                    UpdateManagerInformation();
                }
            }
        }

        #region Size
        [SerializeField]
        float _minimumWidth = 1.5f;
        public float minimumWidth
        {
            get
            {
                return Mathf.Clamp(_minimumWidth, SIZE_MIN_VALUE, _maximumWidth);
            }
            set
            {
                value = Mathf.Clamp(value, SIZE_MIN_VALUE, _maximumWidth);

                if (value != _minimumWidth)
                {
                    _minimumWidth = value;

                    UpdateManagerInformation();
                }
            }
        }

        [SerializeField]
        float _maximumWidth = 2f;
        public float maximumWidth
        {
            get
            {
                return Mathf.Clamp(_maximumWidth, _minimumWidth, SIZE_MAX_VALUE);
            }
            set
            {
                value = Mathf.Clamp(value, _minimumWidth, SIZE_MAX_VALUE);

                if (value != _maximumWidth)
                {
                    _maximumWidth = value;

                    UpdateManagerInformation();
                }
            }
        }

        [SerializeField]
        float _minimumHeight = 1.5f;
        public float minimumHeight
        {
            get
            {
                return Mathf.Clamp(_minimumHeight, SIZE_MIN_VALUE, _maximumHeight);
            }
            set
            {
                value = Mathf.Clamp(value, SIZE_MIN_VALUE, _maximumHeight);

                if (value != _minimumHeight)
                {
                    _minimumHeight = value;

                    UpdateManagerInformation();
                }
            }
        }

        [SerializeField]
        float _maximumHeight = 2f;
        public float maximumHeight
        {
            get
            {
                return Mathf.Clamp(_maximumHeight, _minimumHeight, SIZE_MAX_VALUE);
            }
            set
            {
                value = Mathf.Clamp(value, _minimumHeight, SIZE_MAX_VALUE);

                if (value != _maximumHeight)
                {
                    _maximumHeight = value;

                    UpdateManagerInformation();
                }
            }
        }
        #endregion

        [SerializeField]
        bool _receiveShadows = true;
        public bool receiveShadows
        {
            get
            {
                return _receiveShadows;
            }
            set
            {
                _receiveShadows = value;
            }
        }

        [SerializeField]
        Color _dryColor;
        public Color dryColor
        {
            get
            {
                return _dryColor;
            }
            set
            {
                if (_dryColor != value)
                {
                    _dryColor = value;

                    UpdateManagerInformation();
                }
            }
        }

        [SerializeField]
        Color _healthyColor;
        public Color healthyColor
        {
            get
            {
                return _healthyColor;
            }
            set
            {
                if (_healthyColor != value)
                {
                    _healthyColor = value;

                    UpdateManagerInformation();
                }
            }
        }

        [SerializeField]
        private bool _castShadows;
        public bool castShadows
        {
            get
            {
                return _castShadows;
            }
            set
            {
                _castShadows = value;
            }
        }

        [SerializeField]
        private bool _useCustomFadeDistance;
        public bool useCustomFadeDistance
        {
            get
            {
                return _useCustomFadeDistance;
            }
            set
            {
                if (_useCustomFadeDistance != value)
                {
                    _useCustomFadeDistance = value;

                    UpdateManagerInformation();

                    if (FoliageCore_MainManager.instance != null)
                    {
                        FoliageMeshManager.GenerateFoliageMeshInstances();

                        FoliageMeshManager.RegenerateQueueInstances(); // regenerate queue instances.
                    }
                }
            }
        }

        [SerializeField]
        private int _fadeDistance = 100;
        public int fadeDistance
        {
            get
            {
                return useCustomFadeDistance || FoliageCore_MainManager.instance == null ? _fadeDistance : FoliageCore_MainManager.instance.globalFadeDistance;
            }
            set
            {
                if (_fadeDistance != value)
                {
                    _fadeDistance = value;

                    var tempUseFadeDistance = useCustomFadeDistance;
                    useCustomFadeDistance = true;

                    UpdateManagerInformation();

                    if (FoliageCore_MainManager.instance != null && (tempUseFadeDistance == useCustomFadeDistance)) // if FoliageCore_MainManager != null && custom fade distance didn't change.
                    {
                        FoliageMeshManager.GenerateFoliageMeshInstances();

                        FoliageMeshManager.RegenerateQueueInstances(); // regenerate queue instances.
                    }
                }
            }
        }

        [SerializeField]
        internal byte _maxGeneratedDensity = 10;
        public int maxGeneratedDensity
        {
            get
            {
                return Mathf.Clamp(_maxGeneratedDensity, 1, MAX_GENERATION_DENSITY);
            }
            set
            {
                value = Mathf.Clamp(value, 1, MAX_GENERATION_DENSITY);

                if (_maxGeneratedDensity != value)
                {
                    _maxGeneratedDensity = (byte)value;

                    _densitiesLODs = null;

                    UpdateManagerInformation();

                    if (enabled)
                    {
                        FoliageMeshManager.GenerateFoliageMeshInstances(id);
                        FoliageMeshManager.RegenerateQueueInstances();
                    }
                }
            }
        }
        
        [SerializeField]
        private bool _useInstancing = true;
        public bool useInstancing
        {
            get
            {
                // ONLY SUPPORTED IN VERSIONS ABOVE 5_5

                #if UNITY_5_5_OR_NEWER && !UNITY_2017_3
                return _useInstancing;
                #else
                _useInstancing = false;
                return _useInstancing;
                #endif
            }
            set
            {
                #if UNITY_5_5_OR_NEWER
                if(_useInstancing != value)
                {
                    _useInstancing = value;

                    UpdateKeywords();

                    if(FoliageCore_MainManager.instance != null)
                    {
                        FoliageMeshManager.GenerateFoliageMeshInstances();
                        FoliageMeshManager.RegenerateQueueInstances();
                    }
                }
                #else
                _useInstancing = false;
                #endif
            }
        }

        [SerializeField]
        bool _useColorMap;
        public bool useColorMap
        {
            get
            {
                return _useColorMap;
            }
            set
            {
                if (_useColorMap != value)
                {
                    _useColorMap = value;

                    FoliageInstancedMeshData.mat.SetFloat("_UseColorMap", value ? 1 : 0);
                }
            }
        }

        [SerializeField]
        bool _rotateNormals = false;
        public bool rotateNormals
        {
            get
            {
                return _rotateNormals;
            }
            set
            {
                if (_rotateNormals != value)
                {
                    _rotateNormals = value;
                    UpdateManagerInformation();
                }
            }
        }

        public string name
        {
            get
            {
                return FoliageType == FoliageType.Prefab ? FoliageMesh == null ? "None" : FoliageMesh.name : FoliageTexture == null ? "None" : FoliageTexture.name;
            }
        }

        [SerializeField]
        private bool _enabled = true;
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;

                    if (OnFoliageEnabledStateChangedEvent != null)
                    {
                        OnFoliageEnabledStateChangedEvent(this, value);
                    }

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(FoliageDB.instance);
                    }
#endif
                }
            }
        }

        private const int _meshLodsCount = 4;
        public int meshLodsCount
        {
            get
            {
                return Mathf.Clamp(_meshLodsCount, 1, maxGeneratedDensity);
            }
        }

        [SerializeField]
        private int _renderingLayer = 0;
        public int renderingLayer
        {
            get
            {
                return _renderingLayer;
            }
            set
            {
                _renderingLayer = value;
            }
        }

        [SerializeField]
        private bool _touchBendingEnabled = true;
        public bool touchBendingEnabled
        {
            get
            {
                return _touchBendingEnabled;
            }
            set
            {
                if (_touchBendingEnabled != value)
                {
                    _touchBendingEnabled = value;
                    FoliageInstancedMeshData.mat.SetFloat("touchBendingEnabled", value ? 1 : 0);
                }
            }
        }

        [SerializeField]
        private float _touchBendingStrength = 0.97f;
        public float touchBendingStrength
        {
            get
            {
                return _touchBendingStrength;
            }
            set
            {
                if (_touchBendingStrength != value)
                {
                    _touchBendingStrength = value;
                    FoliageInstancedMeshData.mat.SetFloat("touchBendingStrength", value);
                }
            }
        }

        [SerializeField]
        private float _cutOff = 0.3f;
        public float cutOff
        {
            get
            {
                return _cutOff;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);

                if (_cutOff != value)
                {
                    _cutOff = value;

                    FoliageInstancedMeshData.mat.SetFloat("_Cutoff", value);
                }
            }
        }

        public override bool isEnabled
        {
            get
            {
                return enabled;
            }
        }
        public override bool chooseableOnDisabled
        {
            get
            {
                return true;
            }
        }
#endregion

        #region WindSettings
        [SerializeField]
        private bool _useCustomWind;
        public bool useCustomWind
        {
            get
            {
                return _useCustomWind;
            }
            set
            {
                if (value != _useCustomWind)
                {
                    _useCustomWind = value;

                    FoliageDB.instance.UpdateShaderWindSettings();
                }
            }
        }

        public WindSettings customWindSettings = new WindSettings();
        #endregion

        #region LODs
        [SerializeField]
        bool _useLODs = true;
        public bool useLODs
        {
            get
            {
                return _useLODs;
            }
            set
            {
                if (_useLODs != value)
                {
                    _useLODs = value;

                    UpdateLODs();
                }
            }
        }

        [SerializeField]
        List<FoliageLODLevel> _lods = null;
        public List<FoliageLODLevel> lods
        {
            get
            {
                if (_lods == null)
                {
                    _lods = new List<FoliageLODLevel>(4);

                    _lods.Add(new FoliageLODLevel(20, 1f));
                    _lods.Add(new FoliageLODLevel(40, 0.6f));
                    _lods.Add(new FoliageLODLevel(60, 0.4f));
                    _lods.Add(new FoliageLODLevel(100, 0.2f));
                }

                return _lods;
            }
            set
            {
                _lods = value;

                bool changed = false;
                for (int i = 0; i < _lods.Count; i++)
                {
                    if (_lods[i].isDirty)
                    {
                        changed = true;
                        _lods[i].isDirty = false;
                    }
                }

                if (changed)
                {
                    UpdateLODs();
                }
            }
        }

        public int maxFoliageCapability
        {
            get
            {
                return Mathf.FloorToInt(FoliageMeshInstance.GENERATION_VERTICES_MAX / FoliageInstancedMeshData.vertexCount);
            }
        }
#endregion

        #region Instance
        [SerializeField]
        FoliageMesh _FoliageInstancedMeshData;
        public FoliageMesh FoliageInstancedMeshData
        {
            get
            {
                return _FoliageInstancedMeshData;
            }
        }

        public Vector3 instancedEuler;

        [System.NonSerialized]
        private List<byte> _densitiesLODs = null;
        public List<byte> densitiesLODs
        {
            get
            {
                if (_densitiesLODs == null)
                {
                    _densitiesLODs = new List<byte>();

                    int meshLodsCount = Mathf.Clamp(this.meshLodsCount, 1, maxGeneratedDensity);

                    for (int i = 0; i < meshLodsCount; i++)
                    {
                        _densitiesLODs.Add((byte)Mathf.FloorToInt((float)maxGeneratedDensity / (i + 1)));
                    }
                }

                return _densitiesLODs;
            }
        }
#endregion

        private FoliagePrototype(Texture2D texture, GameObject prefab, float minWidth, float minHeight, float maxWidth, float maxHeight, float spread, int layer, byte id)
        {
            _FoliageMesh = prefab;
            _FoliageTexture = texture;

            this.id = id;

            _spread = spread;

            _renderingLayer = layer;

            _minimumWidth = minWidth;
            _maximumWidth = maxWidth;

            _minimumHeight = minHeight;
            _maximumHeight = maxHeight;
        }

        /// <summary>
        /// Remove current material
        /// </summary>
        internal void DisposeOfCurrentMaterial()
        {
            if (_FoliageInstancedMeshData == null || _FoliageInstancedMeshData.mat == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(FoliageDB.instance);
            }

            if(FoliageInstancedMeshData.mat != null)
            {
                UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(FoliageInstancedMeshData.mat));
            }

            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public bool EqualsToPrototype(DetailPrototype detail)
        {
            return detail.prototype == this.FoliageMesh && detail.prototypeTexture == FoliageTexture;
        }

        public int GetRealPrototypeFadeDistance()
        {
            return _fadeDistance;
        }

        internal void GenerateInstantiatedMesh(Color healthyColor, Color dryColor)
        {
            base.preview = null;

            DisposeOfCurrentMaterial();

            GameObject instance;
            Vector3 offset;

            if (FoliageType == FoliageType.Prefab)
            {
                instance = FoliageMesh;
                offset = Vector3.zero;
            }
            else
            {
                instance = FoliageGameObject;
                instance.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", FoliageTexture);

                offset = new Vector3(0, 0.5f * instance.transform.localScale.y, 0); // assign offset.
            }

            _FoliageInstancedMeshData = new FoliageMesh(instance, id, name, offset);

            if (FoliageType == FoliageType.Prefab)
            {
                maxGeneratedDensity = FoliageInstancedMeshData.MeshInstancesLimiter_Optimization_Clamp;
            }

            this.healthyColor = healthyColor;
            this.dryColor = dryColor;

            _densitiesLODs = null;

            if(FoliageType == FoliageType.Prefab)
            {
                CopyLODsFromExistingLODGroup();
            }

            // Apply lods. & update keywords
            UpdateLODs();
            UpdateManagerInformation();
            UpdateKeywords();
        }

        /// <summary>
        /// Copy lods from existing lod group
        /// </summary>
        public void CopyLODsFromExistingLODGroup()
        {
            if (FoliageMesh == null) return;

            var lodGroup = FoliageMesh.GetComponent<LODGroup>();

            if (lodGroup == null) return;

            var lodGroupLODs = lodGroup.GetLODs();

            if(lodGroupLODs.Length > 4)
            {
                Debug.LogWarning("Cant have more than 4 lods, uNature will copy the first 4 lods instead.");
            }

            lods.Clear();

            FoliageLODLevel lod;

            for(int i = 0; i < 4 && i < lodGroupLODs.Length; i++)
            {
                lod = FoliageLODLevel.CreateInstanceFromLODGroupLOD(lodGroupLODs[i], i);

                if (lod == null) continue;

                lods.Add(lod);
            }
        }

        public List<UNFoliageMeshData> GetLODsData()
        {
            List<UNFoliageMeshData> lodsData = new List<UNFoliageMeshData>();

            if(!useLODs || FoliageType == FoliageType.Texture)
            {
                lodsData.Add(new UNFoliageMeshData(FoliageInstancedMeshData.mesh));
                return lodsData;
            }

            FoliageLODLevel lod;
            Mesh currentMesh;

            UNFoliageMeshData lastMeshData = new UNFoliageMeshData();

            for (int i = 0; i < lods.Count; i++)
            {
                lod = lods[i];

                if (lod == null) continue;

                currentMesh = lod.GetMeshLOD(i, this);

                if (currentMesh == null)
                {
                    lodsData.Add(lastMeshData); // if no mesh found, use the last mesh.
                }
                else
                {
                    lastMeshData = new UNFoliageMeshData(currentMesh);
                    lodsData.Add(lastMeshData);
                }
            }

            return lodsData;
        }

        public void UpdateLODs()
        {
            FoliageInstancedMeshData.mat.SetFloat("lods_Enabled", useLODs ? 1 : 0);

            FoliageLODLevel lod;

            // reset the lods before assigning to make sure that deleted lods arent registered.
            for(int i = 0; i < FoliageLODLevel.LOD_MAX_AMOUNT; i++)
            {
                FoliageInstancedMeshData.mat.SetFloat("lod" + i + "_Distance", -1);
            }

            // apply the lods before assigning to make sure that deleted lods arent registered.
            for (int i = 0; i < lods.Count; i++)
            {
                lod = lods[i];

                lod.LOD_Calculated_Distance = (fadeDistance * (float)lod.LOD_Coverage_Percentage) / 100f;

                FoliageInstancedMeshData.mat.SetFloat("lod" + i + "_Distance", lod.LOD_Calculated_Distance);
                FoliageInstancedMeshData.mat.SetFloat("lod" + i + "_Value", lod.LOD_Value_Multiplier);

                lod.isDirty = false;
            }

            FoliageMeshManager.RegenerateQueueInstances();
        }

        /// <summary>
        /// Get Preview
        /// </summary>
        /// <returns></returns>
        protected override Texture2D GetPreview()
        {
#if UNITY_EDITOR
            if (FoliageType == FoliageType.Prefab)
            {
                return FoliageMesh == null ? null : UnityEditor.AssetPreview.GetAssetPreview(FoliageMesh);
            }
            else if (FoliageType == FoliageType.Texture)
            {
                return FoliageTexture;
            }
#endif

            return null;
        }

        /// <summary>
        /// Apply the wind parameters to this Foliage prototype.
        /// </summary>
        public void ApplyWind()
        {
            WindSettings targetedSettings = useCustomWind ? customWindSettings : FoliageDB.instance.globalWindSettings;

            FoliageInstancedMeshData.mat.SetFloat("_WindSpeed", targetedSettings.windSpeed);
            FoliageInstancedMeshData.mat.SetFloat("_WindBending", targetedSettings.windBending);
        }

        /// <summary>
        /// Apply color map
        /// 
        /// Res = area size.
        /// </summary>
        public void ApplyColorMap(Texture2D map, Texture2D normalMap)
        {
            FoliageInstancedMeshData.mat.SetTexture("_ColorMap", map);
            FoliageInstancedMeshData.mat.SetTexture("_WorldMap", normalMap);
        }

        /// <summary>
        /// Apply color map
        /// 
        /// Res = area size.
        /// </summary>
        public void ApplyGrassMap(Texture2D map)
        {
            FoliageInstancedMeshData.mat.SetTexture("_GrassMap", map);
        }

        /// <summary>
        /// Update keywords.
        /// Like:
        /// 
        /// Use Instancing
        /// Advanced shader
        /// Mobile Shader
        /// 
        /// etc
        /// </summary>
        public void UpdateKeywords()
        {
#region Instancing
            if (useInstancing)
            {
                FoliageInstancedMeshData.mat.EnableKeyword("INSTANCING_ON");
            }
            else
            {
                FoliageInstancedMeshData.mat.DisableKeyword("INSTANCING_ON");
            }

#if UNITY_5_6_OR_NEWER
            FoliageInstancedMeshData.mat.enableInstancing = useInstancing;
#endif

#endregion
        }

        /// <summary>
        /// Update the global spread noise.
        /// </summary>
        public void UpdateManagerInformation()
        {
            if (FoliageCore_MainManager.instance == null) return;

            FoliageInstancedMeshData.mat.SetFloat("_DensityMultiplier", FoliageCore_MainManager.instance.density);
            FoliageInstancedMeshData.mat.SetFloat("_NoiseMultiplier", spread);

            FoliageInstancedMeshData.mat.SetFloat("_MinimumWidth", minimumWidth);
            FoliageInstancedMeshData.mat.SetFloat("_MaximumWidth", maximumWidth);

            FoliageInstancedMeshData.mat.SetFloat("_MinimumHeight", minimumHeight);
            FoliageInstancedMeshData.mat.SetFloat("_MaximumHeight", maximumHeight);

            FoliageInstancedMeshData.mat.SetFloat("_RotateNormals", rotateNormals ? 1 : 0);

            FoliageInstancedMeshData.mat.SetFloat("_FoliageAreaSize", FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE);

            FoliageInstancedMeshData.mat.SetFloat("fadeDistance", fadeDistance);
            FoliageInstancedMeshData.mat.SetFloat("MaxDensity", maxGeneratedDensity);

            FoliageInstancedMeshData.mat.SetColor("_dryColor", _dryColor * FoliageCore_MainManager.instance.foliageGlobalTint);
            FoliageInstancedMeshData.mat.SetColor("_healthyColor", _healthyColor * FoliageCore_MainManager.instance.foliageGlobalTint);

            UpdateLODs();
        }

        /// <summary>
        /// Create a prototype.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="prefab"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <param name="spread"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FoliagePrototype CreatePrototype(Texture2D texture, GameObject prefab, float minWidth, float minHeight, float maxWidth, float maxHeight, float spread, int layer, byte id, Color healthyColor, Color dryColor)
        {
            FoliagePrototype prototype = new FoliagePrototype(texture, prefab, minWidth, minHeight, maxWidth, maxHeight, spread, layer, id);

            FoliageDB.instance.RegisterNewPrototype(prototype);

            prototype.GenerateInstantiatedMesh(healthyColor, dryColor);

            prototype.instancedEuler = prototype.FoliageInstancedMeshData.eulerAngles;
            prototype.UpdateManagerInformation();

            FoliageCore_MainManager.WarmUpGrassMaps();

            return prototype;
        }

        public float GetLODMultiplier(float distance, ref int index)
        {
            if (!useLODs) return 1;

            FoliageLODLevel lod;

            for (int i = 0; i < lods.Count; i++)
            {
                lod = lods[i];

                if (distance <= lod.LOD_Calculated_Distance)
                {
                    index = i;
                    return lod.LOD_Value_Multiplier;
                }
            }

            index = 0;
            return 1;
        }
    }

    public enum FoliageType
    {
        Prefab,
        Texture
    }

    public enum ShaderType
    {
        NaN = 0,
        Basic = 1,
        Advanced = 2,
        Custom = 3
    }

    [System.Serializable]
    public class FoliageMesh
    {
        public const int OPTIMIZATION_MESH_INSTANCES_DENSITIES_LIMITER = 6;
        public Mesh mesh;

        public Material mat;

        public Vector3 eulerAngles;

        [SerializeField]
        private Vector3 _rendererScale;
        public Vector3 rendererScale
        {
            get
            {
                return _rendererScale;
            }
        }

        [SerializeField]
        private Vector3 _worldScale = Vector3.zero;
        public Vector3 worldScale
        {
            get
            {
                if (_worldScale == Vector3.zero)
                {
                    _worldScale = rendererScale;

                    _worldScale.Scale(scale);
                }

                return _worldScale;
            }
        }

        public Vector3 scale = Vector3.one;

        public Vector3 offset;

        public int vertexCount;

        public ShaderType shaderType
        {
            get
            {
                if (mat != null)
                {
                    string shader = mat.shader.name;

                    return shader == FoliagePrototype.SHADER_BASIC_NAME ? ShaderType.Basic : shader == FoliagePrototype.SHADER_ADVANCED_NAME ? ShaderType.Advanced : ShaderType.Custom;
                }

                return ShaderType.NaN;
            }
        }

        [SerializeField]
        private int meshInstancesLimiter_Optimization_Clamp = FoliagePrototype.MAX_GENERATION_DENSITY;
        public int MeshInstancesLimiter_Optimization_Clamp
        {
            get
            {
                return meshInstancesLimiter_Optimization_Clamp;
            }
        }

        public FoliageMesh(GameObject go, int layer, string name, Vector3 offset)
        {
            var filters = go.GetComponentsInChildren<MeshFilter>(true);

            if (filters.Length == 0) return;

            this.offset = offset;

            go.transform.position = Vector3.zero + offset;
            eulerAngles = go.transform.eulerAngles;
            scale = go.transform.localScale;

            CalculateRendererScale(go);

            MeshFilter filter = filters[0]; // take first mesh

            mesh = filter.sharedMesh;
            vertexCount = mesh.vertexCount;

            Material matInstance = GameObject.Instantiate<Material>(go.GetComponentsInChildren<MeshRenderer>(true)[0].sharedMaterial);
            matInstance.name = "Foliage Material";

            mat = matInstance;

            if (vertexCount > OPTIMIZATION_MESH_INSTANCES_DENSITIES_LIMITER)
            {
                int differences = Mathf.Clamp((int)((float)vertexCount / OPTIMIZATION_MESH_INSTANCES_DENSITIES_LIMITER), 0, FoliagePrototype.MAX_GENERATION_DENSITY - 1);

                meshInstancesLimiter_Optimization_Clamp = FoliagePrototype.MAX_GENERATION_DENSITY - differences;
            }

#if UNITY_EDITOR
            string materialsPath = UNSettings.ProjectPath + "Resources/Foliage/Materials/";

            UnityEditor.AssetDatabase.CreateAsset(mat, string.Format(materialsPath + "{0}_{1}_{2}.mat", mat.name, name, layer));
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        private void CalculateRendererScale(GameObject obj)
        {
            Bounds bounds = new Bounds();

            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (i == 0)
                {
                    bounds = renderers[i].bounds;
                    continue;
                }

                bounds.Encapsulate(renderers[i].bounds);
            }

            _rendererScale = bounds.size;
        }
    }

    [System.Serializable]
    public class FoliageLODLevel
    {
        public static Color32[] lodGUIColors = new Color32[4]
        {
            new Color32(32, 178, 170, 255),
            new Color32(240, 128, 128, 255),
            new Color32(152, 251, 152, 255),
            new Color32(210, 105, 30, 255)
        };

        public static Color32[] lodGUIColors_overlay = new Color32[4]
        {
            new Color32(245, 245, 220, 125),
            new Color32(245, 245, 220, 125),
            new Color32(245, 245, 220, 125),
            new Color32(245, 245, 220, 125),
        };

        public const int LOD_MAX_DISTANCE = 500;
        public const int LOD_MAX_AMOUNT = 4;

        [SerializeField]
        private float lod_calculated_distance;
        public float LOD_Calculated_Distance
        {
            get
            {
                return lod_calculated_distance;
            }
            set
            {
                lod_calculated_distance = value;
            }
        }

        [SerializeField]
        private float lod_value_multiplier;
        public float LOD_Value_Multiplier
        {
            get
            {
                return lod_value_multiplier;
            }
            set
            {
                if(lod_value_multiplier != value)
                {
                    lod_value_multiplier = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        private int lod_coverage_percentage;
        public int LOD_Coverage_Percentage
        {
            get
            {
                return lod_coverage_percentage;
            }
            set
            {
                if(lod_coverage_percentage != value)
                {
                    lod_coverage_percentage = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        private Mesh LOD_Mesh;

        public Mesh GetMeshLOD(int lodID, FoliagePrototype prototype)
        {
            if (lodID == 0) // first lod
                return prototype.FoliageInstancedMeshData.mesh;
            else
                return LOD_Mesh;
        }
        public void SetMeshLOD(Mesh mesh, FoliageLODLevel firstLOD)
        {
            if (LOD_Mesh != mesh)
            {
                if(firstLOD != null && firstLOD.LOD_Mesh != null && mesh != null && mesh.vertexCount > firstLOD.LOD_Mesh.vertexCount)
                {
                    Debug.LogError("Can't add lod mesh that has a higher vertex count than the original!!");
                    return;
                }

                LOD_Mesh = mesh;

                if (FoliageCore_MainManager.instance != null)
                {
                    FoliageMeshManager.GenerateFoliageMeshInstances();
                    FoliageMeshManager.RegenerateQueueInstances();
                }
            }
        }

        internal bool isDirty;

        public FoliageLODLevel(int coverage, float valueMultiplier) : this(coverage, valueMultiplier, null)
        {
        }

        public FoliageLODLevel(int coverage, float valueMultiplier, Mesh lod_mesh)
        {
            LOD_Coverage_Percentage = coverage;
            LOD_Value_Multiplier    = valueMultiplier;
            LOD_Mesh = lod_mesh;
        }

        public static FoliageLODLevel CreateInstanceFromLODGroupLOD(LOD lod, int lodIndex)
        {
            if (lod.renderers.Length == 0) return null;

            var mFilter = lod.renderers[0].GetComponent<MeshFilter>();
            Mesh mesh = null;

            if (mFilter == null)
            {
                BillboardRenderer billboard = lod.renderers[0].GetComponent<BillboardRenderer>();

                if(billboard != null)
                {
                    mFilter = FoliagePrototype.FoliageGameObject.GetComponent<MeshFilter>();

                    if (mFilter == null) return null;

                    mesh = mFilter.sharedMesh;
                }
            }
            else
            {
                if (mFilter.sharedMesh == null) return null;
                mesh = mFilter.sharedMesh;
            }

            if (mesh == null) return null;

            int coverage = (int)((1 - lod.screenRelativeTransitionHeight) * 100);
            float valueMultiplier = 1f / (lodIndex + 1); // add 1 to avoid division by 0.

            return new FoliageLODLevel(coverage, valueMultiplier, mesh);
        }
    }

    [System.Serializable]
    public struct UNFoliageMeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;

        public int verticesLength;
        public int normalsLength;
        public int trianglesLength;
        public int uvLength;

        public UNFoliageMeshData(Mesh mesh)
        {
            vertices            = mesh.vertices;
            triangles           = mesh.triangles;
            uv                  = mesh.uv;

            verticesLength      = vertices.Length;
            trianglesLength     = triangles.Length;
            uvLength            = uv.Length;
            normalsLength       = mesh.normals.Length;
        }
    }
}
