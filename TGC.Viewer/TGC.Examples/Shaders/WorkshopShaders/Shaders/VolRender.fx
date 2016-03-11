
float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;

//Textura utilizada por el Pixel Shader
texture tex_buffer;
sampler2D buffer =
sampler_state
{
   Texture = (tex_buffer);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};


void vs_copy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


float4 ps_raytracing(float2 vPos : VPOS,float2 texcoords  : TEXCOORD0) : COLOR0
{ 
	return tex2D(buffer,texcoords);
}

technique volRender
{
    pass p0
    {
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 ps_raytracing();
    }
}