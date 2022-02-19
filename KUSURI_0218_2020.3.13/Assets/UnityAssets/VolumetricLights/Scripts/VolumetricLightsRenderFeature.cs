//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumetricLights {
    public class VolumetricLightsRenderFeature : ScriptableRendererFeature {

        class VolumetricLightsRenderPass : ScriptableRenderPass {

            enum Pass {
                BlurHorizontal = 0,
                BlurVertical = 1,
                BlurVerticalAndBlend = 2
            }

            static class ShaderParams {
                public static int lightBuffer = Shader.PropertyToID("_LightBuffer");
                public static int mainTex = Shader.PropertyToID("_MainTex");
                public static int blurRT = Shader.PropertyToID("_BlurTex");
                public static int blurRT2 = Shader.PropertyToID("_BlurTex2");
                public static int blurScale = Shader.PropertyToID("_BlurScale");
                public static int BlendDest = Shader.PropertyToID("_BlendDest");
                public static int BlendSrc = Shader.PropertyToID("_BlendSrc");
                public static int Brightness = Shader.PropertyToID("_Brightness");
            }

            ScriptableRenderer renderer;
            Material mat;
            RenderTextureDescriptor rtSourceDesc, rtBlurDesc;
            static Matrix4x4 matrix4x4identity = Matrix4x4.identity;
            VolumetricLightsRenderFeature settings;

            public void Setup(Shader shader, ScriptableRenderer renderer, VolumetricLightsRenderFeature settings) {
                this.settings = settings;
                this.renderPassEvent = settings.renderPassEvent;
                this.renderer = renderer;
                if (mat == null) {
                    mat = CoreUtils.CreateEngineMaterial(shader);
                }

                switch (settings.blendMode) {
                    case BlendMode.Additive:
                        mat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.One);
                        break;
                    case BlendMode.Blend:
                        mat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        break;
                    case BlendMode.PreMultiply:
                        mat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.One);
                        break;
                }
                mat.SetFloat(ShaderParams.Brightness, settings.brightness);
                mat.SetFloat(ShaderParams.blurScale, settings.blurSpread);

            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                rtSourceDesc = cameraTextureDescriptor;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

                if (settings.blurPasses < 1) {
                    Cleanup();
                    return;
                }

                RenderTargetIdentifier source = renderer.cameraColorTarget;

                var cmd = CommandBufferPool.Get("Volumetric Lights Render Feature");

                rtBlurDesc = rtSourceDesc;
                rtBlurDesc.colorFormat = RenderTextureFormat.ARGB32;
                rtBlurDesc.depthBufferBits = 0;
                rtBlurDesc.useMipMap = false;

                int renderCount = 0;
                Camera cam = renderingData.cameraData.camera;
                foreach (VolumetricLight vl in VolumetricLight.volumetricLights) {
                    if (vl != null && vl.meshRenderer != null) {
                        vl.ToggleVolumetrics(false);
                        if (vl.meshRenderer.isVisible && (cam.cullingMask & (1 << vl.gameObject.layer)) != 0 && vl.material != null)
                        {
                            if (renderCount++ == 0)
                            {
                                cmd.GetTemporaryRT(ShaderParams.lightBuffer, rtBlurDesc, FilterMode.Bilinear);
                                cmd.SetRenderTarget(ShaderParams.lightBuffer, source);
                                cmd.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));
                            }
                            cmd.DrawRenderer(vl.meshRenderer, vl.material);
                        }
                    }
                }

                if (renderCount > 0)
                {
                    rtBlurDesc.width = Mathf.Max(1, rtSourceDesc.width / settings.blurDownscaling);
                    rtBlurDesc.height = Mathf.Max(1, rtSourceDesc.height / settings.blurDownscaling);
                    cmd.GetTemporaryRT(ShaderParams.blurRT, rtBlurDesc, FilterMode.Bilinear);
                    cmd.GetTemporaryRT(ShaderParams.blurRT2, rtBlurDesc, FilterMode.Bilinear);
                    FullScreenBlit(cmd, ShaderParams.lightBuffer, ShaderParams.blurRT, mat, (int)Pass.BlurHorizontal);
                    for (int k = 0; k < settings.blurPasses - 1; k++) {
                        FullScreenBlit(cmd, ShaderParams.blurRT, ShaderParams.blurRT2, mat, (int)Pass.BlurVertical);
                        FullScreenBlit(cmd, ShaderParams.blurRT2, ShaderParams.blurRT, mat, (int)Pass.BlurHorizontal);
                    }
                    FullScreenBlit(cmd, ShaderParams.blurRT, source, mat, (int)Pass.BlurVerticalAndBlend);

                    cmd.ReleaseTemporaryRT(ShaderParams.blurRT2);
                    cmd.ReleaseTemporaryRT(ShaderParams.blurRT);
                    cmd.ReleaseTemporaryRT(ShaderParams.lightBuffer);
                    context.ExecuteCommandBuffer(cmd);
                }

                CommandBufferPool.Release(cmd);

            }


            void FullScreenBlit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex) {
                destination = new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1);
                cmd.SetRenderTarget(destination);
                cmd.SetGlobalTexture(ShaderParams.mainTex, source);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, matrix4x4identity, material, 0, passIndex);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd) {
            }


            public void Cleanup() {
                foreach (VolumetricLight vl in VolumetricLight.volumetricLights) {
                    if (vl != null && vl.meshRenderer != null) {
                        vl.ToggleVolumetrics(true);
                    }
                }
                CoreUtils.Destroy(mat);
            }

        }

        [SerializeField, HideInInspector]
        Shader shader;
        VolumetricLightsRenderPass m_VLRenderPass;
        public static bool installed;

        public BlendMode blendMode;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        [Range(0, 6)]
        public int blurPasses = 1;
        [Range(1, 8)]
        public int blurDownscaling = 1;
        [Range(1f, 4)]
        public float blurSpread = 1f;
        
        public float brightness = 1f;

        void OnDisable() {
            installed = false;
            if (m_VLRenderPass != null) {
                m_VLRenderPass.Cleanup();
            }
        }

        private void OnValidate()
        {
            brightness = Mathf.Max(0, brightness);
        }

        public override void Create() {
            name = "Volumetric Lights";
            m_VLRenderPass = new VolumetricLightsRenderPass();
            shader = Shader.Find("Hidden/VolumetricLights/Blur");
            if (shader == null) {
                Debug.LogWarning("Could not load Volumetric Lights blur shader.");
            }
        }

        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            m_VLRenderPass.Setup(shader, renderer, this);
            renderer.EnqueuePass(m_VLRenderPass);
            installed = true;
        }
    }
}
