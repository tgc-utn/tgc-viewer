// ---------------------------------------------------------
// Ejemplo shader Minimo:
// ---------------------------------------------------------

float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matWorldInverseTranspose;

float time = 0;

// Textura y sampler de textura
texture base_Tex;
sampler2D baseMap =
sampler_state
{
   Texture = (base_Tex);
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

//Input del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float4 Color :			COLOR0;
};

//Vertex Shader
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;
   //Proyectar posicion
   Output.Position = mul( Input.Position, matWorldViewProj);
   
   //Propago las coordenadas de textura
   Output.Texcoord = Input.Texcoord;

   //Propago el color x vertice
   Output.Color = Input.Color;

   return( Output );
   
}

//Pixel Shader
float4 ps_main( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	// Obtener el texel de textura
	// baseMap es el sampler, Texcoord son las coordenadas interpoladas
	float4 fvBaseColor = tex2D( baseMap, Texcoord );
	// combino color y textura
	// en este ejemplo combino un 80% el color de la textura y un 20%el del vertice
	return 0.8*fvBaseColor + 0.2*Color;
}


// Ejemplo de un vertex shader que anima la posicion de los vertices 
// ------------------------------------------------------------------
VS_OUTPUT vs_main2( VS_INPUT Input )
{
   VS_OUTPUT Output;

   // Animar posicion
   Input.Position.x += sin(time)*30*sign(Input.Position.x);
   Input.Position.y += cos(time)*30*sign(Input.Position.y-20);
   Input.Position.z += sin(time)*30*sign(Input.Position.z);
   
   //Proyectar posicion
   Output.Position = mul( Input.Position, matWorldViewProj);
   
   //Propago las coordenadas de textura
   Output.Texcoord = Input.Texcoord;

	// Animar color
   Input.Color.r = abs(sin(time));
   Input.Color.g = abs(cos(time));
   
   //Propago el color x vertice
   Output.Color = Input.Color;

   return( Output );
   
}

// ------------------------------------------------------------------
technique RenderScene
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_main2();
	  PixelShader = compile ps_2_0 ps_main();
   }

}
