namespace RenderFeatures
{
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class ComputeFeature : ScriptableRendererFeature
    {
        public enum TextureMode
        {
            CameraDepth,
            LinearDepth,
            NormalizedDepth
        }

        public TextureMode SelectedTextureMode;
        private LinearDepthPass _linearDepthPass;

        public RenderTexture RenderTexture
        {
            set => _linearDepthPass.OutputTexture = value;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_linearDepthPass);
        }

        public override void Create()
        {
            _linearDepthPass = new LinearDepthPass(SelectedTextureMode);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            // Setup any resources needed by the compute pass
        }

        protected override void Dispose(bool disposing)
        {
            // Clean up any resources
            if (disposing)
            {
            }
        }
    }
}