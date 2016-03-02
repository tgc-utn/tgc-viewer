using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Viewer;

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

        public override string getCategory()
        {
            return "TextureMapping";
        }

        public override string getName()
        {
            return "TextureFiltering";
        }

        public override string getDescription()
        {
            return "Muestra los distintos modos de Texture Filtering que se puede elegir en DirectX.";
        }

        public override void init()
        {
            box = TgcBox.fromSize(new Vector3(300, 100, 150));

            lastTexture = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\concrete1_4.jpg";
            GuiController.Instance.Modifiers.addTexture("Texture", lastTexture);
            changeBoxTexure(D3DDevice.Instance.Device, box, lastTexture);

            lastFiltering = "Linear";
            GuiController.Instance.Modifiers.addInterval("Filtering",
                new[] { "Nearest", "Linear", "Bilinear", "Anisotropic" }, 0);
            changeTextureFiltering(D3DDevice.Instance.Device, lastFiltering);

            GuiController.Instance.FpsCamera.setCamera(new Vector3(-54.93998f, 2f, -1.118192f),
                new Vector3(-53.94024f, 1.969789f, -1.140801f));
            GuiController.Instance.FpsCamera.MovementSpeed = 150f;
            GuiController.Instance.FpsCamera.Enable = true;
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

        public override void render(float elapsedTime)
        {
            var currentFiltering = (string)GuiController.Instance.Modifiers["Filtering"];
            if (currentFiltering != lastFiltering)
            {
                lastFiltering = currentFiltering;
                changeTextureFiltering(D3DDevice.Instance.Device, currentFiltering);
            }

            var currentTexture = (string)GuiController.Instance.Modifiers["Texture"];
            if (currentTexture != lastTexture)
            {
                lastTexture = currentTexture;
                changeBoxTexure(D3DDevice.Instance.Device, box, lastTexture);
            }

            box.render();
        }

        public override void close()
        {
            box.dispose();
        }
    }
}