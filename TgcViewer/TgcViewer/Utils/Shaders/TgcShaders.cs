using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcKeyFrameLoader;

namespace TgcViewer.Utils.Shaders
{
    /// <summary>
    /// Utilidad para manejo de shaders
    /// </summary>
    public class TgcShaders
    {
        /// <summary>
        /// Technique de Varios para PositionColoredTextured
        /// </summary>
        public const string T_POSITION_COLORED_TEXTURED = "PositionColoredTextured";

        /// <summary>
        /// Technique de Varios para PositionTextured
        /// </summary>
        public const string T_POSITION_TEXTURED = "PositionTextured";

        /// <summary>
        /// Technique de Varios para PositionColored
        /// </summary>
        public const string T_POSITION_COLORED = "PositionColored";

        /// <summary>
        /// Technique de Varios para PositionColoredAlpha
        /// </summary>
        public const string T_POSITION_COLORED_ALPHA = "PositionColoredAlpha";


        Effect tgcMeshShader;
        /// <summary>
        /// Shader generico para TgcMesh
        /// </summary>
        public Effect TgcMeshShader
        {
            get { return tgcMeshShader; }
            set { tgcMeshShader = value; }
        }

        Effect tgcMeshPointLightShader;
        /// <summary>
        /// Shader para TgcMesh con iluminacion dinamica por PointLight
        /// </summary>
        public Effect TgcMeshPointLightShader
        {
            get { return tgcMeshPointLightShader; }
            set { tgcMeshPointLightShader = value; }
        }

        Effect tgcMeshSpotLightShader;
        /// <summary>
        /// Shader para TgcMesh con iluminacion dinamica por SpotLight
        /// </summary>
        public Effect TgcMeshSpotLightShader
        {
            get { return tgcMeshSpotLightShader; }
            set { tgcMeshSpotLightShader = value; }
        }

        Effect tgcSkeletalMeshShader;
        /// <summary>
        /// Shader generico para TgcSkeletalMesh
        /// </summary>
        public Effect TgcSkeletalMeshShader
        {
            get { return tgcSkeletalMeshShader; }
            set { tgcSkeletalMeshShader = value; }
        }

        Effect tgcSkeletalMeshPointLightShader;
        /// <summary>
        /// Shader para TgcSkeletalMesh con iluminacion dinamica por PointLight
        /// </summary>
        public Effect TgcSkeletalMeshPointLightShader
        {
            get { return tgcSkeletalMeshPointLightShader; }
            set { tgcSkeletalMeshPointLightShader = value; }
        }

        Effect tgcKeyFrameMeshShader;
        /// <summary>
        /// Shader generico para TgcKeyFrameMesh
        /// </summary>
        public Effect TgcKeyFrameMeshShader
        {
            get { return tgcKeyFrameMeshShader; }
            set { tgcKeyFrameMeshShader = value; }
        }

        Effect variosShader;
        /// <summary>
        /// Shader de cosas varias
        /// </summary>
        public Effect VariosShader
        {
            get { return variosShader; }
            set { variosShader = value; }
        }


        VertexDeclaration vdecPositionColoredTextured;
        /// <summary>
        /// VertexDeclaration para formato PositionColoredTextured
        /// </summary>
        public VertexDeclaration VdecPositionColoredTextured
        {
            get { return vdecPositionColoredTextured; }
        }

        VertexDeclaration vdecPositionTextured;
        /// <summary>
        /// VertexDeclaration para formato PositionTextured
        /// </summary>
        public VertexDeclaration VdecPositionTextured
        {
            get { return vdecPositionTextured; }
        }

        VertexDeclaration vdecPositionColored;
        /// <summary>
        /// VertexDeclaration para formato PositionColored
        /// </summary>
        public VertexDeclaration VdecPositionColored
        {
            get { return vdecPositionColored; }
        }


        public TgcShaders()
        {

        }

        /// <summary>
        /// Iniciar shaders comunes
        /// </summary>
        public void loadCommonShaders()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar shaders genericos para todo el framework
            string shadersPath = GuiController.Instance.ExamplesMediaDir + "Shaders\\TgcViewer\\";
            this.tgcMeshShader = TgcShaders.loadEffect(shadersPath + "TgcMeshShader.fx");
            this.tgcMeshPointLightShader = TgcShaders.loadEffect(shadersPath + "TgcMeshPointLightShader.fx");
            this.tgcMeshSpotLightShader = TgcShaders.loadEffect(shadersPath + "TgcMeshSpotLightShader.fx");
            this.tgcSkeletalMeshShader = TgcShaders.loadEffect(shadersPath + "TgcSkeletalMeshShader.fx");
            this.tgcSkeletalMeshPointLightShader = TgcShaders.loadEffect(shadersPath + "TgcSkeletalMeshPointLightShader.fx");
            this.tgcKeyFrameMeshShader = TgcShaders.loadEffect(shadersPath + "TgcKeyFrameMeshShader.fx");
            this.variosShader = TgcShaders.loadEffect(shadersPath + "Varios.fx");

            //Crear vertexDeclaration comunes
            this.vdecPositionColoredTextured = new VertexDeclaration(d3dDevice, TgcShaders.PositionColoredTextured_VertexElements);
            this.vdecPositionTextured = new VertexDeclaration(d3dDevice, TgcShaders.PositionTextured_VertexElements);
            this.vdecPositionColored = new VertexDeclaration(d3dDevice, TgcShaders.PositionColored_VertexElements);
        }

        /// <summary>
        /// Cargar archivo .fx de Shaders
        /// </summary>
        /// <param name="path">Path del archivo .fx</param>
        /// <returns>Effect cargado</returns>
        public static Effect loadEffect(string path)
        {
            string compilationErrors;
            Effect effect = Effect.FromFile(GuiController.Instance.D3dDevice, path, null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + path + ". Errores: " + compilationErrors);
            }
            return effect;
        }

        /// <summary>
        /// Obtener technique default para un TgcMesh según su MeshRenderType
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
        /// Obtener technique default para un TgcSkeletalMesh según su MeshRenderType
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
        /// Obtener technique default para un TgcKeyFrameMesh según su MeshRenderType
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
        /// Cargar todas la matrices generales que necesita el shader
        /// </summary>
        public void setShaderMatrix(Effect effect, Matrix world)
        {
            Device device = GuiController.Instance.D3dDevice;

            Matrix matWorldView = world * device.Transform.View;
            Matrix matWorldViewProj = matWorldView * device.Transform.Projection;
            effect.SetValue("matWorld", world);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.TransposeMatrix(Matrix.Invert(world)));
        }

        /// <summary>
        /// Cargar todas la matrices generales que necesita el shader, tomando
        /// como primicia que la matriz de world es la identidad.
        /// Simplica los calculos respecto a setShaderMatrix()
        /// </summary>
        public void setShaderMatrixIdentity(Effect effect)
        {
            Device device = GuiController.Instance.D3dDevice;

            Matrix matWorldView = device.Transform.View;
            Matrix matWorldViewProj = matWorldView * device.Transform.Projection;
            effect.SetValue("matWorld", Matrix.Identity);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.Identity);
        }


        /// <summary>
        /// FVF para formato de vertice PositionColoredTextured
        /// </summary>
        public static readonly VertexElement[] PositionColoredTextured_VertexElements = new VertexElement[]
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
        /// FVF para formato de vertice PositionTextured
        /// </summary>
        public static readonly VertexElement[] PositionTextured_VertexElements = new VertexElement[]
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
        /// FVF para formato de vertice PositionColored
        /// </summary>
        public static readonly VertexElement[] PositionColored_VertexElements = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                                    DeclarationMethod.Default,
                                    DeclarationUsage.Position, 0),

            new VertexElement(0, 12, DeclarationType.Color,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Color, 0),

            VertexElement.VertexDeclarationEnd 
        };

    }
}
