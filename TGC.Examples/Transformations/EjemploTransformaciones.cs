using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Ejemplo EjemploTransformaciones:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     # Unidad 3 - Conceptos Basicos de 3D - Anexo matematica 3D
    ///     Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios.
    ///     Muestra distintas operaciones con matrices de transformacion
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTransformaciones : TGCExampleViewer
    {
        public EjemploTransformaciones(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Operaciones con matrices";
            Description =
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios. Muestra distintas operaciones con matrices de transformacion.";
        }

        public override void Init()
        {
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            // 1) Crear una matriz de transformacion (de 4x4 para 3D) con la identidad
            var m = TGCMatrix.Identity;

            // 2) Crear una matriz de transformacion para traslacion
            var translate = TGCMatrix.Translation(new TGCVector3(100, -5, 0));

            // 3) Crear una matriz de escalado para traslacion
            var scale = TGCMatrix.Scaling(new TGCVector3(2, 4, 2));

            // 4) Crear una matriz de rotacion para traslacion
            var angleY = FastMath.PI_HALF; //En radianes
            var angleX = FastMath.ToRad(60); //De grados a radianes
            var angleZ = FastMath.PI / 14;
            var rotation = TGCMatrix.RotationYawPitchRoll(angleY, angleX, angleZ); //Ojo con el orden de los angulos

            // 5) Combinar varias matrices en una sola. El orden depende del movimiento que se quiera lograr
            var movimientoFinal = scale * rotation * translate;

            // 6) Transformar un punto en base al movimiento de una matriz de transformacion
            var p = new TGCVector3(10, 5, 10);
            var transformedVec4 = TGCVector3.Transform(p, movimientoFinal);
            //Devuelve un Vector4 poque estan las coordenadas homogeneas
            var transformedVec3 = new TGCVector3(transformedVec4.X, transformedVec4.Y, transformedVec4.Z);
            //Ignoramos la componente W

            // 7) Setear la matriz de World de DirectX
            D3DDevice.Instance.Device.Transform.World = movimientoFinal.ToMatrix();

            // 8) Crear una matriz de View mirando hacia un determinado punto y aplicarla a DirectX
            var posicionCamara = new TGCVector3(20, 10, 0);
            var haciaDondeMiro = TGCVector3.Empty;
            var upVector = TGCVector3.Up; //Indica donde es arriba y donde abajo
            var viewMatrix = TGCMatrix.LookAtLH(posicionCamara, haciaDondeMiro, upVector);
            D3DDevice.Instance.Device.Transform.View = viewMatrix.ToMatrix();

            // 9) Crear una matriz de proyeccion y aplicarla en DirectX
            var fieldOfViewY = FastMath.ToRad(45.0f);
            var aspectRatio = D3DDevice.Instance.AspectRatio;
            var zNearPlaneDistance = 1f;
            var zFarPlaneDistance = 10000f;
            var projection = TGCMatrix.PerspectiveFovLH(fieldOfViewY, aspectRatio, zNearPlaneDistance, zFarPlaneDistance);
            D3DDevice.Instance.Device.Transform.Projection = projection.ToMatrix();

            // 10) Proyectar manualmente un punto 3D a la pantalla (lo que normalmente se hace dentro del vertex shader)
            var q = new TGCVector3(100, -15, 2);
            var worldViewProj = D3DDevice.Instance.Device.Transform.World * D3DDevice.Instance.Device.Transform.View *
                                D3DDevice.Instance.Device.Transform.Projection;
            //Obtener la matriz final
            var projectedPoint = TGCVector3.Transform(q, TGCMatrix.FromMatrix(worldViewProj)); //Proyectar
            //Dividir por w
            projectedPoint.X /= projectedPoint.W;
            projectedPoint.Y /= projectedPoint.W;
            projectedPoint.Z /= projectedPoint.W; //La z solo es necesaria si queremos hacer Depth-testing
            //Pasarlo a screen-space
            var screenPoint = new Point(
                (int)(0.5f + (p.X + 1) * 0.5f * D3DDevice.Instance.Device.Viewport.Width),
                (int)(0.5f + (1 - p.Y) * 0.5f * D3DDevice.Instance.Device.Viewport.Height));

            // 11) Con vertir de un punto 2D a uno 3D, al revez de lo que se hizo antes (normalmente para hacer picking con el mouse)
            var screenPoint2 = new Point(15, 100); //punto 2D de la pantalla
            var vAux = TGCVector3.Empty;
            vAux.X = (2.0f * screenPoint2.X / D3DDevice.Instance.Device.Viewport.Width - 1) /
                     D3DDevice.Instance.Device.Transform.Projection.M11;
            vAux.Y = -(2.0f * screenPoint2.Y / D3DDevice.Instance.Device.Viewport.Height - 1) /
                     D3DDevice.Instance.Device.Transform.Projection.M22;
            vAux.Z = 1.0f;
            var inverseView = TGCMatrix.Invert(TGCMatrix.FromMatrix(D3DDevice.Instance.Device.Transform.View)); //Invertir ViewMatrix
            var origin = new TGCVector3(inverseView.M41, inverseView.M42, inverseView.M43);
            var direction = new TGCVector3(
                vAux.X * inverseView.M11 + vAux.Y * inverseView.M21 + vAux.Z * inverseView.M31,
                vAux.X * inverseView.M12 + vAux.Y * inverseView.M22 + vAux.Z * inverseView.M32,
                vAux.X * inverseView.M13 + vAux.Y * inverseView.M23 + vAux.Z * inverseView.M33);
            //Con origin y direction formamos una recta que hay que buscar interseccion contra todos los objetos del escenario y quedarnos con la mas cercana a la camara

            DrawText.drawText(
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios.", 5, 50,
                Color.Yellow);

            PostRender();
        }

        public override void Dispose()
        {
            //nada en state.
        }
    }
}