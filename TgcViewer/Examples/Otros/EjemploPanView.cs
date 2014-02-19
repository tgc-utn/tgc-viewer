using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;

namespace Examples.Otros
{
    /// <summary>
    /// Pan view
    /// </summary>
    public class EjemploPanView : TgcExample
    {

        TgcBox box;

        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Pan view";
        }

        public override string getDescription()
        {
            return "Pan view.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            box = TgcBox.fromSize(new Vector3(0,0,0), new Vector3(20, 20, 20), Color.Red);

            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            box.render(); 
        }

        public override void close()
        {
            box.dispose();
        }



    }
}
