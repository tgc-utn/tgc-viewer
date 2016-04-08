/*
* Shader utilizado para efecto de Environment Map sobre un TgcMesh.
* Solo soporta TgcMesh con RenderType del tipo DIFFUSE_MAP
* Tiene dos techniques:
*	- EnvironmentMapTechnique: EnviromentMap + BumpMap
*	- SimpleEnvironmentMapTechnique: Solo EnviromentMap
*/

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura utilizada para BumpMapping
texture texNormalMap;
sampler2D normalMap = sampler_state
{
	Texture = (texNormalMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura utilizada para EnvironmentMap
texture texCubeMap;
samplerCUBE cubeMap = sampler_state
{
	Texture = (texCubeMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB (tiene canal Alpha)
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Parametros de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float lightAttenuation; //Factor de atenuacion de la luz


//Intensidad de efecto Bump
float bumpiness;
const float3 BUMP_SMOOTH = { 0.5f, 0.5f, 1.0f };

//Factor de reflexion
float reflection;



/**************************************************************************************/
/* EnvironmentMapTechnique */
/**************************************************************************************/


//Input del Vertex Shader
struct VS_INPUT 
{
	float4 Position : POSITION0;
	float3 Normal :   NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	float3 WorldNormal	: TEXCOORD3;
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinormal : TEXCOORD5;
	float3 LightVec	: TEXCOORD6;
};

//Vertex Shader
VS_OUTPUT vs_general(VS_INPUT input)
{
	VS_OUTPUT output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Las Coordenadas de textura quedan igual
	output.Texcoord = input.Texcoord;
	
	//Posicion pasada a World-Space
	output.WorldPosition = mul(input.Position, matWorld).xyz;
	
	//Pasar normal, tangent y binormal a World-Space
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
    output.WorldTangent = mul(input.Tangent, matInverseTransposeWorld).xyz;
    output.WorldBinormal = mul(input.Binormal, matInverseTransposeWorld).xyz;
	
	//Vector desde el vertice hacia la luz
	output.LightVec = lightPosition.xyz - output.WorldPosition;
	
	
	return output;
}



//Input del Pixel Shader
struct PS_INPUT 
{
	float2 Texcoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	float3 WorldNormal	: TEXCOORD3;
	float3 WorldTangent	: TEXCOORD4;
	float3 WorldBinormal : TEXCOORD5;
	float3 LightVec	: TEXCOORD6;
};
	

//Pixel Shader
float4 ps_general(PS_INPUT input) : COLOR0
{      
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
    float3 Tn = normalize(input.WorldTangent);
    float3 Bn = normalize(input.WorldBinormal);
	float3 Ln = normalize(input.LightVec);
    

	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);

	
	
	//Obtener normal de normalMap y ajustar rango de [0, 1] a [-1, 1]
	float3 bumpNormal = tex2D(normalMap, input.Texcoord).rgb;
	bumpNormal = (bumpNormal * 2.0f) - 1.0f;
	
	//Suavizar con bumpiness
	bumpNormal = lerp(BUMP_SMOOTH, bumpNormal, bumpiness);
	
	//Pasar de Tangent-Space a World-Space
	bumpNormal = Nn + bumpNormal.x * Tn + bumpNormal.y * Bn;
	bumpNormal = normalize(bumpNormal);
	

	
	//Obtener texel de CubeMap
	float3 Vn = normalize(eyePosition.xyz - input.WorldPosition);
	float3 R = reflect(Vn,Nn);
    float3 reflectionColor = texCUBE(cubeMap, R).rgb;
	
	


	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
	float intensity = lightIntensity / distAtten;

	//Ambient
	float3 ambientLight = intensity * lightColor * materialAmbientColor;
	
	//Diffuse (N dot L, usando normal de NormalMap)
	float3 n_dot_l = dot(bumpNormal, Ln);
	float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l);
	
	//Specular (como vector de R se usa el HalfAngleVec)
	float3 HalfAngleVec = normalize(Vn + Ln);
	float3 n_dot_h = dot(bumpNormal, HalfAngleVec);
	float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (
				intensity * lightColor * materialSpecularColor 
				* pow(max( 0.0, n_dot_h), materialSpecularExp)
			);
	
	//Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	//El color Alpha sale del diffuse material
	float4 finalColor = float4((materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight + (texelColor * reflectionColor * reflection), materialDiffuseColor.a);
	
	
	//float4 finalColor = float4(reflectionColor, materialDiffuseColor.a);
	
	return finalColor;
}

/*
* Technique de EnvironmentMap
*/
technique EnvironmentMapTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 ps_general();
   }

}





/**************************************************************************************/
/* SimpleEnvironmentMapTechnique */
/**************************************************************************************/



//Output del Vertex Shader
struct VS_OUTPUT_SIMPLE
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	float3 WorldNormal	: TEXCOORD3;
	float3 LightVec	: TEXCOORD4;
};

//Vertex Shader
VS_OUTPUT_SIMPLE vs_simple(VS_INPUT input)
{
	VS_OUTPUT_SIMPLE output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Las Coordenadas de textura quedan igual
	output.Texcoord = input.Texcoord;
	
	//Posicion pasada a World-Space
	output.WorldPosition = mul(input.Position, matWorld).xyz;
	
	//Pasar normal a World-Space
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
	
	//Vector desde el vertice hacia la luz
	output.LightVec = lightPosition.xyz - output.WorldPosition;
	
	return output;
}


//Input del Pixel Shader
struct PS_INPUT_SIMPLE 
{
	float2 Texcoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	float3 WorldNormal	: TEXCOORD3;
	float3 LightVec	: TEXCOORD4;
};
	

//Pixel Shader
float4 ps_simple(PS_INPUT_SIMPLE input) : COLOR0
{      
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Ln = normalize(input.LightVec);
    
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Obtener texel de CubeMap
	float3 Vn = normalize(eyePosition.xyz - input.WorldPosition);
	float3 R = reflect(Vn, Nn);
    float3 reflectionColor = texCUBE(cubeMap, R).rgb;
	
	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
	float intensity = lightIntensity / distAtten;

	//Ambient
	float3 ambientLight = intensity * lightColor * materialAmbientColor;
	
	//Diffuse (N dot L, usando normal de NormalMap)
	float3 n_dot_l = dot(Nn, Ln);
	float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l);
	
	//Specular (como vector de R se usa el HalfAngleVec)
	float3 HalfAngleVec = normalize(Vn + Ln);
	float3 n_dot_h = dot(Nn, HalfAngleVec);
	float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (
				intensity * lightColor * materialSpecularColor 
				* pow(max( 0.0, n_dot_h), materialSpecularExp)
			);
	
	//Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	//El color Alpha sale del diffuse material
	float4 finalColor = float4((materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight + (texelColor * reflectionColor * reflection), materialDiffuseColor.a);
	
	
	//float4 finalColor = float4(reflectionColor, materialDiffuseColor.a);
	
	return finalColor;
}


/*
* Technique de EnvironmentMap simple, sin BumpMapping
*/
technique SimpleEnvironmentMapTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_simple();
	  PixelShader = compile ps_2_0 ps_simple();
   }

}
