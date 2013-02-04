using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Shaders.WorkshopShaders
{
    /// <summary>
    /// Sobrecargo la funcionalidad del TGCmesh, para que soporte shaders.
    /// 
    /// Autor: Mariano Banquiero
    /// 
    /// </summary>
    public class MyMesh : TgcMesh
    {
        public Effect effect;

        public MyMesh(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, renderType)
        {
        }

        public MyMesh(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }

        public MyMesh createMeshInstance(string name, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (this.parentInstance != null)
            {
                throw new Exception("No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            MyMesh instance = new MyMesh(name, this, translation, rotation, scale);
            //instance.effect = effect;

            //BoundingBox
            instance.boundingBox = new TgcBoundingBox(this.boundingBox.PMin, this.boundingBox.PMax);
            instance.updateBoundingBox();

            instance.enabled = true;
            return instance;
        }

        public MyMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, Vector3.Empty, Vector3.Empty, new Vector3(1, 1, 1));
        }


        /// <summary>
        /// Se redefine este método para agregar shaders.
        /// Es el mismo código del render() pero con la sección de "MeshRenderType.DIFFUSE_MAP" ampliada
        /// para Shaders.
        /// </summary>
        public new void render()
        {
            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformacion de malla
            if (autoTransformEnable)
            {
                this.transform = Matrix.Identity
                    * Matrix.Scaling(scale)
                    * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * Matrix.Translation(translation);
            }
            device.Transform.World = this.transform;
            if (parentInstance != null)
                effect = ((MyMesh)parentInstance).effect;


            // incializacion standard para hacer la proyeccion
            effect.SetValue("matWorld", device.Transform.World);
            effect.SetValue("matWorldView", device.Transform.World * device.Transform.View);
            effect.SetValue("matWorldViewProj", device.Transform.World * device.Transform.View * device.Transform.Projection);
           
            // es usual en los shaders pasar directamente la matrix total de transformacion
            // matWorldViewProj = World*View*Proj 
            // para hacer una sola multiplicacion.
            
            // Detalle para las normales
            // Transformar una normal es un poco diferente a transformar una coordenada
            // en general hay que usar la inversa transpuesta. Salvo el caso que la escala
            // sea uniforme, en ese caso es la misma matriz de transformacion. 
            Matrix WorldInverse = device.Transform.World;
            WorldInverse.Invert();
            Matrix WorldInverseTranspose = Matrix.TransposeMatrix(WorldInverse);
            WorldInverseTranspose.M14 = 0;
            WorldInverseTranspose.M24 = 0;
            WorldInverseTranspose.M34 = 0;
            WorldInverseTranspose.M44 = 0;
            effect.SetValue("matWorldInverseTranspose", WorldInverseTranspose);

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;
            // dibujo a traves del effect
            // itero por material, en cada material,
            // El SetTexture(0,diffuseMaps[i].D3dTexture) del fixed pipeline 
            // se reemplaza por 
            // effect.SetValue("base_Tex", diffuseMaps[i].D3dTexture);
            // Una tecnica esta compuesta por una o mas pasadas. 
            // Esta es la estructura habitual para dibujar una primitiva mesh en un shader
            // iterar por pasasdas, y dentro de cada pasada por material. 
            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    effect.SetValue("base_Tex", diffuseMaps[i].D3dTexture);
                    // guarda: Todos los SetValue tienen que ir ANTES del beginPass.
                    // si no hay que llamar effect.CommitChanges para que tome el dato!
                    effect.BeginPass(n);
                    d3dMesh.DrawSubset(i);
                    effect.EndPass();
                }
            }
            effect.End();
        }

    }

    /// <summary>
    /// Factory customizado para poder crear clase TgcMeshShader
    /// </summary>
    public class MyCustomMeshFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new MyMesh(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new MyMesh(meshName, originalMesh, translation, rotation, scale);
        }
    }
}
