using UnityEngine;

using System.Collections.Generic;

using uNature.Core.ClassExtensions;
using uNature.Core.Terrains;
using uNature.Core.Threading;

using uNature.Wrappers.Linq;

namespace uNature.Core.Sectors
{
    /// <summary>
    /// An sector class dedicated only for terrains.
    /// </summary>
    public class UNTerrainSector : Sector
    {
        public UNTerrain unTerrain;

        [SerializeField]
        public Terrain _terrain;
        public Terrain terrain
        {
            get
            {
                if(_terrain == null)
                {
                    _terrain = transform.parent.GetComponent<Terrain>();
                }

                return _terrain;
            }
            set
            {
                if(value != _terrain)
                {
                    _terrain = value;
                    unTerrain = _terrain.GetComponent<UNTerrain>();
                }
            }
        }

        [SerializeField]
        int _treeInstancesCount;
        public int treeInstancesCount
        {
            get { return _treeInstancesCount; }
            set { _treeInstancesCount = value; }
        }

        [SerializeField]
        private List<TIChunk> _treeInstancesChunks;
        public List<TIChunk> treeInstancesChunks
        {
            get
            {
                if (_treeInstancesChunks == null)
                {
                    _treeInstancesChunks = GetComponentsInChildren<TIChunk>().ToList();
                }

                return _treeInstancesChunks;
            }
        }

        #region Runtime Changes To Revert
        [System.NonSerialized]
        public TreeInstance[] originalTreeInstances;

        bool _restoreComplete = true;
        public bool restoreComplete
        {
            get
            {
                return _restoreComplete;
            }
        }
        #endregion

        /// <summary>
        /// Called when the object is created.
        /// <param name="terrain">The terrain we belong to.</param>
        /// </summary>
        public override void OnCreated(Transform owner, int resolution)
        {
            base.OnCreated(owner, resolution);

            terrain = owner.GetComponent<Terrain>();

            FetchTreeInstances(Application.isPlaying, null); // dont fetch tree with multi-threading
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            this.terrain = GetComponentInParent<Terrain>();

            originalTreeInstances = terrain.terrainData.treeInstances;

            if (Application.isPlaying)
            {
                Debug.Log(string.Format("Terrain Loaded : {0}, TreeInstances Chunks : {1}, Resolution {2}", terrain.name, treeInstancesChunks.Count, sectorResolution));
            }
        }

        protected override void OnChunkCreated(Chunk chunk)
        {
            base.OnChunkCreated(chunk);

            TIChunk treeInstanceChunk = chunk as TIChunk;

            if(treeInstanceChunk != null)
            {
                treeInstancesChunks.Add(treeInstanceChunk);
            }
        }

        protected override void OnStartCreatingChunks()
        {
            base.OnStartCreatingChunks();

            for(int i = 0; i < treeInstancesChunks.Count; i++)
            {
                if(treeInstancesChunks[i] != null)
                {
                    DestroyImmediate(treeInstancesChunks[i]);
                }
            }

            treeInstancesChunks.Clear();
        }

        /// <summary>
        /// Delegate which is used to define a specific thread->non thread action.
        /// </summary>
        /// <param name="treeInstances">the used tree instances</param>
        private delegate void GenerateTreeInstancesTask(TreeFetchingTask_MultiThreaded data);

        /// <summary>
        /// Get all the terrain tree instances into chunks
        /// <param name="useUNThread">Do you want to use the uNature thread to reduce performance issues ?</param>
        /// </summary>
        public void FetchTreeInstances(bool useUNThread, System.Action OnFinish)
        {
            if (treeInstancesChunks.Count == 0) return;

            unTerrain.terrainData.UpdateMultithreadedVariables();
            treeInstancesCount = 0;

            ResetChunks(); // reset all chunks to avoid duplications

            TreeInstance instance;

            TreeInstance[] _treeInstances = terrain.terrainData.treeInstances;
            TreePrototype[] _treePrototypes = terrain.terrainData.treePrototypes;

            UNTreePrototype prototype;

            // create delegate action
            GenerateTreeInstancesTask task = new GenerateTreeInstancesTask((TreeFetchingTask_MultiThreaded data) =>
            {
                for (int i = 0; i < data.treeInstances.Length; i++)
                {
                    instance = data.treeInstances[i];

                    prototype = unTerrain.terrainData.GetPrototype(data.treePrototypes[instance.prototypeIndex]);

                    if (prototype == null || !prototype.enabled) continue;

                    var chunk = getChunk(instance.position.LocalToWorld(useUNThread ? unTerrain.terrainData.multiThreaded_terrainDataSize : terrain.terrainData.size, Vector3.zero), 0f) as TIChunk;

                    if (chunk != null)
                    {
                        chunk.AddTreeInstance(i, unTerrain.terrainData.multiThreaded_terrainDataSize, instance, data.tData, unTerrain.threadPosition, this);
                    }
                }

                if (data.isRunning)
                {
                    for (int i = 0; i < treeInstancesChunks.Count; i++)
                    {
                        treeInstancesChunks[i].GenerateTreeInstances(data.treeInstances, unTerrain.terrainData.multiThreaded_terrainDataSize, data.tData, unTerrain.threadPosition);
                    }
                }

                if(data.OnFinish != null)
                {
                    data.OnFinish();
                }
            });

            var threadData = new TreeFetchingTask_MultiThreaded(_treeInstances, _treePrototypes, terrain.terrainData, Application.isPlaying, OnFinish);

            if (useUNThread)
            {
                UNThreadManager.instance.RunOnThread(new ThreadTask<TreeFetchingTask_MultiThreaded>((TreeFetchingTask_MultiThreaded data) =>
                {
                    task(data);
                }, threadData));
            }
            else
            {
                task(threadData);
            }
        }

        /// <summary>
        /// Called when the application has quit.
        /// </summary>
        public override void ApplicationQuit()
        {
            base.ApplicationQuit();

            #if UNITY_EDITOR
            terrain.terrainData.treeInstances = originalTreeInstances;
            _restoreComplete = true;
            #endif
        }
    }

    public struct TreeFetchingTask_MultiThreaded
    {
        public TreeInstance[] treeInstances;
        public TreePrototype[] treePrototypes;
        public TerrainData tData;
        public bool isRunning;
        public System.Action OnFinish;

        public TreeFetchingTask_MultiThreaded(TreeInstance[] treeInstances, TreePrototype[] treePrototypes, TerrainData tData, bool isRunning, System.Action OnFinish)
        {
            this.treeInstances  = treeInstances;
            this.treePrototypes = treePrototypes;
            this.tData          = tData;
            this.isRunning      = isRunning;
            this.OnFinish       = OnFinish;
        }
    }
}
