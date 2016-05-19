/*
* Shader utilizado en el ejemplo DirectX/EjemploGetZBuffer
*/

/*
* Shader genérico para TgcMesh.
* Hay 3 Techniques, una para cada MeshRenderType:
*	- VERTEX_COLOR
*	- DIFFUSE_MAP
*	- DIFFUSE_MAP_AND_LIGHTMAP
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

/**************************************************************************************/
/* GenerateZBuffer */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_GenerateZBuffer
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_GenerateZBuffer
{
	float4 Position :     POSITION0;
	float2 Depth    :     TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_GenerateZBuffer vs_GenerateZBuffer(VS_INPUT_GenerateZBuffer input)
{
	VS_OUTPUT_GenerateZBuffer output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Guardar Z y W proyectado para usar en el pixel shader
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;

	return output;
}

//Input del Pixel Shader
struct PS_GenerateZBuffer
{
	float2 Depth: TEXCOORD0;
};

//Pixel Shader
float4 ps_GenerateZBuffer(PS_GenerateZBuffer input) : COLOR0
{
	//Calcular depth como 1 - (Z / W) y grabar en la textura
	return  1 - (input.Depth.x / input.Depth.y);
}

/*
* Technique GenerateZBuffer
*/
technique GenerateZBuffer
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_GenerateZBuffer();
		PixelShader = compile ps_2_0 ps_GenerateZBuffer();
	}
}

/**************************************************************************************/
/* AlterColorByDepth */
/**************************************************************************************/

//Tamaño de la pantalla
float2 screenDimensions;

//Textura de zBuffer
texture texZBuffer;
sampler2D zBuffer = sampler_state
{
	Texture = (texZBuffer);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Input del Vertex Shader
struct VS_INPUT_AlterColorByDepth
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_AlterColorByDepth
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_AlterColorByDepth vs_AlterColorByDepth(VS_INPUT_AlterColorByDepth input)
{
	VS_OUTPUT_AlterColorByDepth output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;

	return output;
}

//Input del Pixel Shader
struct PS_AlterColorByDepth
{
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
	float2 Pos2D: VPOS; //Esta variable te la otorga automaticamente el shader. Es la posicion en pantalla
};

//Pixel Shader
float4 ps_AlterColorByDepth(PS_AlterColorByDepth input) : COLOR0
{
	//Obtener las coordenadas UV del ZBuffer. Son entre 0 y 1 entonces hay que dividir por las dimensiones de la pantalla
	float2 zBufferUV = input.Pos2D / screenDimensions;

	//Obtener valor del zBuffer
	float depth = tex2D(zBuffer, zBufferUV).r;

	//Color tradicional del mesh
	float4 meshColor = tex2D(diffuseMap, input.Texcoord);

	//Variar color segun la distancia
	float4 adaptedColor = meshColor * depth * 100;
	adaptedColor.r = min(adaptedColor.r, meshColor.r);
	adaptedColor.g = min(adaptedColor.g, meshColor.g);
	adaptedColor.b = min(adaptedColor.b, meshColor.b);
	adaptedColor.a = 1;
	return adaptedColor;
}

/*
* Technique AlterColorByDepth
*/
technique AlterColorByDepth
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_AlterColorByDepth();
		PixelShader = compile ps_3_0 ps_AlterColorByDepth();
	}
}