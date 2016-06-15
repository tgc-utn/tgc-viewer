using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloBasico:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear un triángulo 3D de la forma más sencilla,
    ///     especificando vértice por vértice.
    ///     El triángulo se crea con colores por vértice.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TrianguloBasico : TgcExample
    {
        //Array de vértices para crear el triángulo
        private CustomVertex.PositionColored[] data;

        public TrianguloBasico(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "Triangulo Básico";
            Description = "Crea un triangulo 3D básico con color. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Definir array de vertices para el triangulo, del tipo Coordendas (X,Y,Z) + Color
            data = new CustomVertex.PositionColored[3];

            //Cargar información de vertices. Nesitamos 3 vertices para crear un triángulo
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
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            //Especificar formato de triangulo
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            //Dibujar 1 primitiva (nuestro triangulo)
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);

            helperPostRender();
        }
        public override void Dispose()
        {
            //nada en state.
        }
    }
}