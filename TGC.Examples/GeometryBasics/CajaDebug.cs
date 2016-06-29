using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CajaDebug
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una caja 3D WireFrame, en la cual solo se ven sus aristas, pero no sus caras.
    ///     Se utiliza la herramienta TgcDebugBox.
    ///     Cada arista es un Box rectangular.
    ///     Es util para hacer debug de ciertas estructuras.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class CajaDebug : TGCExampleViewer
    {
        private TgcDebugBox debugBox;

        public CajaDebug(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "Caja Debug";
            Description =
                "Muestra como crear una caja que solo renderiza sus aristas, y no sus caras. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear caja debug vacia
            debugBox = new TgcDebugBox();

            //Modifiers para variar sus parametros
            Modifiers.addVertex3f("size", new Vector3(0, 0, 0), new Vector3(100, 100, 100), new Vector3(20, 20, 20));
            Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            Modifiers.addColor("color", Color.BurlyWood);

            Camara = new TgcRotationalCamera(new Vector3(), 50f);
        }

        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            var size = (Vector3)Modifiers["size"];
            var position = (Vector3)Modifiers["position"];
            var thickness = (float)Modifiers["thickness"];
            var color = (Color)Modifiers["color"];

            //Actualizar valores en la caja.
            debugBox.setPositionSize(position, size);
            debugBox.Thickness = thickness;
            debugBox.Color = color;
            debugBox.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar parametros de la caja
            updateBox();

            debugBox.render();

            PostRender();
        }

        public override void Dispose()
        {
            debugBox.dispose();
        }
    }
}