/*
* Shader utilizado por el ejemplo "Lights/EjemploMultiDiffuseLights.cs"
* Permite aplicar iluminación dinámica con PhongShading a nivel de pixel.
* Soporta hasta 4 luces por objeto en la misma pasada.
* Las luces tienen atenuación por distancia.
* Solo se calcula el componente Diffuse para acelerar los cálculos. Se ignora
* el Specular.
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

//Textura para Lightmap
texture texLightMap;
sampler2D lightMap = sampler_state
{
   Texture = (texLightMap);
};


//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialDiffuseColor; //Color RGB

//Variables de las 4 luces
float3 lightColor[4]; //Color RGB de las 4 luces
float4 lightPosition[4]; //Posicion de las 4 luces
float lightIntensity[4]; //Intensidad de las 4 luces
float lightAttenuation[4]; //Factor de atenuacion de las 4 luces



/**************************************************************************************/
/* MultiDiffuseLightsTechnique */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
   float4 Position : POSITION0;
   float3 Normal : NORMAL0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
};

//Vertex Shader
VS_OUTPUT vs_general(VS_INPUT input)
{
	VS_OUTPUT output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;

	//Posicion pasada a World-Space (necesaria para atenuación por distancia)
	output.WorldPosition = mul(input.Position, matWorld);

	/* Pasar normal a World-Space 
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
	

	return output;
}


//Input del Pixel Shader
struct PS_INPUT
{
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
};




//Funcion para calcular color RGB de Diffuse
float3 computeDiffuseComponent(float3 surfacePosition, float3 N, int i)
{
	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition[i].xyz - surfacePosition);
	float3 Ln = (lightPosition[i].xyz - surfacePosition) / distAtten;
	distAtten = distAtten * lightAttenuation[i];
	float intensity = lightIntensity[i] / distAtten; //Dividimos intensidad sobre distancia

	//Calcular Diffuse (N dot L)
	return intensity * lightColor[i].rgb * materialDiffuseColor * max(0.0, dot(N, Ln));
}



//Pixel Shader para Point Light
float4 point_light_ps(PS_INPUT input) : COLOR0
{      
	float3 Nn = normalize(input.WorldNormal);

	//Emissive + Diffuse de 4 luces PointLight
	float3 diffuseLighting = materialEmissiveColor;

	//Diffuse 0
	diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 0);

	//Diffuse 1
	diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 1);
	
	//Diffuse 2
	diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 2);
	
	//Diffuse 3
	diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 3);
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	texelColor.rgb *= diffuseLighting;

	return texelColor;
}

/*
* Technique con iluminacion
*/
technique MultiDiffuseLightsTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_general();
	  PixelShader = compile ps_2_0 point_light_ps();
   }

}


