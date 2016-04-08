using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using TgcViewer.Utils;

namespace Examples.Shaders.WorkshopShaders
{
    public class F1Circuit
    {
        VertexBuffer vb;
        public Texture textura_piso;
        public Texture textura_guardrail;
        public Texture textura_cartel;
        public int totalVertices;
        public Vector3[] pt_ruta = new Vector3[500];
        public int cant_ptos_ruta;
        public float M_PI = 3.14151f;
        public float ancho_ruta = 200;
        public float ancho_guarray = 3;
        public float dh = 3;		// alto de la pared
        public float scaleXZ = 20;
        public float scaleY = 15;
        public bool en_ruta = false;
        public float Hpiso;			// Altura del piso en la Pos
        public int pos_en_ruta;
        public int cant_carteles;
        public int pos_carteles;

        public TgcMesh []arbol = new TgcMesh[10];
        public int cant_arboles;



        public F1Circuit()
        {
            CrearRuta();

            TgcSceneLoader loader = new TgcSceneLoader();

            arbol[cant_arboles] = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml").Meshes[0];
            cant_arboles++;

            arbol[cant_arboles] = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera3\\Palmera3-TgcScene.xml").Meshes[0];
            arbol[cant_arboles].Scale = new Vector3(2, 2, 2);
            ++cant_arboles;

            arbol[cant_arboles] = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera2\\Palmera2-TgcScene.xml").Meshes[0];
            arbol[cant_arboles].Scale = new Vector3(1, 1, 1);
            ++cant_arboles;

            arbol[cant_arboles] = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml").Meshes[0];
            arbol[cant_arboles].Scale = new Vector3(4, 4, 4);
            ++cant_arboles;


        }


        // Carga los ptos de la ruta
        public int load_pt_ruta()
        {
	        // Genero el path de la ruta 
	        double dt = M_PI/64;
	        double t = 0;
	        double hasta = 2*M_PI;
	        int cant = 0;
            float dw = 10000;
	        while(t<hasta+0.1)
	        {

                pt_ruta[cant].X = (float)(8 * (8 * Math.Cos(t) + Math.Cos(5 * t) * Math.Cos(t))) * scaleXZ;
                pt_ruta[cant].Z = (float)(8 * (8 * Math.Sin(t) + Math.Cos(4 * t) * Math.Sin(t))) * scaleXZ;
                pt_ruta[cant].Y = (float)Math.Max(0, 3 + 2 * Math.Cos(3 + t * 5)) * scaleY;
		        t+=dt;
		        ++cant;
	        }
	        --cant;		// me aseguro que siempre exista el i+1 

	        return cant;

        }


        public void CrearRuta()
        {
        
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Dispose de VertexBuffer anterior, si habia
            if (vb != null && !vb.Disposed)
            {
                vb.Dispose();
            }

            float Kr = 0.5f;
            float dr = ancho_ruta / 2;

            // Cargo la ruta
            cant_ptos_ruta = load_pt_ruta();
            cant_carteles = 6;
            int dc = cant_ptos_ruta / cant_carteles;

            //Crear vertexBuffer
            totalVertices = cant_ptos_ruta * 6 + cant_carteles*4;
            vb = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[totalVertices];

            // piso
            for (int i = 0; i < cant_ptos_ruta; ++i)
            {
                Vector3 dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                Vector3 n = Vector3.Cross(dir , new Vector3(0, 1, 0));
                Vector3 p0 = pt_ruta[i] - n * dr;
                Vector3 p1 = pt_ruta[i] + n * dr;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, 1, i * Kr);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, 0, i * Kr);

            }


            // pared izquierda
            for (int i = 0; i < cant_ptos_ruta; ++i)
            {
                Vector3 dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                Vector3 n = Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 u = Vector3.Cross(n ,dir);
                Vector3 p0 = pt_ruta[i] - n * (dr + ancho_guarray);
                Vector3 p1 = pt_ruta[i] - n * dr;
                p0.Y -= 25;
                p1.Y += 25;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, i * Kr,1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, i * Kr,0);
            }


            // pared derecha
            for (int i = 0; i < cant_ptos_ruta; ++i)
            {
                Vector3 dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                Vector3 n = Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 u = Vector3.Cross(n, dir);
                Vector3 p0 = pt_ruta[i] + n * (dr + ancho_guarray);
                Vector3 p1 = pt_ruta[i] + n * dr;
                p0.Y -= 25;
                p1.Y += 25;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, i * Kr,1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, i * Kr,0);
            }

            // Carteles
            pos_carteles = dataIdx;
            for (int t = 0; t < cant_carteles; ++t)
            {
                int i = t * dc;
                Vector3 dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                Vector3 up = new Vector3(0, 1, 0);
                Vector3 n = Vector3.Cross(dir, up);
                Vector3 p0 = pt_ruta[i] - n * (dr + 50) + up*0;
                Vector3 p1 = pt_ruta[i] - n * (dr + 50) + up * 170;
                Vector3 p2 = pt_ruta[i] + n * (dr + 50) + up * 170;
                Vector3 p3 = pt_ruta[i] + n * (dr + 50) + up*0;
                data[dataIdx++] = new CustomVertex.PositionTextured(p0, 1, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p3, 0, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, 1, 0);
                data[dataIdx++] = new CustomVertex.PositionTextured(p2, 0, 0);
            }



            vb.SetData(data, 0, LockFlags.None);

            // Cargo la textura del piso 
            loadTextures();

        }

        /// <summary>
        /// Carga la textura del terreno
        /// </summary>
        public void loadTextures()
        {
            //Dispose textura anterior, si habia
            if (textura_piso != null && !textura_piso.Disposed)
            {
                textura_piso.Dispose();
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            String MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            textura_piso = Texture.FromBitmap(d3dDevice, (Bitmap)Bitmap.FromFile(MyMediaDir + "f1\\piso2.png"), Usage.None, Pool.Managed);
            textura_guardrail = Texture.FromBitmap(d3dDevice, (Bitmap)Bitmap.FromFile(MyMediaDir + "f1\\guardrail.png"), Usage.None, Pool.Managed);
            textura_cartel = Texture.FromBitmap(d3dDevice, (Bitmap)Bitmap.FromFile(MyMediaDir + "f1\\cartel1.png"), Usage.None, Pool.Managed);
        }


        public void render(Effect effect)
        {
            Device device = GuiController.Instance.D3dDevice;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vb, 0);

            // primero dibujo los objetos opacos:
            // piso de la ruta
            effect.SetValue("texDiffuseMap", textura_piso);
            device.RenderState.AlphaBlendEnable = false;
            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2 * (cant_ptos_ruta - 1));
                effect.EndPass();
            }
            effect.End();

            // vegetacion
            if (pos_en_ruta != -1)
            {
                float[] K = new float[10];
                for (int i = 0; i < cant_arboles; ++i)
                {
                    K[i] = 1;
                    arbol[i].Effect = effect;
                    arbol[i].Technique = "DefaultTechnique";
                }

                int[] P = new int[10];
                P[0] = 11;
                P[1] = 7;
                P[2] = 13;
                P[3] = 5;


                float dr = ancho_ruta / 2;
                for (int i = 0; i < cant_ptos_ruta; ++i)
                {
                    Vector3 dir = pt_ruta[i + 1] - pt_ruta[i];
                    dir.Normalize();
                    Vector3 n = Vector3.Cross(dir, new Vector3(0, 1, 0));
                    Vector3 p0 = pt_ruta[i] - n * dr;
                    Vector3 p1 = pt_ruta[i] + n * dr;

                    for (int j = 0; j < cant_arboles; ++j)
                    {
                        if (i % P[j] == 0)
                        {
                            Vector3 pos = pt_ruta[i] - n * 300 * K[j];
                            pos.Y = 0;
                            arbol[j].Position = pos;
                            arbol[j].render();
                            K[j] *= -1;
                        }
                    }
                }
            }

            // Ahora los objetos transparentes (el guarda rail, y los carteles)
            // guarda rail
            device.RenderState.AlphaBlendEnable = true;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vb, 0);
            effect.SetValue("texDiffuseMap", textura_guardrail);
            numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 2 * cant_ptos_ruta, 2 * (cant_ptos_ruta - 1));
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 4 * cant_ptos_ruta, 2 * (cant_ptos_ruta - 1));
                effect.EndPass();
            }
            effect.End();

            // carteles
            effect.SetValue("texDiffuseMap", textura_cartel);
            numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                for (int i = 0; i < cant_carteles; ++i)
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, pos_carteles + 4 * i, 2);
                effect.EndPass();
            }
            effect.End();

        }




        public float updatePos(float x,float z)
        {
	        // Verifico si el pto x,z esta cerca de la ruta
	        // busco todos los puntos de la ruta cercanos a Pos
	        float mdist = ancho_ruta/2 + ancho_guarray;
	        int aux_tramo = -1;
	        en_ruta = false;
	        int cant_p = 0;
	        int []ndx = new int[700];
	        for(int i=0; i<cant_ptos_ruta-1;++i)
		        if(Math.Abs(x-pt_ruta[i].X)<mdist+5 && Math.Abs(z-pt_ruta[i].Z)<mdist+5)
			        // es un pto posible
			        ndx[cant_p++] = i;
	
	        if(cant_p==0)
		        return 0;			// nivel del suelo

	        float dr = ancho_ruta/2;
            float H = 0;
	        Vector2 p = new Vector2(x,z);
	        for(int t = 0;t<cant_p;++t)
	        {
		        int i = ndx[t];

		        Vector2 r0 = new Vector2(pt_ruta[i].X, pt_ruta[i].Z);
                Vector2 r1 = new Vector2(pt_ruta[i+1].X, pt_ruta[i+1].Z);
		        Vector2 r = r1-r0;
		        float rm = r.Length();
                r.Normalize();
		        float d = Vector2.Dot(p-r0, r);
		        // d ==0 , rm
		
		        if(d>=-0.5 && d<=rm+0.5)
		        {
			        Vector2 rc = r0 + r*d;
                    float dist = (rc - p).Length();
			        if(dist<mdist)
			        {
				        aux_tramo = i;
				        mdist = dist;
				        // interpolo la altura de la ruta
				        float k = d/rm;
				        if(k<0)
					        k = 0;
				        else
				        if(k>1)
					        k = 1;
				        float Hruta = pt_ruta[i].Y*(1-k) + pt_ruta[i+1].Y*k;
				        if(dist<=dr)
				        {
					        // esta en la ruta
					        en_ruta = true;
					        H = Hruta;
				        }
				        else
				        {
					        // esta en el guarray
					        en_ruta = false;
					        H = (1-(dist-dr)/ancho_guarray)*Hruta;
				        }
			        }
		        }
	        }

            // Actualizo el status
            Hpiso = H;
            pos_en_ruta = aux_tramo;

	        return H ;
        }


        // se fue de la ruta, devuelve que posicion mas  cercana en el centro de la ruta 
        public Vector3 que_pos_buena(float x, float z)
        {
            float mdist = 10000000000f;
            int aux_tramo = -1;
            float dr = ancho_ruta / 2;
            float H = 0;
            Vector2 p = new Vector2(x, z);
            for (int i = 0; i < cant_ptos_ruta; ++i)
            {
                Vector2 r0 = new Vector2(pt_ruta[i].X, pt_ruta[i].Z);
                Vector2 r1 = new Vector2(pt_ruta[i + 1].X, pt_ruta[i + 1].Z);
                Vector2 r = r1 - r0;
                float rm = r.Length();
                r.Normalize();
                float d = Vector2.Dot(p - r0, r);
                // d ==0 , rm

                if (d >= -0.5 && d <= rm + 0.5)
                {
                    Vector2 rc = r0 + r * d;
                    float dist = (rc - p).Length();
                    if (dist < mdist)
                    {
                        aux_tramo = i;
                        mdist = dist;
                    }
                }
            }

            if(aux_tramo!=-1)
            {
                x = pt_ruta[aux_tramo].X;
                z = pt_ruta[aux_tramo].Z;
            }


            return new Vector3(x, updatePos(x,z), z);
        }


        public void dispose()
        {
            if (vb != null)
            {
                vb.Dispose();
            }
            if (textura_piso != null)
            {
                textura_piso.Dispose();
            }
            if (textura_guardrail != null)
            {
                textura_guardrail.Dispose();
            }
            if (textura_cartel != null)
            {
                textura_cartel.Dispose();
            }

            for (int i = 0; i < cant_arboles; ++i)
                arbol[i].dispose();
        }
    }
}
