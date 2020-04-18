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

namespace TGC.Examples.ShadersExamples
{
    public class PBRConIBL : TGCExampleViewer
    {
        private List<TgcMesh> meshes;
        private TgcMesh pbrMesh;
        private Effect effect;
        private CubeTexture cubeMap, irradianceMap, prefilterMap;
        private TGCVector3 reflectionProbePosition = new TGCVector3(0f, 100f, 0f);
        private List<Surface> depthStencils = new List<Surface>();
        private List<PBRTexturedMesh> pbrTexturedMeshes = new List<PBRTexturedMesh>();
        private List<PBRMesh> pbrMeshes = new List<PBRMesh>();
        private Texture bdrfLUT;

        private List<Light> lights;
        private List<TGCBox> lightBoxes;
        private TgcMesh unitCube;
        private VertexBuffer fullQuadVertexBuffer;

        private bool save = false;
        private bool firstTime = true;

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

        private struct PBRTexturedMesh
        {
            private TgcMesh mesh;
            private TGCMatrix transform;
            private TgcTexture albedo, metallic, normal, roughness;

            public PBRTexturedMesh(string name, string texturePath, TGCMatrix transform, TgcMesh mesh)
            {
                var basePBR = texturePath + "PBR\\" + name + "\\";
                var device = D3DDevice.Instance.Device;

                albedo = TgcTexture.createTexture(device, basePBR + "Color.jpg");
                normal = TgcTexture.createTexture(device, basePBR + "Normal.jpg");
                if (File.Exists(basePBR + "Metalness.jpg"))
                    metallic = TgcTexture.createTexture(device, basePBR + "Metalness.jpg");
                else
                    metallic = TgcTexture.createTexture(device, texturePath + "green.bmp");
                roughness = TgcTexture.createTexture(device, basePBR + "Roughness.jpg");

                this.mesh = mesh;
                this.transform = transform;
            }

            public void Apply()
            {
                mesh.Transform = transform;
                var meshEffect = mesh.Effect;

                meshEffect.SetValue("textured", true);
                meshEffect.SetValue("albedoTexture", albedo.D3dTexture);
                meshEffect.SetValue("metallicTexture", metallic.D3dTexture);
                meshEffect.SetValue("normalTexture", normal.D3dTexture);
                meshEffect.SetValue("roughnessTexture", roughness.D3dTexture);

                mesh.Render();
            }
        }

        private struct PBRMesh
        {
            private TgcMesh mesh;
            private TGCMatrix transform;
            private TGCVector3 albedo;
            private float metallic, roughness;

            public PBRMesh(TGCVector3 albedo, float metallic, float roughness, TGCMatrix transform, TgcMesh mesh)
            {
                this.albedo = albedo;
                this.metallic = metallic;
                this.roughness = roughness;
                this.mesh = mesh;
                this.transform = transform;
            }

            public void Apply()
            {
                mesh.Transform = transform;
                var meshEffect = mesh.Effect;

                meshEffect.SetValue("textured", false);
                meshEffect.SetValue("albedoValue", TGCVector3.TGCVector3ToFloat3Array(albedo));
                meshEffect.SetValue("metallicValue", metallic);
                meshEffect.SetValue("roughnessValue", roughness);

                mesh.Render();
            }
        }

        public PBRConIBL(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "PBR";
            Name = "PBR con IBL";
            Description = "Ejemplo de Physically Based Rendering con Image Based Lighting";
        }

        public override void Init()
        {
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "PBRIBL.fx");

            InitializeScene();
            InitializePBRMesh();
            InitializeLights();
            InitializeLightBoxes();
            InitializeUnitCube();
            InitializeTextures();
            InitializeFullScreenQuad();

            Camera = new TgcFpsCamera(new TGCVector3(250, 160, -570), Input);
        }

        private float time = 0.0f;

        public override void Update()
        {
            PreUpdate();
            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));
            effect.SetValue("time", time);
            time += ElapsedTime;
            PostUpdate();
        }

        public override void Render()
        {
            var deviceInstance = D3DDevice.Instance;
            var device = deviceInstance.Device;

            if (firstTime)
            {
                var oldRenderTarget = device.GetRenderTarget(0);
                var oldDepth = device.DepthStencilSurface;

                RenderCubeMap(device);

                device.DepthStencilSurface = oldDepth;

                RenderIrradianceMap(device);

                RenderPrefilterMap(device);

                RenderBDRFLut(device);

                if (save)
                    save = false;

                device.Transform.View = Camera.GetViewMatrix();
                device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), deviceInstance.AspectRatio, 1f, 2000f).ToMatrix();

                device.SetRenderTarget(0, oldRenderTarget);
                device.DepthStencilSurface = oldDepth;

                firstTime = false;
            }
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();

            RenderBaseScene();

            effect.SetValue("irradianceMap", irradianceMap);
            effect.SetValue("prefilterMap", prefilterMap);
            effect.SetValue("brdfLut", bdrfLUT);

            pbrMeshes.ForEach(mesh => mesh.Apply());

            pbrTexturedMeshes.ForEach(mesh => mesh.Apply());

            lightBoxes.ForEach(lightBox => lightBox.Render());

            RenderAxis();
            RenderFPS();
            device.EndScene();
            device.Present();
        }

        private void RenderBDRFLut(Device device)
        {
            D3DDevice.Instance.Device.SetRenderTarget(0, bdrfLUT.GetSurfaceLevel(0));

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            effect.Technique = "BDRFLUT";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, fullQuadVertexBuffer, 0);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            device.EndScene();

            if (save)
                TextureLoader.Save(ShadersDir + "brdflut.bmp", ImageFileFormat.Bmp, bdrfLUT);
        }

        private void RenderPrefilterMap(Device device)
        {
            device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, 0.01f, 5f).ToMatrix();

            unitCube.Technique = "Prefilter";
            effect.SetValue("cubeMap", cubeMap);
            int mipLevels = 5;
            for (int lod = 0; lod < mipLevels; lod++)
            {
                float roughness = (float)lod / (float)(mipLevels - 1);
                effect.SetValue("passRoughness", roughness);
                RenderToPrefilterMapFace(CubeMapFace.PositiveX, lod);
                RenderToPrefilterMapFace(CubeMapFace.PositiveY, lod);
                RenderToPrefilterMapFace(CubeMapFace.PositiveZ, lod);
                RenderToPrefilterMapFace(CubeMapFace.NegativeX, lod);
                RenderToPrefilterMapFace(CubeMapFace.NegativeY, lod);
                RenderToPrefilterMapFace(CubeMapFace.NegativeZ, lod);
            }
        }

        private void RenderIrradianceMap(Device device)
        {
            device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, 0.01f, 5f).ToMatrix();

            unitCube.Technique = "Irradiance";
            effect.SetValue("cubeMap", cubeMap);
            RenderToIrradianceCubeMapFace(CubeMapFace.PositiveX);
            RenderToIrradianceCubeMapFace(CubeMapFace.PositiveY);
            RenderToIrradianceCubeMapFace(CubeMapFace.PositiveZ);
            RenderToIrradianceCubeMapFace(CubeMapFace.NegativeX);
            RenderToIrradianceCubeMapFace(CubeMapFace.NegativeY);
            RenderToIrradianceCubeMapFace(CubeMapFace.NegativeZ);
        }

        private void RenderCubeMap(Device device)
        {
            device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, 1f, 2000f).ToMatrix();

            RenderToCubeMapFace(CubeMapFace.PositiveX, reflectionProbePosition);
            RenderToCubeMapFace(CubeMapFace.PositiveY, reflectionProbePosition);
            RenderToCubeMapFace(CubeMapFace.PositiveZ, reflectionProbePosition);
            RenderToCubeMapFace(CubeMapFace.NegativeX, reflectionProbePosition);
            RenderToCubeMapFace(CubeMapFace.NegativeY, reflectionProbePosition);
            RenderToCubeMapFace(CubeMapFace.NegativeZ, reflectionProbePosition);
        }

        private void RenderToPrefilterMapFace(CubeMapFace face, int lod)
        {
            var cubeMapFace = prefilterMap.GetCubeMapSurface(face, lod);
            D3DDevice.Instance.Device.SetRenderTarget(0, cubeMapFace);

            TGCMatrix lookAt = LookAtFromCubeMapFace(face, TGCVector3.Empty);

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.Transform.View = lookAt;

            D3DDevice.Instance.Device.BeginScene();

            unitCube.Render();

            D3DDevice.Instance.Device.EndScene();

            if (save)
                SurfaceLoader.Save(ShadersDir + "prefilter_" + lod + "_" + face.ToString() + ".bmp", ImageFileFormat.Bmp, cubeMapFace);
        }

        private void RenderToIrradianceCubeMapFace(CubeMapFace face)
        {
            var cubeMapFace = irradianceMap.GetCubeMapSurface(face, 0);
            D3DDevice.Instance.Device.SetRenderTarget(0, cubeMapFace);

            TGCMatrix lookAt = LookAtFromCubeMapFace(face, TGCVector3.Empty);

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.Transform.View = lookAt;

            D3DDevice.Instance.Device.BeginScene();

            unitCube.Render();

            D3DDevice.Instance.Device.EndScene();

            if (save)
            {
                SurfaceLoader.Save(ShadersDir + "irradiancemap_" + face.ToString() + ".bmp", ImageFileFormat.Bmp, cubeMapFace);
            }
        }

        private void RenderToCubeMapFace(CubeMapFace face, TGCVector3 position)
        {
            var cubeMapFace = cubeMap.GetCubeMapSurface(face, 0);
            D3DDevice.Instance.Device.SetRenderTarget(0, cubeMapFace);
            D3DDevice.Instance.Device.DepthStencilSurface = depthStencils[(int)face];

            TGCMatrix lookAt = LookAtFromCubeMapFace(face, position);

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.Transform.View = lookAt;

            D3DDevice.Instance.Device.BeginScene();

            RenderBaseScene();

            D3DDevice.Instance.Device.EndScene();

            if (save)
            {
                SurfaceLoader.Save(ShadersDir + "cubemap_" + face.ToString() + ".bmp", ImageFileFormat.Bmp, cubeMapFace);
            }
        }

        private Color ColorFromCubeMapFace(CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.PositiveX:
                    return Color.Red;

                case CubeMapFace.PositiveZ:
                    return Color.Blue;

                case CubeMapFace.PositiveY:
                    return Color.Green;

                case CubeMapFace.NegativeX:
                    return Color.Orange;

                case CubeMapFace.NegativeZ:
                    return Color.Cyan;

                case CubeMapFace.NegativeY:
                    return Color.Yellow;

                default:
                    throw new Exception("Invalid cubemap face");
            }
        }

        private TGCMatrix LookAtFromCubeMapFace(CubeMapFace face, TGCVector3 position)
        {
            switch (face)
            {
                case CubeMapFace.PositiveX:
                    return TGCMatrix.LookAtLH(position, position + new TGCVector3(1f, 0f, 0f), TGCVector3.Up);

                case CubeMapFace.PositiveZ:
                    return TGCMatrix.LookAtLH(position, position + new TGCVector3(0f, 0f, 1f), TGCVector3.Up);

                case CubeMapFace.PositiveY:
                    return TGCMatrix.LookAtLH(position, position + TGCVector3.Up, new TGCVector3(0f, 0f, -1f));

                case CubeMapFace.NegativeX:
                    return TGCMatrix.LookAtLH(position, position + new TGCVector3(-1f, 0f, 0f), TGCVector3.Up);

                case CubeMapFace.NegativeZ:
                    return TGCMatrix.LookAtLH(position, position + new TGCVector3(0f, 0f, -1f), TGCVector3.Up);

                case CubeMapFace.NegativeY:
                    return TGCMatrix.LookAtLH(position, position + -TGCVector3.Up, new TGCVector3(0f, 0f, 1f));

                default:
                    throw new Exception("Invalid cubemap face");
            }
        }

        private void RenderBaseScene()
        {
            meshes.ForEach(m => m.Render());
        }

        public override void Dispose()
        {
            meshes.ForEach(m => m.Dispose());
            pbrMesh.Dispose();
            effect.Dispose();
            cubeMap.Dispose();
            irradianceMap.Dispose();
            prefilterMap.Dispose();
            depthStencils.ForEach(depthStencil => depthStencil.Dispose());
            bdrfLUT.Dispose();
            lightBoxes.ForEach(lightBox => lightBox.Dispose());
            unitCube.Dispose();
            fullQuadVertexBuffer.Dispose();
        }

        private void InitializeScene()
        {
            // Cargamos la escena base
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;
            meshes.ForEach(m =>
            {
                m.Effect = effect;
                m.Technique = "Scene";
                m.Transform = TGCMatrix.Scaling(0.5f, 0.5f, 0.5f);
            });
        }

        private void InitializePBRMesh()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\white.bmp");

            var sphere = new TGCSphere();
            sphere.Radius = 40.0f;
            sphere.LevelOfDetail = 3;
            sphere.setTexture(texture);
            sphere.updateValues();

            pbrMesh = sphere.toMesh("sphere");
            pbrMesh.Effect = effect;
            pbrMesh.Technique = "PBRIBL";
            pbrMesh.DiffuseMaps = new TgcTexture[0];
            pbrMesh.RenderType = TgcMesh.MeshRenderType.VERTEX_COLOR;
            sphere.Dispose();

            var texturePath = MediaDir + "Texturas\\";

            var scaling = TGCMatrix.Scaling(TGCVector3.One * 25f);
            var harsh = new PBRTexturedMesh("Harsh-Metal", texturePath, scaling * TGCMatrix.Translation(0f, 100f, 100f), pbrMesh);
            var gold = new PBRTexturedMesh("Gold", texturePath, scaling * TGCMatrix.Translation(0f, 100f, 0f), pbrMesh);
            var marble = new PBRTexturedMesh("Marble", texturePath, scaling * TGCMatrix.Translation(0f, 100f, -100f), pbrMesh);
            var ground = new PBRTexturedMesh("Ground", texturePath, scaling * TGCMatrix.Translation(0f, 100f, -200f), pbrMesh);
            var metal = new PBRTexturedMesh("Metal", texturePath, scaling * TGCMatrix.Translation(0f, 100f, 200f), pbrMesh);
            pbrTexturedMeshes.AddRange(new List<PBRTexturedMesh> { harsh, gold, marble, ground, metal });

            var greenSomething = new PBRMesh(new TGCVector3(0.05f, 0.74f, 0.3f), 0.5f, 0.5f, scaling * TGCMatrix.Translation(0f, 100f, 400f), pbrMesh);
            var redRubber = new PBRMesh(new TGCVector3(1f, 0.05f, 0.1f), 0.1f, 0.9f, scaling * TGCMatrix.Translation(0f, 100f, 300f), pbrMesh);
            var blueMetal = new PBRMesh(new TGCVector3(0f, 0.2f, 1.0f), 0.75f, 0.3f, scaling * TGCMatrix.Translation(0f, 100f, -300f), pbrMesh);
            pbrMeshes.AddRange(new List<PBRMesh> { redRubber, blueMetal, greenSomething });
        }

        private void InitializeLights()
        {
            lights = new List<Light>();
            var lightOne = new Light();

            lightOne.Position = new TGCVector3(100f, 100f, 0f);
            lightOne.Color = new TGCVector3(400f, 400f, 400f);

            var lightTwo = new Light();
            lightTwo.Position = new TGCVector3(-100f, 100f, 0f);
            lightTwo.Color = new TGCVector3(150f, 60f, 150f);

            var lightThree = new Light();
            lightThree.Position = new TGCVector3(0f, 100f, 200f);
            lightThree.Color = new TGCVector3(150f, 150f, 20f);

            var lightFour = new Light();
            lightFour.Position = new TGCVector3(0f, 100f, -200f);
            lightFour.Color = new TGCVector3(25f, 150f, 150f);

            lights.Add(lightOne);
            lights.Add(lightTwo);
            lights.Add(lightThree);
            lights.Add(lightFour);

            var index = 0;
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

        private void InitializeUnitCube()
        {
            var box = new TGCBox();
            box.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\white.bmp"));
            box.Size = TGCVector3.One;
            box.updateValues();

            unitCube = box.ToMesh("cube");
            unitCube.Transform = TGCMatrix.Scaling(1f, 1f, 1f);
            unitCube.Effect = effect;
            unitCube.DiffuseMaps = new TgcTexture[0];
            unitCube.RenderType = TgcMesh.MeshRenderType.VERTEX_COLOR;
            box.Dispose();
        }

        private void InitializeTextures()
        {
            var device = D3DDevice.Instance.Device;

            cubeMap = new CubeTexture(device, 512, 0, Usage.RenderTarget | Usage.AutoGenerateMipMap, Format.A16B16G16R16F, Pool.Default);

            for (int stencilForSides = 0; stencilForSides < 6; stencilForSides++)
                depthStencils.Add(device.CreateDepthStencilSurface(512, 512, DepthFormat.D24S8, MultiSampleType.None, 0, true));

            irradianceMap = new CubeTexture(device, 32, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            prefilterMap = new CubeTexture(device, 128, 0, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            bdrfLUT = new Texture(device, 512, 512, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
        }

        private void InitializeFullScreenQuad()
        {
            var device = D3DDevice.Instance.Device;

            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1f, 1f, 0f, 0f, 1f),
                new CustomVertex.PositionTextured(-1f, -1f, 0f, 0f, 0f),
                new CustomVertex.PositionTextured(1f, 1f, 0f, 1f, 1f),
                new CustomVertex.PositionTextured(1, -1, 0f, 1f, 0f)
            };
            //vertex buffer de los triangulos
            fullQuadVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            fullQuadVertexBuffer.SetData(vertices, 0, LockFlags.None);
        }
    }
}