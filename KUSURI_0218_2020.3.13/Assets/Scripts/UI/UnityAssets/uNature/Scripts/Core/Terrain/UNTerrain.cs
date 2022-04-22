using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using uNature.Core.Targets;
using uNature.Core.Sectors;
using uNature.Core.Pooling;
using uNature.Core.ClassExtensions;
using uNature.Core.Threading;
using uNature.Core.Settings;
using uNature.Core.FoliageClasses;
using uNature.Core.Utility;

namespace uNature.Core.Terrains
{
    /// <summary>
    /// A class that needs to be on each terrain that needs to be taken into account when managing the system.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public class UNTerrain : UNTarget
    {
        /// <summary>
        /// The height which "destroyed" tree instances will get.
        /// Don't change if not needed.
        /// </summary>
        public const float removedTreeInstanceHeight = 0f;

        /// <summary>
        /// The height which "destroyed" tree instances will get.
        /// Don't change if not needed.
        /// </summary>
        public const int collidersPoolItemInstanceIncrease = 10000;

        /// <summary>
        /// All of the current existing terrains in the current scene.
        /// </summary>
        public static List<UNTerrain> terrains = new List<UNTerrain>();

        #region TerrainVariables
        [SerializeField]
        Terrain _terrain;
        public Terrain terrain
        {
            get
            {
                if(_terrain == null)
                {
                    _terrain = GetComponent<Terrain>();
                    tData = _terrain.terrainData;
                }

                return _terrain;
            }
            set
            {
                _terrain = value;
                tData = value.terrainData;
            }
        }
        [SerializeField]
        TerrainData tData;

        [SerializeField]
        UNTerrainData _terrainData;
        public UNTerrainData terrainData
        {
            get
            {
                if(_terrainData == null)
                {
                    _terrainData = UNTerrainData.GetInstance(terrain.terrainData);
                }

                return _terrainData;
            }
        }

        /// <summary>
        /// By how much will the terrain distance be still considered?
        /// </summary>
        public float distanceOffset;
        #endregion

        #region Foliage
        private float[,] _lastCheckedHeights;
        private float[,] lastCheckedHeights
        {
            get
            {
                if(_lastCheckedHeights == null)
                {
                    _lastCheckedHeights = terrainData.terrainData.GetHeights(0, 0, terrainData.terrainData.heightmapResolution, terrainData.terrainData.heightmapResolution);
                }

                return _lastCheckedHeights;
            }
        }

        [System.NonSerialized]
        public Vector3 lastSceneViewPosition = Vector3.zero;
        #endregion

        #region Sectors
        /// <summary>
        /// our terrain sector which is used to increase performance on big terrains.
        /// </summary>
        [SerializeField]
        UNTerrainSector _sector;
        public UNTerrainSector sector
        {
            get
            {
                if(_sector == null)
                {
                    _sector = GenerateSector(sectorResolution);
                }

                return _sector;
            }
            set
            {
                _sector = value;
            }
        }

        [SerializeField]
        int _sectorResolution = 10;
        /// <summary>
        /// How much times will the terrain be divided?
        /// the more => the slower creation but higher performance on runtime..
        /// the less => faster creation but lower performance on runtime.
        /// </summary>
        public int sectorResolution
        {
            get
            {
                return _sectorResolution;
            }
            set
            {
                value = Mathf.Clamp(value, 1, UNTerrainSector.resolutionLimit);

                if(value != _sectorResolution) // if changed
                {
                    _sectorResolution = value;

                    if(!Application.isPlaying && (sector == null || (sector != null && sector.sectorResolution != value))) // if in editor
                    {
                        GenerateSector(value);
                    }
                }
            }
        }

        [SerializeField]
        bool _manageGrass = true;
        /// <summary>
        /// Will the system try to optimize your grass?
        /// 
        /// Also, make sure to design the grass LODs if the grass doesnt work as you'd like (Window/uNature/Settings).
        /// </summary>
        public bool manageGrass
        {
            get
            {
                return _manageGrass;
            }
            set
            {
                _manageGrass = value;
            }
        }

        [SerializeField]
        bool _updateGrassOnHeightsChange = true;
        public bool updateGrassOnHeightsChange
        {
            get
            {
                return _updateGrassOnHeightsChange;
            }
            set
            {
                _updateGrassOnHeightsChange = value;
            }
        }

        [SerializeField]
        bool _manageTrees = true;
        /// <summary>
        /// Will the system try to optimize your trees?
        /// </summary>
        public bool manageTrees
        {
            get
            {
                return _manageTrees;
            }
            set
            {
                _manageTrees = value;
            }
        }

        /// <summary>
        /// An routine that is used from the editor to perform realtime tree instances updates.
        /// </summary>
        public IEnumerator verifyTreeInstancesChangeRoutine;
        #endregion

        #region Methods

        /// <summary>
        /// Initiate startup variables
        /// </summary>
        protected override void Awake()
        {
            UNTreePrototype.CheckForMissings(terrainData.treePrototypes, terrain.terrainData.treePrototypes);

            var sectors = GetComponentsInChildren<UNTerrainSector>();

            if(sectors.Length > 1) // if there's more than 1 sectors.
            {
                for(var i = 0; i < sectors.Length; i++)
                {
                    GameObject.DestroyImmediate(sectors[i].gameObject);
                }

                _sector = null;
            }

            if (sector == null)
            {
                GenerateSector(sectorResolution);
            }

            if (Pool == null)
            {
                CreatePool(PoolItemType);
            }

            UNThreadManager.InitializeIfNotAvailable(); // try to initialize if thread manager doesnt exist...

            if (!this.enabled) return;

            sector.FetchTreeInstances(true, null);

            if (!Application.isPlaying) return;

            terrain = GetComponent<Terrain>();

            base.Awake();

            if (UNSettings.instance.UN_TreeInstancesRespawnsEnabled)
            {
                InvokeRepeating("CheckForTreeInstancesRespawns", 5, 5);
            }
        }

        /// <summary>
        /// Generate a sector and assign it to the UNTerrain.
        /// </summary>
        /// <param name="sectorResolution">How many pieces will the terrain be divided to? the bigger it is the more pieces.</param>
        public virtual UNTerrainSector GenerateSector(int sectorResolution, bool multiThread)
        {
            _sector = Sector.GenerateSector<UNTerrainSector, TIChunk>(transform, terrainData.terrainData.size, _sector, sectorResolution);

            return _sector;
        }

        /// <summary>
        /// Generate a sector and assign it to the UNTerrain.
        /// </summary>
        /// <param name="sectorResolution">How many pieces will the terrain be divided to? the bigger it is the more pieces.</param>
        public virtual UNTerrainSector GenerateSector(int sectorResolution)
        {
            return GenerateSector(sectorResolution, false);
        }

        /// <summary>
        /// On terrain changed.
        /// </summary>
        /// <param name="changedFlags"></param>
        protected virtual void OnTerrainChanged(int changedFlags)
        {
            TerrainChangedFlags flag = (TerrainChangedFlags)changedFlags;

            if (flag == TerrainChangedFlags.TreeInstances)
            {
                if (sector == null)
                {
                    GenerateSector(sectorResolution);
                }

                #if UNITY_EDITOR
                UNEditorUtility.StartSceneScrollbar("Fetching tree instances...", 1);
                #endif

                sector.FetchTreeInstances(true, () =>
                {
                    #if UNITY_EDITOR
                    UNSettings.Log("Trees Updated.");

                    UNEditorUtility.currentProgressIndex = 1;
                    #endif
                });
            }
            else if (flag == TerrainChangedFlags.TreePrototypesChanged)
            {
                CreatePool(PoolItemType);
            }
            else if (flag == TerrainChangedFlags.DelayedHeightmapUpdate)
            {
                if (FoliageCore_MainManager.instance == null || !updateGrassOnHeightsChange || !manageGrass) return;

                TerrainData tData = terrain.terrainData;

                float[,] heights = tData.GetHeights(0, 0, tData.heightmapResolution, tData.heightmapResolution);
                bool changed = false;

                float cordX;
                float cordY;

                FoliageCore_MainManager manager = FoliageCore_MainManager.instance;

                int mChunkID;
                FoliageCore_Chunk mChunk;
                FoliageManagerInstance mInstance;

                Vector3 position = transform.position;
                Vector3 managerPosition = manager.transform.position;

                FoliageWorldMaps worldMaps;

                int transformedSize;

                for (int x = 0; x < tData.heightmapResolution; x++)
                {
                    for(int y = 0; y < tData.heightmapResolution; y++)
                    {
                        if (heights[y, x] != terrainData.multiThreaded_terrainHeights[y, x]) // change on cord.
                        {
                            changed = true;

                            cordX = ((x * tData.size.x) / tData.heightmapResolution) + position.x;
                            cordY = ((y * tData.size.z) / tData.heightmapResolution) + position.z;

                            mChunkID = manager.GetChunkID(cordX - managerPosition.x, cordY - managerPosition.z);

                            if (manager.CheckChunkInBounds(mChunkID))
                            {
                                mChunk = manager.sector.foliageChunks[mChunkID];

                                if (!mChunk.isFoliageInstanceAttached) continue;

                                mInstance = mChunk.GetOrCreateFoliageManagerInstance();

                                worldMaps = mInstance.worldMaps;

                                cordX = mInstance.TransformCord(cordX, mInstance.transform.position.x);
                                cordY = mInstance.TransformCord(cordY, mInstance.transform.position.z);

                                transformedSize = mInstance.TransformCord(2, 0);

                                worldMaps.UpdateHeightsAndNormals(cordX, cordY, transformedSize, transformedSize, false);
                                worldMaps.SetPixels32Delayed();
                            }
                        }
                    }
                }

                if (changed)
                {
                    FoliageCore_MainManager.SaveDelayedMaps();
                }

                terrainData.multiThreaded_terrainHeights = heights;
            }
        }

        /// <summary>
        /// This method will check every set amount of time the trees in the terrain and restore them if needed.
        /// </summary>
        protected virtual void CheckForTreeInstancesRespawns()
        {
            UNThreadManager.instance.RunOnThread(new ThreadTask(() =>
            {
                ChunkObject chunkObject;

                for (int i = 0; i < sector.treeInstancesChunks.Count; i++)
                {
                    for (int b = 0; b < sector.treeInstancesChunks[i].objects.Count; b++)
                    {
                        chunkObject = sector.treeInstancesChunks[i].objects[b];

                        if (chunkObject.isRemoved && (System.DateTime.Now - chunkObject.removedTime).TotalMinutes > (chunkObject.harvestableComponent == null ? UNSettings.instance.UN_TreeInstancesRespawnsTime : chunkObject.prefabHarvestableComponent.respawnTimeInMinutes))
                        {
                            UNThreadManager.instance.RunOnUnityThread(new ThreadTask<ChunkObject>((ChunkObject _chunkObject) =>
                                {
                                    TerrainPoolItem.RestoreTreeInstanceToTerrain(terrain, _chunkObject.instanceID);
                                }, chunkObject));
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// Add this terrain to the terrains Pool
        /// </summary>
        protected override void OnEnable()
        {
            if (terrainData == null) return;

            terrainData.Initialize();

            if (Application.isPlaying)
            {
                base.OnEnable();
                terrains.Add(this);
            }
        }

        /// <summary>
        /// Remove this terrain to the terrains Pool
        /// </summary>
        protected override void OnDisable()
        {
            if (Application.isPlaying)
            {
                base.OnDisable();
                terrains.Remove(this);

                // this will reset the Foliage & trees to the original state in order to save settings.
                if (sector != null)
                {
                    sector.ApplicationQuit();
                }
            }
        }

        /// <summary>
        /// Check for seeker on terrain.
        /// </summary>
        /// <param name="seeker">Our seeker.</param>
        /// <param name="seekerPos">the seeker pos</param>
        public override void Check(Seekers.UNSeeker seeker, Vector3 seekerPos, float seekingDistance, bool isPlaying)
        {
            #region Tree Instaces
            if (!isPlaying || !manageTrees) return;

            var chunks = sector.getChunks(seekerPos, seeker.seekingDistance, true);
            for(var i = 0; i < chunks.Count; i++)
            {
                var treeInstancesChunk = chunks[i] as TIChunk;
                if(treeInstancesChunk != null)
                {
                    treeInstancesChunk.CheckForNearbyTreeInstances(seeker, this);
                }
            }

            #endregion
        }

        /// <summary>   
        /// Check if the seeker is in range of the terrain.
        /// </summary>
        /// <param name="seeker">Seeker</param>
        /// <returns>in range?</returns>
        public override bool InDistance(Seekers.UNSeeker seeker)
        {
            var tMin = new Vector2(terrain.transform.position.x - distanceOffset, terrain.transform.position.z - distanceOffset);
            var tMax = new Vector2(terrain.transform.position.x + tData.size.x + distanceOffset, terrain.transform.position.z + tData.size.z + distanceOffset);

            var sPos = new Vector2(seeker.transform.position.x, seeker.transform.position.z);
            return tMin.x < sPos.x && tMin.y < sPos.y && tMax.x > sPos.x && tMax.y > sPos.y;
        }

        /// <summary>
        /// Fill up our Pool.
        /// </summary>
        public override void CreatePool(System.Type PoolItemType)
        {
            base.CreatePool(PoolItemType);

            if (terrain == null)
            {
                terrain = GetComponent<Terrain>();
            }

            if (PoolItemType == null)
            {
                PoolItemType = typeof(TerrainPoolItem);
            }

            List<TreePrototype> treeTypes = terrain.terrainData.GetUsedPrototypes();
            UNTreePrototype treeType;

            TerrainPoolItem item;
            IPoolComponent[] itemComponents;
            IPoolComponent itemComponent;

            GameObject go;
            GameObject colliderGO;

            Collider[] colliders;
            Collider collider;
            System.Type colliderType;

            Collider colliderComponent;

            PropertyInfo property;

            for (int i = 0; i < terrainData.treePrototypes.Count; i++)
            {
                treeType = terrainData.treePrototypes[i];

                if (treeType.prototypeObject == null) continue;

                treeType.prototypeObject.transform.position = Vector3.zero;

                if (treeType.enabled && terrainData.ContainsInPrototypes(treeType, treeTypes) || (!treeType.isMissing && treeType.forcePoolCreation)) // create Pool if items are used/ if the Pool creation for that item is forced.
                {
                    for (int count = 0; count < PoolAmount; count++)
                    {
                        #region TreeInstances_PoolItems
                        go = GameObject.Instantiate<GameObject>(treeType.prototypeObject);
                        go.name = treeType.prototypeObject.name + " : " + count;
                        go.transform.parent = Pool.transform;

                        item = go.GetComponent<TerrainPoolItem>();

                        if (item == null)
                        {
                            if (go.GetComponent<PoolItem>() != null)
                            {
                                Destroy(go.GetComponent<PoolItem>()); // remove the component to avoid duplications.
                            }

                            item = (TerrainPoolItem)go.AddComponent(PoolItemType);
                        }

                        itemComponents = go.GetComponents<IPoolComponent>();

                        item.isCollider = false;

                        Pool.AddToPool(item, i, 0);
                        #endregion

                        #region Colliders
                        go = new GameObject(treeType.prototypeObject.name + " Collider : " + count);
                        go.transform.parent = Pool.transform;

                        go.transform.localScale = treeType.prototypeObject.transform.localScale;
                        go.transform.localRotation = treeType.prototypeObject.transform.localRotation;

                        item = go.AddComponent(item.GetType()).GetCopyOf<TerrainPoolItem>(item) as TerrainPoolItem;

                        item.isCollider = true;

                        // presist original item components
                        for (int b = 0; b < itemComponents.Length; b++)
                        {
                            itemComponent = itemComponents[b];

                            go.AddComponent(itemComponent.GetType()).GetCopyOf<IPoolComponent>(itemComponent);
                        }

                        colliders = treeType.prototypeObject.GetComponentsInChildren<Collider>(true);

                        for (int b = 0; b < colliders.Length; b++)
                        {
                            collider = colliders[b];

                            itemComponents = collider.GetComponents<IPoolComponent>();

                            if (!collider.enabled || !collider.gameObject.activeSelf || (treeType.ignoreTriggetColliders && collider.isTrigger)) continue;

                            colliderType = collider.GetType();

                            colliderGO = new GameObject("Collider");
                            colliderGO.transform.parent = go.transform;

                            colliderGO.transform.localPosition = collider.transform.localPosition;
                            colliderGO.transform.localScale = collider.transform.localScale;
                            colliderGO.transform.localRotation = collider.transform.localRotation;
                            colliderGO.transform.tag = collider.transform.tag;

                            colliderComponent = (Collider)colliderGO.AddComponent(collider.GetType());

                            colliderComponent.isTrigger = collider.isTrigger;
                            colliderComponent.sharedMaterial = collider.sharedMaterial;

                            // presist original item components
                            for (int c = 0; c < itemComponents.Length; c++)
                            {
                                itemComponent = itemComponents[c];

                                colliderGO.AddComponent(itemComponent.GetType()).GetCopyOf<IPoolComponent>(itemComponent);
                            }

                            // Reflect size
                            property = colliderType.GetProperty("size");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect radius
                            property = colliderType.GetProperty("radius");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect center
                            property = colliderType.GetProperty("center");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect height
                            property = colliderType.GetProperty("height");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect direction
                            property = colliderType.GetProperty("direction");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect sharedMesh
                            property = colliderType.GetProperty("sharedMesh");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }

                            // Reflect convex (mesh collider)
                            property = colliderType.GetProperty("convex");
                            if (property != null)
                            {
                                property.SetValue(colliderComponent, property.GetValue(collider, null), null);
                            }
                        }

                        Pool.AddToPool(item, i, collidersPoolItemInstanceIncrease);
                        #endregion
                    }
                }
            }

#if UNITY_EDITOR
#if UNITY_5_3_OR_NEWER
            if(!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
#else
            if (UnityEditor.EditorApplication.currentScene != "" && !Application.isPlaying)
            {
                UnityEditor.EditorApplication.MarkSceneDirty();
            }
#endif
#endif
        }

        /// <summary>
        /// Return the position that can be used with the chunks.
        /// </summary>
        /// <param name="position">the original position</param>
        /// <returns>position that can be used in local space with the terrain</returns>
        public override Vector3 FixPosition(Vector3 position)
        {
            return position - transform.position;
        }

        /// <summary>
        /// On terrain position changed
        /// </summary>
        /// <param name="newPosition"></param>
        protected override void OnPositionChanged(Vector3 newPosition)
        {
            GenerateSector(sectorResolution, true);
        }

        #endregion
    }

    [System.Flags]
    public enum TerrainChangedFlags
    {
        NoChange = 0,
        Heightmap = 1,
        TreeInstances = 2,
        DelayedHeightmapUpdate = 4,
        FlushEverythingImmediately = 8,
        RemoveDirtyDetailsImmediately = 16,
        TreePrototypesChanged = 32,
        WillBeDestroyed = 256,
    }
}
