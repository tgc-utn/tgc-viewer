using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

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

        public SistemaSolar(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Transformations";
            Name = "Sistema Solar";
            Description =
                "Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.";
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

            //Deshabilitamos el manejo automatico de Transformaciones de TgcMesh, para poder manipularlas en forma personalizada
            sun.AutoTransformEnable = false;
            earth.AutoTransformEnable = false;
            moon.AutoTransformEnable = false;

            //Camara en primera persona
            Camara = new TgcRotationalCamera(new Vector3(0f, 200f, 1000f), 500f, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
            ClearTextures();

            //Actualizar transformacion y renderizar el sol
            sun.Transform = getSunTransform(ElapsedTime);
            sun.render();

            //Actualizar transformacion y renderizar la tierra
            earth.Transform = getEarthTransform(ElapsedTime);
            earth.render();

            //Actualizar transformacion y renderizar la luna
            moon.Transform = getMoonTransform(ElapsedTime, earth.Transform);
            moon.render();

            axisRotation += AXIS_ROTATION_SPEED * ElapsedTime;
            earthAxisRotation += EARTH_AXIS_ROTATION_SPEED * ElapsedTime;
            earthOrbitRotation += EARTH_ORBIT_SPEED * ElapsedTime;
            moonOrbitRotation += MOON_ORBIT_SPEED * ElapsedTime;

            //Limpiamos todas las transformaciones con la Matrix identidad
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            PostRender();
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

        public override void Dispose()
        {
            sun.dispose();
            moon.dispose();
            earth.dispose();
        }
    }
}