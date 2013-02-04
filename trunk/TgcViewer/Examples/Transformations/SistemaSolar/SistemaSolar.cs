using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Transformations.SistemaSolar
{
    /// <summary>
    /// Ejemplo SistemaSolar:
    /// Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    /// 
    /// Muestra como concatenar transformaciones para generar movimientos de planetas del sistema solar.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class SistemaSolar : TgcExample
    {
        //Escalas de cada uno de los astros
        readonly Vector3 SUN_SCALE = new Vector3(12, 12, 12);
        readonly Vector3 EARTH_SCALE = new Vector3(3, 3, 3);
        readonly Vector3 MOON_SCALE = new Vector3(0.5f, 0.5f, 0.5f);

        const float AXIS_ROTATION_SPEED = 0.5f;
        const float EARTH_AXIS_ROTATION_SPEED = 10f;
        const float EARTH_ORBIT_SPEED = 2f;
        const float MOON_ORBIT_SPEED = 10f;

        const float EARTH_ORBIT_OFFSET = 700;
        const float MOON_ORBIT_OFFSET = 80;

        TgcMesh sun;
        TgcMesh earth;
        TgcMesh moon;

        float axisRotation = 0f;
        float earthAxisRotation = 0f;
        float earthOrbitRotation = 0f;
        float moonOrbitRotation = 0f;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";

            TgcSceneLoader loader = new TgcSceneLoader();

            //Cargar modelos para el sol, la tierra y la luna. Son esfereas a las cuales le cambiamos la textura
            sun = loader.loadSceneFromFile(sphere).Meshes[0];
            sun.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\SunTexture.jpg") });
            
            earth = loader.loadSceneFromFile(sphere).Meshes[0];
            earth.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\EarthTexture.jpg") });
            
            moon = loader.loadSceneFromFile(sphere).Meshes[0];
            moon.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\MoonTexture.jpg") });
            

            //Deshabilitamos el manejo automático de Transformaciones de TgcMesh, para poder manipularlas en forma customizada
            sun.AutoTransformEnable = false;
            earth.AutoTransformEnable = false;
            moon.AutoTransformEnable = false;


            //Color de fondo
            GuiController.Instance.BackgroundColor = Color.Black;


            //Camara en primera persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(705.2938f, 305.347f, -888.1567f), new Vector3(183.6915f, 19.6596f, -84.2204f));
        }



        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

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
            d3dDevice.Transform.World = Matrix.Identity;
        }

        private Matrix getSunTransform(float elapsedTime)
        {
            Matrix scale = Matrix.Scaling(SUN_SCALE);
            Matrix yRot = Matrix.RotationY(axisRotation);

            return scale * yRot;
        }

        private Matrix getEarthTransform(float elapsedTime)
        {
            Matrix scale = Matrix.Scaling(EARTH_SCALE);
            Matrix yRot = Matrix.RotationY(earthAxisRotation);
            Matrix sunOffset = Matrix.Translation(EARTH_ORBIT_OFFSET, 0, 0);
            Matrix earthOrbit = Matrix.RotationY(earthOrbitRotation);

            return scale * yRot * sunOffset * earthOrbit;
        }

        private Matrix getMoonTransform(float elapsedTime, Matrix earthTransform)
        {
            Matrix scale = Matrix.Scaling(MOON_SCALE);
            Matrix yRot = Matrix.RotationY(axisRotation);
            Matrix earthOffset = Matrix.Translation(MOON_ORBIT_OFFSET, 0, 0);
            Matrix moonOrbit = Matrix.RotationY(moonOrbitRotation);

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
