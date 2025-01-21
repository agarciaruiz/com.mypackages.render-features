//using ImageMagick;

namespace RenderFeatures
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class TextureViewer : EditorWindow
    {
        private ComputeFeature _computeFeature;
        private Material _displayMaterial; // For displaying R16 textures
        private RenderTexture _capturedDepthTexture;


        [MenuItem("Tools/Depth Capture")]
        public static void ShowWindow() => GetWindow<TextureViewer>("Depth Capture");

        private void OnDisable()
        {
            if (_displayMaterial != null)
            {
                DestroyImmediate(_displayMaterial);
            }
        }

        private void OnEnable() =>
            // Create material for displaying single channel textures
            _displayMaterial = new Material(Shader.Find("Hidden/Internal-GUITexture"));

        private void OnGUI()
        {
            if (_computeFeature == null)
            {
                UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
                ScriptableRenderer renderer = cameraData?.scriptableRenderer;
                _computeFeature = renderer?.GetRendererFeature<ComputeFeature>();
            }

            if (_computeFeature != null)
            {
                if (GUILayout.Button("Capture depth"))
                {
                    RenderTexture depthTexture =
                        new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelWidth, 0, RenderTextureFormat.R16)
                        {
                            enableRandomWrite = true
                        };
                    depthTexture.Create();
                    _computeFeature.RenderTexture = depthTexture;

                    Camera.main.Render();

                    _capturedDepthTexture = depthTexture;
                }

                if (_capturedDepthTexture != null)
                {
                    GUILayout.Label("Captured Depth Texture:");

                    float textureWidth = position.width - 20;
                    float textureHeight = textureWidth * ((float)_capturedDepthTexture.height / _capturedDepthTexture.width);

                    if (textureHeight > position.height - 100)
                    {
                        textureHeight = position.height - 100;
                        textureWidth = textureHeight * ((float)_capturedDepthTexture.width / _capturedDepthTexture.height);
                    }

                    float offsetX = (position.width - textureWidth) / 2;
                    float offsetY = (position.height - textureHeight) / 2;

                    Rect textureRect = new Rect(offsetX, offsetY, textureWidth, textureHeight);
                    GUI.DrawTexture(textureRect, _capturedDepthTexture, ScaleMode.ScaleToFit, false);
                }
            }
            else
            {
                EditorGUILayout.LabelField("ComputeFeature not found");
            }
        }

        private void SaveTextureToFile(RenderTexture texture, string path)
        {
            RenderTexture.active = texture;
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.R16, false);
            texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture2D.Apply();

            byte[] bytes = texture2D.GetRawTextureData();

            // Create the read settings
            /*MagickReadSettings settings = new MagickReadSettings
        {
            Width = (uint)texture.width,
            Height = (uint)texture.height,
            Format = MagickFormat.Gray,
            Depth = 16,
            Endian = Endian.LSB
        };

        // Create ImageMagick image with the correct settings
        using (MagickImage image = new MagickImage(bytes, settings))
        {
            image.Format = MagickFormat.Png;
            image.Depth = 16;
            image.Flip();
            // Save the image
            image.Write(path);
        }*/

            Debug.Log($"Texture saved to {path}");
        }
    }


    public static class RendererExtensions
    {
        public static T GetRendererFeature<T>(this ScriptableRenderer renderer) where T : ScriptableRendererFeature
        {
            if (renderer == null)
            {
                return null;
            }

            // Try different known field names used across URP versions
            string[] fieldNames = { "m_RendererFeatures", "rendererFeatures" };
            FieldInfo fieldInfo = null;

            foreach (string fieldName in fieldNames)
            {
                fieldInfo = typeof(ScriptableRenderer).GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    break;
                }
            }

            if (fieldInfo == null)
            {
                // If we still can't find it, try searching all private fields
                FieldInfo[] fields = typeof(ScriptableRenderer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo = fields.FirstOrDefault(f => f.FieldType == typeof(List<ScriptableRendererFeature>));
            }

            if (fieldInfo != null)
            {
                if (fieldInfo.GetValue(renderer) is List<ScriptableRendererFeature> features)
                {
                    return features.OfType<T>().FirstOrDefault();
                }
            }

            Debug.LogWarning("Could not find renderer features field. URP version might not be compatible.");
            return null;
        }
    }
}