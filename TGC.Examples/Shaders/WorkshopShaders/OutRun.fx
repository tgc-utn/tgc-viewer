// ---------------------------------------------------------
// Ejemplo toon Shading
// ---------------------------------------------------------

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
	ADDRESSU = MIRROR;
	ADDRESSV = MIRROR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;


//Input del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};

texture g_RenderTarget;
sampler RenderTarget = 
sampler_state
{
    Texture = <g_RenderTarget>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture g_RenderTarget2;
sampler RenderTarget2 = 
sampler_state
{
    Texture = <g_RenderTarget2>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture g_RenderTarget3;
sampler RenderTarget3 = 
sampler_state
{
    Texture = <g_RenderTarget3>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture g_RenderTarget4;
sampler RenderTarget4 = 
sampler_state
{
    Texture = <g_RenderTarget4>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture g_RenderTarget5;
sampler RenderTarget5 = 
sampler_state
{
    Texture = <g_RenderTarget5>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 Norm :          TEXCOORD1;			// Normales
   float3 Pos :   		TEXCOORD2;		// Posicion real 3d
};

//Vertex Shader
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;

   //Proyectar posicion
   Output.Position         = mul( Input.Position, matWorldViewProj);
   
   //Las Texcoord quedan igual
   Output.Texcoord         = Input.Texcoord;

   // Calculo la posicion real
   float4 pos_real = mul(Input.Position,matWorld);
   Output.Pos = float3(pos_real.x,pos_real.y,pos_real.z);
   
   // Transformo la normal y la normalizo
   //Output.Norm = normalize(mul(Input.Normal,matInverseTransposeWorld));
   Output.Norm = normalize(mul(Input.Normal,matWorld));
   return( Output );
   
}

//Pixel Shader
float4 ps_main( float3 Texcoord: TEXCOORD0, float3 N:TEXCOORD1,
	float3 Pos: TEXCOORD2) : COLOR0
{      
	//Obtener el texel de textura
	float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );
	if(fvBaseColor.a<0.1)
		discard;
	return fvBaseColor;
}

technique DefaultTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 ps_main();
   }
}



void VSCopy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


float4 PSFrameCombine( in float2 Tex : TEXCOORD0 , in float2 vpos : VPOS) : COLOR0
{
	return (tex2D(RenderTarget,Tex)  + tex2D(RenderTarget2,Tex) + tex2D(RenderTarget3,Tex) 
		+ + tex2D(RenderTarget4,Tex)+ + tex2D(RenderTarget5,Tex)) *0.2;
}


technique FrameMotionBlur
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 PSFrameCombine();
   }

}


