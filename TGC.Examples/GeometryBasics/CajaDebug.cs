using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CajaDebug
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una caja 3D WireFrame, en la cual solo se ven sus aristas, pero no sus caras.
    ///     Se utiliza la herramienta TgcDebugBox.
    ///     Cada arista es un Box rectangular.
    ///     Es útil para hacer debug de ciertas estructuras.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class CajaDebug : TgcExample
    {
        private TgcDebugBox debugBox;

        public CajaDebug(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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

            //Modifiers para vararis sus parametros
            Modifiers.addVertex3f("size", new Vector3(0, 0, 0), new Vector3(100, 100, 100), new Vector3(20, 20, 20));
            Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            Modifiers.addColor("color", Color.BurlyWood);

            ((TgcRotationalCamera)Camara).CameraDistance = 50;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Actualiza los parámetros de la caja en base a lo cargado por el usuario
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
            IniciarEscena();
            base.Render();

            //Actualizar parametros de la caja
            updateBox();

            debugBox.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Render();

            debugBox.dispose();
        }
    }
}