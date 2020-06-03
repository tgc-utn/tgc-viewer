using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Mathematica;

namespace TGC.Core.Input
{
    /// <summary>
    ///     Manejo de DirectInput para Keyboard y Mouse
    /// </summary>
    public class TgcD3dInput
    {
        /// <summary>
        ///     Botones del mouse para DirectInput
        /// </summary>
        public enum MouseButtons
        {
            BUTTON_LEFT = 0,
            BUTTON_RIGHT = 1,
            BUTTON_MIDDLE = 2
        }

        private const int HISTORY_BUFFER_SIZE = 10;
        private const float WEIGHT_MODIFIER = 0.2f;
        private readonly Point ceroPoint = new Point(0, 0);
        private bool[] currentkeyboardState;
        private bool[] currentMouseButtonsState;
        private TGCVector2[] historyBuffer;

        //Keyboard
        private Device keyboardDevice;

        //Mouse
        private Device mouseDevice;

        private int mouseIndex;
        private bool mouseInside;

        private TGCVector2[] mouseMovement;
        private int mouseX;
        private int mouseY;
        private Control panel3d;

        private bool[] previouskeyboardState;

        private bool[] previousMouseButtonsState;

        public void Initialize(Control guiControl, Control panel3d)
        {
            this.panel3d = panel3d;

            //keyboard
            keyboardDevice = new Device(SystemGuid.Keyboard);
            keyboardDevice.SetCooperativeLevel(guiControl,
                CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            keyboardDevice.Acquire();

            //mouse
            mouseDevice = new Device(SystemGuid.Mouse);
            mouseDevice.SetCooperativeLevel(guiControl,
                CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            mouseDevice.Acquire();
            mouseIndex = 0;
            EnableMouseSmooth = true;
            WeightModifier = WEIGHT_MODIFIER;
            mouseX = 0;
            mouseY = 0;

            //Inicializar mouseMovement
            mouseMovement = new TGCVector2[2];
            for (var i = 0; i < mouseMovement.Length; i++)
            {
                mouseMovement[i] = TGCVector2.Zero;
            }

            //Inicializar historyBuffer
            historyBuffer = new TGCVector2[HISTORY_BUFFER_SIZE];
            for (var i = 0; i < historyBuffer.Length; i++)
            {
                historyBuffer[i] = TGCVector2.Zero;
            }

            //Inicializar ubicacion del cursor
            var ceroToScreen = this.panel3d.PointToScreen(ceroPoint);
            Cursor.Position = new Point(ceroToScreen.X + panel3d.Width / 2, ceroToScreen.Y + panel3d.Height / 2);
            mouseInside = checkMouseInsidePanel3d();

            //Inicializar estados de teclas
            var keysArray = (int[])Enum.GetValues(typeof(Key));
            var maxKeyValue = keysArray[keysArray.Length - 1];
            previouskeyboardState = new bool[maxKeyValue];
            currentkeyboardState = new bool[maxKeyValue];
            for (var i = 0; i < maxKeyValue; i++)
            {
                previouskeyboardState[i] = false;
                currentkeyboardState[i] = false;
            }

            //Inicializar estados de botones del mouse
            previousMouseButtonsState = new bool[3];
            currentMouseButtonsState = new bool[previousMouseButtonsState.Length];
            for (var i = 0; i < previousMouseButtonsState.Length; i++)
            {
                previousMouseButtonsState[i] = false;
                currentMouseButtonsState[i] = false;
            }
        }

        internal void destroy()
        {
            keyboardDevice.Unacquire();
            keyboardDevice.Dispose();

            mouseDevice.Unacquire();
            mouseDevice.Dispose();
        }

        public void update()
        {
            //Ver si el cursor esta dentro del panel3d
            var currentInside = checkMouseInsidePanel3d();

            //Si esta afuera y antes estaba adentro significa que salio. No capturar ningun evento, fuera de jurisdiccion
            if (mouseInside && !currentInside)
            {
                mouseInside = false;
            }

            //Ahora esta adentro, capturar eventos
            else if (currentInside)
            {
                //Estaba afuera y ahora esta adentro, hacer foco en panel3d para perder foco de algun control exterior
                if (!mouseInside)
                {
                    panel3d.Focus();
                }

                mouseInside = true;

                updateKeyboard();
                updateMouse();

                //Terminar ejemplo
                if (keyPressed(Key.Escape))
                {
                    //GuiController.Instance.stopCurrentExample();
                }
            }
        }

        private bool checkMouseInsidePanel3d()
        {
            //Obtener mouse X, Y absolute
            var ceroToScreen = panel3d.PointToScreen(ceroPoint);
            mouseX = Cursor.Position.X - ceroToScreen.X;
            mouseY = Cursor.Position.Y - ceroToScreen.Y;

            //Ver si el cursor esta dentro del panel3d
            return panel3d.ClientRectangle.Contains(mouseX, mouseY);
        }

        internal void updateKeyboard()
        {
            var state = keyboardDevice.GetCurrentKeyboardState();

            //Hacer copia del estado actual
            Array.Copy(currentkeyboardState, previouskeyboardState, currentkeyboardState.Length);

            //Actualizar cada tecla del estado actual
            for (var i = 0; i < currentkeyboardState.Length; i++)
            {
                var k = (Key)(i + 1);
                currentkeyboardState[i] = state[k];
            }
        }

        internal void updateMouse()
        {
            var mouseState = mouseDevice.CurrentMouseState;

            //Hacer copia del estado actual
            Array.Copy(currentMouseButtonsState, previousMouseButtonsState, currentMouseButtonsState.Length);

            //Actualizar estado de cada boton
            var mouseStateButtons = mouseState.GetMouseButtons();
            currentMouseButtonsState[(int)MouseButtons.BUTTON_LEFT] =
                mouseStateButtons[(int)MouseButtons.BUTTON_LEFT] != 0;
            currentMouseButtonsState[(int)MouseButtons.BUTTON_MIDDLE] =
                mouseStateButtons[(int)MouseButtons.BUTTON_MIDDLE] != 0;
            currentMouseButtonsState[(int)MouseButtons.BUTTON_RIGHT] =
                mouseStateButtons[(int)MouseButtons.BUTTON_RIGHT] != 0;

            //Mouse X, Y relative
            if (EnableMouseSmooth)
            {
                performMouseFiltering(mouseState.X, mouseState.Y);
                performMouseSmoothing(XposRelative, YposRelative);
            }
            else
            {
                XposRelative = mouseState.X;
                YposRelative = mouseState.Y;
            }

            //Mouse Wheel
            if (mouseState.Z > 0)
            {
                WheelPos = 1.0f;
            }
            else if (mouseState.Z < 0)
            {
                WheelPos = -1.0f;
            }
            else
            {
                WheelPos = 0.0f;
            }
        }

        /// <summary>
        ///     Filter the relative mouse movement based on a weighted sum of the mouse
        ///     movement from previous frames to ensure that the mouse movement this
        ///     frame is smooth.
        /// </summary>
        private void performMouseFiltering(int x, int y)
        {
            for (var i = historyBuffer.Length - 1; i > 0; --i)
            {
                historyBuffer[i].X = historyBuffer[i - 1].X;
                historyBuffer[i].Y = historyBuffer[i - 1].Y;
            }

            historyBuffer[0].X = x;
            historyBuffer[0].Y = y;

            var averageX = 0.0f;
            var averageY = 0.0f;
            var averageTotal = 0.0f;
            var currentWeight = 1.0f;

            for (var i = 0; i < historyBuffer.Length; i++)
            {
                averageX += historyBuffer[i].X * currentWeight;
                averageY += historyBuffer[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= WeightModifier;
            }

            XposRelative = averageX / averageTotal;
            YposRelative = averageY / averageTotal;
        }

        /// <summary>
        ///     Average the mouse movement across a couple of frames to smooth out mouse movement.
        /// </summary>
        private void performMouseSmoothing(float x, float y)
        {
            mouseMovement[mouseIndex].X = x;
            mouseMovement[mouseIndex].Y = y;

            XposRelative = (mouseMovement[0].X + mouseMovement[1].X) * 0.05f;
            YposRelative = (mouseMovement[0].Y + mouseMovement[1].Y) * 0.05f;

            mouseIndex ^= 1;
            mouseMovement[mouseIndex].X = 0.0f;
            mouseMovement[mouseIndex].Y = 0.0f;
        }

        /// <summary>
        ///     Informa si una tecla se encuentra presionada
        /// </summary>
        public bool keyDown(Key key)
        {
            if (!mouseInside) return false;

            var k = (int)key - 1;
            return currentkeyboardState[k];
        }

        /// <summary>
        ///     Informa si una tecla se dejo de presionar
        /// </summary>
        public bool keyUp(Key key)
        {
            if (!mouseInside) return false;

            var k = (int)key - 1;
            return previouskeyboardState[k] && !currentkeyboardState[k];
        }

        /// <summary>
        ///     Informa si una tecla se presiono y luego se libero
        /// </summary>
        public bool keyPressed(Key key)
        {
            if (!mouseInside) return false;

            var k = (int)key - 1;
            return !previouskeyboardState[k] && currentkeyboardState[k];
        }

        /// <summary>
        ///     Informa si un boton del mouse se encuentra presionado
        /// </summary>
        public bool buttonDown(MouseButtons button)
        {
            if (!mouseInside) return false;

            return currentMouseButtonsState[(int)button];
        }

        /// <summary>
        ///     Informa si un boton del mouse se dejo de presionar
        /// </summary>
        public bool buttonUp(MouseButtons button)
        {
            if (!mouseInside) return false;

            var b = (int)button;
            return previousMouseButtonsState[b] && !currentMouseButtonsState[b];
        }

        /// <summary>
        ///     Informa si un boton del mouse se presiono y luego se libero
        /// </summary>
        public bool buttonPressed(MouseButtons button)
        {
            if (!mouseInside) return false;

            var b = (int)button;
            return !previousMouseButtonsState[b] && currentMouseButtonsState[b];
        }

        #region Getters y Setters

        /// <summary>
        ///     Habilitar Mouse Smooth
        /// </summary>
        public bool EnableMouseSmooth { get; set; }

        /// <summary>
        ///     Influencia para filtrar el movimiento del mouse
        /// </summary>
        public float WeightModifier { get; set; }

        /// <summary>
        ///     Desplazamiento relativo de X del mouse
        /// </summary>
        public float XposRelative { get; private set; }

        /// <summary>
        ///     Desplazamiento relativo de Y del mouse
        /// </summary>
        public float YposRelative { get; private set; }

        /// <summary>
        ///     Posicion absoluta de X del mouse
        /// </summary>
        public float Xpos
        {
            get { return mouseX; }
        }

        /// <summary>
        ///     Posicion absoluta de Y del mouse
        /// </summary>
        public float Ypos
        {
            get { return mouseY; }
        }

        /// <summary>
        ///     Rueda del Mouse
        /// </summary>
        public float WheelPos { get; private set; }

        #endregion Getters y Setters
    }
}