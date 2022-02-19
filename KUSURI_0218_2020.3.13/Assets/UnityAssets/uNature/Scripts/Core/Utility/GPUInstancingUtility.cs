using System.Collections.Generic;
using UnityEngine;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Utility
{
    public static class GPUInstancingUtility
    {
        public const int MAX_INSTANCING_AMOUNT = 1023;

        public static Dictionary<Mesh, GPUInstancing_StackInstance> CreateInstancingStack(GPUMesh gpuMesh)
        {
            Dictionary<Mesh, GPUInstancing_StackInstance> stackInstance = new Dictionary<Mesh, GPUInstancing_StackInstance>();

            for (int a = 0; a < gpuMesh.meshLODsCount; a++)
            {
                for (int b = 0; b < gpuMesh.densityLODsCount; b++)
                {
                    for (int c = 0; c < gpuMesh.meshesCount; c++)
                    {
                        stackInstance.Add(gpuMesh.meshesCache[a, b, c], new GPUInstancing_StackInstance());
                    }
                }
            }

            return stackInstance;
        }
        public static void ResetInstancingStack(Dictionary<Mesh, GPUInstancing_StackInstance> stackInstance, GPUMesh gpuMesh)
        {
            if (stackInstance == null) return;

            for (int a = 0; a < gpuMesh.meshLODsCount; a++)
            {
                for (int b = 0; b < gpuMesh.densityLODsCount; b++)
                {
                    for (int c = 0; c < gpuMesh.meshesCount; c++)
                    {
                        stackInstance[gpuMesh.meshesCache[a, b, c]].Clear();
                    }
                }
            }
        }

        public static void RenderInstancingStack(Dictionary<Mesh, GPUInstancing_StackInstance> stackInstance, GPUMesh gpuMesh, FoliagePrototype prototype, MaterialPropertyBlock mBlock, Camera camera)
        {
            #if UNITY_5_5_OR_NEWER
            if (stackInstance == null) return;


            var castShadows = prototype.castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            var receiveShadows = prototype.receiveShadows;
            var mat = prototype.FoliageInstancedMeshData.mat;

            for (var a = 0; a < gpuMesh.meshLODsCount; a++)
            {
                for (var b = 0; b < gpuMesh.densityLODsCount; b++)
                {
                    for (var c = 0; c < gpuMesh.meshesCount; c++)
                    {
                        var mesh = gpuMesh.meshesCache[a, b, c];
                        var stack = stackInstance[mesh];

                        if (stack.VECTOR_STASH.Count == 0) continue; // don't use empty array.

                        mBlock.SetVectorArray(FoliageMeshManager.PROPERTY_ID_WORLDPOSITION, stack.VECTOR_STASH);
                        Graphics.DrawMeshInstanced(mesh, 0, mat, stackInstance[mesh].MATRIX_STASH, mBlock, castShadows, receiveShadows, prototype.renderingLayer, RenderingQueueMeshInstanceSimulator.SETTINGS_isPlaying ? camera : null);
                    }
                }
            }
            #endif
        }
    }

    public class GPUInstancing_StackInstance
    {
        private List<Matrix4x4> matrixStash = new List<Matrix4x4>();
        private List<Vector4> vectorStash = new List<Vector4>();

        public List<Matrix4x4> MATRIX_STASH
        {
            get
            {
                return matrixStash;
            }
        }
        public List<Vector4> VECTOR_STASH
        {
            get
            {
                return vectorStash;
            }
        }

        public void Clear()
        {
            matrixStash.Clear();
            vectorStash.Clear();
        }

        public void Add(Matrix4x4 matrix, Vector4 worldPosition)
        {
            matrixStash.Add(matrix);
            vectorStash.Add(worldPosition);
        }
    }
}
