using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Transformations.SistemaSolar
{
    /// <summary>
    ///     Ejemplo SistemaSolar:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Transformaciones
    ///     Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class SistemaSolar : TGCExampleViewer
    {
        private const float AXIS_ROTATION_SPEED = 0.125f;
        private const float EARTH_AXIS_ROTATION_SPEED = 2.5f;
        private const float EARTH_ORBIT_SPEED = 0.5f;
        private const float MOON_ORBIT_SPEED = 2.5f;

        private const float EARTH_ORBIT_OFFSET = 700;
        private const float MOON_ORBIT_OFFSET = 80;

        private readonly TGCVector3 EARTH_SCALE = new TGCVector3(3, 3, 3);
        private readonly TGCVector3 MOON_SCALE = new TGCVector3(0.5f, 0.5f, 0.5f);

        //Escalas de cada uno de los astros
        private readonly TGCVector3 SUN_SCALE = new TGCVector3(12, 12, 12);

        private float axisRotation;
        private TgcMesh earth;
        private float earthAxisRotation;
        private float earthOrbitRotation;
        private TgcMesh moon;
        private float moonOrbitRotation;

        private TgcMesh sun;

        public SistemaSolar(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Sistema Solar";
            Description = "Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.";
        }

        public override void Init()
        {
            var sphere = MediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";

            var loader = new TgcSceneLoader();

            //Cargar modelos para el sol, la tierra y la luna. Son esfereas a las cuales le cambiamos la textura
            sun = loader.loadSceneFromFile(sphere).Meshes[0];
            sun.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "SistemaSolar\\SunTexture.jpg")
            });

            earth = loader.loadSceneFromFile(sphere).Meshes[0];
            earth.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "SistemaSolar\\EarthTexture.jpg")
            });

            moon = loader.loadSceneFromFile(sphere).Meshes[0];
            moon.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "SistemaSolar\\MoonTexture.jpg")
            });

            BackgroundColor = Color.Black;

            //Camara en primera persona
            Camera = new TgcRotationalCamera(new TGCVector3(0f, 200f, 1000f), 500f, Input);
        }

        public override void Update()
        {
            //Actualizar transformacion el sol
            sun.Transform = getSunTransform();
            //Actualizar transformacion la tierra
            earth.Transform = getEarthTransform();
            //Actualizar transformacion la luna
            moon.Transform = getMoonTransform(earth.Transform);

            axisRotation += AXIS_ROTATION_SPEED * ElapsedTime;
            earthAxisRotation += EARTH_AXIS_ROTATION_SPEED * ElapsedTime;
            earthOrbitRotation += EARTH_ORBIT_SPEED * ElapsedTime;
            moonOrbitRotation += MOON_ORBIT_SPEED * ElapsedTime;

            //Limpiamos todas las transformaciones con la TGCMatrix identidad
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();
        }

        public override void Render()
        {
            PreRender();

            //Renderizar el sol
            sun.Render();

            //Renderizar la tierra
            earth.Render();

            //Renderizar la luna
            moon.Render();

            PostRender();
        }

        private TGCMatrix getSunTransform()
        {
            var scale = TGCMatrix.Scaling(SUN_SCALE);
            var yRot = TGCMatrix.RotationY(axisRotation);

            return scale * yRot;
        }

        private TGCMatrix getEarthTransform()
        {
            var scale = TGCMatrix.Scaling(EARTH_SCALE);
            var yRot = TGCMatrix.RotationY(earthAxisRotation);
            var sunOffset = TGCMatrix.Translation(EARTH_ORBIT_OFFSET, 0, 0);
            var earthOrbit = TGCMatrix.RotationY(earthOrbitRotation);

            return scale * yRot * sunOffset * earthOrbit;
        }

        private TGCMatrix getMoonTransform(TGCMatrix earthTransform)
        {
            var scale = TGCMatrix.Scaling(MOON_SCALE);
            var yRot = TGCMatrix.RotationY(axisRotation);
            var earthOffset = TGCMatrix.Translation(MOON_ORBIT_OFFSET, 0, 0);
            var moonOrbit = TGCMatrix.RotationY(moonOrbitRotation);

            return scale * yRot * earthOffset * moonOrbit * earthTransform;
        }

        public override void Dispose()
        {
            sun.Dispose();
            moon.Dispose();
            earth.Dispose();
        }
    }
}