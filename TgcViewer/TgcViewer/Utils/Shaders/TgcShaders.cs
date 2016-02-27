using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcKeyFrameLoader;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace TgcViewer.Utils.Shaders
{
    /// <summary>
    ///     Utilidad para manejo de shaders
    /// </summary>
    public class TgcShaders
    {
        /// <summary>
        ///     Technique de Varios para PositionColoredTextured
        /// </summary>
        public const string T_POSITION_COLORED_TEXTURED = "PositionColoredTextured";

        /// <summary>
        ///     Technique de Varios para PositionTextured
        /// </summary>
        public const string T_POSITION_TEXTURED = "PositionTextured";

        /// <summary>
        ///     Technique de Varios para PositionColored
        /// </summary>
        public const string T_POSITION_COLORED = "PositionColored";

        /// <summary>
        ///     Technique de Varios para PositionColoredAlpha
        /// </summary>
        public const string T_POSITION_COLORED_ALPHA = "PositionColoredAlpha";

        /// <summary>
        ///     FVF para formato de vertice PositionColoredTextured
        /// </summary>
        public static readonly VertexElement[] PositionColoredTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 16, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     FVF para formato de vertice PositionTextured
        /// </summary>
        public static readonly VertexElement[] PositionTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     FVF para formato de vertice PositionColored
        /// </summary>
        public static readonly VertexElement[] PositionColored_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        ///     Shader generico para TgcMesh
        /// </summary>
        public Effect TgcMeshShader { get; set; }

        /// <summary>
        ///     Shader para TgcMesh con iluminacion dinamica con PhongShading
        /// </summary>
        public Effect TgcMeshPhongShader { get; set; }

        /// <summary>
        ///     Shader para TgcMesh con iluminacion dinamica por PointLight
        /// </summary>
        public Effect TgcMeshPointLightShader { get; set; }

        /// <summary>
        ///     Shader para TgcMesh con iluminacion dinamica por SpotLight
        /// </summary>
        public Effect TgcMeshSpotLightShader { get; set; }

        /// <summary>
        ///     Shader generico para TgcSkeletalMesh
        /// </summary>
        public Effect TgcSkeletalMeshShader { get; set; }

        /// <summary>
        ///     Shader para TgcSkeletalMesh con iluminacion dinamica por PointLight
        /// </summary>
        public Effect TgcSkeletalMeshPointLightShader { get; set; }

        /// <summary>
        ///     Shader generico para TgcKeyFrameMesh
        /// </summary>
        public Effect TgcKeyFrameMeshShader { get; set; }

        /// <summary>
        ///     Shader de cosas varias
        /// </summary>
        public Effect VariosShader { get; set; }

        /// <summary>
        ///     VertexDeclaration para formato PositionColoredTextured
        /// </summary>
        public VertexDeclaration VdecPositionColoredTextured { get; private set; }

        /// <summary>
        ///     VertexDeclaration para formato PositionTextured
        /// </summary>
        public VertexDeclaration VdecPositionTextured { get; private set; }

        /// <summary>
        ///     VertexDeclaration para formato PositionColored
        /// </summary>
        public VertexDeclaration VdecPositionColored { get; private set; }

        /// <summary>
        ///     Iniciar shaders comunes
        /// </summary>
        public void loadCommonShaders()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar shaders genericos para todo el framework
            var shadersPath = GuiController.Instance.ExamplesMediaDir + "Shaders\\TgcViewer\\";
            TgcMeshShader = loadEffect(shadersPath + "TgcMeshShader.fx");
            TgcMeshPhongShader = loadEffect(shadersPath + "TgcMeshPhongShader.fx");
            TgcMeshPointLightShader = loadEffect(shadersPath + "TgcMeshPointLightShader.fx");
            TgcMeshSpotLightShader = loadEffect(shadersPath + "TgcMeshSpotLightShader.fx");
            TgcSkeletalMeshShader = loadEffect(shadersPath + "TgcSkeletalMeshShader.fx");
            TgcSkeletalMeshPointLightShader = loadEffect(shadersPath + "TgcSkeletalMeshPointLightShader.fx");
            TgcKeyFrameMeshShader = loadEffect(shadersPath + "TgcKeyFrameMeshShader.fx");
            VariosShader = loadEffect(shadersPath + "Varios.fx");

            //Crear vertexDeclaration comunes
            VdecPositionColoredTextured = new VertexDeclaration(d3dDevice, PositionColoredTextured_VertexElements);
            VdecPositionTextured = new VertexDeclaration(d3dDevice, PositionTextured_VertexElements);
            VdecPositionColored = new VertexDeclaration(d3dDevice, PositionColored_VertexElements);
        }

        /// <summary>
        ///     Cargar archivo .fx de Shaders
        /// </summary>
        /// <param name="path">Path del archivo .fx</param>
        /// <returns>Effect cargado</returns>
        public static Effect loadEffect(string path)
        {
            string compilationErrors;
            var effect = Effect.FromFile(GuiController.Instance.D3dDevice, path, null, null, ShaderFlags.None, null,
                out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + path + ". Errores: " + compilationErrors);
            }
            return effect;
        }

        /// <summary>
        ///     Obtener technique default para un TgcMesh según su MeshRenderType
        /// </summary>
        /// <param name="renderType">MeshRenderType</param>
        /// <returns>Nombre del Technique que le corresponde</returns>
        public string getTgcMeshTechnique(TgcMesh.MeshRenderType renderType)
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
        ///     Obtener technique default para un TgcSkeletalMesh según su MeshRenderType
        /// </summary>
        /// <param name="renderType">MeshRenderType</param>
        /// <returns>Nombre del Technique que le corresponde</returns>
        public string getTgcSkeletalMeshTechnique(TgcSkeletalMesh.MeshRenderType renderType)
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
        ///     Obtener technique default para un TgcKeyFrameMesh según su MeshRenderType
        /// </summary>
        /// <param name="renderType">MeshRenderType</param>
        /// <returns>Nombre del Technique que le corresponde</returns>
        public string getTgcKeyFrameMeshTechnique(TgcKeyFrameMesh.MeshRenderType renderType)
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
        ///     Cargar todas la matrices generales que necesita el shader
        /// </summary>
        public void setShaderMatrix(Effect effect, Matrix world)
        {
            var device = GuiController.Instance.D3dDevice;

            var matWorldView = world*device.Transform.View;
            var matWorldViewProj = matWorldView*device.Transform.Projection;
            effect.SetValue("matWorld", world);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.TransposeMatrix(Matrix.Invert(world)));
        }

        /// <summary>
        ///     Cargar todas la matrices generales que necesita el shader, tomando
        ///     como primicia que la matriz de world es la identidad.
        ///     Simplica los calculos respecto a setShaderMatrix()
        /// </summary>
        public void setShaderMatrixIdentity(Effect effect)
        {
            var device = GuiController.Instance.D3dDevice;

            var matWorldView = device.Transform.View;
            var matWorldViewProj = matWorldView*device.Transform.Projection;
            effect.SetValue("matWorld", Matrix.Identity);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.Identity);
        }
    }
}