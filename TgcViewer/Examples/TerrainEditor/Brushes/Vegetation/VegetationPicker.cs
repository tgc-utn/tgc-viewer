using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;
using System.Drawing;
using System;

namespace Examples.TerrainEditor.Brushes.Vegetation
{
    public class VegetationPicker:ITerrainEditorBrush
    {
      
        protected Font font;

        protected TgcMesh Mesh { get; set; }         

    
    
        public bool Enabled { get; set; }
        public Vector3 Position { get; set; }
        protected TgcText2d label;
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
        public VegetationPicker()
        {
            label = new TgcText2d();
            label.Color = Color.Yellow;
            label.Align = TgcText2d.TextAlign.LEFT;
            font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            label.changeFont(font);

            Rotation = RotationAxis.Y;
            SoundEnabled = true;
            sound = new TgcStaticSound();
            sound.loadSound(GuiController.Instance.ExamplesMediaDir + "Sound\\pisada arena dcha.wav", -2000);
                
        }
        #region ITerrainEditorBrush

        public virtual bool mouseMove(TgcTerrainEditor editor)
        {
            Vector3 pos;
            Enabled = true;
            if (Mesh != null)
            {
                if (editor.mousePositionInTerrain(out pos))
                {
                    Position = pos;
                    Mesh.Position = pos;

                    setLabelPosition();
                }
            }
           
            return false;
        }

        private void setLabelPosition()
        {
            
            label.Text = String.Format("\"{0}\"( {1} ; {2} ; {3} )", Mesh.Name, Mesh.Position.X, Mesh.Position.Y, Mesh.Position.Z);
            SizeF nameSize;
            using (Graphics g = GuiController.Instance.Panel3d.CreateGraphics())
            {
                nameSize = g.MeasureString(label.Text, font);
            }

            label.Size = nameSize.ToSize();
            label.Position = new Point((int)(GuiController.Instance.D3dInput.Xpos - nameSize.Width / 2), (int)(GuiController.Instance.D3dInput.Ypos + nameSize.Height + 5));

        }

        public bool mouseLeave(TgcTerrainEditor editor)
        {
            this.Enabled = false;
            return false;
        }

        public virtual bool update(TgcTerrainEditor editor)
        {
            bool changes = false;
            if (Enabled)
            {

                if (Mesh != null) updateMeshScaleAndRotation();

                if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    if (Mesh != null)
                    {
                        addVegetation(editor);
                        changes = true;
                    }
                    else
                    {
                        Mesh = pickVegetation(editor);
                        if (Mesh != null)
                        {
                            changes = true;
                            setLabelPosition();

                        }
                    }
                }

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Delete))
                {
                    removeFloatingVegetation();
                }

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Z))
                {
                    if (editor.HasVegetation)
                    {
                        removeFloatingVegetation();
                        Mesh = editor.vegetationPop();
                    }
                }
            }

            return changes;
        }
        
        public void render(TgcTerrainEditor editor)
        {
            editor.doRender();
            if (Enabled) renderFloatingVegetation();
        }


        public void dispose()
        {
            removeFloatingVegetation();
            label.dispose();
            sound.dispose();

        }

        #endregion



        private TgcMesh pickVegetation(TgcTerrainEditor editor)
        {
            TgcPickingRay ray = new TgcPickingRay();
            ray.updateRay();
            float minT = -1;
            TgcMesh closerMesh = null;
            foreach (TgcMesh v in editor.Vegetation)
            {

                Vector3 q;
                if (TgcCollisionUtils.intersectRayAABB(ray.Ray, v.BoundingBox, out q))
                {
                    float t = 0;
                    if (q != ray.Ray.Origin)
                    {
                        if (ray.Ray.Direction.X != 0) t = (q.X - ray.Ray.Origin.X) / ray.Ray.Direction.X;
                        else if (ray.Ray.Direction.Y != 0) t = (q.Y - ray.Ray.Origin.Y) / ray.Ray.Direction.Y;
                        else if (ray.Ray.Direction.Z != 0) t = (q.Z - ray.Ray.Origin.Z) / ray.Ray.Direction.Z;
                    }

                    if (minT == -1 || t < minT)
                    {
                        minT = t;
                        closerMesh = v;
                    }

                }

            }

            if (closerMesh != null) editor.removeVegetation(closerMesh);

            return closerMesh;
        }

        private void updateMeshScaleAndRotation()
        {

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
            {
                rotate(FastMath.ToRad(60 * GuiController.Instance.ElapsedTime));

            }
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightArrow))
            {
                rotate(FastMath.ToRad(-60 * GuiController.Instance.ElapsedTime));

            }
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow))
            {
                Mesh.Scale += ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

            }
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
            {
                Mesh.Scale -= ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

            }
        }

        private void rotate(float p)
        {
            switch (Rotation)
            {
                case RotationAxis.X:
                    // mesh.rotateX(p);
                    Mesh.Rotation = new Vector3((Mesh.Rotation.X + p) % FastMath.TWO_PI, Mesh.Rotation.Y, Mesh.Rotation.Z);

                    break;

                case RotationAxis.Y:
                    //mesh.rotateY(p);
                    Mesh.Rotation = new Vector3(Mesh.Rotation.X, (Mesh.Rotation.Y + p) % FastMath.TWO_PI, Mesh.Rotation.Z);
                    break;

                case RotationAxis.Z:
                    //mesh.rotateZ(p);
                    Mesh.Rotation = new Vector3(Mesh.Rotation.X, Mesh.Rotation.Y, (Mesh.Rotation.Z + p) % FastMath.TWO_PI);
                    break;
            }
        }

  
       

        protected virtual void addVegetation(TgcTerrainEditor editor)
        {

            editor.addVegetation(Mesh);
            if (SoundEnabled) playSound();
            Mesh = null;
          
        }

        public void playSound()
        {
            sound.play();
        } 

      
        private void renderFloatingVegetation()
        {
            if (Mesh != null)
            {
                Mesh.render();
                Mesh.BoundingBox.render();
                label.render();
            }
        }

    
        public void removeFloatingVegetation()
        {
            if (Mesh != null)
            {
                Mesh.dispose();
                Mesh = null;
            }
        }

    }
}
