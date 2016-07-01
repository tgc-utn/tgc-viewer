using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 2:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e iluminacion - Texturas
    ///     Muestra como crear una caja 3D con una imagen 2D como textura para darle color.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial2 : TGCExampleViewer
    {
        private TgcBox box;

        public Tutorial2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 2";
            Description = "Muestra como crear una caja 3D con una imagen 2D como textura para darle color.";
        }

        public override void Init()
        {
            //Cargamos una textura
            //Una textura es una imágen 2D que puede dibujarse arriba de un polígono 3D para darle color.
            //Es muy útil para generar efectos de relieves y superficies.
            //Puede ser cualquier imágen 2D (jpg, png, gif, etc.) y puede ser editada con cualquier editor
            //normal (photoshop, paint, descargada de goole images, etc).
            //El framework viene con un montón de texturas incluidas y organizadas en categorias (texturas de
            //madera, cemento, ladrillo, pasto, etc). Se encuentran en la carpeta del framework:
            //  TgcViewer\Examples\Media\MeshCreator\Textures
            //Podemos acceder al path de la carpeta "Media" utilizando la variable "this.MediaDir".
            //Esto evita que tengamos que hardcodear el path de instalación del framework.
            var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Madera\\cajaMadera3.jpg");

            //Creamos una caja 3D ubicada en (0, -3, 0), dimensiones (5, 10, 5) y la textura como color.
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 10, 5);
            box = TgcBox.fromSize(center, size, texture);

            //Hacemos que la cámara esté centrada el box.
            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(),
                box.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            box.render();

            PostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}