#if UN_MapMagic
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MapMagic;
using uNature.Core.FoliageClasses;
using uNature.Core.Utility;
using uNature.Core.Threading;

using System.Reflection;

namespace uNature.Core.Extensions.MapMagicIntegration
{
    /// <summary>
    /// An manager class for the map magic integration, attach this script once in each one of your map magic integrated scenes.
    /// </summary>
    [ExecuteInEditMode]
    public class UNMapMagic_Manager : MonoBehaviour
    {
        const int frameThresholdDefault = 30;

        static FoliageCore_MainManager _manager;
        static FoliageCore_MainManager manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = FoliageCore_MainManager.instance;
                }

                return _manager;
            }
        }

        public static void ApplyGrassOutput(uNatureGrassTuple grassTuple)
        {
            FoliageCore_MainManager.instance.StartCoroutine(ApplyGrassOutput_IEnumerator(grassTuple, Application.isPlaying));
        }
        private static IEnumerator ApplyGrassOutput_IEnumerator(uNatureGrassTuple grassTuple, bool playing, int framesThreshold = frameThresholdDefault)
        {
            if (manager == null) yield break;

            grassTuple.data.Warmup();

            uNatureGrassTuple uNatureTuple = (grassTuple as uNatureGrassTuple);
            var uNatureTupleData = uNatureTuple.data;

            DetailPrototype[] prototypes = uNatureTuple.tupleInformation.item2;
            FoliagePrototype[] uNaturePrototypes = UNStandaloneUtility.AddPrototypesIfDontExist(prototypes);

            var data = uNatureTupleData.details;
            int lengthX = data.GetLength(0);
            int lengthZ = data.GetLength(1);

            FoliageManagerInstance mInstance;

            for (var i = 0; i < uNatureTupleData.usedChunks.Length; i++)
            {
                mInstance = uNatureTupleData.usedChunks[i].GetOrCreateFoliageManagerInstance(false);
                mInstance.grassMap.SetPixels32Delayed();

                if (playing)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            var maxIntervalsPerFrame = lengthX / 50;
            for (var x = 0; x < lengthX; x++)
            {
                for (var z = 0; z < lengthZ; z++)
                {
                    var detail = data[x, z];

                    var mChunk = detail.mChunk;
                    mInstance = mChunk.attachedFoliageInstance;
                    var grassMap = mInstance.grassMap;
                    
                    for (var prototypeIndex = 0; prototypeIndex < uNaturePrototypes.Length; prototypeIndex++)
                    {
                        var prototype = (byte)uNaturePrototypes[prototypeIndex].id;
                        grassMap.SetDensity(detail.interpolatedIndex, prototype, grassTuple.densities[prototypeIndex][x, z]);
                    }
                }

                if ((x % maxIntervalsPerFrame) == 0 && playing)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            FoliageCore_MainManager.SaveDelayedMaps();
            FoliageMeshManager.RegenerateQueueInstances();
        }

        public static void ApplyHeightOutput(uNatureHeightTuple heightTuple, Terrain terrain)
        {
            FoliageCore_MainManager.instance.StartCoroutine(ApplyHeightOutput_IEnumerator(heightTuple, terrain, Application.isPlaying));
        }
        private static IEnumerator ApplyHeightOutput_IEnumerator(uNatureHeightTuple heightTuple, Terrain terrain, bool playing)
        {
            if (manager == null || terrain == null) yield break;

            yield return new WaitForSeconds(0.1f);

            playing = Application.isPlaying;

            heightTuple.chunksData.Warmup();

            var data = heightTuple.chunksData;
            int lengthX = data.details.GetLength(0);
            int lengthZ = data.details.GetLength(1);
            int mask = manager.FoliageGenerationLayerMask;

            FoliageManagerInstance mInstance;
            CalculatedChunksData.uNatureDetailInformation detail;

            Vector3 pos = new Vector3();

            // wait for generation to be completed...
            for (int i = 0; i < data.usedChunks.Length; i++)
            {
                mInstance = data.usedChunks[i].GetOrCreateFoliageManagerInstance(false);
                mInstance.worldMaps.heightMap.SetPixels32Delayed();

                if (playing)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            NormalizedHeightData[,] terrainHeights = new NormalizedHeightData[lengthX, lengthZ];
            int maxIntervalsPerFrame = lengthX / 50;
            for (int x = 0; x < lengthX; x++)
            {
                for (int z = 0; z < lengthZ; z++)
                {
                    detail = data.details[x, z];

                    pos.x = detail.worldPositionX;
                    pos.z = detail.worldPositionZ;

                    terrainHeights[x, z].height = terrain.SampleHeight(pos);
                    terrainHeights[x, z].hMap = detail.mChunk.attachedFoliageInstance._worldMaps.heightMapFast;
                    terrainHeights[x, z].index = detail.interpolatedIndex;

                    /*
                    nHeight = FoliageWorldMaps.NormalizeHeight(height, _manager);

                    mInstance = detail.mChunk.attachedFoliageInstance;
                    mInstance.worldMaps.heightMapFast.SetHeightFast(detail.interpolatedIndex, tempColor, nHeight.x, nHeight.y);
                    */
                }

                if ((x % maxIntervalsPerFrame) == 0 && playing)
                {
                    yield return new WaitForEndOfFrame();

                    while (terrain == null) yield return new WaitForEndOfFrame();
                }
            }

            if (playing) // thread...
            {
                UNThreadManager.instance.RunOnThread(new ThreadTask<NormalizedHeightData[,], bool>(NormalizedHeight_Threaded, terrainHeights, playing));
            }
            else // main thread.
            {
                NormalizedHeight_Threaded(terrainHeights, playing);
            }
            //FoliageCore_MainManager.SaveDelayedMaps();
        }

        private struct NormalizedHeightData
        {
            public float height;
            public FoliageHeightMap hMap;
            public int index;

            public NormalizedHeightData(float height, FoliageHeightMap hMap, int index)
            {
                this.height = height;
                this.hMap = hMap;
                this.index = index;
            }
        }
        private static void NormalizedHeight_Threaded(NormalizedHeightData[,] heights, bool playing)
        {
            var manager = UNMapMagic_Manager.manager;

            Color32 tempColor = new Color32();

            int sizeX = heights.GetLength(0);
            int sizeZ = heights.GetLength(1);

            Vector2b height;
            NormalizedHeightData data;
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    data = heights[x, z];

                    height = FoliageWorldMaps.NormalizeHeight(data.height, manager);
                    data.hMap.SetHeightFast(data.index, tempColor, height.x, height.y);
                    data.hMap.SetPixels32Delayed();
                }
            }

            if (playing)
            {
                UNThreadManager.instance.RunOnUnityThread(new ThreadTask(() =>
                {
                    FoliageCore_MainManager.SaveDelayedMaps();
                }));
            }
            else
            {
                FoliageCore_MainManager.SaveDelayedMaps();
            }
        }

        public static void RegisterGrassPrototypesChange(DetailPrototype[] tempPrototypes)
        {
            var oldPrototypes = FoliageDB.unSortedPrototypes;
            var newPrototypes = GetPrototypeLayers();

            FoliagePrototype oldPrototype;
            GrassOutput.Layer newPrototype;

            List<DetailPrototype> existPrototypes = new List<DetailPrototype>();

            // This loop will check for existing prototypes (don't do anything) OR removed prototypes (remove from uNature)
            for (int i = 0; i < oldPrototypes.Count; i++)
            {
                oldPrototype = oldPrototypes[i];

                for (int b = 0; b < newPrototypes.Length; b++)
                {
                    newPrototype = newPrototypes[b];

                    if (oldPrototype.FoliageTexture == newPrototype.det.prototypeTexture && oldPrototype.FoliageMesh == newPrototype.det.prototype) // found this prototype.
                    {
                        existPrototypes.Add(newPrototype.det);
                        break;
                    }
                    else if (b == newPrototypes.Length - 1) // didn't find this prototype at all! (most likely removed)
                    {
                        FoliageDB.instance.RemovePrototype(oldPrototype);

                        RegisterGrassPrototypesChange(tempPrototypes);
                        return;
                    }
                }
            }

            // This loop will check for new prototypes
            for (int i = 0; i < newPrototypes.Length; i++)
            {
                newPrototype = newPrototypes[i];

                if (!existPrototypes.Contains(newPrototype.det))
                {
                    FoliageDB.instance.AddPrototype(newPrototype.det);
                }
            }
        }

        public static GrassOutput.Layer[] GetPrototypeLayers()
        {
            GrassOutput output;

            for (int i = 0; i < MapMagic.MapMagic.instance.gens.list.Length; i++)
            {
                output = MapMagic.MapMagic.instance.gens.list[i] as GrassOutput;

                if (output != null)
                {
                    return output.baseLayers;
                }
            }

            return null;
        }
        internal static CalculatedChunksData CalculateChunksThreaded(float terrainPositionX, float terrainPositionZ, int resolution)
        {
            if (manager == null) return null;

            CalculatedChunksData chunksData = new CalculatedChunksData();
            chunksData.details = new CalculatedChunksData.uNatureDetailInformation[resolution, resolution];

            float resolutionRatio = (float)MapMagic.MapMagic.instance.terrainSize / resolution;

            Vector3 worldPosition;

            FoliageCore_Chunk mChunk;
            FoliageManagerInstance mInstance = null;

            Vector3 managerPosition = manager.threadPosition;

            var foliageChunks = manager.sector.foliageChunks;

            int chunkIndex = 0;

            int interpolatedX;
            int interpolatedZ;
            int interpolatedIndex;

            List<FoliageCore_Chunk> currentManagerChunks = new List<FoliageCore_Chunk>();

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    mInstance = null;

                    worldPosition.x = x * resolutionRatio + terrainPositionX;
                    worldPosition.z = z * resolutionRatio + terrainPositionZ;

                    chunkIndex = manager.GetChunkID(worldPosition.x - managerPosition.x, worldPosition.z - managerPosition.z);

                    mChunk = foliageChunks[chunkIndex];

                    if (mChunk.isFoliageInstanceAttached)
                    {
                        mInstance = mChunk.GetOrCreateFoliageManagerInstance(false);
                    }

                    if (!currentManagerChunks.Contains(mChunk))
                    {
                        currentManagerChunks.Add(mChunk);
                    }

                    if (mInstance != null)
                    {
                        interpolatedX = mInstance.TransformCord(worldPosition.x, mInstance.pos.x);
                        interpolatedZ = mInstance.TransformCord(worldPosition.z, mInstance.pos.z);

                        interpolatedIndex = interpolatedX + interpolatedZ * mInstance.foliageAreaResolutionIntegral;
                    }
                    else
                    {
                        interpolatedX = (int)(worldPosition.x - mChunk.worldPosition3D.x);
                        interpolatedZ = (int)(worldPosition.z - mChunk.worldPosition3D.z);

                        interpolatedIndex = interpolatedX + interpolatedZ * FoliageManagerInstance.AREA_SIZE;
                    }

                    chunksData.details[x, z] = new CalculatedChunksData.uNatureDetailInformation(mChunk, interpolatedIndex, worldPosition.x, worldPosition.z);
                }
            }

            chunksData.usedChunks = currentManagerChunks.ToArray();
            chunksData.terrainPositionX = terrainPositionX;
            chunksData.terrainPositionZ = terrainPositionZ;

            return chunksData;
        }
    }

    public class uNatureGrassTuple
    {
        private CalculatedChunksData _data;
        public CalculatedChunksData data
        {
            get
            {
                return _data;
            }
            private set
            {
                _data = value;
            }
        }

        public TupleSet<int[][,], DetailPrototype[]> tupleInformation;
        public List<byte[,]> densities;

        public uNatureGrassTuple(TupleSet<int[][,], DetailPrototype[]> tuple, Vector3 position)
        {
            tupleInformation = tuple;

            data = UNMapMagic_Manager.CalculateChunksThreaded(position.x, position.z, MapMagic.MapMagic.instance.terrainSize);
            CreateCachce();
        }
        private void CreateCachce()
        {
            int detailsLength = MapMagic.MapMagic.instance.resolution;
            int arraySize = MapMagic.MapMagic.instance.terrainSize;
            float densityMultiplier = Mathf.Clamp((float)arraySize / detailsLength, 1, 5);

            var details = tupleInformation.item1;
            var prototypes = tupleInformation.item2;

            densities = new List<byte[,]>();
            for (int i = 0; i < prototypes.Length; i++)
            {
                densities.Add(new byte[arraySize, arraySize]);
            }

            FoliageCore_Chunk mChunk;
            CalculatedChunksData.uNatureDetailInformation detail;
            float rndValue;

            System.Random rnd = new System.Random();
            for (int x = 0; x < arraySize; x++)
            {
                for (int z = 0; z < arraySize; z++)
                {
                    detail = data.details[x, z];
                    mChunk = detail.mChunk;

                    for (int prototypeIndex = 0; prototypeIndex < prototypes.Length; prototypeIndex++)
                    {
                        rndValue = rnd.GetRndRange(0f, 1f);

                        if (densityMultiplier > 1)
                        {
                            if (rndValue > 0.75f) continue;
                        }

                        rndValue *= densityMultiplier; // multiply to get normalized result
                        densities[prototypeIndex][x, z] = (byte)(details[prototypeIndex][(int)(z / densityMultiplier), (int)(x / densityMultiplier)] / rndValue);
                    }
                }
            }
        }
    }
    public class uNatureHeightTuple
    {
        private CalculatedChunksData _chunksData;
        public CalculatedChunksData chunksData
        {
            get
            {
                return _chunksData;
            }
        }

        public CalculatedDetailsNormalizedHeightData[,] interpolatedHeights;

        public float[,] normalizedHeights;

        public struct CalculatedDetailsNormalizedHeightData
        {
            public byte h1;
            public byte h2;

            public CalculatedDetailsNormalizedHeightData(Vector2 normalizedHeights)
            {
                h1 = (byte)normalizedHeights.x;
                h2 = (byte)normalizedHeights.y;
            }
        }

        public uNatureHeightTuple(float[,] heights, Vector3 terrainPos)
        {
            normalizedHeights = heights;

            _chunksData = UNMapMagic_Manager.CalculateChunksThreaded(terrainPos.x, terrainPos.z, MapMagic.MapMagic.instance.terrainSize);
            //interpolatedHeights = CalculateHeights(terrainPos.x, terrainPos.z);
        }

        private CalculatedDetailsNormalizedHeightData[,] CalculateHeights(float terrainPositionX, float terrainPositionZ)
        {
            /*
            if (FoliageCore_MainManager.instance == null) return null;

            int lengthX = chunksData.details.GetLength(0);
            int lengthZ = chunksData.details.GetLength(1);

            var height = MapMagic.MapMagic.instance.terrainHeight;

            CalculatedDetailsNormalizedHeightData[,] heights = new CalculatedDetailsNormalizedHeightData[lengthX, lengthZ];
            for (int x = 0; x < lengthX; x++)
            {
                for (int z = 0; z < lengthZ; z++)
                {
                    heights[x, z] = new CalculatedDetailsNormalizedHeightData(FoliageWorldMaps.NormalizeHeight(normalizedHeights[z, x] * height)); // get normalized height
                }
            }

            return heights;
            */

            return null;
        }
    }
    public class CalculatedChunksData
    {
        public class uNatureDetailInformation
        {
            public FoliageCore_Chunk mChunk;
            public int interpolatedIndex;

            public float worldPositionX;
            public float worldPositionZ;

            public uNatureDetailInformation(FoliageCore_Chunk mChunk, int interpolatedIndex, float worldPositionX, float  worldPositionZ)
            {
                this.mChunk = mChunk;
                this.interpolatedIndex = interpolatedIndex;
                this.worldPositionX = worldPositionX;
                this.worldPositionZ = worldPositionZ;
            }
        }

        public uNatureDetailInformation[,] details;
        public FoliageCore_Chunk[] usedChunks;

        public float terrainPositionX;
        public float terrainPositionZ;

        public void Warmup()
        {
            FoliageManagerInstance mInstance;
            FoliageWorldMaps wMaps;

            for (int i = 0; i < usedChunks.Length; i++)
            {
                mInstance = usedChunks[i].attachedFoliageInstance;

                if(mInstance != null)
                {
                    wMaps = mInstance.worldMaps;

                    mInstance.UpdateGrassMapsForMaterials(true);

                    if (wMaps.heightMap.mapPixels == null) continue;
                }
            }
        }
    }
}
#endif