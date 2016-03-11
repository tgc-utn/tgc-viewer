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
float4x4 matWorldViewProjAnt;		//Matriz World * View * Proj anterior

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


texture texVelocityMap;
sampler2D velocityMap = sampler_state
{
	Texture = (texVelocityMap);
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture texVelocityMapAnt;
sampler2D velocityMapAnt = sampler_state
{
	Texture = (texVelocityMapAnt);
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :    POSITION0;
   float2 Texcoord :    TEXCOORD0;
   float3 Norm :        TEXCOORD1;			// Normales
   float3 Pos :   		TEXCOORD2;		// Posicion real 3d
   float2 Vel :			TEXCOORD3;		// velocidad por pixel

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
   Output.Norm = normalize(mul(Input.Normal,matWorld));


    // Computo la velocidad del vertice
	// posicion actual
	float4 vPosActual = Output.Position;
	// posicion anterior
	float4 vPosAnterior = mul( Input.Position,matWorldViewProjAnt);
	vPosActual /= vPosActual.w;
	vPosAnterior /= vPosAnterior.w;
	float2 velocity = vPosActual - vPosAnterior;    
    // lo propago
    Output.Vel = velocity;



   return( Output );
   
}

//Pixel Shader
float4 ps_main( float3 Texcoord: TEXCOORD0) : COLOR0
{      
	float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );
	if(fvBaseColor.a<0.1)
		discard;
	return fvBaseColor;
}


//Pixel Shader Velocity
float4 ps_velocity( float3 Texcoord: TEXCOORD0, float2 Vel:TEXCOORD3) : COLOR0
{      
	//Obtener el texel de textura
	float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );
	if(fvBaseColor.a<0.1)
		discard;
	return float4(Vel.x,Vel.y,0.0f,1.0f);
}

technique DefaultTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 ps_main();
   }
}

technique VelocityMap
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 ps_velocity();
   }

}




void vs_copy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


float PixelBlurConst = 400;
static const float NumberOfPostProcessSamples = 12.0f;

float4 ps_motion_blur( in float2 Tex : TEXCOORD0) : COLOR0
{
    float4 curFramePixelVelocity = tex2D(velocityMap, Tex);
    float4 lastFramePixelVelocity = tex2D(velocityMapAnt, Tex);

	float2 pixelVelocity;
    float curVelocitySqMag = curFramePixelVelocity.r * curFramePixelVelocity.r +
                             curFramePixelVelocity.g * curFramePixelVelocity.g;
    float lastVelocitySqMag = lastFramePixelVelocity.r * lastFramePixelVelocity.r +
                              lastFramePixelVelocity.g * lastFramePixelVelocity.g;
                                   
    if( lastVelocitySqMag > curVelocitySqMag )
    {
        pixelVelocity.x =  lastFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -lastFramePixelVelocity.g * PixelBlurConst;
    }
    else
    {
        pixelVelocity.x =  curFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -curFramePixelVelocity.g * PixelBlurConst;    
    }
	
	
	//= curFramePixelVelocity.rg* PixelBlurConst;
	//pixelVelocity.y *= -1;

    float3 Blurred = 0;    
    for(float i = 0; i < NumberOfPostProcessSamples; i++)
    {   
        float2 lookup = pixelVelocity * i / NumberOfPostProcessSamples + Tex;
        float4 Current = tex2D(RenderTarget, lookup);
        Blurred += Current.rgb;
    }
	return float4(Blurred / NumberOfPostProcessSamples, 1.0f);

//	return tex2D(velocityMap,Tex)  ;
//	return tex2D(RenderTarget,Tex) ;

}




technique PostProcessMotionBlur
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_copy();
	  PixelShader = compile ps_3_0 ps_motion_blur();
   }

}


