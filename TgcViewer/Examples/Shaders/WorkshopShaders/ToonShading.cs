using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Shaders;
using TGC.Core.Utils;

namespace Examples.Shaders.WorkshopShaders
{

    /// <summary>
    /// Ejemplo ShadowMap:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    /// 
    /// Muestra como utilizar el efecto de ToonShading, que le da a un mesh un aspecto de caricatura.
    /// 
    /// Autor: Mariano Banquiero
    /// 
    /// </summary>
    public class ToonShading: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        TgcMesh mesh;
        List<TgcMesh> instances;
        Effect effect;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget;
        Texture g_pNormals;
        VertexBuffer g_pVBV3D;
        bool efecto_blur;


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
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Cargar mesh
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                        + "ModelosTgc\\Teapot\\Teapot-TgcScene.xml");

            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(1f, 1f, 1f);
            mesh.Position = new Vector3(-100f, -5f, 0f);

            // Arreglo las normales
            int [] adj = new int[mesh.D3dMesh.NumberFaces*3];
            mesh.D3dMesh.GenerateAdjacency(0,adj);
            mesh.D3dMesh.ComputeNormals(adj); 

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\ToonShading.fx");

            // le asigno el efecto a la malla 
            mesh.Effect = effect;
            mesh.Technique = "DefaultTechnique";

            // Creo las instancias de malla
            instances = new List<TgcMesh>();
            for (int i = -5; i < 5; i++)
                for (int j = -5; j < 5; j++)
                {
                    TgcMesh instance = mesh.createMeshInstance(mesh.Name + i);
                    instance.move(i * 50, (i + j) * 5, j * 50);
                    instances.Add(instance);
                }


            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 40, 0));
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

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // inicializo el mapa de normales
            g_pNormals = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);

            effect.SetValue("g_Normals", g_pNormals);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            //Se crean 2 triangulos con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x=1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            efecto_blur = false;
            
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                efecto_blur = !efecto_blur;


            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);

            device.EndScene();

            // dibujo la escena una textura 
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            foreach (TgcMesh instance in instances)
            {
                instance.Technique = "DefaultTechnique";
                instance.render();
            }
            device.EndScene();
            //TextureLoader.Save("scene.bmp", ImageFileFormat.Bmp, g_pRenderTarget);


            // genero el normal map: 
            pSurf = g_pNormals.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            foreach (TgcMesh instance in instances)
            {
                instance.Technique = "NormalMap";
                instance.render();
            }

            device.EndScene();
            // restuaro el render target y el stencil
            device.DepthStencilSurface = pOldDS;
            device.SetRenderTarget(0, pOldRT);
            //TextureLoader.Save("normal.bmp", ImageFileFormat.Bmp, g_pNormals);

            // dibujo el quad pp dicho :
            device.BeginScene();
            effect.Technique = efecto_blur? "CopyScreen" : "EdgeDetect";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_Normals", g_pNormals);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
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
