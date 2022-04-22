using UnityEngine;
using System.Collections.Generic;

namespace uNature.Core.Sectors
{
    public delegate void SectorRecalculated(List<Chunk> newChunks, Vector2 newChunkSize);

    /// <summary>
    /// A sector which is used to divide the UNTerrain objects in the world to increase performance (can handle more than 200k trees!!)
    /// </summary>
    [System.Serializable]
    public class Sector : MonoBehaviour
    {
        public const int resolutionLimit = 40;

        public int sectorResolution;
        public Transform sectorOwner;

        public event SectorRecalculated OnSectorRecalculated;

        public List<Chunk> chunks = new List<Chunk>();

        [SerializeField]
        Vector2 _chunkSize = Vector2.zero;
        public Vector2 chunkSize
        {
            get
            {
                return _chunkSize;
            }
        }

        /// <summary>
        /// This list is used when trying to get nearby chunks, in order to save GC we are allocating a temp list with a high value which is the current chunks count and assigning null when there is no chunk key
        /// </summary>
        [System.NonSerialized]
        private List<Chunk> _temportaryChunkThreshold;
        private List<Chunk> temportaryChunkThreshold
        {
            get
            {
                if(_temportaryChunkThreshold == null)
                {
                    _temportaryChunkThreshold = new List<Chunk>(chunks.Count / 10);
                }

                return _temportaryChunkThreshold;
            }
        }

        /// <summary>
        /// Called when the object is created.
        /// <param name="terrain">The terrain we belong to.</param>
        /// </summary>
        public virtual void OnCreated(Transform owner, int resolution)
        {
            this.sectorOwner = owner;
            this.sectorResolution = resolution;

            if (OnSectorRecalculated != null)
            {
                OnSectorRecalculated(chunks, chunkSize);
            }
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        /// <param name="terrain">The terrain we belong to</param>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// This method will reset the chunks' propoties, so it can be used again instead of recreating the whole sector.
        /// 
        /// Resets:
        /// 
        /// TreeInstances
        /// </summary>
        public void ResetChunks()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].ResetChunk();
            }
        }

        /// <summary>
        /// Generate a new sector
        /// </summary>
        /// <param name="terrain">The terrain this sector will be generated on</param>
        /// <param name="res">the resolution of the sector (how many times will it be sliced</param>
        /// <returns>The new generated sector.</returns>
        public static T GenerateSector<T, T1>(Transform owner, Vector3 bounds, T sector, int res) where T : Sector where T1 : Chunk
        {
            res = Mathf.Clamp(res, 5, resolutionLimit); // clamp

            if (sector == null)
            {
                GameObject obj = new GameObject("Sector");
                obj.transform.parent = owner;
                obj.transform.localPosition = Vector3.zero;

                sector = obj.AddComponent<T>();
            }

            sector.sectorResolution = res;
            sector.sectorOwner = owner;

            Vector2 size = new Vector2(bounds.x / res, bounds.z / res);
            Vector2 position = new Vector2();

            sector.OnStartCreatingChunks();

            sector._chunkSize = size;

            T1 chunkInstance;
            short chunkCount = 0;

            for (int y = 0; y < sector.sectorResolution; y++)
            {
                for (int x = 0; x < sector.sectorResolution; x++)
                {
                    position.x = x * size.x;
                    position.y = y * size.y;

                    chunkInstance = Chunk.CreateChunk<T1>(sector, position, x, y, size, chunkCount);

                    sector.chunks.Add(chunkInstance);
                    sector.OnChunkCreated(chunkInstance);

                    chunkCount++;
                }
            }

            sector.OnCreated(owner, res);

            sector.OnResolutionChanged();

            return sector;
        }

        /// <summary>
        /// Called when the resolution has been updated.
        /// </summary>
        protected virtual void OnResolutionChanged()
        {

        }

        /// <summary>
        /// Called when a chunk is created to allow custom logic on the inherited sectors.
        /// </summary>
        /// <param name="chunk"></param>
        protected virtual void OnChunkCreated(Chunk chunk)
        {

        }

        /// <summary>
        /// Called right before starting to create the chunks.
        /// </summary>
        protected virtual void OnStartCreatingChunks()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i] == null) continue;

                GameObject.DestroyImmediate(chunks[i].gameObject);
            }
            chunks.Clear();
        }

        /// <summary>
        /// Get a chunk on a certain local space position
        /// </summary>
        /// <param name="pos">the local space position</param>
        /// <param name="offset">the offset (The bigger it is, the farder chunks it will find)</param>
        /// <returns></returns>
        public Chunk getChunk(Vector2 pos, float offset)
        {
            return getChunk(new Vector3(pos.x, 0, pos.y), offset);
        }

        /// <summary>
        /// Get a chunk on a certain local space position
        /// </summary>
        /// <param name="pos">the local space position</param>
        /// <param name="offset">the offset (The bigger it is, the farder chunks it will find)</param>
        /// <returns></returns>
        public Chunk getChunk(Vector3 pos, float offset)
        {
            var chunks = getChunks(pos, offset, true);

            return chunks.Count == 0 ? null : chunks[0];
        }

        /// <summary>
        /// Get a chunk on a certain local space position
        /// </summary>
        /// <param name="pos">the local space position</param>
        /// <param name="offset">the offset (The bigger it is, the farder chunks it will find)</param>
        /// <returns></returns>
        public Chunk getChunk(Vector3 pos)
        {
            return chunks[Mathf.FloorToInt(pos.x / _chunkSize.x) + Mathf.FloorToInt(pos.z / _chunkSize.y) * sectorResolution];
        }

        /// <summary>
        /// Get all of the chunks that contains this specific position
        /// </summary>
        /// <param name="pos">a local space position</param>
        /// <param name="offset">the offset (The bigger it is, the farder chunks it will find)</param>
        /// <returns>The chunks that contains the local space position</returns>
        public List<Chunk> getChunks(Vector2 pos, float offset, bool sortResult)
        {
            return getChunks(new Vector3(pos.x, 0, pos.y), offset, sortResult);
        }

        /// <summary>
        /// Get all of the chunks that contains this specific position
        /// </summary>
        /// <param name="pos">a local space position</param>
        /// <param name="offset">the offset (The bigger it is, the farder chunks it will find)</param>
        /// <returns>The chunks that contains the local space position</returns>
        public List<Chunk> getChunks(Vector3 pos, float offset, bool sortResult)
        {
            temportaryChunkThreshold.Clear();

            Chunk chunk;

            for (int i = 0; i < chunks.Count; i++)
            {
                chunk = chunks[i];

                if (chunk != null)
                {
                    if (chunk.Contains(pos, offset))
                    {
                        temportaryChunkThreshold.Add(chunk);
                    }
                }
            }

            if (sortResult)
            {
                temportaryChunkThreshold.Sort(delegate (Chunk a, Chunk b)
                {
                    if (a == null || b == null) return 0;

                    return Vector2.Distance(a.position, pos).CompareTo(Vector2.Distance(b.position, pos));
                });
            }

            return temportaryChunkThreshold;
        }

        /// <summary>
        /// This method will be called when the application quits, used to revert all changes on terrain.
        /// </summary>
        public virtual void ApplicationQuit()
        {
        }
    }
}