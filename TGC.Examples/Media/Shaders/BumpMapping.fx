/*
* Shader utilizado para efecto de BumpMapping sobre un TgcMesh.
* Solo soporta TgcMesh con RenderType del tipo DIFFUSE_MAP
* Tiene una sola technique: BumpMappingTechnique
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
/* BumpMappingTechnique */
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
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinormal : TEXCOORD4;
	float3 LightVec	: TEXCOORD5;
	float3 HalfAngleVec	: TEXCOORD6;
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
	
	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
	output.LightVec = lightPosition.xyz - output.WorldPosition;
	
	//ViewVec (V): vector que va desde el vertice hacia la camara.
	float3 viewVector = eyePosition.xyz - output.WorldPosition;
	
	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
	output.HalfAngleVec = viewVector + output.LightVec;
	
	
	return output;
}


//Input del Pixel Shader
struct PS_INPUT 
{
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinormal : TEXCOORD4;
	float3 LightVec	: TEXCOORD5;
	float3 HalfAngleVec	: TEXCOORD6;
};

	

//Pixel Shader
float4 ps_general(PS_INPUT input) : COLOR0
{      
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Ln = normalize(input.LightVec);
	float3 Hn = normalize(input.HalfAngleVec);
	float3 Tn = normalize(input.WorldTangent);
    float3 Bn = normalize(input.WorldBinormal);
	
	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
	float intensity = lightIntensity / distAtten; //Dividimos intensidad sobre distancia (lo hacemos lineal pero tambien podria ser i/d^2)
	
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
	
	//Componente Ambient
	float3 ambientLight = intensity * lightColor * materialAmbientColor;
	
	//Componente Diffuse: N dot L, usando normal de NormalMap
	float3 n_dot_l = dot(bumpNormal, Ln);
	float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l); //Controlamos que no de negativo
	
	//Componente Specular: (N dot H)^exp, usando normal de NormalMap
	float3 n_dot_h = dot(bumpNormal, Hn);
	float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (intensity * lightColor * materialSpecularColor * pow(max( 0.0, n_dot_h), materialSpecularExp));
	
	/* Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	   El color Alpha sale del diffuse material */
	float4 finalColor = float4((materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight, materialDiffuseColor.a);
	
	
	return finalColor;
}

/*
* Technique de BumpMapping
*/
technique BumpMappingTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 ps_general();
   }

}
