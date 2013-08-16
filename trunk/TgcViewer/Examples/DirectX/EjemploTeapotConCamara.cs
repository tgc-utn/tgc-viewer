using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Input;

namespace Examples.DirectX
{
    /// <summary>
    /// Ejemplo EjemploTeapotConCamara:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///  
    /// 
    /// Crea un Teapot de DirectX y Box que sirve de piso.
    /// El ejemplo permite ver como funciona la camara rotacional y
    /// muestra como renderizar dos Mesh en posiciones distintas utilizando
    /// Transformaciones
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploTeapotConCamara : TgcExample
    {

        Mesh teapot;
        Mesh box;
        Material materialTeapot;

        public override string getCategory()
        {
            return "DirectX";
        }

        public override string getName()
        {
            return "Teapot + Box";
        }

        public override string getDescription()
        {
            return "Crea un Teapot y un Box de DirectX y los renderiza utilizando transformaciones para ubicarlos en distintos lugares.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear mesh de Box de DirectX
            box = Mesh.Box(d3dDevice, 5, 1, 5);

            //Crear mesh de Teapot de DirectX
            teapot = Mesh.Teapot(d3dDevice);

            //Crear Material para Teapot
            materialTeapot = new Material();
            materialTeapot.Ambient = Color.Red;
            materialTeapot.Diffuse = Color.Red;
            materialTeapot.Specular = Color.Red;

            //Crear una fuente de Luz en la posición 0 (Cada adaptador de video soporta hasta un límite máximo de luces)
            d3dDevice.Lights[0].Type = LightType.Directional;
            d3dDevice.Lights[0].Diffuse = Color.Red;
            d3dDevice.Lights[0].Position = new Vector3(0, 10, 0);
            d3dDevice.Lights[0].Direction = new Vector3(0, -1, 0);
            d3dDevice.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminación Dinámica
            d3dDevice.RenderState.Lighting = true;


            //Configurar camara rotacional
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 10f;
        }




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Restaurar la matriz identidad, sino queda sucio del cuadro anterior
            d3dDevice.Transform.World = Matrix.Identity;

            //Cargar material de Teapot y renderizar malla
            d3dDevice.Material = materialTeapot;
            teapot.DrawSubset(0);

            //Aplicar transformacion para dibujar el Box mas abajo
            d3dDevice.Transform.World = Matrix.Identity * Matrix.Translation(0, -2f, 0);
            //Renderizar Box
            box.DrawSubset(0);
            
        }

        public override void close()
        {
            teapot.Dispose();
            box.Dispose();
        }

    }
}
