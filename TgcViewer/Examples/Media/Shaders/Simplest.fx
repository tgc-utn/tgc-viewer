/*
El Shader mas simple - TGC
*/

//La matriz de proyeccion.
float4x4 WorldViewProj : WorldViewProjection;

float4 mainVS(float4 pos : POSITION) : POSITION{
	//Multiplicar el vertice por la matriz de proyeccion.
	return mul(pos, WorldViewProj);
}

float4 mainPS() : COLOR {
	//Retornar el color azul.
	return float4(0.0, 0.0, 1.0, 1.0);
}

technique DefaultTechnique {
	pass p0 {
		CullMode = None;
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();
	}
}
