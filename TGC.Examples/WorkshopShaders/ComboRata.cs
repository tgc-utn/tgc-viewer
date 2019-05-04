using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.WorkshopShaders
{
    public class ComboRata : TGCExampleViewer
    {
        private TGCVertex3fModifier lightDirModifier;
        private TGCFloatModifier minSampleModifier;
        private TGCFloatModifier maxSampleModifier;
        private TGCFloatModifier heightMapScaleModifier;

        private string MyShaderDir;
        private TgcScene scene;
        private Effect effect;
        private Texture g_pBaseTexture;
        private Texture g_pHeightmap;
        private Texture g_pBaseTexture2;
        private Texture g_pHeightmap2;
        private Texture g_pBaseTexture3;
        private Texture g_pHeightmap3;
        private Texture g_pBaseTexture4;
        private Texture g_pHeightmap4;
        private float time;
        private List<TgcBoundingAxisAlignBox> rooms = new List<TgcBoundingAxisAlignBox>();

        private List<TgcSkeletalMesh> enemigos = new List<TgcSkeletalMesh>();
        private float[] enemigo_an = new float[50];
        private int cant_enemigos = 0;

        // gui
        private DXGui gui = new DXGui();

        public ComboRata(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-ComboRata";
            Description = "ComboRata";
        }

        public override void Init()
        {
            time = 0f;
            Device d3dDevice = D3DDevice.Instance.Device;

            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "WorkshopShaders\\comborata\\comborata-TgcScene.xml");

            g_pBaseTexture = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\rocks.jpg");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_rocks.tga");

            g_pBaseTexture2 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\stones.bmp");
            g_pHeightmap2 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_stones.tga");

            g_pBaseTexture3 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\granito.jpg");
            g_pHeightmap3 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_saint.tga");

            g_pBaseTexture4 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\granito.jpg");
            g_pHeightmap4 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_four_height.tga");

            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (mesh.Name.Contains("Floor"))
                {
                    rooms.Add(mesh.BoundingBox);
                }
            }

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            lightDirModifier = AddVertex3f("LightDir", new TGCVector3(-1, -1, -1), new TGCVector3(1, 1, 1), TGCVector3.Down);
            minSampleModifier = AddFloat("minSample", 1f, 10f, 10f);
            maxSampleModifier = AddFloat("maxSample", 11f, 50f, 50f);
            heightMapScaleModifier = AddFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            Camara = new TgcFpsCamera(new TGCVector3(147.2558f, 8.0f, 262.2509f), 100f, 10f, Input);
            Camara.SetCamera(new TGCVector3(147.2558f, 8.0f, 262.2509f), new TGCVector3(148.2558f, 8.0f, 263.2509f));

            //Cargar personaje con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            Random rnd = new Random();

            // meto un enemigo por cada cuarto
            cant_enemigos = 0;
            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (mesh.Name.Contains("Floor"))
                {
                    float kx = rnd.Next(25, 75) / 100.0f;
                    float kz = rnd.Next(25, 75) / 100.0f;
                    float pos_x = mesh.BoundingBox.PMin.X * kx + mesh.BoundingBox.PMax.X * (1 - kx);
                    float pos_z = mesh.BoundingBox.PMin.Z * kz + mesh.BoundingBox.PMax.Z * (1 - kz);

                    TgcSkeletalMesh enemigo = skeletalLoader.loadMeshAndAnimationsFromFile(MediaDir + "SkeletalAnimations\\BasicHuman\\" + "CombineSoldier-TgcSkeletalMesh.xml", MediaDir + "SkeletalAnimations\\BasicHuman\\", new string[] { MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml", });
                    //TODO remove AutoTransformEnable dependency
                    enemigo.AutoTransformEnable = true;

                    enemigos.Add(enemigo);

                    //Configurar animacion inicial
                    enemigos[cant_enemigos].playAnimation("Walk", true);
                    enemigos[cant_enemigos].Position = new TGCVector3(pos_x, 1f, pos_z);
                    enemigos[cant_enemigos].Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
                    enemigo_an[cant_enemigos] = 0;
                    cant_enemigos++;
                }
            }

            // levanto el GUI
            float W = D3DDevice.Instance.Width;
            float H = D3DDevice.Instance.Height;
            gui.Create(MediaDir);
            gui.InitDialog(false);
            gui.InsertFrame("Combo Rata", 10, 10, 200, 200, Color.FromArgb(32, 120, 255, 132), frameBorder.sin_borde);
            gui.InsertFrame("", 10, (int)H - 150, 200, 140, Color.FromArgb(62, 120, 132, 255), frameBorder.sin_borde);
            gui.cursor_izq = gui.cursor_der = tipoCursor.sin_cursor;

            // le cambio el font
            gui.font.Dispose();
            // Fonts
            gui.font = new Microsoft.DirectX.Direct3D.Font(d3dDevice, 12, 0, FontWeight.Bold, 0, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch, "Lucida Console");
            gui.font.PreloadGlyphs('0', '9');
            gui.font.PreloadGlyphs('a', 'z');
            gui.font.PreloadGlyphs('A', 'Z');

            gui.RTQ = gui.rectToQuad(0, 0, W, H, 0, 0, W - 150, 160, W - 200, H - 150, 0, H);
        }

        public override void Update()
        {
            PreUpdate();

            Random rnd = new Random();
            float speed = 20f * ElapsedTime;
            for (int t = 0; t < cant_enemigos; ++t)
            {
                float an = enemigo_an[t];
                TGCVector3 vel = new TGCVector3((float)Math.Sin(an), 0, (float)Math.Cos(an));
                //Mover personaje
                TGCVector3 lastPos = enemigos[t].Position;
                enemigos[t].Move(vel * speed);
                enemigos[t].Rotation = new TGCVector3(0, (float)Math.PI + an, 0);           // +(float)Math.PI/2

                //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
                bool collide = false;
                foreach (TgcMesh obstaculo in scene.Meshes)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(enemigos[t].BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collide = true;
                        break;
                    }
                }

                //Si hubo colision, restaurar la posicion anterior
                if (collide)
                {
                    enemigos[t].Position = lastPos;
                    enemigo_an[t] += rnd.Next(0, 100) / 100.0f;
                }

                enemigos[t].updateAnimation(ElapsedTime);
            }

            PostUpdate();
        }

        public override void Render()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            time += ElapsedTime;

            TGCVector3 lightDir = lightDirModifier.Value;
            effect.SetValue("g_LightDir", TGCVector3.Vector3ToFloat3Array(lightDir));
            effect.SetValue("min_cant_samples", minSampleModifier.Value);
            effect.SetValue("max_cant_samples", maxSampleModifier.Value);
            effect.SetValue("fHeightMapScale", heightMapScaleModifier.Value);
            effect.SetValue("fvEyePosition", TGCVector3.Vector3ToFloat3Array(Camara.Position));

            effect.SetValue("time", time);
            effect.SetValue("aux_Tex", g_pBaseTexture);
            effect.SetValue("height_map", g_pHeightmap);
            effect.SetValue("phong_lighting", true);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            d3dDevice.BeginScene();

            foreach (TgcMesh mesh in scene.Meshes)
            {
                bool va = true;
                int nro_textura = 0;
                mesh.Effect = effect;
                if (mesh.Name.Contains("Floor"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Down));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, 1)));
                    nro_textura = 0;
                }
                else
                if (mesh.Name.Contains("Roof"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Up));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, 1)));
                    nro_textura = 0;

                    va = false;
                }
                else
                if (mesh.Name.Contains("East"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(1, 0, 0)));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, 1)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Up));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("West"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(-1, 0, 0)));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, 1)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Up));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("North"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, -1)));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Up));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("South"))
                {
                    effect.SetValue("g_normal", TGCVector3.Vector3ToFloat3Array(new TGCVector3(0, 0, 1)));
                    effect.SetValue("g_tangent", TGCVector3.Vector3ToFloat3Array(new TGCVector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TGCVector3.Vector3ToFloat3Array(TGCVector3.Up));
                    nro_textura = 1;
                }

                switch (nro_textura)
                {
                    case 0:
                    default:
                        effect.SetValue("aux_Tex", g_pBaseTexture);
                        effect.SetValue("height_map", g_pHeightmap);
                        break;

                    case 1:
                        effect.SetValue("aux_Tex", g_pBaseTexture2);
                        effect.SetValue("height_map", g_pHeightmap2);
                        break;

                    case 2:
                        effect.SetValue("aux_Tex", g_pBaseTexture3);
                        effect.SetValue("height_map", g_pHeightmap3);
                        break;

                    case 3:
                        effect.SetValue("aux_Tex", g_pBaseTexture4);
                        effect.SetValue("height_map", g_pHeightmap4);
                        break;
                }

                if (va)
                {
                    mesh.Technique = "ParallaxOcclusion2";
                    mesh.Render();
                }
            }

            //Render personames enemigos
            foreach (TgcSkeletalMesh m in enemigos)
                m.Render();

            // Render hud
            renderHUD();

            gui.trapezoidal_style = false;
            //radar de proximidad
            float max_dist = 80;
            foreach (TgcSkeletalMesh m in enemigos)
            {
                TGCVector3 pos_personaje = Camara.Position;
                TGCVector3 pos_enemigo = m.Position * 1;
                float dist = (pos_personaje - pos_enemigo).Length();

                if (dist < max_dist)
                {
                    pos_enemigo.Y = m.BoundingBox.PMax.Y * 0.75f + m.BoundingBox.PMin.Y * 0.25f;
                    pos_enemigo.Project(d3dDevice.Viewport, TGCMatrix.FromMatrix(d3dDevice.Transform.Projection), TGCMatrix.FromMatrix(d3dDevice.Transform.View), TGCMatrix.FromMatrix(d3dDevice.Transform.World));
                    if (pos_enemigo.Z > 0 && pos_enemigo.Z < 1)
                    {
                        float an = (max_dist - dist) / max_dist * 3.1415f * 2.0f;
                        int d = (int)dist;
                        gui.DrawArc(new TGCVector2(pos_enemigo.X + 20, pos_enemigo.Y), 40, 0, an, 10, dist < 30 ? Color.Tomato : Color.WhiteSmoke);
                        gui.DrawLine(pos_enemigo.X, pos_enemigo.Y, pos_enemigo.X + 20, pos_enemigo.Y, 3, Color.PowderBlue);
                        gui.DrawLine(pos_enemigo.X + 20, pos_enemigo.Y, pos_enemigo.X + 40, pos_enemigo.Y - 20, 3, Color.PowderBlue);
                        gui.TextOut((int)pos_enemigo.X + 50, (int)pos_enemigo.Y - 20, "Proximidad " + d, Color.PowderBlue);
                    }
                }
            }
            gui.trapezoidal_style = true;

            PostRender();
        }

        public void renderHUD()
        {
            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.RenderState.ZBufferEnable = false;
            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;

            // Elapsed time
            int an = (int)(time * 10) % 360;
            float hasta = an / 180.0f * (float)Math.PI;
            gui.DrawArc(new TGCVector2(40, H - 100), 25, 0, hasta, 8, Color.Yellow);
            gui.TextOut(20, H - 140, "Elapsed Time:" + Math.Round(time), Color.LightSteelBlue);

            // dibujo los enemigos
            TGCVector3 pos_personaje = Camara.Position;
            TGCVector3 dir_view = Camara.LookAt - pos_personaje;
            TGCVector2 dir_v = new TGCVector2(dir_view.X, dir_view.Z);
            dir_v.Normalize();
            TGCVector2 dir_w = new TGCVector2(dir_v.Y, -dir_v.X);

            int dx = 1000;
            int dy = 1000;
            int dW = 200;
            int dH = 200;
            float ex = dW / (float)dx;
            float ey = dH / (float)dy;
            int ox = 10 + dW / 2;
            int oy = 10 + dH / 2;

            for (int t = 0; t < cant_enemigos; ++t)
            {
                TGCVector3 pos = enemigos[t].Position - pos_personaje;
                TGCVector2 p = new TGCVector2(pos.X, pos.Z);
                float x = TGCVector2.Dot(dir_w, p);
                float y = TGCVector2.Dot(dir_v, p);
                int xm = (int)(ox + x * ex);
                int ym = (int)(oy + y * ey);

                if (Math.Abs(xm - ox) < dW / 2 - 10 && Math.Abs(ym - oy) < dH / 2 - 10)
                    gui.DrawRect(xm - 2, ym - 2, xm + 2, ym + 2, 1, Color.WhiteSmoke);
            }

            TGCVector2[] P = new TGCVector2[20];
            P[0] = new TGCVector2(ox - 5, oy + 5);
            P[1] = new TGCVector2(ox + 5, oy + 5);
            P[2] = new TGCVector2(ox, oy - 10);
            P[3] = P[0];
            gui.DrawSolidPoly(P, 4, Color.Tomato, false);
            gui.DrawCircle(new TGCVector2(ox, oy), 14, 3, Color.Yellow);

            foreach (TgcBoundingAxisAlignBox room in rooms)
            {
                TGCVector2[] Q = new TGCVector2[4];
                TGCVector2[] Qp = new TGCVector2[5];

                float xm = 0;
                float ym = 0;
                Q[0] = new TGCVector2(room.PMin.X - pos_personaje.X, room.PMin.Z - pos_personaje.Z);
                Q[1] = new TGCVector2(room.PMin.X - pos_personaje.X, room.PMax.Z - pos_personaje.Z);
                Q[2] = new TGCVector2(room.PMax.X - pos_personaje.X, room.PMax.Z - pos_personaje.Z);
                Q[3] = new TGCVector2(room.PMax.X - pos_personaje.X, room.PMin.Z - pos_personaje.Z);
                for (int t = 0; t < 4; ++t)
                {
                    float x = TGCVector2.Dot(dir_w, Q[t]);
                    float y = TGCVector2.Dot(dir_v, Q[t]);
                    Qp[t] = new TGCVector2(ox + x * ex, oy + y * ey);
                    xm += x * ex;
                    ym += y * ey;
                }
                Qp[4] = Qp[0];
                xm /= 4;
                ym /= 4;

                if (Math.Abs(xm) < dW / 2 - 10 && Math.Abs(ym) < dH / 2 - 10)
                    gui.DrawPoly(Qp, 5, 1, Color.Tomato);
            }

            // posicion X,Z
            float kx = pos_personaje.X * ex;
            P[0] = new TGCVector2(10, H - 10);
            P[1] = new TGCVector2(15, H - 30);
            P[2] = new TGCVector2(5 + kx, H - 30);
            P[3] = new TGCVector2(25 + kx, H - 10);
            P[4] = P[0];
            gui.DrawSolidPoly(P, 5, Color.Tomato);
            gui.DrawPoly(P, 5, 2, Color.HotPink);

            float kz = pos_personaje.Z * ey;
            P[0] = new TGCVector2(10, H - 40);
            P[1] = new TGCVector2(15, H - 60);
            P[2] = new TGCVector2(5 + kz, H - 60);
            P[3] = new TGCVector2(25 + kz, H - 40);
            P[4] = P[0];
            gui.DrawSolidPoly(P, 5, Color.Green);
            gui.DrawPoly(P, 5, 2, Color.YellowGreen);

            d3dDevice.RenderState.ZBufferEnable = true;
            gui.Render();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
            effect.Dispose();
            g_pBaseTexture.Dispose();
            g_pHeightmap.Dispose();
            g_pBaseTexture2.Dispose();
            g_pHeightmap2.Dispose();
            g_pBaseTexture3.Dispose();
            g_pHeightmap3.Dispose();
            g_pBaseTexture4.Dispose();
            g_pHeightmap4.Dispose();
            gui.Dispose();
            foreach (TgcSkeletalMesh m in enemigos)
            {
                m.Dispose();
            }
        }
    }
}