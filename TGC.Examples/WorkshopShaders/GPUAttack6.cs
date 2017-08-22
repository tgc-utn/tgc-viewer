using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace Examples.Shaders
{
    public class EjemploGPUAttack6 : TGCExampleViewer
    {
        private string MyMediaDir;
        private string MyShaderDir;
        private Effect effect;
        private Texture g_pRenderTarget, g_pTempData;
        private Surface g_pDepthStencil;     // Depth-stencil buffer
        private VertexBuffer g_pVB;
        private static int MAX_DS = 1024;
        public int a = 33;
        public int c = 213;
        public int m = 251;
        public int[] hash = new int[6];
        public bool found = false;
        public int prefix_x = 0;
        public int prefix_y = 0;

        public EjemploGPUAttack6(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-GPUAttack6";
            Description = "GPUAttack6";
        }

        public unsafe override void Init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;

            MyMediaDir = MediaDir + "\\WorkshopShaders\\";
            MyShaderDir = ShadersDir + "\\WorkshopShaders\\";

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "GPUAttack.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            // temporaria para recuperar los valores
            g_pTempData = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, 0, Format.A8R8G8B8, Pool.SystemMemory);
            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(MAX_DS, MAX_DS, DepthFormat.D24S8, MultiSampleType.None, 0, true);

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
            g_pVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVB.SetData(vertices, 0, LockFlags.None);

            string input = InputBox("Ingrese la Clave de 6 letras (A..Z)", "Clave", "CHARLY");

            int[] clave = new int[6];
            for (int i = 0; i < 6; ++i)
                clave[i] = input[i];
            Hash(clave, hash);
            effect.SetValue("hash_buscado6", hash);

            char[] buffer = new char[7];
            for (int i = 0; i < 6; ++i)
                buffer[i] = (char)hash[i];
            buffer[6] = (char)0;
            string msg = new string(buffer);
            msg = "El hash es " + msg + "\n";
            MessageBox.Show(msg);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public unsafe override void Render()
        {
            if (found)
                return;

            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;

            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            effect.SetValue("prefix_x", prefix_x);
            effect.SetValue("prefix_y", prefix_y);

            device.RenderState.ZBufferEnable = false;
            device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            device.BeginScene();
            effect.Technique = "ComputeHash6";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVB, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            device.EndScene();

            device.SetRenderTarget(0, pOldRT);
            device.DepthStencilSurface = pOldDS;

            // leo los datos de la textura
            // ----------------------------------------------------------------------
            Surface pDestSurf = g_pTempData.GetSurfaceLevel(0);
            device.GetRenderTargetData(pSurf, pDestSurf);
            Byte* pData = (Byte*)pDestSurf.LockRectangle(LockFlags.None).InternalData.ToPointer();

            string msg = "";

            for (int i = 0; i < MAX_DS; i++)
            {
                for (int j = 0; j < MAX_DS; j++)
                {
                    Byte A = *pData++;
                    Byte R = *pData++;
                    Byte G = *pData++;
                    Byte B = *pData++;

                    if (R == 255 && G == 255 && B == 255)
                    {
                        int group_x = j / 32;
                        int x = j % 32;
                        int group_y = i / 32;
                        int y = i % 32;

                        int[] clave = new int[6];
                        clave[0] = 'A' + prefix_x;
                        clave[1] = 'A' + prefix_y;
                        clave[2] = 'A' + group_x;
                        clave[3] = 'A' + group_y;
                        clave[4] = 'A' + x;
                        clave[5] = 'A' + y;
                        Hash(clave, hash);
                        char[] buffer = new char[7];
                        for (int t = 0; t < 6; ++t)
                            buffer[t] = (char)clave[t];
                        buffer[6] = (char)0;
                        msg = new string(buffer);
                        msg = "La clave que elegiste es " + msg;
                        found = true;
                        i = j = MAX_DS;
                    }

                    /*

                    int group_x = j / 32;
                    int x = j % 32;
                    int group_y = i/ 32;
                    int y = i % 32;

                    int[] clave = new int[6];
                    clave[0] = 'A' + prefix_x;
                    clave[1] = 'A' + prefix_y;
                    clave[2] = 'A' + group_x;
                    clave[3] = 'A' + group_y;
                    clave[4] = 'A' + x;
                    clave[5] = 'A' + y;
                    Hash(clave, hash);

                    if (hash[0] != G || hash[1] != R || hash[2] != A || hash[3] != B)
                    {
                        int a = 0;
                    }
                     */
                }
            }
            pDestSurf.UnlockRectangle();
            pSurf.Dispose();

            if (found)
                MessageBox.Show(msg);

            char[] saux = new char[3];
            saux[0] = (char)('A' + prefix_x);
            saux[1] = (char)('A' + prefix_y);
            saux[2] = (char)0;
            string s = new string(saux);
            s = "Hacking " + s + "AAAA - " + s + "ZZZZ";

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            GuiController.Instance.Text3d.drawText(s, 100, 300, Color.Yellow);
            device.EndScene();

            if (++prefix_x == 32)
            {
                prefix_x = 0;
                ++prefix_y;
            }
        }

        public void Hash(int[] clave, int[] buffer)
        {
            int k = (clave[0] + clave[1] + clave[2] + clave[3] + clave[4] + clave[5]) % 256;
            for (int i = 0; i < 6; ++i)
            {
                k = (a * k + c) % m;
                buffer[i] = k;
                k += clave[i];
            }
        }

        public override void Dispose()
        {
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pDepthStencil.Dispose();
            g_pVB.Dispose();
            g_pTempData.Dispose();
        }

        public static String InputBox(String caption, String prompt, String defaultText)
        {
            String localInputText = defaultText;
            if (InputQuery(caption, prompt, ref localInputText))
            {
                return localInputText;
            }
            else
            {
                return "";
            }
        }

        public static int MulDiv(int a, float b, int c)
        {
            return (int)((float)a * b / (float)c);
        }

        public static Boolean InputQuery(String caption, String prompt, ref String value)
        {
            Form form;
            form = new Form();
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Font = SystemFonts.IconTitleFont;

            SizeF dialogUnits;
            dialogUnits = form.AutoScaleDimensions;

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = caption;

            form.ClientSize = new Size(
                        MulDiv(180, dialogUnits.Width, 4),
                        MulDiv(63, dialogUnits.Height, 8));

            form.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.Label lblPrompt;
            lblPrompt = new System.Windows.Forms.Label();
            lblPrompt.Parent = form;
            lblPrompt.AutoSize = true;
            lblPrompt.Left = MulDiv(8, dialogUnits.Width, 4);
            lblPrompt.Top = MulDiv(8, dialogUnits.Height, 8);
            lblPrompt.Text = prompt;

            System.Windows.Forms.TextBox edInput;
            edInput = new System.Windows.Forms.TextBox();
            edInput.Parent = form;
            edInput.Left = lblPrompt.Left;
            edInput.Top = MulDiv(19, dialogUnits.Height, 8);
            edInput.Width = MulDiv(164, dialogUnits.Width, 4);
            edInput.Text = value;
            edInput.SelectAll();

            int buttonTop = MulDiv(41, dialogUnits.Height, 8);
            Size buttonSize = new Size(MulDiv(50, dialogUnits.Width, 4), MulDiv(14, dialogUnits.Height, 8));
            System.Windows.Forms.Button bbOk = new System.Windows.Forms.Button();
            bbOk.Parent = form;
            bbOk.Text = "OK";
            bbOk.DialogResult = DialogResult.OK;
            form.AcceptButton = bbOk;
            bbOk.Location = new Point(MulDiv(38, dialogUnits.Width, 4), buttonTop);
            bbOk.Size = buttonSize;

            System.Windows.Forms.Button bbCancel = new System.Windows.Forms.Button();
            bbCancel.Parent = form;
            bbCancel.Text = "Cancel";
            bbCancel.DialogResult = DialogResult.Cancel;
            form.CancelButton = bbCancel;
            bbCancel.Location = new Point(MulDiv(92, dialogUnits.Width, 4), buttonTop);
            bbCancel.Size = buttonSize;

            if (form.ShowDialog() == DialogResult.OK)
            {
                value = edInput.Text;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}