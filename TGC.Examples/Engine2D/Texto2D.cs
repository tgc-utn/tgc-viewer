using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Text;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Texto2D:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 2 - Conceptos Basicos de 2D - Primitivas
    ///     Muestra como crear texto 2D con DirectX.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class Texto2D : TGCExampleViewer
    {
        private TgcText2D text1;
        private TgcText2D text2;
        private TgcText2D text3;

        public Texto2D(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "2D";
            Name = "Texto 2D";
            Description = "Muestra como crear texto 2D con DirectX.";
        }

        public override void Init()
        {
            //Crear texto 1, basico
            text1 = new TgcText2D();
            text1.Text = "Texto de prueba";

            //Crear texto 2, especificando color, alineacion, posicion, tamano y fuente.
            text2 = new TgcText2D();
            text2.Text = "Texto largo que no entra en el ancho especificado, y se hace WordWrap.";
            text2.Color = Color.BlueViolet;
            text2.Align = TgcText2D.TextAlign.LEFT;
            text2.Position = new Point(300, 100);
            text2.Size = new Size(300, 100);
            text2.changeFont(new Font("TimesNewRoman", 25, FontStyle.Bold | FontStyle.Italic));

            //Crear texto 3, especificando color, alineacion, posicion y tamano.
            text3 = new TgcText2D();
            text3.Text = "Texto alineado a la derecha con color.";
            text3.Align = TgcText2D.TextAlign.RIGHT;
            text3.Position = new Point(50, 50);
            text3.Size = new Size(300, 100);
            text3.Color = Color.Gold;
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
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
            text1.Dispose();
            text2.Dispose();
            text3.Dispose();
        }
    }
}