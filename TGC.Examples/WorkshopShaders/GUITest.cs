using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SkeletalAnimation;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.WorkshopShaders
{
    public class GuiTest : TGCExampleViewer
    {
        private Effect effect;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PeekMessage(ref MSG lpMsg, Int32 hwnd, Int32 wMsgFilterMin, Int32 wMsgFilterMax, PeekMessageOption wRemoveMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern Int32 DispatchMessage(ref MSG lpMsg);

        public const Int32 WM_QUIT = 0x12;

        // Defines
        public const int IDOK = 0;

        public const int IDCANCEL = 1;
        public const int ID_ABRIR_MISION = 100;
        public const int ID_GRABAR_MISION = 101;
        public const int ID_NUEVA_MISION = 102;
        public const int ID_CONFIGURAR = 103;
        public const int ID_APP_EXIT = 105;
        public const int ID_PROGRESS1 = 107;
        public const int ID_RESET_CAMARA = 108;

        public gui_progress_bar progress_bar;
        public bool msg_box_app_exit = false;
        public bool profiling = false;

        private TgcSkeletalMesh mesh;
        private Color[] lst_colores = new Color[12];
        private int cant_colores = 12;

        public struct POINTAPI
        {
            public Int32 x;
            public Int32 y;
        }

        public struct MSG
        {
            public Int32 hwmd;
            public Int32 message;
            public Int32 wParam;
            public Int32 lParam;
            public Int32 time;
            public POINTAPI pt;
        }

        public enum PeekMessageOption
        {
            PM_NOREMOVE = 0,
            PM_REMOVE
        }

        // gui
        private DXGui gui = new DXGui();

        public GuiTest(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-GuiTest";
            Description = "Gui Demo";
        }

        public override void Init()
        {
            Cursor.Hide();

            Device d3dDevice = D3DDevice.Instance.Device;

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, ShadersDir + "TgcViewer\\TgcSkeletalMeshShader.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DIFFUSE_MAP";

            //Camara en primera persona
            Camara.SetCamera(new TGCVector3(0, 60, 200), new TGCVector3(0, 0, 0));

            //Cargar personaje con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            mesh = skeletalLoader.loadMeshAndAnimationsFromFile(MediaDir + "SkeletalAnimations\\BasicHuman\\" + "CombineSoldier-TgcSkeletalMesh.xml", MediaDir + "SkeletalAnimations\\BasicHuman\\", new string[] { MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml", MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml", });
            mesh.AutoTransform = true;

            //Configurar animacion inicial
            mesh.playAnimation("StandBy", true);
            mesh.Position = new TGCVector3(0, -50, 0);
            mesh.Scale = new TGCVector3(2f, 2f, 2f);
            mesh.Effect = effect;
            mesh.Technique = "DIFFUSE_MAP";

            // levanto el GUI
            gui.Create(MediaDir);

            // menu principal
            gui.InitDialog(true);
            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;
            int x0 = 70;
            int y0 = 10;
            int dy = 120;
            int dy2 = dy;
            int dx = 400;
            gui.InsertMenuItem(ID_ABRIR_MISION, "Abrir Mision", "open.png", x0, y0, MediaDir, dx, dy);
            gui.InsertMenuItem(ID_NUEVA_MISION, "Play", "Play.png", x0, y0 += dy2, MediaDir, dx, dy);
            gui.InsertMenuItem(ID_CONFIGURAR, "Configurar", "navegar.png", x0, y0 += dy2, MediaDir, dx, dy);
            gui.InsertMenuItem(ID_APP_EXIT, "Salir", "salir.png", x0, y0 += dy2, MediaDir, dx, dy);

            // lista de colores
            lst_colores[0] = Color.FromArgb(100, 220, 255);
            lst_colores[1] = Color.FromArgb(100, 255, 220);
            lst_colores[2] = Color.FromArgb(220, 100, 255);
            lst_colores[3] = Color.FromArgb(220, 255, 100);
            lst_colores[4] = Color.FromArgb(255, 100, 220);
            lst_colores[5] = Color.FromArgb(255, 220, 100);
            lst_colores[6] = Color.FromArgb(128, 128, 128);
            lst_colores[7] = Color.FromArgb(64, 255, 64);
            lst_colores[8] = Color.FromArgb(64, 64, 255);
            lst_colores[9] = Color.FromArgb(255, 0, 255);
            lst_colores[10] = Color.FromArgb(255, 255, 0);
            lst_colores[11] = Color.FromArgb(0, 255, 255);
        }

        public override void Update()
        {
            PreUpdate();

            mesh.RotateY(ElapsedTime * 1.2f);

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            gui_render(ElapsedTime);

            if (profiling)
            {
                Device device = D3DDevice.Instance.Device;
                Viewport ant_view = device.Viewport;
                Viewport view = new Viewport();
                view.X = (int)(400 * gui.ex);
                view.Y = (int)(100 * gui.ey);
                view.Width = (int)(400 * gui.ex);
                view.Height = (int)(300 * gui.ey);
                view.MinZ = 0;
                view.MaxZ = 1;

                device.Viewport = view;

                mesh.Render();

                device.Viewport = ant_view;
            }

            PostRender();
        }

        public void gui_render(float elapsedTime)
        {
            // ------------------------------------------------
            GuiMessage msg = gui.Update(elapsedTime, Input);

            // proceso el msg
            switch (msg.message)
            {
                case MessageType.WM_COMMAND:
                    switch (msg.id)
                    {
                        case IDOK:
                        case IDCANCEL:
                            // Resultados OK, y CANCEL del ultimo messagebox
                            gui.EndDialog();
                            profiling = false;
                            if (msg_box_app_exit)
                            {
                                // Es la resupuesta a un messagebox de salir del sistema
                                if (msg.id == IDOK)
                                {
                                    // Salgo del sistema
                                    //GuiController.Instance.shutDown();
                                    Cursor.Show();
                                }
                            }
                            msg_box_app_exit = false;
                            break;

                        case ID_ABRIR_MISION:
                            ProgressBarDlg();
                            break;

                        case ID_NUEVA_MISION:
                            gui.MessageBox("Nueva Misión", "TGC Gui Demo");
                            break;

                        case ID_CONFIGURAR:
                            Configurar();
                            break;

                        case ID_APP_EXIT:
                            gui.MessageBox("Desea Salir?", "TGC Gui Demo");
                            msg_box_app_exit = true;
                            break;

                        default:
                            if (msg.id >= 0 && msg.id < cant_colores)
                            {
                                // Cambio el color
                                int color = msg.id;

                                effect.SetValue("color_global", new TGCVector4((float)lst_colores[color].R / 255.0f, (float)lst_colores[color].G / 255.0f, (float)lst_colores[color].B / 255.0f, 1));
                            }
                            break;
                    }
                    break;

                default:
                    break;
            }
            gui.Render();
        }

        public void Configurar()
        {
            gui.InitDialog(false, false);
            profiling = true;
            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;
            int x0 = 50;
            int y0 = 50;
            int dy = H - 100;
            int dx = W - 100;
            GUIItem frame = gui.InsertIFrame("Profiling", x0, y0, dx, dy, Color.FromArgb(140, 240, 140));
            frame.c_font = Color.FromArgb(0, 0, 0);
            gui.InsertButton(IDOK, "OK", x0 + dx - 300, y0 + dy - 60, 120, 60);
            gui.InsertButton(IDCANCEL, "Salir", x0 + dx - 140, y0 + dy - 60, 120, 60);
            gui.InsertItem("Configure el color", x0 + 50, y0 + 130);

            int cdx = 50;
            int pos_x = x0 + 100;
            int pos_y = y0 + 220;
            int s = 0;
            for (int i = 0; i < cant_colores; ++i)
            {
                GUIItem item = gui.InsertItemColor(pos_x, pos_y, lst_colores[i], i);
                if ((i + 1) % 4 == 0)
                {
                    if (s % 2 == 1)
                        pos_x = x0 + 100;
                    else
                        pos_x = x0 + 100 - 38;
                    pos_y += cdx / 2;
                    s++;
                }
                else
                    pos_x += cdx + cdx / 2;

                // uso el texto para meter el dato del nro de color
                item.text = "" + i;
            }
        }

        public void ProgressBarDlg()
        {
            gui.InitDialog(false, false);

            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;
            int x0 = -20;
            int y0 = 100;
            int dy = 350;
            int dx = W + 50;

            GUIItem frame = gui.InsertFrame("Cargando mision", x0, y0, dx, dy, Color.FromArgb(240, 240, 240), frameBorder.sin_borde);
            frame.c_font = Color.FromArgb(0, 0, 0);
            progress_bar = gui.InsertProgressBar(ID_PROGRESS1, 50, y0 + 150, W - 100, 60);

            Device d3dDevice = D3DDevice.Instance.Device;
            int cant_textures = 5;
            progress_bar.SetRange(0, cant_textures, "Descargando archivos..");
            progress_bar.SetPos(1);
            for (int i = 0; i < cant_textures; ++i)
            {
                progress_bar.SetPos(i);
                progress_bar.text = "Descargando archivo: " + MediaDir + "Texturas\\f1\\f1piso2.png";

                Texture textura_piso = Texture.FromBitmap(d3dDevice, (Bitmap)Bitmap.FromFile(MediaDir + "Texturas\\f1\\f1piso2.png"), Usage.None, Pool.Managed);
                textura_piso.Dispose();
                MessageLoop();
            }

            gui.EndDialog();            // progress bar dialog
        }

        public bool MessageLoop()
        {
            MSG msg = new MSG();
            PeekMessage(ref msg, 0, 0, 0, PeekMessageOption.PM_REMOVE);
            if (msg.message == WM_QUIT)
                return false;
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);

            return true;
        }

        public override void Dispose()
        {
            mesh.Dispose();
            gui.Dispose();
            effect.Dispose();
            Cursor.Show();
        }
    }
}