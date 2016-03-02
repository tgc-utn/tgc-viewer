using Microsoft.DirectX;
using TGC.Core.Example;
using TGC.Core.Terrain;
using TGC.Viewer;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearSkyBox.
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - SkyBox
    ///     Muestra como utilizar la herramienta TgcSkyBox para crear
    ///     un cubo de 6 caras con una textura en cada una, que permite
    ///     lograr el efecto de cielo envolvente en la escena.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class CrearSkyBox : TgcExample
    {
        private TgcSkyBox skyBox;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "SkyBox";
        }

        public override string getDescription()
        {
            return
                "Muestra como utilizar la herramienta TgcSkyBox para crear un cielo envolvente en la escena. Movimiento con mouse.";
        }

        public override void init()
        {
            var texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox1\\";

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(1000, 1000, 1000);

            //Configurar color
            //skyBox.Color = Color.OrangeRed;

            //Configurar las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");

            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");

            //Actualizar todos los valores para crear el SkyBox
            skyBox.updateValues();

            GuiController.Instance.FpsCamera.Enable = true;
        }

        public override void render(float elapsedTime)
        {
            //Renderizar SkyBox
            skyBox.render();
        }

        public override void close()
        {
            //Liberar recursos del SkyBox
            skyBox.dispose();
        }
    }
}