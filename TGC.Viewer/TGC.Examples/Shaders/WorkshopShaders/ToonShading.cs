using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;
using TGC.Viewer;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.Shaders.WorkshopShaders
{
    /// <summary>
    ///     Ejemplo ShadowMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    ///     Muestra como utilizar el efecto de ToonShading, que le da a un mesh un aspecto de caricatura.
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class ToonShading : TgcExample
    {
        private bool efecto_blur;
        private Effect effect;
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pNormals;
        private Texture g_pRenderTarget;
        private VertexBuffer g_pVBV3D;
        private List<TgcMesh> instances;
        private TgcMesh mesh;
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-ToonShading";
        }

        public override string getDescription()
        {
            return "Ejemplo de Render no-realistico. [BARRA]->Activa/Desactiva efecto Blur";
        }

        public override void init()
        {
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar mesh
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                                             + "ModelosTgc\\Teapot\\Teapot-TgcScene.xml");

            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(1f, 1f, 1f);
            mesh.Position = new Vector3(-100f, -5f, 0f);

            // Arreglo las normales
            var adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);

            //Cargar Shader personalizado
            effect =
                TgcShaders.loadEffect(GuiController.Instance.ExamplesDir +
                                      "Shaders\\WorkshopShaders\\Shaders\\ToonShading.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;
            mesh.Technique = "DefaultTechnique";

            // Creo las instancias de malla
            instances = new List<TgcMesh>();
            for (var i = -5; i < 5; i++)
                for (var j = -5; j < 5; j++)
                {
                    var instance = mesh.createMeshInstance(mesh.Name + i);
                    instance.move(i * 50, (i + j) * 5, j * 50);
                    instances.Add(instance);
                }

            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100, -100, -100),
                new Vector3(100, 100, 100), new Vector3(0, 40, 0));
            GuiController.Instance.Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            GuiController.Instance.Modifiers.addFloat("Specular", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);

            GuiController.Instance.RotCamera.setCamera(new Vector3(20, 20, 0), 200);
            GuiController.Instance.RotCamera.CameraDistance = 300;
            GuiController.Instance.RotCamera.RotationSpeed = 1.5f;

            // Creo un depthbuffer sin multisampling, para que sea compatible con el render to texture

            // Nota:
            // El render to Texture no es compatible con el multisampling en dx9
            // Por otra parte la mayor parte de las placas de ultima generacion no soportan
            // mutisampling para texturas de punto flotante con lo cual
            // hay que suponer con generalidad que no se puede usar multisampling y render to texture

            // Para resolverlo hay que crear un depth buffer que no tenga multisampling,
            // (de lo contrario falla el zbuffer y se producen artifacts tipicos de que no tiene zbuffer)

            // Si uno quisiera usar el multisampling, la tecnica habitual es usar un RenderTarget
            // en lugar de una textura.
            // Por ejemplo en c++:
            //
            // Render Target formato color buffer con multisampling
            //
            //  g_pd3dDevice->CreateRenderTarget(Ancho,Alto,
            //          D3DFMT_A8R8G8B8, D3DMULTISAMPLE_2_SAMPLES, 0,
            //          FALSE, &g_pRenderTarget, NULL);
            //
            // Luego, ese RenderTarget NO ES una textura, y nosotros necesitamos acceder a esos
            // pixeles, ahi lo que se hace es COPIAR del rendertartet a una textura,
            // para poder trabajar con esos datos en el contexto del Pixel shader:
            //
            // Eso se hace con la funcion StretchRect:
            // copia de rendertarget ---> sceneSurface (que es la superficie asociada a una textura)
            // g_pd3dDevice->StretchRect(g_pRenderTarget, NULL, g_pSceneSurface, NULL, D3DTEXF_NONE);
            //
            // Esta tecnica se llama downsampling
            // Y tiene el costo adicional de la transferencia de memoria entre el rendertarget y la
            // textura, pero que no traspasa los limites de la GPU. (es decir es muy performante)
            // no es lo mismo que lockear una textura para acceder desde la CPU, que tiene el problema
            // de transferencia via AGP.

            g_pDepthStencil =
                D3DDevice.Instance.Device.CreateDepthStencilSurface(
                    D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth,
                    D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight,
                    DepthFormat.D24S8,
                    MultiSampleType.None,
                    0,
                    true);

            // inicializo el render target
            g_pRenderTarget = new Texture(D3DDevice.Instance.Device,
                D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth
                , D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // inicializo el mapa de normales
            g_pNormals = new Texture(D3DDevice.Instance.Device,
                D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth
                , D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);

            effect.SetValue("g_Normals", g_pNormals);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight);

            //Se crean 2 triangulos con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x=1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            efecto_blur = false;
        }

        public override void render(float elapsedTime)
        {
            var panel3d = GuiController.Instance.Panel3d;
            var aspectRatio = panel3d.Width / (float)panel3d.Height;

            var lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            if (GuiController.Instance.D3dInput.keyPressed(Key.Space))
                efecto_blur = !efecto_blur;

            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition",
                TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);

            D3DDevice.Instance.Device.EndScene();

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            D3DDevice.Instance.Device.DepthStencilSurface = g_pDepthStencil;

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
            foreach (var instance in instances)
            {
                instance.Technique = "DefaultTechnique";
                instance.render();
            }
            D3DDevice.Instance.Device.EndScene();
            //TextureLoader.Save("scene.bmp", ImageFileFormat.Bmp, g_pRenderTarget);

            // genero el normal map:
            pSurf = g_pNormals.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
            foreach (var instance in instances)
            {
                instance.Technique = "NormalMap";
                instance.render();
            }

            D3DDevice.Instance.Device.EndScene();
            // restuaro el render target y el stencil
            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            //TextureLoader.Save("normal.bmp", ImageFileFormat.Bmp, g_pNormals);

            // dibujo el quad pp dicho :
            D3DDevice.Instance.Device.BeginScene();
            effect.Technique = efecto_blur ? "CopyScreen" : "EdgeDetect";
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_Normals", g_pNormals);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
        }

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            g_pRenderTarget.Dispose();
            g_pNormals.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }
}