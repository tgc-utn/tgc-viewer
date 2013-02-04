using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace Examples.Shaders
{
    public class EjemploShader: TgcExample
    {

        Dictionary<string,Effect> effects;
        Dictionary<string,Material[]> meshMaterials;
        Dictionary<string,Texture[]> meshTextures;
        Dictionary<string,Mesh> testMeshes;

        float counter;

        string currentSelectionString;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "EjemploShader";
        }

        public override string getDescription()
        {
            return "Ejemplo de shaders";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            testMeshes = new Dictionary<string, Mesh>();
            effects = new Dictionary<string,Effect>();
            meshMaterials = new Dictionary<string, Material[]>();
            meshTextures = new Dictionary<string, Texture[]>();

            
            //Selecciona el ejemplo de shader.
            GuiController.Instance.Modifiers.addInterval("Shader", new String[] { "Basico", "Bandera", "TexturaOnda", "Bordes" }, 3);
            
            //Habilita o deshabilita el remarcado de los bordes de cada triangulo.
            GuiController.Instance.Modifiers.addBoolean("Wireframe", "Enable Wireframe", false);



            testMeshes["Basico"] = Mesh.Teapot(d3dDevice);
            effects["Basico"] = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\Simplest.fx", null, null, ShaderFlags.None, null);

            testMeshes["Bandera"] = loadMesh(GuiController.Instance.ExamplesMediaDir + "Shaders\\flag.x", "Bandera");
            effects["Bandera"] = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\Flag.fx", null, null, ShaderFlags.None, null);

            testMeshes["TexturaOnda"] = loadMesh(GuiController.Instance.ExamplesMediaDir + "Shaders\\tiny.x", "TexturaOnda");
            effects["TexturaOnda"] = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\Filters.fx", null, null, ShaderFlags.None, null);


            testMeshes["Bordes"] = loadMesh(GuiController.Instance.ExamplesMediaDir + "Shaders\\tiny.x", "Bordes");
            effects["Bordes"] = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\Edge.fx", null, null, ShaderFlags.None, null);


            currentSelectionString = (string)GuiController.Instance.Modifiers["Shader"];
            
            effects[currentSelectionString].Technique = "DefaultTechnique";

            


        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            Matrix worldViewProj = d3dDevice.Transform.World * d3dDevice.Transform.View * d3dDevice.Transform.Projection;

            currentSelectionString = (string)GuiController.Instance.Modifiers["Shader"];
            Effect currentEffect = effects[currentSelectionString];
            Mesh currentMesh = testMeshes[currentSelectionString];


            Material[] currentMeshMaterials;
            Texture[] currentMeshTextures;

            if (meshMaterials.ContainsKey(currentSelectionString))
                currentMeshMaterials = meshMaterials[currentSelectionString];
            else
                currentMeshMaterials = null;

            if (meshTextures.ContainsKey(currentSelectionString))
                currentMeshTextures = meshTextures[currentSelectionString];
            else
                currentMeshTextures = null;
            
            int numPasses = currentEffect.Begin(0);

            currentEffect.SetValue("WorldViewProj", worldViewProj);

            if (currentSelectionString == "Bandera" || currentSelectionString == "TexturaOnda" )
               currentEffect.SetValue("Counter", counter);

            for (int i = 0; i < numPasses; i++)
            {
                currentEffect.BeginPass(i);

                int matIterations;
                if (currentMeshMaterials == null)
                    matIterations = 1;
                else
                    matIterations = currentMeshMaterials.Length;

                for (int j = 0; j < matIterations; j++)
                {
                    if( currentMeshMaterials != null)
                        d3dDevice.Material = currentMeshMaterials[j];

                    if( currentMeshTextures != null)
                        d3dDevice.SetTexture(0, currentMeshTextures[j]);
                    
                    if ((bool)GuiController.Instance.Modifiers["Wireframe"])
                    {
                        d3dDevice.RenderState.FillMode = FillMode.WireFrame;
                    }

                    currentMesh.DrawSubset(0);
                    d3dDevice.RenderState.FillMode = FillMode.Solid;
                }

                currentEffect.EndPass();
            }

            currentEffect.End();
            counter++;

        }

        public override void close()
        {

        }

        private Mesh loadMesh(string path, string exampleName)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            ExtendedMaterial[] mtrl;
            Mesh mesh;
            //Cargar mesh con utilidad de DirectX
            mesh = Mesh.FromFile(path, MeshFlags.Managed, d3dDevice, out mtrl);

            //Analizar todos los subset de la malla
            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials[exampleName] = new Material[mtrl.Length];
                meshTextures[exampleName] = new Texture[mtrl.Length];

                //Cargar los material y texturas en un array
                for (int i = 0; i < mtrl.Length; i++)
                {
                    //Cargar material
                    (meshMaterials[exampleName])[i] = mtrl[i].Material3D;

                    //Si hay textura, intentar cargarla
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename !=
                        string.Empty))
                    {
                        //Cargar textura con TextureLoader
                        (meshTextures[exampleName])[i] = TextureLoader.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders" + "\\" +
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

            return mesh;
        }

    }
}
