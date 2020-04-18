using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    public class PBR : TGCExampleViewer
    {
        public enum Material
        {
            GRASS,
            RUSTED_METAL,
            GOLD,
            MARBLE,
            METAL
        }

        private Material current;
        private TGCEnumModifier materials;

        private TgcMesh sphereMesh;
        private Effect effect;
        private List<Light> lights;
        private List<TGCBox> lightBoxes;
        private List<TGCTextureAutoUpdateModifier> textures;
        private TGCTextureAutoUpdateModifier albedo, ao, metalness, roughness, normals;

        private struct Light
        {
            public TGCVector3 Position;
            public TGCVector3 Color;

            public void SetLight(int index, Effect effect)
            {
                effect.SetValue("lights[" + index + "].Position", TGCVector3.TGCVector3ToFloat3Array(Position));
                effect.SetValue("lights[" + index + "].Color", TGCVector3.TGCVector3ToFloat3Array(Color));
            }
        }

        public PBR(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "PBR";
            Name = "PBR basic example";
            Description = "Ejemplo de PBR regular";
        }

        public override void Init()
        {
            InitializeLights();
            InitializeEffect();
            InitializeTextures();
            InitializeSphere();
            InitializeLightBoxes();
            InitializeMaterials();

            Camera = new TgcRotationalCamera(TGCVector3.Empty, 80f, Input);
        }

        public override void Update()
        {
            PreUpdate();

            UpdateMaterial();

            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));
            textures.ForEach(texture => texture.Update());

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            sphereMesh.Render();
            lightBoxes.ForEach(lightBox => lightBox.Render());
            PostRender();
        }

        public override void Dispose()
        {
            effect.Dispose();
            textures.ForEach(t => t.Dispose());
            lightBoxes.ForEach(l => l.Dispose());
            sphereMesh.Dispose();
        }

        private void InitializeSphere()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\white.bmp");

            var sphere = new TGCSphere();
            sphere.Radius = 40.0f;
            sphere.LevelOfDetail = 3;
            sphere.setTexture(texture);
            sphere.updateValues();

            sphereMesh = sphere.toMesh("sphere");
            sphereMesh.Transform = TGCMatrix.Scaling(TGCVector3.One * 30f);
            sphereMesh.Effect = effect;
            sphereMesh.Technique = "PBR";
            sphereMesh.DiffuseMaps = new TgcTexture[0];
            sphereMesh.RenderType = TgcMesh.MeshRenderType.VERTEX_COLOR;
            sphere.Dispose();
        }

        private void InitializeEffect()
        {
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "PBR.fx");

            int index = 0;
            lights.ForEach(light =>
            {
                light.SetLight(index, effect);
                index++;
            });
        }

        private void InitializeLightBoxes()
        {
            lightBoxes = new List<TGCBox>();

            var lightBoxOne = new TGCBox();
            lightBoxOne.Color = Color.White;
            lightBoxOne.Size = TGCVector3.One * 10.0f;
            lightBoxOne.Transform = TGCMatrix.Translation(lights[0].Position);
            lightBoxOne.updateValues();

            var lightBoxTwo = new TGCBox();
            lightBoxTwo.Color = Color.Purple;
            lightBoxTwo.Size = TGCVector3.One * 10.0f;
            lightBoxTwo.Transform = TGCMatrix.Translation(lights[1].Position);
            lightBoxTwo.updateValues();

            var lightBoxThree = new TGCBox();
            lightBoxThree.Color = Color.Yellow;
            lightBoxThree.Size = TGCVector3.One * 10.0f;
            lightBoxThree.Transform = TGCMatrix.Translation(lights[2].Position);
            lightBoxThree.updateValues();

            var lightBoxFour = new TGCBox();
            lightBoxFour.Color = Color.Cyan;
            lightBoxFour.Size = TGCVector3.One * 10.0f;
            lightBoxFour.Transform = TGCMatrix.Translation(lights[3].Position);
            lightBoxFour.updateValues();

            lightBoxes.Add(lightBoxOne);
            lightBoxes.Add(lightBoxTwo);
            lightBoxes.Add(lightBoxThree);
            lightBoxes.Add(lightBoxFour);
        }

        private void InitializeLights()
        {
            lights = new List<Light>();
            var lightOne = new Light();

            float distance = 45f;

            lightOne.Position = TGCVector3.One * distance;
            lightOne.Color = new TGCVector3(200f, 200f, 200f);

            var lightTwo = new Light();
            lightTwo.Position = TGCVector3.One * -distance;
            lightTwo.Color = new TGCVector3(100f, 30f, 100f);

            var lightThree = new Light();
            lightThree.Position = TGCVector3.One * distance - new TGCVector3(2f * distance, 0f, 0f);
            lightThree.Color = new TGCVector3(100f, 100f, 0f);

            var lightFour = new Light();
            lightFour.Position = TGCVector3.One * -distance + new TGCVector3(2f * distance, 0f, 0f);
            lightFour.Color = new TGCVector3(0f, 100f, 100f);

            lights.Add(lightOne);
            lights.Add(lightTwo);
            lights.Add(lightThree);
            lights.Add(lightFour);
        }

        private void InitializeTextures()
        {
            var defaultTexturePath = MediaDir + "Texturas\\PBR\\Harsh-Metal\\";

            textures = new List<TGCTextureAutoUpdateModifier>();

            normals = new TGCTextureAutoUpdateModifier("normalTexture");
            normals.OnTextureChange += OnTextureChange;

            ao = new TGCTextureAutoUpdateModifier("aoTexture");
            ao.OnTextureChange += OnTextureChange;

            metalness = new TGCTextureAutoUpdateModifier("metallicTexture");
            metalness.OnTextureChange += OnTextureChange;

            roughness = new TGCTextureAutoUpdateModifier("roughnessTexture");
            roughness.OnTextureChange += OnTextureChange;

            albedo = new TGCTextureAutoUpdateModifier("albedoTexture");
            albedo.OnTextureChange += OnTextureChange;

            textures.Add(normals);
            textures.Add(ao);
            textures.Add(metalness);
            textures.Add(roughness);
            textures.Add(albedo);
        }

        private void OnTextureChange(object sender, EventArgs e)
        {
            var modifier = (TGCTextureAutoUpdateModifier)sender;
            effect.SetValue(modifier.Name, modifier.Texture.D3dTexture);
        }

        private void InitializeMaterials()
        {
            materials = AddEnum("Material", typeof(Material), Material.RUSTED_METAL);
        }

        private void UpdateMaterial()
        {
            var value = (Material)materials.Value;
            if (!value.Equals(current))
            {
                var defaultTexturePath = MediaDir + "Texturas\\PBR\\";
                current = value;
                switch (current)
                {
                    case Material.RUSTED_METAL:
                        defaultTexturePath += "Harsh-Metal";
                        break;

                    case Material.MARBLE:
                        defaultTexturePath += "Marble";
                        break;

                    case Material.GOLD:
                        defaultTexturePath += "Gold";
                        break;

                    case Material.METAL:
                        defaultTexturePath += "Metal";
                        break;

                    case Material.GRASS:
                        defaultTexturePath += "Ground";
                        break;
                }
                defaultTexturePath += "\\";

                albedo.SetValue(defaultTexturePath + "Color.jpg");

                if (File.Exists(defaultTexturePath + "AmbientOcclusion.jpg"))
                    ao.SetValue(defaultTexturePath + "AmbientOcclusion.jpg");
                else
                    ao.SetValue(MediaDir + "Texturas\\white.bmp");

                if (File.Exists(defaultTexturePath + "Metalness.jpg"))
                    metalness.SetValue(defaultTexturePath + "Metalness.jpg");
                else
                    metalness.SetValue(MediaDir + "Texturas\\green.bmp");

                normals.SetValue(defaultTexturePath + "Normal.jpg");
                roughness.SetValue(defaultTexturePath + "Roughness.jpg");
            }
        }
    }

    // Helper for texture changes, unrelated with the concept
    public class TGCTextureAutoUpdateModifier
    {
        public event EventHandler OnTextureChange;

        private TgcTexture texture;
        private string name, path, setPath;

        public TGCTextureAutoUpdateModifier(string name)
        {
            this.name = name;
            path = "";
            setPath = "";
        }

        public TgcTexture Texture { get => texture; }

        public string Name { get => name; }

        public void Update()
        {
            if (!setPath.Equals(path))
            {
                path = setPath;
                ChangeTexture();
            }
        }

        public void SetValue(string newPath)
        {
            setPath = newPath;
        }

        private void ChangeTexture()
        {
            if (texture != null)
                texture.dispose();
            texture = TgcTexture.createTexture(D3DDevice.Instance.Device, path);
            OnTextureChange.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            texture.dispose();
        }
    }
}