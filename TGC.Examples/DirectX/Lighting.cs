using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo Lighting:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - Iluminaci�n Din�mica, Material, Gouraud Shading
    ///     Crea una tetera y una cara, en la cual puede modificarse varios parametros de iluminacion.
    ///     Autor: Leandro Barbagallo, Mat�as Leone
    /// </summary>
    public class Lighting : TgcExample
    {
        private readonly float lightDistance = 7;
        private float angleX;
        private float angleY;
        private float angleZ;
        private Mesh lightBulb;
        private Vector3 lightVectorToCenter;
        private CustomVertex.PositionColored[] lightVectorVB;
        private Material material;
        private Mesh SelectedMesh;
        private CustomVertex.PositionColored[] selectedNormalVB;
        private Mesh teapotMesh, faceMesh;
        private CustomVertex.PositionColored[] teapotMeshNormalsVB;

        public Lighting(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "DirectX";
            Name = "Lighting";
            Description = "Permite modificar varios parametros del modelo de iluminacion de DirectX.";
        }

        public override void Init()
        {
            createMeshes();

            //Crear Material
            material = new Material();

            //Crear una fuente de luz direccional en la posici�n 0.
            D3DDevice.Instance.Device.Lights[0].Type = LightType.Directional;
            D3DDevice.Instance.Device.Lights[0].Diffuse = Color.White;
            D3DDevice.Instance.Device.Lights[0].Ambient = Color.White;
            D3DDevice.Instance.Device.Lights[0].Specular = Color.White;
            D3DDevice.Instance.Device.Lights[0].Range = 1000;
            D3DDevice.Instance.Device.Lights[0].Enabled = true;

            //Habilitar esquema de Iluminaci�n Din�mica
            D3DDevice.Instance.Device.RenderState.Lighting = true;

            //Configurar camara rotacional
            Camara = new TgcRotationalCamera(new Vector3(0, 0, 0), 10f);

            //El tipo de mesh para seleccionar.
            Modifiers.addInterval("SelectedMesh", new[] { "Teapot", "Face" }, 0);

            //Habilito o deshabilito mostrar las normales
            Modifiers.addBoolean("Normales", "Mostrar normales", false);

            //Los distintos colores e intensidades de cada uno de los tipos de iluminacion.
            Modifiers.addColor("Ambient", Color.FromArgb(0, 0, 0));
            Modifiers.addColor("Diffuse", Color.FromArgb(0, 0, 0));
            Modifiers.addColor("Specular", Color.FromArgb(255, 255, 255));

            //El exponente del nivel de brillo de la iluminacion especular.
            Modifiers.addFloat("SpecularSharpness", 0, 500f, 100.00f);

            //Habilita o deshabilita el brillo especular.
            Modifiers.addBoolean("SpecularEnabled", "Enable Specular", false);

            //Habilita o deshabilita el remarcado de los bordes de cada triangulo.
            Modifiers.addBoolean("Wireframe", "Enable Wireframe", false);

            //Habilita o deshabilita el back face culling
            Modifiers.addBoolean("BackFaceCull", "Enable BackFaceCulling", true);

            //Selecciona el modo de shading.
            Modifiers.addInterval("ShaderMode", new[] { "Gouraud", "Flat" }, 1);

            //Modifiers para �ngulos de rotaci�n de la luz
            Modifiers.addFloat("angleX", 0, 0.005f, 0.0f);
            Modifiers.addFloat("angleY", 0, 0.005f, 0.0f);
            Modifiers.addFloat("angleZ", 0, 0.005f, 0.0f);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            //Pongo el fondo negro
            D3DDevice.Instance.Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);

            //Obtener valores de Modifiers
            var vAngleX = (float)Modifiers["angleX"];
            var vAngleY = (float)Modifiers["angleY"];
            var vAngleZ = (float)Modifiers["angleZ"];

            //Rotar la luz en base los �ngulos especificados
            angleX += vAngleX;
            angleY += vAngleY;
            angleZ += vAngleZ;

            //Posicionar la luz a una distancia y rotarla segun los parametros
            var LightRotationMatrix = Matrix.Identity;
            LightRotationMatrix *= Matrix.Translation(lightDistance, 0, 0);
            LightRotationMatrix *= Matrix.RotationYawPitchRoll(angleX, angleY, angleZ);
            D3DDevice.Instance.Device.Lights[0].Position = Vector3.TransformCoordinate(new Vector3(0, 0, 0),
                LightRotationMatrix);
            lightVectorToCenter = D3DDevice.Instance.Device.Lights[0].Position;
            D3DDevice.Instance.Device.Lights[0].Direction = -D3DDevice.Instance.Device.Lights[0].Position;
            D3DDevice.Instance.Device.Lights[0].Direction.Normalize();
            D3DDevice.Instance.Device.Lights[0].Update();

            //Poner el vertex buffer de la linea que muestra la direccion de la luz.
            lightVectorVB[0].Position = lightVectorToCenter;
            lightVectorVB[0].Color = Color.White.ToArgb();
            lightVectorVB[1].Position = new Vector3(0, 0, 0);
            lightVectorVB[1].Color = Color.Blue.ToArgb();

            //Variar el color del material
            material.Ambient = (Color)Modifiers["Ambient"];
            material.Diffuse = (Color)Modifiers["Diffuse"];
            material.Specular = (Color)Modifiers["Specular"];

            material.SpecularSharpness = (float)Modifiers["SpecularSharpness"];

            D3DDevice.Instance.Device.Material = material;

            switch ((string)Modifiers["ShaderMode"])
            {
                case "Gouraud":
                    D3DDevice.Instance.Device.RenderState.ShadeMode = ShadeMode.Gouraud;
                    break;

                case "Flat":
                    D3DDevice.Instance.Device.RenderState.ShadeMode = ShadeMode.Flat;
                    break;
            }

            D3DDevice.Instance.Device.RenderState.SpecularEnable =
                (bool)Modifiers["SpecularEnabled"];

            D3DDevice.Instance.Device.RenderState.ColorVertex = true;

            //Habilito o deshabilito el backface culling.
            if ((bool)Modifiers["BackFaceCull"])
            {
                D3DDevice.Instance.Device.RenderState.CullMode = Cull.CounterClockwise;
            }
            else
            {
                D3DDevice.Instance.Device.RenderState.CullMode = Cull.None;
            }

            //Selecciono el mesh y el vertex buffer del modelo.
            switch ((string)Modifiers["SelectedMesh"])
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
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            //Renderizar malla
            SelectedMesh.DrawSubset(0);

            //Para dibujar el wireframe se desabilita la luz y se pone el fill mode en modo wireframe.
            if ((bool)Modifiers["Wireframe"])
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;
                D3DDevice.Instance.Device.RenderState.Lighting = false;

                SelectedMesh.DrawSubset(0);

                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
                D3DDevice.Instance.Device.RenderState.Lighting = true;
            }

            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            D3DDevice.Instance.Device.RenderState.Lighting = false;

            //Dibujo la linea que va desde la luz al centro.
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;

            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList,
                1,
                lightVectorVB);

            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            //Dibujo las normales si estan habilitadas y si es la tetera.
            if (selectedNormalVB != null && (bool)Modifiers["Normales"])
                D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList,
                    selectedNormalVB.Length / 2,
                    selectedNormalVB);

            //Traslado y renderizo la esfera que hace de lampara.
            D3DDevice.Instance.Device.Transform.World *= Matrix.Translation(lightVectorToCenter);
            lightBulb.DrawSubset(0);

            helperPostRender();
        }

        private void createMeshes()
        {
            //Crear Teapot
            teapotMesh = Mesh.Teapot(D3DDevice.Instance.Device);
            teapotMesh.ComputeNormals();

            //Cargar cara
            faceMesh = Mesh.FromFile(MediaDir + "ModelosX" + "\\" + "Cara.x", MeshFlags.Managed,
                D3DDevice.Instance.Device);
            faceMesh.ComputeNormals();

            //El vertex buffer con la linea que apunta a la direccion de la luz.
            lightVectorVB = new CustomVertex.PositionColored[2];

            //Obtener los vertices para obtener las normales de la tetera.
            var verts = (CustomVertex.PositionNormal[])
                teapotMesh.VertexBuffer.Lock(0, typeof(CustomVertex.PositionNormal), LockFlags.None,
                    teapotMesh.NumberVertices);

            //El vertex buffer que tiene las lineas de las normales de la tetera;
            teapotMeshNormalsVB = new CustomVertex.PositionColored[verts.Length * 2];

            for (var i = 0; i < verts.Length; i++)
            {
                //El origen del vector normal esta en la posicion del vertice.
                teapotMeshNormalsVB[i * 2].Position = verts[i].Position;

                //El extremo del vector normal es la posicion mas la normal en si misma. Se escala para que se mas proporcionada.
                teapotMeshNormalsVB[i * 2 + 1].Position = verts[i].Position + Vector3.Scale(verts[i].Normal, 1 / 10f);
                teapotMeshNormalsVB[i * 2].Color = teapotMeshNormalsVB[i * 2 + 1].Color = Color.Yellow.ToArgb();
            }

            //Libero el vertex buffer.
            teapotMesh.VertexBuffer.Unlock();

            //Creo el mesh que representa el foco de luz.
            lightBulb = Mesh.Sphere(D3DDevice.Instance.Device, 0.5f, 10, 10);
        }

        public override void Close()
        {
            base.Close();

            SelectedMesh.Dispose();
        }
    }
}