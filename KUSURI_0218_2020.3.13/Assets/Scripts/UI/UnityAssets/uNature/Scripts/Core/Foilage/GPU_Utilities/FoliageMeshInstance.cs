using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

using uNature.Core.Utility;

namespace uNature.Core.FoliageClasses
{
    public class FoliageMeshInstance
    {
        internal const int GENERATION_VERTICES_MAX = 65535;
        internal const int GENERATION_SAFE_DISTANCE = 3;
        internal const int GENERATION_SAFE_DISTANCE_RADIUS_ADJUSTER = 1;

        /// <summary>
        /// Pre generate the matrix4x4 identity to optimize the code as its being generated each frame for each mesh instance.
        /// </summary>
        private readonly static Vector3 GENERATION_OPTIMIZATION_PRE_GENERATED_VECTOR3_ZERO = Vector3.zero;
        private readonly static Vector3 GENERATION_OPTIMIZATION_PRE_GENERATED_VECTOR3_ONE = Vector3.one;
        private readonly static Quaternion GENERATION_OPTIMIZATION_PRE_GENERATED_QUATERNION_IDENTITY = Quaternion.identity;
        internal readonly static Matrix4x4 GENERATION_OPTIMIZATION_PRE_GENERATION_MATRIX_IDENTITY = Matrix4x4.identity;
        private readonly static List<UNCombineInstance> REUSABLE_COMBINE_INSTANCES = new List<UNCombineInstance>();

        #region Variables
        public FoliagePrototype prototype;

        public Vector3 position;
        public Vector3 center;
        public Vector3 GetPosition(Vector3 pos)
        {
            return pos + position;
        }

        [System.NonSerialized]
        public Mesh mesh;

        [System.NonSerialized]
        public Bounds cullBounds;

        [System.NonSerialized]
        public int boundsSizeX;

        [System.NonSerialized]
        public int boundsSizeZ;

        [System.NonSerialized]
        public Vector3 boundsExtents;

        [System.NonSerialized]
        public int meshIndex;

        [System.NonSerialized]
        private float _distanceToCenter;
        public float distanceToCenter
        {
            get
            {
                return _distanceToCenter;
            }
            private set
            {
                _distanceToCenter = value;
            }
        }
        
        #endregion

        /// <summary>
        /// Create a new foliage mesh.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="position"></param>
        /// <param name="latestCreatedMesh"></param>
        /// <returns></returns>
        internal static FoliageMeshInstance CreateFoliageMeshInstance(FoliagePrototype prototype, Vector3 position, int meshIndex)
        {
            FoliageMeshInstance instance = new FoliageMeshInstance();

            instance.prototype = prototype;
            instance.position = position;
            instance.meshIndex = meshIndex;
            
            return instance;
        }

        /// <summary>
        /// Generates the mesh instances
        /// </summary>
        /// <param name="prototypeIndex"></param>
        /// <param name="densities"></param>
        /// <param name="meshGroup"></param>
        /// <param name="resolution"></param>
        /// <returns>
        /// 1RD Dimension -> mesh lods index
        /// 2RD Dimension -> density lods index
        /// 3RD Dimension -> Mesh index
        /// </returns>
        public static Mesh[,,] CreateFoliageInstances(int prototypeIndex, List<byte> densities, out FoliageMeshInstancesGroup meshGroup, FoliageResolutions resolution)
        {
            #region Pre
            meshGroup = new FoliageMeshInstancesGroup();

            FoliagePrototype prototype = FoliageDB.sortedPrototypes[prototypeIndex];
            float resMultiplier = (float)resolution / FoliageCore_MainManager.FOLIAGE_INSTANCE_AREA_SIZE;
            int size = (prototype.fadeDistance + GENERATION_SAFE_DISTANCE) * 2;
            var lodsData = prototype.GetLODsData();

            byte currentDensity;

            int densityCount = densities.Count;

            List<byte> densitiesToRemove = new List<byte>();

            for (int i = 0; i < densityCount; i++)
            {
                currentDensity = densities[i];

                densities[i] = (byte)Mathf.CeilToInt(currentDensity * resMultiplier);

                currentDensity = densities[i];

                if (densities.FindAll(delegate (byte a) { return a == currentDensity; }).Count > 1) // if already exists
                {
                    densitiesToRemove.Add(currentDensity);
                    continue;
                }
            }

            for (int i = 0; i < densitiesToRemove.Count; i++)
            {
                densities.Remove(densitiesToRemove[i]);
            }

            int realDensity = densities[0];

            int maxCapability = prototype.maxFoliageCapability / realDensity;
            maxCapability = Mathf.FloorToInt(Mathf.Sqrt(maxCapability));

            float unitsNeeded = (float)size / maxCapability;
            int unitsNeededFloored = Mathf.FloorToInt(unitsNeeded);
            int unitsNeededCeiled = Mathf.CeilToInt(unitsNeeded);

            int unitCoverage = Mathf.FloorToInt(size / unitsNeeded);
            int remainderCoverage = size - (unitCoverage * unitsNeededFloored);
            #endregion

            #region Meshes
            Mesh[,,] lodMeshes = new Mesh[lodsData.Count, densities.Count, remainderCoverage > 0 ? 2 : 1];
            UNFoliageMeshData meshData;

            for (int lodMeshesIndex = 0; lodMeshesIndex < lodsData.Count; lodMeshesIndex++)
            {
                meshData = lodsData[lodMeshesIndex];

                lodMeshes = CreateMeshes(meshData, prototype, lodMeshes, lodMeshesIndex, 0, densities, unitCoverage);

                if (remainderCoverage > 0)
                {
                    lodMeshes = CreateMeshes(meshData, prototype, lodMeshes, lodMeshesIndex, 1, densities, remainderCoverage);
                }
            }
            #endregion

            #region mInstances
            Vector3 instancePosition = new Vector3();
            Vector3 instancesCenter = new Vector3(size / 2, 0, size / 2);

            for (int x = 0; x < unitsNeededCeiled; x++)
            {
                instancePosition.x = x * unitCoverage;

                for (int z = 0; z < unitsNeededCeiled; z++)
                {
                    instancePosition.z = z * unitCoverage;

                    meshGroup.Add(CreateFoliageMeshInstance(prototype, instancePosition, x > unitsNeededFloored || z > unitsNeededFloored ? 1 : 0));
                }
            }
            meshGroup.Finish();
            #endregion

            Mesh currentMesh;
            bool hasRemainder;
            int meshesCount;

            for (int lodsIndex = 0; lodsIndex < lodsData.Count; lodsIndex++)
            {
                hasRemainder = remainderCoverage > 0;
                meshesCount = hasRemainder ? 2 : 1;

                for (int count = 0; count < meshesCount; count++)
                {
                    for (int lodIndex = 0; lodIndex < densities.Count; lodIndex++)
                    {
                        currentDensity = densities[lodIndex];

                        currentMesh = lodMeshes[lodsIndex, lodIndex, count];

                        for (int i = 0; i < meshGroup.Count; i++)
                        {
                            var mInstance = meshGroup.meshInstances[i];

                            if (mInstance.meshIndex == count)
                            {
                                mInstance.UpdateMeshBounds(currentMesh, instancesCenter);
                            }
                        }

                        currentMesh.name = string.Format("uNature Mesh ({0}) ({1})", currentDensity, currentMesh.vertexCount);
                        currentMesh.bounds = FoliageCore_MainManager.FOLIAGE_MAIN_AREA_BOUNDS;
                    }
                }
            }

            return lodMeshes;
        }

        /// <summary>
        /// Create meshes for a certain coverage.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="densities"></param>
        /// <param name="coverage"></param>
        /// <param name="reminder"></param>
        /// <returns></returns>
        private static Mesh[,,] CreateMeshes(UNFoliageMeshData meshData, FoliagePrototype prototype, Mesh[,,] meshes, int lodMeshIndex, int meshIndex, List<byte> densities, int coverageUnits)
        {
            int density;
            for (int i = 0; i < densities.Count; i++)
            {
                density = densities[i];

                meshes[lodMeshIndex, i, meshIndex] = FillMesh(meshData, prototype, density, coverageUnits);
            }

            return meshes;
        }

        /// <summary>
        /// Fill a mesh with information.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="density"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        private static Mesh FillMesh(UNFoliageMeshData meshData, FoliagePrototype prototype, int density, int units)
        {
            Mesh mesh = FoliageMeshManager.CreateNewMesh();
            Vector3 position = new Vector3();
            position.y = prototype.FoliageInstancedMeshData.offset.y;

            REUSABLE_COMBINE_INSTANCES.Clear();

            Matrix4x4 matrix;

            var prototypeMesh = prototype.FoliageInstancedMeshData.mesh;
            float spread = prototype.spread;
            int prototypeID = prototype.id;

            for (int x = 0; x < units; x++)
            {
                for (int z = 0; z < units; z++)
                {
                    position.x = x;
                    position.z = z;

                    matrix = Matrix4x4.TRS(position, GENERATION_OPTIMIZATION_PRE_GENERATED_QUATERNION_IDENTITY, GENERATION_OPTIMIZATION_PRE_GENERATED_VECTOR3_ONE);

                    for (int densityIndex = 1; densityIndex <= density; densityIndex++)
                    {
                        REUSABLE_COMBINE_INSTANCES.Add(new UNCombineInstance(matrix, prototypeMesh, spread, densityIndex, prototypeID));
                    }
                }
            }

            UNBatchUtility.CombineMeshes(REUSABLE_COMBINE_INSTANCES, mesh, meshData);

            return mesh;
        }

        /// <summary>
        /// get density of the mesh instance.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public byte GetDensity(float positionX, float positionZ, byte maxDensity, FoliageGrassMap map, int mapWidth)
        {
            byte density = 0;

            var posX = UNMath.iClamp((int)positionX - 1, 0, mapWidth);
            var posZ = UNMath.iClamp((int)positionZ - 1, 0, mapWidth);

            var targetX = UNMath.iClampMax(posX + boundsSizeX + 2, mapWidth);
            var targetZ = UNMath.iClampMax(posZ + boundsSizeZ + 2, mapWidth);

            for (var x = posX; x < targetX; x++)
            {
                for (var z = posZ; z < targetZ; z++)
                {
                    var currentDensity = map.GetPrototypeDensity(x, z, prototype.id);

                    if (currentDensity >= maxDensity)
                    {
                        return maxDensity;
                    }

                    if (currentDensity > density)
                    {
                        density = currentDensity;
                    }
                }
            }

            return density;
        }

        /// <summary>
        /// Updates the bounds of the mesh instance from a mesh.
        /// </summary>
        /// <param name="mesh"></param>
        internal void UpdateMeshBounds(Mesh mesh, Vector3 instancesCenter)
        {
            if (mesh == null || boundsSizeX == 0 || boundsSizeZ == 0)
            {
                this.mesh = mesh;

                cullBounds.size = new Vector3(mesh.bounds.size.x, 100, mesh.bounds.size.z);
                boundsSizeX = Mathf.CeilToInt(mesh.bounds.size.x);
                boundsSizeZ = Mathf.CeilToInt(mesh.bounds.size.z);
                boundsExtents = cullBounds.extents;

                center = position + boundsExtents;
                center.y = 0;

                distanceToCenter = Vector3.Distance(instancesCenter, center);

                float radius = Vector3.Distance(center, position);

                distanceToCenter -= radius;
            }
        }

        /// <summary>
        /// Check if in the view port.
        /// </summary>
        /// <returns></returns>
        internal bool CheckViewPort(Plane[] cameraPlanes, Vector3 position, float height)
        {
            position.x += boundsExtents.x;
            position.y = height;
            position.z += boundsExtents.z;

            cullBounds.center = position;

            return GeometryUtility.TestPlanesAABB(cameraPlanes, cullBounds);
        }

        /// <summary>
        /// Destroy a mesh instance
        /// </summary>
        internal void Destroy()
        {
            prototype = null;

            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Draw this mesh instance.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="mesh"></param>
        /// <param name="mat"></param>
        /// <param name="camera"></param>
        /// <param name="cameraPos"></param>
        /// <param name="prototype"></param>
        /// <param name="matBlock"></param>
        /// <param name="useQualitySettingsShadows"></param>
        /// <param name="shadowDistance"></param>
        internal void DrawAndUpdate(Vector3 position, Mesh mesh, Camera camera, Vector3 cameraPos, MaterialPropertyBlock matBlock, bool useQualitySettingsShadows, float shadowDistance)
        {
            if (prototype == null) return;

            ShadowCastingMode castMode = prototype.castShadows && (camera == null || (useQualitySettingsShadows || Vector3.Distance(position, cameraPos) < shadowDistance)) ? ShadowCastingMode.On : ShadowCastingMode.Off;

            Graphics.DrawMesh(mesh, GENERATION_OPTIMIZATION_PRE_GENERATED_VECTOR3_ZERO, GENERATION_OPTIMIZATION_PRE_GENERATED_QUATERNION_IDENTITY, prototype.FoliageInstancedMeshData.mat, prototype.renderingLayer, camera, 0, matBlock, castMode, prototype.receiveShadows, null);
        }
    }
}
