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
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
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

texture g_GlowMap;
sampler GlowMap = 
sampler_state
{
    Texture = <g_GlowMap>;
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


// dibuja negro (tiene que ocultar lo que esta oscuro)
float4 PSOscuros(float3 Texcoord: TEXCOORD0) : COLOR0
{
	float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );
	return float4(0,0,0,fvBaseColor.a);
}

technique DibujarObjetosOscuros
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 PSOscuros();
   }
}



void VSCopy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}

// Gaussian Blur

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

technique GaussianBlur
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 Blur();
   }

}



void BlurH(float2 screen_pos  : TEXCOORD0,out float4 Color : COLOR)
{ 
    Color = 0;
	for(int i=0;i<kernel_size;++i)
		Color += tex2D(RenderTarget, screen_pos+float2((float)(i-kernel_r)/screen_dx,0)) * Kernel[i];
	Color.a = 1;
}

void BlurV(float2 screen_pos  : TEXCOORD0,out float4 Color : COLOR)
{ 
    Color = 0;
	for(int i=0;i<kernel_size;++i)
		Color += tex2D(RenderTarget, screen_pos+float2(0,(float)(i-kernel_r)/screen_dy)) * Kernel[i];
	Color.a = 1;

}

technique GaussianBlurSeparable
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 BlurH();
   }
   pass Pass_1
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 BlurV();
   }

}

float4 PSDownFilter4( in float2 Tex : TEXCOORD0 ) : COLOR0
{
    float4 Color = 0;
    for (int i = 0; i < 4; i++)
    for (int j = 0; j < 4; j++)
		Color += tex2D(RenderTarget, Tex+float2((float)i/screen_dx,(float)j/screen_dy));

	return Color / 16;
}



technique DownFilter4
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 PSDownFilter4();
   }

}

float4 PSGrayScale( in float2 Tex : TEXCOORD0 , in float2 vpos : VPOS) : COLOR0
{
	float4 ColorBase = tex2D(RenderTarget, Tex);
	float4 ColorBrillante = tex2D(GlowMap, Tex+float2((float)16/screen_dx,(float)16/screen_dy));
	 // Y = 0.2126 R + 0.7152 G + 0.0722 B
	float Yb = 0.2126*ColorBase.r + 0.7152*ColorBase.g + 0.0722*ColorBase.b;
	float Yk = 0.2126*ColorBrillante.r + 0.7152*ColorBrillante.g + 0.0722*ColorBrillante.b;
	if( round(vpos.y/2)%2 == 0)
	{
		Yb *= 0.85;
		Yk *= 0.85;
	}

	return float4(Yk*0.75,Yb*0.6+Yk*4,Yk*0.75,1);
}


technique GrayScale
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 VSCopy();
	  PixelShader = compile ps_3_0 PSGrayScale();
   }

}


