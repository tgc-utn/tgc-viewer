
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


float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;
float time;

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



float4 PSPostProcess( in float2 Tex : TEXCOORD0 , in float2 vpos : VPOS) : COLOR0
{
	float4 ColorBase = tex2D(RenderTarget, Tex);
	// float4 ColorBase = tex2D(RenderTarget, Tex + float2(34*sin(time+Tex.x*15)/screen_dx,25*cos(time+Tex.y*15)/screen_dy));

	return ColorBase;

	// gray scale
	 // Y = 0.2126 R + 0.7152 G + 0.0722 B
	//float Y = 0.2126*ColorBase.r + 0.7152*ColorBase.g + 0.0722*ColorBase.b;
	//return float4(Y,Y,Y,1);
}






static const int kernel_r = 6;
static const int kernel_size = 13;
static const float Kernel[kernel_size] = 
{
    0.002216,    0.008764,    0.026995,    0.064759,    0.120985,    0.176033,    0.199471,    0.176033,    0.120985,    0.064759,    0.026995,    0.008764,    0.002216,
};


void Blur(float2 screen_pos  : TEXCOORD0,out float4 Color : COLOR)
{ 
    Color = 0;
	for(int i=0;i<kernel_size;++i)
	for(int j=0;j<kernel_size;++j)
		Color += tex2D(RenderTarget, screen_pos+float2((float)(i-kernel_r)/screen_dx,(float)(j-kernel_r)/screen_dy)) * Kernel[i]*Kernel[j];
	Color.a = 1;

}


technique PostProcess
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 PSPostProcess();
   }

}


