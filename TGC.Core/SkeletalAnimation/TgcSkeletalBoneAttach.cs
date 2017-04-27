using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Representa un modelo que se adjunta a un hueso del esqueleto, para que se modifique
    ///     su ubicicación en el espacio en base a las transformaciones del hueso durante la animación.
    ///     El modelo no debe ser transformado por afuera una vez que es adjuntado a un hueso.
    ///     El renderizado del modelo debe hacerse por afuera de la animación esquelética.
    /// </summary>
    public class TgcSkeletalBoneAttach
    {
        /// <summary>
        ///     Crear un modelo adjunto a un hueso, vacio
        /// </summary>
        public TgcSkeletalBoneAttach()
        {
        }

        /// <summary>
        ///     Crear un modelo adjunto a un hueso
        /// </summary>
        /// <param name="model">Modelo a adjuntar</param>
        /// <param name="bone">Hueso al cual adjuntarse</param>
        /// <param name="offset">Offset desde el cual el modelo sigue al hueso</param>
        public TgcSkeletalBoneAttach(TgcMesh mesh, TgcSkeletalBone bone, TGCMatrix offset)
        {
            Bone = bone;
            Mesh = mesh;
            Offset = offset;
            updateValues();
        }

        /// <summary>
        ///     Modelo que se adjunta al hueso
        /// </summary>
        public TgcMesh Mesh { get; set; }

        /// <summary>
        ///     Hueso al cual se le adjunta un modelo
        /// </summary>
        public TgcSkeletalBone Bone { get; set; }

        /// <summary>
        ///     Desplazamiento desde el cual el modelo sigue al hueso
        /// </summary>
        public TGCMatrix Offset { get; set; }

        /// <summary>
        ///     Configurar modelo
        /// </summary>
        private void setMesh(TgcMesh mesh)
        {
            Mesh = mesh;
            Mesh.AutoTransformEnable = false;
            Mesh.Transform = Bone.MatFinal;
        }

        /// <summary>
        ///     Actualiza los valores del Attachment en base a los parámetros configurados.
        ///     Debe ejecutarse para que las modificaciones realmente se utilicen.
        /// </summary>
        public void updateValues()
        {
            Mesh.AutoTransformEnable = false;
            updateMeshTransform(TGCMatrix.Identity);
        }

        /// <summary>
        ///     Actualiza la transformacion del modelo en base al a transformacion actual
        ///     del hueso y el offset configurado
        /// </summary>
        internal void updateMeshTransform(TGCMatrix meshTransform)
        {
            Mesh.Transform = Offset * Bone.MatFinal * meshTransform;
        }
    }
}