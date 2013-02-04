using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using System.Security.AccessControl;

namespace Examples.Quake3Loader
{
    /// <summary>
    /// Herramienta para parsear un archivo BSP de Quake 3
    /// 
    /// Autor: Martin Giachetti
    /// 
    /// </summary>
    public class BspLoader
    {

        private string[] textureFullPath;
        private Texture[] textures;
        private Texture[] lightMaps;
        private QShaderData[] shaderXTextura;
        private List<QShaderData> shadersData;

        /// <summary>
        /// Crear Loader
        /// </summary>
        public BspLoader()
        {
            shadersData = new List<QShaderData>();
        }

        /// <summary>
        /// Parsear un archivo BSP y cargar todas las estructuras de DirectX necesarias
        /// </summary>
        /// <param name="bspFilePath">Path del mapa BSP</param>
        /// <param name="mediaPath">Carpeta root de todas las texturas, shaders y modelos de Quake 3</param>
        /// <returns>Mapa cargado</returns>
        public BspMap loadBsp(string bspFilePath, string mediaPath)
        {
            GuiController.Instance.Logger.log("Cargando BSP file: " + bspFilePath);

            BspMap bspMap = new BspMap();

            //QShaderParser.GetFxFromFile("test.shader");
            parseBspFile(bspFilePath, bspMap);
            loadTextures(bspMap, mediaPath);
            loadLightMaps(bspMap);
            CreateMeshes(bspMap);
            SetInitialPos(bspMap);

            //Limpiar todo lo del loader
            textureFullPath = null;
            textures = null;
            lightMaps = null;
            shaderXTextura = null;
            shadersData.Clear();

            return bspMap;
        }

        /// <summary>
        /// Parsear archivo BSP
        /// </summary>
        private void parseBspFile(string file, BspMap bspMap)
        {
            bspMap.Data.filePath = file;

            FileStream fp = new FileStream(file, FileMode.Open, FileAccess.Read);

            byte [] arrayByte = new byte[(int)fp.Length];
            fp.Read(arrayByte, 0, (int) fp.Length);
            fp.Close();

            Header header = new Header();

            header.ident = BitConverter.ToInt32(arrayByte, 0);
            header.version = BitConverter.ToInt32(arrayByte, 4);


            for (int i = 0; i < Header.CANT_LUMPS; i++)
            {
                header.lumps[i] = new Lump();
                header.lumps[i].fileofs = BitConverter.ToInt32(arrayByte, 8 + 8*i);
                header.lumps[i].filelen = BitConverter.ToInt32(arrayByte, 12 + 8 * i);
            }

            //modelos
            int offset = header.lumps[(int) LumpEnum.Models].fileofs;
            int cant_t = header.lumps[(int) LumpEnum.Models].filelen/QModel.SIZE;
            bspMap.Data.models = new QModel[cant_t];
            for(int i = 0; i < cant_t ;i++)
            {
                bspMap.Data.models[i] = new QModel();
                bspMap.Data.models[i].LoadFromByteArray(arrayByte, offset);
                offset += QModel.SIZE;
            }

            //shaders
            offset = header.lumps[(int)LumpEnum.Shaders].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Shaders].filelen / QShader.SIZE;
            bspMap.Data.shaders = new QShader[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.shaders[i] = new QShader();
                bspMap.Data.shaders[i].LoadFromByteArray(arrayByte, offset);
                offset += QShader.SIZE;
            }

            //entdata
            ASCIIEncoding enc = new ASCIIEncoding();
            bspMap.Data.entdata = enc.GetString(arrayByte, header.lumps[(int)LumpEnum.Entities].fileofs,
                                            header.lumps[(int) LumpEnum.Entities].filelen);

            //GuiController.Instance.Logger.log(bspMap.Data.entdata);

            //leafs
            offset = header.lumps[(int)LumpEnum.Leafs].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Leafs].filelen / QLeaf.SIZE;
            bspMap.Data.leafs = new QLeaf[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.leafs[i] = new QLeaf();
                bspMap.Data.leafs[i].LoadFromByteArray(arrayByte, offset);
                offset += QLeaf.SIZE;
            }

            //planes
            offset = header.lumps[(int)LumpEnum.Planes].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Planes].filelen / QPlane.SIZE;
            bspMap.Data.planes = new QPlane[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.planes[i] = new QPlane();
                bspMap.Data.planes[i].LoadFromByteArray(arrayByte, offset);
                offset += QPlane.SIZE;
            }

            //nodes
            offset = header.lumps[(int)LumpEnum.Nodes].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Nodes].filelen / QNode.SIZE;
            bspMap.Data.nodes = new QNode[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.nodes[i] = new QNode();
                bspMap.Data.nodes[i].LoadFromByteArray(arrayByte, offset);
                offset += QNode.SIZE;
            }

            //leafSurfaces
            offset = header.lumps[(int)LumpEnum.Leafsurfaces].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Leafsurfaces].filelen / 4;
            bspMap.Data.leafSurfaces = new int[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.leafSurfaces[i] = BitConverter.ToInt32(arrayByte, offset);
                offset += 4;
            }

            //leafbrushes
            offset = header.lumps[(int)LumpEnum.Leafbrushes].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Leafbrushes].filelen / 4;
            bspMap.Data.leafbrushes = new int[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.leafbrushes[i] = BitConverter.ToInt32(arrayByte, offset);
                offset += 4;
            }

            //brushes
            offset = header.lumps[(int)LumpEnum.Brushes].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Brushes].filelen / QBrush.SIZE;
            bspMap.Data.brushes = new QBrush[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.brushes[i] = new QBrush();
                bspMap.Data.brushes[i].LoadFromByteArray(arrayByte, offset);
                offset += QBrush.SIZE;
            }

            //brushSides
            offset = header.lumps[(int)LumpEnum.Brushsides].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Brushsides].filelen / QBrushSide.SIZE;
            bspMap.Data.brushSides = new QBrushSide[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.brushSides[i] = new QBrushSide();
                bspMap.Data.brushSides[i].LoadFromByteArray(arrayByte, offset);
                offset += QBrushSide.SIZE;
            }

            //lightBytes
            offset = header.lumps[(int)LumpEnum.Lightmaps].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Lightmaps].filelen;
            bspMap.Data.lightBytes = new byte[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.lightBytes[i] = arrayByte[offset];
                offset += 1;
            }

            //gridData
            offset = header.lumps[(int)LumpEnum.Lightgrid].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Lightgrid].filelen;
            bspMap.Data.gridData = new byte[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.gridData[i] = arrayByte[offset];
                offset += 1;
            }

            //visBytes
            bspMap.Data.visData = new QVisData();
            offset = header.lumps[(int)LumpEnum.Visibility].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Visibility].filelen;
            bspMap.Data.visData.nVec = BitConverter.ToInt32(arrayByte, offset);
            bspMap.Data.visData.sizeVec = BitConverter.ToInt32(arrayByte, offset + 4);
            offset += 8;
            if (cant_t > 8)
            {
                bspMap.Data.visData.data = new byte[cant_t - 8];
                for (int i = 0; i < cant_t - 8; i++)
                {
                    bspMap.Data.visData.data[i] = arrayByte[offset];
                    offset += 1;
                }
            }
            else
                bspMap.Data.visData = null;

            //drawVerts
            offset = header.lumps[(int)LumpEnum.Drawverts].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Drawverts].filelen / QDrawVert.SIZE;
            bspMap.Data.drawVerts = new QDrawVert[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.drawVerts[i] = new QDrawVert();
                bspMap.Data.drawVerts[i].LoadFromByteArray(arrayByte, offset);
                offset += QDrawVert.SIZE;
            }

            //drawIndexes
            offset = header.lumps[(int)LumpEnum.Drawindexes].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Drawindexes].filelen / 4;
            bspMap.Data.drawIndexes = new int[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.drawIndexes[i] = BitConverter.ToInt32(arrayByte, offset);
                offset += 4;
            }

            //drawSurfaces
            offset = header.lumps[(int)LumpEnum.Surfaces].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Surfaces].filelen / QSurface.SIZE;
            bspMap.Data.drawSurfaces = new QSurface[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.drawSurfaces[i] = new QSurface();
                bspMap.Data.drawSurfaces[i].LoadFromByteArray(arrayByte, offset);
                offset += QSurface.SIZE;
            }

            //fogs
            offset = header.lumps[(int)LumpEnum.Fogs].fileofs;
            cant_t = header.lumps[(int)LumpEnum.Fogs].filelen / QFog.SIZE;
            bspMap.Data.fogs = new QFog[cant_t];
            for (int i = 0; i < cant_t; i++)
            {
                bspMap.Data.fogs[i] = new QFog();
                bspMap.Data.fogs[i].LoadFromByteArray(arrayByte, offset);
                offset += QFog.SIZE;
            }

            bspMap.Data.shaderXSurface = new QShaderData[bspMap.Data.drawSurfaces.Length];
            
        }

        /// <summary>
        /// Carga la posicion inicial en el escenario
        /// </summary>
        private void SetInitialPos(BspMap bspMap)
        {
            string entdata = bspMap.Data.entdata;

            int start = entdata.IndexOf("info_player_deathmatch");
            string temp = entdata.Substring(0, start);
            start = temp.LastIndexOf("{");
            int end = entdata.IndexOf("origin",start);
            int enter = entdata.IndexOf("\n",end);
            string origen = entdata.Substring(end + 9, enter - end - 9);
            string[] cords = origen.Split(new char[]{' ','\"','\n'});

            int x = int.Parse(cords[0]);
            int y = int.Parse(cords[2]) + 100;
            int z = int.Parse(cords[1]);

            bspMap.CollisionManager.InitialPos = new Vector3(x, y, z);
        }

        /// <summary>
        /// Carga todos los meshes
        /// </summary>
        private void CreateMeshes(BspMap bspMap)
        {
            Device device = GuiController.Instance.D3dDevice;

            for (int id = 0; id < bspMap.Data.drawSurfaces.Length; id++)
            {
                QSurface surface = bspMap.Data.drawSurfaces[id];

                //Crear Patch (curva=
                if (surface.surfaceType == QMapSurfaceType.Patch)
                {
                    tessellatePatch(bspMap, id);
                    continue;
                }

                /* billboards, no soportados actualmente
                if (surface.surfaceType == QMapSurfaceType.Flare)
                {

                }
                */

                //Superficies no soportadas
                if (surface.surfaceType != QMapSurfaceType.Planar && surface.surfaceType != QMapSurfaceType.TriangleSoup)
                {
                    //Agregar null para no mover todos los índices
                    bspMap.Meshes.Add(null);
                    continue;
                }

                //Cargar superficies soportadas: de tipo QMapSurfaceType.Planar o QMapSurfaceType.TriangleSoup

                int cant_indices = surface.numIndexes;
                int cant_vertices = surface.numVerts;
                

                if (cant_vertices <= 0)
                    continue;

                //Mesh de DirectX
                Mesh mesh = new Mesh(cant_indices / 3, cant_vertices, MeshFlags.Managed, TgcSceneLoader.DiffuseMapAndLightmapVertexElements, device);

                //Cargar vertexBuffer
                using (VertexBuffer vb = mesh.VertexBuffer)
                {
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] vertices = new TgcSceneLoader.DiffuseMapAndLightmapVertex[cant_vertices];
                    
                    int j = 0;
                    for (int i = surface.firstVert; i < surface.firstVert + surface.numVerts; i++, j++)
                    {
                        vertices[j] = new TgcSceneLoader.DiffuseMapAndLightmapVertex();

                        vertices[j].Position = bspMap.Data.drawVerts[i].xyz;
                        vertices[j].Normal = bspMap.Data.drawVerts[i].normal;
                        vertices[j].Tu0 = bspMap.Data.drawVerts[i].st.X;
                        vertices[j].Tv0 = bspMap.Data.drawVerts[i].st.Y;
                        vertices[j].Tu1 = bspMap.Data.drawVerts[i].lightmap.X;
                        vertices[j].Tv1 = bspMap.Data.drawVerts[i].lightmap.Y;
                        vertices[j].Color = Color.White.ToArgb();//drawVerts[i].color;
                    }

                    vb.SetData(vertices, 0, LockFlags.None);
                }

                using (IndexBuffer ib = mesh.IndexBuffer)
                {
                    short[] indices = new short[cant_indices];
                    int j = 0;
                    for (int i = surface.firstIndex; i < surface.firstIndex + surface.numIndexes; i++, j++)
                    {
                        indices[j] = (short)bspMap.Data.drawIndexes[i];
                    }
                    ib.SetData(indices, 0, LockFlags.None);
                }


                //Crea el tgcMesh
                TgcMesh tgcMesh = CreateTgcMesh(bspMap, mesh, id);
                bspMap.Meshes.Add(tgcMesh);
            }
        }

        /// <summary>
        /// Adaptar mesh de DirectX a mesh de TGC
        /// </summary>
        public TgcMesh CreateTgcMesh(BspMap bspMap, Mesh mesh,int surfaceId)
        {
            QSurface surface = bspMap.Data.drawSurfaces[surfaceId];

            Texture lightmap = surface.lightmapNum >= 0 ? lightMaps[surface.lightmapNum] : null;

            Texture texture = textures[surface.shaderNum];

            //asigno el shader si es que tiene
            bspMap.Data.shaderXSurface[surfaceId] = shaderXTextura[surface.shaderNum];

            if (texture == null && bspMap.Data.shaderXSurface[surfaceId] != null)
                foreach (QShaderStage stage in bspMap.Data.shaderXSurface[surfaceId].Stages)
                {
                    if (stage.Textures.Count > 0)
                    {
                        texture = stage.Textures[0];
                        break;
                    }

                }



            string textPath = textureFullPath[surface.shaderNum];
            string textName = "";
            if(textPath != null)
                textName = textPath.Substring(textPath.LastIndexOf('\\') + 1);;
            
            //Cargar lightMap
            TgcTexture tgcLightMap = null;
            if (lightmap != null)
            {
                tgcLightMap = new TgcTexture(null, null, lightmap, false);
            }

            TgcTexture[] meshTextures = null;
            if (texture != null)
            {
                meshTextures = new TgcTexture[1];
                meshTextures[0] = new TgcTexture(textName, textPath, texture, false);
            }

            TgcMesh.MeshRenderType renderType = TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP;

            if (lightmap == null)
                renderType = TgcMesh.MeshRenderType.DIFFUSE_MAP;

            if (texture == null)
                renderType = TgcMesh.MeshRenderType.VERTEX_COLOR;


            Material mat = new Material();
            mat.Ambient = Color.White;

            //Crear mesh de TGC
            TgcMesh tgcMesh = new TgcMesh(mesh, "mesh" + surfaceId, renderType);
            tgcMesh.Materials = new Material[] { mat };
            tgcMesh.DiffuseMaps = meshTextures;
            tgcMesh.LightMap = tgcLightMap;
            tgcMesh.Enabled = true;
            tgcMesh.createBoundingBox();

            return tgcMesh;
        }

        /// <summary>
        /// Verificar existencia de textura
        /// </summary>
        /// <returns>Vacio si no encuentra la textura</returns>
        public static string FindTextureExtension(string text, string mediaPath)
        {
            text = text.TrimEnd(new char[] { '\0' }).Replace('/','\\');
            string tga = mediaPath + text + ".tga";
            string jpg = mediaPath + text + ".jpg";

            if (File.Exists(tga))
                return tga;

            if (File.Exists(jpg))
                return jpg;

            return "";
            
        }

        /// <summary>
        /// Cargar texturas y shaders
        /// </summary>
        private void loadTextures(BspMap bspMap, string mediaPath)
        {
            textures = new Texture[bspMap.Data.shaders.Length];
            shaderXTextura = new QShaderData[bspMap.Data.shaders.Length];
            textureFullPath = new string[bspMap.Data.shaders.Length];

            for (int i = 0; i < bspMap.Data.shaders.Length; i++)
            {
                // Find the extension if any and append it to the file name
                string file = FindTextureExtension(bspMap.Data.shaders[i].shader, mediaPath);

                // Create a texture from the image
                if (file.Length > 0)
                {
                    textures[i] = TextureLoader.FromFile(GuiController.Instance.D3dDevice, file);
                    textureFullPath[i] = file;
                }

                //Si no tiene textura entonces tiene un shader
                else
                {
                    string shader_text = bspMap.Data.shaders[i].shader.TrimEnd(new char[] { '\0' });

                    //puede que sea un shader
                    foreach (QShaderData shaderData in shadersData)
                    {
                        if (shaderData.Name.Equals(shader_text))
                        {
                            shaderXTextura[i] = shaderData;
                            break;
                        }
                    }

                    if (shaderXTextura[i] != null)
                        //ya tiene un shader asignado
                        continue;

                    if (!shader_text.Contains("/") && !bspMap.Data.shaders[i].shader.Contains("\\"))
                        continue;

                    string scriptDir = mediaPath + @"scripts\";
                    string fileScript = shader_text.Split(new char[] {'\\', '/'})[1] + ".shader";
                    string pathScript = scriptDir + fileScript;
                    if (File.Exists(pathScript))
                    {
                        shadersData.AddRange(Q3ShaderParser.GetFxFromFile(pathScript, mediaPath));

                        //asigno el shader a la textura
                        foreach (QShaderData shaderData in shadersData)
                        {
                            if (shaderData.Name.Equals(shader_text))
                            {
                                shaderXTextura[i] = shaderData;
                                break;
                            }
                        }

                    }
                    else
                    {
                        //logea el shader que no se pudo cargar
                        GuiController.Instance.Logger.log("Textura o shader no encontrado - ID:" + i + " " + shader_text);
                    }
                }
            }
        }


        /// <summary>
        /// Utilidad para detectar los recursos necesarios de mapa (texturas, shaders, etc)
        /// y empaquetarlos en una carpeta destino.
        /// Es útil para depurar todos los archivos no utilizados y crear una copia limpia
        /// del mapa.
        /// </summary>
        /// <param name="bspMap">Mapa ya cargado</param>
        /// <param name="mediaPath">Carpeta root de todas las texturas, shaders y modelos de Quake 3</param>
        /// <param name="targetFolder">Carpeta destino</param>
        public void packLevel(BspMap bspMap, string mediaPath, string targetFolder)
        {
            GuiController.Instance.Logger.log("Empaquetando: nivel: " + bspMap.Data.filePath);
            //copia el archivo bsp en la carpeta maps
            string mapa = bspMap.Data.filePath.Substring(bspMap.Data.filePath.LastIndexOf("\\") + 1);
            fileCopy(bspMap.Data.filePath, targetFolder + "\\maps\\" + mapa);


            //salva todas las texturas y los shaders
            textures = new Texture[bspMap.Data.shaders.Length];
            shaderXTextura = new QShaderData[bspMap.Data.shaders.Length];
            textureFullPath = new string[bspMap.Data.shaders.Length];

            for (int i = 0; i < bspMap.Data.shaders.Length; i++)
            {
                // Find the extension if any and append it to the file name
                string file = FindTextureExtension(bspMap.Data.shaders[i].shader, mediaPath);

                // Create a texture from the image
                if (file.Length > 0)
                {
                    string shader_text = bspMap.Data.shaders[i].shader.TrimEnd(new char[] { '\0' }).Replace('/', '\\');
                    string ext = file.Substring(file.LastIndexOf("."));
                    string newTex = targetFolder + "\\" + shader_text + ext;
                    fileCopy(file, newTex);
                }
                //Si no tiene textura entonces tiene un shader
                else
                {
                    string shader_text = bspMap.Data.shaders[i].shader.TrimEnd(new char[] { '\0' });

                    if (!shader_text.Contains("/") && !bspMap.Data.shaders[i].shader.Contains("\\"))
                        continue;

                    string scriptDir = mediaPath + @"scripts\";
                    string fileScript = shader_text.Split(new char[] { '\\', '/' })[1] + ".shader";
                    string pathScript = scriptDir + fileScript;
                    if (File.Exists(pathScript))
                    {
                        string newShader = targetFolder + @"\scripts\" + fileScript;
                        fileCopy(pathScript, newShader);

                        //Reviso si el shader hace llamada a alguna textura y la guardo
                        QShaderTokenizer tokenizer = new QShaderTokenizer(File.ReadAllText(pathScript));
                        while (!tokenizer.EOF)
                        {
                            string token = tokenizer.GetNext();
                            if (token.Contains(".tga") || token.Contains(".jpg"))
                            {
                                token = token.Replace('/', '\\');
                                string src = mediaPath + "\\" + token;
                                //hay una textura que debe ser salvada
                                if (File.Exists(src))
                                {
                                    fileCopy(src, targetFolder + "\\" + token);
                                }
                            }
                        }
                    }
                }
            }

            GuiController.Instance.Logger.log("Empaquetando Exitoso");
        }

        /// <summary>
        /// Utilidad para copiar archivos y carpetas
        /// </summary>
        private void fileCopy(string src, string dst)
        {
            string carpeta = dst.Substring(0, dst.LastIndexOf("\\"));

            if (File.Exists(src))
            {
                Directory.CreateDirectory(carpeta);
                File.Copy(src, dst, true);
            }
        }

        /// <summary>
        /// Cargar lightmaps
        /// </summary>
        private void loadLightMaps(BspMap bspMap)
        {
            const int LIGHTMAP_SIZE = 128*128;
            int cant_lmaps = bspMap.Data.lightBytes.Length / (LIGHTMAP_SIZE * 3);
            lightMaps = new Texture[cant_lmaps];
            int[] lightInfo = new int[LIGHTMAP_SIZE];

            for (int i = 0; i < cant_lmaps; i++)
            {
                //transformo de RGB a XRGB agregandole un canal mas
                for (int j = 0; j < LIGHTMAP_SIZE; j++)
                {
                    int offset = (i*LIGHTMAP_SIZE +j)*3;

                    lightInfo[j] = changeGamma(bspMap.Data.lightBytes[offset + 0], bspMap.Data.lightBytes[offset + 1],
                                                    bspMap.Data.lightBytes[offset + 2]);
                }

                Texture tex = new Texture(GuiController.Instance.D3dDevice, 128, 128, 0, Usage.None,
                                          Format.X8R8G8B8, Pool.Managed);

                GraphicsStream graphicsStream = tex.LockRectangle(0, LockFlags.None);
                graphicsStream.Write(lightInfo);
                tex.UnlockRectangle(0);

                lightMaps[i] = tex;
            }
        }


        /// <summary>
        /// Obtener intensidad correcta de colores de Lightmap
        /// </summary>
        private int changeGamma(byte r, byte g, byte b)
        {
	        int ir, ig, ib, Gamma, imax;
	        float factor;
            Gamma = 1/*2*/;

            ir = r << Gamma;
            ig = g << Gamma;
            ib = b << Gamma;

	        imax = Math.Max(ir, Math.Max(ig, ib ));
	        if(imax>255) {
		        factor = 255.0f/imax;
                ir = (int)(ir * factor);
                ig = (int)(ig * factor);
                ib = (int)(ib * factor);
	        }

	        return Color.FromArgb(255, ir, ig, ib ).ToArgb();
        }

        /// <summary>
        /// Toma una curva de Quake 3 y la convierte a un TgcMesh, mediante el proceso de Tessellation 
        /// </summary>
        private void tessellatePatch(BspMap bspMap, int surfaceId)
        {
            // los patch de quake 3 estan formados por curvas de bezier
            // todos los patch tienen 9 puntos que son los puntos de control para la curva
            // Esos puntos hay que tesselarlos para conseguir los triangulos

            int L = 5;

            QSurface surface = bspMap.Data.drawSurfaces[surfaceId];
            int offVert = surface.firstVert;

            // coeficientes de bezier
            float[,] B = new float[L+1, 3];

            //Numero de patches de 3x3 en cada direccion
            int num1 = (surface.patchHeight - 1) / 2;
            int num2 = (surface.patchWidth - 1) / 2;
            int cantVertices = (L * num1 + 1) * (L * num2 + 1);
            int cantIndices = 2 * (L * num2 + 2) * (L * num1);
                
            //aloco el espacio para los vertices y los indices
            Vector3[] vertices = new Vector3[cantVertices];
            Vector3[] normals = new Vector3[cantVertices];
            Vector2[] textCords = new Vector2[cantVertices];
            Vector2[] textCords2 = new Vector2[cantVertices];
            int[] indices = new int[cantIndices];
            {
                //se cargan las constantes de bezier
                float dt = 1.0f / L;
                float t = 0;
                for (int i = 0; i < L + 1; i++)
                {
                    if (i == L)
                        t = 1.0f;
                    float mt = 1 - t;

                    B[i,0] = mt * mt;
                    B[i,1] = 2 * mt * t;
                    B[i,2] = t * t;
 
                    t += dt;
                }
            }

            //Calculo de los bicubic bezier patch para cada strip
            int pointsXStrip = L * num2 + 1;
            int pointsXPatch = pointsXStrip * L;
            int indexCount = 0;
            int vertexNum = 0;
            int[] controls = new int[3]; // subindices a los vertices de puntos de control
            // por cada patch
            for (int i = 0, Li = L; i < num1; i++)
            {
                if (i == num1 - 1)
                    Li = L + 1;

                // seteo los indices
                for (int j = 0; j < L; j++)
                {
                    for (int k = 0; k < pointsXStrip; k++)
                    {
                        indices[indexCount++] = i * pointsXPatch + j * pointsXStrip + k;
                        indices[indexCount++] = i * pointsXPatch + (j + 1) * pointsXStrip + k;
                    }

                    //repito primer y ultimo indice
                    indices[indexCount++] = i * pointsXPatch + (j + 2) * pointsXStrip - 1;
                    indices[indexCount++] = i * pointsXPatch + (j + 1) * pointsXStrip;
                }


                //Ahora van los puntos de control y los vertices
                for (int j = 0, Lj = L; j < num2; j++)
                {
                    if (j == num2 - 1)
                        Lj = L + 1;

                    // calculo de los puntos de control para este patch
                    // controls[fila] = indice del primer vertice de la fila de este patch

                    controls[0] = offVert + (i * 2) * (2 * num2 + 1) + (j * 2);
                    controls[1] = offVert + (i * 2 + 1) * (2 * num2 + 1) + (j * 2);
                    controls[2] = offVert + (i * 2 + 2) * (2 * num2 + 1) + (j * 2);

                    //por cada punto en el patch
                    for (int i2 = 0; i2 < Li; i2++)
                    {
                        vertexNum = i * pointsXPatch + i2 * pointsXStrip + j * L;

                        for (int j2 = 0; j2 < Lj; j2++, vertexNum++)
                        {
                            vertices[vertexNum] = new Vector3();
                            normals[vertexNum] = new Vector3();
                            textCords[vertexNum] = new Vector2();
                            textCords2[vertexNum] = new Vector2();

                            for (int i3 = 0; i3 < 3; i3++)
                            {
                                for (int j3 = 0; j3 < 3; j3++)
                                {
                                    float blendFactor = B[i2, i3] * B[j2, j3];
                                    vertices[vertexNum] += bspMap.Data.drawVerts[controls[i3] + j3].xyz * blendFactor;
                                    normals[vertexNum] += bspMap.Data.drawVerts[controls[i3] + j3].normal * blendFactor;
                                    textCords[vertexNum] += bspMap.Data.drawVerts[controls[i3] + j3].st * blendFactor;
                                    textCords2[vertexNum] += bspMap.Data.drawVerts[controls[i3] + j3].lightmap * blendFactor;
                                }
                            }
                        }
                    }
                }
            }


            short[] indexBuffer = new short[3 * cantIndices - 6];
            int cant_ibuffer = 0;
            
            for (int i = 2; i < cantIndices; i++)
            {
                if (i % 2 == 0)
                {
                    indexBuffer[cant_ibuffer++] = (short) indices[i - 2];
                    indexBuffer[cant_ibuffer++] = (short) indices[i - 1];
                    indexBuffer[cant_ibuffer++] = (short) indices[i];
                }
                else
                {
                    indexBuffer[cant_ibuffer++] = (short)indices[i];
                    indexBuffer[cant_ibuffer++] = (short)indices[i - 1];
                    indexBuffer[cant_ibuffer++] = (short)indices[i - 2];
                }
            }

            //Mesh de DirectX
            Mesh mesh = new Mesh(indexBuffer.Length / 3, vertices.Length, MeshFlags.Managed,
                                 TgcSceneLoader.DiffuseMapAndLightmapVertexElements,
                                 GuiController.Instance.D3dDevice);

            //Cargar vertexBuffer
            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                TgcSceneLoader.DiffuseMapAndLightmapVertex[] vertex =
                    new TgcSceneLoader.DiffuseMapAndLightmapVertex[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertex[i] = new TgcSceneLoader.DiffuseMapAndLightmapVertex();

                    vertex[i].Position = vertices[i];
                    vertex[i].Normal = normals[i];//drawVerts[offVert].normal;
                    vertex[i].Tu0 = textCords[i].X;//drawVerts[offVert].st.X;
                    vertex[i].Tv0 = textCords[i].Y;//drawVerts[offVert].st.Y;
                    vertex[i].Tu1 = textCords2[i].X;//drawVerts[offVert].lightmap.X;
                    vertex[i].Tv1 = textCords2[i].Y;//drawVerts[offVert].lightmap.Y;
                    vertex[i].Color = Color.White.ToArgb(); //drawVerts[i].color;

                    vb.SetData(vertex, 0, LockFlags.None);
                }
            }

            //IndexBuffer
            using (IndexBuffer ib = mesh.IndexBuffer)
            {
                ib.SetData(indexBuffer, 0, LockFlags.None);
            }

            //Crea el tgcMesh
            TgcMesh tgcMesh = CreateTgcMesh(bspMap, mesh, surfaceId);

            bspMap.Meshes.Add(tgcMesh);  
        }


        
         
    }
}
