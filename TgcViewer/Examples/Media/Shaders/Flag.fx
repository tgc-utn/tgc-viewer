/*
Vertex shader de una bandera - TGC
*/

//La matriz de proyeccion.
float4x4 WorldViewProj : WorldViewProjection;

//Un contador de tiempo transcurrido;
float Counter;

//La estructura que devuelve el vertex shader
struct VS_OUTPUT{
    float4 Pos : POSITION;
    float2 TexCoords : TEXCOORD0;
};

VS_OUTPUT mainVS(float4 pos : POSITION,
				 float2 inputTexCoords : TEXCOORD0
){
	
	VS_OUTPUT Out = (VS_OUTPUT) 0;
	
	//Calculo la ondulacion en base al tiempo transcurrido y al desplazamiento en x.
	float angle = sin(pos.x + Counter/100);
	
	pos.y += angle*2;
	pos.z += angle*4;
	
	//Multiplicar el vertice por la matriz de proyeccion.
	Out.Pos = mul(pos, WorldViewProj);
	
	//Mantengo las coordenadas de textura intactas.
	Out.TexCoords = inputTexCoords;
	
	return Out;
}

sampler2D input : register(s0); 

float4 mainPS( float2 texCoords : TEXCOORD0 ) : COLOR {
	//Retorno el color correspondiente a la posicion indicada en la textura.
	return tex2D(input, texCoords);
}

technique DefaultTechnique {
	pass p0 {
		CullMode = None;
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();
	}
}
