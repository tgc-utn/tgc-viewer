/*
* Shader genérico para TgcSkeletalMesh con iluminación dinámica por pixel (Phong Shading)
* utilizando un tipo de luz Point-Light con atenuación por distancia
* Hay 2 Techniques, una para cada MeshRenderType:
*	- VERTEX_COLOR
*	- DIFFUSE_MAP
*/

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Matrix Pallette para skinning
static const int MAX_MATRICES = 26;
float4x3 bonesMatWorldArray[MAX_MATRICES];

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB (tiene canal Alpha)
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Parametros de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float lightAttenuation; //Factor de atenuacion de la luz

/**************************************************************************************/
/* VERTEX_COLOR */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_VERTEX_COLOR
{
    float4 Position : POSITION0;
    float4 Color : COLOR;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
    float4 BlendWeights : BLENDWEIGHT;
    float4 BlendIndices : BLENDINDICES;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX_COLOR
{
    float4 Position : POSITION0;
    float4 Color : COLOR;
    float3 WorldNormal : TEXCOORD0;
    float3 WorldTangent : TEXCOORD1;
    float3 WorldBinormal : TEXCOORD2;
    float3 WorldPosition : TEXCOORD3;
    float3 LightVec : TEXCOORD4;
    float3 HalfAngleVec : TEXCOORD5;
};

//Vertex Shader
VS_OUTPUT_VERTEX_COLOR vs_VertexColor(VS_INPUT_VERTEX_COLOR input)
{
    VS_OUTPUT_VERTEX_COLOR output;

	//Pasar indices de float4 a array de int
    int BlendIndicesArray[4] = (int[4]) input.BlendIndices;

	//Skinning de posicion
    float3 skinPosition = mul(input.Position, bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;

	//Skinning de normal
    float3 skinNormal = mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldNormal = normalize(skinNormal);

	//Skinning de tangent
    float3 skinTangent = mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldTangent = normalize(skinTangent);

	//Skinning de binormal
    float3 skinBinormal = mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldBinormal = normalize(skinBinormal);

	//Proyectar posicion (teniendo en cuenta lo que se hizo por skinning)
    output.Position = mul(float4(skinPosition.xyz, 1.0), matWorldViewProj);

	//Enviar color directamente
    output.Color = input.Color;

	//Posicion pasada a World-Space (necesaria para atenuación por distancia)
    output.WorldPosition = mul(input.Position, matWorld);

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
    output.LightVec = lightPosition.xyz - output.WorldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
    float3 viewVector = eyePosition.xyz - output.WorldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
    output.HalfAngleVec = viewVector + output.LightVec;

    return output;
}

//Input del Pixel Shader
struct PS_INPUT_VERTEX_COLOR
{
    float4 Color : COLOR0;
    float3 WorldNormal : TEXCOORD0;
    float3 WorldPosition : TEXCOORD3;
    float3 LightVec : TEXCOORD4;
    float3 HalfAngleVec : TEXCOORD5;
};

//Pixel Shader
float4 ps_VertexColor(PS_INPUT_VERTEX_COLOR input) : COLOR0
{
	//Normalizar vectores
    float3 Nn = normalize(input.WorldNormal);
    float3 Ln = normalize(input.LightVec);
    float3 Hn = normalize(input.HalfAngleVec);

	//Calcular intensidad de luz, con atenuacion por distancia
    float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
    float intensity = lightIntensity / distAtten; //Dividimos intensidad sobre distancia (lo hacemos lineal pero tambien podria ser i/d^2)

	//Componente Ambient
    float3 ambientLight = intensity * lightColor * materialAmbientColor;

	//Componente Diffuse: N dot L
    float3 n_dot_l = dot(Nn, Ln);
    float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l); //Controlamos que no de negativo

	//Componente Specular: (N dot H)^exp
    float3 n_dot_h = dot(Nn, Hn);
    float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (intensity * lightColor * materialSpecularColor * pow(max(0.0, n_dot_h), materialSpecularExp));

	/* Color final: modular (Emissive + Ambient + Diffuse) por el color del mesh, y luego sumar Specular.
	   El color Alpha sale del diffuse material */
    float4 finalColor = float4(saturate(materialEmissiveColor + ambientLight + diffuseLight) * input.Color + specularLight, materialDiffuseColor.a);

    return finalColor;
}

/*
* Technique VERTEX_COLOR
*/
technique VERTEX_COLOR
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_VertexColor();
        PixelShader = compile ps_3_0 ps_VertexColor();
    }
}

/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
    float4 BlendWeights : BLENDWEIGHT;
    float4 BlendIndices : BLENDINDICES;
};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldTangent : TEXCOORD2;
    float3 WorldBinormal : TEXCOORD3;
    float3 WorldPosition : TEXCOORD4;
    float3 LightVec : TEXCOORD5;
    float3 HalfAngleVec : TEXCOORD6;
};

//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
    VS_OUTPUT_DIFFUSE_MAP output;

	//Pasar indices de float4 a array de int
    int BlendIndicesArray[4] = (int[4]) input.BlendIndices;

	//Skinning de posicion
    float3 skinPosition = mul(input.Position, bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;

	//Skinning de normal
    float3 skinNormal = mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinNormal += mul(input.Normal, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldNormal = normalize(skinNormal);

	//Skinning de tangent
    float3 skinTangent = mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinTangent += mul(input.Tangent, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldTangent = normalize(skinTangent);

	//Skinning de binormal
    float3 skinBinormal = mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
    skinBinormal += mul(input.Binormal, (float3x3) bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
    output.WorldBinormal = normalize(skinBinormal);

	//Proyectar posicion (teniendo en cuenta lo que se hizo por skinning)
    output.Position = mul(float4(skinPosition.xyz, 1.0), matWorldViewProj);

	//Enviar color directamente
    output.Color = input.Color;

	//Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

	//Posicion pasada a World-Space (necesaria para atenuación por distancia)
    output.WorldPosition = mul(input.Position, matWorld);

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
    output.LightVec = lightPosition.xyz - output.WorldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
    float3 viewVector = eyePosition.xyz - output.WorldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
    output.HalfAngleVec = viewVector + output.LightVec;

    return output;
}

//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD4;
    float3 LightVec : TEXCOORD5;
    float3 HalfAngleVec : TEXCOORD6;
};

//Pixel Shader
float4 ps_DiffuseMap(PS_DIFFUSE_MAP input) : COLOR0
{
	//Normalizar vectores
    float3 Nn = normalize(input.WorldNormal);
    float3 Ln = normalize(input.LightVec);
    float3 Hn = normalize(input.HalfAngleVec);

	//Calcular intensidad de luz, con atenuacion por distancia
    float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
    float intensity = lightIntensity / distAtten; //Dividimos intensidad sobre distancia (lo hacemos lineal pero tambien podria ser i/d^2)

	//Obtener texel de la textura
    float4 texelColor = tex2D(diffuseMap, input.Texcoord);

	//Componente Ambient
    float3 ambientLight = intensity * lightColor * materialAmbientColor;

	//Componente Diffuse: N dot L
    float3 n_dot_l = dot(Nn, Ln);
    float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l); //Controlamos que no de negativo

	//Componente Specular: (N dot H)^exp
    float3 n_dot_h = dot(Nn, Hn);
    float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (intensity * lightColor * materialSpecularColor * pow(max(0.0, n_dot_h), materialSpecularExp));

	/* Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	   El color Alpha sale del diffuse material */
    float4 finalColor = float4(saturate(materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight, materialDiffuseColor.a);

    return finalColor;
}

/*
* Technique DIFFUSE_MAP
*/
technique DIFFUSE_MAP
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_DiffuseMap();
        PixelShader = compile ps_3_0 ps_DiffuseMap();
    }
}