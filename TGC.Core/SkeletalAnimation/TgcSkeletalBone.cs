using Microsoft.DirectX;

namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Hueso del esqueleto
    /// </summary>
    public class TgcSkeletalBone
    {
        public TgcSkeletalBone(int index, string name, Vector3 startPosition, Quaternion startRotation)
        {
            Index = index;
            Name = name;
            StartPosition = startPosition;
            StartRotation = startRotation;

            MatLocal = Matrix.RotationQuaternion(StartRotation) * Matrix.Translation(StartPosition);
        }

        /// <summary>
        ///     Posicion inicial del hueso
        /// </summary>
        public Vector3 StartPosition { get; }

        /// <summary>
        ///     Rotacion inicial del hueso
        /// </summary>
        public Quaternion StartRotation { get; }

        /// <summary>
        ///     Matriz local de transformacion
        /// </summary>
        public Matrix MatLocal { get; set; }

        /// <summary>
        ///     Matriz final de transformacion
        /// </summary>
        public Matrix MatFinal { get; set; }

        /// <summary>
        ///     Matriz de transformacion inversa de la posicion inicial del hueso, para la animacion actual
        /// </summary>
        public Matrix MatInversePose { get; set; }

        /// <summary>
        ///     Posición del hueso dentro del array de huesos de todo el esqueleto
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Nombre del hueso
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Hueso padre. Es null si no tiene
        /// </summary>
        public TgcSkeletalBone ParentBone { get; set; }

        public override string ToString()
        {
            return "Bone: " + Name;
        }
    }
}