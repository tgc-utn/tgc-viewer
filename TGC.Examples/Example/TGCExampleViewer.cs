using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Example
{
    public abstract class TGCExampleViewer : TgcExample
    {
        public TGCExampleViewer(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir)
        {
            UserVars = userVars;
            Modifiers = modifiers;
        }

        /// <summary>
        ///     Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicacion.
        /// </summary>
        public TgcUserVars UserVars { get; set; }

        /// <summary>
        ///     Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicacion.
        /// </summary>
        public TgcModifiers Modifiers { get; set; }

        /// <summary>
        ///     Vuelve la configuracion de Render y otras cosas a la configuracion inicial
        /// </summary>
        public override void ResetDefaultConfig()
        {
            base.ResetDefaultConfig();
            UserVars.ClearVars();
            Modifiers.Clear();
        }
    }
}