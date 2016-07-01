using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploBatchPrimitivesDx
    /// </summary>
    public class EjemploBatchPrimitivesDx : TGCExampleViewer
    {
        private const float boxSize = 3f;
        private const int boxPerSquare = 50;
        private readonly int totalBoxes = boxPerSquare * boxPerSquare;
        private TgcTexture box1Texture;
        private TgcTexture box2Texture;
        private TgcTexture box3Texture;

        private RenderMethod currentRenderMethod;
        private Mesh[] meshes;

        public EjemploBatchPrimitivesDx(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Others";
            Name = "BatchPrimitivesDx";
            Description = "BatchPrimitivesDx";
        }

        public override void Init()
        {
            box1Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg");
            box2Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            box3Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");

            Modifiers.addEnum("Render Method", typeof(RenderMethod), RenderMethod.Unsorted);
            createMeshes(D3DDevice.Instance.Device);

            Camara = new TgcFpsCamera(new Vector3(32.1944f, 42.1327f, -68.7882f), Input);
        }

        private void createMeshes(Device d3dDevice)
        {
            meshes = new Mesh[totalBoxes];
            for (var i = 0; i < meshes.Length; i++)
            {
                meshes[i] = Mesh.Box(d3dDevice, boxSize, boxSize, boxSize);
            }
        }

        private void doRender(RenderMethod renderMethod)
        {
            if (currentRenderMethod != renderMethod)
            {
                currentRenderMethod = renderMethod;
            }

            switch (currentRenderMethod)
            {
                case RenderMethod.Unsorted:
                    doUnsortedRender();
                    break;

                case RenderMethod.Texture_Sort:
                    doTextureSortRender();
                    break;
            }
        }

        /// <summary>
        ///     Renderizar haciendo que se tenga que alternar la textura a proposito
        /// </summary>
        private void doUnsortedRender()
        {
            for (var i = 0; i < boxPerSquare; i++)
            {
                for (var j = 0; j < boxPerSquare; j++)
                {
                    D3DDevice.Instance.Device.GetTexture(0);

                    //Forzar a proposito el cambio de textura
                    D3DDevice.Instance.Device.SetTexture(0, null);
                    D3DDevice.Instance.Device.SetTexture(0, box3Texture.D3dTexture);
                    D3DDevice.Instance.Device.SetTexture(0, box2Texture.D3dTexture);
                    D3DDevice.Instance.Device.SetTexture(0, box1Texture.D3dTexture);

                    D3DDevice.Instance.Device.SetTexture(0, box1Texture.D3dTexture);

                    D3DDevice.Instance.Device.Transform.World = Matrix.Translation(boxSize * 2 * i, 0, boxSize * 2 * j);
                    meshes[i].DrawSubset(0);
                }
            }
        }

        /// <summary>
        ///     Renderizar primero las de la textura 1 y despues la de textura 2, para minimizar los Texture State Change
        /// </summary>
        private void doTextureSortRender()
        {
            //Un solo texture change
            D3DDevice.Instance.Device.SetTexture(0, box1Texture.D3dTexture);

            for (var i = 0; i < boxPerSquare; i++)
            {
                for (var j = 0; j < boxPerSquare; j++)
                {
                    D3DDevice.Instance.Device.Transform.World = Matrix.Translation(boxSize * 2 * i, 0, boxSize * 2 * j);
                    meshes[i].DrawSubset(0);
                }
            }
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var renderMethod = (RenderMethod)Modifiers["Render Method"];
            doRender(renderMethod);

            PostRender();
        }

        public override void Dispose()
        {
            currentRenderMethod = RenderMethod.Unsorted;
            disposeCajas();
        }

        /// <summary>
        ///     Liberar recursos de cajas
        /// </summary>
        private void disposeCajas()
        {
            for (var i = 0; i < meshes.Length; i++)
            {
                meshes[i].Dispose();
            }
        }

        private enum RenderMethod
        {
            Unsorted,
            Texture_Sort
        }
    }
}