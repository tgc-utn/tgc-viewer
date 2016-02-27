using System.Drawing;
using Microsoft.DirectX;
using TgcViewer;
using TGC.Core.Example;
using TGC.Core.Utils;

namespace Examples.Matematica
{
    /// <summary>
    ///     Ejemplo EjemploTransformaciones:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     # Unidad 3 - Conceptos Básicos de 3D - Anexo matemática 3D
    ///     Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.
    ///     Muestra distintas operaciones con matrices de transformacion
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTransformaciones : TgcExample
    {
        public override string getCategory()
        {
            return "Matematica";
        }

        public override string getName()
        {
            return "Transformaciones";
        }

        public override string getDescription()
        {
            return
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios. Muestra distintas operaciones con matrices de transformacion.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            // 1) Crear una matriz de transformacion (de 4x4 para 3D) con la identidad
            var m = Matrix.Identity;

            // 2) Crear una matriz de transformacion para traslacion
            var translate = Matrix.Translation(new Vector3(100, -5, 0));

            // 3) Crear una matriz de escalado para traslacion
            var scale = Matrix.Scaling(new Vector3(2, 4, 2));

            // 4) Crear una matriz de rotacion para traslacion
            var angleY = FastMath.PI_HALF; //En radianes
            var angleX = FastMath.ToRad(60); //De grados a radianes
            var angleZ = FastMath.PI/14;
            var rotation = Matrix.RotationYawPitchRoll(angleY, angleX, angleZ); //Ojo con el orden de los angulos

            // 5) Combinar varias matrices en una sola. El orden depende del movimiento que se quiera lograr
            var movimientoFinal = scale*rotation*translate;

            // 6) Transformar un punto en base al movimiento de una matriz de transformacion
            var p = new Vector3(10, 5, 10);
            var transformedVec4 = Vector3.Transform(p, movimientoFinal);
                //Devuelve un Vector4 poque estan las coordenadas homogeneas
            var transformedVec3 = new Vector3(transformedVec4.X, transformedVec4.Y, transformedVec4.Z);
                //Ignoramos la componente W

            // 7) Setear la matriz de World de DirectX
            d3dDevice.Transform.World = movimientoFinal;

            // 8) Crear una matriz de View mirando hacia un determinado punto y aplicarla a DirectX
            var posicionCamara = new Vector3(20, 10, 0);
            var haciaDondeMiro = new Vector3(0, 0, 0);
            var upVector = new Vector3(0, 1, 0); //Indica donde es arriba y donde abajo
            var viewMatrix = Matrix.LookAtLH(posicionCamara, haciaDondeMiro, upVector);
            d3dDevice.Transform.View = viewMatrix;

            // 9) Crear una matriz de proyeccion y aplicarla en DirectX
            var fieldOfViewY = FastMath.ToRad(45.0f);
            var aspectRatio = (float) GuiController.Instance.Panel3d.Width/GuiController.Instance.Panel3d.Height;
            var zNearPlaneDistance = 1f;
            var zFarPlaneDistance = 10000f;
            var projection = Matrix.PerspectiveFovLH(fieldOfViewY, aspectRatio, zNearPlaneDistance, zFarPlaneDistance);
            d3dDevice.Transform.Projection = projection;

            // 10) Proyectar manualmente un punto 3D a la pantalla (lo que normalmente se hace dentro del vertex shader)
            var q = new Vector3(100, -15, 2);
            var worldViewProj = d3dDevice.Transform.World*d3dDevice.Transform.View*d3dDevice.Transform.Projection;
                //Obtener la matriz final
            var projectedPoint = Vector3.Transform(q, worldViewProj); //Proyectar
            //Dividir por w
            projectedPoint.X /= projectedPoint.W;
            projectedPoint.Y /= projectedPoint.W;
            projectedPoint.Z /= projectedPoint.W; //La z solo es necesaria si queremos hacer Depth-testing
            //Pasarlo a screen-space
            var screenPoint = new Point(
                (int) (0.5f + (p.X + 1)*0.5f*d3dDevice.Viewport.Width),
                (int) (0.5f + (1 - p.Y)*0.5f*d3dDevice.Viewport.Height)
                );

            // 11) Con vertir de un punto 2D a uno 3D, al revez de lo que se hizo antes (normalmente para hacer picking con el mouse)
            var screenPoint2 = new Point(15, 100); //punto 2D de la pantalla
            var vAux = new Vector3();
            vAux.X = (2.0f*screenPoint2.X/d3dDevice.Viewport.Width - 1)/d3dDevice.Transform.Projection.M11;
            vAux.Y = -(2.0f*screenPoint2.Y/d3dDevice.Viewport.Height - 1)/d3dDevice.Transform.Projection.M22;
            vAux.Z = 1.0f;
            var inverseView = Matrix.Invert(d3dDevice.Transform.View); //Invertir ViewMatrix
            var origin = new Vector3(inverseView.M41, inverseView.M42, inverseView.M43);
            var direction = new Vector3(
                vAux.X*inverseView.M11 + vAux.Y*inverseView.M21 + vAux.Z*inverseView.M31,
                vAux.X*inverseView.M12 + vAux.Y*inverseView.M22 + vAux.Z*inverseView.M32,
                vAux.X*inverseView.M13 + vAux.Y*inverseView.M23 + vAux.Z*inverseView.M33
                );
            //Con origin y direction formamos una recta que hay que buscar interseccion contra todos los objetos del escenario y quedarnos con la mas cercana a la camara

            GuiController.Instance.Text3d.drawText(
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.", 5, 50,
                Color.Yellow);
        }

        public override void close()
        {
        }
    }
}