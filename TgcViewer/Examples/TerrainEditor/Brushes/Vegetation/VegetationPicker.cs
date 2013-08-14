using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.TerrainEditor.Brushes.Vegetation
{
    public class VegetationPicker:ITerrainEditorBrush
    {
        protected TgcMesh mesh;
        public bool Enabled { get; set; }
        public Vector3 Position { get; set; }
       
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
            if (mesh != null)
            {
                if (editor.mousePositionInTerrain(out pos))
                {
                    Position = pos;
                    mesh.Position = pos;

                }
            }
           
            return false;
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

                if (mesh != null) updateMeshScaleAndRotation();

                if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    if (mesh != null)
                    {
                        addVegetation(editor);
                        changes = true;
                    }
                    else
                    {
                        mesh = pickVegetation(editor);
                        if (mesh != null) changes = true;
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
                        mesh = editor.vegetationPop();
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
                        if (ray.Ray.Direction.X != 0) minT = (q.X - ray.Ray.Origin.X) / ray.Ray.Direction.X;
                        else if (ray.Ray.Direction.Y != 0) minT = (q.Y - ray.Ray.Origin.Y) / ray.Ray.Direction.Y;
                        else if (ray.Ray.Direction.Z != 0) minT = (q.Z - ray.Ray.Origin.Z) / ray.Ray.Direction.Z;
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
                mesh.Scale += ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

            }
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
            {
                mesh.Scale -= ScaleAxis * 1.5f * GuiController.Instance.ElapsedTime;

            }
        }

        private void rotate(float p)
        {
            switch (Rotation)
            {
                case RotationAxis.X:
                    // mesh.rotateX(p);
                    mesh.Rotation = new Vector3((mesh.Rotation.X + p) % FastMath.TWO_PI, mesh.Rotation.Y, mesh.Rotation.Z);

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

  
       

        protected virtual void addVegetation(TgcTerrainEditor editor)
        {

            editor.addVegetation(mesh);
            if (SoundEnabled) playSound();
            mesh = null;
          
        }

        public void playSound()
        {
            sound.play();
        } 

      
        private void renderFloatingVegetation()
        {
            if (mesh != null)
            {
                mesh.render();
                mesh.BoundingBox.render();
            }
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
