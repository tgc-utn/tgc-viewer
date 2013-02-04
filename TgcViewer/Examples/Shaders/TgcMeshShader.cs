using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils;

namespace Examples.Shaders
{
    /// <summary>
    /// Extendemos de TgcMesh para poder redefinir el método executeRender() y agregar renderizado de Shaders. 
    /// 
    /// </summary>
    public class TgcMeshShader : TgcMesh
    {
        Effect effect;
        /// <summary>
        /// Shader
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        /// Se llama antes de iniciar el shader.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        public delegate void ShaderBeginHandler(TgcMeshShader mesh);

        /// <summary>
        /// Evento que se llama antes de iniciar el shader.
        /// </summary>
        public event ShaderBeginHandler ShaderBegin;

        /// <summary>
        /// Se llama antes de iniciar una pasada del shader.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="pass">Número de pasada del shader</param>
        public delegate void ShaderPassBeginHandler(TgcMeshShader mesh, int pass);

        /// <summary>
        /// Evento que se llama antes de iniciar una pasada del shader.
        /// </summary>
        public event ShaderPassBeginHandler ShaderPassBegin;


        public TgcMeshShader(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, renderType)
        {
        }

        public TgcMeshShader(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }


        /// <summary>
        /// Se redefine este método para agregar shaders.
        /// Es el mismo código del render() pero con la sección de "MeshRenderType.DIFFUSE_MAP" ampliada
        /// para Shaders.
        /// </summary>
        public new void render()
        {
            if (!enabled)
                return;

            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformacion de malla
            updateMeshTransform();

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;

            //Activar AlphaBlending
            activateAlphaBlend();

            //Cargar valores de shader de matrices que dependen de la posición del mesh
            Matrix matWorldView = this.transform * device.Transform.View;
            Matrix matWorldViewProj = matWorldView * device.Transform.Projection;
            effect.SetValue("matWorld", this.transform);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);

            //Renderizar segun el tipo de render de la malla
            int numPasses;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:

                    //Hacer reset de texturas
                    texturesManager.clear(0);
                    texturesManager.clear(1);
                    device.Material = TgcD3dDevice.DEFAULT_MATERIAL;

                    //Llamar evento para configurar inicio del shader
                    if (ShaderBegin != null)
                    {
                        ShaderBegin.Invoke(this);
                    }

                    //Iniciar Shader e iterar sobre sus Render Passes
                    numPasses = effect.Begin(0);
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Llamar evento para configurar inicio de la pasada del shader
                        if (ShaderPassBegin != null)
                        {
                            ShaderPassBegin.Invoke(this, n);
                        }

                        //Iniciar pasada de shader
                        effect.BeginPass(n);
                        d3dMesh.DrawSubset(0);
                        effect.EndPass();
                    }

                    //Finalizar shader
                    effect.End();

                    break;

                case MeshRenderType.DIFFUSE_MAP:

                    //Hacer reset de Lightmap
                    texturesManager.clear(1);

                    //Llamar evento para configurar inicio del shader
                    if (ShaderBegin != null)
                    {
                        ShaderBegin.Invoke(this);
                    }

                    //Iniciar Shader e iterar sobre sus Render Passes
                    numPasses = effect.Begin(0);
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Llamar evento para configurar inicio de la pasada del shader
                        if (ShaderPassBegin != null)
                        {
                            ShaderPassBegin.Invoke(this, n);
                        }

                        //Dibujar cada subset con su Material y DiffuseMap correspondiente
                        for (int i = 0; i < materials.Length; i++)
                        {
                            device.Material = materials[i];
                            
                            //Setear textura en shader
                            texturesManager.shaderSet(effect, "diffuseMap_Tex", diffuseMaps[i]);

                            //Iniciar pasada de shader
                            // guarda: Todos los SetValue tienen que ir ANTES del beginPass.
                            // si no hay que llamar effect.CommitChanges para que tome el dato!
                            effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            effect.EndPass();
                        }
                    }

                    //Finalizar shader
                    effect.End();

                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:

                    throw new Exception("Caso no contemplado en este ejemplo");
            }


            //Activar AlphaBlending
            resetAlphaBlend();

        }

    }

    /// <summary>
    /// Factory customizado para poder crear clase TgcMeshShader
    /// </summary>
    public class CustomMeshShaderFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new TgcMeshShader(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new TgcMeshShader(meshName, originalMesh, translation, rotation, scale);
        }
    }
}
