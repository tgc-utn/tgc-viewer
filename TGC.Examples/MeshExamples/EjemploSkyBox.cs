using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo CrearSkyBox.
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - SkyBox
    ///     Muestra como utilizar la herramienta TgcSkyBox para crear
    ///     un cubo de 6 caras con una textura en cada una, que permite
    ///     lograr el efecto de cielo envolvente en la escena.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class CrearSkyBox : TGCExampleViewer
    {
        private TGCBooleanModifier moveWithCameraModifier;

        private TgcSkyBox skyBox;

        public CrearSkyBox(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Mesh Examples";
            Name = "SkyBox";
            Description = "Muestra como utilizar la herramienta TgcSkyBox para crear un cielo envolvente en la escena. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(10000, 10000, 10000);

            //Configurar color
            //skyBox.Color = Color.OrangeRed;

            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox1\\";

            //Configurar las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");

            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.SkyEpsilon = 25f;
            //Inicializa todos los valores para crear el SkyBox
            skyBox.Init();

            //Modifier para mover el skybox con la posicion de la caja con traslaciones.
            moveWithCameraModifier = AddBoolean("moveWithCamera", "Move With Camera", false);

            Camara = new TgcFpsCamera(Input);
        }

        public override void Update()
        {
            PreUpdate();

            //Se cambia el valor por defecto del farplane para evitar cliping de farplane.
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance * 2f).ToMatrix();

            //Se actualiza la posicion del skybox.
            if (moveWithCameraModifier.Value)
                skyBox.Center = Camara.Position;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Camera pos: " + TGCVector3.PrintTGCVector3(Camara.Position), 5, 20, Color.Red);
            //Renderizar SkyBox
            skyBox.Render();

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar recursos del SkyBox
            skyBox.Dispose();
        }
    }
}