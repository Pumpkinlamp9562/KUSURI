using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Utility
{
    /// <summary>
    /// An utility class for batching items.
    /// </summary>
    public static class UNBatchUtility
    {
        static Vector3 upNormal = new Vector3(0, 1, 0);

        public static void CombineMeshes(List<UNCombineInstance> instances, Mesh mesh, UNFoliageMeshData meshData)
        {
            if (instances.Count == 0)
            {
                mesh.Clear(); // clear mesh as its null at the moment.
                return;
            }

            int instancesCount = instances.Count;

            Vector3[] vertices = new Vector3[meshData.verticesLength * instancesCount];
            Vector3[] normals = new Vector3[meshData.normalsLength * instancesCount];
            Vector2[] uv1s = new Vector2[meshData.uvLength * instancesCount];
            Vector2[] uv2s = new Vector2[meshData.verticesLength * instancesCount];
            Vector2[] uv3s = new Vector2[meshData.verticesLength * instancesCount];
            Vector2[] uv4s = new Vector2[meshData.verticesLength * instancesCount];
            int[] subMeshes = new int[meshData.trianglesLength * instancesCount];

            int verticesOffset = 0;
            int normalsOffset = 0;
            int uv1sOffset = 0;
            int uv2sOffset = 0;
            int subMeshesOffset = 0;

            for (int i = 0; i < instances.Count; i++)
            {
                MergeMesh(instances[i], i, vertices, normals, uv1s, uv2s, uv3s, uv4s, subMeshes, meshData, verticesOffset, normalsOffset, uv1sOffset, uv2sOffset, subMeshesOffset);

                verticesOffset += meshData.verticesLength;
                normalsOffset += meshData.normalsLength;
                uv1sOffset += meshData.uvLength;
                uv2sOffset += meshData.verticesLength;
                subMeshesOffset += meshData.trianglesLength;
            }

            //assign data
            mesh.Clear();

            mesh.vertices = vertices;

            mesh.normals = normals;

            mesh.uv = uv1s;
            mesh.uv2 = uv2s;
            mesh.uv3 = uv3s;
            mesh.uv4 = uv4s;

            mesh.SetTriangles(subMeshes, 0);

            mesh.RecalculateBounds();

            mesh.bounds = new Bounds(mesh.bounds.center, new Vector3(Mathf.Clamp(mesh.bounds.size.x, 1, int.MaxValue), 0, Mathf.Clamp(mesh.bounds.size.z, 1, int.MaxValue)));
        }

        private static void MergeMesh(UNCombineInstance batchInstance, int id, Vector3[] vertices, Vector3[] normals, Vector2[] uv1s, Vector2[] uv2s, Vector2[] uv3s, Vector2[] uv4s, int[] subMeshes, UNFoliageMeshData meshData, int verticesOffset, int normalsOffset, int uv1sOffset, int uv2sOffset, int subMeshesOffset)
        {
            Vector3 centerMesh = batchInstance.transform.MultiplyPoint3x4(Vector3.zero);

            for (int i = 0; i < meshData.verticesLength; i++)
            {
                vertices[verticesOffset + i] = centerMesh;
            }

            for (int i = 0; i < meshData.normalsLength; i++)
            {
                normals[normalsOffset + i] = upNormal;
            }

            for (int i = 0; i < meshData.uvLength; i++)
            {
                uv1s[uv1sOffset + i] = meshData.uv[i];
            }

            //add centers
            for (int i = 0; i < meshData.verticesLength; i++)
            {
                uv2s[uv2sOffset + i] = new Vector2(meshData.vertices[i].x, meshData.vertices[i].y);
                uv3s[uv2sOffset + i] = batchInstance.densityOffset;
                uv4s[uv2sOffset + i] = new Vector2(meshData.vertices[i].z, batchInstance.density);
            }

            for (int t = 0; t < meshData.trianglesLength; t++)
            {
                subMeshes[subMeshesOffset + t] = meshData.triangles[t] + verticesOffset;
            }
        }
    }

    public struct UNCombineInstance
    {
        public Matrix4x4 transform;
        public Mesh mesh;

        public Vector2 densityOffset;

        public int density;

        public UNCombineInstance(Matrix4x4 transform, Mesh mesh, float spread, int density, int id)
        {
            this.transform = transform;
            this.mesh = mesh;

            this.density = density;

            #if UNITY_5_4_OR_NEWER
            Random.InitState(density * 1000 * (id + 1));
            #else
            Random.seed = density * 1000 * (id + 1);
            #endif

            densityOffset.x = Random.Range(-1f, 1f);
            densityOffset.y = Random.Range(-1f, 1f);
        }
    }
}
