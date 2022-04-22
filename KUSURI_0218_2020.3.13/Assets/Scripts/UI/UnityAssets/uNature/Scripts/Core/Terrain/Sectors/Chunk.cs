using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using uNature.Core.Threading;
using uNature.Core.Pooling;
using uNature.Core.Seekers;
using uNature.Core.Settings;
using uNature.Core.Terrains;
using uNature.Core.Collections;
using uNature.Core.ClassExtensions;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Sectors
{
    /// <summary>
    /// part of the sector which contains information.
    /// </summary>
    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        Vector2 _position;
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                minPoint = position;
                maxPoint = position + size;

                _position3D = new Vector3(position.x, 0, position.y);
            }
        }

        [SerializeField]
        protected Vector3 _position3D;
        public Vector3 position3D
        {
            get
            {
                return _position3D;
            }
        }

        [SerializeField]
        protected Vector3 _worldPosition3D;
        public Vector3 worldPosition3D
        {
            get
            {
                return _worldPosition3D;
            }
        }

        public Vector2 terrainRelativeSize;

        [SerializeField]
        Vector2 _size;
        public Vector2 size
        {
            get { return _size; }
            set
            {
                _size = value;
                _extents = value / 2;

                OnSizeChanged();
            }
        }

        [SerializeField]
        Vector2 _extents;
        public Vector2 extents
        {
            get { return _extents; }
            set
            {
                extents = value;
                _size = value * 2;
            }
        }

        public Vector2 minPoint;
        public Vector2 maxPoint;

        public Vector2 center
        {
            get
            {
                return new Vector2(position.x + (size.x / 2), position.y + (size.y / 2));
            }
        }

        public short chunkID;
        public int x;
        public int z;

        public Transform sectorOwner;

        protected virtual string chunkType
        {
            get
            {
                return "Default";
            }
        }
        #endregion

        /// <summary>
        /// Create a new chunk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sector"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="unTerrain"></param>
        /// <returns></returns>
        public static T CreateChunk<T>(Sector sector, Vector2 position, int x, int z, Vector2 scale, short chunkID) where T : Chunk
        {
            GameObject chunkObject = new GameObject();
            chunkObject.transform.parent = sector.transform;

            chunkObject.transform.localPosition = new Vector3(position.x, 0, position.y);
            chunkObject.transform.localRotation = Quaternion.identity;
            chunkObject.transform.localScale = Vector3.one;

            T chunkType = chunkObject.AddComponent<T>();

            chunkType.sectorOwner = sector.sectorOwner;
            chunkType.position = position;
            chunkType._position3D = new Vector3(position.x, 0, position.y);
            chunkType._worldPosition3D = chunkObject.transform.position;
            chunkType.size = scale;

            chunkType.x = x;
            chunkType.z = z;

            chunkType.minPoint = position;
            chunkType.maxPoint = position + scale;

            chunkType.chunkID = chunkID;

            chunkObject.name = chunkType.chunkType + " Chunk : " + position;

            if (!Application.isPlaying)
            {
                chunkType.Awake();
            }

            chunkType.OnCreated();
            return chunkType;
        }

        /// <summary>
        /// On size parameter changed.
        /// </summary>
        protected virtual void OnSizeChanged()
        {
        }

        /// <summary>
        /// Called when the object is created/ initializes.
        /// <param name="terrain">the terrain</param>
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// Called on disable
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// Called on disable
        /// </summary>
        protected virtual void OnDisable()
        {

        }

        /// <summary>
        /// Is this point inside the chunk?
        /// </summary>
        /// <param name="point">the point</param>
        /// <returns></returns>
        public bool Contains(Vector2 point, float offset)
        {
            return Contains(new Vector3(point.x, 0, point.y), offset);
        }

        /// <summary>
        /// Is this point inside the chunk?
        /// </summary>
        /// <param name="point">the point</param>
        /// <returns></returns>
        public bool Contains(Vector3 point, float offset)
        {
            var min = new Vector2(minPoint.x - offset, minPoint.y - offset);
            var max = new Vector2(maxPoint.x + offset, maxPoint.y + offset);

            return point.x >= min.x && point.z >= min.y && point.x <= max.x && point.z <= max.y;
        }

        /// <summary>
        /// Called when the chunk is created.
        /// </summary>
        public virtual void OnCreated()
        {
        }

        /// <summary>
        /// Reset the chunk's propoties.
        /// </summary>
        public virtual void ResetChunk()
        {

        }

        /// <summary>
        /// Draw gizmos.
        /// </summary>
        public virtual void OnDrawGizmos()
        {
            if (Settings.UNSettings.instance.UN_Debugging_Enabled)
            {
                Vector2 chunkCenter;
                Vector3 chunkSize;

                Gizmos.matrix = Matrix4x4.identity;

                chunkCenter = center;
                chunkSize = new Vector3(size.x, 1, size.y);

                Gizmos.DrawWireCube(new Vector3(chunkCenter.x, 0, chunkCenter.y), chunkSize);
            }
        }
    }

    [System.Serializable]
    public class ChunkObject
    {
        public TreeInstance treeInstance;
        public int prototypeID;
        public int instanceID;

        public Vector3 worldPosition;
        public Vector2 depthPosition;

        [System.NonSerialized]
        public System.DateTime removedTime;

        public float originalHeight;
        public HarvestableTIPoolItem prefabHarvestableComponent;

        public HarvestableTIPoolItem harvestableComponent;

        public bool isRemoved
        {
            get { return treeInstance.heightScale == uNature.Core.Terrains.UNTerrain.removedTreeInstanceHeight; }
        }

        public ChunkObject(int _instanceID, TreeInstance treeInstance, Vector3 terrainSize, TerrainData tData, Vector3 terrainPosition)
        {
            this.treeInstance = treeInstance;
            this.originalHeight = treeInstance.heightScale;

            worldPosition = treeInstance.position.LocalToWorld(terrainSize, terrainPosition);

            this.depthPosition = new Vector2(worldPosition.x, worldPosition.z);

            this.prototypeID = treeInstance.prototypeIndex;

            this.instanceID = _instanceID;

            UNThreadManager.instance.RunOnUnityThread(new ThreadTask<ChunkObject, TerrainData>((ChunkObject chunkObject, TerrainData terrainData) =>
                {
                    GameObject prefab = terrainData.treePrototypes[chunkObject.prototypeID].prefab;

                    if (prefab != null)
                    {
                        HarvestableTIPoolItem harvestableComponent = prefab.GetComponent<HarvestableTIPoolItem>();

                        if (harvestableComponent != null)
                        {
                            chunkObject.harvestableComponent = harvestableComponent;
                        }
                    }
                }, this, tData));
        }

        public void Remove()
        {
            removedTime = System.DateTime.Now;
        }

    }

    /// <summary>
    /// A class that holds a level which all assigned on different frames.
    /// </summary>
    public class GrassLODLevel
    {
        public UNDimensionalList<int> details = new UNDimensionalList<int>();
        public Vector2 position = new Vector2(Mathf.Infinity, Mathf.Infinity);

        public void Add(int x, int value, Vector2 pos)
        {
            if(!details.ContainsKey(x))
            {
                details.TryAddKey(new List<int>());
            }

            if (pos.x <= position.x && pos.y <= position.y)
            {
                this.position = pos;
            }

            details[details.Count - 1].Add(value);
        }

        public static GrassLODLevel Create()
        {
            var lod = new GrassLODLevel();
            return lod;
        }
    }
}