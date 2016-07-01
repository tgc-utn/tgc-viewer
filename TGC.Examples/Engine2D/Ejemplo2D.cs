using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Ejemplo2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animacion, Sprites, Primitivas
    ///     MiniJuego de nave con asteroides.
    ///     Muestra como utilizar las herramientas de dibujado 2D de DirectX.
    ///     Muestra como utilizar eventos del mouse y deteccion de colisiones 2D.
    ///     Autor: Leandro Barbagallo, Matias Leone
    /// </summary>
    public class Ejemplo2D : TGCExampleViewer
    {
        private GameManager gameManager;

        public Ejemplo2D(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "2D";
            Name = "Ejemplo 2D";
            Description = "Ejemplo 2D utilizando Sprites. Hacer clic con el mouse para mover la nave.";
        }

        public override void Init()
        {
            gameManager = GameManager.Instance;

            gameManager.Init(MediaDir + "Engine2D\\", UserVars, Input);
        }

        public override void Update()
        {
            PreUpdate();
            gameManager.Update(ElapsedTime);
        }

        public override void Render()
        {
            PreRender();

            gameManager.Render(ElapsedTime);

            PostRender();
        }

        public override void Dispose()
        {
            gameManager.Dispose();
        }
    }
}