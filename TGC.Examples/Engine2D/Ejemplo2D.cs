using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Ejemplo2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animación, Sprites, Primitivas
    ///     MiniJuego de nave con asteroides.
    ///     Muestra como utilizar las herramientas de dibujado 2D de DirectX.
    ///     Muestra como utilizar eventos del mouse y detección de colisiones 2D.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class Ejemplo2D : TgcExample
    {
        private GameManager gameManager;

        public Ejemplo2D(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "2D";
            Name = "Ejemplo 2D";
            Description = "Ejemplo 2D utilizando Sprites. Hacer clic con el mouse para mover la nave.";
        }

        public override void Init()
        {
            gameManager = GameManager.Instance;

            gameManager.Init(MediaDir + "Engine2D\\", UserVars);
        }

        public override void Update()
        {
            base.PreUpdate();
            gameManager.Update(ElapsedTime);
        }

        public override void Render()
        {
            base.PreRender();
            
            gameManager.Render(ElapsedTime);

            base.PostRender();
        }

        public override void Dispose()
        {
            gameManager.Dispose();
        }
    }
}