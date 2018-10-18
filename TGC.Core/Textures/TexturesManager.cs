using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Core.Textures
{
    /// <summary>
    ///     Herrramienta para administrar las texturas cargadas en el Device.
    ///     Antes evita hacer device.SetTexture() innecesarios, dado que es una operación
    ///     bastante costosa.
    ///     Pero ahora quedo deprecada esa parte, porque DirectX hace ese control internamente y no
    ///     tiene sentido hacerlo.
    /// </summary>
    public class TexturesManager
    {
        /// <summary>
        ///     Cantidad de texturas simultaneas soportadas por DirectX
        /// </summary>
        public static readonly int DIRECTX_MULTITEXTURE_COUNT = 8;

        /// <summary>
        /// Constructor privado para el Singleton.
        /// </summary>
        private TexturesManager() { }

        /// <summary>
        /// Permite acceder a la instancia del Singleton.
        /// </summary>
        public static TexturesManager Instance { get; } = new TexturesManager();

        /// <summary>
        ///     Carga una textura en el Stage especificado.
        ///     Si la textura es null, es similar aa hacer clear()
        /// </summary>
        /// <param name="stage">Stage en el cual configurar la textura</param>
        /// <param name="texture">Textura a configurar</param>
        public void set(int stage, TgcTexture texture)
        {
            D3DDevice.Instance.Device.SetTexture(stage, texture.D3dTexture);
        }

        /// <summary>
        ///     Carga una textura como parámetro de un Shader
        /// </summary>
        /// <param name="effect">Shader</param>
        /// <param name="parameterName">Nombre del parámetro en el Shader</param>
        /// <param name="texture">Textura a aplicar</param>
        public void shaderSet(Effect effect, string parameterName, TgcTexture texture)
        {
            effect.SetValue(parameterName, texture.D3dTexture);
        }

        /// <summary>
        ///     Limpiar la textura de un Stage particular
        /// </summary>
        public void clear(int stage)
        {
            D3DDevice.Instance.Device.SetTexture(stage, null);
        }

        /// <summary>
        ///     Limpiar las texturas de todos los Stages
        /// </summary>
        public void clearAll()
        {
            for (var i = 0; i < DIRECTX_MULTITEXTURE_COUNT; i++)
            {
                clear(i);
            }
        }
    }
}