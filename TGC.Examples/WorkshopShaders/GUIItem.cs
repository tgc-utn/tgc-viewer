using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Examples.WorkshopShaders
{
    // item generico, con soporte de texto, bitmap, etc
    public class GUIItem
    {
        public int item_id;
        public int nro_item;
        public int flags;
        public Rectangle rc;
        public string text;
        public Texture textura;
        public int image_width;
        public int image_height;
        public Texture textura_sel;
        public TgcMesh mesh;
        public float ftime;
        public Color c_fondo;               // color del fondo
        public Color c_font;		        // color de los textos
        public Color c_selected;	        // color seleccionado
        public bool seleccionable;          // indica que el item es seleccionable
        public bool auto_seleccionable;     // no hace falta presionarlo para que genere eventos
        public bool scrolleable;            // indica que el item escrollea junto con el gui
        public bool item3d;
        public bool disabled;
        public itemState state;
        public Microsoft.DirectX.Direct3D.Font font;
        public bool siempre_visible;
        public bool marcado;                // indica que el item esta "chequeado"
        public bool image_centrada;         // indica que la imagen se dibuja centrada

        // auxiliares
        public Point center;

        public int len;

        public GUIItem()
        {
            Clean();
        }

        public void Clean()
        {
            item_id = -1;
            ftime = 0;
            state = itemState.normal;
            seleccionable = false;
            auto_seleccionable = false;
            scrolleable = true;
            text = "";
            rc = Rectangle.Empty;
            textura_sel = textura = null;
            len = 0;
            c_fondo = DXGui.c_fondo;
            c_font = DXGui.c_font;
            c_selected = DXGui.c_selected;
            center = Point.Empty;
            item3d = false;
            disabled = false;
            image_width = image_height = 0;
            siempre_visible = false;
            marcado = false;
            image_centrada = true;
        }

        public GUIItem(DXGui gui, string s, int x, int y, int dx = 0, int dy = 0, int id = -1)
        {
            Clean();
            item_id = id;
            nro_item = gui.cant_items;
            font = gui.font;
            text = s;
            rc = new Rectangle(x, y, dx, dy);
            center = new Point(x + dx / 2, y + dy / 2);
            len = s.Length;
        }

        public void cargar_textura(string imagen, string mediaDir)
        {
            // Cargo la imagen en el gui
            if ((textura = DXGui.cargar_textura(imagen, mediaDir, true)) != null)
            {
                // Aprovecho para calcular el tamaño de la imagen del boton
                SurfaceDescription desc = textura.GetLevelDescription(0);
                image_width = desc.Width;
                image_height = desc.Height;
            }

            // x defecto la imagen seleccionada tiene el mismo nombre con el S_ al principio
            textura_sel = DXGui.cargar_textura("S_" + imagen, mediaDir, true);
        }

        public void Dispose()
        {
            if (textura != null)
                textura.Dispose();

            if (textura_sel != null)
                textura_sel.Dispose();
        }

        // interface:
        public bool pt_inside(DXGui gui, Point p)
        {
            TGCVector2[] Q = new TGCVector2[2];
            Q[0] = new TGCVector2(rc.X, rc.Y);
            Q[1] = new TGCVector2(rc.X + rc.Width, rc.Y + rc.Height);
            gui.Transform(Q, 2);
            Rectangle r = new Rectangle((int)Q[0].X, (int)Q[0].Y, (int)(Q[1].X - Q[0].X), (int)(Q[1].Y - Q[0].Y));
            return r.Contains(p);
        }

        public virtual bool ProcessMsg()
        {
            return false;
        }

        public virtual void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item && !disabled ? true : false;
            Color color = sel ? c_selected : c_font;
            if (rc.Width == 0 || rc.Height == 0)
            {
                // Ajusta el rectangulo para que adapte al texto a dibujar
                Rectangle tw = gui.font.MeasureString(gui.sprite, text, DrawTextFormat.NoClip | DrawTextFormat.Top, color);
                rc.Width = tw.Width + 20;
                rc.Height = tw.Height + 10;
                rc.X -= 10;
                rc.Y -= 5;

                // Recalcula el centro
                center = new Point(rc.X + rc.Width / 2, rc.Y + rc.Height / 2);
            }

            Texture tx = sel && textura_sel != null ? textura_sel : textura;
            if (tx != null)
            {
                TGCVector3 pos = image_centrada ? new TGCVector3(rc.X - image_width, rc.Y + (rc.Height - image_height) / 2, 0) : new TGCVector3(rc.X, rc.Y, 0);
                gui.sprite.Draw(tx, Rectangle.Empty, TGCVector3.Empty, pos, Color.FromArgb(gui.alpha, 255, 255, 255));
            }

            if (sel)
            {
                gui.RoundRect(rc.Left - 8, rc.Top - 6, rc.Right + 8, rc.Bottom + 6, 6, 3,
                    Color.FromArgb(gui.alpha, DXGui.c_selected_frame), false);
                int dy = rc.Height / 2;

                gui.line.Width = 2f;
                gui.line.Begin();

                byte r0 = DXGui.c_grad_inf_0.R;
                byte g0 = DXGui.c_grad_inf_0.G;
                byte b0 = DXGui.c_grad_inf_0.B;
                byte r1 = DXGui.c_grad_inf_1.R;
                byte g1 = DXGui.c_grad_inf_1.G;
                byte b1 = DXGui.c_grad_inf_1.B;

                // Gradiente de abajo
                for (int i = 0; i < dy; ++i)
                {
                    TGCVector2[] pt = new TGCVector2[2];
                    pt[0].X = rc.X - 3;
                    pt[1].X = rc.X + rc.Width + 3;
                    pt[1].Y = pt[0].Y = rc.Y + rc.Height / 2 - i;
                    gui.Transform(pt, 2);
                    float t = (float)i / (float)dy;
                    byte r = (byte)(r0 * t + r1 * (1 - t));
                    byte g = (byte)(g0 * t + g1 * (1 - t));
                    byte b = (byte)(b0 * t + b1 * (1 - t));
                    gui.line.Draw(TGCVector2.ToVector2Array(pt), Color.FromArgb(gui.alpha, r, g, b));
                }

                // Gradiente de arriba
                r0 = DXGui.c_grad_sup_0.R;
                g0 = DXGui.c_grad_sup_0.G;
                b0 = DXGui.c_grad_sup_0.B;
                r1 = DXGui.c_grad_sup_1.R;
                g1 = DXGui.c_grad_sup_1.G;
                b1 = DXGui.c_grad_sup_1.B;

                for (int i = 0; i < dy; ++i)
                {
                    TGCVector2[] pt = new TGCVector2[2];
                    pt[0].X = rc.X - 3;
                    pt[1].X = rc.X + rc.Width + 3;
                    pt[1].Y = pt[0].Y = rc.Y + rc.Height / 2 + i;
                    gui.Transform(pt, 2);
                    float t = (float)i / (float)dy;
                    byte r = (byte)(r0 * t + r1 * (1 - t));
                    byte g = (byte)(g0 * t + g1 * (1 - t));
                    byte b = (byte)(b0 * t + b1 * (1 - t));
                    gui.line.Draw(TGCVector2.ToVector2Array(pt), Color.FromArgb(gui.alpha, r, g, b));
                }
                gui.line.End();
            }

            // dibujo el texto pp dicho
            gui.font.DrawText(gui.sprite, text, rc, DrawTextFormat.NoClip | DrawTextFormat.VerticalCenter, disabled ? Color.FromArgb(gui.alpha, DXGui.c_item_disabled) : sel ? Color.FromArgb(gui.alpha, 0, 32, 128) : c_font);
        }
    }

    // menu item
    public class gui_menu_item : GUIItem
    {
        public gui_menu_item(DXGui gui, string s, string imagen, int id, int x, int y, string mediaDir, int dx = 0, int dy = 0, bool penabled = true) :
            base(gui, s, x, y, dx, dy, id)
        {
            disabled = !penabled;
            seleccionable = true;
            cargar_textura(imagen, mediaDir);
        }
    }

    // standard button
    public class gui_button : GUIItem
    {
        public gui_button(DXGui gui, string s, int id, int x, int y, int dx = 0, int dy = 0) :
            base(gui, s, x, y, dx, dy, id)
        {
            seleccionable = true;
        }

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;
            if (textura != null)
            {
                TGCVector3 pos = new TGCVector3(rc.Left - 64, rc.Top - 8, 0);
                gui.sprite.Draw(textura, Rectangle.Empty, TGCVector3.Empty, pos, Color.FromArgb(gui.alpha, 255, 255, 255));
            }

            // recuadro del boton
            gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 15, 3, DXGui.c_buttom_frame);

            if (sel)
                // boton seleccionado: lleno el interior
                gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 10, 1, DXGui.c_buttom_selected, true);

            // Texto del boton
            Rectangle rc2 = new Rectangle(rc.Left, rc.Top + 10, rc.Width, rc.Height - 20);
            Color color = sel ? DXGui.c_buttom_sel_text : DXGui.c_buttom_text;
            gui.font.DrawText(gui.sprite, text, rc, DrawTextFormat.VerticalCenter | DrawTextFormat.Center, color);
        }
    }

    public class gui_color : GUIItem
    {
        public gui_color(DXGui gui, string s, int id, int x, int y) : base(gui, s, x, y, 50, 50, id)
        {
            seleccionable = true;
        }

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;

            float rx = rc.Width / 2;
            float ry = rc.Height / 2;
            float x0 = rc.Left + rx;
            float y0 = rc.Top + ry;

            if (sel || marcado)
            {
                rx += 12;
                ry += 12;
                x0 -= 6;
                y0 -= 6;
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
            if (marcado)
                gui.DrawPoly(Q, 7, 3, Color.FromArgb(240, 245, 245));
            else
                gui.DrawPoly(Q, 7, 1, Color.FromArgb(120, 120, 64));
        }
    }

    public class gui_edit : GUIItem
    {
        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;
            bool foco = gui.foco == nro_item ? true : false;

            // recuadro del edit
            gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 11, 2, Color.FromArgb(80, 220, 20));

            if (foco)
                // tiene foco
                gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 8, 1, Color.FromArgb(255, 255, 255, 255), true);

            // Texto del edit
            Color color = foco ? Color.FromArgb(0, 0, 0) : Color.FromArgb(130, 255, 130);
            gui.font.DrawText(gui.sprite, text, rc, DrawTextFormat.Top | DrawTextFormat.Left, color);

            if (foco)
            {
                // si esta vacio, le agrego una I para que cuente bien el alto del caracter
                string p = text;
                if (p.Length == 0)
                    p += "I";
                Rectangle tw = gui.font.MeasureString(gui.sprite, p, DrawTextFormat.Top | DrawTextFormat.NoClip, color);
                Rectangle rc2 = new Rectangle(rc.Right + tw.Width, rc.Top, 12, rc.Height);
                // dibujo el cursor titilando
                int cursor = (int)(gui.time * 5);
                if (cursor % 2 != 0)
                {
                    gui.line.Width = 8;
                    TGCVector2[] pt = new TGCVector2[2];
                    pt[0].X = rc2.Left;
                    pt[1].X = rc2.Right;
                    pt[1].Y = pt[0].Y = rc2.Bottom;

                    gui.Transform(pt, 2);
                    gui.line.Begin();
                    gui.line.Draw(TGCVector2.ToVector2Array(pt), Color.FromArgb(0, 64, 0));
                    gui.line.End();
                }
            }
        }
    }

    // Rectangular frame
    public class gui_frame : GUIItem
    {
        public frameBorder borde;

        public gui_frame(DXGui gui, string s, int x, int y, int dx, int dy, Color color, frameBorder tipo_borde = frameBorder.rectangular) :
            base(gui, s, x, y, dx, dy)
        {
            c_fondo = color;
            borde = tipo_borde;
        }

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;
            Color clr = Color.FromArgb(Math.Min(gui.alpha, c_fondo.A), c_fondo);
            switch (borde)
            {
                case frameBorder.sin_borde:
                    // dibujo solo interior
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 1, clr, true);
                    break;

                case frameBorder.redondeado:
                    // Interior
                    gui.RoundRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 30, 6, clr, true);
                    // Contorno
                    gui.RoundRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 30, 6, Color.FromArgb(gui.alpha, DXGui.c_frame_border));
                    break;

                case frameBorder.solapa:
                    {
                        float r = 40;
                        TGCVector2[] pt = new TGCVector2[10];
                        pt[0].X = rc.X;
                        pt[0].Y = rc.Y + rc.Height;
                        pt[1].X = rc.X;
                        pt[1].Y = rc.Y;
                        pt[2].X = rc.X + rc.Width - r;
                        pt[2].Y = rc.Y;
                        pt[3].X = rc.X + rc.Width;
                        pt[3].Y = rc.Y + r;
                        pt[4].X = rc.X + rc.Width;
                        pt[4].Y = rc.Y + rc.Height;
                        pt[5].X = rc.X;
                        pt[5].Y = rc.Y + rc.Height;
                        pt[6] = pt[0];

                        gui.DrawSolidPoly(pt, 7, clr, false);
                        gui.DrawPoly(pt, 5, 6, DXGui.c_frame_border);
                    }

                    break;

                case frameBorder.rectangular:
                default:

                    // interior
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 1, Color.FromArgb(gui.alpha, c_fondo), true);
                    // contorno
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 6, Color.FromArgb(gui.alpha, DXGui.c_frame_border));
                    break;
            }

            // Texto del frame
            Rectangle rc2 = new Rectangle(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height);
            rc2.Y += 30;
            rc2.X += 30;
            Color color = sel ? c_selected : c_font;
            gui.font.DrawText(gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, Color.FromArgb(gui.alpha, color));
        }
    }

    // Irregular frame
    public class gui_iframe : GUIItem
    {
        public gui_iframe(DXGui gui, string s, int x, int y, int dx, int dy, Color color) :
            base(gui, s, x, y, dx, dy)
        {
            c_fondo = color;
        }

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;

            float M_PI = (float)Math.PI;
            TGCVector2[] pt = new TGCVector2[255];
            float da = M_PI / 8;
            float alfa;

            float x0 = rc.Left;
            float x1 = rc.Right;
            float y0 = rc.Top;
            float y1 = rc.Bottom;
            float r = 10;
            int t = 0;
            float x = x0;
            float y = y0;
            for (alfa = 0; alfa < M_PI / 2; alfa += da)
            {
                pt[t].X = (float)(x - r * Math.Cos(alfa));
                pt[t].Y = (float)(y - r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y - r;
            ++t;

            pt[t].X = (x1 + x0) / 2;
            pt[t].Y = y - r;
            ++t;
            pt[t].X = (x1 + x0) / 2 + 50;
            pt[t].Y = y + 20 - r;
            ++t;

            x = x1;
            y = y0 + 20;
            for (alfa = M_PI / 2; alfa < M_PI; alfa += da)
            {
                pt[t].X = (float)(x - r * Math.Cos(alfa));
                pt[t].Y = (float)(y - r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x + r;
            pt[t].Y = y;
            ++t;

            x = x1;
            y = y1;
            for (alfa = 0; alfa < M_PI / 2; alfa += da)
            {
                pt[t].X = (float)(x + r * Math.Cos(alfa));
                pt[t].Y = (float)(y + r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y + r;
            ++t;

            pt[t].X = x0 + 150;
            pt[t].Y = y + r;

            ++t;
            pt[t].X = x0 + 100;
            pt[t].Y = y - 20 + r;
            ++t;

            x = x0;
            y = y - 20;
            for (alfa = M_PI / 2; alfa < M_PI; alfa += da)
            {
                pt[t].X = (float)(x + r * Math.Cos(alfa));
                pt[t].Y = (float)(y + r * Math.Sin(alfa));
                ++t;
            }
            pt[t++] = pt[0];

            // interior
            gui.DrawSolidPoly(pt, t, c_fondo);

            // contorno
            gui.DrawPoly(pt, t, 6, DXGui.c_frame_border);

            // Texto del frame
            Rectangle rc2 = new Rectangle(rc.Top, rc.Left, rc.Width, rc.Height);
            rc2.Y += 25;
            rc2.X += 30;
            Color color = sel ? c_selected : c_font;
            gui.font.DrawText(gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, color);
        }
    }

    public class gui_rect : GUIItem
    {
        public int radio;

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;
            Color color = sel ? Color.FromArgb(gui.alpha, 255, 220, 220) : Color.FromArgb(gui.alpha, 130, 255, 130);
            gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, radio, 2, color, false);
        }
    }

    public class gui_progress_bar : GUIItem
    {
        public int desde;
        public int hasta;
        public int pos;

        public gui_progress_bar(DXGui gui, int x, int y, int dx, int dy, int id = -1) :
            base(gui, "", x, y, dx, dy, id)
        {
            pos = desde = 1;
            hasta = 100;
            seleccionable = false;
        }

        public void SetRange(int d, int h, string s = "")
        {
            desde = d;
            hasta = h;
            text = s;
        }

        public void SetPos(int p)
        {
            pos = p;
        }

        public override void Render(DXGui gui)
        {
            float k = (float)(pos - desde) / (float)(hasta - desde);
            gui.DrawRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 1, Color.FromArgb(240, 240, 240), true);
            gui.DrawRect(rc.Left, rc.Top, rc.Right, rc.Bottom, 1, Color.FromArgb(0, 0, 0));
            gui.DrawRect(rc.Left, rc.Top, rc.Left + (int)(rc.Width * k), rc.Bottom, 1, Color.FromArgb(0, 100, 255), true);
            gui.DrawRect(rc.Left, rc.Top, rc.Left + (int)(rc.Width * k), rc.Bottom, 1, Color.FromArgb(0, 0, 0));

            Rectangle rc2 = new Rectangle(rc.Left, rc.Top - 50, rc.Width, 50);
            gui.font.DrawText(gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, Color.FromArgb(0, 0, 0));
        }
    }

    public class gui_tile_button : GUIItem
    {
        public float ox, oy, ex, ey, k;
        public bool sel;
        public DXGui gui;
        public bool border;

        public gui_tile_button(DXGui gui, string s, string imagen, int id, int x, int y, string mediaDir, int dx, int dy, bool bscrolleable = true) :
            base(gui, s, x, y, dx, dy, id)
        {
            seleccionable = true;
            scrolleable = bscrolleable;
            border = true;
            // Cargo la imagen en el gui
            cargar_textura(imagen, mediaDir);
        }

        public virtual void InitRender(DXGui p_gui)
        {
            // inicializacion comun a todos los controles
            gui = p_gui;

            // estado del control
            sel = state == itemState.hover;

            // Calcula la escala pp dicha
            ex = gui.ex;
            ey = gui.ey;
            ox = gui.ox;
            oy = gui.oy;
            if (scrolleable)
            {
                // como este boton es un item scrolleable, tiene que aplicar tambien el origen sox,soy
                ox += gui.sox;
                oy += gui.soy;
            }

            // sobre escala por estar seleccionado
            k = 1;
            if (sel)
            {
                // aumento las escala
                k = 1 + (float)(0.5 * (gui.delay_sel0 - gui.delay_sel));

                // Le aplico una matriz de escalado adicional, solo sobre el TEXTO.
                // El glyph tiene su propia matriz

                // Este kilombo es porque una cosa es la escala global que se aplica uniformemente en todo el gui
                // y esta centrada en el origen.
                // Pero esta escala es local, del texto, que se aplica centra en centro del texto, luego de haberlo
                // escalado por la escala global.
                gui.sprite.Transform = gui.sprite.Transform * TGCMatrix.Transformation2D(new TGCVector2((center.X + ox) * ex, (center.Y + oy) * ey), 0, new TGCVector2(k, k),
                        new TGCVector2(0, 0), 0, new TGCVector2(0, 0));
            }
        }

        public virtual void RenderText()
        {
            // dibujo el texto pp dicho
            string buffer = text;
            Color color = sel ? Color.FromArgb(gui.alpha, c_selected) : Color.FromArgb(gui.alpha, c_font);
            Rectangle pos_texto = new Rectangle((int)ox + rc.Left, (int)oy + rc.Bottom + 15, rc.Width, 32);
            gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.Top | DrawTextFormat.Center, sel ? Color.FromArgb(gui.alpha, 0, 32, 128) : Color.FromArgb(gui.alpha, c_font));
        }

        public virtual void RenderFrame()
        {
            // Dibujo un rectangulo
            int x0 = (int)(rc.Left + ox);
            int x1 = (int)(rc.Right + ox);
            int y0 = (int)(rc.Top + oy);
            int y1 = (int)(rc.Bottom + oy);

            if (sel)
            {
                int dmx = (int)(rc.Width * (k - 1) * 0.5);
                int dmy = (int)(rc.Height * (k - 1) * 0.5);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 2, Color.FromArgb(gui.alpha, 0, 0, 0), true);
            }
            else
            if (state == itemState.pressed)
            {
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(gui.alpha, 32, 140, 55));
                float k2 = 1 + (float)(0.5 * gui.delay_press);
                int dmx = (int)(rc.Width * (k2 - 1) * 1.1);
                int dmy = (int)(rc.Height * (k2 - 1) * 1.1);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 8, Color.FromArgb(gui.alpha, 255, 0, 0));
            }
            else
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(gui.alpha, 32, 140, 55));
        }

        public virtual void RenderGlyph()
        {
            // dibujo el glyph
            if (textura != null)
            {
                TGCVector3 pos = new TGCVector3(center.X * ex, center.Y * ey, 0);
                TGCVector3 c0 = new TGCVector3(image_width / 2, image_height / 2, 0);
                // Determino la escala para que entre justo
                TGCVector2 scale = new TGCVector2(k * ex * (float)rc.Width / (float)image_width, k * ey * (float)rc.Height / (float)image_height);
                TGCVector2 offset = new TGCVector2(ox * ex, oy * ey);
                gui.sprite.Transform = TGCMatrix.Transformation2D(new TGCVector2(center.X * ex, center.Y * ey), 0, scale, new TGCVector2(0, 0), 0, offset) * gui.RTQ;
                gui.sprite.Draw(textura, c0, pos, Color.FromArgb(gui.alpha, 255, 255, 255).ToArgb());
            }
        }

        public override void Render(DXGui gui)
        {
            // Guardo la TGCMatrix anterior
            TGCMatrix matAnt = TGCMatrix.FromMatrix(gui.sprite.Transform) * TGCMatrix.Identity;
            // Inicializo escalas, matrices, estados
            InitRender(gui);
            // Secuencia standard: texto + Frame + Glyph
            RenderText();
            if (border)
                RenderFrame();
            RenderGlyph();
            // Restauro la transformacion del sprite
            gui.sprite.Transform = matAnt;
        }
    }

    public class gui_circle_button : gui_tile_button
    {
        public Color c_border = Color.FromArgb(0, 0, 0);
        public Color c_interior_sel = Color.FromArgb(30, 240, 40);
        public bool texto_derecha;

        public gui_circle_button(DXGui gui, string s, string imagen, int id, int x, int y, string mediaDir, int r) :
            base(gui, s, imagen, id, x, y, mediaDir, r, r)

        {
            texto_derecha = false;              // indica si el texto va a derecha o debajo del glyph
        }

        public override void RenderText()
        {
            // dibujo el texto pp dicho
            string buffer = text;
            Color color = sel ? Color.FromArgb(gui.alpha, c_selected) : Color.FromArgb(gui.alpha, c_font);
            if (texto_derecha)
            {
                Rectangle pos_texto = new Rectangle((int)ox + rc.Right, (int)oy + rc.Top + rc.Height / 2, rc.Width, 32);
                gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.VerticalCenter | DrawTextFormat.Left, sel ? Color.FromArgb(gui.alpha, 0, 32, 128) : Color.FromArgb(gui.alpha, c_font));
            }
            else
            {
                Rectangle pos_texto = new Rectangle((int)ox + rc.Left, (int)oy + rc.Bottom + 15, rc.Width, 32);
                gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.Top | DrawTextFormat.Center, sel ? Color.FromArgb(gui.alpha, 0, 32, 128) : Color.FromArgb(gui.alpha, c_font));
            }
        }

        public override void RenderFrame()
        {
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            // circulo
            int R = (int)(rc.Width / 2 * k);

            gui.DrawCircle(new TGCVector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R, 10, Color.FromArgb(gui.alpha, c_border));

            // relleno
            if (sel)
                gui.DrawDisc(new TGCVector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R - 10,
                    Color.FromArgb((byte)(255 * tr), c_interior_sel.R, c_interior_sel.G, c_interior_sel.B));
            else
                if (state == itemState.pressed)
            {
                gui.DrawDisc(new TGCVector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R - 10, Color.FromArgb(255, 255, 0, 0));
                int R2 = (int)(rc.Width / 2 + gui.delay_press * 100);
                gui.DrawCircle(new TGCVector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R2, 10, Color.FromArgb(255, 120, 120));
            }
        }
    }
}