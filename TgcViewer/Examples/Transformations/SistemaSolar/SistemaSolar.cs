using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.Transformations.SistemaSolar
{
    /// <summary>
    ///     Ejemplo SistemaSolar:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    ///     Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class SistemaSolar : TgcExample
    {
        private const float AXIS_ROTATION_SPEED = 0.5f;
        private const float EARTH_AXIS_ROTATION_SPEED = 10f;
        private const float EARTH_ORBIT_SPEED = 2f;
        private const float MOON_ORBIT_SPEED = 10f;

        private const float EARTH_ORBIT_OFFSET = 700;
        private const float MOON_ORBIT_OFFSET = 80;

        private readonly Vector3 EARTH_SCALE = new Vector3(3, 3, 3);
        private readonly Vector3 MOON_SCALE = new Vector3(0.5f, 0.5f, 0.5f);

        //Escalas de cada uno de los astros
        private readonly Vector3 SUN_SCALE = new Vector3(12, 12, 12);

        private float axisRotation;
        private TgcMesh earth;
        private float earthAxisRotation;
        private float earthOrbitRotation;
        private TgcMesh moon;
        private float moonOrbitRotation;

        private TgcMesh sun;

        public override string getCategory()
        {
            return "Transformations";
        }

        public override string getName()
        {
            return "Sistema Solar";
        }

        public override string getDescription()
        {
            return "Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.";
        }

        public override void init()
        {
            var sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";

            var loader = new TgcSceneLoader();

            //Cargar modelos para el sol, la tierra y la luna. Son esfereas a las cuales le cambiamos la textura
            sun = loader.loadSceneFromFile(sphere).Meshes[0];
            sun.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\SunTexture.jpg")
            });

            earth = loader.loadSceneFromFile(sphere).Meshes[0];
            earth.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\EarthTexture.jpg")
            });

            moon = loader.loadSceneFromFile(sphere).Meshes[0];
            moon.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\MoonTexture.jpg")
            });

            //Deshabilitamos el manejo automático de Transformaciones de TgcMesh, para poder manipularlas en forma customizada
            sun.AutoTransformEnable = false;
            earth.AutoTransformEnable = false;
            moon.AutoTransformEnable = false;

            //Color de fondo
            GuiController.Instance.BackgroundColor = Color.Black;

            //Camara en primera persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(705.2938f, 305.347f, -888.1567f),
                new Vector3(183.6915f, 19.6596f, -84.2204f));
        }

        public override void render(float elapsedTime)
        {
            //Actualizar transformacion y renderizar el sol
            sun.Transform = getSunTransform(elapsedTime);
            sun.render();

            //Actualizar transformacion y renderizar la tierra
            earth.Transform = getEarthTransform(elapsedTime);
            earth.render();

            //Actualizar transformacion y renderizar la luna
            moon.Transform = getMoonTransform(elapsedTime, earth.Transform);
            moon.render();

            axisRotation += AXIS_ROTATION_SPEED * elapsedTime;
            earthAxisRotation += EARTH_AXIS_ROTATION_SPEED * elapsedTime;
            earthOrbitRotation += EARTH_ORBIT_SPEED * elapsedTime;
            moonOrbitRotation += MOON_ORBIT_SPEED * elapsedTime;

            //Limpiamos todas las transformaciones con la Matrix identidad
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
        }

        private Matrix getSunTransform(float elapsedTime)
        {
            var scale = Matrix.Scaling(SUN_SCALE);
            var yRot = Matrix.RotationY(axisRotation);

            return scale * yRot;
        }

        private Matrix getEarthTransform(float elapsedTime)
        {
            var scale = Matrix.Scaling(EARTH_SCALE);
            var yRot = Matrix.RotationY(earthAxisRotation);
            var sunOffset = Matrix.Translation(EARTH_ORBIT_OFFSET, 0, 0);
            var earthOrbit = Matrix.RotationY(earthOrbitRotation);

            return scale * yRot * sunOffset * earthOrbit;
        }

        private Matrix getMoonTransform(float elapsedTime, Matrix earthTransform)
        {
            var scale = Matrix.Scaling(MOON_SCALE);
            var yRot = Matrix.RotationY(axisRotation);
            var earthOffset = Matrix.Translation(MOON_ORBIT_OFFSET, 0, 0);
            var moonOrbit = Matrix.RotationY(moonOrbitRotation);

            return scale * yRot * earthOffset * moonOrbit * earthTransform;
        }

        public override void close()
        {
            sun.dispose();
            moon.dispose();
            earth.dispose();
        }
    }
}