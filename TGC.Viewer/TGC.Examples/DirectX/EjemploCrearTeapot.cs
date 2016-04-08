using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Viewer;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo EjemploCrearTeapot:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación Dinámica, Material
    ///     Crea una malla de Teapot (tetera) que viene pre-fabricada en DirectX.
    ///     A esta malla se le agrega un Material y se configura una fuente de luz
    ///     para mostrar como se utiliza el esquema de iluminación dinámica.
    ///     El Teapot gira sobre los distintso ejes, en base a los valores especificados
    ///     por el usuario en los Modifiers
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploCrearTeapot : TgcExample
    {
        private float angleX;
        private float angleY;
        private float angleZ;
        private Material material;
        private Mesh mesh;

        public override string getCategory()
        {
            return "DirectX";
        }

        public override string getName()
        {
            return "Teapot + Light";
        }

        public override string getDescription()
        {
            return "Crea un Teapot de DirectX con iluminación dinámica que gira sobre los tres ejes";
        }

        public override void init()
        {
            //Crear Teapot
            mesh = Mesh.Teapot(D3DDevice.Instance.Device);

            //Crear Material
            material = new Material();
            material.Ambient = Color.Blue;
            material.Diffuse = Color.Green;
            material.Specular = Color.Red;

            //Crear una fuente de Luz en la posición 0 (Cada adaptador de video soporta hasta un límite máximo de luces)
            D3DDevice.Instance.Device.Lights[0].Type = LightType.Directional;
            D3DDevice.Instance.Device.Lights[0].Diffuse = Color.Yellow;
            D3DDevice.Instance.Device.Lights[0].Position = new Vector3(0, 10, 0);
            D3DDevice.Instance.Device.Lights[0].Direction = new Vector3(0, -1, 0);
            D3DDevice.Instance.Device.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminación Dinámica
            D3DDevice.Instance.Device.RenderState.Lighting = true;

            //Configurar camara rotacional
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 10f);

            //Modifiers para ángulos de rotación
            GuiController.Instance.Modifiers.addFloat("angleX", 0, 10, 0);
            GuiController.Instance.Modifiers.addFloat("angleY", 0, 10, 1);
            GuiController.Instance.Modifiers.addFloat("angleZ", 0, 10, 0);

            //Modifiers para cambiar de rotacion de Euler a Quaternion
            GuiController.Instance.Modifiers.addBoolean("quaternion", "Use Quaternion?", false);

            //Modifier para color de Teapot
            GuiController.Instance.Modifiers.addColor("color", Color.Green);
        }

        public override void render(float elapsedTime)
        {
            //Obtener valores de Modifiers
            var vAngleX = (float)GuiController.Instance.Modifiers["angleX"];
            var vAngleY = (float)GuiController.Instance.Modifiers["angleY"];
            var vAngleZ = (float)GuiController.Instance.Modifiers["angleZ"];

            //Convertir a radianes
            vAngleX = Geometry.DegreeToRadian(vAngleX);
            vAngleY = Geometry.DegreeToRadian(vAngleY);
            vAngleZ = Geometry.DegreeToRadian(vAngleZ);

            //Acumular rotacion actual, sin pasarnos de una vuelta entera
            var doublePI = (float)Math.PI * 2f;
            angleX = (angleX + vAngleX) % doublePI;
            angleY = (angleY + vAngleY) % doublePI;
            angleZ = (angleZ + vAngleZ) % doublePI;

            //Ver si hay que usar Quaternions
            var useQuat = (bool)GuiController.Instance.Modifiers["quaternion"];

            //Rotación Euler
            if (!useQuat)
            {
                D3DDevice.Instance.Device.Transform.World = Matrix.RotationYawPitchRoll(angleY, angleX, angleZ);
            }
            //Rotación Quaternion
            else
            {
                var q = Quaternion.RotationYawPitchRoll(angleY, angleX, angleZ);
                D3DDevice.Instance.Device.Transform.World = Matrix.RotationQuaternion(q);
            }

            //Variar el color de Diffuse del Material
            material.Diffuse = (Color)GuiController.Instance.Modifiers["color"];
            D3DDevice.Instance.Device.Material = material;

            //Renderizar malla
            mesh.DrawSubset(0);
        }

        public override void close()
        {
            mesh.Dispose();
        }
    }
}