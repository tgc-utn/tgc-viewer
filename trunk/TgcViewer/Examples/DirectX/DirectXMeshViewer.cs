using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;

namespace Examples.DirectX
{
    /// <summary>
    /// Ejemplo DirectXMeshViewer:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Visualizador de modelos con formato .X de Microsoft.
    /// Muestra como cargar un modelo .X y lo renderiza en pantalla.
    /// Muestra como interactuar con los distintos Subset que puede tener la malla
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class DirectXMeshViewer : TgcExample
    {

        private Material[] meshMaterials;
        private Texture[] meshTextures;
        private Mesh mesh;
        string currentMeshFile;

        public override string getCategory()
        {
            return "DirectX";
        }

        public override string getName()
        {
            return "Mesh Viewer";
        }

        public override string getDescription()
        {
            return "Visualizador de modelo con formato .X. Permite cargar distintos modelos .X desde el FileSystem.";
        }

        public override void init()
        {
            currentMeshFile = GuiController.Instance.ExamplesMediaDir + "ModelosX" + "\\" + "shampoo.x";
            
            //cargar mesh
            loadMesh(currentMeshFile);

           
            //User Vars
            GuiController.Instance.UserVars.addVar("Vertices", mesh.NumberVertices);
            GuiController.Instance.UserVars.addVar("Triangles", mesh.NumberFaces);

            //Modifiers
            GuiController.Instance.Modifiers.addFile("Mesh", currentMeshFile, ".X files|*.x");

        }

        /// <summary>
        /// Cargar malla de DirectX.
        /// Una malla de DirectX posee varios Subset, que son distintos grupos
        /// de triángulos. Cada grupo puede tener su propia textura y material.
        /// </summary>
        /// <param name="path"></param>
        private void loadMesh(string path)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            ExtendedMaterial[] mtrl;

            //Cargar mesh con utilidad de DirectX
            mesh = Mesh.FromFile(path, MeshFlags.Managed, d3dDevice, out mtrl);

            //Analizar todos los subset de la malla
            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials = new Material[mtrl.Length];
                meshTextures = new Texture[mtrl.Length];

                //Cargar los material y texturas en un array
                for (int i = 0; i < mtrl.Length; i++)
                {
                    //Cargar material
                    meshMaterials[i] = mtrl[i].Material3D;

                    //Si hay textura, intentar cargarla
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename !=
                        string.Empty))
                    {
                        //Cargar textura con TextureLoader
                        meshTextures[i] = TextureLoader.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "ModelosX" + "\\" +
                            mtrl[i].TextureFilename);
                    }
                }
            }


            //Crear Bounding Sphere con herramienta de Geometry DirectX 
            float objectRadius = 0.0f;
            Vector3 objectCenter = new Vector3();

            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                GraphicsStream vertexData = vb.Lock(0, 0, LockFlags.None);
                objectRadius = Geometry.ComputeBoundingSphere(vertexData,
                                                              mesh.NumberVertices,
                                                              mesh.VertexFormat,
                                                              out objectCenter);
                vb.Unlock();
            }

            //Alejar camara rotacional, respecto de su Bounding Sphere
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), objectRadius * 4, 5f / objectRadius);
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio el modelo elegido por el usuario
            string selectedPath = (string)GuiController.Instance.Modifiers["Mesh"];
            if (selectedPath != currentMeshFile)
            {
                //cargar nuevo modelo
                currentMeshFile = selectedPath;
                loadMesh(currentMeshFile);

                //Actualizar contadores de triangulos y vertices
                GuiController.Instance.UserVars.setValue("Vertices", mesh.NumberVertices);
                GuiController.Instance.UserVars.setValue("Triangles", mesh.NumberFaces);
            }

            //Renderizar la malla.
            //Hay que renderizar cada subset por separado
            for (int i = 0; i < meshMaterials.Length; i++)
            {
                d3dDevice.Material = meshMaterials[i];
                d3dDevice.SetTexture(0, meshTextures[i]);
                mesh.DrawSubset(i);
            }

        }

        public override void close()
        {
            //Liberar recursos de la malla
            mesh.Dispose();
        }

    }
}
