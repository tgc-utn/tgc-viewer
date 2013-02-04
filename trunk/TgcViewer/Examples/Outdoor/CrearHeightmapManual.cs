using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;

namespace Examples.Outdoor
{
    /// <summary>
    /// Ejemplo CrearHeightmapManual:
    /// Unidades Involucradas:
    ///     # Unidad 7 - Técnicas de Optimización - Heightmap
	///
    /// Crea un terreno en base a una textura de Heightmap.
    /// Aplica sobre el terreno una textura para dar color (DiffuseMap).
    /// Se parsea la textura y se crea un VertexBuffer en base las distintas
    /// alturas de la imagen.
    /// Ver el ejemplo EjemploSimpleTerrain para aprender como realizar esto mismo
    /// pero en forma mas simple con la herramienta TgcSimpleTerrain
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CrearHeightmapManual : TgcExample
    {
        VertexBuffer vbTerrain;
        Texture terrainTexture;
        int totalVertices;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ;
        float currentScaleY;

        public override string getCategory()
        {
            return "Outdor";
        }

        public override string getName()
        {
            return "Heightmap Manual";
        }

        public override string getDescription()
        {
            return "Muestra como crear un terreno en base a una textura de HeightMap en forma manual";
        }


        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap1.jpg";
            GuiController.Instance.Modifiers.addTexture("heightmap", currentHeightmap);

            //Modifiers para variar escala del mapa
            currentScaleXZ = 20f;
            GuiController.Instance.Modifiers.addFloat("scaleXZ", 0.1f, 100f, currentScaleXZ);
            currentScaleY = 1.3f;
            GuiController.Instance.Modifiers.addFloat("scaleY", 0.1f, 10f, currentScaleY);
            createHeightMapMesh(d3dDevice, currentHeightmap, currentScaleXZ, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture1-256x256.jpg";
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture);
            loadTerrainTexture(d3dDevice, currentTexture);



            //Configurar FPS Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 100f;
            GuiController.Instance.FpsCamera.JumpSpeed = 100f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-24.9069f, 386.3114f, 673.7542f), new Vector3(844.4131f, -107.726f, 688.2306f));
            

            //UserVars para cantidad de vertices
            GuiController.Instance.UserVars.addVar("Vertices", totalVertices);
            GuiController.Instance.UserVars.addVar("Triangles", totalVertices / 3);
        }


        /// <summary>
        /// Crea y carga el VertexBuffer en base a una textura de Heightmap
        /// </summary>
        private void createHeightMapMesh(Device d3dDevice, string path, float scaleXZ, float scaleY)
        {
            //parsear bitmap y cargar matriz de alturas
            int[,] heightmap = loadHeightMap(d3dDevice, path);

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Crear array temporal de vertices
            int dataIdx = 0;
            CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[totalVertices];

            //Iterar sobre toda la matriz del Heightmap y crear los triangulos necesarios para el terreno
            for (int i = 0; i < heightmap.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < heightmap.GetLength(1) - 1; j++)
                {
                    //Crear los cuatro vertices que conforman este cuadrante, aplicando la escala correspondiente
                    Vector3 v1 = new Vector3(i * scaleXZ, heightmap[i, j] * scaleY, j * scaleXZ);
                    Vector3 v2 = new Vector3(i * scaleXZ, heightmap[i, j + 1] * scaleY, (j + 1) * scaleXZ);
                    Vector3 v3 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j] * scaleY, j * scaleXZ);
                    Vector3 v4 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j + 1] * scaleY, (j + 1) * scaleXZ);

                    //Crear las coordenadas de textura para los cuatro vertices del cuadrante
                    Vector2 t1 = new Vector2(i / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
                    Vector2 t2 = new Vector2(i / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));
                    Vector2 t3 = new Vector2((i + 1) / (float)heightmap.GetLength(0), j / (float)heightmap.GetLength(1));
                    Vector2 t4 = new Vector2((i + 1) / (float)heightmap.GetLength(0), (j + 1) / (float)heightmap.GetLength(1));

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
        /// Cargar textura
        /// </summary>
        private void loadTerrainTexture(Device d3dDevice, string path)
        {
            //Rotar e invertir textura
            Bitmap b = (Bitmap)Bitmap.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }

        /// <summary>
        /// Cargar Bitmap y obtener el valor en escala de gris de Y
        /// para cada coordenada (x,z)
        /// </summary>
        private int[,] loadHeightMap(Device d3dDevice, string path)
        {
            //Cargar bitmap desde el FileSystem
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;
            int[,] heightmap = new int[width, height];

            for (int i = 0; i < width; i++)
			{
			     for (int j = 0; j < height; j++)
			    {
                    //Obtener color
                    //(j, i) invertido para primero barrer filas y despues columnas
                    Color pixel = bitmap.GetPixel(j, i);

                    //Calcular intensidad en escala de grises
                    float intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
			    }
			}

            return heightmap;
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio el heightmap
            string selectedHeightmap = (string)GuiController.Instance.Modifiers["heightmap"];
            if (currentHeightmap != selectedHeightmap)
            {
                currentHeightmap = selectedHeightmap;
                createHeightMapMesh(d3dDevice, currentHeightmap, currentScaleXZ, currentScaleY);
            }

            //Ver si cambio alguno de los valores de escala
            float selectedScaleXZ = (float)GuiController.Instance.Modifiers["scaleXZ"];
            float selectedScaleY = (float)GuiController.Instance.Modifiers["scaleY"];
            if (currentScaleXZ != selectedScaleXZ || currentScaleY != selectedScaleY)
            {
                currentScaleXZ = selectedScaleXZ;
                currentScaleY = selectedScaleY;
                createHeightMapMesh(d3dDevice, currentHeightmap, currentScaleXZ, currentScaleY);
            }

            //Ver si cambio la textura del terreno
            string selectedTexture = (string)GuiController.Instance.Modifiers["texture"];
            if (currentTexture != selectedTexture)
            {
                currentTexture = selectedTexture;
                loadTerrainTexture(d3dDevice, currentTexture);
            }


            //Render terrain 
            d3dDevice.SetTexture(0, terrainTexture);
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
        }

        public override void close()
        {
            vbTerrain.Dispose();
            terrainTexture.Dispose();
        }

        

        
    }
}
