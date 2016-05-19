using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Fog;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Fog
{
    /// <summary>
    ///     Ejemplo EfectoNiebla
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Conceptos Básicos de 3D - Fog
    ///     Muestra como utilizar los efectos de niebla provistos por el Pipeline, a través de la herramienta TgcFog
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EfectoNiebla : TgcExample
    {
        private TgcBox box;
        private TgcFog fog;

        public EfectoNiebla(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Fog";
            Name = "Efecto Niebla";
            Description = "Muestra como utilizar el efecto niebla y como configurar sus diversos atributos.";
        }

        public override void Init()
        {
            //Crear caja
            box = TgcBox.fromSize(new Vector3(100, 100, 100),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg"));

            //Camara rotacional
            ((TgcRotationalCamera)Camara).targetObject(box.BoundingBox);

            //Modifiers para configurar valores de niebla
            Modifiers.addBoolean("Enabled", "Enabled", true);
            Modifiers.addFloat("startDistance", 1, 1000, 100);
            Modifiers.addFloat("endDistance", 1, 1000, 500);
            Modifiers.addFloat("density", 1, 10, 1);
            Modifiers.addColor("color", Color.Gray);

            fog = new TgcFog();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Cargar valores de niebla
            fog.Enabled = (bool)Modifiers["Enabled"];
            fog.StartDistance = (float)Modifiers["startDistance"];
            fog.EndDistance = (float)Modifiers["endDistance"];
            fog.Density = (float)Modifiers["density"];
            fog.Color = (Color)Modifiers["color"];

            //Actualizar valores
            fog.updateValues();

            box.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            box.dispose();
        }
    }
}