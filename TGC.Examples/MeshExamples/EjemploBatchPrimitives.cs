using Microsoft.DirectX;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     EjemploBatchPrimitives
    /// </summary>
    public class EjemploBatchPrimitives : TGCExampleViewer
    {
        private TgcTexture box1Texture;
        private TgcTexture box2Texture;
        private TgcTexture box3Texture;
        private TgcBox[] cajasNivel1;
        private TgcBox[] cajasNivel2;
        private TgcBox[] cajasNivel3;

        private RenderMethod currentRenderMethod;

        public EjemploBatchPrimitives(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Mesh Examples";
            Name = "Texture Mesh render order";
            Description =
                "En este ejemplo podemos ver como afecta el orden de renderisado cuando tenemos un mismo mesh" +
                " con diferentes texturas. si realizamos multiples render set se nota el costo que tiene. en cambio si " +
                "asignamos la textura y luego renderisamos todos los mesh de esa textura tiene menos costo.";
            //TO FIX IT, este ejemplo no funciona correctamente porque TGCMesh siempre setea las texturas,
            //entonces el costo es igual si se utiliza el framework.
        }

        public override void Init()
        {
            box1Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg");
            box2Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            box3Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg");

            Modifiers.addEnum("Render Method", typeof(RenderMethod), RenderMethod.Unsorted);
            createMeshes(25);

            Camara.SetCamera(new Vector3(40f, 20f, -70f), new Vector3(40f, 20f, -60f));
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
                    cajasNivel1[cajas].AutoTransformEnable = true;

                    cajasNivel2[cajas] = TgcBox.fromSize(new Vector3(i * boxSize, boxSize, j * boxSize * 1.5f),
                        new Vector3(boxSize, boxSize, boxSize), box2Texture);
                    cajasNivel2[cajas].AutoTransformEnable = true;

                    cajasNivel3[cajas] = TgcBox.fromSize(new Vector3(i * boxSize, boxSize * 2, j * boxSize * 1.5f),
                        new Vector3(boxSize, boxSize, boxSize), box3Texture);
                    cajasNivel3[cajas].AutoTransformEnable = true;
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
        ///     Renderizar haciendo que se tenga que alternar la textura a proposito
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