using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SceneLoader
{
    /// <summary>
    /// Ejemplo EjemploInstanciasPalmeras
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 7 - Técnicas de Optimización - Instancias de Modelos
    /// 
    /// Muestra como crear varias instancias de un mismo TgcMesh.
    /// De esta forma se reutiliza su información gráfica (triángulos, vértices, textura, etc).
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploInstanciasPalmeras : TgcExample
    {
        TgcBox suelo;
        List<TgcMesh> meshes;
        TgcMesh palmeraOriginal;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "Instancias Palmeras";
        }

        public override string getDescription()
        {
            return "Muestra como crear varias instancias de un mismo TgcMesh.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear suelo
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\pasto.jpg");
            suelo = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(2000, 0, 2000), pisoTexture);

            //Cargar modelo de palmera original
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmeraOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 5;
            int cols = 6;
            float offset = 200;
            meshes = new List<TgcMesh>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = palmeraOriginal.createMeshInstance( palmeraOriginal.Name + i + "_" + j );
                    
                    //Desplazarlo
                    instance.move(i * offset, 70, j * offset);
                    instance.Scale = new Vector3(0.25f, 0.25f, 0.25f);

                    meshes.Add(instance);
                }
            }


            //Camara en primera persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400;
            GuiController.Instance.FpsCamera.JumpSpeed = 400;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(61.8657f, 403.7024f, -527.558f), new Vector3(379.7143f, 12.9713f, 336.3295f));

        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar suelo
            suelo.render();

            //Renderizar instancias
            foreach (TgcMesh mesh in meshes)
            {
                mesh.render();
            }
        }

        public override void close()
        {
            suelo.dispose();

            //Al hacer dispose del original, se hace dispose automáticamente de todas las instancias
            palmeraOriginal.dispose();
        }

    }
}
