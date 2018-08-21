using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     EjemploBatchPrimitives
    /// </summary>
    public class EjemploBatchPrimitives : TGCExampleViewer
    {
        private TGCEnumModifier renderMethodModifier;

        private TgcTexture box1Texture;
        private TgcTexture box2Texture;
        private TgcTexture box3Texture;
        private TGCBox[] cajasNivel1;
        private TGCBox[] cajasNivel2;
        private TGCBox[] cajasNivel3;

        private RenderMethod currentRenderMethod;

        public EjemploBatchPrimitives(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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

            renderMethodModifier = AddEnum("Render Method", typeof(RenderMethod), RenderMethod.Unsorted);
            createMeshes(25);

            Camara.SetCamera(new TGCVector3(40f, 20f, -70f), new TGCVector3(40f, 20f, -60f));
        }

        private void createMeshes(int cajasPorCuadrante)
        {
            var cantCajasPorNivel = cajasPorCuadrante * cajasPorCuadrante;

            //Crear nuevas cajas
            var boxSize = 3f;
            cajasNivel1 = new TGCBox[cantCajasPorNivel];
            cajasNivel2 = new TGCBox[cantCajasPorNivel];
            cajasNivel3 = new TGCBox[cantCajasPorNivel];
            var cajas = 0;
            for (var i = 0; i < cajasPorCuadrante; i++)
            {
                for (var j = 0; j < cajasPorCuadrante; j++)
                {
                    //Crear tres niveles de caja, una abajo y otra arriba, con texturas diferentes
                    cajasNivel1[cajas] = TGCBox.fromSize(new TGCVector3(i * boxSize, 0, j * boxSize * 1.5f),
                        new TGCVector3(boxSize, boxSize, boxSize), box1Texture);
                    cajasNivel1[cajas].AutoTransform = true;

                    cajasNivel2[cajas] = TGCBox.fromSize(new TGCVector3(i * boxSize, boxSize, j * boxSize * 1.5f),
                        new TGCVector3(boxSize, boxSize, boxSize), box2Texture);
                    cajasNivel2[cajas].AutoTransform = true;

                    cajasNivel3[cajas] = TGCBox.fromSize(new TGCVector3(i * boxSize, boxSize * 2, j * boxSize * 1.5f),
                        new TGCVector3(boxSize, boxSize, boxSize), box3Texture);
                    cajasNivel3[cajas].AutoTransform = true;
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
                cajasNivel1[i].Render();
                cajasNivel2[i].Render();
                cajasNivel3[i].Render();
            }
        }

        /// <summary>
        ///     Renderizar primero las de la textura 1 y despues la de textura 2, para minimizar los Texture State Change
        /// </summary>
        private void doTextureSortRender()
        {
            for (var i = 0; i < cajasNivel1.Length; i++)
            {
                cajasNivel1[i].Render();
            }
            for (var i = 0; i < cajasNivel2.Length; i++)
            {
                cajasNivel2[i].Render();
            }
            for (var i = 0; i < cajasNivel3.Length; i++)
            {
                cajasNivel3[i].Render();
            }
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            var renderMethod = (RenderMethod)renderMethodModifier.Value;
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
                    box.Dispose();
                }
                cajasNivel1 = null;
            }
            if (cajasNivel2 != null)
            {
                foreach (var box in cajasNivel2)
                {
                    box.Dispose();
                }
                cajasNivel2 = null;
            }
            if (cajasNivel3 != null)
            {
                foreach (var box in cajasNivel3)
                {
                    box.Dispose();
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