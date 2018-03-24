using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TgcViewer.Utils.Gui;

namespace Examples.WorkshopShaders
{
    // menu item
    public class menu_item : GUIItem
    {
        public TGCVector3 pos_imagen = new TGCVector3(500, 60, 0);

        public menu_item(DXGui gui, string s, string imagen, int id, int x, int y, string mediaDir, int dx = 0, int dy = 0, bool penabled = true) :
            base(gui, s, x, y, dx, dy, id)
        {
            disabled = !penabled;
            seleccionable = true;
            cargar_textura(imagen, mediaDir);
            font = gui.font_medium;
        }

        // custom draw del menu item
        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;

            gui.DrawLine(rc.Left, rc.Bottom, rc.Left + 30, rc.Top, 5, Color.FromArgb(81, 100, 100));
            gui.DrawLine(rc.Left + 30, rc.Top, rc.Right, rc.Top, 5, Color.FromArgb(81, 100, 100));

            if (sel)
            {
                // boton seleccionado: lleno el interior
                TGCVector2[] p = new TGCVector2[20];
                p[0].X = rc.Left;
                p[0].Y = rc.Bottom;
                p[1].X = rc.Left + 30;
                p[1].Y = rc.Top;
                p[2].X = rc.Right;
                p[2].Y = rc.Top;
                p[3].X = rc.Right;
                p[3].Y = rc.Bottom;
                p[4].X = rc.Left;
                p[4].Y = rc.Bottom;
                p[5] = p[0];
                gui.DrawGradientPoly(p, 6, Color.White, Color.FromArgb(35, 56, 68));

                // solo si esta seleccionado (hightlighted) muestro la imagen en un lugar fijo
                if (textura != null)
                    gui.sprite.Draw(textura, Rectangle.Empty, TGCVector3.Empty, pos_imagen,
                        Color.FromArgb(gui.alpha, 255, 255, 255));
            }
            // Texto del boton
            Rectangle rc2 = new Rectangle(rc.Left + 40, rc.Top + 3, rc.Width, rc.Height - 3);
            font.DrawText(gui.sprite, text, rc2, DrawTextFormat.VerticalCenter | DrawTextFormat.Left, Color.WhiteSmoke);
        }
    }

    // menu item secundario
    public class menu_item2 : GUIItem
    {
        public menu_item2(DXGui gui, string s, string imagen, int id, int x, int y, string mediaDir, int dx = 0, int dy = 0, bool penabled = true) :
            base(gui, s, x, y, dx, dy, id)
        {
            disabled = !penabled;
            seleccionable = true;
            cargar_textura(imagen, mediaDir);
            font = gui.font_medium;
        }

        // custom draw del menu item
        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;

            float rx = rc.Height / 2;
            float ry = rc.Height / 2;
            float x0 = rc.Left + rx;
            float y0 = rc.Top + ry;

            if (sel)
            {
                rx += 12;
                ry += 12;
                x0 -= 6;
                y0 -= 6;
            }

            TGCVector2[] p = new TGCVector2[20];
            p[0].X = rc.Left + 50;
            p[0].Y = rc.Bottom - 15;
            p[1].X = rc.Left + 50;
            p[1].Y = rc.Top + 15;
            p[2].X = rc.Right;
            p[2].Y = rc.Top + 15;
            p[3].X = rc.Right;
            p[3].Y = rc.Bottom - 15;
            p[4].X = rc.Left + 50;
            p[4].Y = rc.Bottom - 15;
            p[5] = p[0];

            if (sel)
            {
                gui.DrawGradientPoly(p, 6, Color.White, Color.FromArgb(35, 56, 68));
            }
            else
            {
                gui.DrawSolidPoly(p, 6, Color.FromArgb(42, 71, 90), false);
                gui.DrawPoly(p, 6, 1, Color.FromArgb(50, 75, 240));
            }

            TGCVector2[] Q = new TGCVector2[7];
            for (int i = 0; i < 6; ++i)
            {
                Q[i].X = (float)(x0 + rx * Math.Cos(2 * Math.PI / 6 * i));
                Q[i].Y = (float)(y0 + ry * Math.Sin(2 * Math.PI / 6 * i));
            }
            Q[6] = Q[0];

            gui.DrawSolidPoly(Q, 7, c_fondo, false);

            if (sel)
                // boton seleccionado: lleno el interior
                gui.DrawPoly(Q, 7, 4, DXGui.c_buttom_selected);
            else
                gui.DrawPoly(Q, 7, 2, Color.FromArgb(61, 96, 100));

            if (textura != null)
            {
                gui.sprite.Draw(textura, Rectangle.Empty, TGCVector3.Empty,
                    new TGCVector3(x0 - image_width / 2, y0 - image_height / 2, 0),
                    Color.FromArgb(gui.alpha, 255, 255, 255));
            }

            // Texto del boton
            Rectangle rc2 = new Rectangle(rc.Left + 90, rc.Top + 3, rc.Width, rc.Height - 3);
            font.DrawText(gui.sprite, text, rc2, DrawTextFormat.VerticalCenter | DrawTextFormat.Left,
                sel ? Color.Black : Color.WhiteSmoke);
        }
    }

    // static text
    public class static_text : GUIItem
    {
        public static_text(DXGui gui, string s, int x, int y, int dx = 0, int dy = 0) :
            base(gui, s, x, y, dx, dy, -1)
        {
            disabled = false;
            seleccionable = false;
        }

        // custom draw
        public override void Render(DXGui gui)
        {
            gui.font.DrawText(gui.sprite, text, rc, DrawTextFormat.Left, Color.WhiteSmoke);
            gui.DrawLine(rc.Left, rc.Bottom, rc.Right, rc.Bottom, 6, Color.FromArgb(131, 108, 34));
            gui.DrawLine(rc.Left, rc.Bottom - 2, rc.Right, rc.Bottom - 2, 2, Color.FromArgb(255, 240, 134));
        }
    }

    public class Transformers : TGCExampleViewer
    {
        private string MyMediaDir;
        private string MyShaderDir;

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
        public const int ID_SCOUT = 100;
        public const int ID_HUNTER = 101;
        public const int ID_COMMANDER = 102;
        public const int ID_WARRIOR = 103;
        public int dialog_sel = 0;

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

        public Transformers(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-Transformers";
            Description = "Gui Demo";
        }

        public override void Init()
        {
            Cursor.Hide();

            Device d3dDevice = D3DDevice.Instance.Device;
            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            // levanto el GUI
            gui.Create(MediaDir);

            // menu principal
            gui.InitDialog(false, false);
            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;
            int x0 = 70;
            int y0 = 10;
            int dy = 30;
            int dy2 = dy;
            int dx = 250;
            GUIItem item = gui.InsertImage("transformers//custom_char.png", x0, y0, MediaDir);
            item.image_centrada = false;
            y0 += dy;
            gui.InsertItem(new static_text(gui, "SCOUT", x0, y0, 400, 25));
            y0 += 45;
            item = gui.InsertImage("transformers//scout.png", x0 + dx, y0, MediaDir);
            item.image_centrada = false;
            gui.InsertItem(new menu_item(gui, "SCOUT 1", "transformers//scout1.png", ID_SCOUT, x0, y0, MediaDir, dx, dy));
            y0 += dy + 5;
            gui.InsertItem(new menu_item(gui, "SCOUT 2", "transformers//scout2.png", ID_SCOUT, x0, y0, MediaDir, dx, dy));
            y0 += 2 * dy;

            gui.InsertItem(new static_text(gui, "HUNTER", x0, y0, 400, 25));
            y0 += 45;
            item = gui.InsertImage("transformers//hunter.png", x0 + dx, y0, MediaDir);
            item.image_centrada = false;
            menu_item hunter1 = (menu_item)gui.InsertItem(new menu_item(gui, "HUNTER 1", "transformers//hunter1.png", ID_HUNTER, x0, y0, MediaDir, dx, dy));
            hunter1.pos_imagen.Y = y0;

            y0 += 2 * dy;

            gui.InsertItem(new static_text(gui, "COMMANDER", x0, y0, 400, 25));
            y0 += 45;
            item = gui.InsertImage("transformers//commander.png", x0 + dx, y0, MediaDir);
            item.image_centrada = false;
            menu_item commander1 = (menu_item)gui.InsertItem(new menu_item(gui, "COMMANDER 1", "transformers//commander1.png", ID_COMMANDER, x0, y0, MediaDir, dx, 25));
            commander1.pos_imagen.Y = y0;
            y0 += 2 * dy;

            gui.InsertItem(new static_text(gui, "WARRIOR", x0, y0, 400, 25));
            y0 += 45;
            item = gui.InsertImage("transformers//warrior.png", x0 + dx, y0, MediaDir);
            item.image_centrada = false;
            menu_item warrior1 = (menu_item)gui.InsertItem(new menu_item(gui, "WARRIOR 1", "transformers//warrior1.png", ID_WARRIOR, x0, y0, MediaDir, dx, 30));
            warrior1.pos_imagen.Y = y0;

            dialog_sel = 0;
        }

        public void Configurar()
        {
            gui.InitDialog(false, false);
            int W = D3DDevice.Instance.Width;
            int H = D3DDevice.Instance.Height;
            int x0 = 50;
            int y0 = 50;
            int dy = H - 100;
            int dx = 320;
            dialog_sel = 1;

            gui.InsertItem(new static_text(gui, "WARRIOR", x0, y0, 400, 25));
            y0 += 45;
            gui.InsertItem(new menu_item2(gui, "WARRIOR 1", "transformers//w1.png", ID_WARRIOR, x0, y0, MediaDir, dx, 70));
            y0 += 75;
            gui.InsertItem(new menu_item2(gui, "WARRIOR 2", "transformers//w2.png", ID_WARRIOR, x0, y0, MediaDir, dx, 70));
            y0 += 75;
            gui.InsertItem(new menu_item2(gui, "WARRIOR 3", "transformers//w3.png", ID_WARRIOR, x0, y0, MediaDir, dx, 70));
            y0 += 75;
            gui.InsertItem(new menu_item2(gui, "WARRIOR 4", "transformers//w4.png", ID_WARRIOR, x0, y0, MediaDir, dx, 70));
            y0 += 95;
            gui_circle_button button = gui.InsertCircleButton(0, "SELECT", "ok.png", x0 + 70, y0, MediaDir, 40);
            button.texto_derecha = true;
            button = gui.InsertCircleButton(1, "BACK", "cancel.png", x0 + 300, y0, MediaDir, 40);
            button.texto_derecha = true;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(35, 56, 68), 1.0f, 0);
            gui_render(ElapsedTime);

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

                            if (dialog_sel == 1)
                            {
                                // Estaba en el dialogo de configurar, paso al dialogo principal
                                dialog_sel = 0;
                            }
                            break;

                        case ID_SCOUT:
                            // Abro un nuevo dialogo
                            Configurar();
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
            gui.Render();
        }

        public override void Dispose()
        {
            gui.Dispose();
            Cursor.Show();
        }
    }
}