

float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;
float screen_inc_x;					// 1/screen_dx
float screen_inc_y;
int cant = 0;
float time = 0;

//Textura utilizada por el Pixel Shader
texture base_Tex;
sampler2D baseMap =
sampler_state
{
   Texture = (base_Tex);
   ADDRESSU = MIRROR;
   ADDRESSV = MIRROR;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};


texture g_RenderTarget;              // Render target
texture g_RenderTarget_ant;              // Render target anterior

sampler RenderTarget = 
sampler_state
{
    Texture = <g_RenderTarget>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

sampler RenderTargetAnt = 
sampler_state
{
    Texture = <g_RenderTarget_ant>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};



void vs_copy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


float4 PSGaussianBlur(float2 vPos : VPOS,float2 TextureUV  : TEXCOORD0) : COLOR0
{ 
	float3 color11 = tex2D(baseMap, TextureUV+float2(-2*screen_inc_x,-2*screen_inc_y)).rgb;
	float3 color12 = tex2D(baseMap, TextureUV+float2(-2*screen_inc_x,-screen_inc_y)).rgb;
	float3 color13 = tex2D(baseMap, TextureUV+float2(-2*screen_inc_x,0)).rgb;
	float3 color14 = tex2D(baseMap, TextureUV+float2(-2*screen_inc_x,screen_inc_y)).rgb;
	float3 color15 = tex2D(baseMap, TextureUV+float2(-2*screen_inc_x,2*screen_inc_y)).rgb;
	
	float3 color21 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,-2*screen_inc_y)).rgb;
	float3 color22 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,-screen_inc_y)).rgb;
	float3 color23 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,0)).rgb;
	float3 color24 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,screen_inc_y)).rgb;
	float3 color25 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,2*screen_inc_y)).rgb;

	float3 color31 = tex2D(baseMap, TextureUV+float2(0,-2*screen_inc_y)).rgb;
	float3 color32 = tex2D(baseMap, TextureUV+float2(0,-screen_inc_y)).rgb;
	float3 color33 = tex2D(baseMap, TextureUV+float2(0,0)).rgb;
	float3 color34 = tex2D(baseMap, TextureUV+float2(0,screen_inc_y)).rgb;
	float3 color35 = tex2D(baseMap, TextureUV+float2(0,2*screen_inc_y)).rgb;

	float3 color41 = tex2D(baseMap, TextureUV+float2(screen_inc_x,-2*screen_inc_y)).rgb;
	float3 color42 = tex2D(baseMap, TextureUV+float2(screen_inc_x,-screen_inc_y)).rgb;
	float3 color43 = tex2D(baseMap, TextureUV+float2(screen_inc_x,0)).rgb;
	float3 color44 = tex2D(baseMap, TextureUV+float2(screen_inc_x,screen_inc_y)).rgb;
	float3 color45 = tex2D(baseMap, TextureUV+float2(screen_inc_x,2*screen_inc_y)).rgb;
	
	float3 color51 = tex2D(baseMap, TextureUV+float2(2*screen_inc_x,-2*screen_inc_y)).rgb;
	float3 color52 = tex2D(baseMap, TextureUV+float2(2*screen_inc_x,-screen_inc_y)).rgb;
	float3 color53 = tex2D(baseMap, TextureUV+float2(2*screen_inc_x,0)).rgb;
	float3 color54 = tex2D(baseMap, TextureUV+float2(2*screen_inc_x,screen_inc_y)).rgb;
	float3 color55 = tex2D(baseMap, TextureUV+float2(2*screen_inc_x,2*screen_inc_y)).rgb;

	float3 T = 0;
	T += 2*color11+4*color12+5*color13+4*color14+2*color15;
	T += 4*color21+9*color22+12*color23+9*color24+4*color25;
	T += 5*color31+12*color32+15*color33+12*color34+5*color35;
	T += 4*color41+9*color42+12*color43+9*color44+4*color45;
	T += 2*color51+4*color52+5*color53+4*color54+2*color55;
	T /= 159.0;
	
	return float4(T,1);
}


float4 PSIntensidad(float2 vPos : VPOS,float2 TextureUV  : TEXCOORD0) : COLOR0
{ 
	//	Red 0.212
	//	Green 0.701
	//	Blue 0.087
	float3 color = tex2D(baseMap, TextureUV).rgb;
	float K = color.r*0.212 + color.g*0.701  + color.b*0.087;
	return float4(K,K,K,1);
}

float4 PSErotionFilter(float2 vPos : VPOS,float2 TextureUV  : TEXCOORD0) : COLOR0
{ 
	float color11 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,-screen_inc_y)).r;
	float color12 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,0)).r;
	float color13 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,screen_inc_y)).r;

	float color21 = tex2D(baseMap, TextureUV+float2(0,-screen_inc_y)).r;
	float color22 = tex2D(baseMap, TextureUV+float2(0,0)).r;
	float color23 = tex2D(baseMap, TextureUV+float2(0,screen_inc_y)).r;

	float color31 = tex2D(baseMap, TextureUV+float2(screen_inc_x,-screen_inc_y)).r;
	float color32 = tex2D(baseMap, TextureUV+float2(screen_inc_x,0)).r;
	float color33 = tex2D(baseMap, TextureUV+float2(screen_inc_x,screen_inc_y)).r;

	float min1 = min(color11,min(color12,color13));
	float min2 = min(color21,min(color22,color23));
	float min3 = min(color31,min(color32,color33));
	float K = min(min1,min(min2,min3));
	return float4(K,K,K,1);
}


float4 PSSobelOperator(float2 vPos : VPOS,float2 TextureUV  : TEXCOORD0) : COLOR0
{ 
	float color11 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,-screen_inc_y)).r;
	float color12 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,0)).r;
	float color13 = tex2D(baseMap, TextureUV+float2(-screen_inc_x,screen_inc_y)).r;

	float color21 = tex2D(baseMap, TextureUV+float2(0,-screen_inc_y)).r;
	float color22 = tex2D(baseMap, TextureUV+float2(0,0)).r;
	float color23 = tex2D(baseMap, TextureUV+float2(0,screen_inc_y)).r;

	float color31 = tex2D(baseMap, TextureUV+float2(screen_inc_x,-screen_inc_y)).r;
	float color32 = tex2D(baseMap, TextureUV+float2(screen_inc_x,0)).r;
	float color33 = tex2D(baseMap, TextureUV+float2(screen_inc_x,screen_inc_y)).r;
	
	float Gx =	-color11	+ color13 -2*color22 + 2*color23 -color31	+ color33;
	float Gy =	-color11 - 2*color21 - color13 + color31 + 2*color32+color33;
	float gradiente = sqrt(Gx*Gx + Gy*Gy);
	float angulo = atan2(Gy,Gx);
	return float4(gradiente,gradiente,gradiente,1);
}



technique ImageFilter
{
    pass p0
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 PSIntensidad();
    }
    pass p1
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 PSErotionFilter();
    }
    
    pass p2
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 PSSobelOperator();
    }
    
    pass p3
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 PSGaussianBlur();
    }
}


float4 PSMotionDetect(float2 vPos : VPOS,float2 TextureUV  : TEXCOORD0) : COLOR0
{ 
	return tex2D(RenderTarget,TextureUV);

	/*
	float t = distance(tex2D(RenderTarget, TextureUV) , tex2D(RenderTargetAnt,TextureUV));
	if(t>0.5)
		t = 1;
	else 
		t = 0;
	//return float4(t,t,t,1);

	return tex2D(baseMap, TextureUV) + float4(0,t,t,1);
	*/
	
	
}

technique motionDetect
{
    pass p0
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 PSMotionDetect();
    }
}
