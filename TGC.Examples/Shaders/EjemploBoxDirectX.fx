/*
* Shader utilizado en el ejemplo DirectX/EjemploBoxDirectX
*/

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura 0
texture tex0;
sampler2D sampler0 = sampler_state
{
	Texture = (tex0);
};

//Textura 1
texture tex1;
sampler2D sampler1 = sampler_state
{
	Texture = (tex1);
};

//Textura 2
texture tex2;
sampler2D sampler2 = sampler_state
{
	Texture = (tex2);
};

/**************************************************************************************/
/* EjemploBoxDirectX */
/**************************************************************************************/

//Input del Vertex Shader. Tiene que coincidir con el VertexDeclaration del ejemplo
struct VS_INPUT
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 texcoord0 : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	float3 auxValue1 : TEXCOORD3;
	float3 auxValue2 : TEXCOORD4;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 texcoord0 : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	float3 auxValue1 : TEXCOORD3;
	float3 auxValue2 : TEXCOORD4;
};

//Vertex Shader
VS_OUTPUT vs_EjemploBoxDirectX(VS_INPUT input)
{
	VS_OUTPUT output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);
	output.Normal = mul(input.Normal, matWorldViewProj);

	//Enviar el resto de los valores directamente al pixel shader
	output.Color = input.Color;
	output.texcoord0 = input.texcoord0;
	output.texcoord1 = input.texcoord1;
	output.texcoord2 = input.texcoord2;
	output.auxValue1 = input.auxValue1;
	output.auxValue2 = input.auxValue2;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT
{
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 texcoord0 : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	float3 auxValue1 : TEXCOORD3;
	float3 auxValue2 : TEXCOORD4;
};

//Pixel Shader
float4 ps_EjemploBoxDirectX(PS_INPUT input) : COLOR0
{
	//Obtener texel de las 3 texturas (usamos cualquier uv)
	float4 texel0 = tex2D(sampler0, input.texcoord0);
	float4 texel1 = tex2D(sampler1, input.texcoord1);
	float4 texel2 = tex2D(sampler2, input.texcoord2);

	//return float4(input.Normal, 1);
	return input.Color;
}

//technique EjemploBoxDirectX
technique EjemploBoxDirectX
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_EjemploBoxDirectX();
		PixelShader = compile ps_3_0 ps_EjemploBoxDirectX();
	}
}