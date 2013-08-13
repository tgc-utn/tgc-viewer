using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using Examples.TerrainEditor.Vegetation;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;
using Microsoft.DirectX.DirectSound;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.TerrainEditor.Brushes.Vegetation
{
    public class VegetationBrush:ITerrainEditorBrush
    {


        private TgcMesh mesh;
        public bool Enabled { get; set; }
        public Vector3 Position { get; set; }
        private string vegetationName;

        private Vector3 rotationAxis = new Vector3(0, 1, 0);
        public enum RotationAxis
        {
            X,
            Y,
            Z

        }
        private Vector3 scaleAxis  = new Vector3(1, 1, 1);
        public RotationAxis Rotation { get; set; }
        public Vector3 ScaleAxis { get { return scaleAxis; } set { scaleAxis = Vector3.Normalize(value); } }
        private TgcStaticSound sound;

        public bool SoundEnabled { get; set; }
        public VegetationBrush()
        {
            Rotation = RotationAxis.Y;
            SoundEnabled = true;
            sound = new TgcStaticSound();
            sound.loadSound(GuiController.Instance.ExamplesMediaDir + "Sound\\pisada arena dcha.wav", -2000);
                
        }
        #region ITerrainEditorBrush

        public bool mouseMove(TgcTerrainEditor editor)
        {
            Vector3 pos;
            if (editor.mousePositionInTerrain(out pos))
            {
                Enabled = true;
                Position = pos;
                if (mesh == null) mesh = InstancesManager.Instance.newMeshInstanceOf(vegetationName);
                mesh.Position = pos;
          

            }
            return false;
        }

        public bool mouseLeave(TgcTerrainEditor editor)
        {
            this.Enabled = false;
            return false;
        }

        public bool update(TgcTerrainEditor editor)
        {
            if (Enabled)
            {

                if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
                {
                     rotate(FastMath.ToRad(60*GuiController.Instance.ElapsedTime));

                }
                else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightArrow))
                {
                    rotate(FastMath.ToRad(-60 * GuiController.Instance.ElapsedTime));

                }else if(GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow))
                {
                    mesh.Scale += ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

                }else if(GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
                {
                    mesh.Scale -= ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

                }

                if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    addVegetation(editor);
                    return true;

                }

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Z))
                {
                    TgcMesh last = editor.vegetationPop();
                    if (last != null)
                    {
                        mesh.dispose();
                        mesh = last;
                        mesh.Position = Position;
                    }
                }

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Delete))
                {
                    removeFloatingVegetation();
                }

            }

            return false;
        }

        private void rotate(float p)
        {
            switch (Rotation)
            {
                case RotationAxis.X:
                   // mesh.rotateX(p);
                    mesh.Rotation = new Vector3((mesh.Rotation.X+p)%FastMath.TWO_PI, mesh.Rotation.Y, mesh.Rotation.Z);
      
                    break;

                case RotationAxis.Y:
                    //mesh.rotateY(p);
                    mesh.Rotation = new Vector3(mesh.Rotation.X, (mesh.Rotation.Y + p) % FastMath.TWO_PI, mesh.Rotation.Z);
                    break;

                case RotationAxis.Z:
                    //mesh.rotateZ(p);
                    mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y, (mesh.Rotation.Z + p) % FastMath.TWO_PI);
                    break;
            }
        }

        public void render(TgcTerrainEditor editor)
        {
            editor.doRender();
            if (Enabled) renderFloatingVegetation();
        }


        public void dispose()
        {
            removeFloatingVegetation();
            sound.dispose();

        }

        #endregion

       

        public void setVegetation(string name)
        {
            vegetationName = name;
            removeFloatingVegetation();
        }
     
      
       

        private void addVegetation(TgcTerrainEditor editor)
        {

            editor.addVegetation(mesh);
            TgcMesh copy = InstancesManager.Instance.newMeshInstanceOf(vegetationName);
            copy.Position = mesh.Position;
            copy.Scale = mesh.Scale;
            copy.Rotation = mesh.Rotation;
            mesh = copy;
            if(SoundEnabled)sound.play();
          
        }

      
        private void renderFloatingVegetation()
        {
           if(mesh!=null)mesh.render();
        }

    
        public void removeFloatingVegetation()
        {
            if (mesh != null)
            {
                mesh.dispose();
                mesh = null;
            }
        }

       
    }
}
