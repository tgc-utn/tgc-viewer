using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Direct3D;
using TGC.Core.KeyFrameLoader;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;

namespace TGC.Core.Shaders
{
    /// <summary>
    /// Utilidad para manejo de shaders.
    /// </summary>
    public class TGCShaders
    {
        /// <summary>
        /// Technique de Varios para PositionColoredTextured.
        /// </summary>
        public const string T_POSITION_COLORED_TEXTURED = "PositionColoredTextured";

        /// <summary>
        /// Technique de Varios para PositionTextured.
        /// </summary>
        public const string T_POSITION_TEXTURED = "PositionTextured";

        /// <summary>
        /// Technique de Varios para PositionColored.
        /// </summary>
        public const string T_POSITION_COLORED = "PositionColored";

        /// <summary>
        /// Technique de Varios para PositionColoredAlpha.
        /// </summary>
        public const string T_POSITION_COLORED_ALPHA = "PositionColoredAlpha";

        /// <summary>
        /// FVF para formato de vertice PositionColoredTextured.
        /// </summary>
        public static readonly VertexElement[] PositionColoredTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        /// FVF para formato de vertice PositionTextured.
        /// </summary>
        public static readonly VertexElement[] PositionTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        /// FVF para formato de vertice PositionColored.
        /// </summary>
        public static readonly VertexElement[] PositionColored_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        /// Permite acceder a la instancia del Singleton.
        /// </summary>
        public static TGCShaders Instance { get; } = new TGCShaders();

        /// <summary>
        /// Constructor privado para el Singleton.
        /// </summary>
        private TGCShaders() { }

        /// <summary>
        /// Ruta donde estan los Shaders comunes.
        /// </summary>
        public string CommonShadersPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private D3DDevice D3DDevice { get; set; }

        /// <summary>
        /// Shader generico para TgcMesh.
        /// </summary>
        public Effect TgcMeshShader { get; set; }

        /// <summary>
        /// Shader para TgcMesh con iluminacion dinamica con PhongShading.
        /// </summary>
        public Effect TgcMeshPhongShader { get; set; }

        /// <summary>
        /// Shader para TgcMesh con iluminacion dinamica por PointLight.
        /// </summary>
        public Effect TgcMeshPointLightShader { get; set; }

        /// <summary>
        /// Shader para TgcMesh con iluminacion dinamica por SpotLight.
        /// </summary>
        public Effect TgcMeshSpotLightShader { get; set; }

        /// <summary>
        /// Shader generico para TgcSkeletalMesh.
        /// </summary>
        public Effect TgcSkeletalMeshShader { get; set; }

        /// <summary>
        /// Shader para TgcSkeletalMesh con iluminacion dinamica por PointLight.
        /// </summary>
        public Effect TgcSkeletalMeshPointLightShader { get; set; }

        /// <summary>
        /// Shader generico para TgcKeyFrameMesh.
        /// </summary>
        public Effect TgcKeyFrameMeshShader { get; set; }

        /// <summary>
        /// Shader de cosas varias.
        /// </summary>
        public Effect VariosShader { get; set; }

        /// <summary>
        /// VertexDeclaration para formato PositionColoredTextured.
        /// </summary>
        public VertexDeclaration VdecPositionColoredTextured { get; private set; }

        /// <summary>
        /// VertexDeclaration para formato PositionTextured.
        /// </summary>
        public VertexDeclaration VdecPositionTextured { get; private set; }

        /// <summary>
        /// VertexDeclaration para formato PositionColored.
        /// </summary>
        public VertexDeclaration VdecPositionColored { get; private set; }

        /// <summary>
        /// Iniciar shaders comunes.
        /// </summary>
        /// <param name="shadersPath">Ruta donde estan los Shaders comunes.</param>
        /// <param name="d3dDevice">Device.</param>
        public void LoadCommonShaders(string shadersPath, D3DDevice d3dDevice)
        {
            CommonShadersPath = shadersPath;
            D3DDevice = d3dDevice;

            //Cargar shaders genericos para todo el framework.
            TgcMeshShader = LoadEffect(shadersPath + "TgcMeshShader.fx");
            TgcMeshPhongShader = LoadEffect(shadersPath + "TgcMeshPhongShader.fx");
            TgcMeshPointLightShader = LoadEffect(shadersPath + "TgcMeshPointLightShader.fx");
            TgcMeshSpotLightShader = LoadEffect(shadersPath + "TgcMeshSpotLightShader.fx");
            TgcSkeletalMeshShader = LoadEffect(shadersPath + "TgcSkeletalMeshShader.fx");
            TgcSkeletalMeshPointLightShader = LoadEffect(shadersPath + "TgcSkeletalMeshPointLightShader.fx");
            TgcKeyFrameMeshShader = LoadEffect(shadersPath + "TgcKeyFrameMeshShader.fx");
            VariosShader = LoadEffect(shadersPath + "Varios.fx");

            //Crear vertexDeclaration comunes.
            VdecPositionColoredTextured = new VertexDeclaration(d3dDevice.Device, PositionColoredTextured_VertexElements);
            VdecPositionTextured = new VertexDeclaration(d3dDevice.Device, PositionTextured_VertexElements);
            VdecPositionColored = new VertexDeclaration(d3dDevice.Device, PositionColored_VertexElements);
        }

        /// <summary>
        /// Cargar archivo .fx de Shaders.
        /// </summary>
        /// <param name="path">Path del archivo fx.</param>
        /// <returns>Effect cargado.</returns>
        public Effect LoadEffect(string path)
        {
            var effect = Effect.FromFile(D3DDevice.Device, path, null, null, ShaderFlags.None, null, out var compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + path + ". Errores: " + compilationErrors);
            }
            return effect;
        }

        /// <summary>
        /// Obtener technique default para un TgcMesh según su MeshRenderType.
        /// </summary>
        /// <param name="renderType">MeshRenderType.</param>
        /// <returns>Nombre del Technique que le corresponde.</returns>
        public string GetTGCMeshTechnique(TgcMesh.MeshRenderType renderType)
        {
            switch (renderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    return "VERTEX_COLOR";

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    return "DIFFUSE_MAP";

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    return "DIFFUSE_MAP_AND_LIGHTMAP";
            }

            throw new Exception("RenderType incorrecto");
        }

        /// <summary>
        /// Obtener technique default para un TgcSkeletalMesh según su MeshRenderType.
        /// </summary>
        /// <param name="renderType">MeshRenderType.</param>
        /// <returns>Nombre del Technique que le corresponde.</returns>
        public string GetTGCSkeletalMeshTechnique(TgcSkeletalMesh.MeshRenderType renderType)
        {
            switch (renderType)
            {
                case TgcSkeletalMesh.MeshRenderType.VERTEX_COLOR:
                    return "VERTEX_COLOR";

                case TgcSkeletalMesh.MeshRenderType.DIFFUSE_MAP:
                    return "DIFFUSE_MAP";
            }

            throw new Exception("RenderType incorrecto");
        }

        /// <summary>
        /// Obtener technique default para un TgcKeyFrameMesh según su MeshRenderType.
        /// </summary>
        /// <param name="renderType">MeshRenderType.</param>
        /// <returns>Nombre del Technique que le corresponde.</returns>
        public string GetTGCKeyFrameMeshTechnique(TgcKeyFrameMesh.MeshRenderType renderType)
        {
            switch (renderType)
            {
                case TgcKeyFrameMesh.MeshRenderType.VERTEX_COLOR:
                    return "VERTEX_COLOR";

                case TgcKeyFrameMesh.MeshRenderType.DIFFUSE_MAP:
                    return "DIFFUSE_MAP";
            }

            throw new Exception("RenderType incorrecto");
        }

        /// <summary>
        /// Cargar todas la matrices generales que necesita el shader.
        /// </summary>
        public void SetShaderMatrix(Effect effect, TGCMatrix world)
        {
            var matWorldView = world.ToMatrix() * D3DDevice.Device.Transform.View;
            var matWorldViewProj = matWorldView * D3DDevice.Device.Transform.Projection;
            effect.SetValue("matWorld", world.ToMatrix());
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", TGCMatrix.TransposeMatrix(TGCMatrix.Invert(world)).ToMatrix());
        }

        /// <summary>
        /// Cargar todas la matrices generales que necesita el shader,
        /// tomando como primicia que la matriz de world es la identidad.
        /// Simplica los calculos respecto a SetShaderMatrix().
        /// </summary>
        public void SetShaderMatrixIdentity(Effect effect)
        {
            var matWorldView = D3DDevice.Device.Transform.View;
            var matWorldViewProj = matWorldView * D3DDevice.Device.Transform.Projection;
            effect.SetValue("matWorld", TGCMatrix.Identity.ToMatrix());
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", TGCMatrix.Identity.ToMatrix());
        }
    }
}