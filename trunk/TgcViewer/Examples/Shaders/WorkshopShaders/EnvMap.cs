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

namespace Examples.Shaders.WorkshopShaders
{

    /// <summary>
    /// Ejemplo EnvMap:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/WorkshopShaders/BasicShader".
    /// Muestra como reflejar un Enviroment Map en un mesh.
    /// 
    /// Autor: Mariano Banquiero
    /// 
    /// </summary>
    public class EnvMap: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene, scene2, scene3, sceneX;
        MyMesh mesh,meshX;
        TgcMesh palmera, avion;
        Effect effect;
        List<TgcMesh> bosque;

        // enviroment map
        TgcSimpleTerrain terrain;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ;
        float currentScaleY;
        TgcSkyBox skyBox;
        float kx;       // coef. de reflexion
        float kc;       // coef. de refraccion
        bool fresnel = true;        // combinar kx y kc s/ factor de fresnel

        float time;
        float largo_tanque, alto_tanque;
        float vel_tanque;       // grados x segundo
        Vector3 dir_avion;
        bool volar = false;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-EnviromentMap";
        }

        public override string getDescription()
        {
            return "Ejemplo de Reflexion y Refraccion con CubeMap. " +
                "[BARRA]->cambia velocidad del tanque. [X]->volar" +
                    "[S]->Tanque o Esfera";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            Vector3 PosTerrain = new Vector3(0, 0, 0);
            currentHeightmap = MyMediaDir + "Heighmaps\\Heightmap2.jpg";
            currentScaleXZ = 100f;
            currentScaleY = 2f;
            currentTexture = MyMediaDir + "Heighmaps\\TerrainTexture3.jpg";
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, PosTerrain);
            terrain.loadTexture(currentTexture);

            // ------------------------------------------------------------
            // Crear SkyBox:
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(8000, 8000, 8000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox1\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.SkyEpsilon = 50f;
            skyBox.updateValues();

            // ------------------------------------------------------------
            //Cargar los mesh:
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                            + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = (MyMesh)scene.Meshes[0];

            sceneX = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                            + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml");
            meshX = (MyMesh)sceneX.Meshes[0];



            scene2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                            + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmera = (MyMesh)scene2.Meshes[0];
            
            scene3 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                    + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene3.Meshes[0];

            mesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new Vector3(0f, 0f, 0f);
            mesh.AutoTransformEnable = false;
            Vector3 size = mesh.BoundingBox.calculateSize();
            largo_tanque = Math.Abs(size.Z);
            alto_tanque = Math.Abs(size.Y) * mesh.Scale.Y;
            avion.Scale = new Vector3(1f, 1f, 1f);
            avion.Position = new Vector3(3000f, 550f, 0f);
            avion.AutoTransformEnable = false;
            dir_avion = new Vector3(0, 0, 1);
            size = palmera.BoundingBox.calculateSize();
            float alto_palmera = Math.Abs(size.Y);
            int i;
            bosque = new List<TgcMesh>();
            float[] r = { 1900f, 2100f, 2300f, 1800f };
            for (i = 0; i < 4; i++)
                for (int j = 0; j < 15; j++)
                {
                    TgcMesh instance = palmera.createMeshInstance(palmera.Name + i);
                    instance.Scale = new Vector3(0.5f, 1.5f, 0.5f);
                    float x = r[i] * (float)Math.Cos(Geometry.DegreeToRadian(180 + 10.0f * j));
                    float z = r[i] * (float)Math.Sin(Geometry.DegreeToRadian(180 + 10.0f * j));
                    instance.Position = new Vector3(x, CalcularAltura(x, z) /*+ alto_palmera / 2 * instance.Scale.Y*/, z);
                    bosque.Add(instance);
                }

            GuiController.Instance.RotCamera.CameraDistance = 300;
            GuiController.Instance.RotCamera.RotationSpeed = 1.5f;

            // Arreglo las normales del tanque 
            /*int[] adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);
             */

            // Arreglo las normales de la esfera
            {
                int[] adj = new int[meshX.D3dMesh.NumberFaces * 3];
                meshX.D3dMesh.GenerateAdjacency(0, adj);
                meshX.D3dMesh.ComputeNormals(adj);
            }
             
            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "EnvMap.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            // le asigno el efecto a la malla 
            mesh.effect = effect;
            meshX.effect = effect;

            vel_tanque = 10;

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

            kx = kc = 0.5f;
            GuiController.Instance.Modifiers.addFloat("Reflexion", 0, 1, kx);
            GuiController.Instance.Modifiers.addFloat("Refraccion", 0, 1, kc);
            GuiController.Instance.Modifiers.addBoolean("Fresnel", "fresnel", true);
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
            {
                vel_tanque++;
                if (vel_tanque > 10)
                    vel_tanque = 0;
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.X))
                volar = !volar;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.S))
            {
                // swap mesh
                MyMesh mesh_aux = mesh;
                mesh = meshX;
                meshX = mesh;
            }

            effect.Technique = "RenderScene";
            //Cargar variables de shader
            effect.SetValue("fvLightPosition", new Vector4(0,400,0,0));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("kx", (float)GuiController.Instance.Modifiers["Reflexion"]);
            effect.SetValue("kc", (float)GuiController.Instance.Modifiers["Refraccion"]);
            effect.SetValue("usar_fresnel", (bool)GuiController.Instance.Modifiers["Fresnel"]);

            time += elapsedTime;
            // animar tanque
            float alfa = -time * Geometry.DegreeToRadian(vel_tanque);
            float x0 = 2000f * (float)Math.Cos(alfa);
            float z0 = 2000f * (float)Math.Sin(alfa);
            float offset_rueda = 13;
            float H = CalcularAltura(x0, z0) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H += 300;
            mesh.Position = new Vector3(x0, H, z0);
            // direccion tangente sobre el piso: 
            Vector2 dir_tanque = new Vector2(-(float)Math.Sin(alfa), (float)Math.Cos(alfa));
            dir_tanque.Normalize();
            // Posicion de la parte de adelante del tanque
            Vector2 pos2d = new Vector2(x0, z0);
            pos2d = pos2d + dir_tanque * (largo_tanque / 2);
            float H_frente = CalcularAltura(pos2d.X, pos2d.Y) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H_frente += 300;
            Vector3 pos_frente = new Vector3(pos2d.X, H_frente, pos2d.Y);
            Vector3 Vel = pos_frente - mesh.Position;
            Vel.Normalize();


            mesh.Transform = CalcularMatriz(mesh.Position, mesh.Scale, Vel);

            float beta = -time * Geometry.DegreeToRadian(120.0f);
            avion.Position = new Vector3(x0 + 300f * (float)Math.Cos(beta),
                    400 + H, z0 + 300f * (float)Math.Sin(alfa));
            dir_avion = new Vector3(-(float)Math.Sin(beta), 0, (float)Math.Cos(beta));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);

            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
            GuiController.Instance.CurrentCamera.updateCamera();
            // --------------------------------------------------------------------
            device.EndScene();
            CubeTexture g_pCubeMap = new CubeTexture(device, 256, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            Surface pOldRT = device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f),
                    1f, 1f, 10000f);

            // Genero las caras del enviroment map
            for (CubeMapFace nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                Surface pFace = g_pCubeMap.GetCubeMapSurface(nFace, 0);
                device.SetRenderTarget(0, pFace);
                Vector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new Vector3(1, 0, 0);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Black;
                        break;
                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new Vector3(-1, 0, 0);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Red;
                        break;
                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = new Vector3(0, 1, 0);
                        VUP = new Vector3(0, 0, -1);
                        color = Color.Gray;
                        break;
                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = new Vector3(0, -1, 0);
                        VUP = new Vector3(0, 0, 1);
                        color = Color.Yellow;
                        break;
                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new Vector3(0, 0, 1);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Green;
                        break;
                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new Vector3(0, 0, -1);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Blue;
                        break;
                }

                //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
                Vector3 Pos = mesh.Position;
                device.Transform.View = Matrix.LookAtLH(Pos, Pos + Dir, VUP);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                device.BeginScene();

                //Renderizar 
                renderScene(elapsedTime, true);

                device.EndScene();
                //string fname = string.Format("face{0:D}.bmp", nFace);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pFace);


            }
            // restuaro el render target
            device.SetRenderTarget(0, pOldRT);
            //TextureLoader.Save("test.bmp", ImageFileFormat.Bmp, g_pCubeMap);

            // Restauro el estado de las transformaciones
            GuiController.Instance.CurrentCamera.updateViewMatrix(device);
            device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                    aspectRatio, 1f, 10000f);

            // dibujo pp dicho
            device.BeginScene();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("g_txCubeMap", g_pCubeMap);
            renderScene(elapsedTime, false);
            g_pCubeMap.Dispose();




        }


        public void renderScene(float elapsedTime, bool cubemap)
        {
            Device device = GuiController.Instance.D3dDevice;
            //Renderizar terreno
            terrain.render();
            //Renderizar SkyBox
            skyBox.render();
            // dibujo el bosque
            foreach (TgcMesh instance in bosque)
                instance.render();
            // avion
            avion.render();

            if (!cubemap)
            {
                // dibujo el mesh
                effect.Technique = "RenderCubeMap";
                mesh.render();
            }
        }

        // helper
        public Matrix CalcularMatriz(Vector3 Pos, Vector3 Scale, Vector3 Dir)
        {
            Vector3 VUP = new Vector3(0, 1, 0);

            Matrix matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            Vector3 U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            Vector3 V = Vector3.Cross(Dir, U);
            Matrix Orientacion;
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.Translation(Pos);
            return matWorld;
        }


        public Matrix CalcularMatrizUp(Vector3 Pos, Vector3 Scale, Vector3 Dir, Vector3 VUP)
        {
            Matrix matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            Vector3 U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            Vector3 V = Vector3.Cross(Dir, U);
            Matrix Orientacion;
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.Translation(Pos);
            return matWorld;
        }

        public float CalcularAltura(float x, float z)
        {
            float largo = currentScaleXZ * 64;
            float pos_i = 64f * (0.5f + x / largo);
            float pos_j = 64f * (0.5f + z / largo);

            int pi = (int)pos_i;
            float fracc_i = pos_i - pi;
            int pj = (int)pos_j;
            float fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else
                if (pi > 63)
                    pi = 63;

            if (pj < 0)
                pj = 0;
            else
                if (pj > 63)
                    pj = 63;

            int pi1 = pi + 1;
            int pj1 = pj + 1;
            if (pi1 > 63)
                pi1 = 63;
            if (pj1 > 63)
                pj1 = 63;

            // 2x2 percent closest filtering usual: 
            float H0 = terrain.HeightmapData[pi, pj] * currentScaleY;
            float H1 = terrain.HeightmapData[pi1, pj] * currentScaleY;
            float H2 = terrain.HeightmapData[pi, pj1] * currentScaleY;
            float H3 = terrain.HeightmapData[pi1, pj1] * currentScaleY;
            float H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
                      (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H;
        }


        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            scene2.disposeAll();
            scene3.disposeAll();
            sceneX.disposeAll();
        }            
    }

}
