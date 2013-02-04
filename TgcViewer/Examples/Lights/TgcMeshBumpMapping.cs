using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Lights
{
    /// <summary>
    /// Mesh para ser utilizado en efectos de BumpMapping.
    /// El efecto de BumpMapping requiere que cada vertice tenga ademas de la normal, la tangente y la binormal.
    /// Esta clase tiene utilidades para calcular estos dos vectores.
    /// Extendemos de TgcMesh para poder redefinir el método render() y agregar datos
    /// de tangent y binormal al VertexBuffer que son necesarios para el efecto de BumpMapping
    /// 
    /// </summary>
    public class TgcMeshBumpMapping : TgcMesh
    {

        TgcTexture[] normalMaps;
        /// <summary>
        /// Mapa de normales para BumpMapping, uno para cada subset del mesh
        /// </summary>
        public TgcTexture[] NormalMaps
        {
            get { return normalMaps; }
            set { normalMaps = value; }
        }


        public TgcMeshBumpMapping(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, renderType)
        {
        }

        public TgcMeshBumpMapping(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }


        /// <summary>
        /// Se redefine este método para agregar shaders.
        /// Es el mismo código del render() pero con la sección de "MeshRenderType.DIFFUSE_MAP" ampliada
        /// para Shaders.
        /// </summary>
        public new void render()
        {
            if (!enabled)
                return;

            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformacion de malla
            updateMeshTransform();

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;

            //Activar AlphaBlending
            activateAlphaBlend();

            //Cargar matrices para el shader
            setShaderMatrix();

            //Renderizar segun el tipo de render de la malla
            effect.Technique = this.technique;
            int numPasses = effect.Begin(0);
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:

                    throw new Exception("Caso no contemplado para BumpMapping");

                case MeshRenderType.DIFFUSE_MAP:

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (int n = 0; n < numPasses; n++)
                    {

                        //Dibujar cada subset con su Material y DiffuseMap correspondiente
                        for (int i = 0; i < materials.Length; i++)
                        {
                            device.Material = materials[i];
                            
                            //Setear textura en shader
                            texturesManager.shaderSet(effect, "texDiffuseMap", diffuseMaps[i]);

                            //Setear normalMap en shader
                            texturesManager.shaderSet(effect, "texNormalMap", normalMaps[i]);

                            //Iniciar pasada de shader
                            // guarda: Todos los SetValue tienen que ir ANTES del beginPass.
                            // si no hay que llamar effect.CommitChanges para que tome el dato!
                            effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            effect.EndPass();
                        }
                    }

                    //Finalizar shader
                    effect.End();

                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:

                    throw new Exception("Caso no contemplado para BumpMappingo");
            }

            //Finalizar shader
            effect.End();

            //Activar AlphaBlending
            resetAlphaBlend();

        }

        /// <summary>
        /// Crear un TgcMeshBumpMapping en base a un TgcMesh y su normalMap.
        /// Solo esta soportado un TgcMehs MeshRenderType = DiffuseMap
        /// </summary>
        public static TgcMeshBumpMapping fromTgcMesh(TgcMesh mesh, TgcTexture[] normalMaps)
        {
            if (mesh.RenderType != MeshRenderType.DIFFUSE_MAP)
            {
                throw new Exception("Solo esta soportado MeshRenderType = DiffuseMap");
            }


            //Obtener vertexBuffer original
            TgcSceneLoader.DiffuseMapVertex[] origVertexBuffer = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
            mesh.D3dMesh.UnlockVertexBuffer();

            //Crear nuevo Mesh de DirectX
            int triCount = origVertexBuffer.Length / 3;
            Mesh d3dMesh = new Mesh(triCount, origVertexBuffer.Length, MeshFlags.Managed, BumpMappingVertexElements, GuiController.Instance.D3dDevice);

            //Calcular normales recorriendo los triangulos
            Vector3[] normals = new Vector3[origVertexBuffer.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector3(0, 0, 0);
            }
            for (int i = 0; i < triCount; i++)
            {
                //Los 3 vertices del triangulo
                TgcSceneLoader.DiffuseMapVertex v1 = origVertexBuffer[i * 3];
                TgcSceneLoader.DiffuseMapVertex v2 = origVertexBuffer[i * 3 + 1];
                TgcSceneLoader.DiffuseMapVertex v3 = origVertexBuffer[i * 3 + 2];

                //Face-normal (left-handend)
                Vector3 a = v2.Position - v1.Position;
                Vector3 b = v3.Position - v1.Position;
                Vector3 n = Vector3.Cross(a, b);

                //Acumular normal del vertice segun todas sus Face-normal
                normals[i * 3] += n;
                normals[i * 3 + 1] += n;
                normals[i * 3 + 2] += n;
            }

            //Normalizar normales
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
            }

            //Crear nuevo VertexBuffer
            using (VertexBuffer vb = d3dMesh.VertexBuffer)
            {
                //Iterar sobre triangulos
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                for (int i = 0; i < triCount; i++)
                {
                    //Vertices originales
                    TgcSceneLoader.DiffuseMapVertex vOrig1 = origVertexBuffer[i * 3];
                    TgcSceneLoader.DiffuseMapVertex vOrig2 = origVertexBuffer[i * 3 + 1];
                    TgcSceneLoader.DiffuseMapVertex vOrig3 = origVertexBuffer[i * 3 + 2];

                    //Nuevo vertice 1
                    BumpMappingVertex v1 = new BumpMappingVertex();
                    v1.Position = vOrig1.Position;
                    v1.Color = vOrig1.Color;
                    v1.Tu = vOrig1.Tu;
                    v1.Tv = vOrig1.Tv;
                    v1.Normal = normals[i * 3];

                    //Nuevo vertice 2
                    BumpMappingVertex v2 = new BumpMappingVertex();
                    v2.Position = vOrig2.Position;
                    v2.Color = vOrig2.Color;
                    v2.Tu = vOrig2.Tu;
                    v2.Tv = vOrig2.Tv;
                    v2.Normal = normals[i * 3 + 1];

                    //Nuevo vertice 3
                    BumpMappingVertex v3 = new BumpMappingVertex();
                    v3.Position = vOrig3.Position;
                    v3.Color = vOrig3.Color;
                    v3.Tu = vOrig3.Tu;
                    v3.Tv = vOrig3.Tv;
                    v3.Normal = normals[i * 3 + 2];

                    //Calcular tangente y binormal para todo el triangulo y cargarlas en cada vertice
                    Vector3 tangent;
                    Vector3 binormal;
                    TgcMeshBumpMapping.computeTangentBinormal(v1, v2, v3, out tangent, out binormal);
                    v1.Tangent = tangent;
                    v1.Binormal = binormal;
                    v2.Tangent = tangent;
                    v2.Binormal = binormal;
                    v3.Tangent = tangent;
                    v3.Binormal = binormal;

                    //Cargar VertexBuffer
                    data.Write(v1);
                    data.Write(v2);
                    data.Write(v3);
                }

                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (IndexBuffer ib = d3dMesh.IndexBuffer)
            {
                short[] indices = new short[origVertexBuffer.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = (short)i;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Clonar texturas y materials
            TgcTexture[] diffuseMaps = new TgcTexture[mesh.DiffuseMaps.Length];
            Material[] materials = new Material[mesh.Materials.Length];
            for (int i = 0; i < mesh.DiffuseMaps.Length; i++)
			{
                diffuseMaps[i] = mesh.DiffuseMaps[i].clone();
                materials[i] = TgcD3dDevice.DEFAULT_MATERIAL;
			}

            //Cargar attributeBuffer
            if (diffuseMaps.Length > 1)
            {
                int[] origAttributeBuffer = mesh.D3dMesh.LockAttributeBufferArray(LockFlags.None);
                int[] newAttributeBuffer = d3dMesh.LockAttributeBufferArray(LockFlags.None);
                Array.Copy(origAttributeBuffer, newAttributeBuffer, origAttributeBuffer.Length);
                mesh.D3dMesh.UnlockAttributeBuffer();
                d3dMesh.UnlockAttributeBuffer(newAttributeBuffer);
            }


            //Crear mesh de BumpMapping Mesh
            TgcMeshBumpMapping bumpMesh = new TgcMeshBumpMapping(d3dMesh, mesh.Name, mesh.RenderType);
            bumpMesh.diffuseMaps = diffuseMaps;
            bumpMesh.materials = materials;
            bumpMesh.normalMaps = normalMaps;
            bumpMesh.layer = mesh.Layer;
            bumpMesh.alphaBlendEnable = mesh.AlphaBlendEnable;
            bumpMesh.UserProperties = mesh.UserProperties;
            bumpMesh.boundingBox = mesh.BoundingBox.clone();
            bumpMesh.enabled = true;

            return bumpMesh;
        }

        /// <summary>
        /// Calcular Tangent y Binormal en base a los 3 vertices de un triangulo y la normal del primero de ellos
        /// Basado en: http://www.dhpoware.com/demos/d3d9NormalMapping.html
        /// </summary>
        public static void computeTangentBinormal(BumpMappingVertex v1, BumpMappingVertex v2, BumpMappingVertex v3, out Vector3 tangent, out Vector3 binormal)
        {
            // Given the 3 vertices (position and texture coordinates) of a triangle
            // calculate and return the triangle's tangent vector. The handedness of
            // the local coordinate system is stored in tangent.w. The bitangent is
            // then: float3 bitangent = cross(normal, tangent.xyz) * tangent.w.

            // Create 2 vectors in object space.
            //
            // edge1 is the vector from vertex positions v1 to v2.
            // edge2 is the vector from vertex positions v1 to v3.
            Vector3 edge1 = v2.Position - v1.Position;
            Vector3 edge2 = v3.Position - v1.Position;
            edge1.Normalize();
            edge2.Normalize();

            // Create 2 vectors in tangent (texture) space that point in the same
            // direction as edge1 and edge2 (in object space).
            //
            // texEdge1 is the vector from texture coordinates texCoord1 to texCoord2.
            // texEdge2 is the vector from texture coordinates texCoord1 to texCoord3.
            Vector2 texEdge1 = new Vector2(v2.Tu - v1.Tu, v2.Tv - v1.Tv);
            Vector2 texEdge2 = new Vector2(v3.Tu - v1.Tu, v3.Tv - v1.Tv);
            texEdge1.Normalize();
            texEdge2.Normalize();

            // These 2 sets of vectors form the following system of equations:
            //
            //  edge1 = (texEdge1.x * tangent) + (texEdge1.y * bitangent)
            //  edge2 = (texEdge2.x * tangent) + (texEdge2.y * bitangent)
            //
            // Using matrix notation this system looks like:
            //
            //  [ edge1 ]     [ texEdge1.x  texEdge1.y ]  [ tangent   ]
            //  [       ]  =  [                        ]  [           ]
            //  [ edge2 ]     [ texEdge2.x  texEdge2.y ]  [ bitangent ]
            //
            // The solution is:
            //
            //  [ tangent   ]        1     [ texEdge2.y  -texEdge1.y ]  [ edge1 ]
            //  [           ]  =  -------  [                         ]  [       ]
            //  [ bitangent ]      det A   [-texEdge2.x   texEdge1.x ]  [ edge2 ]
            //
            //  where:
            //        [ texEdge1.x  texEdge1.y ]
            //    A = [                        ]
            //        [ texEdge2.x  texEdge2.y ]
            //
            //    det A = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)
            //
            // From this solution the tangent space basis vectors are:
            //
            //    tangent = (1 / det A) * ( texEdge2.y * edge1 - texEdge1.y * edge2)
            //  bitangent = (1 / det A) * (-texEdge2.x * edge1 + texEdge1.x * edge2)
            //     normal = cross(tangent, bitangent)

            float det = (texEdge1.X * texEdge2.Y) - (texEdge1.Y * texEdge2.X);

            if (FastMath.Abs(det) < 0.0001f)    // almost equal to zero
            {
                tangent.X = 1.0f;
                tangent.Y = 0.0f;
                tangent.Z = 0.0f;

                binormal.X = 0.0f;
                binormal.Y = 1.0f;
                binormal.Z = 0.0f;
            }
            else
            {
                det = 1.0f / det;

                tangent.X = (texEdge2.Y * edge1.X - texEdge1.Y * edge2.X) * det;
                tangent.Y = (texEdge2.Y * edge1.Y - texEdge1.Y * edge2.Y) * det;
                tangent.Z = (texEdge2.Y * edge1.Z - texEdge1.Y * edge2.Z) * det;
                //tangent.W = 0.0f;
                
                binormal.X = (-texEdge2.X * edge1.X + texEdge1.X * edge2.X) * det;
                binormal.Y = (-texEdge2.X * edge1.Y + texEdge1.X * edge2.Y) * det;
                binormal.Z = (-texEdge2.X * edge1.Z + texEdge1.X * edge2.Z) * det;

                tangent.Normalize();
                binormal.Normalize();
            }

            // Calculate the handedness of the local tangent space.
            // The bitangent vector is the cross product between the triangle face
            // normal vector and the calculated tangent vector. The resulting bitangent
            // vector should be the same as the bitangent vector calculated from the
            // set of linear equations above. If they point in different directions
            // then we need to invert the cross product calculated bitangent vector.
            Vector3 b = Vector3.Cross(v1.Normal, tangent);
            float w = Vector3.Dot(b, binormal) < 0.0f ? -1.0f : 1.0f;
            binormal = b * w;
        }

     

        /// <summary>
        /// FVF para formato de malla con Bump Mapping
        /// </summary>
        public static readonly VertexElement[] BumpMappingVertexElements = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                                    DeclarationMethod.Default,
                                    DeclarationUsage.Position, 0),
                                    
            new VertexElement(0, 12, DeclarationType.Float3,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Normal, 0),

            new VertexElement(0, 24, DeclarationType.Color,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Color, 0),

            new VertexElement(0, 28, DeclarationType.Float2,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.TextureCoordinate, 0),

            new VertexElement(0, 36,DeclarationType.Float3,
                            DeclarationMethod.Default,
                            DeclarationUsage.Tangent, 0),

            new VertexElement(0, 48,DeclarationType.Float3,
                            DeclarationMethod.Default,
                            DeclarationUsage.BiNormal, 0),

            VertexElement.VertexDeclarationEnd 
        };


        /// <summary>
        /// Estructura de Vertice para formato de malla con Bump Mapping
        /// </summary>
        public struct BumpMappingVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;
            public float Tu;
            public float Tv;
            public Vector3 Tangent;
            public Vector3 Binormal;
        }

    }

    /// <summary>
    /// Factory customizado para poder crear clase TgcMeshBumpMapping
    /// </summary>
    public class TgcMeshBumpMappingFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new TgcMeshBumpMapping(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new TgcMeshBumpMapping(meshName, originalMesh, translation, rotation, scale);
        }
    }
}
