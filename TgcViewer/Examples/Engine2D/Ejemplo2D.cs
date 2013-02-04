using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using Microsoft.DirectX;

using System.Drawing;

namespace Examples.Engine2D 
{
    /// <summary>
    /// Ejemplo Ejemplo2D:
    /// Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animación, Sprites, Primitivas
    /// 
    /// MiniJuego de nave con asteroides.
    /// Muestra como utilizar las herramientas de dibujado 2D de DirectX.
    /// Muestra como utilizar eventos del mouse y detección de colisiones 2D.
    /// 
    /// Autor: Leandro Barbagallo, Matías Leone
    /// 
    /// </summary>
    public class Ejemplo2D : TgcExample
    {
        GameManager gameManager;

        public override string getCategory()
        {
            return "2D";
        }

        public override string getName()
        {
            return "Ejemplo 2D";
        }

        public override string getDescription()
        {
            return "Ejemplo 2D utilizando Sprites";
        }

        public override void init()
        {

            gameManager = GameManager.Instance;

            gameManager.Init(this.ExampleDir);

        }

        public override void close()
        {

        }

        public override void render(float elapsedTime)
        {
            
            gameManager.Update(elapsedTime);


            gameManager.Render(elapsedTime);

        }
  
    }
}
