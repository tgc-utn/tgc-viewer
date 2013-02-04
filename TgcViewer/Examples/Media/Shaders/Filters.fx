/*
Pixel shader de filtros - TGC
*/

//La matriz de proyeccion.
float4x4 WorldViewProj : WorldViewProjection;

//La estructura que devuelve el vertex shader
struct VS_OUTPUT{
    float4 Pos : POSITION;
    float2 TexCoords : TEXCOORD0;
};

float Counter;

VS_OUTPUT mainVS(float4 pos : POSITION,
				 float2 inputTexCoords : TEXCOORD0
){
	
	VS_OUTPUT Out = (VS_OUTPUT) 0;
	
	//Multiplicar el vertice por la matriz de proyeccion.
	Out.Pos = mul(pos, WorldViewProj);
	
	//Mantengo las coordenadas de textura intactas.
	Out.TexCoords = inputTexCoords;
	
	return Out;
}

sampler2D input : register(s0); 

float4 mainPS( float2 texCoords : TEXCOORD0 ) : COLOR {
	//Retorno el color correspondiente a la posicion indicada en la textura.
	
	const float2 filter[4] = { 
		float2(-0.01f, -0.01f),
		float2(-0.01f, 0.01f),
		float2(0.01f, 0.01f),
		float2(0.01f, 0.01f),
	};
	
	float4 Color = float4(0.0f,0.0f,0.0f,1.0f);
	 texCoords.x =  texCoords.x  + (sin( texCoords.x*100 + Counter/100)*0.005);
Color = tex2D( input ,  texCoords.xy);


	//for (int i = 0 ;  i < 4 ; i++ )
		//Color += 0.25f * tex2D(input, texCoords + filter[i]);
	

	return Color;
}

technique DefaultTechnique {
	pass p0 {
		CullMode = None;
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();
	}
}
