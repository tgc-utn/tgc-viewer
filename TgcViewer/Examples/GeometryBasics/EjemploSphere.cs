using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.GeometryBasics
{
    public class Esfera : TgcExample
    {
        TgcSphere sphere;
        string currentTexture;
        bool useTexture = false;
        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Crear Esfera 3D";
        }

        public override string getDescription()
        {
            return "Muestra como crear una  esfera 3D con la herramienta TgcSphere, cuyos parámetros " +
                "pueden ser modificados. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear esfera
            sphere = new TgcSphere();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            GuiController.Instance.Modifiers.addEnum("base", typeof(TgcSphere.eBasePoly), TgcSphere.eBasePoly.ICOSAHEDRON);
            GuiController.Instance.Modifiers.addBoolean("inflate", "yes", true);
            GuiController.Instance.Modifiers.addInterval("level of detail", new object[] { 0, 1, 2, 3, 4 }, 2);
            GuiController.Instance.Modifiers.addBoolean("edges", "show", false);
            GuiController.Instance.Modifiers.addFloat("radius", 0, 100, 10);
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-180, -180, -180), new Vector3(180, 180, 180), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addBoolean("Use texture", "yes", true);
            GuiController.Instance.Modifiers.addTexture("texture", GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            GuiController.Instance.Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f), new Vector2(0, 0));
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4), new Vector2(1, 1));
           
            GuiController.Instance.Modifiers.addColor("color", Color.White);
            GuiController.Instance.Modifiers.addBoolean("boundingsphere", "show", false);

            GuiController.Instance.UserVars.addVar("Vertices");
            GuiController.Instance.UserVars.addVar("Triangulos");

            GuiController.Instance.RotCamera.CameraDistance = 50;
        }

        /// <summary>
        /// Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateSphere()
        {

            Device d3dDevice = GuiController.Instance.D3dDevice;
            bool bTexture = (bool)GuiController.Instance.Modifiers["Use texture"];
            Color color =(Color)GuiController.Instance.Modifiers["color"];
            sphere.RenderEdges = (bool)GuiController.Instance.Modifiers["edges"];
            sphere.Inflate = (bool)GuiController.Instance.Modifiers["inflate"];
            sphere.BasePoly = (TgcSphere.eBasePoly) GuiController.Instance.Modifiers.getValue("base");
        
            if (bTexture)
            {
                //Cambiar textura
                string texturePath = (string)GuiController.Instance.Modifiers["texture"];
                if (texturePath != currentTexture || !useTexture || (sphere.RenderEdges && sphere.Color!=color))
                {
                    currentTexture = texturePath;
                    sphere.setColor(color);
                    sphere.setTexture(TgcTexture.createTexture(d3dDevice, currentTexture));

                }
            }
            else sphere.setColor(color);


                    useTexture = bTexture;
            
            //Radio, posición y color
            sphere.Radius = (float)GuiController.Instance.Modifiers["radius"];
            sphere.Position = (Vector3)GuiController.Instance.Modifiers["position"];
            sphere.LevelOfDetail = (int)GuiController.Instance.Modifiers["level of detail"];


            //Rotación, converitr a radianes
            Vector3 rotation = (Vector3)GuiController.Instance.Modifiers["rotation"];
            sphere.Rotation = new Vector3(Geometry.DegreeToRadian(rotation.X), Geometry.DegreeToRadian(rotation.Y), Geometry.DegreeToRadian(rotation.Z));

            //Offset de textura
            sphere.UVOffset = (Vector2)GuiController.Instance.Modifiers["offset"];

            //Tiling de textura
            sphere.UVTiling = (Vector2)GuiController.Instance.Modifiers["tiling"];

            //Actualizar valores en la caja.
            sphere.updateValues();
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Actualizar parametros de la caja
            updateSphere();



            GuiController.Instance.UserVars.setValue("Vertices", sphere.VertexCount);
            GuiController.Instance.UserVars.setValue("Triangulos", sphere.TriangleCount);
            //Renderizar caja
            sphere.render();

            //Mostrar Boundingsphere de la caja
            bool boundingsphere = (bool)GuiController.Instance.Modifiers["boundingsphere"];
            if (boundingsphere)
            {
                sphere.BoundingSphere.render();
            }



        }


        public override void close()
        {
            sphere.dispose();
        }






    }






  
}
