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
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Tutorial
{
    /// <summary>
    /// Tutorial 2:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e iluminacion - Texturas
    /// 
    /// Muestra como crear una caja 3D con una imagen 2D como textura para darle color.
    /// 
    /// Autor: Matías Leone
    /// 
    /// </summary>
    public class Tutorial2 : TgcExample
    {

        TgcBox box;


        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 2";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D con una imagen 2D como textura para darle color.";
        }


        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargamos una textura
            //Una textura es una imágen 2D que puede dibujarse arriba de un polígono 3D para darle color.
            //Es muy útil para generar efectos de relieves y superficies.
            //Puede ser cualquier imágen 2D (jpg, png, gif, etc.) y puede ser editada con cualquier editor
            //normal (photoshop, paint, descargada de goole images, etc).
            //El framework viene con un montón de texturas incluidas y organizadas en categorias (texturas de
            //madera, cemento, ladrillo, pasto, etc). Se encuentran en la carpeta del framework:
            //  TgcViewer\Examples\Media\MeshCreator\Textures
            //Podemos acceder al path de la carpeta "Media" utilizando la variable "GuiController.Instance.ExamplesMediaDir".
            //Esto evita que tengamos que hardcodear el path de instalación del framework.
            TgcTexture texture = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Textures\\Madera\\cajaMadera3.jpg");

            //Creamos una caja 3D ubicada en (0, -3, 0), dimensiones (5, 10, 5) y la textura como color.
            Vector3 center = new Vector3(0, -3, 0);
            Vector3 size = new Vector3(5, 10, 5);
            box = TgcBox.fromSize(center, size, texture);


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
