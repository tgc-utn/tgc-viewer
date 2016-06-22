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

// variable de fogs
float4 ColorFog;
float4 CameraPos;
float StartFogDistance;
float EndFogDistance;
float Density;

//Input del Vertex Shader
struct VS_INPUT_VERTEX
{
	float4 Position : POSITION0;
	float3 Texture : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX
{
	float4 Position : POSITION0;
	float2 Texture:    TEXCOORD0;
	float1 Fog:     FOG;
};

//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
	VS_OUTPUT_VERTEX output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);
	output.Texture = input.Texture;
	float4 CameraPosWorld = mul(CameraPos, matWorld);

	//Calcula fog lineal
	//output.Fog = saturate((EndFogDistance - CameraPosWorld.z) / (EndFogDistance - StartFogDistance));
	//calcula fog Exponencial
	float DistFog = distance(input.Position.xyz, CameraPosWorld.xyz);
	output.Fog = saturate(exp((StartFogDistance - DistFog)*Density));
	//Calcula fog exponencial 2
	//output.Fog = saturate(exp((StartFogDistance-DistFog)*Density*(StartFogDistance-DistFog)*Density));
	return output;
}

//Input del Pixel Shader
struct PS_INPUT_PIXEL
{
	float2 Texture:    TEXCOORD0;
	float1 Fog:     FOG;
};

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
	// Obtener el texel de textura
	// diffuseMap es el sampler, Texcoord son las coordenadas interpoladas
	float4 fvBaseColor = tex2D(diffuseMap, input.Texture);
	// combino fog y textura
	float4 fogFactor = float4(input.Fog,input.Fog,input.Fog,input.Fog);
	float4 fvFogColor = (1.0 - fogFactor) * ColorFog;
	return fogFactor * fvBaseColor + fvFogColor;
}

// ------------------------------------------------------------------
technique RenderScene
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_main();
	}
}