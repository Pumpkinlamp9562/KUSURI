using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Seekers;
using uNature.Core.ClassExtensions;
using uNature.Core.Pooling;
using uNature.Core.Terrains;
using uNature.Core.Threading;

namespace uNature.Core.Sectors
{
    public class TIChunk : Chunk
    {
        #region Variables
        public List<int> objectsInstanceIDs = new List<int>();

        public List<ChunkObject> objects = new List<ChunkObject>();

        protected override string chunkType
        {
            get
            {
                return "TreeInstances";
            }
        }

        [SerializeField]
        Terrain _terrain;
        public Terrain terrain
        {
            get
            {
                if (_terrain == null)
                {
                    _terrain = transform.parent.parent.GetComponent<Terrain>();
                }

                return _terrain;
            }
            set
            {
                _terrain = value;
            }
        }
        #endregion

        /// <summary>
        /// Called on awake
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="terrainBase"></param>
        public override void Awake()
        {
            base.Awake();
            GenerateTreeInstances(terrain.terrainData.treeInstances, terrain.terrainData.size, terrain.terrainData, terrain.transform.position);
        }

        /// <summary>
        /// Called when the size is changed
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            UNTerrain baseTerrain = GetComponentInParent<UNTerrain>();

            terrainRelativeSize = size.GetRelativeScale(baseTerrain);
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Settings.UNSettings.instance.UN_Debugging_Enabled)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    Gizmos.DrawCube(objects[i].worldPosition, Vector3.one * 2f);
                }
            }
        }

        /// <summary>
        /// Called when created.
        /// </summary>
        public override void OnCreated()
        {
            base.OnCreated();

            this._terrain = sectorOwner.GetComponent<Terrain>();
        }

        /// <summary>
        /// Generate tree intances, derived from a certain provided tree instances
        /// </summary>
        /// <param name="trees">the tree instances</param>
        /// <param name="tData">the terrain data</param>
        public virtual void GenerateTreeInstances(TreeInstance[] trees, Vector3 terrainSize, TerrainData tData, Vector3 terrainPos)
        {
            objects = new List<ChunkObject>();

            for (int i = 0; i < objectsInstanceIDs.Count; i++)
            {
                try
                {
                    objects.Add(new ChunkObject(objectsInstanceIDs[i], trees[objectsInstanceIDs[i]], terrainSize, tData, terrainPos));
                }
                catch
                {
                    objectsInstanceIDs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Add a tree instance into this chunk
        /// </summary>
        /// <param name="instanceID">the targeted tree instance.</param>
        /// <param name="treeInstance">the tree instance you want to add</param>
        /// <param name="terrainData">the terrain data that this chunk belongs to</param>
        public void AddTreeInstance(int instanceID, Vector3 terrainSize, TreeInstance treeInstance, TerrainData terrainData, Vector3 terrainPos, UNTerrainSector sector)
        {
            try
            {
                objectsInstanceIDs.Add(instanceID);

                objects.Add(new ChunkObject(instanceID, treeInstance, terrainSize, terrainData, terrainPos));

                sector.treeInstancesCount++;
            }
            catch { }
        }

        /// <summary>
        /// Reset chunk.
        /// </summary>
        public override void ResetChunk()
        {
            objects.Clear();
            objectsInstanceIDs.Clear();
        }

        /// <summary>
        /// Check and assign nearby tree instances.
        /// </summary>
        public void CheckForNearbyTreeInstances(UNSeeker seeker, UNTerrain terrain)
        {
            if (objects.Count == 0) return; // no objects found on this specific chunk.

            objects.Sort((objA, objB) => Vector2
                .Distance(objA.depthPosition, seeker.threadPositionDepth)
                .CompareTo(Vector2.Distance(objB.depthPosition, seeker.threadPositionDepth)));

            for (var b = 0; b < objects.Count; b++)
            {
                var item = objects[b];

                if (Vector2.Distance(item.depthPosition, seeker.threadPositionDepth) > seeker.seekingDistance || item.isRemoved) continue; // continue if out of distance or if the tree is "removed"

                PoolItem poolItem = terrain.Pool.TryPool<TerrainPoolItem>(item.prototypeID, UNTerrain.collidersPoolItemInstanceIncrease, item.instanceID, false, false); // add 1000 at the start to provide a unique symbol.
                if (poolItem != null)
                {
                    poolItem.OnPool();

                    var threadTask = new ThreadTask<PoolItem, Vector3, TreeInstance>((_PoolItem, _pos, _item) =>
                    {
                        var scale = _item.GetWorldScale();
                        var rotation = _item.GetWorldRotation();

                        _PoolItem.transform.rotation = rotation;
                        _PoolItem.transform.localScale = scale;

                        _PoolItem.MoveItem(_pos);
                    }, poolItem, item.worldPosition, item.treeInstance);

                    UNThreadManager.instance.RunOnUnityThread(threadTask);
                }
                else
                {
                    if (terrain.Pool.IsAlreadyPooled(item.instanceID)) continue;

                    var poolItems = terrain.Pool.GetPoolOfItem(item.prototypeID, UNTerrain.collidersPoolItemInstanceIncrease);
                    if (poolItems.Count <= 0) continue;

                    poolItems.Sort((poolItemA, poolItemB) =>
                        Vector2.Distance(poolItemA.threadPositionDepth, seeker.threadPositionDepth)
                            .CompareTo(Vector2.Distance(poolItemB.threadPositionDepth, seeker.threadPositionDepth)));

                    poolItem = poolItems[poolItems.Count - 1];
                    terrain.Pool.PoolItem(poolItem, false, item.instanceID, true);

                    var threadTask = new ThreadTask<PoolItem, Vector3, TreeInstance>((_PoolItem, _pos, _item) =>
                    {
                        var scale = _item.GetWorldScale();
                        var rotation = _item.GetWorldRotation();

                        _PoolItem.transform.rotation = rotation;
                        _PoolItem.transform.localScale = scale;

                        _PoolItem.MoveItem(_pos);
                    }, poolItem, item.worldPosition, item.treeInstance);
                    UNThreadManager.instance.RunOnUnityThread(threadTask);
                }
            }
        }
    }
}
