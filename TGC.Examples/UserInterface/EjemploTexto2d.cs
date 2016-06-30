using System.Drawing;
using TGC.Core._2D;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.UserInterface
{
    /// <summary>
    ///     Ejemplo EjemploTextureFiltering:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 2 - Conceptos Basicos de 2D - Primitivas
    ///     Muestra como crear texto 2D con DirectX.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTexto2d : TGCExampleViewer
    {
        private TgcText2d text1;
        private TgcText2d text2;
        private TgcText2d text3;

        public EjemploTexto2d(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "UserInterface";
            Name = "Texto 2D";
            Description = "Muestra como crear texto 2D con DirectX.";
        }

        public override void Init()
        {
            //Crear texto 1, basico
            text1 = new TgcText2d();
            text1.Text = "Texto de prueba";

            //Crear texto 2, especificando color, alineacion, posicion, tamano y fuente.
            text2 = new TgcText2d();
            text2.Text = "Texto largo que no entra en el ancho especificado, y se hace WordWrap.";
            text2.Color = Color.BlueViolet;
            text2.Align = TgcText2d.TextAlign.LEFT;
            text2.Position = new Point(300, 100);
            text2.Size = new Size(300, 100);
            text2.changeFont(new Font("TimesNewRoman", 25, FontStyle.Bold | FontStyle.Italic));

            //Crear texto 3, especificando color, alineacion, posicion y tamano.
            text3 = new TgcText2d();
            text3.Text = "Texto alineado a la derecha con color.";
            text3.Align = TgcText2d.TextAlign.RIGHT;
            text3.Position = new Point(50, 50);
            text3.Size = new Size(300, 100);
            text3.Color = Color.Gold;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Renderizar los tres textoss
            text1.render();
            text2.render();
            text3.render();

            PostRender();
        }

        public override void Dispose()
        {
            text1.dispose();
            text2.dispose();
            text3.dispose();
        }
    }
}