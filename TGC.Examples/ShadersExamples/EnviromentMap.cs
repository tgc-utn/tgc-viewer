using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EnvMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    ///     Muestra como reflejar un Enviroment Map en un mesh.
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class EnviromentMap : TGCExampleViewer
    {
        private TGCFloatModifier reflexionModifier;
        private TGCFloatModifier refraccionModifier;
        private TGCBooleanModifier fresnelModifier;

        private List<TgcMesh> bosque;
        private TgcRotationalCamera CamaraRot;

        private string currentHeightmap;
        private float currentScaleXZ;
        private float currentScaleY;
        private string currentTexture;
        private TGCVector3 dir_avion;
        private Effect effect;

        //private bool fresnel = true; // combinar kx y kc s/ factor de fresnel
        private float kc; // coef. de refraccion

        private float kx; // coef. de reflexion
        private float largo_tanque, alto_tanque;
        private TgcMesh mesh, meshX;
        private string MyMediaDir;
        private TgcMesh palmera, avion;
        private TgcScene scene, scene2, scene3, sceneX;
        private TgcSkyBox skyBox;

        // enviroment map
        private TgcSimpleTerrain terrain;

        private float time;
        private float vel_tanque; // grados x segundo
        private bool volar;

        public EnviromentMap(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel Shaders";
            Name = "Enviroment Map";
            Description =
                "Ejemplo de Reflexion y Refraccion con CubeMap. [BARRA]->cambia velocidad del tanque. [X]->volar [S]->Tanque o Esfera";
        }

        public override void Init()
        {
            MyMediaDir = MediaDir;

            //Crear loader
            var loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            var PosTerrain = TGCVector3.Empty;
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
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(8000, 8000, 8000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox1\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.SkyEpsilon = 50f;
            skyBox.Init();

            // ------------------------------------------------------------
            //Cargar los mesh:
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];

            sceneX = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml");
            meshX = sceneX.Meshes[0];
            meshX.AutoTransform = true;

            scene2 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmera = scene2.Meshes[0];
            palmera.AutoTransform = true;

            scene3 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene3.Meshes[0];

            mesh.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new TGCVector3(0f, 0f, 0f);
            var size = mesh.BoundingBox.calculateSize();
            largo_tanque = Math.Abs(size.Z);
            alto_tanque = Math.Abs(size.Y) * mesh.Scale.Y;
            avion.Scale = new TGCVector3(1f, 1f, 1f);
            avion.Position = new TGCVector3(3000f, 550f, 0f);
            dir_avion = new TGCVector3(0, 0, 1);
            size = palmera.BoundingBox.calculateSize();
            var alto_palmera = Math.Abs(size.Y);
            int i;
            bosque = new List<TgcMesh>();
            float[] r = { 1900f, 2100f, 2300f, 1800f };
            for (i = 0; i < 4; i++)
                for (var j = 0; j < 15; j++)
                {
                    var instance = palmera.createMeshInstance(palmera.Name + i);
                    instance.AutoTransform = true;
                    instance.Scale = new TGCVector3(0.5f, 1.5f, 0.5f);
                    var x = r[i] * (float)Math.Cos(Geometry.DegreeToRadian(180 + 10.0f * j));
                    var z = r[i] * (float)Math.Sin(Geometry.DegreeToRadian(180 + 10.0f * j));
                    instance.Position = new TGCVector3(x, CalcularAltura(x, z) /*+ alto_palmera / 2 * instance.Scale.Y*/, z);
                    bosque.Add(instance);
                }

            // Arreglo las normales del tanque
            /*int[] adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);
             */

            // Arreglo las normales de la esfera
            {
                var adj = new int[meshX.D3dMesh.NumberFaces * 3];
                meshX.D3dMesh.GenerateAdjacency(0, adj);
                meshX.D3dMesh.ComputeNormals(adj);
            }

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "WorkshopShaders\\EnvMap.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;
            meshX.Effect = effect;

            vel_tanque = 10;

            //Centrar camara rotacional respecto a este mesh
            CamaraRot = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2, Input);
            CamaraRot.CameraDistance = 300;
            CamaraRot.RotationSpeed = 1.5f;
            Camara = CamaraRot;

            kx = kc = 0.5f;
            reflexionModifier = AddFloat("Reflexion", 0, 1, kx);
            refraccionModifier = AddFloat("Refraccion", 0, 1, kc);
            fresnelModifier = AddBoolean("Fresnel", "fresnel", true);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            var aspectRatio = D3DDevice.Instance.AspectRatio;
            if (Input.keyPressed(Key.Space))
            {
                vel_tanque++;
                if (vel_tanque > 10)
                    vel_tanque = 0;
            }
            if (Input.keyPressed(Key.X))
                volar = !volar;

            if (Input.keyPressed(Key.S))
            {
                // swap mesh
                var mesh_aux = mesh;
                mesh = meshX;
                meshX = mesh;
            }

            //Cargar variables de shader
            effect.SetValue("fvLightPosition", new Vector4(0, 400, 0, 0));
            effect.SetValue("fvEyePosition", TGCVector3.Vector3ToFloat3Array(Camara.Position));
            effect.SetValue("kx", reflexionModifier.Value);
            effect.SetValue("kc", refraccionModifier.Value);
            effect.SetValue("usar_fresnel", fresnelModifier.Value);

            time += ElapsedTime;
            // animar tanque
            var alfa = -time * Geometry.DegreeToRadian(vel_tanque);
            var x0 = 2000f * (float)Math.Cos(alfa);
            var z0 = 2000f * (float)Math.Sin(alfa);
            float offset_rueda = 13;
            var H = CalcularAltura(x0, z0) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H += 300;
            mesh.Position = new TGCVector3(x0, H, z0);
            // direccion tangente sobre el piso:
            var dir_tanque = new TGCVector2(-(float)Math.Sin(alfa), (float)Math.Cos(alfa));
            dir_tanque.Normalize();
            // Posicion de la parte de adelante del tanque
            var pos2d = new TGCVector2(x0, z0);
            pos2d = pos2d + dir_tanque * (largo_tanque / 2);
            var H_frente = CalcularAltura(pos2d.X, pos2d.Y) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H_frente += 300;
            var pos_frente = new TGCVector3(pos2d.X, H_frente, pos2d.Y);
            var Vel = pos_frente - mesh.Position;
            Vel.Normalize();

            mesh.Transform = CalcularMatriz(mesh.Position, mesh.Scale, Vel);

            var beta = -time * Geometry.DegreeToRadian(120.0f);
            avion.Position = new TGCVector3(x0 + 300f * (float)Math.Cos(beta), 400 + H, z0 + 300f * (float)Math.Sin(alfa));
            dir_avion = new TGCVector3(-(float)Math.Sin(beta), 0, (float)Math.Cos(beta));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);

            // --------------------------------------------------------------------
            D3DDevice.Instance.Device.EndScene();
            var g_pCubeMap = new CubeTexture(D3DDevice.Instance.Device, 256, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, 1f, 10000f).ToMatrix();

            // Genero las caras del enviroment map
            for (var nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                var pFace = g_pCubeMap.GetCubeMapSurface(nFace, 0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pFace);
                TGCVector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new TGCVector3(1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Black;
                        break;

                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new TGCVector3(-1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Red;
                        break;

                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = TGCVector3.Up;
                        VUP = new TGCVector3(0, 0, -1);
                        color = Color.Gray;
                        break;

                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = TGCVector3.Down;
                        VUP = new TGCVector3(0, 0, 1);
                        color = Color.Yellow;
                        break;

                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new TGCVector3(0, 0, 1);
                        VUP = TGCVector3.Up;
                        color = Color.Green;
                        break;

                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new TGCVector3(0, 0, -1);
                        VUP = TGCVector3.Up;
                        color = Color.Blue;
                        break;
                }

                //como queremos usar la camara rotacional pero siguendo a un objetivo comentamos el seteo del view.
                //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
                //var Pos = mesh.Position;
                //D3DDevice.Instance.Device.Transform.View = TGCMatrix.LookAtLH(Pos, Pos + Dir, VUP);
                CamaraRot.CameraCenter = mesh.Position;

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                D3DDevice.Instance.Device.BeginScene();

                //Renderizar
                renderScene(ElapsedTime, true);

                D3DDevice.Instance.Device.EndScene();
                //string fname = string.Format("face{0:D}.bmp", nFace);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pFace);
            }
            // restuaro el render target
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            //TextureLoader.Save("test.bmp", ImageFileFormat.Bmp, g_pCubeMap);

            // Restauro el estado de las transformaciones
            D3DDevice.Instance.Device.Transform.View = Camara.GetViewMatrix().ToMatrix();
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), aspectRatio, 1f, 10000f).ToMatrix();

            // dibujo pp dicho
            D3DDevice.Instance.Device.BeginScene();
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("g_txCubeMap", g_pCubeMap);
            renderScene(ElapsedTime, false);
            g_pCubeMap.Dispose();

            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public void renderScene(float elapsedTime, bool cubemap)
        {
            //Renderizar terreno
            terrain.Render();
            //Renderizar SkyBox
            skyBox.Render();
            // dibujo el bosque
            foreach (var instance in bosque)
            {
                instance.Render();
            }

            // avion
            //avion.Technique = cubemap ? "RenderCubeMap" : "RenderScene";
            avion.Render();

            if (!cubemap)
            {
                // dibujo el mesh
                mesh.Technique = "RenderCubeMap";
                mesh.Render();
            }
        }

        // helper
        public TGCMatrix CalcularMatriz(TGCVector3 Pos, TGCVector3 Scale, TGCVector3 Dir)
        {
            var VUP = TGCVector3.Up;

            var matWorld = TGCMatrix.Scaling(Scale);
            // determino la orientacion
            var U = TGCVector3.Cross(VUP, Dir);
            U.Normalize();
            var V = TGCVector3.Cross(Dir, U);
            TGCMatrix Orientacion = new TGCMatrix();
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
            matWorld = matWorld * TGCMatrix.Translation(Pos);
            return matWorld;
        }

        public TGCMatrix CalcularMatrizUp(TGCVector3 Pos, TGCVector3 Scale, TGCVector3 Dir, TGCVector3 VUP)
        {
            var matWorld = TGCMatrix.Scaling(Scale);
            // determino la orientacion
            var U = TGCVector3.Cross(VUP, Dir);
            U.Normalize();
            var V = TGCVector3.Cross(Dir, U);
            TGCMatrix Orientacion = new TGCMatrix();
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
            matWorld = matWorld * TGCMatrix.Translation(Pos);
            return matWorld;
        }

        public float CalcularAltura(float x, float z)
        {
            var largo = currentScaleXZ * 64;
            var pos_i = 64f * (0.5f + x / largo);
            var pos_j = 64f * (0.5f + z / largo);

            var pi = (int)pos_i;
            var fracc_i = pos_i - pi;
            var pj = (int)pos_j;
            var fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else if (pi > 63)
                pi = 63;

            if (pj < 0)
                pj = 0;
            else if (pj > 63)
                pj = 63;

            var pi1 = pi + 1;
            var pj1 = pj + 1;
            if (pi1 > 63)
                pi1 = 63;
            if (pj1 > 63)
                pj1 = 63;

            // 2x2 percent closest filtering usual:
            var H0 = terrain.HeightmapData[pi, pj] * currentScaleY;
            var H1 = terrain.HeightmapData[pi1, pj] * currentScaleY;
            var H2 = terrain.HeightmapData[pi, pj1] * currentScaleY;
            var H3 = terrain.HeightmapData[pi1, pj1] * currentScaleY;
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) + (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H;
        }

        public override void Dispose()
        {
            effect.Dispose();
            scene.DisposeAll();
            scene2.DisposeAll();
            scene3.DisposeAll();
            sceneX.DisposeAll();
        }
    }
}