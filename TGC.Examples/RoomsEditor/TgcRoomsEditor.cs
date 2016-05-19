using Microsoft.DirectX;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 7 - Técnicas de Optimización - Indoor
    ///     Herramienta para crear escenarios Indoor compuestos por cuartos rectangulares
    ///     que se comunican entre sí.
    ///     Permite crear cuartos rectangulares en un plano 2D con vista superior y, a partir
    ///     de este plano, genera el escenario 3D.
    ///     Calcula automáticamente los lados de los rectángulos que tocan entre sí y genera
    ///     las aberturas necesarias que permien comunicar ambos cuartos (simulando puertas o ventanas)
    ///     Las instrucciones se muestran al hacer clic en el botón "Help" de este Modifier.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TgcRoomsEditor : TgcExample
    {
        private RoomsEditorModifier modifier;

        public TgcRoomsEditor(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines) : base(mediaDir, shadersDir, userVars, modifiers, axisLines)
        {
            this.Category = "Utils";
            this.Name = "RoomsEditor";
            this.Description = "Herramienta para crear escenarios Indoor compuestos por cuartos rectangulares que se comunican entre sí.";
        }

        public override void Init()
        {
            modifier = new RoomsEditorModifier("RoomsEditor", this);
            this.Modifiers.add(modifier);

            this.Camara = new TgcFpsCamera();
            ((TgcFpsCamera)this.Camara).MovementSpeed = 200f;
            ((TgcFpsCamera)this.Camara).JumpSpeed = 200f;
            this.Camara.setCamera(new Vector3(133.0014f, 264.8258f, -119.0311f),
                new Vector3(498.1584f, -299.4199f, 621.433f));
        }

        public override void Update(float elapsedTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);

            foreach (var room in modifier.Rooms)
            {
                foreach (var wall in room.Walls)
                {
                    wall.render();
                }
            }
        }

        public override void Close()
        {
            base.Close();

            modifier.dispose();
        }

        /// <summary>
        ///     Método que se llama cuando se quiere exportar la informacion de la escena a un XML,
        ///     a través del botón "Custom Export"
        ///     MODIFICAR ESTA SECCION PARA ADAPTARSE A LAS NECESIDADES DEL ALUMNO
        /// </summary>
        internal void customExport(string savePath)
        {
        }
    }
}