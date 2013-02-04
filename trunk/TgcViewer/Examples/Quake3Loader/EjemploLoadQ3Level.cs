using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using System.IO;

namespace Examples.Quake3Loader
{
    /// <summary>
    /// Ejemplo EjemploLoadQ3Level
    /// Unidades Involucradas:
    ///     # Unidad 7 - Optimizaci�n - BSP y PVS
    ///     
    /// 
    /// General:
    /// --------
    /// Muestra como cargar un escenario de Quake 3. 
    /// Herramienta en estado BETA. Su uso es responsabilidad del alumno.
    /// Los escenarios cargados son de la demo del Quake 3.
    /// Pueden obtenerse nuevos escenarios del juego completo y otros sitios.
    /// Cargar un escenario grande puede tardar bastante.
    /// No todas las funcionalidades del escenario son soportadas por la herramienta. Algunos objetos no poseen textura o movimiento.
    /// Pueden crearse escenarios propios con el editor de escenarios GtkRadiant.
    /// Este ejemplo muestra como cargar un nivel customizado llamado "NivelPrueba2"
    /// 
    /// 
    /// Especificaciones t�cnicas:
    /// --------------------------
    /// La clase BspLoader lee el formato de BSP de escenario de Quake 3 y lo convierte a una lista de TgcMesh, dentro
    /// de la clase BspMap.
    /// Las curvas (patches) son teseladas y convertidas a un TgcMesh.
    /// El renderizado del escenario se hace con Frustum Culling utilizando la matriz PVS precalculada en el archivo BSP.
    /// Se utilizan Lightmaps.
    /// El editor GtkRadiant precalcula la matriz PVS, lightmaps y otras cosas.
    /// La clase BspCollisionManager realiza un chequeo de colsiones con el escenario. Las colisiones soportadas son una versi�n
    /// simplificada de la totalidad de colisiones utilizadas en el juego. Quedan muchos aspectos por mejorar.
    /// Quake 3 posee shaders en formato propio. BspLoader y Q3ShaderParser carga y parsea estos shaders propios de Quake 3 y los convierte 
    /// a un equivalente de shader en HLSL de DirectX. Este proceso es experimental, a�n est� incompleto y los shaders no se 
    /// utilizan actualmente para renderizar, solo sus texturas asociadas. Es por eso que muchos escenarios poseen objetos en blanco,
    /// sin textura, o con Alpha Blending mal aplicado.
    /// BspLoader imprime por pantalla todas las texturas o shaders que no se pudieron cargar.
    /// 
    /// 
    /// Editor GtkRadiant
    /// -----------------
    /// Obtener GtkRadiant: http://www.qeradiant.com/cgi-bin/trac.cgi
    /// Tutorial para uso de editor GtkRadiant: http://clankiller.com/games/quake/gtkradianttut/ 
    /// No es posible editar un escenario ya compilado de Quake 3 con la herramienta GtkRadiant.
    /// Si un escenario existente no cumple las expectativas del alumno, es preferible crear uno nuevo, en lugar de intentar modificarlo.
    /// Para cargar un escenario es necesario apuntar el mediaPath a la carpeta con todo el contenido gr�fico del juego Quake original
    /// (con carpetas textures, maps, models, scripts, sprites, etc.) 
    /// Esta carpeta puede obtenerse descomprimiendo el archivo ".pk3" del juego (renombrar a .zip).
    /// El editor GtkRadiant tambi�n necesita todos estos archivos del juego.
    /// La carpeta posee muchas mas texturas y modelos de los que probablemente use un mapa particular.
    /// Para crear un carpeta solo con los m�nimos recursos necesarios, la clase BspLoader posee el m�todo "packLevel()".
    /// Se utiliza de la siguiente manera:
    /// 
    ///     BspLoader loader = new BspLoader();
    ///     BspMap bspMap = loader.loadBsp(mapFile, mediaPath);
    ///     loader.packLevel(bspMap, mediaPath, levelFolder);
    /// 
    /// Este m�todo solo debe llamarse una vez por mapa, en forma offline. Ver ejemplo EjemploEmpaquetarQ3Level.cs
    /// Luego BspLoader puede cargar directamente la versi�n empaquetada.
    /// Los escenarios entregados en el TP de la materia deben estar empaquetados de esta forma.
    /// Los escenarios deben ser compilados en GtkRadiant con la opci�n "Build => Q3Map2 (final) BSP -meta, -vis, -light -fast -filter -super 2 -bounce 8" (ante �ltima)
    /// Los escenarios deben tener luces y la entidad "info_player_deathmatch".
    /// Este ejemplo viene con un escenario customizado llamado "EjemploPrueba2.bsp". El mismo se adjunta con el archivo ".map" para poder
    /// ser abierto y estudiado con GtkRadiant.
    /// Es recomendable evitar usar shaders y efectos raros en el editor.
    /// El editor permite cargar metadata customizada del usuario sobre el escenario. La misma puede ser accedida en:
    ///     bspMap.Data.entdata;
    /// 
    /// 
    /// Autor: Martin Giachetti, Mat�as Leone
    /// 
    /// </summary>
    public class EjemploLoadQ3Level : TgcExample
    {
        BspMap bspMap;
        string currentLevelFile;
        string exampleDir;

        public override string getCategory()
        {
            return "Quake3";
        }

        public override string getName()
        {
            return "Load BSP Level";
        }

        public override string getDescription()
        {
            return "Permite cargar escenarios de Quake 3 en formato BSP. Utiliza matriz PVS para renderizado y posee detecci�n de colisiones. Presionar L para capturar el mouse. Puede tardar unos minutos en cargar.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de este ejemplo
            exampleDir = GuiController.Instance.ExamplesDir + "Quake3Loader\\Levels\\";

            //Cargar nivel inicial
            currentLevelFile = exampleDir + "q3dm1\\maps\\q3dm1.bsp";
            loadLevel(currentLevelFile);


            //Modifiers
            GuiController.Instance.Modifiers.addFile("Level", currentLevelFile, ".Niveles Quake 3|*.bsp");
            GuiController.Instance.Modifiers.addFloat("Speed",0,500f,350f);
            GuiController.Instance.Modifiers.addFloat("Gravity", 0, 600, 180);
            GuiController.Instance.Modifiers.addFloat("JumpSpeed", 60, 600, 100);
            GuiController.Instance.Modifiers.addBoolean("NoClip","NoClip", false);
        }

        /// <summary>
        /// Cargar un escenario de Quake 3
        /// </summary>
        private void loadLevel(string levelFile)
        {
            //Dispose del anterior
            if (bspMap != null)
            {
                bspMap.dispose();
                bspMap = null;
            }

            //Cargar escenario de Quake 3
            string mediaPath = new DirectoryInfo(levelFile).Parent.Parent.FullName + "\\";
            BspLoader loader = new BspLoader();
            bspMap = loader.loadBsp(levelFile, mediaPath);

            //Iniciar visiblidad
            bspMap.initVisibility();

            //Cargar posici�n inicial del escenario
            bspMap.CollisionManager.initCamera();
            bspMap.CollisionManager.Camera.RotationSpeed = 2f;


            //Acceso a metadata del escenario, por si se quiere obtener alguna informaci�n customizada (hay que parsearla)
            string entdata = bspMap.Data.entdata;
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio el nivel elegido
            string selectedFileName = (string)GuiController.Instance.Modifiers["Level"];
            if (selectedFileName != currentLevelFile)
            {
                currentLevelFile = selectedFileName;
                loadLevel(currentLevelFile);
            }


            //Actualizar valores de Modifiers
            bspMap.CollisionManager.Camera.MovementSpeed = (float)GuiController.Instance.Modifiers.getValue("Speed");
            bspMap.CollisionManager.Gravity = (float)GuiController.Instance.Modifiers.getValue("Gravity");
            bspMap.CollisionManager.JumpSpeed = (float)GuiController.Instance.Modifiers.getValue("JumpSpeed");
            bspMap.CollisionManager.NoClip = (bool)GuiController.Instance.Modifiers.getValue("NoClip");


            //Actualizar estado de colsiones y renderizar con Frustum Culling utilizando matriz PVS
            Vector3 currentPosition = bspMap.CollisionManager.update();
            bspMap.render(currentPosition);

        }

        public override void close()
        {
            //Liberar lock de camara
            bspMap.CollisionManager.Camera.LockCam = false;

            //Liberar recursos del escenario
            bspMap.dispose();
        }

    }
}
