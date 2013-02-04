/*
Pixel shader de filtro de Gauss- TGC
*/

//La matriz de proyeccion.
float4x4 WorldViewProj : WorldViewProjection;

sampler2D input : register(s0);

//La estructura que devuelve el vertex shader
struct VS_OUTPUT{
    float4 Pos : POSITION;
    float2 TexCoords : TEXCOORD0;
};



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

const float2 offsets[4] = {     
     0,  1,
     1,  0,
     0, -1,
     -1, 0 
};   


float4 mainPS( float2 texCoords : TEXCOORD0) : COLOR {

	float4 Orig = tex2D( input, texCoords.xy );

	float epsilon = 1 / 256.0f;
    float4 Sum = Orig ;
	
    for( int i = 0; i < 4; i++ )
        Sum += tex2D( input, texCoords + offsets[i]*epsilon );

    return saturate( Sum / 4 );

}

technique DefaultTechnique {
	pass p0 {
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();
	}
}
