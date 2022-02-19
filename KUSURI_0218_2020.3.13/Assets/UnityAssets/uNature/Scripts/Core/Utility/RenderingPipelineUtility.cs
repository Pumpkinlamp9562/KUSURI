using System.Collections.Generic;
using UnityEngine;

using uNature.Core.Utility;
using uNature.Core.Threading;
using uNature.Core.FoliageClasses.Interactions;

namespace uNature.Core.FoliageClasses
{
    /// <summary>
    /// The rendering pipeline utility for uNature.
    /// [Handles grass creation, Rendering Management and more]
    /// </summary>
    public static class RenderingPipielineUtility
    {
        internal const float INSTANCES_OFFSET_MULTIPLIER = 1.75f;
        internal static Stack<Dictionary<int, RenderingQueueInstance>> QueueInstancesPool = new Stack<Dictionary<int, RenderingQueueInstance>>();
        internal static Stack<UNFastList<RenderingQueueMeshInstanceSimulator>> FastListPool = new Stack<UNFastList<RenderingQueueMeshInstanceSimulator>>();

        static MaterialPropertyBlock _mBlock;
        internal static MaterialPropertyBlock mBlock
        {
            get
            {
                if (_mBlock == null)
                {
                    _mBlock = new MaterialPropertyBlock();

#if UNITY_5_4_OR_NEWER
                    _mBlock.SetVectorArray(FoliageMeshManager.PROPERTY_ID_WORLDPOSITION, new Vector4[GPUInstancingUtility.MAX_INSTANCING_AMOUNT]);
#endif
                }

                return _mBlock;
            }
        }

        private struct ThreadedRenderingQueueData
        {
            public RenderingQueueReceiver queueReceiver;
            public Vector3 originalCameraPosition;

            public FoliageCore_Chunk[] targetedManagerInstances;
            //public List<Dictionary<int, RenderingQueueInstance>> fetchedInstances;

            public ThreadedRenderingQueueData(RenderingQueueReceiver queueReceiver, Vector3 originalCameraPosition)
            {
                this.queueReceiver = queueReceiver;
                this.originalCameraPosition = originalCameraPosition;

                targetedManagerInstances = queueReceiver.neighbors;
            }
        }

        /// <summary>
        /// Create a rendering queue for the grass.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="fetchedQueueInstances"></param>
        /// <param name="threaded"></param>
        public static void CreateRenderingQueue(RenderingQueueReceiver receiver, bool threaded = true)
        {
            Vector3 originalCameraPosition = receiver.camera.transform.position + UNStandaloneUtility.GetStreamingAdjuster();
            originalCameraPosition.x = Mathf.Floor(originalCameraPosition.x);
            originalCameraPosition.z = Mathf.Floor(originalCameraPosition.z);

            FoliageCore_MainManager.WarmUpGrassMaps(receiver.neighbors, true);

            ThreadedRenderingQueueData threadData = new ThreadedRenderingQueueData(receiver, originalCameraPosition);

            var task = new ThreadTask<ThreadedRenderingQueueData>((ThreadedRenderingQueueData data) =>
            {
                CreateRenderingQueue_Threaded(data.queueReceiver, data.targetedManagerInstances, data.originalCameraPosition);
            }, threadData);

            if (threaded)
            {
                UNThreadManager.instance.RunOnThread(task);
            }
            else
            {
                task.Invoke();
            }
        }

        /// <summary>
        /// Private threaded operation for creating grass queue.
        /// </summary>
        /// <param name="queueReceiver"></param>
        /// <param name="targetedManagerInstances"></param>
        /// <param name="originalCameraPosition"></param>
        /// <param name="fetchedQueueInstances"></param>
        private static void CreateRenderingQueue_Threaded(RenderingQueueReceiver queueReceiver, FoliageCore_Chunk[] targetedManagerInstances, Vector3 originalCameraPosition)
        {
            FoliageCore_Chunk currentManagerInstanceChunk;
            FoliageManagerInstance currentManagerInstance;
            RenderingQueue renderingQueueInstance;
            Vector3 normalizedCameraPosition;

            queueReceiver.threaded_Cold_RenderingCache.Clear();

            for (int i = 0; i < targetedManagerInstances.Length; i++)
            {
                currentManagerInstanceChunk = targetedManagerInstances[i];

                normalizedCameraPosition = originalCameraPosition;

                if (currentManagerInstanceChunk == null || !currentManagerInstanceChunk.isFoliageInstanceAttached
                    || !currentManagerInstanceChunk.attachedFoliageInstance.enabled) continue;

                currentManagerInstance = currentManagerInstanceChunk.GetOrCreateFoliageManagerInstance();

                renderingQueueInstance = new RenderingQueue(currentManagerInstance, currentManagerInstanceChunk, normalizedCameraPosition, GetFetchedInstance());

                if (renderingQueueInstance.queueInstance == null || renderingQueueInstance.queueInstance.Count == 0 || renderingQueueInstance.queueInstanceNull)
                {
                    continue;
                }

                renderingQueueInstance.UpdateDensities(); // update density

                queueReceiver.threaded_Cold_RenderingCache.Add(renderingQueueInstance);
            }

            queueReceiver.threaded_Warmed_RenderingCache = queueReceiver.threaded_Cold_RenderingCache.ToArray();
        }

        /// <summary>
        /// Render the supplied grass queue.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="camera"></param>
        public static void RenderQueue(RenderingQueueReceiver receiver, Camera camera)
        {
            camera.transform.position += UNStandaloneUtility.GetStreamingAdjuster();
            receiver.INTERNAL_ReusableCameraPlanes = UNStandaloneUtility.CalculateFrustumPlanes(camera, receiver.INTERNAL_ReusableCameraPlanes);
            camera.transform.position -= UNStandaloneUtility.GetStreamingAdjuster();

            FoliageCore_MainManager.WarmUpGrassMaps(receiver.neighbors);


            // ASSIGN SETTINGS
            UpdateInternalSettings();
            UpdateInteractionMaterialPropertyBlock(receiver);

            var queue = receiver.renderingQueue;

            if (queue == null)
            {
                return;
            }

            for (int i = 0; i < queue.Length; i++)
            {
                if (queue[i] == null) continue;

                queue[i].Render(camera, receiver.INTERNAL_ReusableCameraPlanes);
            }
        }

        /// <summary>
        /// Render the supplied queue in debug mode (please only call from OnGizmosDraw)
        /// </summary>
        /// <param name="queue"></param>
        public static void RenderQueueDebugMode(RenderingQueue[] queue)
        {
            if (queue == null) return;

            RenderingQueue queueInstance;

            for (int i = 0; i < queue.Length; i++)
            {
                queueInstance = queue[i];

                if (queueInstance == null) return;

                queueInstance.DrawDebug();
            }
        }

        /// <summary>
        /// Update the internal settings of the system like shadows, culling settings etc.
        /// </summary>
        private static void UpdateInternalSettings()
        {
            RenderingQueueMeshInstanceSimulator.SETTINGS_isPlaying = Application.isPlaying;
            RenderingQueueMeshInstanceSimulator.SETTINGS_USE_QUALITY_SETTINGS_SHADOWS = FoliageCore_MainManager.instance.useQualitySettingsShadowDistance;
            RenderingQueueMeshInstanceSimulator.SETTINGS_SHADOWS_DISTANCE = FoliageCore_MainManager.instance.foliageShadowDistance;
            RenderingQueueMeshInstanceSimulator.CULLING_DISALBED = Settings.UNSettings.instance.UN_Foliage_Disable_Culling;
        }

        /// <summary>
        /// This method will update all of the interaction properties on the material property block.
        /// </summary>
        private static void UpdateInteractionMaterialPropertyBlock(RenderingQueueReceiver receiver)
        {
            RenderingQueue_InteractionReceiver interactionReceiver = receiver as RenderingQueue_InteractionReceiver;

            if (interactionReceiver == null) return;

            //mBlock.SetTexture("interactionMap", interactionReceiver.interactionMap.map);
            mBlock.SetVector("interactionCenter", interactionReceiver.interactionCenter);
            mBlock.SetFloat("interactionMapSize", (int)interactionReceiver.interactionMapSize);
            mBlock.SetFloat("interactionMapResolution", (int)interactionReceiver.interactionMapResolution);
        }

        /// <summary>
        /// This will generate the fetched instances for the grass generation process.
        /// </summary>
        private static Dictionary<int, RenderingQueueInstance> GetFetchedInstance()
        {
            Dictionary<int, RenderingQueueInstance> queueInstance;
            FoliagePrototype prototype;

            queueInstance = QueueInstancesPool.Count > 0 ? QueueInstancesPool.Pop() : null;

            if (queueInstance == null)
            {
                queueInstance = new Dictionary<int, RenderingQueueInstance>();

                for (int b = 0; b < FoliageDB.unSortedPrototypes.Count; b++)
                {
                    prototype = FoliageDB.unSortedPrototypes[b];

                    if (!prototype.enabled) continue;

                    queueInstance.Add(prototype.id, null);
                }
            }
            else
            {
                for (int b = 0; b < FoliageDB.unSortedPrototypes.Count; b++)
                {
                    prototype = FoliageDB.unSortedPrototypes[b];

                    if (!prototype.enabled) continue;

                    queueInstance[prototype.id] = null;
                }
            }

            return queueInstance;
        }

        /// <summary>
        /// Cleanup and recycle the queue instances of this simulator so it gets added to the pool.
        /// </summary>
        /// <param name="simulator"></param>
        internal static void CleanupQueueInstances(RenderingQueue[] renderingQueueStash)
        {
            var prototypes = FoliageDB.unSortedPrototypes;
            RenderingQueue renderingQueue;
            RenderingQueueInstance renderingQueueInstance;

            int pID;

            for (int i = 0; i < renderingQueueStash.Length; i++)
            {
                renderingQueue = renderingQueueStash[i];

                QueueInstancesPool.Push(renderingQueue.queueInstance);

                if (renderingQueue.queueInstance != null)
                {
                    for (int prototypeIndex = 0; prototypeIndex < prototypes.Count; prototypeIndex++)
                    {
                        pID = prototypes[prototypeIndex].id;

                        if (!renderingQueue.queueInstance.ContainsKey(pID)) continue;

                        renderingQueueInstance = renderingQueue.queueInstance[pID];

                        if (renderingQueueInstance == null) continue;

                        renderingQueueInstance.simulatedMeshInstances.Clear();

                        FastListPool.Push(renderingQueueInstance.simulatedMeshInstances);

                        renderingQueueInstance.simulatedMeshInstances = null;
                    }

                    renderingQueue.queueInstance = null;
                }
            }
        }
    }

    /// <summary>
    /// An rendering queue.
    /// </summary>
    public class RenderingQueue
    {
        private int RenderingInstancesPoolCount(FoliageManagerInstance mInstance, FoliagePrototype prototype)
        {
            int meshInstancesAmount = 0;
            GPUMesh gpuMesh;

            if (prototype.enabled)
            {
                gpuMesh = mInstance.meshInstances[prototype.id];

                meshInstancesAmount += gpuMesh.LODMeshInstances.meshInstances.Length;
            }

            meshInstancesAmount *= 9; // multiply by the possible amount of manager instances.

            return meshInstancesAmount;
        }

        public const int GENERATION_RADIUS = 3;
        public const int GENERATION_RADIUS_OFFSET = 1;

        public FoliageManagerInstance mInstance;

        public Dictionary<int, RenderingQueueInstance> queueInstance;
        public bool queueInstanceNull = true;

        public RenderingQueue(FoliageManagerInstance mInstance, FoliageCore_Chunk mChunk, Vector3 snapPosition, Dictionary<int, RenderingQueueInstance> fetchedQueueInstance)
        {
            this.mInstance = mInstance;

            queueInstance = fetchedQueueInstance;
            GenerateQueueInstance(snapPosition, mInstance, mChunk);
        }

        internal void UpdateDensities()
        {
            var prototypes = FoliageDB.unSortedPrototypes;
            FoliagePrototype prototype;

            var mapWidth = mInstance.foliageAreaResolutionIntegral;
            float densityMultiplier = FoliageCore_MainManager.instance.density;

            RenderingQueueInstance qInstance;

            for (int i = 0; i < prototypes.Count; i++)
            {
                prototype = prototypes[i];

                if (!prototype.enabled || queueInstance == null) continue;

                qInstance = queueInstance[prototype.id];

                if (qInstance != null)
                {
                    qInstance.UpdateDensities(mapWidth, densityMultiplier);
                }
            }
        }

        internal void Render(Camera camera, Plane[] cameraPlanes)
        {
            UpdateMaterialBlock();

            var prototypes = FoliageDB.unSortedPrototypes;

            if (mInstance == null || camera == null || queueInstance == null)
            {
                return;
            }

            var normalizedCameraPosition = (camera.transform.position + UNStandaloneUtility.GetStreamingAdjuster()) - mInstance.pos;

            normalizedCameraPosition.x = Mathf.Clamp(normalizedCameraPosition.x, 0, FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE - 1);
            normalizedCameraPosition.y = camera.transform.position.y;
            normalizedCameraPosition.z = Mathf.Clamp(normalizedCameraPosition.z, 0, FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE - 1);

            for (var i = 0; i < prototypes.Count; i++)
            {
                var prototype = prototypes[i];

                if (!prototype.enabled) continue;

                //if (queueInstance != null&& prototype != null && queueInstance.Count > prototype.id)
                var currentQueueInstance = queueInstance[prototype.id];
                //else
                //  currentQueueInstance = null;
                if (currentQueueInstance == null)
                {
                    continue;
                }

                currentQueueInstance.Render(RenderingPipielineUtility.mBlock, camera, cameraPlanes, normalizedCameraPosition, prototype.useInstancing);
            }
        }

        public void DrawDebug()
        {
            var prototypes = FoliageDB.unSortedPrototypes;
            RenderingQueueInstance currentQueueInstance;
            FoliagePrototype prototype;

            for (int i = 0; i < prototypes.Count; i++)
            {
                prototype = prototypes[i];

                if (!prototype.enabled) continue;

                currentQueueInstance = queueInstance[prototype.id];

                if (currentQueueInstance == null) continue;

                currentQueueInstance.DrawDebug();
            }
        }

        internal void Destroy()
        {
            mInstance = null;

            var prototypes = FoliageDB.unSortedPrototypes;
            RenderingQueueInstance currentQueueInstance;
            FoliagePrototype prototype;

            for (int i = 0; i < prototypes.Count; i++)
            {
                prototype = prototypes[i];

                if (!prototype.enabled) continue;

                currentQueueInstance = queueInstance[prototype.id];

                if (currentQueueInstance == null) continue;

                currentQueueInstance.simulatedMeshInstances.Clear();
            }

            queueInstance = null;
        }

        private void UpdateMaterialBlock()
        {
            mInstance.UpdateMaterialBlock(RenderingPipielineUtility.mBlock);
            RenderingPipielineUtility.mBlock.SetVector("_StreamingAdjuster", UNStandaloneUtility.GetStreamingAdjuster());

#if UNITY_5_4_OR_NEWER
            RenderingPipielineUtility.mBlock.SetVectorArray(FoliageMeshManager.PROPERTY_ID_FOLIAGE_INTERACTION_TOUCH_BENDING_OBJECTS, TouchBending.bendingTargets);
#endif
        }

        private void GenerateQueueInstance(Vector3 snapPosition, FoliageManagerInstance mInstance, FoliageCore_Chunk mChunk)
        {
            var normalizedPosition = snapPosition;
            snapPosition -= mInstance.pos;
            snapPosition.y = 0;

            var prototypes = FoliageDB.instance._prototypes;
            var instancePosition = new Vector3();

            var grassMap = mInstance.grassMap;

            for (var i = 0; i < prototypes.Count; i++)
            {
                var prototype = prototypes[i];

                UNFastList<RenderingQueueMeshInstanceSimulator> renderingQueueInstances = null;

                if (!prototype.enabled || !mChunk.InBounds(normalizedPosition, prototype.fadeDistance * RenderingPipielineUtility.INSTANCES_OFFSET_MULTIPLIER)) continue; // safe distanc

                var gpuMesh = mInstance.meshInstances[prototype.id];
                var gpuMeshLodsCount = gpuMesh.LODMeshInstances.Count;

                var fadeDistance = (prototype.fadeDistance + FoliageMeshInstance.GENERATION_SAFE_DISTANCE) * 2;

                var relativeSnapPoint = snapPosition;
                relativeSnapPoint.x -= fadeDistance / 2;
                relativeSnapPoint.z -= fadeDistance / 2;

                for (var meshInstancesCount = 0; meshInstancesCount < gpuMeshLodsCount; meshInstancesCount++)
                {
                    var meshInstance = gpuMesh.LODMeshInstances.meshInstances[meshInstancesCount];

                    instancePosition.x = meshInstance.position.x + relativeSnapPoint.x;
                    instancePosition.z = meshInstance.position.z + relativeSnapPoint.z;

                    if ((instancePosition.x + meshInstance.boundsSizeX < 0 || instancePosition.z + meshInstance.boundsSizeZ < 0) || (instancePosition.x > FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE || instancePosition.z > FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE)) continue;

                    if (renderingQueueInstances == null)
                    {
                        renderingQueueInstances = RenderingPipielineUtility.FastListPool.Count > 0 ? RenderingPipielineUtility.FastListPool.Pop() : null;

                        if (renderingQueueInstances == null)
                        {
                            renderingQueueInstances = new UNFastList<RenderingQueueMeshInstanceSimulator>(RenderingInstancesPoolCount(mInstance, prototype));
                        }
                    }

                    renderingQueueInstances.Add(new RenderingQueueMeshInstanceSimulator(instancePosition, meshInstance, mInstance, prototype, snapPosition));
                }

                if (renderingQueueInstances == null || renderingQueueInstances.Count == 0)
                {
                    continue;
                }

                var pipelineInstance =
                    new RenderingQueueInstance(mInstance, prototype, gpuMesh, grassMap)
                    {
                        simulatedMeshInstances = renderingQueueInstances
                    };

                queueInstance[prototype._foliageID_INTERNAL] = pipelineInstance;
                queueInstanceNull = false;
            }

            if (queueInstance.Count == 0)
            {
                queueInstance = null;
            }
        }
    }

    /// <summary>
    /// An rendering queue instance.
    /// </summary>
    public class RenderingQueueInstance
    {
        internal FoliageManagerInstance mInstance;
        internal FoliagePrototype prototype;
        internal FoliageGrassMap foliageGrassMap;
        private GPUMesh gpuMesh;
        private byte maxPrototypeDensity;

        internal UNFastList<RenderingQueueMeshInstanceSimulator> simulatedMeshInstances;
        internal Dictionary<Mesh, GPUInstancing_StackInstance> gpuInstancingStash = null;

        public static int count = 0;

        public RenderingQueueInstance(FoliageManagerInstance mInstance, FoliagePrototype prototype, GPUMesh gpuMesh, FoliageGrassMap grassMap)
        {
            this.mInstance = mInstance;
            this.prototype = prototype;
            this.gpuMesh = gpuMesh;

            maxPrototypeDensity = prototype._maxGeneratedDensity;
            simulatedMeshInstances = null;
            foliageGrassMap = grassMap;

            if (prototype.useInstancing)
            {
                gpuInstancingStash = GPUInstancingUtility.CreateInstancingStack(gpuMesh); // update instancing stuck.
            }
        }

        internal void UpdateDensities(int mapWidth, float densityMultiplier)
        {
            int simulatedMeshInstancesCount = simulatedMeshInstances.Count;
            var mapPixels = foliageGrassMap.mapPixels;

            for (int i = 0; i < simulatedMeshInstancesCount; i++)
            {
                simulatedMeshInstances.arrayAllocation[i].UpdateDensity(gpuMesh, maxPrototypeDensity, mInstance, mapWidth, densityMultiplier, prototype, gpuInstancingStash);
            }
        }
        internal void Render(MaterialPropertyBlock mBlock, Camera camera, Plane[] cameraPlanes, Vector3 normalizedCameraPosition, bool instancingEnabled)
        {
            mBlock.SetTexture(FoliageMeshManager.PROPERTY_ID_GRASSMAP, mInstance.grassMap.map);
            mBlock.SetFloat(FoliageMeshManager.PROPERTY_ID_PROTOTYPE, (byte)prototype.id);

            if (simulatedMeshInstances == null) return;

            if (instancingEnabled)
            {
                GPUInstancingUtility.ResetInstancingStack(gpuInstancingStash, gpuMesh);
            }

            for (var i = 0; i < simulatedMeshInstances.Count; i++)
            {
                simulatedMeshInstances.arrayAllocation[i].Render(mBlock, camera, cameraPlanes, normalizedCameraPosition, instancingEnabled);
            }

            if (instancingEnabled)
            {
                GPUInstancingUtility.RenderInstancingStack(gpuInstancingStash, gpuMesh, prototype, mBlock, camera);
            }
        }
        internal void DrawDebug()
        {
            for (int i = 0; i < simulatedMeshInstances.Count; i++)
            {
                simulatedMeshInstances.arrayAllocation[i].DrawDebug();
            }
        }
    }

    /// <summary>
    /// An imposter struct to simulate an instance.
    /// </summary>
    public struct RenderingQueueMeshInstanceSimulator
    {
        internal static bool SETTINGS_isPlaying = false;
        internal static bool SETTINGS_USE_QUALITY_SETTINGS_SHADOWS = false;
        internal static float SETTINGS_SHADOWS_DISTANCE = 0;
        internal static bool CULLING_DISALBED = false;

        private static Vector3 REUSABLE_VECTOR3_INSTANCE = new Vector3();
        private static Vector4 REUSABLE_VECTOR4_INSTANCE = new Vector4();

        public float x, z;
        public float worldX, worldZ;

        [SerializeField]
        private FoliageMeshInstance meshInstance;

        [SerializeField]
        private byte _density;
        public byte density
        {
            get
            {
                return _density;
            }
        }

        [SerializeField]
        private Mesh _mesh;
        public Mesh mesh
        {
            get
            {
                return _mesh;
            }
        }

        private GPUInstancing_StackInstance _instancingStackInstance;
        public GPUInstancing_StackInstance instancingStackInstance
        {
            get
            {
                return _instancingStackInstance;
            }
        }

        private int lodIndex;

        public float distanceToCenter;

        public RenderingQueueMeshInstanceSimulator(Vector3 position, FoliageMeshInstance _meshInstance, FoliageManagerInstance mInstance, FoliagePrototype _prototype, Vector4 worldCameraPosition)
        {
            x = position.x;
            z = position.z;

            worldX = x + mInstance._pos.x;
            worldZ = z + mInstance._pos.z;

            meshInstance = _meshInstance;
            _mesh = null;
            _density = 0;
            _instancingStackInstance = null;

            lodIndex = 0;

            distanceToCenter = meshInstance.distanceToCenter; // assign first before using the method.
        }

        public RenderingQueueMeshInstanceSimulator UpdateDensity(GPUMesh gpuMesh, byte maxDensity, FoliageManagerInstance mInstance, int mapWidth, float densityMultiplier, FoliagePrototype prototype, Dictionary<Mesh, GPUInstancing_StackInstance> stackInstances)
        {
            byte newDensity = meshInstance.GetDensity(x, z, maxDensity, mInstance.grassMap, mapWidth);
            lodIndex = 0;
            float lodMultiplier = prototype.GetLODMultiplier(distanceToCenter, ref lodIndex);

            int interpolatedDensity = (int)(newDensity * densityMultiplier);

            if (interpolatedDensity > 0)
            {
                interpolatedDensity = Mathf.CeilToInt(interpolatedDensity * lodMultiplier);
            }

            _density = newDensity;

            if (_density == 0) return this;

            int meshLODIndex = gpuMesh.GetMesh(interpolatedDensity, prototype);

            if (meshLODIndex == -1)
            {
                _mesh = null;
                return this;
            }

            if (gpuMesh.meshLODsCount == 1) // 0 lods
            {
                _mesh = gpuMesh.meshesCache[0, meshLODIndex, meshInstance.meshIndex];
            }
            else
            {
                _mesh = gpuMesh.meshesCache[lodIndex, meshLODIndex, meshInstance.meshIndex];
            }

            if (stackInstances != null)
            {
                _instancingStackInstance = stackInstances[_mesh];
            }

            return this;
        }

        public void Render(MaterialPropertyBlock mBlock, Camera camera, Plane[] cameraPlanes, Vector3 normalizedCameraPosition, bool instancing)
        {
            REUSABLE_VECTOR3_INSTANCE.x = worldX;
            REUSABLE_VECTOR3_INSTANCE.z = worldZ;

            if (_density == 0 || (!CULLING_DISALBED && !meshInstance.CheckViewPort(cameraPlanes, REUSABLE_VECTOR3_INSTANCE, normalizedCameraPosition.y))) return;

            REUSABLE_VECTOR4_INSTANCE.x = x;
            REUSABLE_VECTOR4_INSTANCE.z = z;

            if (instancing && _instancingStackInstance != null) // add to rendering queue
            {
                _instancingStackInstance.Add(FoliageMeshInstance.GENERATION_OPTIMIZATION_PRE_GENERATION_MATRIX_IDENTITY, REUSABLE_VECTOR4_INSTANCE); // add instance to stack.
            }
            else // draw instantly
            {
                mBlock.SetVector(FoliageMeshManager.PROPERTY_ID_WORLDPOSITION, REUSABLE_VECTOR4_INSTANCE);

                meshInstance.DrawAndUpdate(REUSABLE_VECTOR4_INSTANCE, _mesh, SETTINGS_isPlaying ? camera : null, normalizedCameraPosition, mBlock, SETTINGS_USE_QUALITY_SETTINGS_SHADOWS, SETTINGS_SHADOWS_DISTANCE);
            }
        }

        public void DrawDebug()
        {
            if (density == 0) return;

            Gizmos.color = FoliageLODLevel.lodGUIColors[lodIndex];

            Vector3 boundsSize = meshInstance.cullBounds.size;
            boundsSize.y = 0.1f;

            Vector3 pos = new Vector3(worldX, 10000, worldZ) + boundsSize / 2;

            Ray ray = new Ray(pos, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                pos.y = hit.point.y;
            }
            else
            {
                pos.y = 2;
            }

            Gizmos.DrawCube(pos, boundsSize);
        }
    }

    /// <summary>
    /// An instance that can handle a foliage rendering queue.
    /// </summary>
    public class RenderingQueueReceiver
    {
        [System.NonSerialized]
        RenderingQueue[] _renderingQueue;
        internal RenderingQueue[] renderingQueue
        {
            get
            {
                if (threaded_Warmed_RenderingCache != null)
                {
                    renderingQueue = threaded_Warmed_RenderingCache;

                    threaded_Warmed_RenderingCache = null;
                }

                return _renderingQueue;
            }
            set
            {
                if (_renderingQueue != value)
                {
                    if (_renderingQueue != null)
                    {
                        RenderingPipielineUtility.CleanupQueueInstances(_renderingQueue);
                    }

                    _renderingQueue = value;
                }
            }
        }

        #region Threading Operation
        /// <summary>
        /// Read by the system after the threading has been finished.
        /// </summary>
        [System.NonSerialized]
        internal RenderingQueue[] threaded_Warmed_RenderingCache;

        /// <summary>
        /// Poppulated by the thread.
        /// </summary>
        [System.NonSerialized]
        internal List<RenderingQueue> threaded_Cold_RenderingCache = new List<RenderingQueue>();
        #endregion

        private FoliageCore_Chunk[] _neighbors;
        public FoliageCore_Chunk[] neighbors
        {
            get
            {
                if (_neighbors == null)
                {
                    _neighbors = UNStandaloneUtility.GetFoliageChunksNeighbors(transform.position - FoliageCore_MainManager.instance.transform.position, _neighbors);
                }

                return _neighbors;
            }
            set
            {
                _neighbors = value;
            }
        }

        internal Plane[] INTERNAL_ReusableCameraPlanes = new Plane[6];

        public Transform transform;
        public Camera camera;

        private Vector2 lastCheckedPosition;
        private bool checkedPosition = false;

        /// <summary>
        /// Check if the instance moved.
        /// This will - 
        /// 
        /// 1) Update Neighboors
        /// 2) Update and recalculate the densities of the grass.
        /// </summary>
        public void CheckPositionChange()
        {
            Vector2 tPosition2D = new Vector2(transform.position.x, transform.position.z);

            if (!checkedPosition || Vector2.Distance(lastCheckedPosition, tPosition2D) >= 1 + FoliageMeshInstance.GENERATION_SAFE_DISTANCE)
            {
                _neighbors = UNStandaloneUtility.GetFoliageChunksNeighbors(transform.position + UNStandaloneUtility.GetStreamingAdjuster() - FoliageCore_MainManager.instance.transform.position, _neighbors);

                CreateInstances(checkedPosition);

                lastCheckedPosition = tPosition2D;
                checkedPosition = true;

                OnUpdated();
            }
        }

        /// <summary>
        /// On update grass.
        /// </summary>
        protected virtual void OnUpdated()
        {
        }

        /// <summary>
        /// Create & Update Grass Instances
        /// </summary>
        /// <param name="threaded"></param>
        public void CreateInstances(bool threaded)
        {
            RenderingPipielineUtility.CreateRenderingQueue(this, threaded);
        }

        /// <summary>
        /// Reset the density of the grass instances
        /// </summary>
        public void ResetDensity()
        {
            RenderingQueue queue;

            if (_renderingQueue == null) return;

            for (int i = 0; i < _renderingQueue.Length; i++)
            {
                queue = _renderingQueue[i];
                queue.UpdateDensities();
            }
        }
    }
}

