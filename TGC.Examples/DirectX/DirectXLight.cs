using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo DirectXLight:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion Dinamica, Material, Gouraud Shading
    ///     Crea una tetera y una cara, en la cual puede modificarse varios parametros de iluminacion.
    ///     Autor: Leandro Barbagallo, Matias Leone
    /// </summary>
    public class DirectXLight : TGCExampleViewer
    {
        private TGCIntervalModifier selectedMeshModifier;
        private TGCIntervalModifier shaderModeModifier;
        private TGCBooleanModifier normalesModifier;
        private TGCFloatModifier specularSharpnessModifier;
        private TGCBooleanModifier specularEnabledModifier;
        private TGCColorModifier ambientModifier;
        private TGCColorModifier diffuseModifier;
        private TGCColorModifier specularModifier;
        private TGCBooleanModifier wireframeModifier;
        private TGCBooleanModifier backFaceCullModifier;
        private TGCFloatModifier angleXModifier;
        private TGCFloatModifier angleYModifier;
        private TGCFloatModifier angleZModifier;

        private readonly float lightDistance = 7;
        private float angleX;
        private float angleY;
        private float angleZ;
        private Mesh lightBulb;
        private TGCVector3 lightVectorToCenter;
        private CustomVertex.PositionColored[] lightVectorVB;
        private Material material;
        private Mesh SelectedMesh;
        private CustomVertex.PositionColored[] selectedNormalVB;
        private Mesh teapotMesh, faceMesh;
        private CustomVertex.PositionColored[] teapotMeshNormalsVB;

        public DirectXLight(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "DirectX";
            Name = "DirectX Light";
            Description = "Permite modificar varios parametros del modelo de iluminacion de DirectX.";
        }

        public override void Init()
        {
            createMeshes();

            //Crear Material
            material = new Material();

            //Crear una fuente de luz direccional en la posicion 0.
            D3DDevice.Instance.Device.Lights[0].Type = LightType.Directional;
            D3DDevice.Instance.Device.Lights[0].Diffuse = Color.White;
            D3DDevice.Instance.Device.Lights[0].Ambient = Color.White;
            D3DDevice.Instance.Device.Lights[0].Specular = Color.White;
            D3DDevice.Instance.Device.Lights[0].Range = 1000;
            D3DDevice.Instance.Device.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminacion Dinamica
            D3DDevice.Instance.Device.RenderState.Lighting = true;

            //Configurar camara rotacional
            Camera = new TgcRotationalCamera(TGCVector3.Empty, 15f, Input);

            //El tipo de mesh para seleccionar.
            selectedMeshModifier = AddInterval("SelectedMesh", new[] { "Teapot", "Face" }, 0);

            //Selecciona el modo de shading.
            shaderModeModifier = AddInterval("ShaderMode", new[] { "Gouraud", "Flat" }, 1);

            //Habilito o deshabilito mostrar las normales
            normalesModifier = AddBoolean("Normales", "Mostrar normales", false);

            //El exponente del nivel de brillo de la iluminacion especular.
            specularSharpnessModifier = AddFloat("SpecularSharpness", 0, 500f, 100.00f);

            //Habilita o deshabilita el brillo especular.
            specularEnabledModifier = AddBoolean("SpecularEnabled", "Enable Specular", true);

            //Los distintos colores e intensidades de cada uno de los tipos de iluminacion.
            ambientModifier = AddColor("Ambient", Color.LightSlateGray);
            diffuseModifier = AddColor("Diffuse", Color.Gray);
            specularModifier = AddColor("Specular", Color.LightSteelBlue);

            //Habilita o deshabilita el remarcado de los bordes de cada triangulo.
            wireframeModifier = AddBoolean("Wireframe", "Enable Wireframe", false);

            //Habilita o deshabilita el back face culling
            backFaceCullModifier = AddBoolean("BackFaceCull", "Enable BackFaceCulling", false);

            //Modifiers para angulos de rotacion de la luz
            angleXModifier = AddFloat("angleX", 0, 0.005f, 0.0f);
            angleYModifier = AddFloat("angleY", 0, 0.005f, 0.0f);
            angleZModifier = AddFloat("angleZ", 0, 0.005f, 0.0f);

            //Pongo el fondo negro
            BackgroundColor = Color.Black;
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            //Obtener valores de Modifiers
            var vAngleX = angleXModifier.Value;
            var vAngleY = angleYModifier.Value;
            var vAngleZ = angleZModifier.Value;

            //Rotar la luz en base los angulos especificados
            angleX += vAngleX;
            angleY += vAngleY;
            angleZ += vAngleZ;

            //Posicionar la luz a una distancia y rotarla segun los parametros
            var LightRotationMatrix = TGCMatrix.Identity;
            LightRotationMatrix *= TGCMatrix.Translation(lightDistance, 0, 0);
            LightRotationMatrix *= TGCMatrix.RotationYawPitchRoll(angleX, angleY, angleZ);
            D3DDevice.Instance.Device.Lights[0].Position = TGCVector3.TransformCoordinate(TGCVector3.Empty, LightRotationMatrix);
            lightVectorToCenter = new TGCVector3(D3DDevice.Instance.Device.Lights[0].Position);
            D3DDevice.Instance.Device.Lights[0].Direction = -D3DDevice.Instance.Device.Lights[0].Position;
            D3DDevice.Instance.Device.Lights[0].Direction.Normalize();
            D3DDevice.Instance.Device.Lights[0].Update();

            //Poner el vertex buffer de la linea que muestra la direccion de la luz.
            lightVectorVB[0].Position = lightVectorToCenter;
            lightVectorVB[0].Color = Color.White.ToArgb();
            lightVectorVB[1].Position = TGCVector3.Empty;
            lightVectorVB[1].Color = Color.Blue.ToArgb();

            //Variar el color del material
            material.Ambient = ambientModifier.Value;
            material.Diffuse = diffuseModifier.Value;
            material.Specular = specularModifier.Value;

            material.SpecularSharpness = specularSharpnessModifier.Value;

            D3DDevice.Instance.Device.Material = material;

            switch (shaderModeModifier.Value.ToString())
            {
                case "Gouraud":
                    D3DDevice.Instance.Device.RenderState.ShadeMode = ShadeMode.Gouraud;
                    break;

                case "Flat":
                    D3DDevice.Instance.Device.RenderState.ShadeMode = ShadeMode.Flat;
                    break;
            }

            D3DDevice.Instance.Device.RenderState.SpecularEnable = specularEnabledModifier.Value;

            D3DDevice.Instance.Device.RenderState.ColorVertex = true;

            //Habilito o deshabilito el backface culling.
            if (backFaceCullModifier.Value)
            {
                D3DDevice.Instance.Device.RenderState.CullMode = Cull.CounterClockwise;
            }
            else
            {
                D3DDevice.Instance.Device.RenderState.CullMode = Cull.None;
            }

            //Selecciono el mesh y el vertex buffer del modelo.
            switch (selectedMeshModifier.Value.ToString())
            {
                case "Teapot":
                    SelectedMesh = teapotMesh;
                    selectedNormalVB = teapotMeshNormalsVB;
                    break;

                case "Face":
                    SelectedMesh = faceMesh;
                    selectedNormalVB = null;
                    break;
            }

            D3DDevice.Instance.Device.RenderState.Lighting = true;
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();

            //Renderizar malla
            SelectedMesh.DrawSubset(0);

            //Para dibujar el wireframe se desabilita la luz y se pone el fill mode en modo wireframe.
            if (wireframeModifier.Value)
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;
                D3DDevice.Instance.Device.RenderState.Lighting = false;

                SelectedMesh.DrawSubset(0);

                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
                D3DDevice.Instance.Device.RenderState.Lighting = true;
            }

            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();

            D3DDevice.Instance.Device.RenderState.Lighting = false;

            //Dibujo la linea que va desde la luz al centro.
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;

            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, 1, lightVectorVB);

            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Identity.ToMatrix();

            //Dibujo las normales si estan habilitadas y si es la tetera.
            if (selectedNormalVB != null && normalesModifier.Value)
                D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, selectedNormalVB.Length / 2, selectedNormalVB);

            //Traslado y renderizo la esfera que hace de lampara.
            D3DDevice.Instance.Device.Transform.World *= TGCMatrix.Translation(lightVectorToCenter).ToMatrix();
            lightBulb.DrawSubset(0);

            PostRender();
        }

        private void createMeshes()
        {
            //Crear Teapot
            teapotMesh = Mesh.Teapot(D3DDevice.Instance.Device);
            teapotMesh.ComputeNormals();

            //Cargar cara
            faceMesh = Mesh.FromFile(MediaDir + "ModelosX" + "\\" + "Cara.x", MeshFlags.Managed, D3DDevice.Instance.Device);
            faceMesh.ComputeNormals();

            //El vertex buffer con la linea que apunta a la direccion de la luz.
            lightVectorVB = new CustomVertex.PositionColored[2];

            //Obtener los vertices para obtener las normales de la tetera.
            var verts = (CustomVertex.PositionNormal[])teapotMesh.VertexBuffer.Lock(0, typeof(CustomVertex.PositionNormal), LockFlags.None, teapotMesh.NumberVertices);

            //El vertex buffer que tiene las lineas de las normales de la tetera;
            teapotMeshNormalsVB = new CustomVertex.PositionColored[verts.Length * 2];

            for (var i = 0; i < verts.Length; i++)
            {
                //El origen del vector normal esta en la posicion del vertice.
                teapotMeshNormalsVB[i * 2].Position = verts[i].Position;

                //El extremo del vector normal es la posicion mas la normal en si misma. Se escala para que se mas proporcionada.
                teapotMeshNormalsVB[i * 2 + 1].Position = verts[i].Position + TGCVector3.Scale(new TGCVector3(verts[i].Normal), 1 / 10f);
                teapotMeshNormalsVB[i * 2].Color = teapotMeshNormalsVB[i * 2 + 1].Color = Color.Yellow.ToArgb();
            }

            //Libero el vertex buffer.
            teapotMesh.VertexBuffer.Unlock();

            //Creo el mesh que representa el foco de luz.
            lightBulb = Mesh.Sphere(D3DDevice.Instance.Device, 0.5f, 10, 10);
        }

        public override void Dispose()
        {
            SelectedMesh.Dispose();
        }
    }
}