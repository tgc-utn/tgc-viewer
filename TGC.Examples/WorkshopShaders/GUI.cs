using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.IO;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;

namespace TgcViewer.Utils.Gui
{
    public enum tipoCursor
    {
        sin_cursor,
        targeting,
        over,
        progress,
        pressed,
        gripped
    }

    public enum itemState
    {
        normal,
        hover,
        pressed,
        disabled
    }

    public enum MessageType
    {
        WM_NOTHING,
        WM_PRESSING,
        WM_COMMAND,
        WM_CLOSE,
    }

    public enum frameBorder
    {
        rectangular,
        sin_borde,
        redondeado,
        solapa,
    }

    public struct GuiMessage
    {
        public MessageType message;
        public int id;
    }

    // Vertex format para dibujar en 2d
    public struct VERTEX2D
    {
        public float x, y, z, rhw;		// Posicion
        public int color;		// Color
    };

    public struct st_dialog
    {
        public int item_0;
        public bool trapezoidal_style;
        public bool autohide;
        public bool hoover_enabled;
    };

    public struct st_bitmap
    {
        public Texture texture;
        public string fname;
        public int Width;
        public int Height;
    }

    public class DXGui
    {
        // Defines
        public const int MAX_GUI_ITEMS = 100;

        public const int MAX_BITMAPS = 10;
        public const int MAX_CURSOR = 10;
        public const int MAX_DIALOG = 20;

        // Eventos
        public const int EVENT_FIRST_SCROLL = 60000;

        public const int EVENT_SCROLL_LEFT = 60000;
        public const int EVENT_SCROLL_RIGHT = 60001;

        // Otras
        public float M_PI = (float)Math.PI;

        public float M_PI_2 = (float)Math.PI * 0.5f;

        // Pila de items seleccionados
        public const int MAX_ITEMS_SEL = 300;

        public const float MENU_OFFSET = 400;
        public const float MENU_OFFSET_SALIDA = 800;

        public GUIItem[] items = new GUIItem[MAX_GUI_ITEMS];
        public int cant_items;
        public int item_0;
        public int sel;		            // item seleccionado
        public int item_pressed;		// item prsionado

        public int foco;		        // item con foco
        public float delay_sel;
        public float delay_sel0;
        public float delay_press;
        public float delay_press0;
        public float time;
        public float delay_show;
        public bool hidden;
        public TGCMatrix RTQ;              // Matriz de cambio de Rectangle to Quad (Trapezoidal quad)
        public float timer_sel;

        // Estilos del dialogo actual
        public bool trapezoidal_style;

        public bool autohide;

        // Colores x defecto
        public static Color c_fondo = Color.FromArgb(80, 30, 155, 110);        // Color de fondo

        public static Color c_font = Color.FromArgb(255, 255, 255);            // Color de font item normal
        public static Color c_selected = Color.FromArgb(255, 128, 128);        // Color de font item seleccionado
        public static Color c_selected_frame = Color.FromArgb(128, 192, 255);  // Color de borde selected rect
        public static Color c_grad_inf_0 = Color.FromArgb(255, 254, 237);       // Color gradiente inferior menu item valor hasta
        public static Color c_grad_inf_1 = Color.FromArgb(255, 235, 182);       // Color gradiente inferior menu item valor desde
        public static Color c_grad_sup_0 = Color.FromArgb(255, 231, 162);       // Color gradiente superior menu item valor hasta
        public static Color c_grad_sup_1 = Color.FromArgb(255, 217, 120);       // Color gradiente superior menu item valor desde
        public static Color c_buttom_frame = Color.FromArgb(80, 220, 20);       // Color recuadro del boton
        public static Color c_buttom_selected = Color.FromArgb(184, 224, 248);  // Color interior del boton seleccionado
        public static Color c_buttom_text = Color.FromArgb(130, 255, 130);      // Color texto del boton
        public static Color c_buttom_sel_text = Color.FromArgb(255, 220, 220);  // Color texto del boton seleccionado
        public static Color c_frame_border = Color.FromArgb(130, 255, 130);     // Color borde los frames
        public static Color c_item_disabled = Color.FromArgb(128, 128, 128);    // color texto item deshabilitado

        // Cableados
        public int rbt;				    // radio button transfer

        public Color sel_color;		    // color seleccionado

        // Escala y origen global de todo el dialogo
        public float ex, ey, ox, oy;

        // origen de los items scrolleables
        public float sox, soy;

        // pila para dialogos
        public st_dialog[] dialog = new st_dialog[MAX_DIALOG];		// pila para guardar el primer item

        public int cant_dialog;

        public Sprite sprite;
        public Line line;
        public Microsoft.DirectX.Direct3D.Font font;
        public Microsoft.DirectX.Direct3D.Font font_small;
        public Microsoft.DirectX.Direct3D.Font font_medium;

        // Cursores
        public Texture[] cursores = new Texture[MAX_CURSOR];

        public tipoCursor cursor_der, cursor_izq;

        // Pool de texturas para DrawImage
        public st_bitmap[] bitmaps = new st_bitmap[MAX_BITMAPS];

        public int cant_bitmaps;

        // Transicion entre dialogos
        public float delay_initDialog;

        public byte alpha;

        // Posicion del mouse
        public float mouse_x;

        public float mouse_y;

        // Posicion anterior
        public float mouse_x_ant;

        public float mouse_y_ant;
        public float delay_move;
        public float delay_move0 = 0.5f;

        // Camara TGC
        public TgcFpsCamera camera;

        // Parametros srcoll automatico
        public float vel_scroll = 500;           // pixeles por segundo

        public float min_sox = -3000;
        public float max_sox = 3000;

        public bool hoover_enabled;             // indica si esta habilitado que si se queda parado en un lugar sintetize un press
        public bool closing;                    // indica que el dialogo se esta cerrado.
        public bool blocked;

        public DXGui()
        {
            cant_items = 0;
            cant_dialog = 0;
            trapezoidal_style = true;
            autohide = false;
            hoover_enabled = true;
            alpha = 255;

            // Computo la matrix de cambio rect to quad
            float W = D3DDevice.Instance.Width;
            float H = D3DDevice.Instance.Height;
            RTQ = rectToQuad(0, 0, W, H, 0, 0, W - 50, 60, W - 100, H - 50, 0, H);
            closing = false;
        }

        public void Reset()
        {
            cant_items = 0;
            item_pressed = sel = -1;
            time = 0;
            item_0 = 0;
            ey = ex = 1;
            ox = oy = 0;
            sox = soy = 0;
            mouse_x = mouse_y = -1;
            mouse_x_ant = mouse_y_ant = -1;
            delay_move = 0;
            for (int i = 0; i < MAX_CURSOR; ++i)
                cursores[i] = null;
            cant_bitmaps = 0;
            cursor_izq = tipoCursor.targeting;
            cursor_der = tipoCursor.targeting;
            alpha = 1;
            timer_sel = 0;
            closing = false;
        }

        public void Dispose()
        {
            font.Dispose();
            font_small.Dispose();
            font_medium.Dispose();
            sprite.Dispose();
            line.Dispose();
            for (int i = 0; i < MAX_CURSOR; ++i)
                if (cursores[i] != null)
                    cursores[i].Dispose();

            for (int i = 0; i < cant_bitmaps; ++i)
                bitmaps[i].texture.Dispose();
        }

        // interface
        public void Create(String mediaDir)
        {
            Reset();
            // Creo el sprite
            Device d3dDevice = D3DDevice.Instance.Device;
            sprite = new Sprite(d3dDevice);
            // lines varios
            line = new Line(d3dDevice);
            // Fonts
            font = new Microsoft.DirectX.Direct3D.Font(d3dDevice, 22, 0, FontWeight.Light, 0, false, CharacterSet.Default,
                    Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch, "Lucida Console");
            font.PreloadGlyphs('0', '9');
            font.PreloadGlyphs('a', 'z');
            font.PreloadGlyphs('A', 'Z');

            font_small = new Microsoft.DirectX.Direct3D.Font(d3dDevice, 10, 0, FontWeight.Light, 0, false, CharacterSet.Default,
                    Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch, "Lucida Console");
            font_small.PreloadGlyphs('0', '9');
            font_small.PreloadGlyphs('a', 'z');
            font_small.PreloadGlyphs('A', 'Z');

            font_medium = new Microsoft.DirectX.Direct3D.Font(d3dDevice, 16, 0, FontWeight.Bold, 0, false, CharacterSet.Default,
                    Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch, "Lucida Console");
            font_medium.PreloadGlyphs('0', '9');
            font_medium.PreloadGlyphs('a', 'z');
            font_medium.PreloadGlyphs('A', 'Z');

            // Cargo las textura del cursor
            cursores[(int)tipoCursor.targeting] = cargar_textura("cursor_default.png", mediaDir, true);
            cursores[(int)tipoCursor.over] = cargar_textura("cursor_over.png", mediaDir, true);
            cursores[(int)tipoCursor.gripped] = cargar_textura("cursor_gripper.png", mediaDir, true);
        }

        // dialog support
        public void InitDialog(bool pautohide = false, bool trapezoidal = true, bool delay = false)
        {
            // guardo el valor de item_0 en la pila
            dialog[cant_dialog].item_0 = item_0;
            // y el valor del estilo del dialogo actual
            dialog[cant_dialog].trapezoidal_style = trapezoidal_style;
            dialog[cant_dialog].autohide = autohide;
            dialog[cant_dialog].hoover_enabled = hoover_enabled;

            ++cant_dialog;
            // y el primer item del nuevo dialog es cant items
            item_0 = cant_items;
            // y seteo el nuevo estilo de dialogo
            trapezoidal_style = trapezoidal;
            autohide = pautohide;
            hoover_enabled = true;           // X defecto tiene el hoover enabled
            foco = -1;
            rbt = -1;
            sel = -1;
            timer_sel = 0;
            ox = oy = 0;
            Show();
            delay_initDialog = delay ? 1.0f : 0;
            closing = false;
        }

        public void EndDialog()
        {
            // actualizo la cantidad de items
            cant_items = item_0;
            // recupero el valor de item_0 y del estilo del dialogo
            --cant_dialog;
            item_0 = dialog[cant_dialog].item_0;
            trapezoidal_style = dialog[cant_dialog].trapezoidal_style;
            autohide = dialog[cant_dialog].autohide;
            hoover_enabled = dialog[cant_dialog].hoover_enabled;
            // Saco el foco
            foco = -1;
            // valores x defecto del scroll
            ox = oy = 0;
            sox = soy = 0;
            // Resteo cualquier item seleccionado anterior y timer de seleccion
            sel = -1;
            timer_sel = 0;

            // el closing puede tardar un huevo, porque usualmente despues de hacer el endialog, se ejecuta
            // el comando que se selecciono, y si esa opcion tarda mucho, cuando se repinta el dialogo anterior
            // todo el tiempo en iddle que esta almacenado en el elapsedtime, se asimila a la posicion del mouse
            // y se puede dar el caso que se presione (hoover) un boton no deseado
            // entonces, al cerrar ponemos este flag en true hasta el siguiente frame donde el sistema retome el control
            closing = true;
        }

        public void Show(bool show = true)
        {
            hidden = !show;
            delay_show = autohide ? 0.5f : 0f;
        }

        // Alerts
        public void MessageBox(string msg, string mediaDir, string titulo = "")
        {
            InitDialog(false, false);
            float W = D3DDevice.Instance.Width / ex;
            float H = D3DDevice.Instance.Height / ey;

            int dx = (int)(700.0f / ex);
            int dy = (int)(450.0f / ey);
            int x0 = (int)((W - dx) / 2);
            int y0 = (int)((H - dy) / 2);
            int r = 170;

            InsertFrame(titulo, x0, y0, dx, dy, Color.FromArgb(64, 32, 64));
            InsertItem(msg, x0 + 50, y0 + 80);
            InsertCircleButton(0, "OK", "ok.png", x0 + 70, y0 + dy - r - 90, mediaDir, r);
            InsertCircleButton(1, "CANCEL", "cancel.png", x0 + dx - r - 70, y0 + dy - r - 90, mediaDir, r);
        }

        // input
        public GuiMessage ProcessInput(float elapsed_time, TgcD3dInput input)
        {
            GuiMessage msg = new GuiMessage();
            msg.message = MessageType.WM_NOTHING;
            msg.id = -1;
            int ant_sel = sel;

            // Hardcodeado escala dinamica

            if (input.keyPressed(Microsoft.DirectX.DirectInput.Key.F2))
            {
                ex /= 1.1f;
                ey = ex;
            }
            else
                if (input.keyPressed(Microsoft.DirectX.DirectInput.Key.F3))
            {
                ex *= 1.1f;
                ey = ex;
            }

            // tomo la posicion del mouse
            float sx = input.Xpos;
            float sy = input.Ypos;

            // Autohide dialog
            if (autohide)
            {
                if (mouse_x < MENU_OFFSET && hidden)
                    // El dialogo esta oculto y se mueve con el mouse a posicion izquierda
                    Show();
                else
                    if (mouse_x > MENU_OFFSET_SALIDA && !hidden)
                    // El dialogo esta visible y se mueve con el mouse a posicion derecha
                    Show(false);
            }

            // verifico si el cusor pasa por arriba de un item, si es seleccionable, lo muestro
            sel = -1;
            int t = item_0;
            while (t < cant_items && sel == -1)
            {
                if (!items[t].disabled && (items[t].seleccionable || items[t].auto_seleccionable))
                {
                    Point pt = new Point(0, 0);
                    if (items[t].scrolleable)
                    {
                        pt.X = (int)(sx - sox * ex);
                        pt.Y = (int)(sy - soy * ey);
                    }
                    else
                    {
                        pt.X = (int)sx;
                        pt.Y = (int)sy;
                    }
                    if (items[t].pt_inside(this, pt))
                        sel = t;
                }
                ++t;
            }

            if (ant_sel != sel)
            {
                // cambio de seleccion
                if (ant_sel != -1)
                    items[ant_sel].state = itemState.normal;
                if (sel != -1)
                    items[sel].state = itemState.hover;

                // inicio el timer de seleccion
                delay_sel0 = delay_sel = 0.5f;

                // inicio el timer de quieto
                timer_sel = 0;
            }

            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                // Presiona el item actual
                if (sel != -1)
                {
                    items[sel].state = itemState.pressed;
                    // y cambio el estado de checked del item
                    // primero limpio todo porque solo puede haber un item marcado en todo el dialogo
                    for (int j = item_0; j < cant_items; ++j)
                        items[j].marcado = false;
                    // Y ahora marco el item donde hice click
                    items[sel].marcado = true;
                    // Esta version no soporta varios Grupos de checkbox (como el group y el tabstop de windows RC)

                    // inicio el timer de press
                    delay_press0 = delay_press = 0.1f;
                    // Reseteo el timer sel
                    timer_sel = 0;
                    // genero el mensaje
                    msg.message = MessageType.WM_PRESSING;
                    msg.id = items[sel].item_id;
                    // guardo el item presionado, por si se mueve del mismo antes que se genere el evento wm_command
                    item_pressed = sel;
                }
            }

            // mouse move ?
            if (Math.Abs(mouse_x - sx) > 0.5f || Math.Abs(mouse_y - sy) > 0.5f)
            {
                // Guardo la pos. anterior del mouse
                mouse_x_ant = mouse_x;
                mouse_y_ant = mouse_y;
                // Inicializo el timer de mov. suave del mouse
                delay_move = delay_move0;
            }
            // Actualizo la pos actual
            mouse_x = sx;
            mouse_y = sy;
            return msg;
        }

        public GuiMessage Update(float elapsed_time, TgcD3dInput input)
        {
            if (closing)
            {
                // el dialogo se estaba cerrando,
                closing = false;
                GuiMessage msg = new GuiMessage();
                msg.message = MessageType.WM_CLOSE;
                msg.id = -1;
                item_pressed = -1;
                return msg;     // y termino de procesar por este frame
            }

            // Actualizo los timers
            time += elapsed_time;

            if (delay_initDialog > 0)
            {
                delay_initDialog -= elapsed_time;
                if (delay_initDialog < 0)
                    delay_initDialog = 0;
            }

            // Si hay un item seleccionado inicio el timer para el press (hoover model)
            // Salvo que sea un autoselccionable, en ese caso no hay timer, ya que se presiona constantemente
            if (sel != -1 && !items[sel].auto_seleccionable)
                timer_sel += elapsed_time;
            else
                timer_sel = 0;

            if (delay_show > 0)
            {
                delay_show -= elapsed_time;
                if (delay_show < 0)
                    delay_show = 0;

                if (hidden)
                    ox = -MENU_OFFSET * (1 - delay_show);
                else
                    ox = -MENU_OFFSET * delay_show;
            }

            if (delay_sel > 0)
            {
                delay_sel -= elapsed_time;
                if (delay_sel < 0)
                    delay_sel = 0;
            }

            if (delay_move > 0)
            {
                delay_move -= elapsed_time;
                if (delay_move < 0)
                    delay_move = 0;
            }

            // computo la matriz de transformacion final RTQ
            if (trapezoidal_style)
            {
                float W = D3DDevice.Instance.Width;
                float H = D3DDevice.Instance.Height;
                RTQ = rectToQuad(0, 0, W, H,
                              0, 0, W - 50, 60, W - 100, H - 50, 0, H);
            }
            else
                RTQ = TGCMatrix.Identity;

            if (delay_press > 0)
            {
                delay_press -= elapsed_time;
                if (delay_press < 0)
                {
                    // Termino el delay de press
                    delay_press = 0;
                    // Si habia algun item presionado, lo libero
                    if (sel != -1 && items[sel].state == itemState.pressed)
                        items[sel].state = itemState.normal;

                    // Aca es el mejor momento para generar el msg, asi el usuario tiene tiempo de ver la animacion
                    // de que el boton se esta presionando, antes que se triggere el comando
                    // genero el mensaje, ojo, uso item_pressed, porque por ahi se movio desde el momento que se genero
                    // el primer evento de pressing y en ese caso sel!=item_pressed
                    if (item_pressed != -1)
                    {
                        GuiMessage msg = new GuiMessage();
                        msg.message = MessageType.WM_COMMAND;
                        msg.id = items[item_pressed].item_id;
                        // Y limpio el item pressed, evitando cualquier posibilidad de generar 2 veces el mismo msg
                        item_pressed = -1;
                        return msg;     // y termino de procesar por este frame
                    }
                }
            }

            // Actualizo el timer de los items actuales
            for (int i = item_0; i < cant_items; ++i)
                items[i].ftime += elapsed_time;

            // Proceso el input y devuelve el resultado
            return ProcessInput(elapsed_time, input);
        }

        public void Render()
        {
            Device d3dDevice = D3DDevice.Instance.Device;
            if (sprite.Disposed)
                return;

            // elimino cualquier textura que me cague el modulate del vertex color
            d3dDevice.SetTexture(0, null);
            // Desactivo el zbuffer
            bool ant_zenable = d3dDevice.RenderState.ZBufferEnable;
            d3dDevice.RenderState.ZBufferEnable = false;

            // 1- dibujo los items 2d con una interface de sprites
            sprite.Begin(SpriteFlags.AlphaBlend);
            TGCMatrix matAnt = TGCMatrix.FromMatrix(sprite.Transform) * TGCMatrix.Identity;
            TGCVector2 scale = new TGCVector2(ex, ey);
            TGCVector2 offset = new TGCVector2(ox, oy);
            sprite.Transform = TGCMatrix.Transformation2D(new TGCVector2(0, 0), 0, scale, new TGCVector2(0, 0), 0, offset) * RTQ;

            int item_desde = item_0;
            // Transicion entre dialogos
            bool hay_tr = false;
            if (delay_initDialog > 0 && cant_dialog > 0)
            {
                // Dibujo los items del dialogo anterior
                item_desde = dialog[cant_dialog - 1].item_0;
                hay_tr = true;
                alpha = (byte)(255 * delay_initDialog);
            }
            else
                alpha = 255;            // No hay transicion

            // si hay un item en modo hover postergo su renderizado para lo ultimo de todo
            // Esto es porque tiene que verse arriba de los demas items y no hay info de Z
            // de momento solo soporta un solo item en modo hover al mismo tiempo
            int item_sel = -1;
            for (int i = item_desde; i < cant_items; ++i)
            {
                if (hay_tr && i == item_0)
                    // Llegue al dialogo actual, actualizo el alpha blend
                    alpha = (byte)(255 - alpha);

                if (!items[i].item3d)
                {
                    if (items[i].state != itemState.hover || item_sel != -1)
                        items[i].Render(this);
                    else
                        item_sel = i;           // Este item lo dibujo al final de todo
                }
            }

            // Item seleccionado
            if (item_sel != -1)
            {
                sprite.End();           // Termino el anterior
                sprite.Begin(SpriteFlags.AlphaBlend);
                items[item_sel].Render(this);
            }
            sprite.End();

            // 3- dibujo los items 3d a travez de la interface usual del TGC (usando la camara y un viewport)
            item_sel = -1;
            d3dDevice.RenderState.ZBufferEnable = true;
            for (int i = item_0; i < cant_items; ++i)
                if (items[i].item3d)
                {
                    if (items[i].state != itemState.hover || item_sel != -1)
                        items[i].Render(this);
                    else
                        item_sel = i;           // Este item lo dibujo al final de todo
                }

            // Item seleccionado
            if (item_sel != -1)
                items[item_sel].Render(this);

            // Desactivo el z buffer para los sprites
            d3dDevice.RenderState.ZBufferEnable = false;

            // Items con el flag siempre visible (que no pertenecen a este dialogo)
            for (int i = 0; i < item_desde; ++i)
                if (items[i].siempre_visible)
                    items[i].Render(this);

            // 4 - dibujo el cusor con la misma interface de prites
            sprite.Begin(SpriteFlags.AlphaBlend);
            sprite.Transform = TGCMatrix.Transformation2D(new TGCVector2(0, 0), 0, new TGCVector2(1, 1), new TGCVector2(0, 0), 0, new TGCVector2(0, 0));

            // mano derecha
            float t = delay_move / delay_move0;
            TGCVector3 hand_pos = new TGCVector3(mouse_x, mouse_y, 0) * (1 - t) + new TGCVector3(mouse_x_ant, mouse_y_ant, 0) * t;
            if (cursores[(int)cursor_der] != null)
            {
                sprite.Transform = TGCMatrix.Transformation2D(new TGCVector2(0, 0), 0, new TGCVector2(1, 1), TGCVector2.Zero, 0, new TGCVector2(0, 0));
                sprite.Draw(cursores[(int)cursor_der], Rectangle.Empty, new TGCVector3(32, 32, 0), hand_pos, Color.FromArgb(255, 255, 255, 255));
            }
            sprite.End();

            // Restauro el zbuffer
            d3dDevice.RenderState.ZBufferEnable = ant_zenable;
            // Restauro la transformacion del sprite
            sprite.Transform = matAnt;
        }

        public void TextOut(int x, int y, string text)
        {
            TextOut(x, y, text, Color.Black);
        }

        public void TextOut(int x, int y, string text, Color clr)
        {
            Device d3dDevice = D3DDevice.Instance.Device;
            // elimino cualquier textura que me cague el modulate del vertex color
            d3dDevice.SetTexture(0, null);
            // Desactivo el zbuffer
            bool ant_zenable = d3dDevice.RenderState.ZBufferEnable;
            d3dDevice.RenderState.ZBufferEnable = false;
            // pongo la matriz identidad
            TGCMatrix matAnt = TGCMatrix.FromMatrix(sprite.Transform) * TGCMatrix.Identity;
            sprite.Transform = trapezoidal_style ? RTQ : TGCMatrix.Identity;

            Rectangle rc = new Rectangle(x, y, x + 600, y + 100);
            sprite.Begin(SpriteFlags.AlphaBlend);
            font.DrawText(sprite, text, rc, DrawTextFormat.NoClip | DrawTextFormat.Top | DrawTextFormat.Left, clr);
            sprite.End();

            // Restauro el zbuffer
            d3dDevice.RenderState.ZBufferEnable = ant_zenable;
            // Restauro la transformacion del sprite
            sprite.Transform = matAnt;
        }

        // Interface para agregar items al UI
        public GUIItem InsertItem(GUIItem item)
        {
            // Inserto el gui item
            items[cant_items] = item;
            // Devuelvo el item pp dicho agregado a la lista
            return items[cant_items++];
        }

        // Inserta un item generico
        public GUIItem InsertItem(String s, int x, int y, int dx = 0, int dy = 0)
        {
            // Static text = item generico
            return InsertItem(new GUIItem(this, s, x, y, dx, dy));
        }

        // Inserta un item imagen estatica
        public GUIItem InsertImage(String image, int x, int y, string mediaDir)
        {
            // Agrego un item generico
            GUIItem item = InsertItem(new GUIItem(this, "", x, y));

            // Cargo el bitmap
            item.cargar_textura(image, mediaDir);

            return item;
        }

        // Pop up menu item
        public gui_menu_item InsertMenuItem(int id, String s, String imagen, int x, int y, string mediaDir, int dx = 0, int dy = 0, bool penabled = true)
        {
            return (gui_menu_item)InsertItem(new gui_menu_item(this, s, imagen, id, x, y, mediaDir, dx, dy, penabled));
        }

        // Standard push button
        public gui_button InsertButton(int id, String s, int x, int y, int dx, int dy)
        {
            return (gui_button)InsertItem(new gui_button(this, s, id, x, y, dx, dy));
        }

        // button
        public gui_circle_button InsertCircleButton(int id, String s, String imagen, int x, int y, string mediaDir, int r)
        {
            return (gui_circle_button)InsertItem(new gui_circle_button(this, s, imagen, id, x, y, mediaDir, r));
        }

        public gui_tile_button InsertTileButton(int id, String s, String imagen, int x, int y, string mediaDir, int dx, int dy, bool scrolleable = true)
        {
            return (gui_tile_button)InsertItem(new gui_tile_button(this, s, imagen, id, x, y, mediaDir, dx, dy, scrolleable));
        }

        // Dialog Frame
        public gui_frame InsertFrame(String s, int x, int y, int dx, int dy, Color c_fondo, frameBorder borde = frameBorder.rectangular)
        {
            return (gui_frame)InsertItem(new gui_frame(this, s, x, y, dx, dy, c_fondo, borde));
        }

        public gui_iframe InsertIFrame(String s, int x, int y, int dx, int dy, Color c_fondo)
        {
            return (gui_iframe)InsertItem(new gui_iframe(this, s, x, y, dx, dy, c_fondo));
        }

        // Progress bar
        public gui_progress_bar InsertProgressBar(int id, int x, int y, int dx, int dy)
        {
            return (gui_progress_bar)InsertItem(new gui_progress_bar(this, x, y, dx, dy, id));
        }

        // color
        public gui_color InsertItemColor(int x, int y, Color color, int id)
        {
            gui_color pitem = (gui_color)InsertItem(new gui_color(this, "", id, x, y));
            pitem.c_fondo = color;
            return pitem;
        }

        public int GetDlgItemCtrl(int id)
        {
            int rta = -1;
            int i = item_0;
            while (i < cant_items && rta == -1)
                if (items[i].item_id == id)
                    rta = i;
                else
                    ++i;
            return rta;
        }

        public GUIItem GetDlgItem(int id)
        {
            int rta = -1;
            int i = item_0;
            while (i < cant_items && rta == -1)
                if (items[i].item_id == id)
                    rta = i;
                else
                    ++i;

            return rta != -1 ? items[rta] : null;
        }

        public void EnableItem(int id, bool enable = true)
        {
            int nro_item = GetDlgItemCtrl(id);
            if (nro_item != -1)
                items[nro_item].disabled = !enable;
        }

        // line support
        public void Transform(VERTEX2D[] pt, int cant_ptos)
        {
            for (int i = 0; i < cant_ptos; ++i)
            {
                float x = ox + pt[i].x * ex;
                float y = oy + pt[i].y * ey;

                if (trapezoidal_style)
                {
                    pt[i].x = x * RTQ.M11 + y * RTQ.M21 + RTQ.M41;
                    pt[i].y = x * RTQ.M12 + y * RTQ.M22 + RTQ.M42;
                    float W = x * RTQ.M14 + y * RTQ.M24 + RTQ.M44;

                    pt[i].x /= W;
                    pt[i].y /= W;
                }
                else
                {
                    pt[i].x = x;
                    pt[i].y = y;
                }
            }
        }

        public void Transform(TGCVector2[] pt, int cant_ptos)
        {
            for (int i = 0; i < cant_ptos; ++i)
            {
                float x = ox + pt[i].X * ex;
                float y = oy + pt[i].Y * ey;

                if (trapezoidal_style)
                {
                    pt[i].X = x * RTQ.M11 + y * RTQ.M21 + RTQ.M41;
                    pt[i].Y = x * RTQ.M12 + y * RTQ.M22 + RTQ.M42;
                    float W = x * RTQ.M14 + y * RTQ.M24 + RTQ.M44;

                    pt[i].X /= W;
                    pt[i].Y /= W;
                }
                else
                {
                    pt[i].X = x;
                    pt[i].Y = y;
                }
            }
        }

        public void DrawLine(float x0, float y0, float x1, float y1, int dw, Color color)
        {
            TGCVector2[] V = new TGCVector2[4];
            V[0].X = x0;
            V[0].Y = y0;
            V[1].X = x1;
            V[1].Y = y1;

            if (dw < 1)
                dw = 1;

            // direccion normnal
            TGCVector2 v = V[1] - V[0];
            v.Normalize();
            TGCVector2 n = new TGCVector2(-v.Y, v.X);

            V[2] = V[1] + n * dw;
            V[3] = V[0] + n * dw;

            VERTEX2D[] pt = new VERTEX2D[16];
            // 1er triangulo
            pt[0].x = V[0].X;
            pt[0].y = V[0].Y;
            pt[1].x = V[1].X;
            pt[1].y = V[1].Y;
            pt[2].x = V[2].X;
            pt[2].y = V[2].Y;

            // segundo triangulo
            pt[3].x = V[0].X;
            pt[3].y = V[0].Y;
            pt[4].x = V[2].X;
            pt[4].y = V[2].Y;
            pt[5].x = V[3].X;
            pt[5].y = V[3].Y;

            for (int t = 0; t < 6; ++t)
            {
                pt[t].z = 0.5f;
                pt[t].rhw = 1;
                pt[t].color = color.ToArgb();
                ++t;
            }

            Transform(pt, 6);

            // dibujo como lista de triangulos
            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2, pt);
        }

        public void DrawPoly(TGCVector2[] V, int cant_ptos, int dw, Color color)
        {
            if (dw < 1)
                dw = 1;
            // Elimino ptos repetidos
            TGCVector2[] P = new TGCVector2[1000];
            int cant = 1;
            P[0] = V[0];
            for (int i = 1; i < cant_ptos; ++i)
                if ((V[i] - V[i - 1]).Length() > 0.01)
                    P[cant++] = V[i];

            cant_ptos = cant;
            bool closed = (P[0] - P[cant_ptos - 1]).Length() < 0.1;

            // calculo el offset
            TGCVector2[] Q = new TGCVector2[1000];
            TGCVector2[] N = new TGCVector2[1000];
            for (int i = 0; i < cant_ptos - 1; ++i)
            {
                TGCVector2 p0 = P[i];
                TGCVector2 p1 = P[i + 1];
                TGCVector2 v = p1 - p0;
                v.Normalize();
                // N = V.normal()
                N[i].X = -v.Y;
                N[i].Y = v.X;
            }

            // ptos intermedios
            int i0 = closed ? 0 : 1;
            for (int i = i0; i < cant_ptos; ++i)
            {
                int ia = i != 0 ? i - 1 : cant_ptos - 2;
                TGCVector2 n = N[ia] + N[i];
                n.Normalize();
                float r = TGCVector2.Dot(N[ia], n);
                if (r != 0)
                    Q[i] = P[i] + n * ((float)dw / r);
                else
                    Q[i] = P[i];
            }

            if (!closed)
            {
                // poligono abierto: primer y ultimo punto:
                Q[0] = P[0] + N[0] * dw;
                Q[cant_ptos - 1] = P[cant_ptos - 1] + N[cant_ptos - 2] * dw;
            }
            else
                Q[cant_ptos - 1] = Q[0];

            VERTEX2D[] pt = new VERTEX2D[4000];
            int t = 0;
            for (int i = 0; i < cant_ptos - 1; ++i)
            {
                // 1er triangulo
                pt[t].x = P[i].X;
                pt[t].y = P[i].Y;
                pt[t + 1].x = Q[i].X;
                pt[t + 1].y = Q[i].Y;
                pt[t + 2].x = P[i + 1].X;
                pt[t + 2].y = P[i + 1].Y;

                // segundo triangulo
                pt[t + 3].x = Q[i].X;
                pt[t + 3].y = Q[i].Y;
                pt[t + 4].x = P[i + 1].X;
                pt[t + 4].y = P[i + 1].Y;
                pt[t + 5].x = Q[i + 1].X;
                pt[t + 5].y = Q[i + 1].Y;

                for (int j = 0; j < 6; ++j)
                {
                    pt[t].z = 0.5f;
                    pt[t].rhw = 1;
                    pt[t].color = color.ToArgb();
                    ++t;
                }
            }

            Transform(pt, t);

            // dibujo como lista de triangulos
            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2 * (cant_ptos - 1), pt);
        }

        public void DrawSolidPoly(TGCVector2[] P, int cant_ptos, Color color, bool gradiente = true)
        {
            // calculo el centro de gravedad
            float xc = 0;
            float yc = 0;
            float ymin = 100000;
            float ymax = -100000;

            for (int i = 0; i < cant_ptos - 1; ++i)
            {
                xc += P[i].X;
                yc += P[i].Y;

                if (P[i].Y > ymax)
                    ymax = P[i].Y;
                if (P[i].Y < ymin)
                    ymin = P[i].Y;
            }

            xc /= (float)(cant_ptos - 1);
            yc /= (float)(cant_ptos - 1);

            float dy = Math.Max(1, ymax - ymin);

            byte a = color.A;
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            VERTEX2D[] pt = new VERTEX2D[4000];
            pt[0].x = xc;
            pt[0].y = yc;
            for (int i = 0; i < cant_ptos; ++i)
            {
                pt[i + 1].x = P[i].X;
                pt[i + 1].y = P[i].Y;
            }

            for (int i = 0; i < cant_ptos + 1; ++i)
            {
                pt[i].z = 0.5f;
                pt[i].rhw = 1;
                if (gradiente)
                {
                    double k = 1 - (pt[i].y - ymin) / dy * 0.5;
                    pt[i].color = Color.FromArgb(a, (byte)Math.Min(255, r * k), (byte)Math.Min(255, g * k), (byte)Math.Min(255, b * k)).ToArgb();
                }
                else
                    pt[i].color = color.ToArgb();
            }

            Transform(pt, cant_ptos + 1);

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, cant_ptos - 1, pt);
        }

        public void DrawGradientPoly(TGCVector2[] P, int cant_ptos, Color color1, Color color2)
        {
            // calculo el centro de gravedad
            float xc = 0;
            float yc = 0;
            float ymin = 100000;
            float ymax = -100000;
            float xmin = 100000;
            float xmax = -100000;

            for (int i = 0; i < cant_ptos - 1; ++i)
            {
                xc += P[i].X;
                yc += P[i].Y;

                if (P[i].Y > ymax)
                    ymax = P[i].Y;
                if (P[i].Y < ymin)
                    ymin = P[i].Y;
                if (P[i].X > xmax)
                    xmax = P[i].X;
                if (P[i].X < xmin)
                    xmin = P[i].X;
            }

            xc /= (float)(cant_ptos - 1);
            yc /= (float)(cant_ptos - 1);

            float dy = Math.Max(1, ymax - ymin);
            float dx = Math.Max(1, xmax - xmin);

            byte a = color1.A;
            // Color desde
            byte r1 = color1.R;
            byte g1 = color1.G;
            byte b1 = color1.B;
            // color hasta
            byte r2 = color2.R;
            byte g2 = color2.G;
            byte b2 = color2.B;

            VERTEX2D[] pt = new VERTEX2D[4000];
            pt[0].x = xc;
            pt[0].y = yc;
            for (int i = 0; i < cant_ptos; ++i)
            {
                pt[i + 1].x = P[i].X;
                pt[i + 1].y = P[i].Y;
            }

            for (int i = 0; i < cant_ptos + 1; ++i)
            {
                pt[i].z = 0.5f;
                pt[i].rhw = 1;
                // Gradiente horizontal
                float k = (pt[i].x - xmin) / dx;           // k = 0..1
                float r = r1 * (1 - k) + r2 * k;
                float g = g1 * (1 - k) + g2 * k;
                float b = b1 * (1 - k) + b2 * k;
                pt[i].color = Color.FromArgb(a, (byte)Math.Min(255, r),
                                                (byte)Math.Min(255, g),
                                                (byte)Math.Min(255, b)).ToArgb();
            }

            Transform(pt, cant_ptos + 1);

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, cant_ptos - 1, pt);
        }

        public void RoundRect(int x0, int y0, int x1, int y1, int r, int dw, Color color, bool solid = false)
        {
            if (dw < 1)
                dw = 1;
            TGCVector2[] pt = new TGCVector2[1000];

            float da = M_PI / 8.0f;
            float alfa;

            x0 += r;
            y0 += r;
            x1 -= r;
            y1 -= r;

            int t = 0;
            float x = x0;
            float y = y0;
            for (alfa = 0; alfa < M_PI_2; alfa += da)
            {
                pt[t].X = x - r * (float)Math.Cos(alfa);
                pt[t].Y = y - r * (float)Math.Sin(alfa);
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y - r;
            ++t;

            x = x1;
            y = y0;
            for (alfa = M_PI_2; alfa < M_PI; alfa += da)
            {
                pt[t].X = x - r * (float)Math.Cos(alfa);
                pt[t].Y = y - r * (float)Math.Sin(alfa);
                ++t;
            }
            pt[t].X = x + r;
            pt[t].Y = y;
            ++t;

            x = x1;
            y = y1;
            for (alfa = 0; alfa < M_PI_2; alfa += da)
            {
                pt[t].X = x + r * (float)Math.Cos(alfa);
                pt[t].Y = y + r * (float)Math.Sin(alfa);
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y + r;
            ++t;

            x = x0;
            y = y1;
            for (alfa = M_PI_2; alfa < M_PI; alfa += da)
            {
                pt[t].X = x + r * (float)Math.Cos(alfa);
                pt[t].Y = y + r * (float)Math.Sin(alfa);
                ++t;
            }
            pt[t++] = pt[0];

            if (solid)
                DrawSolidPoly(pt, t, color, false);
            else
                DrawPoly(pt, t, dw, color);
        }

        public void DrawRect(int x0, int y0, int x1, int y1, int dw, Color color, bool solid = false)
        {
            if (dw < 1)
                dw = 1;
            TGCVector2[] pt = new TGCVector2[5];

            pt[0].X = x0;
            pt[0].Y = y0;
            pt[1].X = x1;
            pt[1].Y = y0;
            pt[2].X = x1;
            pt[2].Y = y1;
            pt[3].X = x0;
            pt[3].Y = y1;
            pt[4] = pt[0];

            if (solid)
                DrawSolidPoly(pt, 5, color, false);
            else
                DrawPoly(pt, 5, dw, color);
        }

        public void DrawDisc(TGCVector2 c, int r, Color color)
        {
            // demasiado pequeño el radio
            if (r < 10)
                return;

            // quiero que cada linea como maximo tenga 3 pixeles
            float da = 3.0f / (float)r;
            int cant_ptos = (int)(2 * M_PI / da);

            VERTEX2D[] pt = new VERTEX2D[cant_ptos + 10];           // + 10 x las dudas

            int t = 0;              // Cantidad de vertices
            // el primer vertice es el centro del circulo
            pt[t].x = c.X;
            pt[t].y = c.Y;
            ++t;
            for (int i = 0; i < cant_ptos; ++i)
            {
                float an = (float)i / (float)cant_ptos * 2 * M_PI;
                pt[t].x = c.X + (float)Math.Cos(an) * r;
                pt[t].y = c.Y + (float)Math.Sin(an) * r;
                ++t;
            }
            pt[t++] = pt[1];      // Cierro el circulo

            for (int j = 0; j < t; ++j)
            {
                pt[j].z = 0.5f;
                pt[j].rhw = 1;
                pt[j].color = color.ToArgb();
            }

            Transform(pt, t);

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, t - 2, pt);
        }

        public void DrawCircle(TGCVector2 c, int r, int esp, Color color)
        {
            // demasiado pequeño el radio
            if (r - esp < 10)
                return;

            // quiero que cada linea como maximo tenga 5 pixeles
            float da = 5.0f / (float)r;
            int cant_ptos = (int)(2 * M_PI / da);

            VERTEX2D[] pt = new VERTEX2D[2 * cant_ptos + 10];           // + 10 x las dudas

            int t = 0;              // Cantidad de vertices

            for (int i = 0; i < cant_ptos; ++i)
            {
                float an = (float)i / (float)cant_ptos * 2 * M_PI;

                // alterno los radios interior y exterior entre los pares e impares

                pt[t].x = c.X + (float)Math.Cos(an) * r;
                pt[t].y = c.Y + (float)Math.Sin(an) * r;
                ++t;

                pt[t].x = c.X + (float)Math.Cos(an) * (r - esp);
                pt[t].y = c.Y + (float)Math.Sin(an) * (r - esp);
                ++t;
            }

            pt[t++] = pt[0];      // Cierro el circulo
            pt[t++] = pt[1];      // Cierro el circulo

            for (int j = 0; j < t; ++j)
            {
                pt[j].z = 0.5f;
                pt[j].rhw = 1;
                pt[j].color = color.ToArgb();
            }

            Transform(pt, t);

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, t - 2, pt);
        }

        public void DrawArc(TGCVector2 c, int r, float desde, float hasta, int esp, Color color)
        {
            // demasiado pequeño el radio
            if (r - esp < 10)
                return;

            if (desde > hasta)
            {
                float aux = desde;
                desde = hasta;
                hasta = aux;
            }

            // quiero que cada linea como maximo tenga 5 pixeles
            float da = 5.0f / (float)r;
            float arc_len = hasta - desde;
            int cant_ptos = (int)(arc_len / da);

            if (cant_ptos <= 0)
                return;

            VERTEX2D[] pt = new VERTEX2D[2 * cant_ptos + 10];           // + 10 x las dudas

            int t = 0;              // Cantidad de vertices

            for (int i = 0; i < cant_ptos; ++i)
            {
                float an = desde + (float)i / (float)cant_ptos * arc_len;

                // alterno los radios interior y exterior entre los pares e impares

                pt[t].x = c.X + (float)Math.Cos(an) * r;
                pt[t].y = c.Y + (float)Math.Sin(an) * r;
                ++t;

                pt[t].x = c.X + (float)Math.Cos(an) * (r - esp);
                pt[t].y = c.Y + (float)Math.Sin(an) * (r - esp);
                ++t;
            }

            for (int j = 0; j < t; ++j)
            {
                pt[j].z = 0.5f;
                pt[j].rhw = 1;
                pt[j].color = color.ToArgb();
            }

            Transform(pt, t);

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.VertexFormat = VertexFormats.Transformed | VertexFormats.Diffuse;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, t - 2, pt);
        }

        public void DrawImage(string fname, int x0, int y0, int x1, int y1, string mediaDir)
        {
            if (sprite.Disposed)
                return;

            // Verifico si la imagen ya esta cargada
            bool flag = false;
            int i = 0;
            while (i < cant_bitmaps && !flag)
                if (fname == bitmaps[i].fname)
                    flag = true;
                else
                    ++i;

            if (!flag)
            {
                // No estaba, cargo la textura
                i = cant_bitmaps++;
                bitmaps[i].texture = cargar_textura(fname, mediaDir);
                bitmaps[i].fname = fname;
                // Aprovecho para calcular el tamaño de la imagen del boton
                SurfaceDescription desc = bitmaps[i].texture.GetLevelDescription(0);
                bitmaps[i].Width = desc.Width;
                bitmaps[i].Height = desc.Height;
            }

            TGCVector3 pos = new TGCVector3((x0 + x1) / 2, (y0 + y1) / 2, 0);
            TGCVector3 c0 = new TGCVector3(bitmaps[i].Width / 2, bitmaps[i].Height / 2, 0);
            // Determino la escala para que entre justo
            TGCVector2 scale2 = new TGCVector2((float)(x1 - x0) / (float)bitmaps[i].Width, (float)(y1 - y0) / (float)bitmaps[i].Height);
            sprite.Transform = TGCMatrix.Transformation2D(new TGCVector2(pos.X, pos.Y), 0, scale2, new TGCVector2(0, 0), 0, new TGCVector2(0, 0));

            Device d3dDevice = D3DDevice.Instance.Device;
            d3dDevice.SetTexture(0, null);
            bool ant_zenable = d3dDevice.RenderState.ZBufferEnable;
            d3dDevice.RenderState.ZBufferEnable = false;
            sprite.Begin(SpriteFlags.AlphaBlend);
            sprite.Draw(bitmaps[i].texture, c0, pos, Color.FromArgb(255, 255, 255, 255).ToArgb());
            sprite.End();
            d3dDevice.RenderState.ZBufferEnable = ant_zenable;
        }

        // Helper para cargar una textura
        public static Texture cargar_textura(String filename, string mediaDir, bool alpha_channel = false)
        {
            Texture textura = null;
            filename.TrimEnd();
            // cargo la textura
            Device d3dDevice = D3DDevice.Instance.Device;
            String fname_aux = mediaDir + "WorkshopShaders\\gui\\" + filename;
            if (!File.Exists(fname_aux))
                // Pruebo con el nombre directo
                fname_aux = filename;
            if (!File.Exists(fname_aux))
                return null;            // File doesnt exist
            try
            {
                if (alpha_channel)
                {
                    textura = TextureLoader.FromFile(d3dDevice, fname_aux, -2, -2, 1, Usage.None,
                        Format.A8B8G8R8, Pool.Managed, Filter.None, Filter.None, 0);
                    // Mask transparente
                    //SetAlphaChannel(textura, 255, 0, 255);
                }
                else
                    textura = TextureLoader.FromFile(d3dDevice, fname_aux, -2, -2, 1, Usage.None,
                        Format.A8B8G8R8, Pool.Managed, Filter.None, Filter.None, 0);
                //textura = TextureLoader.FromFile(d3dDevice, fname_aux);
            }
            catch (Exception error)
            {
            }
            return textura;
        }

        public static int SetAlphaChannel(Texture g_pTexture, byte r0, byte g0, byte b0)
        {
            // Initialize the alpha channel
            // tengo que hacer el reemplazo en TODOS los mipmaps generados para esta textura
            int cant_mipmaps = g_pTexture.LevelCount;
            for (int i = 0; i < cant_mipmaps; ++i)
            {
                SurfaceDescription desc = g_pTexture.GetLevelDescription(i);
                int m_dwWidth = desc.Width;
                int m_dwHeight = desc.Height;
                Surface surf = g_pTexture.GetSurfaceLevel(i);
                int pitch;
                GraphicsStream gs = surf.LockRectangle(LockFlags.Discard, out pitch);
                int size = m_dwHeight * pitch;
                byte[] buffer = new byte[size];
                gs.Read(buffer, 0, size);
                surf.UnlockRectangle();

                for (int y = 0; y < m_dwHeight; y++)
                {
                    for (int x = 0; x < m_dwWidth; x++)
                    {
                        int dwOffset = y * pitch + x * 4;
                        byte b = buffer[dwOffset];
                        byte g = buffer[dwOffset + 1];
                        byte r = buffer[dwOffset + 2];
                        byte a;
                        if (Math.Abs(b - b0) < 15 && Math.Abs(g - g0) < 15 && Math.Abs(r - r0) < 15)		// es el mask transparente
                            a = 0;
                        else
                            a = 255;
                        buffer[dwOffset + 3] = a;
                    }
                }

                gs = surf.LockRectangle(LockFlags.Discard);
                gs.Write(buffer, 0, size);
                surf.UnlockRectangle();
            }

            return 1;
        }

        public TGCMatrix rectToQuad(float X, float Y, float W, float H, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            float y21 = y2 - y1;
            float y32 = y4 - y2;
            float y43 = y3 - y4;
            float y14 = y1 - y3;
            float y31 = y4 - y1;
            float y42 = y3 - y2;

            float a = -H * (x2 * x4 * y14 + x2 * x3 * y31 - x1 * x3 * y32 + x1 * x4 * y42);
            float b = W * (x2 * x4 * y14 + x4 * x3 * y21 + x1 * x3 * y32 + x1 * x2 * y43);
            float c = H * X * (x2 * x4 * y14 + x2 * x3 * y31 - x1 * x3 * y32 + x1 * x4 * y42) - H * W * x1 * (x3 * y32 - x4 * y42 + x2 * y43) - W * Y * (x2 * x4 * y14 + x4 * x3 * y21 + x1 * x3 * y32 + x1 * x2 * y43);

            float d = H * (-x3 * y21 * y4 + x2 * y1 * y43 - x1 * y2 * y43 - x4 * y1 * y3 + x4 * y2 * y3);
            float e = W * (x3 * y2 * y31 - x4 * y1 * y42 - x2 * y31 * y3 + x1 * y4 * y42);
            float f = -(W * (x3 * (Y * y2 * y31 + H * y1 * y32) - x4 * (H + Y) * y1 * y42 + H * x2 * y1 * y43 + x2 * Y * (y1 - y4) * y3 + x1 * Y * y4 * (-y2 + y3)) - H * X * (x3 * y21 * y4 - x2 * y1 * y43 + x4 * (y1 - y2) * y3 + x1 * y2 * (-y4 + y3)));

            float g = H * (x4 * y21 - x3 * y21 + (-x1 + x2) * y43);
            float h = W * (-x2 * y31 + x3 * y31 + (x1 - x4) * y42);
            float i = W * Y * (x2 * y31 - x3 * y31 - x1 * y42 + x4 * y42) + H * (X * (-(x4 * y21) + x3 * y21 + x1 * y43 - x2 * y43) + W * (-(x4 * y2) + x3 * y2 + x2 * y4 - x3 * y4 - x2 * y3 + x4 * y3));

            const double ep = 0.0001;

            if (Math.Abs(i) < ep)
            {
                i = (float)(ep * (i > 0 ? 1.0f : -1.0f));
            }

            TGCMatrix transform = new TGCMatrix();

            // X
            transform.M11 = a / i;
            transform.M21 = b / i;
            transform.M31 = 0;
            transform.M41 = c / i;
            // Y
            transform.M12 = d / i;
            transform.M22 = e / i;
            transform.M32 = 0;
            transform.M42 = f / i;

            // Z
            transform.M13 = 0;
            transform.M23 = 0;
            transform.M33 = 1;
            transform.M43 = 0;

            // W
            transform.M14 = g / i;
            transform.M24 = h / i;
            transform.M34 = 0;
            transform.M44 = 1.0f;

            return transform;
        }

        // Devuelve true si el punto x,y esta sobre una region "hot", es decir que
        // corresponde a un item seleccionable.
        // se usa para determinar si aplicar o no snap al movimiento de la mano
        // No interesa que boton es, solo si esta o no en su region
        public bool IsHotRegion(int x, int y)
        {
            bool rta = false;
            int t = item_0;
            while (t < cant_items && !rta)
            {
                if (!items[t].disabled && (items[t].seleccionable || items[t].auto_seleccionable))
                {
                    Point pt = new Point(0, 0);
                    if (items[t].scrolleable)
                    {
                        pt.X = (int)(x - sox * ex);
                        pt.Y = (int)(y - soy * ey);
                    }
                    else
                    {
                        pt.X = (int)x;
                        pt.Y = (int)y;
                    }
                    if (items[t].pt_inside(this, pt))
                        rta = true;
                }
                ++t;
            }
            return rta;
        }
    }
}