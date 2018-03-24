using System;
using System.IO;
using System.Windows.Forms;
using TGC.Core.SceneLoader;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Ejemplo EjemploLoadQ3Level
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimizaci�n - BSP y PVS
    ///     General:
    ///     --------
    ///     Muestra como cargar un escenario de Quake 3.
    ///     Herramienta en estado BETA. Su uso es responsabilidad del alumno.
    ///     Los escenarios cargados son de la demo del Quake 3.
    ///     Pueden obtenerse nuevos escenarios del juego completo y otros sitios.
    ///     Cargar un escenario grande puede tardar bastante.
    ///     No todas las funcionalidades del escenario son soportadas por la herramienta. Algunos objetos no poseen textura o
    ///     movimiento.
    ///     Pueden crearse escenarios propios con el editor de escenarios GtkRadiant.
    ///     Este ejemplo muestra como cargar un nivel customizado llamado "NivelPrueba2"
    ///     Especificaciones t�cnicas:
    ///     --------------------------
    ///     La clase BspLoader lee el formato de BSP de escenario de Quake 3 y lo convierte a una lista de TgcMesh, dentro
    ///     de la clase BspMap.
    ///     Las curvas (patches) son teseladas y convertidas a un TgcMesh.
    ///     El renderizado del escenario se hace con Frustum Culling utilizando la matriz PVS precalculada en el archivo BSP.
    ///     Se utilizan Lightmaps.
    ///     El editor GtkRadiant precalcula la matriz PVS, lightmaps y otras cosas.
    ///     La clase BspCollisionManager realiza un chequeo de colsiones con el escenario. Las colisiones soportadas son una
    ///     versi�n
    ///     simplificada de la totalidad de colisiones utilizadas en el juego. Quedan muchos aspectos por mejorar.
    ///     Quake 3 posee shaders en formato propio. BspLoader y Q3ShaderParser carga y parsea estos shaders propios de Quake 3
    ///     y los convierte
    ///     a un equivalente de shader en HLSL de DirectX. Este proceso es experimental, a�n est� incompleto y los shaders no
    ///     se
    ///     utilizan actualmente para renderizar, solo sus texturas asociadas. Es por eso que muchos escenarios poseen objetos
    ///     en blanco,
    ///     sin textura, o con Alpha Blending mal aplicado.
    ///     BspLoader imprime por pantalla todas las texturas o shaders que no se pudieron cargar.
    ///     Editor GtkRadiant
    ///     -----------------
    ///     Obtener GtkRadiant: http://www.qeradiant.com/cgi-bin/trac.cgi
    ///     Tutorial para uso de editor GtkRadiant: http://clankiller.com/games/quake/gtkradianttut/
    ///     No es posible editar un escenario ya compilado de Quake 3 con la herramienta GtkRadiant.
    ///     Si un escenario existente no cumple las expectativas del alumno, es preferible crear uno nuevo, en lugar de
    ///     intentar modificarlo.
    ///     Para cargar un escenario es necesario apuntar el mediaPath a la carpeta con todo el contenido gr�fico del juego
    ///     Quake original
    ///     (con carpetas textures, maps, models, scripts, sprites, etc.)
    ///     Esta carpeta puede obtenerse descomprimiendo el archivo ".pk3" del juego (renombrar a .zip).
    ///     El editor GtkRadiant tambi�n necesita todos estos archivos del juego.
    ///     La carpeta posee muchas mas texturas y modelos de los que probablemente use un mapa particular.
    ///     Para crear un carpeta solo con los m�nimos recursos necesarios, la clase BspLoader posee el m�todo "packLevel()".
    ///     Se utiliza de la siguiente manera:
    ///     BspLoader loader = new BspLoader();
    ///     BspMap bspMap = loader.loadBsp(mapFile, mediaPath);
    ///     loader.packLevel(bspMap, mediaPath, levelFolder);
    ///     Este m�todo solo debe llamarse una vez por mapa, en forma offline. Ver ejemplo EjemploEmpaquetarQ3Level.cs
    ///     Luego BspLoader puede cargar directamente la versi�n empaquetada.
    ///     Los escenarios entregados en el TP de la materia deben estar empaquetados de esta forma.
    ///     Los escenarios deben ser compilados en GtkRadiant con la opci�n "Build => Q3Map2 (final) BSP -meta, -vis, -light
    ///     -fast -filter -super 2 -bounce 8" (ante �ltima)
    ///     Los escenarios deben tener luces y la entidad "info_player_deathmatch".
    ///     Este ejemplo viene con un escenario customizado llamado "EjemploPrueba2.bsp". El mismo se adjunta con el archivo
    ///     ".map" para poder
    ///     ser abierto y estudiado con GtkRadiant.
    ///     Es recomendable evitar usar shaders y efectos raros en el editor.
    ///     El editor permite cargar metadata customizada del usuario sobre el escenario. La misma puede ser accedida en:
    ///     bspMap.Data.entdata;
    ///     Autor: Martin Giachetti, Mat�as Leone
    /// </summary>
    public class EjemploLoadQ3Level : TGCExampleViewer
    {
        private TGCFileModifier openFileModifier;
        private TGCFloatModifier speedModifier;
        private TGCFloatModifier gravityModifier;
        private TGCFloatModifier jumpSpeedModifier;
        private TGCBooleanModifier noClipModifier;
        private TGCButtonModifier exportButtonModifier;

        private BspMap bspMap;
        private string currentLevelFile;
        private string exampleDir;

        public EjemploLoadQ3Level(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            this.Category = "Quake3";
            this.Name = "Load BSP Level";
            this.Description = "Permite cargar escenarios de Quake 3 en formato BSP. Utiliza matriz PVS para renderizado y posee detecci�n de colisiones. Presionar L para capturar el mouse. Puede tardar unos minutos en cargar.";
        }

        public override void Init()
        {
            //Path de este ejemplo
            exampleDir = this.MediaDir + "Quake3Levels\\";

            //Cargar nivel inicial
            currentLevelFile = exampleDir + "q3dm1\\maps\\q3dm1.bsp";
            loadLevel(currentLevelFile);

            //Modifiers
            openFileModifier = AddFile("Level", currentLevelFile, ".Niveles Quake 3|*.bsp");
            speedModifier = AddFloat("Speed", 0, 500f, 350f);
            gravityModifier = AddFloat("Gravity", 0, 600, 180);
            jumpSpeedModifier = AddFloat("JumpSpeed", 60, 600, 100);
            noClipModifier = AddBoolean("NoClip", "NoClip", false);
            exportButtonModifier = AddButton("exportButton", "Exportar XML", Export_ButtonClick);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        /// <summary>
        ///     Evento de clic en Reload
        /// </summary>
        private void Export_ButtonClick(object sender, EventArgs e)
        {
            //saveSceneDialog
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".xml";
            saveDialog.Filter = ".XML |*.xml";
            saveDialog.AddExtension = true;

            saveDialog.Title = "Export Scene";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var fInfo = new FileInfo(saveDialog.FileName);
                var sceneName = fInfo.Name.Split('.')[0];
                var saveDir = fInfo.DirectoryName;

                var exportScene = new TgcScene(sceneName, saveDir);
                foreach (var m in bspMap.Meshes)
                {
                    exportScene.Meshes.Add(m);
                }
                var exporter = new TgcSceneExporter();
                exporter.exportSceneToXml(exportScene, saveDir);

                MessageBox.Show("Escena exportada a formato TGC satisfactoriamente.", "Export Scene",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        ///     Cargar un escenario de Quake 3
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
            var mediaPath = new DirectoryInfo(levelFile).Parent.Parent.FullName + "\\";
            var loader = new BspLoader();
            bspMap = loader.loadBsp(levelFile, mediaPath);

            //Iniciar visiblidad
            bspMap.initVisibility();

            //Cargar posici�n inicial del escenario
            bspMap.CollisionManager.initCamera();
            bspMap.CollisionManager.Camera.RotationSpeed = 2f;

            //Acceso a metadata del escenario, por si se quiere obtener alguna informaci�n customizada (hay que parsearla)
            var entdata = bspMap.Data.entdata;
        }

        public override void Render()
        {
            PreRender();

            //Ver si cambio el nivel elegido
            var selectedFileName = openFileModifier.Value;
            if (selectedFileName != currentLevelFile)
            {
                currentLevelFile = selectedFileName;
                loadLevel(currentLevelFile);
            }

            //Actualizar valores de Modifiers
            bspMap.CollisionManager.Camera.MovementSpeed = speedModifier.Value;
            bspMap.CollisionManager.Gravity = gravityModifier.Value;
            bspMap.CollisionManager.JumpSpeed = jumpSpeedModifier.Value;
            bspMap.CollisionManager.NoClip = noClipModifier.Value;

            //Actualizar estado de colsiones y renderizar con Frustum Culling utilizando matriz PVS
            var currentPosition = bspMap.CollisionManager.update(ElapsedTime, Input);
            bspMap.render(currentPosition, Frustum, ElapsedTime);

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar lock de camara
            bspMap.CollisionManager.Camera.LockCam = false;

            //Liberar recursos del escenario
            bspMap.dispose();

            bspMap = null;
        }
    }
}