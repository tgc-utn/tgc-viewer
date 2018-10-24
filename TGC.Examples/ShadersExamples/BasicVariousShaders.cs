using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplos varios de pixel y vertex shaders:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Autor: Ronan Vinitzca
    /// </summary>
    public class BasicVariousShaders : TGCExampleViewer
    {
        private float time;
        private TgcMesh mesh;
        private Effect effect;

        private TgcArrow effectVectorArrow;

        private TGCMatrix scale;

        private TGCFloatModifier rotation;
        private TGCFloatModifier factor;
        private TGCVertex3fModifier effectVector;
        private TGCVertex3fModifier center;
        private TGCEnumModifier techniques;
        private TGCBooleanModifier wireframe;
        private TGCBooleanModifier showArrow;

        private enum Techniques
        {
            RenderMesh,
            Expansion,
            Extrude,
            IdentityPlaneExtrude,
            PlanarExtrude,
            TextureCycling,
            ColorCycling,
            ExtrudeCombined,
            Phong,
        }

        public BasicVariousShaders(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "Varios Shaders Basicos";
            Description = "Ejemplos de Shaders Basicos en una esfera";
        }

        public override void Init()
        {
            this.time = 0;

            this.techniques = AddEnum("Tecnica", typeof(Techniques), Techniques.RenderMesh);
            this.rotation = AddFloat("Rotacion", 0, FastMath.TWO_PI, 0);
            this.factor = AddFloat("Factor", 0, 1, 0.5f);
            this.effectVector = AddVertex3f("Vector", new TGCVector3(-20, -20, -20), new TGCVector3(20, 20, 20), TGCVector3.Empty);
            this.center = AddVertex3f("Centro", new TGCVector3(-10, -10, -10), new TGCVector3(10, 10, 10), TGCVector3.Empty);
            this.wireframe = AddBoolean("Wireframe", "Prende el efecto wireframe", false);
            this.showArrow = AddBoolean("Vector del Efecto", "Muestra el vector del efecto", false);

            this.CreateEffect();
            this.CreateMesh();
            this.CreateArrow();

            this.scale = TGCMatrix.Scaling(20, 20, 20);

            var cameraPosition = new TGCVector3(0, 0, 125);
            var lookAt = TGCVector3.Empty;
            Camara.SetCamera(cameraPosition, lookAt);
        }

        private void CreateEffect()
        {
            this.effect = TGCShaders.Instance.LoadEffect(ShadersDir + "Varios.fx");
        }

        private void CreateMesh()
        {
            var lava = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "\\Texturas\\lava.jpg");
            var sphere = new TGCSphere(1, lava, TGCVector3.Empty);
            sphere.LevelOfDetail = 4;
            sphere.updateValues();

            this.mesh = sphere.toMesh("Mesh");

            this.mesh.Effect = this.effect;
        }

        private void CreateArrow()
        {
            this.effectVectorArrow = new TgcArrow();
            effectVectorArrow.HeadColor = Color.Blue;
            effectVectorArrow.BodyColor = Color.Red;
            effectVectorArrow.Thickness = 1;
            effectVectorArrow.Enabled = false;
            effectVectorArrow.HeadSize = new TGCVector2(1, 1);
            effectVectorArrow.updateValues();
        }

        public override void Update()
        {
            this.PreUpdate();

            // Manejamos la entrada
            this.HandleInput();

            // Actualizamos nuestro tiempo y se lo enviamos al shader
            this.time += ElapsedTime;
            this.effect.SetValue("time", time);

            // Le enviamos al shader la matriz ViewProj
            this.effect.SetValue("matViewProj", D3DDevice.Instance.Device.Transform.View * D3DDevice.Instance.Device.Transform.Projection);

            // Le enviamos al shader la posicion de la camara
            this.effect.SetValue("eyePosition", new[] { this.Camara.Position.X, this.Camara.Position.Y, this.Camara.Position.Z });

            // Actualizamos los modificadores
            this.rotation.Update();
            this.factor.Update();
            this.effectVector.Update();
            this.center.Update();
            this.techniques.Update();
            this.wireframe.Update();
            this.showArrow.Update();

            // Hacemos uso de los valores de los modificadores actualizados
            // (Excepto el Wireframe, eso se usa en el Render)
            this.mesh.Transform = this.scale * TGCMatrix.RotationYawPitchRoll(this.rotation.Value, 0, 0) * TGCMatrix.Translation(this.center.Value);
            this.effect.SetValue("factor", this.factor.Value);
            TGCVector3 realVector = this.center.Value + this.effectVector.Value;
            this.effect.SetValue("effectVector", TGCVector3.Vector3ToFloat4Array(this.effectVector.Value));
            this.effect.SetValue("center", TGCVector3.Vector3ToFloat4Array(this.center.Value));

            this.mesh.Technique = ((Techniques)this.techniques.Value).ToString();
            this.effectVectorArrow.Enabled = this.showArrow.Value;
            this.effectVectorArrow.PStart = this.center.Value;
            this.effectVectorArrow.PEnd = realVector;
            this.effectVectorArrow.updateValues();

            this.PostUpdate();
        }

        private void HandleInput()
        {
            if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                Camara.SetCamera(Camara.Position + new TGCVector3(0, 10f, 0), Camara.LookAt);
                if (Camara.Position.Y > 300f)
                {
                    Camara.SetCamera(new TGCVector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
                }
            }
        }

        public override void Render()
        {
            PreRender();

            if (this.wireframe.Value)
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;
                D3DDevice.Instance.Device.RenderState.Lighting = false;
            }
            this.mesh.Render();
            if (this.wireframe.Value)
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
                D3DDevice.Instance.Device.RenderState.Lighting = true;
            }

            this.effectVectorArrow.Render();

            PostRender();
        }

        public override void Dispose()
        {
            this.effect.Dispose();
            this.effectVectorArrow.Dispose();
            this.mesh.Dispose();

            this.rotation.Dispose();
            this.factor.Dispose();
            this.effectVector.Dispose();
            this.center.Dispose();
            this.techniques.Dispose();
            this.wireframe.Dispose();
            this.showArrow.Dispose();
        }
    }
}