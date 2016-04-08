/*
* Shader utilizado por el ejemplos Lights
* Permite aplicar iluminación dinámica con PhongShading utilizando distintos tipos de luz
*/

//Matrices de transformacion
float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Variables de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Para DirectionalLight es la direccion
float lightIntensity;
float lightAttenuation; //No vale para DirectionalLight


//Estructura para guardar datos de la Luz
struct LightSampleValues {
	float3 L;
	float iL;
};



/*-------------- DIRECTIONAL LIGHT ---------------*/

//Calcular valores de luz para Directional Light
LightSampleValues computeDirLightValues()
{
	LightSampleValues values;
	values.L = lightPosition.xyz;
	values.iL = lightIntensity;
	return values;
}


/*-------------- POINT LIGHT ---------------*/

//Calcular valores de luz para Point Light
LightSampleValues computePointLightValues(in float4 surfacePosition)
{
	LightSampleValues values;
	values.L = lightPosition.xyz - surfacePosition.xyz;
	float dist = length(values.L);
	values.L = values.L / dist; // normalize
	
	//attenuation
	float distAtten = dist * lightAttenuation;
	
	values.iL = lightIntensity / distAtten;
	return values;
}


/*-------------- SPOT LIGHT ---------------*/

//Variables propias de SpotLight
float3 spotLightDir; // unit-length
float spotLightAngleCos;
float spotLightExponent;

//Calcular valores de luz para Spot Light
LightSampleValues computeSpotLightValues(in float4 surfacePosition)
{
	LightSampleValues values;
	values.L = lightPosition.xyz - surfacePosition.xyz;
	float dist = length(values.L);
	values.L = values.L / dist; // normalize
	
	//attenuation
	float distAtten = dist * lightAttenuation;
	
	float spotAtten = dot(-spotLightDir, values.L);
	spotAtten = (spotAtten > spotLightAngleCos) 
					? pow(spotAtten, spotLightExponent)
					: 0.0;
	values.iL = lightIntensity * spotAtten / distAtten;
	return values;
}


/*-------------- Calculo de componentes: AMBIENT, DIFFUSE y SPECULAR ---------------*/

//Calcular color RGB de Ambient
float3 computeAmbientComponent(in LightSampleValues light)
{
	return light.iL * lightColor * materialAmbientColor;
}

//Calcular color RGB de Diffuse
float3 computeDiffuseComponent(in float3 surfaceNormal, in LightSampleValues light)
{
	return light.iL * lightColor * materialDiffuseColor.rgb * max(0.0, dot(surfaceNormal, light.L));
}

//Calcular color RGB de Specular
float3 computeSpecularComponent(in float3 surfaceNormal, in float4 surfacePosition, in LightSampleValues light)
{
	float3 viewVector = normalize(-surfacePosition.xyz);
	float3 reflectionVector = 2.0 * dot(light.L, surfaceNormal) * surfaceNormal - light.L;
	return (dot(surfaceNormal, light.L) <= 0.0)
			? float3(0.0,0.0,0.0)
			: (
				light.iL * lightColor * materialSpecularColor 
				* pow( max( 0.0, dot(reflectionVector, viewVector) ), materialSpecularExp )
			);
}




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
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float4 lightingPosition : TEXCOORD1;
	float3 lightingNormal : TEXCOORD2;
};

//Vertex Shader
VS_OUTPUT vs_general(VS_INPUT input)
{
	VS_OUTPUT output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Las Coordenadas de textura quedan igual
	output.Texcoord = input.Texcoord;

	// The position and normal for lighting
	// must be in camera space, not homogeneous space
	
	//Almacenar la posicion del vertice en ViewSpace para ser usada luego por la luz
	output.lightingPosition = mul(input.Position, matWorldView);
	
	//Almacenar la normal del vertice en ViewSpace para ser usada luego por la luz
	output.lightingNormal = mul(input.Normal, matWorldView);

	return output;
}


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
	float2 Texcoord : TEXCOORD0;
	float4 lightingPosition : TEXCOORD1;
	float3 lightingNormal : TEXCOORD2;
};

//Pixel Shader para Directional Light
float4 directional_light_ps(PS_INPUT input) : COLOR0
{      
	//Calcular datos de iluminacion para Directional Light
	LightSampleValues light = computeDirLightValues();
	
	//Sumar Emissive + Ambient + Diffuse
	float3 interpolatedNormal = normalize(input.lightingNormal);
	float4 diffuseLighting;
	diffuseLighting.rgb = materialEmissiveColor + computeAmbientComponent(light) + computeDiffuseComponent(interpolatedNormal, light);
	diffuseLighting.a = materialDiffuseColor.a;
	
	//Calcular Specular por separado
	float4 specularLighting;
	specularLighting.rgb = computeSpecularComponent(interpolatedNormal, input.lightingPosition, light);
	specularLighting.a = 0;
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Modular Diffuse con color de la textura y sumar luego Specular
	float4 finalColor = diffuseLighting * texelColor + specularLighting;

	return finalColor;
}



/*
* Technique para Directional Light
*/
technique DirectionalLightTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 directional_light_ps();
   }

}




//Pixel Shader para Point Light
float4 point_light_ps(PS_INPUT input) : COLOR0
{      
	//Calcular datos de iluminacion para Directional Light
	LightSampleValues light = computePointLightValues(input.lightingPosition);
	
	//Sumar Emissive + Ambient + Diffuse
	float3 interpolatedNormal = normalize(input.lightingNormal);
	float4 diffuseLighting;
	diffuseLighting.rgb = materialEmissiveColor + computeAmbientComponent(light) + computeDiffuseComponent(interpolatedNormal, light);
	diffuseLighting.a = materialDiffuseColor.a;
	
	//Calcular Specular por separado
	float4 specularLighting;
	specularLighting.rgb = computeSpecularComponent(interpolatedNormal, input.lightingPosition, light);
	specularLighting.a = 0;
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Modular Diffuse con color de la textura y sumar luego Specular
	float4 finalColor = diffuseLighting * texelColor + specularLighting;

	
	return finalColor;
}

/*
* Technique para Point Light
*/
technique PointLightTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 point_light_ps();
   }

}






//Pixel Shader para Spot Light
float4 spot_light_ps(PS_INPUT input) : COLOR0
{      
	//Calcular datos de iluminacion para Directional Light
	LightSampleValues light = computeSpotLightValues(input.lightingPosition);
	
	//Sumar Emissive + Ambient + Diffuse
	float3 interpolatedNormal = normalize(input.lightingNormal);
	float4 diffuseLighting;
	diffuseLighting.rgb = materialEmissiveColor + computeAmbientComponent(light) + computeDiffuseComponent(interpolatedNormal, light);
	diffuseLighting.a = materialDiffuseColor.a;
	
	//Calcular Specular por separado
	float4 specularLighting;
	specularLighting.rgb = computeSpecularComponent(interpolatedNormal, input.lightingPosition, light);
	specularLighting.a = 0;
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Modular Diffuse con color de la textura y sumar luego Specular
	float4 finalColor = diffuseLighting * texelColor + specularLighting;


	return finalColor;
}

/*
* Technique para Spot Light
*/
technique SpotLightTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 spot_light_ps();
   }

}







//Output del Vertex Shader
struct VS_OUTPUT_NO_LIGHT 
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_NO_LIGHT no_light_vs(VS_INPUT input)
{
	VS_OUTPUT_NO_LIGHT output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Las Coordenadas de textura quedan igual
	output.Texcoord = input.Texcoord;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_NO_LIGHT 
{
	float2 Texcoord : TEXCOORD0;
};

//Pixel Shader sin luz
float4 no_light_ps(PS_INPUT_NO_LIGHT input) : COLOR0
{      
	//Obtener texel de la textura
	return tex2D(diffuseMap, input.Texcoord);
}

/*
* Technique para cuando no hay luz
*/
technique NoLightTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 no_light_vs();
	  PixelShader = compile ps_2_0 no_light_ps();
   }

}