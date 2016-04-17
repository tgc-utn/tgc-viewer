float3 fvLightPosition = float3( -4.00, -44.00, 52.00 );
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float4x4 matWorld;
float4x4 matWorldViewProjection;

struct VS_INPUT 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float2 Texcoord : TEXCOORD0;
   
};

struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection:   TEXCOORD2;
   
};

VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;

   Output.Position         = mul( Input.Position, matWorldViewProjection );
   Output.Texcoord         = Input.Texcoord;
   
   float3 fvObjectPosition = mul( Input.Position, matWorld );
   
   Output.ViewDirection  = normalize(fvEyePosition - fvObjectPosition);
   Output.LightDirection = normalize(fvLightPosition - fvObjectPosition);
   
   return( Output );
   
}



float4 fvAmbient = float4( 0.39, 0.49, 0.53, 1.00 );
float4 fvSpecular = float4( 1.00, 1.00, 0.00, 1.00 );
float4 fvDiffuse = float4( 0.17, 0.24, 0.41, 0.25 );
float fSpecularPower = float( 1.99 );

texture base_Tex;
sampler2D baseMap = sampler_state
{
   Texture = (base_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};
texture bump_Tex;
sampler2D bumpMap = sampler_state
{
   Texture = (bump_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

struct PS_INPUT 
{
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection:   TEXCOORD2;
   
};

float4 ps_main( PS_INPUT Input ) : COLOR0
{      
   float3 fvLightDirection = normalize( Input.LightDirection );
   float3 fvNormal         = mul( tex2D( bumpMap, Input.Texcoord ), matWorld);
   float  fNDotL           = clamp(dot( fvNormal, fvLightDirection ), 0, 1); 
   
   float3 fvReflection     = normalize( ( ( 2.0f * fvNormal ) * ( fNDotL ) ) - fvLightDirection ); 
   float3 fvViewDirection  = normalize( Input.ViewDirection );
   float  fRDotV           = max( 0.0f, dot( fvReflection, fvViewDirection ) );
   
   float4 fvBaseColor      = tex2D( baseMap, Input.Texcoord );
   
   float4 fvTotalAmbient   = fvAmbient * fvBaseColor; 
   float4 fvTotalDiffuse   = fvDiffuse * fNDotL * fvBaseColor; 
   float4 fvTotalSpecular  = fvSpecular * pow( fRDotV, fSpecularPower );
   
   return( saturate( fvTotalAmbient + fvTotalDiffuse + fvTotalSpecular ) );
     
	//return float4(fvNormal.xyz, 1);
	//return float4(Input.Texcoord.x, Input.Texcoord.y, 0,1) ;
	//return tex2D( bumpMap, Input.Texcoord );

}




//--------------------------------------------------------------//
// Technique Section for Effect Workspace.Effect Group 1.Textured Bump
//--------------------------------------------------------------//
technique DefaultTechnique
{
   pass Pass_0
   {
      VertexShader = compile vs_2_0 vs_main();
      PixelShader = compile ps_2_0 ps_main();
   }

}

