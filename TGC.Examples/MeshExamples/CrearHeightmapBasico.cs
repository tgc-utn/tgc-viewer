using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo CrearHeightmapBasico:
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Tecnicas de Optimizacion - Heightmap
    ///     Crea un terreno en base a una textura de Heightmap.
    ///     Aplica sobre el terreno una textura para dar color (DiffuseMap).
    ///     Se parsea la textura y se crea un VertexBuffer en base las distintas
    ///     alturas de la imagen.
    ///     Ver el ejemplo EjemploSimpleTerrain para aprender como realizar esto mismo
    ///     pero en forma mas simple con la herramienta TgcSimpleTerrain
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class CrearHeightmapBasico : TGCExampleViewer
    {
        private string currentHeightmap;
        private float currentScaleXZ;
        private float currentScaleY;
        private string currentTexture;
        private Texture terrainTexture;
        private int totalVertices;
        private VertexBuffer vbTerrain;

        public CrearHeightmapBasico(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Mesh Examples";
            Name = "Simple Heightmap Manual";
            Description = "Muestra como crear un terreno en base a una textura de HeightMap en forma manual.";
        }

        public override void Init()
        {
            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = MediaDir + "Heighmaps\\" + "Heightmap3.jpg";
            Modifiers.addTexture("heightmap", currentHeightmap);

            //Modifiers para variar escala del mapa
            currentScaleXZ = 50f;
            Modifiers.addFloat("scaleXZ", 0.1f, 100f, currentScaleXZ);
            currentScaleY = 1.5f;
            Modifiers.addFloat("scaleY", 0.1f, 10f, currentScaleY);
            createHeightMapMesh(D3DDevice.Instance.Device, currentHeightmap, currentScaleXZ, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = MediaDir + "Heighmaps\\" + "TerrainTexture3.jpg";
            Modifiers.addTexture("texture", currentTexture);
            loadTerrainTexture(D3DDevice.Instance.Device, currentTexture);

            //Configurar FPS Camara
            Camara = new TgcFpsCamera(new Vector3(3200f, 450f, 1500f), Input);

            //UserVars para cantidad de vertices
            UserVars.addVar("Vertices", totalVertices);
            UserVars.addVar("Triangles", totalVertices / 3);
        }

        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Crea y carga el VertexBuffer en base a una textura de Heightmap
        /// </summary>
        private void createHeightMapMesh(Device d3dDevice, string path, float scaleXZ, float scaleY)
        {
            //parsear bitmap y cargar matriz de alturas
            var heightmap = loadHeightMap(path);

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Crear array temporal de vertices
            var dataIdx = 0;
            var data = new CustomVertex.PositionTextured[totalVertices];

            //Iterar sobre toda la matriz del Heightmap y crear los triangulos necesarios para el terreno
            for (var i = 0; i < heightmap.GetLength(0) - 1; i++)
            {
                for (var j = 0; j < heightmap.GetLength(1) - 1; j++)
                {
                    //Crear los cuatro vertices que conforman este cuadrante, aplicando la escala correspondiente
                    var v1 = new Vector3(i * scaleXZ, heightmap[i, j] * scaleY, j * scaleXZ);
                    var v2 = new Vector3(i * scaleXZ, heightmap[i, j + 1] * scaleY, (j + 1) * scaleXZ);
                    var v3 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j] * scaleY, j * scaleXZ);
                    var v4 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j + 1] * scaleY, (j + 1) * scaleXZ);

                    //Crear las coordenadas de textura para los cuatro vertices del cuadrante
                    var t1 = new Vector2(i / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
                    var t2 = new Vector2(i / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));
                    var t3 = new Vector2((i + 1) / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
                    var t4 = new Vector2((i + 1) / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));

                    //Cargar triangulo 1
                    data[dataIdx] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
                    data[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);

                    //Cargar triangulo 2
                    data[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                    data[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);

                    dataIdx += 6;
                }
            }

            //Llenar todo el VertexBuffer con el array temporal
            vbTerrain.SetData(data, 0, LockFlags.None);
        }

        /// <summary>
        ///     Cargar textura
        /// </summary>
        private void loadTerrainTexture(Device d3dDevice, string path)
        {
            //Rotar e invertir textura
            var b = (Bitmap)Image.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }

        /// <summary>
        ///     Cargar Bitmap y obtener el valor en escala de gris de Y
        ///     para cada coordenada (x,z)
        /// </summary>
        private int[,] loadHeightMap(string path)
        {
            //Cargar bitmap desde el FileSystem
            var bitmap = (Bitmap)Image.FromFile(path);
            var width = bitmap.Size.Width;
            var height = bitmap.Size.Height;
            var heightmap = new int[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    //Obtener color
                    //(j, i) invertido para primero barrer filas y despues columnas
                    var pixel = bitmap.GetPixel(j, i);

                    //Calcular intensidad en escala de grises
                    var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
                }
            }

            return heightmap;
        }

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Camera pos: " + TgcParserUtils.printVector3(Camara.Position), 5, 20, Color.Red);
            DrawText.drawText("Camera LookAt: " + TgcParserUtils.printVector3(Camara.LookAt), 5, 40, Color.Red);

            //Ver si cambio el heightmap
            var selectedHeightmap = (string)Modifiers["heightmap"];
            if (currentHeightmap != selectedHeightmap)
            {
                currentHeightmap = selectedHeightmap;
                createHeightMapMesh(D3DDevice.Instance.Device, currentHeightmap, currentScaleXZ, currentScaleY);
            }

            //Ver si cambio alguno de los valores de escala
            var selectedScaleXZ = (float)Modifiers["scaleXZ"];
            var selectedScaleY = (float)Modifiers["scaleY"];
            if (currentScaleXZ != selectedScaleXZ || currentScaleY != selectedScaleY)
            {
                currentScaleXZ = selectedScaleXZ;
                currentScaleY = selectedScaleY;
                createHeightMapMesh(D3DDevice.Instance.Device, currentHeightmap, currentScaleXZ, currentScaleY);
            }

            //Ver si cambio la textura del terreno
            var selectedTexture = (string)Modifiers["texture"];
            if (currentTexture != selectedTexture)
            {
                currentTexture = selectedTexture;
                loadTerrainTexture(D3DDevice.Instance.Device, currentTexture);
            }

            //Render terrain
            D3DDevice.Instance.Device.SetTexture(0, terrainTexture);
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, vbTerrain, 0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);

            PostRender();
        }

        public override void Dispose()
        {
            vbTerrain.Dispose();
            terrainTexture.Dispose();
        }
    }
}