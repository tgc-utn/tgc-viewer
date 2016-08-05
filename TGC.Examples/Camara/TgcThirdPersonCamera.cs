using Microsoft.DirectX;
using TGC.Core.Camara;

namespace TGC.Examples.Camara
{
    /// <summary>
    ///     Camara en tercera persona que sigue a un objeto a un determinada distancia.
    /// </summary>
    public class TgcThirdPersonCamera : TgcCamera
    {
        private Vector3 position;

        /// <summary>
        ///     Crear una nueva camara
        /// </summary>
        public TgcThirdPersonCamera()
        {
            resetValues();
        }

        public TgcThirdPersonCamera(Vector3 target, float offsetHeight, float offsetForward) : this()
        {
            Target = target;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
        }

        public TgcThirdPersonCamera(Vector3 target, Vector3 targetDisplacement, float offsetHeight, float offsetForward)
            : this()
        {
            Target = target;
            TargetDisplacement = targetDisplacement;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
        }

        /// <summary>
        ///     Desplazamiento en altura de la camara respecto del target
        /// </summary>
        public float OffsetHeight { get; set; }

        /// <summary>
        ///     Desplazamiento hacia adelante o atras de la camara repecto del target.
        ///     Para que sea hacia atras tiene que ser negativo.
        /// </summary>
        public float OffsetForward { get; set; }

        /// <summary>
        ///     Desplazamiento final que se le hace al target para acomodar la camara en un cierto
        ///     rincon de la pantalla
        /// </summary>
        public Vector3 TargetDisplacement { get; set; }

        /// <summary>
        ///     Rotacion absoluta en Y de la camara
        /// </summary>
        public float RotationY { get; set; }

        /// <summary>
        ///     Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public Vector3 Target { get; set; }

        public override void UpdateCamera(float elapsedTime)
        {
            Vector3 targetCenter;
            CalculatePositionTarget(out position, out targetCenter);
            SetCamera(position, targetCenter);
        }

        /// <summary>
        ///     Carga los valores default de la camara y limpia todos los cálculos intermedios
        /// </summary>
        public void resetValues()
        {
            OffsetHeight = 20;
            OffsetForward = -120;
            RotationY = 0;
            TargetDisplacement = Vector3.Empty;
            Target = Vector3.Empty;
            position = Vector3.Empty;
        }

        /// <summary>
        ///     Configura los valores iniciales de la cámara
        /// </summary>
        /// <param name="target">Objetivo al cual la camara tiene que apuntar</param>
        /// <param name="offsetHeight">Desplazamiento en altura de la camara respecto del target</param>
        /// <param name="offsetForward">Desplazamiento hacia adelante o atras de la camara repecto del target.</param>
        public void setTargetOffsets(Vector3 target, float offsetHeight, float offsetForward)
        {
            Target = target;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
        }

        /// <summary>
        ///     Genera la proxima matriz de view, sin actualizar aun los valores internos
        /// </summary>
        /// <param name="pos">Futura posicion de camara generada</param>
        /// <param name="pos">Futuro centro de camara a generada</param>
        public void CalculatePositionTarget(out Vector3 pos, out Vector3 targetCenter)
        {
            //alejarse, luego rotar y lueg ubicar camara en el centro deseado
            targetCenter = Vector3.Add(Target, TargetDisplacement);
            var m = Matrix.Translation(0, OffsetHeight, OffsetForward) * Matrix.RotationY(RotationY) *
                    Matrix.Translation(targetCenter);

            //Extraer la posicion final de la matriz de transformacion
            pos.X = m.M41;
            pos.Y = m.M42;
            pos.Z = m.M43;
        }

        /// <summary>
        ///     Rotar la camara respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateY(float angle)
        {
            RotationY += angle;
        }
    }
}