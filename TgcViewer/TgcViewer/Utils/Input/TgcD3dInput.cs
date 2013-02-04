using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX;
using System.Drawing;

namespace TgcViewer.Utils.Input
{

    /// <summary>
    /// Manejo de DirectInput para Keyboard y Mouse
    /// </summary>
    public class TgcD3dInput
    {
        /// <summary>
        /// Botones del mouse para DirectInput
        /// </summary>
        public enum MouseButtons : int {
            BUTTON_LEFT   = 0,
            BUTTON_RIGHT  = 1,
            BUTTON_MIDDLE = 2,
        }

        Control guiControl;
        Control panel3d;
        
        //Keyboard
        Microsoft.DirectX.DirectInput.Device keyboardDevice;
        bool[] previouskeyboardState;
        bool[] currentkeyboardState;

        //Mouse
        Microsoft.DirectX.DirectInput.Device mouseDevice;
        bool[] previousMouseButtonsState;
        bool[] currentMouseButtonsState;
        float deltaMouseX;
        float deltaMouseY;
        float deltaMouseWheel;
        int mouseX;
        int mouseY;
        Vector2[] mouseMovement;
        int mouseIndex;
        Vector2[] historyBuffer;
        const int HISTORY_BUFFER_SIZE = 10;
        const float WEIGHT_MODIFIER = 0.2f;
        readonly Point ceroPoint = new Point(0, 0);
        bool mouseInside;


        #region Getters y Setters

        bool enableMouseFiltering;
        /// <summary>
        /// Habilitar Mouse Smooth
        /// </summary>
        public bool EnableMouseSmooth
        {
            get { return enableMouseFiltering; }
            set { enableMouseFiltering = value; }
        }

        float weightModifier;
        /// <summary>
        /// Influencia para filtrar el movimiento del mouse
        /// </summary>
        public float WeightModifier
        {
            get { return weightModifier; }
            set { weightModifier = value; }
        }

        /// <summary>
        /// Desplazamiento relativo de X del mouse
        /// </summary>
        public float XposRelative
        {
            get{ return deltaMouseX; }
        }

        /// <summary>
        /// Desplazamiento relativo de Y del mouse
        /// </summary>
        public float YposRelative
        {
            get{ return deltaMouseY; }
        }

        /// <summary>
        /// Posicion absoluta de X del mouse
        /// </summary>
        public float Xpos
        {
            get { return mouseX; }
        }

        /// <summary>
        /// Posicion absoluta de Y del mouse
        /// </summary>
        public float Ypos
        {
            get { return mouseY; }
        }

        /// <summary>
        /// Rueda del Mouse
        /// </summary>
        public float WheelPos
        {
            get{ return deltaMouseWheel; }
        }

        #endregion


        public TgcD3dInput(Control guiControl, Control panel3d)
        {
            this.guiControl = guiControl;
            this.panel3d = panel3d;

            //keyboard
            keyboardDevice = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyboardDevice.SetCooperativeLevel(guiControl, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            keyboardDevice.Acquire();

            //mouse
            mouseDevice = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Mouse);
            mouseDevice.SetCooperativeLevel(guiControl, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            mouseDevice.Acquire();
            mouseIndex = 0;
            enableMouseFiltering = true;
            weightModifier = WEIGHT_MODIFIER;
            mouseX = 0;
            mouseY = 0;

            //Inicializar mouseMovement
            mouseMovement = new Vector2[2];
            for (int i = 0; i < mouseMovement.Length; i++)
            {
                mouseMovement[i] = new Vector2(0.0f, 0.0f);
            }

            //Inicializar historyBuffer
            historyBuffer = new Vector2[HISTORY_BUFFER_SIZE];
            for (int i = 0; i < historyBuffer.Length; i++)
            {
                historyBuffer[i] = new Vector2(0.0f, 0.0f);
            }

            //Inicializar ubicacion del cursor
            Point ceroToScreen = this.panel3d.PointToScreen(ceroPoint);
            Cursor.Position = new Point(ceroToScreen.X + panel3d.Width / 2, ceroToScreen.Y + panel3d.Height / 2);
            mouseInside = checkMouseInsidePanel3d();


            //Inicializar estados de teclas
            int[] keysArray = (int[])Enum.GetValues(typeof(Key));
            int maxKeyValue = keysArray[keysArray.Length - 1];
            previouskeyboardState = new bool[maxKeyValue];
            currentkeyboardState = new bool[maxKeyValue];
            for (int i = 0; i < maxKeyValue; i++)
            {
                previouskeyboardState[i] = false;
                currentkeyboardState[i] = false;
            }

            //Inicializar estados de botones del mouse
            previousMouseButtonsState = new bool[3];
            currentMouseButtonsState = new bool[previousMouseButtonsState.Length];
            for (int i = 0; i < previousMouseButtonsState.Length; i++)
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

        internal void update()
        {
            //Ver si el cursor esta dentro del panel3d
            bool currentInside = checkMouseInsidePanel3d();

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
                    GuiController.Instance.stopCurrentExample();
                }
            }
        }

        private bool checkMouseInsidePanel3d()
        {
            //Obtener mouse X, Y absolute
            Point ceroToScreen = this.panel3d.PointToScreen(ceroPoint);
            mouseX = Cursor.Position.X - ceroToScreen.X;
            mouseY = Cursor.Position.Y - ceroToScreen.Y;

            //Ver si el cursor esta dentro del panel3d
            return panel3d.ClientRectangle.Contains(mouseX, mouseY);
        }


        internal void updateKeyboard()
        {
            KeyboardState state = keyboardDevice.GetCurrentKeyboardState();

            //Hacer copia del estado actual
            Array.Copy(currentkeyboardState, previouskeyboardState, currentkeyboardState.Length);

            //Actualizar cada tecla del estado actual
            for (int i = 0; i < currentkeyboardState.Length; i++)
            {
                Key k = (Key)(i+1);
                currentkeyboardState[i] = state[k];
            }
        }

        internal void updateMouse()
        {
            MouseState mouseState = mouseDevice.CurrentMouseState;

            //Hacer copia del estado actual
            Array.Copy(currentMouseButtonsState, previousMouseButtonsState, currentMouseButtonsState.Length);

            //Actualizar estado de cada boton
            byte[] mouseStateButtons = mouseState.GetMouseButtons();
            currentMouseButtonsState[(int)MouseButtons.BUTTON_LEFT] = mouseStateButtons[(int)MouseButtons.BUTTON_LEFT] != 0;
            currentMouseButtonsState[(int)MouseButtons.BUTTON_MIDDLE] = mouseStateButtons[(int)MouseButtons.BUTTON_MIDDLE] != 0;
            currentMouseButtonsState[(int)MouseButtons.BUTTON_RIGHT] = mouseStateButtons[(int)MouseButtons.BUTTON_RIGHT] != 0;
                

            //Mouse X, Y relative
            if (enableMouseFiltering)
            {
                performMouseFiltering(mouseState.X, mouseState.Y);
                performMouseSmoothing(deltaMouseX, deltaMouseY);
            }
            else
            {
                deltaMouseX = mouseState.X;
                deltaMouseY = mouseState.Y;
            }


            //Mouse Wheel
            if (mouseState.Z > 0)
            {
                deltaMouseWheel = 1.0f;
            }
            else if (mouseState.Z < 0)
            {
                deltaMouseWheel = -1.0f;
            }
            else
            {
                deltaMouseWheel = 0.0f;
            }

        }

        /// <summary>
        /// Filter the relative mouse movement based on a weighted sum of the mouse
        /// movement from previous frames to ensure that the mouse movement this
        /// frame is smooth. 
        /// </summary>
        private void performMouseFiltering(int x, int y)
        {
            for (int i = historyBuffer.Length - 1; i > 0; --i)
            {
                historyBuffer[i].X = historyBuffer[i - 1].X;
                historyBuffer[i].Y = historyBuffer[i - 1].Y;
            }

            historyBuffer[0].X = x;
            historyBuffer[0].Y = y;

            float averageX = 0.0f;
            float averageY = 0.0f;
            float averageTotal = 0.0f;
            float currentWeight = 1.0f;

            for (int i = 0; i < historyBuffer.Length; i++)
            {
                averageX += historyBuffer[i].X * currentWeight;
                averageY += historyBuffer[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= weightModifier;
            }

            deltaMouseX = averageX / averageTotal;
            deltaMouseY = averageY / averageTotal;
        }

        /// <summary>
        /// Average the mouse movement across a couple of frames to smooth out mouse movement.
        /// </summary>
        private void performMouseSmoothing(float x, float y)
        {
            mouseMovement[mouseIndex].X = x;
            mouseMovement[mouseIndex].Y = y;

            deltaMouseX = (mouseMovement[0].X + mouseMovement[1].X) * 0.05f;
            deltaMouseY = (mouseMovement[0].Y + mouseMovement[1].Y) * 0.05f;

            mouseIndex ^= 1;
            mouseMovement[mouseIndex].X = 0.0f;
            mouseMovement[mouseIndex].Y = 0.0f;
        }

        
        /// <summary>
        /// Informa si una tecla se encuentra presionada
        /// </summary>
        public bool keyDown(Key key)
        {
            if (!mouseInside) return false;

            int k = (int)key - 1;
            return currentkeyboardState[k];
        }

        /// <summary>
        /// Informa si una tecla se dejo de presionar
        /// </summary>
        public bool keyUp(Key key)
        {
            if (!mouseInside) return false;

            int k = (int)key - 1;
            return previouskeyboardState[k] && !currentkeyboardState[k];
        }


        /// <summary>
        /// Informa si una tecla se presiono y luego se libero
        /// </summary>
        public bool keyPressed(Key key)
        {
            if (!mouseInside) return false;

            int k = (int)key - 1;
            return !previouskeyboardState[k] && currentkeyboardState[k];
        }

        /// <summary>
        /// Informa si un boton del mouse se encuentra presionado
        /// </summary>
        public bool buttonDown(MouseButtons button)
        {
            if (!mouseInside) return false;

            return currentMouseButtonsState[(int)button];
        }

        /// <summary>
        /// Informa si un boton del mouse se dejo de presionar
        /// </summary>
        public bool buttonUp(MouseButtons button)
        {
            if (!mouseInside) return false;

            int b = (int)button;
            return previousMouseButtonsState[b] && !currentMouseButtonsState[b];
        }

        /// <summary>
        /// Informa si un boton del mouse se presiono y luego se libero
        /// </summary>
        public bool buttonPressed(MouseButtons button)
        {
            if (!mouseInside) return false;

            int b = (int)button;
            return !previousMouseButtonsState[b] && currentMouseButtonsState[b];
        }


        
        

        

    }
}
