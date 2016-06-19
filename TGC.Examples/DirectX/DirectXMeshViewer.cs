using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo DirectXMeshViewer:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Visualizador de modelos con formato .X de Microsoft.
    ///     Muestra como cargar un modelo .X y lo renderiza en pantalla.
    ///     Muestra como interactuar con los distintos Subset que puede tener la malla
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class DirectXMeshViewer : TgcExample
    {
        private string currentMeshFile;
        private Mesh mesh;
        private Material[] meshMaterials;
        private Texture[] meshTextures;

        public DirectXMeshViewer(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "DirectX";
            Name = "Mesh Viewer";
            Description =
                "Visualizador de modelo con formato .X. Permite cargar distintos modelos .X desde el FileSystem.";
        }

        public override void Init()
        {
            currentMeshFile = MediaDir + "ModelosX" + "\\" + "shampoo.x";

            //cargar mesh
            loadMesh(currentMeshFile);

            //User Vars
            UserVars.addVar("Vertices", mesh.NumberVertices);
            UserVars.addVar("Triangles", mesh.NumberFaces);

            //Modifiers
            Modifiers.addFile("Mesh", currentMeshFile, ".X files|*.x");
        }

        public override void Update()
        {
            base.PreUpdate();
        }

        /// <summary>
        ///     Cargar malla de DirectX.
        ///     Una malla de DirectX posee varios Subset, que son distintos grupos
        ///     de triángulos. Cada grupo puede tener su propia textura y material.
        /// </summary>
        /// <param name="path"></param>
        private void loadMesh(string path)
        {
            ExtendedMaterial[] mtrl;

            //Cargar mesh con utilidad de DirectX
            mesh = Mesh.FromFile(path, MeshFlags.Managed, D3DDevice.Instance.Device, out mtrl);

            //Analizar todos los subset de la malla
            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials = new Material[mtrl.Length];
                meshTextures = new Texture[mtrl.Length];

                //Cargar los material y texturas en un array
                for (var i = 0; i < mtrl.Length; i++)
                {
                    //Cargar material
                    meshMaterials[i] = mtrl[i].Material3D;

                    //Si hay textura, intentar cargarla
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename !=
                                                              string.Empty))
                    {
                        //Cargar textura con TextureLoader
                        meshTextures[i] = TextureLoader.FromFile(D3DDevice.Instance.Device,
                            MediaDir + "ModelosX" + "\\" +
                            mtrl[i].TextureFilename);
                    }
                }
            }

            //Crear Bounding Sphere con herramienta de Geometry DirectX
            var objectRadius = 0.0f;
            var objectCenter = new Vector3();

            using (var vb = mesh.VertexBuffer)
            {
                var vertexData = vb.Lock(0, 0, LockFlags.None);
                objectRadius = Geometry.ComputeBoundingSphere(vertexData,
                    mesh.NumberVertices,
                    mesh.VertexFormat,
                    out objectCenter);
                vb.Unlock();
            }

            //Alejar camara rotacional, respecto de su Bounding Sphere
            Camara = new TgcRotationalCamera(new Vector3(0, 0, 0), objectRadius * 4, 5f / objectRadius);
        }

        public override void Render()
        {
            base.PreRender();
            

            //Ver si cambio el modelo elegido por el usuario
            var selectedPath = (string)Modifiers["Mesh"];
            if (selectedPath != currentMeshFile)
            {
                //cargar nuevo modelo
                currentMeshFile = selectedPath;
                loadMesh(currentMeshFile);

                //Actualizar contadores de triangulos y vertices
                UserVars.setValue("Vertices", mesh.NumberVertices);
                UserVars.setValue("Triangles", mesh.NumberFaces);
            }

            //Renderizar la malla.
            //Hay que renderizar cada subset por separado
            for (var i = 0; i < meshMaterials.Length; i++)
            {
                D3DDevice.Instance.Device.Material = meshMaterials[i];
                D3DDevice.Instance.Device.SetTexture(0, meshTextures[i]);
                mesh.DrawSubset(i);
            }

            PostRender();
        }

        public override void Dispose()
        {
            

            //Liberar recursos de la malla
            mesh.Dispose();
        }
    }
}