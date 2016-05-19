/*
* Shader para ejemplo Lights/Integrador2
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

float4 eyePosition; //Posicion de la camara

//Parametros de luces
float3 lightColor[3]; //Color RGB de la luz
float4 lightPosition[3]; //Posicion de la luz
float lightIntensity[3]; //Intensidad de la luz
float lightAttenuation[3]; //Factor de atenuacion de la luz

//Intensidad de efecto Bump
float bumpiness;
const float3 BUMP_SMOOTH = { 0.5f, 0.5f, 1.0f };

//Factor de reflexion
float reflection;

/**************************************************************************************/
/* ThreeLightsTechnique */
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
	float3 LightVec1 : TEXCOORD6;
	float3 LightVec2 : TEXCOORD7;
	float3 LightVec3 : TEXCOORD8;
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
	output.LightVec1 = lightPosition[0].xyz - output.WorldPosition;
	output.LightVec2 = lightPosition[1].xyz - output.WorldPosition;
	output.LightVec3 = lightPosition[2].xyz - output.WorldPosition;

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
	float3 LightVec1 : TEXCOORD6;
	float3 LightVec2 : TEXCOORD7;
	float3 LightVec3 : TEXCOORD8;
};

//Resultado de computo de lighting
struct LightingResult
{
	float3 ambientLight;
	float3 diffuseLight;
	float3 specularLight;
};

//Calcular colores para una luz
LightingResult computeLighting(int i, float3 WorldPosition, float3 Ln, float3 surfaceNormal, float3 Vn)
{
	LightingResult res;

	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition[i].xyz - WorldPosition) * lightAttenuation[i];
	float intensity = lightIntensity[i] / distAtten;

	//Ambient
	res.ambientLight = intensity * lightColor[i] * materialAmbientColor;

	//Diffuse (N dot L)
	float3 n_dot_l = dot(surfaceNormal, Ln);
	res.diffuseLight = intensity * lightColor[i] * materialDiffuseColor.rgb * max(0.0, n_dot_l);

	//Specular (como vector de R se usa el HalfAngleVec)
	float3 HalfAngleVec = normalize(Vn + Ln);
	float3 n_dot_h = dot(surfaceNormal, HalfAngleVec);
	res.specularLight = n_dot_l <= 0.0
		? float3(0.0, 0.0, 0.0)
		: (
			intensity * lightColor[i] * materialSpecularColor
			* pow(max(0.0, n_dot_h), materialSpecularExp)
			);

	return res;
}

//Pixel Shader
float4 ps_general(PS_INPUT input) : COLOR0
{
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Tn = normalize(input.WorldTangent);
	float3 Bn = normalize(input.WorldBinormal);
	float3 Ln1 = normalize(input.LightVec1);
	float3 Ln2 = normalize(input.LightVec2);
	float3 Ln3 = normalize(input.LightVec3);

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

	//Calcular iluminacion para las 3 luces
	LightingResult res1 = computeLighting(0, input.WorldPosition, Ln1, bumpNormal, Vn);
	LightingResult res2 = computeLighting(1, input.WorldPosition, Ln2, bumpNormal, Vn);
	LightingResult res3 = computeLighting(2, input.WorldPosition, Ln3, bumpNormal, Vn);

	//Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	//El color Alpha sale del diffuse material
	float4 finalColor = float4(
		saturate(
			materialEmissiveColor +
			(res1.ambientLight + res2.ambientLight + res3.ambientLight) +
			(res1.diffuseLight + res2.diffuseLight + res3.diffuseLight))
		* texelColor +
		(res1.specularLight + res2.specularLight + res3.specularLight) +
		(texelColor * reflectionColor * reflection),
	materialDiffuseColor.a);

	return finalColor;
}

/*
* ThreeLightsTechnique
*/
technique ThreeLightsTechnique
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_general();
		PixelShader = compile ps_3_0 ps_general();
	}
}