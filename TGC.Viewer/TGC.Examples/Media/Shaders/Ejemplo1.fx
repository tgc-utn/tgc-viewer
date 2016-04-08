/*
* Shader utilizado en ejemplo "EjemploShaderTgcMesh"
* Tiene varias techniques para hacer distintos tipos de efectos simples
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

float darkFactor;
float random;
float textureOffset;

/*********************************************** Vertex Shaders ***************************************************/

struct VS_OUTPUT
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT VS_main (
	float4 Position : POSITION,
	float3 Normal : NORMAL,
	float4 Color : COLOR,
	float2 TexCoord : TEXCOORD0
)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.Position = mul(Position, matWorldViewProj);
	Out.Color = Color;
	Out.TexCoord = TexCoord;
	
	return Out;
}

VS_OUTPUT VS_randomTexCoord (
	float4 Position : POSITION,
	float3 Normal : NORMAL,
	float4 Color : COLOR,
	float2 TexCoord : TEXCOORD0
)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.Position = mul(Position, matWorldViewProj);
	Out.Color = Color;
	Out.TexCoord = TexCoord + random;
	
	return Out;
}

VS_OUTPUT VS_randomColor (
	float4 Position : POSITION,
	float3 Normal : NORMAL,
	float4 Color : COLOR,
	float2 TexCoord : TEXCOORD0
)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.Position = mul(Position, matWorldViewProj);
	Out.Color = random;
	Out.TexCoord = TexCoord;
	
	return Out;
}

VS_OUTPUT VS_textureOffset (
	float4 Position : POSITION,
	float3 Normal : NORMAL,
	float4 Color : COLOR,
	float2 TexCoord : TEXCOORD0
)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.Position = mul(Position, matWorldViewProj);
	Out.Color = random;
	Out.TexCoord = TexCoord;
	Out.TexCoord[0] += textureOffset;
	
	return Out;
}


/*********************************************** Pixel Shaders ***************************************************/


float4 PS_onlyColor(VS_OUTPUT In): COLOR
{
	return In.Color;
}

float4 PS_onlyTexture(VS_OUTPUT In): COLOR
{
	return tex2D(diffuseMap, In.TexCoord);
}

float4 PS_darkening(VS_OUTPUT In): COLOR
{
	return darkFactor * tex2D(diffuseMap, In.TexCoord); 
}

float4 PS_complementing(VS_OUTPUT In): COLOR
{
	return 1 - tex2D(diffuseMap, In.TexCoord); 
}

float4 PS_maskRedOut(VS_OUTPUT In): COLOR
{
	float4 outColor = tex2D(diffuseMap, In.TexCoord); 
	outColor.r = 0.0f;
	return outColor;
}

float4 PS_redOnly(VS_OUTPUT In): COLOR
{
	float4 outColor = tex2D(diffuseMap, In.TexCoord); 
	outColor.bga = 0.0f;
	return outColor;
}


/*********************************************** Techniques ***************************************************/

technique OnlyTexture {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_onlyTexture();
	}
}

technique OnlyColor {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_onlyColor();
	}
}

technique Darkening {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_darkening();
	}
}

technique Complementing {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_complementing();
	}
}

technique MaskRedOut {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_maskRedOut();
	}
}

technique RedOnly {
	pass p0 {
		VertexShader = compile vs_2_0 VS_main();
		PixelShader = compile ps_2_0 PS_redOnly();
	}
}

technique RandomTexCoord {
	pass p0 {
		VertexShader = compile vs_2_0 VS_randomTexCoord();
		PixelShader = compile ps_2_0 PS_onlyTexture();
	}
}

technique RandomColorVS {
	pass p0 {
		VertexShader = compile vs_2_0 VS_randomColor();
		PixelShader = compile ps_2_0 PS_onlyColor();
	}
}

technique TextureOffset {
	pass p0 {
		VertexShader = compile vs_2_0 VS_textureOffset();
		PixelShader = compile ps_2_0 PS_onlyTexture();
	}
}

