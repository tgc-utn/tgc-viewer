using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.TextureMapping
{
    /// <summary>
    ///     Ejemplo EjemploTextureFiltering:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 4 - Texturas e Iluminación - Texture Filtering
    ///     Muestra los distintos modos de Texture Filtering de DirectX
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTextureFiltering : TgcExample
    {
        private TgcBox box;
        private string lastFiltering;
        private string lastTexture;

        public EjemploTextureFiltering(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "TextureMapping";
            Name = "TextureFiltering";
            Description = "Muestra los distintos modos de Texture Filtering que se puede elegir en DirectX.";
        }

        public override void Init()
        {
            box = TgcBox.fromSize(new Vector3(300, 100, 150));

            lastTexture = MediaDir + "Texturas\\Quake\\TexturePack2\\concrete1_4.jpg";
            Modifiers.addTexture("Texture", lastTexture);
            changeBoxTexure(D3DDevice.Instance.Device, box, lastTexture);

            lastFiltering = "Linear";
            Modifiers.addInterval("Filtering", new[] { "Nearest", "Linear", "Bilinear", "Anisotropic" }, 0);
            changeTextureFiltering(D3DDevice.Instance.Device, lastFiltering);

            Camara = new TgcFpsCamera(new Vector3(-54.93998f, 2f, -1.118192f), 150f, 500f);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        private void changeBoxTexure(Device d3dDevice, TgcBox box, string texturePath)
        {
            var t = TgcTexture.createTexture(d3dDevice, texturePath);
            box.setTexture(t);
        }

        private void changeTextureFiltering(Device d3dDevice, string filtering)
        {
            if (filtering == "Nearest")
            {
                d3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Point);
            }
            else if (filtering == "Linear")
            {
                d3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);
            }
            else if (filtering == "Bilinear")
            {
                d3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.GaussianQuad);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.GaussianQuad);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.GaussianQuad);
            }
            else if (filtering == "Anisotropic")
            {
                d3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Anisotropic);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Anisotropic);
                d3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Anisotropic);
            }
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            var currentFiltering = (string)Modifiers["Filtering"];
            if (currentFiltering != lastFiltering)
            {
                lastFiltering = currentFiltering;
                changeTextureFiltering(D3DDevice.Instance.Device, currentFiltering);
            }

            var currentTexture = (string)Modifiers["Texture"];
            if (currentTexture != lastTexture)
            {
                lastTexture = currentTexture;
                changeBoxTexure(D3DDevice.Instance.Device, box, lastTexture);
            }

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