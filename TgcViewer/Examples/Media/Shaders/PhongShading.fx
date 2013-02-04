/*
* Shader utilizado por el ejemplo "Shaders/EjemploPhongShading"
* Permite aplicar iluminación dinámica con el algoritmo Phong.
*/


//Variables utilizadas por el Vertex Shader
float3 fvLightPosition = float3( -100.00, 100.00, -100.00 );
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;

//Input del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
   
   
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection :  TEXCOORD2;
   float3 Normal :          TEXCOORD3;
   
};

//Vertex Shader
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;

   //Proyectar posicion
   Output.Position         = mul( Input.Position, matWorldViewProj );
   
   //Las Texcoord quedan igual
   Output.Texcoord         = Input.Texcoord;
   
   //Obtener direccion a la que mira la camara y direccion de la luz
   float3 fvObjectPosition = mul( Input.Position, matWorld );
   Output.ViewDirection    = fvEyePosition - fvObjectPosition;
   Output.LightDirection   = fvLightPosition - fvObjectPosition;
   
   //Proyectar normal
   Output.Normal           = mul( Input.Normal, matWorld );
      
   return( Output );
   
}


//Variables utilizadas por el Pixel Shader
float4 fvAmbient = float4( 0.37, 0.38, 0.37, 1.00 );
float4 fvDiffuse = float4( 0.89, 0.89, 0.33, 1.00 );
float4 fvSpecular = float4( 0.49, 1.00, 0.49, 1.00 );
float fSpecularPower = float( 16.84 );

//Textura utilizada por el Pixel Shader
texture diffuseMap_Tex;
sampler2D diffuseMap = sampler_state
{
   Texture = (diffuseMap_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

//Input del Pixel Shader
struct PS_INPUT 
{
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection:   TEXCOORD2;
   float3 Normal :          TEXCOORD3;
   
};

//Pixel Shader
float4 ps_main( PS_INPUT Input ) : COLOR0
{      
	//Aplicar algoritmo de Diffuse Light
   float3 fvLightDirection = normalize( Input.LightDirection );
   float3 fvNormal         = normalize( Input.Normal );
   float  fNDotL           = dot( fvNormal, fvLightDirection ); 
   
   //Aplicar algoritmo de Specular Light
   float3 fvReflection     = normalize( ( ( 2.0f * fvNormal ) * ( fNDotL ) ) - fvLightDirection ); 
   float3 fvViewDirection  = normalize( Input.ViewDirection );
   float  fRDotV           = max( 0.0f, dot( fvReflection, fvViewDirection ) );
   
   //Obtener el texel de textura
   float4 fvBaseColor      = tex2D( diffuseMap, Input.Texcoord );
   
   //Sumar Ambient + Diffuse + Specular
   float4 fvTotalAmbient   = fvAmbient * fvBaseColor; 
   float4 fvTotalDiffuse   = fvDiffuse * fNDotL * fvBaseColor; 
   float4 fvTotalSpecular  = fvSpecular * pow( fRDotV, fSpecularPower );
   
   //Saturar para no pasarse del máximo color
   return( saturate( fvTotalAmbient + fvTotalDiffuse + fvTotalSpecular ) );
}



/*
* Technique default
*/
technique DefaultTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_main();
	  PixelShader = compile ps_2_0 ps_main();
   }

}


//******************************************************************************//


//Input del Vertex Shader
struct VS_INPUT_NO_TEXTURE 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float4 Color : 	 COLOR;
};

//Output del Vertex Shader
struct VS_OUTPUT_NO_TEXTURE  
{
   float4 Position :        POSITION0;
   float4 Color :        	COLOR;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection :  TEXCOORD2;
   float3 Normal :          TEXCOORD3;
   
};

//Vertex Shader
VS_OUTPUT_NO_TEXTURE  vs_main_noTexture( VS_INPUT_NO_TEXTURE  Input )
{
   VS_OUTPUT_NO_TEXTURE  Output;

   //Proyectar posicion
   Output.Position         = mul( Input.Position, matWorldViewProj );
   
   //Color
   Output.Color         = Input.Color;
   
   //Obtener direccion a la que mira la camara y direccion de la luz
   float3 fvObjectPosition = mul( Input.Position, matWorld );
   Output.ViewDirection    = fvEyePosition - fvObjectPosition;
   Output.LightDirection   = fvLightPosition - fvObjectPosition;
   
   //Proyectar normal
   Output.Normal           = mul( Input.Normal, matWorld );
      
   return( Output );
   
}

//Input del Pixel Shader
struct PS_INPUT_NO_TEXTURE 
{
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection:   TEXCOORD2;
   float3 Normal :          TEXCOORD3;
   float4 Color  :			COLOR;
};

//Pixel Shader
float4 ps_main_noTexture( PS_INPUT_NO_TEXTURE Input ) : COLOR0
{      
	//Aplicar algoritmo de Diffuse Light
   float3 fvLightDirection = normalize( Input.LightDirection );
   float3 fvNormal         = normalize( Input.Normal );
   float  fNDotL           = dot( fvNormal, fvLightDirection ); 
   
   //Aplicar algoritmo de Specular Light
   float3 fvReflection     = normalize( ( ( 2.0f * fvNormal ) * ( fNDotL ) ) - fvLightDirection ); 
   float3 fvViewDirection  = normalize( Input.ViewDirection );
   float  fRDotV           = max( 0.0f, dot( fvReflection, fvViewDirection ) );
   
   //Obtener color base
   float4 fvBaseColor      = Input.Color;
   
   //Sumar Ambient + Diffuse + Specular
   float4 fvTotalAmbient   = fvAmbient * fvBaseColor; 
   float4 fvTotalDiffuse   = fvDiffuse * fNDotL * fvBaseColor; 
   float4 fvTotalSpecular  = fvSpecular * pow( fRDotV, fSpecularPower );
   
   //Saturar para no pasarse del máximo color
   return( saturate( fvTotalAmbient + fvTotalDiffuse + fvTotalSpecular ) );
}

/*
* Technique NoTexture
*/
technique NoTextureTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_main_noTexture();
	  PixelShader = compile ps_2_0 ps_main_noTexture();
   }

}
