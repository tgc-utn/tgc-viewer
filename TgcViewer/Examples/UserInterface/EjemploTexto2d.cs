using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils._2D;

namespace Examples
{
    /// <summary>
    /// Ejemplo EjemploTextureFiltering:
    /// Unidades PlayStaticSound:
    ///     # Unidad 2 - Conceptos Básicos de 2D - Primitivas
    /// 
    /// Muestra como crear texto 2D con DirectX.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploTexto2d : TgcExample
    {

        TgcText2d text1;
        TgcText2d text2;
        TgcText2d text3;

        public override string getCategory()
        {
            return "UserInterface";
        }

        public override string getName()
        {
            return "Texto 2D";
        }

        public override string getDescription()
        {
            return "Muestra como crear texto 2D con DirectX.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear texto 1, básico
            text1 = new TgcText2d();
            text1.Text = "Texto de prueba";

            //Crear texto 2, especificando color, alineación, posición, tamaño y fuente.
            text2 = new TgcText2d();
            text2.Text = "Texto largo que no entra en el ancho especificado, y se hace WordWrap.";
            text2.Color = Color.BlueViolet;
            text2.Align = TgcText2d.TextAlign.LEFT;
            text2.Position = new Point(300, 100);
            text2.Size = new Size(300, 100);
            text2.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold | FontStyle.Italic));

            //Crear texto 3, especificando color, alineación, posición y tamaño.
            text3 = new TgcText2d();
            text3.Text = "Texto alineado a la derecha con color.";
            text3.Align = TgcText2d.TextAlign.RIGHT;
            text3.Position = new Point(50, 50);
            text3.Size = new Size(300, 100);
            text3.Color = Color.Gold;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar los tres textoss
            text1.render();
            text2.render();
            text3.render();
        }

        public override void close()
        {
            text1.dispose();
            text2.dispose();
            text3.dispose();
        }

    }
}
