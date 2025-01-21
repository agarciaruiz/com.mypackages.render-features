namespace RenderFeatures
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.Universal;

    public class LinearDepthPass : ScriptableRenderPass
    {
        public RenderTexture OutputTexture;

        private readonly ComputeShader _computeShader;
        private readonly ComputeFeature.TextureMode _textureMode;


        public LinearDepthPass(ComputeFeature.TextureMode textureMode)
        {
            renderPassEvent = RenderPassEvent.AfterRendering;
            _computeShader = Resources.Load<ComputeShader>("ComputeShaders/DepthToLinear");
            _textureMode = textureMode;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (OutputTexture == null)
            {
                return;
            }

            // Use AddComputePass instead of AddRasterRenderPass.
            using (IComputeRenderGraphBuilder builder = renderGraph.AddComputePass("LinearDepthPass", out PassData data))
            {
                RTHandle rtHandle = RTHandles.Alloc(OutputTexture);

                TextureHandle tex = renderGraph.ImportTexture(rtHandle);

                // Initialize PassData with required resources
                data.ComputeShader = _computeShader;

                Camera currentCamera = frameData.Get<UniversalCameraData>().camera;
                data.ZNear = currentCamera.nearClipPlane;
                data.ZFar = currentCamera.farClipPlane;
                data.DisplayMode = (int)_textureMode;
                data.TargetTexture = tex;

                builder.UseTexture(data.TargetTexture, AccessFlags.Write);
                builder.AllowPassCulling(false); // Ensures the pass is not skipped
                builder.SetRenderFunc((PassData passData, ComputeGraphContext context) => { ExecutePass(passData, context); });
            }
        }

        private static void ExecutePass(PassData passData, ComputeGraphContext context)
        {
            context.cmd.SetComputeTextureParam(passData.ComputeShader, passData.ComputeShader.FindKernel("CSMain"),
                "TargetTexture",
                passData.TargetTexture);
            context.cmd.SetComputeFloatParam(passData.ComputeShader, "zNear", passData.ZNear);
            context.cmd.SetComputeFloatParam(passData.ComputeShader, "zFar", passData.ZFar);
            context.cmd.SetComputeIntParam(passData.ComputeShader, "displayMode", passData.DisplayMode);
            context.cmd.DispatchCompute(passData.ComputeShader, passData.ComputeShader.FindKernel("CSMain"), Screen.width / 8,
                Screen.height / 8, 1);
        }

        public class PassData
        {
            public ComputeShader ComputeShader;
            public float ZFar;
            public float ZNear;
            public int DisplayMode;
            public TextureHandle TargetTexture;
        }
    }
}