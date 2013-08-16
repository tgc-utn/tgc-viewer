using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;

namespace Examples.DirectX
{
    /// <summary>
    /// Ejemplo EjemploCrearTeapot:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminaci�n - Iluminaci�n Din�mica, Material
    /// 
    /// 
    /// Crea una malla de Teapot (tetera) que viene pre-fabricada en DirectX.
    /// A esta malla se le agrega un Material y se configura una fuente de luz
    /// para mostrar como se utiliza el esquema de iluminaci�n din�mica.
    /// El Teapot gira sobre los distintso ejes, en base a los valores especificados
    /// por el usuario en los Modifiers
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploCrearTeapot : TgcExample
    {

        Mesh mesh;
        float angleX = 0f;
        float angleY = 0f;
        float angleZ = 0f;
        Material material;

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
            return "Crea un Teapot de DirectX con iluminaci�n din�mica que gira sobre los tres ejes";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Teapot
            mesh = Mesh.Teapot(d3dDevice);

            //Crear Material
            material = new Material();
            material.Ambient = Color.Blue;
            material.Diffuse = Color.Green;
            material.Specular = Color.Red;

            //Crear una fuente de Luz en la posici�n 0 (Cada adaptador de video soporta hasta un l�mite m�ximo de luces)
            d3dDevice.Lights[0].Type = LightType.Directional;
            d3dDevice.Lights[0].Diffuse = Color.Yellow;
            d3dDevice.Lights[0].Position = new Vector3(0, 10, 0);
            d3dDevice.Lights[0].Direction = new Vector3(0, -1, 0);
            d3dDevice.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminaci�n Din�mica
            d3dDevice.RenderState.Lighting = true;


            //Configurar camara rotacional
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 10f);

            //Modifiers para �ngulos de rotaci�n
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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener valores de Modifiers
            float vAngleX = (float)GuiController.Instance.Modifiers["angleX"];
            float vAngleY = (float)GuiController.Instance.Modifiers["angleY"];
            float vAngleZ = (float)GuiController.Instance.Modifiers["angleZ"];

            //Convertir a radianes
            vAngleX = Geometry.DegreeToRadian(vAngleX);
            vAngleY = Geometry.DegreeToRadian(vAngleY);
            vAngleZ = Geometry.DegreeToRadian(vAngleZ);

            //Acumular rotacion actual, sin pasarnos de una vuelta entera
            float doublePI = (float)Math.PI * 2f;
            angleX = (angleX + vAngleX) % doublePI;
            angleY = (angleY + vAngleY) % doublePI;
            angleZ = (angleZ + vAngleZ) % doublePI;

            //Ver si hay que usar Quaternions
            bool useQuat = (bool)GuiController.Instance.Modifiers["quaternion"];
            
            //Rotaci�n Euler
            if (!useQuat)
            {
                d3dDevice.Transform.World = Matrix.RotationYawPitchRoll(angleY, angleX, angleZ);
            }
            //Rotaci�n Quaternion
            else
            {
                Quaternion q = Quaternion.RotationYawPitchRoll(angleY, angleX, angleZ);
                d3dDevice.Transform.World = Matrix.RotationQuaternion(q);
            }

            


            //Variar el color de Diffuse del Material
            material.Diffuse = (Color)GuiController.Instance.Modifiers["color"];
            d3dDevice.Material = material;

            //Renderizar malla
            mesh.DrawSubset(0);
            
        }

        public override void close()
        {
            mesh.Dispose();
        }

    }
}
