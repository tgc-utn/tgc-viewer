using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
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
        private TgcSkyBox skyBox;

        public CrearSkyBox(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "SkyBox";
            Description =
                "Muestra como utilizar la herramienta TgcSkyBox para crear un cielo envolvente en la escena. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);

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

            //Modifier para ver BoundingBox
            Modifiers.addBoolean("moveWhitCamera", "Move Whit Camera", false);

            Camara = new TgcFpsCamera(Input);
        }

        public override void Update()
        {
            PreUpdate();

            //Se cambia el valor por defecto del farplane
            D3DDevice.Instance.Device.Transform.Projection =
                Matrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView,
                    D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance,
                    D3DDevice.Instance.ZFarPlaneDistance * 2f);

            //Se actualiza la posicion del skybox.
            if ((bool)Modifiers.getValue("moveWhitCamera"))
                skyBox.Center = Camara.Position;
        }

        public override void Render()
        {
            PreRender();

            //Renderizar SkyBox
            skyBox.render();

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar recursos del SkyBox
            skyBox.dispose();
        }
    }
}