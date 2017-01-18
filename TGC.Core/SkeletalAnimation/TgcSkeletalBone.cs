using Microsoft.DirectX;
using TGC.Core.Mathematica;

namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Hueso del esqueleto
    /// </summary>
    public class TgcSkeletalBone
    {
        public TgcSkeletalBone(int index, string name, TGCVector3 startPosition, Quaternion startRotation)
        {
            Index = index;
            Name = name;
            StartPosition = startPosition;
            StartRotation = startRotation;

            MatLocal = TGCMatrix.RotationQuaternion(StartRotation) * TGCMatrix.Translation(StartPosition);
        }

        /// <summary>
        ///     Posicion inicial del hueso
        /// </summary>
        public TGCVector3 StartPosition { get; }

        /// <summary>
        ///     Rotacion inicial del hueso
        /// </summary>
        public Quaternion StartRotation { get; }

        /// <summary>
        ///     Matriz local de transformacion
        /// </summary>
        public TGCMatrix MatLocal { get; set; }

        /// <summary>
        ///     Matriz final de transformacion
        /// </summary>
        public TGCMatrix MatFinal { get; set; }

        /// <summary>
        ///     Matriz de transformacion inversa de la posicion inicial del hueso, para la animacion actual
        /// </summary>
        public TGCMatrix MatInversePose { get; set; }

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