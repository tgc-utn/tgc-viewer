using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples
{
    /// <summary>
    /// Ejemplo Escenario4toPiso:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminaci�n - Lightmap
    ///     # Unidad 7 - T�cnicas de Optimizaci�n - Frustum Culling, Fuerza Bruta
    /// 
    /// Utiliza la herramienta TgcSceneLoader para cargar un escenario 3D
    /// similar al 4to piso de la sede Medrano de la facultad.
    /// Este modelo fue utilizado en el segundo Trabajo Pr�ctico de Gesti�n de Datos de 2009.
    /// El escenario no se encuentra optimizado (sin Frustum Culling ni Occlusion Culling) y 
    /// su performance puede variar de acuerdo a cada dispositivo gr�fico.
    /// Permite aplicar Frustum Culling con m�todo de Fuerza Bruta para mostrar el cambio de performance.
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class Escenario4toPiso : TgcExample
    {

        TgcScene tgcScene;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "4toPiso";
        }

        public override string getDescription()
        {
            return "Utiliza la herramienta TgcSceneLoader para cargar el 4to Piso de la facultado modelado en formato TGC. " +
                "Muestra como utilizar FrustumCulling con fuerza bruta para acelerar la performance de renderizado.";
        }

        public override void init()
        {
            //FPS Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-140f, 40f, -50f), new Vector3(-140f,40f,-120f));
            GuiController.Instance.FpsCamera.MovementSpeed = 200f;
            GuiController.Instance.FpsCamera.JumpSpeed = 200f;


            
            //Cargar escena desde archivo ZIP
            TgcSceneLoader loader = new TgcSceneLoader();
            tgcScene = loader.loadSceneFromZipFile(
                "4toPiso-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "4toPiso\\4toPiso.zip",
                GuiController.Instance.ExamplesMediaDir + "4toPiso\\Extract\\" );

            /*
            //Version para cargar escena desde carpeta descomprimida
            TgcSceneLoader loader = new TgcSceneLoader();
            tgcScene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "4toPiso\\Extract\\4toPiso-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "4toPiso\\Extract\\");
            */


            //Modifier para habilitar o deshabilitar FrustumCulling
            GuiController.Instance.Modifiers.addBoolean("culling", "Frustum culling", false);

            //UserVar para contar la cantidad de meshes que se renderizan
            GuiController.Instance.UserVars.addVar("Meshes renderizadas");

        }


        public override void render(float elapsedTime)
        {
            bool frustumCullingEnabled = (bool)GuiController.Instance.Modifiers["culling"];

            //Renderizar sin ninguna optimizacion
            if (!frustumCullingEnabled)
            {
                tgcScene.renderAll();
                GuiController.Instance.UserVars.setValue("Meshes renderizadas", tgcScene.Meshes.Count);
            }

            //Renderizar con Frustum Culling
            else
            {
                //Analizar cada malla contra el Frustum - con fuerza bruta
                TgcFrustum frustum = GuiController.Instance.Frustum;
                int totalMeshes = 0;
                foreach (TgcMesh mesh in tgcScene.Meshes)
                {
                    //Nos ocupamos solo de las mallas habilitadas
                    if (mesh.Enabled)
                    {
                        //Solo mostrar la malla si colisiona contra el Frustum
                        TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, mesh.BoundingBox);
                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            mesh.render();
                            totalMeshes++;
                        }
                    }
                }
                //Actualizar cantidad de meshes dibujadas
                GuiController.Instance.UserVars.setValue("Meshes renderizadas", totalMeshes);
            }
            
        }

        public override void close()
        {
            tgcScene.disposeAll();
        }

    }
}
