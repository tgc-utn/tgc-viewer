using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloBasico:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear un triangulo 3D de la forma mas sencilla,
    ///     especificando vertice por vertice.
    ///     El triangulo se crea con colores por vertice.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class TrianguloBasico : TGCExampleViewer
    {
        //Array de vertices para crear el triangulo
        private CustomVertex.PositionColored[] data;

        public TrianguloBasico(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "Triangulo Basico";
            Description = "Crea un triangulo 3D basico con color. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Definir array de vertices para el triangulo, del tipo Coordendas (X,Y,Z) + Color
            data = new CustomVertex.PositionColored[3];

            //Cargar informacion de vertices. Nesitamos 3 vertices para crear un triangulo
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Configurar camara en rotacion
            Camara = new TgcRotationalCamera(new Vector3(0, 0.5f, 0), 3f);

            //Cargar variables de usuario con alguna informacion util para ver en pantalla
            UserVars.addVar("Cantida de Vertices", data.Length);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Especificar formato de triangulo
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            //Dibujar 1 primitiva (nuestro triangulo)
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);

            PostRender();
        }

        public override void Dispose()
        {
            //nada en state.
        }
    }
}