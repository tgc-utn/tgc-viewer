using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Viewer;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo EjemploTeapotConCamara:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Crea un Teapot de DirectX y Box que sirve de piso.
    ///     El ejemplo permite ver como funciona la camara rotacional y
    ///     muestra como renderizar dos Mesh en posiciones distintas utilizando
    ///     Transformaciones
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTeapotConCamara : TgcExample
    {
        private Mesh box;
        private Material materialTeapot;
        private Mesh teapot;

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
            return
                "Crea un Teapot y un Box de DirectX y los renderiza utilizando transformaciones para ubicarlos en distintos lugares.";
        }

        public override void init()
        {
            //Crear mesh de Box de DirectX
            box = Mesh.Box(D3DDevice.Instance.Device, 5, 1, 5);

            //Crear mesh de Teapot de DirectX
            teapot = Mesh.Teapot(D3DDevice.Instance.Device);

            //Crear Material para Teapot
            materialTeapot = new Material();
            materialTeapot.Ambient = Color.Red;
            materialTeapot.Diffuse = Color.Red;
            materialTeapot.Specular = Color.Red;

            //Crear una fuente de Luz en la posición 0 (Cada adaptador de video soporta hasta un límite máximo de luces)
            D3DDevice.Instance.Device.Lights[0].Type = LightType.Directional;
            D3DDevice.Instance.Device.Lights[0].Diffuse = Color.Red;
            D3DDevice.Instance.Device.Lights[0].Position = new Vector3(0, 10, 0);
            D3DDevice.Instance.Device.Lights[0].Direction = new Vector3(0, -1, 0);
            D3DDevice.Instance.Device.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminación Dinámica
            D3DDevice.Instance.Device.RenderState.Lighting = true;

            //Configurar camara rotacional
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 10f;
        }

        public override void render(float elapsedTime)
        {
            //Restaurar la matriz identidad, sino queda sucio del cuadro anterior
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            //Cargar material de Teapot y renderizar malla
            D3DDevice.Instance.Device.Material = materialTeapot;
            teapot.DrawSubset(0);

            //Aplicar transformacion para dibujar el Box mas abajo
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity * Matrix.Translation(0, -2f, 0);
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