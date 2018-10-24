using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;

namespace TGC.Examples.ShadersExamples
{
    public class F1Circuit
    {
        public float ancho_guarray = 3;
        public float ancho_ruta = 200;

        public TgcMesh[] arbol = new TgcMesh[10];
        public int cant_arboles;
        public int cant_carteles;
        public int cant_ptos_ruta;
        public float dh = 3; // alto de la pared
        public bool en_ruta;
        public float Hpiso; // Altura del piso en la Pos
        public float M_PI = 3.14151f;
        public int pos_carteles;
        public int pos_en_ruta;
        public TGCVector3[] pt_ruta = new TGCVector3[500];
        public float scaleXZ = 20;
        public float scaleY = 15;
        public Texture textura_cartel;
        public Texture textura_guardrail;
        public Texture textura_piso;
        public int totalVertices;
        private VertexBuffer vb;

        public F1Circuit(string mediaDir)
        {
            CrearRuta(mediaDir);

            var loader = new TgcSceneLoader();

            arbol[cant_arboles] =
                loader.loadSceneFromFile(mediaDir +
                                         "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml")
                    .Meshes[0];
            cant_arboles++;

            arbol[cant_arboles] =
                loader.loadSceneFromFile(mediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera3\\Palmera3-TgcScene.xml")
                    .Meshes[0];
            arbol[cant_arboles].Scale = new TGCVector3(2, 2, 2);
            ++cant_arboles;

            arbol[cant_arboles] =
                loader.loadSceneFromFile(mediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera2\\Palmera2-TgcScene.xml")
                    .Meshes[0];
            arbol[cant_arboles].Scale = TGCVector3.One;
            ++cant_arboles;

            arbol[cant_arboles] =
                loader.loadSceneFromFile(mediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml").Meshes[0
                    ];
            arbol[cant_arboles].Scale = new TGCVector3(4, 4, 4);
            ++cant_arboles;
        }

        // Carga los ptos de la ruta
        public int load_pt_ruta()
        {
            // Genero el path de la ruta
            double dt = M_PI / 64;
            double t = 0;
            double hasta = 2 * M_PI;
            var cant = 0;
            //float dw = 10000;
            while (t < hasta + 0.1)
            {
                pt_ruta[cant].X = (float)(8 * (8 * Math.Cos(t) + Math.Cos(5 * t) * Math.Cos(t))) * scaleXZ;
                pt_ruta[cant].Z = (float)(8 * (8 * Math.Sin(t) + Math.Cos(4 * t) * Math.Sin(t))) * scaleXZ;
                pt_ruta[cant].Y = (float)Math.Max(0, 3 + 2 * Math.Cos(3 + t * 5)) * scaleY;
                t += dt;
                ++cant;
            }
            --cant; // me aseguro que siempre exista el i+1

            return cant;
        }

        public void CrearRuta(string mediaDir)
        {
            //Dispose de VertexBuffer anterior, si habia
            if (vb != null && !vb.Disposed)
            {
                vb.Dispose();
            }

            var Kr = 0.5f;
            var dr = ancho_ruta / 2;

            // Cargo la ruta
            cant_ptos_ruta = load_pt_ruta();
            cant_carteles = 6;
            var dc = cant_ptos_ruta / cant_carteles;

            //Crear vertexBuffer
            totalVertices = cant_ptos_ruta * 6 + cant_carteles * 4;
            vb = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            var dataIdx = 0;
            var data = new CustomVertex.PositionTextured[totalVertices];

            // piso
            for (var i = 0; i < cant_ptos_ruta; ++i)
            {
                var dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                var n = TGCVector3.Cross(dir, TGCVector3.Up);
                var p0 = pt_ruta[i] - n * dr;
                var p1 = pt_ruta[i] + n * dr;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, 1, i * Kr);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, 0, i * Kr);
            }

            // pared izquierda
            for (var i = 0; i < cant_ptos_ruta; ++i)
            {
                var dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                var n = TGCVector3.Cross(dir, TGCVector3.Up);
                var u = TGCVector3.Cross(n, dir);
                var p0 = pt_ruta[i] - n * (dr + ancho_guarray);
                var p1 = pt_ruta[i] - n * dr;
                p0.Y -= 25;
                p1.Y += 25;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, i * Kr, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, i * Kr, 0);
            }

            // pared derecha
            for (var i = 0; i < cant_ptos_ruta; ++i)
            {
                var dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                var n = TGCVector3.Cross(dir, TGCVector3.Up);
                var u = TGCVector3.Cross(n, dir);
                var p0 = pt_ruta[i] + n * (dr + ancho_guarray);
                var p1 = pt_ruta[i] + n * dr;
                p0.Y -= 25;
                p1.Y += 25;

                data[dataIdx++] = new CustomVertex.PositionTextured(p0, i * Kr, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, i * Kr, 0);
            }

            // Carteles
            pos_carteles = dataIdx;
            for (var t = 0; t < cant_carteles; ++t)
            {
                var i = t * dc;
                var dir = pt_ruta[i + 1] - pt_ruta[i];
                dir.Normalize();
                var up = TGCVector3.Up;
                var n = TGCVector3.Cross(dir, up);
                var p0 = pt_ruta[i] - n * (dr + 50) + up * 0;
                var p1 = pt_ruta[i] - n * (dr + 50) + up * 170;
                var p2 = pt_ruta[i] + n * (dr + 50) + up * 170;
                var p3 = pt_ruta[i] + n * (dr + 50) + up * 0;
                data[dataIdx++] = new CustomVertex.PositionTextured(p0, 1, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p3, 0, 1);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, 1, 0);
                data[dataIdx++] = new CustomVertex.PositionTextured(p2, 0, 0);
            }

            vb.SetData(data, 0, LockFlags.None);

            // Cargo la textura del piso
            loadTextures(mediaDir);
        }

        /// <summary>
        ///     Carga la textura del terreno
        /// </summary>
        public void loadTextures(string mediaDir)
        {
            //Dispose textura anterior, si habia
            if (textura_piso != null && !textura_piso.Disposed)
            {
                textura_piso.Dispose();
            }

            var d3dDevice = D3DDevice.Instance.Device;

            var MyMediaDir = mediaDir + "Texturas\\f1\\";
            textura_piso = Texture.FromBitmap(d3dDevice, (Bitmap)Image.FromFile(MyMediaDir + "f1piso2.png"), Usage.None,
                Pool.Managed);
            textura_guardrail = Texture.FromBitmap(d3dDevice, (Bitmap)Image.FromFile(MyMediaDir + "guardrail.png"),
                Usage.None, Pool.Managed);
            textura_cartel = Texture.FromBitmap(d3dDevice, (Bitmap)Image.FromFile(MyMediaDir + "cartel1.png"),
                Usage.None, Pool.Managed);
        }

        public void render(Effect effect)
        {
            var device = D3DDevice.Instance.Device;
            TGCShaders.Instance.SetShaderMatrixIdentity(effect);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vb, 0);

            // primero dibujo los objetos opacos:
            // piso de la ruta
            effect.SetValue("texDiffuseMap", textura_piso);
            device.RenderState.AlphaBlendEnable = false;
            var numPasses = effect.Begin(0);
            for (var n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2 * (cant_ptos_ruta - 1));
                effect.EndPass();
            }
            effect.End();

            // vegetacion
            if (pos_en_ruta != -1)
            {
                var K = new float[10];
                for (var i = 0; i < cant_arboles; ++i)
                {
                    K[i] = 1;
                    arbol[i].Effect = effect;
                    arbol[i].Technique = "DefaultTechnique";
                }

                var P = new int[10];
                P[0] = 11;
                P[1] = 7;
                P[2] = 13;
                P[3] = 5;

                var dr = ancho_ruta / 2;
                for (var i = 0; i < cant_ptos_ruta; ++i)
                {
                    var dir = pt_ruta[i + 1] - pt_ruta[i];
                    dir.Normalize();
                    var n = TGCVector3.Cross(dir, TGCVector3.Up);
                    var p0 = pt_ruta[i] - n * dr;
                    var p1 = pt_ruta[i] + n * dr;

                    for (var j = 0; j < cant_arboles; ++j)
                    {
                        if (i % P[j] == 0)
                        {
                            var pos = pt_ruta[i] - n * 300 * K[j];
                            pos.Y = 0;
                            arbol[j].Position = pos;
                            arbol[j].UpdateMeshTransform();
                            arbol[j].Render();
                            K[j] *= -1;
                        }
                    }
                }
            }

            // Ahora los objetos transparentes (el guarda rail, y los carteles)
            // guarda rail
            device.RenderState.AlphaBlendEnable = true;
            TGCShaders.Instance.SetShaderMatrixIdentity(effect);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vb, 0);
            effect.SetValue("texDiffuseMap", textura_guardrail);
            numPasses = effect.Begin(0);
            for (var n = 0; n < numPasses; n++)
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
            for (var n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                for (var i = 0; i < cant_carteles; ++i)
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, pos_carteles + 4 * i, 2);
                effect.EndPass();
            }
            effect.End();
        }

        public float updatePos(float x, float z)
        {
            // Verifico si el pto x,z esta cerca de la ruta
            // busco todos los puntos de la ruta cercanos a Pos
            var mdist = ancho_ruta / 2 + ancho_guarray;
            var aux_tramo = -1;
            en_ruta = false;
            var cant_p = 0;
            var ndx = new int[700];
            for (var i = 0; i < cant_ptos_ruta - 1; ++i)
                if (Math.Abs(x - pt_ruta[i].X) < mdist + 5 && Math.Abs(z - pt_ruta[i].Z) < mdist + 5)
                    // es un pto posible
                    ndx[cant_p++] = i;

            if (cant_p == 0)
                return 0; // nivel del suelo

            var dr = ancho_ruta / 2;
            float H = 0;
            var p = new TGCVector2(x, z);
            for (var t = 0; t < cant_p; ++t)
            {
                var i = ndx[t];

                var r0 = new TGCVector2(pt_ruta[i].X, pt_ruta[i].Z);
                var r1 = new TGCVector2(pt_ruta[i + 1].X, pt_ruta[i + 1].Z);
                var r = r1 - r0;
                var rm = r.Length();
                r.Normalize();
                var d = TGCVector2.Dot(p - r0, r);
                // d ==0 , rm

                if (d >= -0.5 && d <= rm + 0.5)
                {
                    var rc = r0 + r * d;
                    var dist = (rc - p).Length();
                    if (dist < mdist)
                    {
                        aux_tramo = i;
                        mdist = dist;
                        // interpolo la altura de la ruta
                        var k = d / rm;
                        if (k < 0)
                            k = 0;
                        else if (k > 1)
                            k = 1;
                        var Hruta = pt_ruta[i].Y * (1 - k) + pt_ruta[i + 1].Y * k;
                        if (dist <= dr)
                        {
                            // esta en la ruta
                            en_ruta = true;
                            H = Hruta;
                        }
                        else
                        {
                            // esta en el guarray
                            en_ruta = false;
                            H = (1 - (dist - dr) / ancho_guarray) * Hruta;
                        }
                    }
                }
            }

            // Actualizo el status
            Hpiso = H;
            pos_en_ruta = aux_tramo;

            return H;
        }

        // se fue de la ruta, devuelve que posicion mas  cercana en el centro de la ruta
        public TGCVector3 que_pos_buena(float x, float z)
        {
            var mdist = 10000000000f;
            var aux_tramo = -1;
            var dr = ancho_ruta / 2;
            //float H = 0;
            var p = new TGCVector2(x, z);
            for (var i = 0; i < cant_ptos_ruta; ++i)
            {
                var r0 = new TGCVector2(pt_ruta[i].X, pt_ruta[i].Z);
                var r1 = new TGCVector2(pt_ruta[i + 1].X, pt_ruta[i + 1].Z);
                var r = r1 - r0;
                var rm = r.Length();
                r.Normalize();
                var d = TGCVector2.Dot(p - r0, r);
                // d ==0 , rm

                if (d >= -0.5 && d <= rm + 0.5)
                {
                    var rc = r0 + r * d;
                    var dist = (rc - p).Length();
                    if (dist < mdist)
                    {
                        aux_tramo = i;
                        mdist = dist;
                    }
                }
            }

            if (aux_tramo != -1)
            {
                x = pt_ruta[aux_tramo].X;
                z = pt_ruta[aux_tramo].Z;
            }

            return new TGCVector3(x, updatePos(x, z), z);
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

            for (var i = 0; i < cant_arboles; ++i)
                arbol[i].Dispose();
        }
    }
}