using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class SpaceRunPipelineRender : RenderPipeline
    {
        private ScriptableRenderContext _context;
        private Camera _camera;
        private CullingResults _cullingResults;

        private const string bufferName = "Camera Render";


        private static readonly List<ShaderTagId> drawingShaderTagIds = new List<ShaderTagId> { new ShaderTagId("SRPDefaultUnlit"),};


#if UNITY_EDITOR
        private static readonly ShaderTagId[] _legacyShaderTagIds =
            {new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")};

        private static Material _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

        private void DrawUnsupportedShaders()
        {
            var drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new
            SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial,
            };
            for (var i = 1; i < _legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
            }
            var filteringSettings = FilteringSettings.defaultValue;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref  filteringSettings);
        }
        private void DrawGizmos()
        {
            if (!Handles.ShouldRenderGizmos())
            {
                return;
            }
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
#endif

        protected override void Render(ScriptableRenderContext context,
        Camera[] cameras)
        {
            CamerasRender(context, cameras);
        }
        private void CamerasRender(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                Render(context, camera);
            }
        }
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _camera = camera;
            _context = context;
            CommandBuffer _commandBuffer = new CommandBuffer { name = _camera.name };
            _commandBuffer.BeginSample(_camera.name);
            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();

            PrepareForSceneWindow();
            if (!_camera.TryGetCullingParameters(out var parameters))
            {
                return;
            }

            _cullingResults = _context.Cull(ref parameters);
            _context.SetupCameraProperties(_camera);
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            _commandBuffer.BeginSample(bufferName);
            //ExecuteCommandBuffer();
            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();


            var drawingSettings = CreateDrawingSettings(drawingShaderTagIds, SortingCriteria.CommonOpaque, out var sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            _context.DrawSkybox(_camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
#if UNITY_EDITOR
            DrawUnsupportedShaders();
            DrawGizmos();
#endif
            _commandBuffer.EndSample(bufferName);

            _context.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
            _context.Submit();
        }


        private void PrepareForSceneWindow()
        {
            if (_camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }
            private DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTags, SortingCriteria sortingCriteria, out SortingSettings sortingSettings)
        {
            sortingSettings = new SortingSettings(_camera)
            {
                criteria = sortingCriteria,
            };
            var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);
            for (var i = 1; i < shaderTags.Count; i++)
            {
                drawingSettings.SetShaderPassName(i, shaderTags[i]);
            }
            return drawingSettings;
        }
    }
}