using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Engine2D.Spaceship.Core;
using TGC.Examples.Example;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Transforms2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     Muestra como dibujar un Sprite. Un Sprite es una imagen que se dibuja en dos dimensiones.
    ///     Se aplican diferentes matrices de transformación al sprite.
    ///     Si realizamos animaciones sobre el sprite puede ser útiles para crear menues, huds, etc.
    ///     Autor: Rodrigo, García.
    /// </summary>
    public class Transforms2D : TGCExampleViewer
    {
       private Drawer2D drawer2D;
       private CustomSprite sprite;
        private Vector2 centerScreen;
        private Matrix matrixIdentity;
        private Matrix scaling;
        private Matrix rotation;
        private Matrix traslation;
        private Vector2 centroSprite;
        private Matrix rotationAnimate;
        private float acumlatedTime;
        private Matrix rotationAnimateFrameDepend;
        private Matrix rotationAnimateOneSec;

        public Transforms2D(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "2D";
            Name = "Transforms 2D";
            Description = "Muestra como dibujar un Sprite y transformarlo por la pantalla.";
        }

        public override void Init()
        {
            drawer2D = new Drawer2D();

            //Crear Sprite
            sprite = new CustomSprite();
            sprite.Bitmap = new CustomBitmap(MediaDir + "\\Texturas\\LogoTGC.png", D3DDevice.Instance.Device);

            //Ubicar centrado en la pantalla
            var textureSize = sprite.Bitmap.Size;
            centerScreen = new Vector2(FastMath.Max(D3DDevice.Instance.Width / 2 - textureSize.Width / 2, 0),
                FastMath.Max(D3DDevice.Instance.Height / 2 - textureSize.Height / 2, 0));

            //Calculo el centro de la imagen.
            centroSprite = new Vector2(textureSize.Width / 2, textureSize.Height / 2);

            //Rotaciones animadas iniciales.
            rotationAnimateFrameDepend = Matrix.Identity;
            rotationAnimateOneSec = Matrix.Identity;
            rotationAnimate = Matrix.Identity;


            //tiempo acumulado
            acumlatedTime = 0f;

            //Camara estatica (es igual que definir un view matrix en cada render)
            Camara.SetCamera(new Vector3(0,0,-10), Vector3.Empty);
        }

        public override void Update()
        {
            PreUpdate();

            //Actualizar valores al sprite interno.
            sprite.Position = new Vector2(0f, 0f);
            sprite.Scaling = new Vector2(0.5f, 0.5f);
            sprite.Rotation = 0f;
            //Internamente realiza Matrix.Transformation2D(scalingCenter, 0, scaling, rotationCenter, rotation, position); 
            /*El método Transformation2D calcula la matriz de transformación afín por medio de la fórmula siguiente, evaluando la concatenación de la matriz de izquierda a derecha.
            M salida = (M ce )-1 * (M re )-1 * M s* M re* M ce * (M cr )-1 * M r* M cr* M t
            donde:
            M salida = matriz de transformación de salida (el valor devuelto)
            M ce = matriz de centro de escala (scalingCenter)
            M re = matriz de rotación de escala(scalingRotation)
            M e = matriz de escala(scaling)
            M cr = matriz de centro de rotación(rotationCenter)
            M r = matriz de rotación(rotation)
            M t = matriz de traslación(translation)
            */
           
            //Sacar toda transformación.
            matrixIdentity = Matrix.Identity;
            
            //Traslación al centro de la pantalla.
            traslation = Matrix.Translation(centerScreen.X, centerScreen.Y, 0f);

            //Un escalado, siempre olvidar la coordenada Z.
            scaling = Matrix.Scaling(sprite.Scaling.X, sprite.Scaling.Y, 1f);

            //Las rotaciones en 2D serán siempre rotaciones con eje Z.
            rotation = Matrix.RotationZ(FastMath.ToRad(45));

            //Rotación animada (dependiente de frames)
            rotationAnimateFrameDepend = rotationAnimateFrameDepend * Matrix.RotationZ(FastMath.ToRad(25));

            //Una rotación animada cada 1 segundo.
            if (acumlatedTime > 1f)
            {
                acumlatedTime = 0;
                rotationAnimateOneSec = rotationAnimateOneSec * Matrix.RotationZ(FastMath.ToRad(25)); //roto 25 grados por segundo
            }
            acumlatedTime += ElapsedTime;

            //rotación animada por render (Sin acoplar), 
            //Si ElapsedTime es muy chico (como suele pasar con buenas computadoras y poco procesamiento)
            //Al multiplicarlo por nuestra velocidad angular, se transformaran en "pequeñas rotaciones".
            rotationAnimate = rotationAnimate * Matrix.RotationZ(FastMath.ToRad(25)*ElapsedTime);
            
        }

        public override void Render()
        {
            PreRender();
            //Unos ejes especiales, para orientarnos. Estos no corresponden al origen en 2D, sino al centro de la pantalla.
            //axis.render();


            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            drawer2D.BeginDrawSprite();

            //Aplicar las transformaciones necesarias y luego invocar al render.

            //Ninguna transformación.
            sprite.TransformationMatrix = matrixIdentity;
            //drawer2D.DrawSprite(sprite);

            //Rotamos y trasladamos.
            sprite.TransformationMatrix = rotation * traslation;
            //drawer2D.DrawSprite(sprite);
            
            //Aplicar en orden inverso a la rotación.
            sprite.TransformationMatrix = traslation * rotation;
            //drawer2D.DrawSprite(sprite);

            //Rotar en centro de pantalla.
            var pivotCentro = Matrix.Translation(-centroSprite.X, -centroSprite.Y, 0);
            var invPivotCentro = Matrix.Translation(centroSprite.X, centroSprite.Y, 0);
            sprite.TransformationMatrix = pivotCentro * rotation * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);

            //Diferentes configuraciones de escalado.
            //Primero escalo luego muevo al centro de pantalla.
            sprite.TransformationMatrix = scaling * pivotCentro * rotation * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);

            //Primero muevo al centro de pantalla y escalo.
            sprite.TransformationMatrix = pivotCentro * rotation * invPivotCentro * traslation * scaling;
            //drawer2D.DrawSprite(sprite);

            //voy al centro de la imagen, roto y luego escalo, luego vuelvo al centro de la imagen y al centro de pantalla.
            sprite.TransformationMatrix = pivotCentro * rotation * scaling * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);

            //voy al centro de la imagen, escalo y luego roto, luego vuelvo al centro de la imagen y al centro de pantalla.
            sprite.TransformationMatrix = pivotCentro * scaling * rotation * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);
            
            //Como se puede apreciar las ultimas dos transformaciones dan igual resultado, 
            
            //¿que pasaría si el escalado no es uniforme?
            sprite.TransformationMatrix = pivotCentro * Matrix.Scaling(1f,0.5f,1) * rotation * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);

            sprite.TransformationMatrix = pivotCentro * rotation * Matrix.Scaling(1f, 0.5f, 1) * invPivotCentro * traslation;
            //drawer2D.DrawSprite(sprite);

            //Nos adelantamos un poco ..... ¿¿¿Animaciones????
            sprite.TransformationMatrix = pivotCentro * scaling * rotationAnimate * invPivotCentro * traslation;
            
                drawer2D.DrawSprite(sprite);
            
                

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

            

            PostRender();
        }

        public override void Dispose()
        {
            sprite.Dispose();           
        }
    }
}