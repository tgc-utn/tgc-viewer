using Microsoft.DirectX;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploBatchPrimitives
    /// </summary>
    public class EjemploBatchPrimitives : TgcExample
    {
        private TgcTexture box1Texture;
        private TgcTexture box2Texture;
        private TgcTexture box3Texture;
        private TgcBox[] cajasNivel1;
        private TgcBox[] cajasNivel2;
        private TgcBox[] cajasNivel3;

        private RenderMethod currentRenderMethod;

        public EjemploBatchPrimitives(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Others";
            Name = "BatchPrimitives";
            Description = "BatchPrimitives";
        }

        public override void Init()
        {
            box1Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg");
            box2Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            box3Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");

            Modifiers.addEnum("Render Method", typeof(RenderMethod), RenderMethod.Unsorted);
            createMeshes(25);

            Camara = new TgcFpsCamera(new Vector3(32.1944f, 42.1327f, -68.7882f));
        }

        private void createMeshes(int cajasPorCuadrante)
        {
            var cantCajasPorNivel = cajasPorCuadrante * cajasPorCuadrante;

            //Crear nuevas cajas
            var boxSize = 3f;
            cajasNivel1 = new TgcBox[cantCajasPorNivel];
            cajasNivel2 = new TgcBox[cantCajasPorNivel];
            cajasNivel3 = new TgcBox[cantCajasPorNivel];
            var cajas = 0;
            for (var i = 0; i < cajasPorCuadrante; i++)
            {
                for (var j = 0; j < cajasPorCuadrante; j++)
                {
                    //Crear tres niveles de caja, una abajo y otra arriba, con texturas diferentes
                    cajasNivel1[cajas] = TgcBox.fromSize(new Vector3(i * boxSize, 0, j * boxSize * 1.5f),
                        new Vector3(boxSize, boxSize, boxSize), box1Texture);
                    cajasNivel2[cajas] = TgcBox.fromSize(new Vector3(i * boxSize, boxSize, j * boxSize * 1.5f),
                        new Vector3(boxSize, boxSize, boxSize), box2Texture);
                    cajasNivel3[cajas] = TgcBox.fromSize(new Vector3(i * boxSize, boxSize * 2, j * boxSize * 1.5f),
                        new Vector3(boxSize, boxSize, boxSize), box3Texture);
                    cajas++;
                }
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
        ///     Renderizar haciendo que se tenga que alternar la textura a propósito
        /// </summary>
        private void doUnsortedRender()
        {
            for (var i = 0; i < cajasNivel1.Length; i++)
            {
                cajasNivel1[i].render();
                cajasNivel2[i].render();
                cajasNivel3[i].render();
            }
        }

        /// <summary>
        ///     Renderizar primero las de la textura 1 y despues la de textura 2, para minimizar los Texture State Change
        /// </summary>
        private void doTextureSortRender()
        {
            for (var i = 0; i < cajasNivel1.Length; i++)
            {
                cajasNivel1[i].render();
            }
            for (var i = 0; i < cajasNivel2.Length; i++)
            {
                cajasNivel2[i].render();
            }
            for (var i = 0; i < cajasNivel3.Length; i++)
            {
                cajasNivel3[i].render();
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
            if (cajasNivel1 != null)
            {
                foreach (var box in cajasNivel1)
                {
                    box.dispose();
                }
                cajasNivel1 = null;
            }
            if (cajasNivel2 != null)
            {
                foreach (var box in cajasNivel2)
                {
                    box.dispose();
                }
                cajasNivel2 = null;
            }
            if (cajasNivel3 != null)
            {
                foreach (var box in cajasNivel3)
                {
                    box.dispose();
                }
                cajasNivel3 = null;
            }
        }

        private enum RenderMethod
        {
            Unsorted,
            Texture_Sort
        }
    }
}