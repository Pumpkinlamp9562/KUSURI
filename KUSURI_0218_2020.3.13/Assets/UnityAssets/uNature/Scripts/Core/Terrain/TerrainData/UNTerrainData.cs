using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Settings;
using uNature.Core.Utility;
using uNature.Core.Threading;
using uNature.Core.FoliageClasses;

using System.IO;
using System;

using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uNature.Core.Terrains
{
    /// <summary>
    /// The terrain data class which is used by uNature.
    /// Can be accesed by "UNTerrain.terrainData".
    /// </summary>
    [System.Serializable]
    public class UNTerrainData
    {
        public static string GetBackUpPath(string backUpName)
        {
            return UNSettings.ProjectPath + "Resources/Backups/" + backUpName + ".asset";
        }

        #region Variables
        [SerializeField]
        TerrainData _terrainData;
        /// <summary>
        /// The terrain data which this object resembles
        /// </summary>
        public TerrainData terrainData
        {
            get { return _terrainData; }
            protected set
            {
                if (value != _terrainData)
                {
                    _terrainData = value;

                    CheckForTreePrototypesChange();
                }
            }
        }

        [SerializeField]
        TerrainData _backedUpTerrainData;
        public TerrainData backedUpTerrainData
        {
            get
            {
                return _backedUpTerrainData;
            }
        }

        /// <summary>
        /// hold all of the custom tree prototypes of the terrain data.
        /// </summary>
        [SerializeField]
        List<UNTreePrototype> _treePrototypes;
        public List<UNTreePrototype> treePrototypes
        {
            get
            {
                if (_treePrototypes == null)
                {
                    _treePrototypes = new List<UNTreePrototype>();
                }

                if (UNThreadManager.inUnityThread)
                {
                    FetchTreePrototypes();
                }

                return _treePrototypes;
            }
        }

        int activeTreePrototypes
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _treePrototypes.Count; i++)
                {
                    if (!_treePrototypes[i].isMissing) continue;

                    count++;
                }

                return count;
            }
        }

        private float[,] _heights;

        private bool loadedIndexes = false;

        /// <summary>
        /// Get the heights of the terrain (used in multi-threading).
        /// </summary>
        public float[,] heights
        {
            get
            {
                return _heights;
            }
        }

        public Vector2 FoliageRelativeMultiplier;

        public bool isDirty
        {
            get
            {
                if (terrainData == null || terrainData.treePrototypes == null || _treePrototypes == null) return false;

                for (int i = 0; i < _treePrototypes.Count; i++)
                {
                    for (int b = 0; b < terrainData.treePrototypes.Length; b++)
                    {
                        if (!_treePrototypes[i].Equals(terrainData.treePrototypes[b]))
                        {
                            return true;
                        }
                    }
                }

                return terrainData.treePrototypes.Length != _treePrototypes.Count;
            }
        }
        #endregion

        #region TerrainData - Multi-threading.
        [System.NonSerialized]
        private Vector3 _multiThreaded_terrainDataSize;
        public Vector3 multiThreaded_terrainDataSize
        {
            get
            {
                if (_multiThreaded_terrainDataSize == Vector3.zero)
                {
                    this.UpdateMultithreadedVariables();
                }

                return _multiThreaded_terrainDataSize;
            }
            set
            {
                _multiThreaded_terrainDataSize = value;
            }
        }
        [System.NonSerialized]
        public int multiThreaded_detailResolution;

        [System.NonSerialized]
        public int multiThreaded_detailWidth;
        [System.NonSerialized]
        public int multiThreaded_detailHeight;

        [System.NonSerialized]
        private float[,] _multiThreaded_terrainHeights = null;
        public float[,] multiThreaded_terrainHeights
        {
            get
            {
                if(_multiThreaded_terrainHeights == null)
                {
                    _multiThreaded_terrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
                }

                return _multiThreaded_terrainHeights;
            }
            set
            {
                _multiThreaded_terrainHeights = value;
            }
        }

        [System.NonSerialized]
        public Vector3[,] multiThreaded_terrainSampleNormals;

        [System.NonSerialized]
        public DetailPrototype[] multiThreaded_detailPrototypes;

        [System.NonSerialized]
        public float multiThreaded_heightMapWidth;

        [System.NonSerialized]
        public float multiThreaded_heightMapHeight;
        private object unPrototype;
        #endregion

        #region Methods
        /// <summary>
        /// This method will pull the tree prototypes from the provided terrain data.
        /// </summary>
        void FetchTreePrototypes()
        {
            if (terrainData == null || terrainData.treePrototypes == null) return;

            if (!loadedIndexes)
            {

            }

            if (_treePrototypes == null)
            {
                _treePrototypes = new List<UNTreePrototype>();
            }

            if (isDirty)
            {
                CheckForTreePrototypesChange();
            }

            UNTreePrototype.CheckForMissings(_treePrototypes, terrainData.treePrototypes);
        }

        private void RefreshIndexes()
        {
            if (_treePrototypes == null) return;

            loadedIndexes = true;

            for (int i = 0; i < _treePrototypes.Count; i++)
            {
                for (int b = 0; b < terrainData.treePrototypes.Length; b++)
                {
                    if (_treePrototypes[i].prototypeObject == terrainData.treePrototypes[b].prefab)
                    {
                        _treePrototypes[i].prototypeIndex = b;
                    }
                }
            }
        }

        /// <summary>
        /// Checks and updates dirty terrain tree prototypes.
        /// </summary>
        protected virtual void CheckForTreePrototypesChange()
        {
            UNTreePrototype prototype;

            TreePrototype dirtyPrototype = null;
            bool isAnyPrototypeDirty = false;

            if (_treePrototypes == null || _treePrototypes.Count == 0) // if we dont have any tree prototypes created, simply fetch all of the tree prototypes from the terrain.
            {
                _treePrototypes = new List<UNTreePrototype>();

                for (int i = 0; i < terrainData.treePrototypes.Length; i++)
                {
                    _treePrototypes.Add(new UNTreePrototype(terrainData.treePrototypes[i]));
                }

                RefreshIndexes();

                SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreePrototypesChanged); // send prototypes changed event
                SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreeInstances); // send prototypes changed event

                return;
            }

            for (int i = 0; i < _treePrototypes.Count; i++)
            {
                prototype = _treePrototypes[i];

                if(prototype.prototypeIndex >= terrainData.treePrototypes.Length)
                {
                    _treePrototypes.Remove(prototype);

                    SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreePrototypesChanged); // send prototypes changed event
                    SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreeInstances); // send prototypes changed event

                    return;
                }

                dirtyPrototype = terrainData.treePrototypes[prototype.prototypeIndex];

                if (dirtyPrototype.prefab != prototype.prototypeObject)
                {
                    prototype.prototypeObject = dirtyPrototype.prefab;

                    #if UNITY_EDITOR
                    Settings.UNSettings.Log("Tree Prototype Updated : " + dirtyPrototype.prefab);
                    #endif

                    prototype.preview = null;
                    prototype.isMissing = false;

                    isAnyPrototypeDirty = true;
                }
            }

            for (int i = 0; i < terrainData.treePrototypes.Length; i++)
            {
                for (int b = 0; b < _treePrototypes.Count; b++)
                {
                    if (_treePrototypes[b].prototypeObject == terrainData.treePrototypes[i].prefab) break;

                    if (b == _treePrototypes.Count - 1)
                    {
                        _treePrototypes.Add(new UNTreePrototype(terrainData.treePrototypes[i]));
                        RefreshIndexes();

                        isAnyPrototypeDirty = true;

                        break;
                    }
                }
            }

            if (isAnyPrototypeDirty)
            {
                SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreePrototypesChanged); // send prototypes changed event
                SendUpdateEventToLinkedTerrains(TerrainChangedFlags.TreeInstances); // send prototypes changed event
            }
        }

        /// <summary>
        /// Send an TerrainData changed event to all linked terrains.
        /// </summary>
        protected void SendUpdateEventToLinkedTerrains(TerrainChangedFlags flag)
        {
            UNTerrain[] terrains = GameObject.FindObjectsOfType<UNTerrain>();
            UNTerrain terrain;

            for(int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];

                if(terrain.terrainData == this)
                {
                    terrain.SendMessage("OnTerrainChanged", (int)flag);
                }
            }
        }

        internal bool ContainsInPrototypes(UNTreePrototype prototype, List<TreePrototype> prototypes)
        {
            for(int i = 0; i < prototypes.Count; i++)
            {
                if (prototypes[i].prefab == prototype.prototypeObject)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get a terrain tree prototype (UNTerrainData) from a unity's tree prototype.
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public UNTreePrototype GetPrototype(TreePrototype prototype)
        {
            for(int i = 0; i < treePrototypes.Count; i++)
            {
                if (treePrototypes[i].Equals(prototype))
                    return treePrototypes[i];
            }

            return null;
        }

        /// <summary>
        /// Get the UNTerrainData instance from providing a terrainData.
        /// </summary>
        /// <param name="terrainData">the terrain data which this UNTerrainData belongs to.</param>
        /// <returns>the UNTerrainData instance for the provided terrain data.</returns>
        public static UNTerrainData GetInstance(TerrainData terrainData)
        {
            var instance = new UNTerrainData();
            instance._terrainData = terrainData;

            return instance;
        }

        /// <summary>
        /// Remove all current details that are placed on the terrian
        /// </summary>
        public void ClearDetails()
        {
            if (terrainData.detailPrototypes.Length > 0)
            {
                int size = terrainData.detailWidth;

                int[,] details = terrainData.GetDetailLayer(0, 0, size, size, 0);

                for (int x = 0; x < size; x++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        details[z, x] = 0;
                    }
                }

                for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
                {
                    terrainData.SetDetailLayer(0, 0, i, details);
                }
            }
        }

        /// <summary>
        /// Update the current multi-threaded variables on this terrain so it can be used on a different thread.
        /// </summary>
        public void UpdateMultithreadedVariables()
        {
            multiThreaded_terrainDataSize = terrainData.size;
            multiThreaded_detailResolution = terrainData.detailResolution;

            multiThreaded_detailWidth = terrainData.detailWidth;
            multiThreaded_detailHeight = terrainData.detailHeight;

            multiThreaded_detailPrototypes = terrainData.detailPrototypes;

            multiThreaded_heightMapWidth = terrainData.heightmapResolution;
            multiThreaded_heightMapHeight = terrainData.heightmapResolution;

            FoliageRelativeMultiplier.x = (multiThreaded_terrainDataSize.x / multiThreaded_detailWidth);
            FoliageRelativeMultiplier.y = (multiThreaded_terrainDataSize.z / multiThreaded_detailHeight);

            if (_heights == null)
            {
                _heights = new float[(int)terrainData.size.x, (int)terrainData.size.z];

                for (int x = 0; x < (int)terrainData.size.x; x++)
                {
                    for (int z = 0; z < (int)terrainData.size.z; z++)
                    {
                        _heights[x, z] = terrainData.GetInterpolatedHeight(x / terrainData.size.x, z / terrainData.size.z);
                    }
                }
            }
        }

        /// <summary>
        /// Backup the terrain data.
        /// </summary>
        public void Backup()
        {
            if (FoliageCore_MainManager.instance == null) return;

            #if UNITY_EDITOR
            _backedUpTerrainData = GameObject.Instantiate<TerrainData>(terrainData);

            string path = GetBackUpPath("BACKUP_" + FoliageCore_MainManager.instance.guid + "-" + terrainData.name);

            AssetDatabase.CreateAsset(backedUpTerrainData, path);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
            #endif
        }

        /// <summary>
        /// Apply the current backup.
        /// </summary>
        public void ApplyBackup(Terrain terrain)
        {
            if (backedUpTerrainData == null) return;

            terrain.terrainData.detailPrototypes = backedUpTerrainData.detailPrototypes;
            terrain.terrainData.SetDetailResolution(backedUpTerrainData.detailResolution, 8);

            for (int i = 0; i < terrain.terrainData.detailPrototypes.Length; i++)
            {
                terrain.terrainData.SetDetailLayer(0, 0, i, backedUpTerrainData.GetDetailLayer(0, 0, backedUpTerrainData.detailWidth, backedUpTerrainData.detailHeight, i));
            }
        }

        /// <summary>
        /// Delete the current backup.
        /// </summary>
        public void DeleteBackup()
        {
            if (backedUpTerrainData == null) return;

            #if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(_backedUpTerrainData);

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            _backedUpTerrainData = null;
            #endif
        }

        /// <summary>
        /// Initialize the terrain data.
        /// </summary>
        public void Initialize()
        {
            UpdateMultithreadedVariables();
        }
        #endregion
    }

    [System.Serializable]
    /// <summary>
    /// A custom class for the normal tree prototypes.
    /// Holds custom data that is used over this certain terrain data.
    /// </summary>
    public class UNTreePrototype : BasePrototypeItem
    {
        #region SerializationVariables
        /// <summary>
        /// the game object of the tree prototype.
        /// </summary>
        public GameObject prototypeObject;

        /// <summary>
        /// Is this prototype missing on the terrainData?
        /// if so, make sure to wait for it to "comeback"
        /// and meanwhile store its data.
        /// </summary>
        public bool isMissing = false;

        /// <summary>
        /// Is this prototype missing?
        /// </summary>
        public override bool isEnabled
        {
            get
            {
                return !isMissing;
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// Is this prototype enabled ? 
        /// </summary>
        [SerializeField]
        bool _enabled = true;
        public bool enabled
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

        /// <summary>
        /// Create this item on the Pool even if not used currently on the terrain.
        /// </summary>
        [SerializeField]
        bool _forcePoolCreation = false;
        public bool forcePoolCreation
        {
            get { return _forcePoolCreation; }
            set { _forcePoolCreation = value; }
        }

        [SerializeField]
        bool _ignoreTriggerColliders = false;
        public bool ignoreTriggetColliders
        {
            get
            {
                return _ignoreTriggerColliders;
            }
            set
            {
                _ignoreTriggerColliders = value;
            }
        }

        public int prototypeIndex;
        #endregion

        /// <summary>
        /// Create a new instance of the object
        /// </summary>
        /// <param name="prototype">The tree prototype this instance is based on.</param>
        public UNTreePrototype(TreePrototype prototype)
        {
            this.prototypeObject = prototype.prefab;
            this.isMissing = false;
        }

        /// <summary>
        /// Custom equals operator to take into account treePrototypes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return prototypeObject == null;

            System.Type instanceType = obj.GetType();
            bool prototypeType = instanceType == typeof(TreePrototype);

            if(prototypeType)
            {
                TreePrototype instance = obj as TreePrototype;

                if (instance == null) return false;

                return instance.prefab == this.prototypeObject;
            }
            else
            {
                UNTreePrototype instance = obj as UNTreePrototype;

                if (instance == null) return false;

                return this.prototypeObject == instance.prototypeObject;
            }
        }

        /// <summary>
        /// Override to avoid warnings.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        /// <summary>
        /// Get Item Preview
        /// </summary>
        /// <returns></returns>
        protected override Texture2D GetPreview()
        {
            #if UNITY_EDITOR
            return prototypeObject == null ? null : UnityEditor.AssetPreview.GetAssetPreview(prototypeObject);
            #else
            return null;
            #endif
        }

        /// <summary>
        /// This method will check whether any of the items is missing.
        /// </summary>
        /// <param name="items">the items list</param>
        /// <param name="prototypes">the tree prototypes of the terrain data</param>
        public static void CheckForMissings(List<UNTreePrototype> items, TreePrototype[] prototypes)
        {
            UNTreePrototype item;
            for (int i = 0; i < items.Count; i++)
            {
                item = items[i];
                item.isMissing = true;

                for(int b = 0; b < prototypes.Length; b++)
                {
                    if(prototypes[b].prefab == item.prototypeObject)
                    {
                        item.isMissing = false;
                        break;
                    }
                }
            }
        }
    }
}
 