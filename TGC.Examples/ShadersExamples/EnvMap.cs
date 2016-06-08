using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
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
    public class EnvMap : TgcExample
    {
        private List<TgcMesh> bosque;

        private string currentHeightmap;
        private float currentScaleXZ;
        private float currentScaleY;
        private string currentTexture;
        private Vector3 dir_avion;
        private Effect effect;

        //private bool fresnel = true; // combinar kx y kc s/ factor de fresnel
        private float kc; // coef. de refraccion

        private float kx; // coef. de reflexion
        private float largo_tanque, alto_tanque;
        private TgcMesh mesh, meshX;
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcMesh palmera, avion;
        private TgcScene scene, scene2, scene3, sceneX;
        private TgcSkyBox skyBox;

        // enviroment map
        private TgcSimpleTerrain
            terrain;

        private float time;
        private float vel_tanque; // grados x segundo
        private bool volar;

        public EnvMap(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Workshop-EnviromentMap";
            Description =
                "Ejemplo de Reflexion y Refraccion con CubeMap. [BARRA]->cambia velocidad del tanque. [X]->volar [S]->Tanque o Esfera";
        }

        public override void Init()
        {
            MyMediaDir = MediaDir;
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Crear loader
            var loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            var PosTerrain = new Vector3(0, 0, 0);
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
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox1\\";
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
            scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];

            sceneX = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml");
            meshX = sceneX.Meshes[0];

            scene2 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmera = scene2.Meshes[0];

            scene3 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene3.Meshes[0];

            mesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new Vector3(0f, 0f, 0f);
            mesh.AutoTransformEnable = false;
            var size = mesh.BoundingBox.calculateSize();
            largo_tanque = System.Math.Abs(size.Z);
            alto_tanque = System.Math.Abs(size.Y) * mesh.Scale.Y;
            avion.Scale = new Vector3(1f, 1f, 1f);
            avion.Position = new Vector3(3000f, 550f, 0f);
            avion.AutoTransformEnable = false;
            dir_avion = new Vector3(0, 0, 1);
            size = palmera.BoundingBox.calculateSize();
            var alto_palmera = System.Math.Abs(size.Y);
            int i;
            bosque = new List<TgcMesh>();
            float[] r = { 1900f, 2100f, 2300f, 1800f };
            for (i = 0; i < 4; i++)
                for (var j = 0; j < 15; j++)
                {
                    var instance = palmera.createMeshInstance(palmera.Name + i);
                    instance.Scale = new Vector3(0.5f, 1.5f, 0.5f);
                    var x = r[i] * (float)System.Math.Cos(Geometry.DegreeToRadian(180 + 10.0f * j));
                    var z = r[i] * (float)System.Math.Sin(Geometry.DegreeToRadian(180 + 10.0f * j));
                    instance.Position = new Vector3(x, CalcularAltura(x, z) /*+ alto_palmera / 2 * instance.Scale.Y*/, z);
                    bosque.Add(instance);
                }

            ((TgcRotationalCamera)Camara).CameraDistance = 300;
            ((TgcRotationalCamera)Camara).RotationSpeed = 1.5f;

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
            effect =
                TgcShaders.loadEffect(ShadersDir + "WorkshopShaders\\EnvMap.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;
            meshX.Effect = effect;

            vel_tanque = 10;

            //Centrar camara rotacional respecto a este mesh
            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);

            kx = kc = 0.5f;
            Modifiers.addFloat("Reflexion", 0, 1, kx);
            Modifiers.addFloat("Refraccion", 0, 1, kc);
            Modifiers.addBoolean("Fresnel", "fresnel", true);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            var aspectRatio = D3DDevice.Instance.AspectRatio;
            if (TgcD3dInput.Instance.keyPressed(Key.Space))
            {
                vel_tanque++;
                if (vel_tanque > 10)
                    vel_tanque = 0;
            }
            if (TgcD3dInput.Instance.keyPressed(Key.X))
                volar = !volar;

            if (TgcD3dInput.Instance.keyPressed(Key.S))
            {
                // swap mesh
                var mesh_aux = mesh;
                mesh = meshX;
                meshX = mesh;
            }

            //Cargar variables de shader
            effect.SetValue("fvLightPosition", new Vector4(0, 400, 0, 0));
            effect.SetValue("fvEyePosition",
                TgcParserUtils.vector3ToFloat3Array(Camara.Position));
            effect.SetValue("kx", (float)Modifiers["Reflexion"]);
            effect.SetValue("kc", (float)Modifiers["Refraccion"]);
            effect.SetValue("usar_fresnel", (bool)Modifiers["Fresnel"]);

            time += ElapsedTime;
            // animar tanque
            var alfa = -time * Geometry.DegreeToRadian(vel_tanque);
            var x0 = 2000f * (float)System.Math.Cos(alfa);
            var z0 = 2000f * (float)System.Math.Sin(alfa);
            float offset_rueda = 13;
            var H = CalcularAltura(x0, z0) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H += 300;
            mesh.Position = new Vector3(x0, H, z0);
            // direccion tangente sobre el piso:
            var dir_tanque = new Vector2(-(float)System.Math.Sin(alfa), (float)System.Math.Cos(alfa));
            dir_tanque.Normalize();
            // Posicion de la parte de adelante del tanque
            var pos2d = new Vector2(x0, z0);
            pos2d = pos2d + dir_tanque * (largo_tanque / 2);
            var H_frente = CalcularAltura(pos2d.X, pos2d.Y) + alto_tanque / 2 - offset_rueda;
            if (volar)
                H_frente += 300;
            var pos_frente = new Vector3(pos2d.X, H_frente, pos2d.Y);
            var Vel = pos_frente - mesh.Position;
            Vel.Normalize();

            mesh.Transform = CalcularMatriz(mesh.Position, mesh.Scale, Vel);

            var beta = -time * Geometry.DegreeToRadian(120.0f);
            avion.Position = new Vector3(x0 + 300f * (float)System.Math.Cos(beta),
                400 + H, z0 + 300f * (float)System.Math.Sin(alfa));
            dir_avion = new Vector3(-(float)System.Math.Sin(beta), 0, (float)System.Math.Cos(beta));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);

            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);
            Camara.updateCamera(ElapsedTime);
            // --------------------------------------------------------------------
            D3DDevice.Instance.Device.EndScene();
            var g_pCubeMap = new CubeTexture(D3DDevice.Instance.Device, 256, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            D3DDevice.Instance.Device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f),
                    1f, 1f, 10000f);

            // Genero las caras del enviroment map
            for (var nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                var pFace = g_pCubeMap.GetCubeMapSurface(nFace, 0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pFace);
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
                var Pos = mesh.Position;
                D3DDevice.Instance.Device.Transform.View = Matrix.LookAtLH(Pos, Pos + Dir, VUP);

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
            D3DDevice.Instance.Device.Transform.View = Camara.getViewMatrix();
            D3DDevice.Instance.Device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                    aspectRatio, 1f, 10000f);

            // dibujo pp dicho
            D3DDevice.Instance.Device.BeginScene();
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("g_txCubeMap", g_pCubeMap);
            renderScene(ElapsedTime, false);
            g_pCubeMap.Dispose();

            FinalizarEscena();
        }

        public void renderScene(float elapsedTime, bool cubemap)
        {
            //Renderizar terreno
            terrain.render();
            //Renderizar SkyBox
            skyBox.render();
            // dibujo el bosque
            foreach (var instance in bosque)
            {
                instance.render();
            }

            // avion
            //avion.Technique = cubemap ? "RenderCubeMap" : "RenderScene";
            avion.render();

            if (!cubemap)
            {
                // dibujo el mesh
                mesh.Technique = "RenderCubeMap";
                mesh.render();
            }
        }

        // helper
        public Matrix CalcularMatriz(Vector3 Pos, Vector3 Scale, Vector3 Dir)
        {
            var VUP = new Vector3(0, 1, 0);

            var matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            var U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            var V = Vector3.Cross(Dir, U);
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
            var matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            var U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            var V = Vector3.Cross(Dir, U);
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
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
                    (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H;
        }

        public override void Close()
        {
            base.Close();

            effect.Dispose();
            scene.disposeAll();
            scene2.disposeAll();
            scene3.disposeAll();
            sceneX.disposeAll();
        }
    }
}