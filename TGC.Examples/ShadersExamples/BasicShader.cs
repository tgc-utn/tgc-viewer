using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EnvMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Es el hola mundo de los shaders
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class BasicShader : TgcExample
    {
        private Effect effect;
        private TgcMesh mesh;
        private TgcScene scene;
        private float time;

        public BasicShader(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Workshop-BasicShader";
            Description = "Ejemplo de Shader Basico";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar los mesh:
            scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new Vector3(0f, 0f, 0f);

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "WorkshopShaders\\BasicShader.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;

            // indico que tecnica voy a usar
            // Hay effectos que estan organizados con mas de una tecnica.
            mesh.Technique = "RenderScene";

            //Centrar camara rotacional respecto a este mesh

            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);

            time = 0;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            time += ElapsedTime;

            ((TgcRotationalCamera)Camara).targetObject(mesh.BoundingBox);
            Camara.updateCamera(ElapsedTime);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);

            // dibujo la malla pp dicha
            mesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            effect.Dispose();
            scene.disposeAll();
        }
    }
}