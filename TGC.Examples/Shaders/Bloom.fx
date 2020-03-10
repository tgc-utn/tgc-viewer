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

texture glowyFrameBuffer;
sampler GlowyFrameBuffer =
sampler_state
{
	Texture = <glowyFrameBuffer>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture horizontalBlurFrameBuffer;
sampler HorizontalBlurFrameBuffer =
sampler_state
{
	Texture = <horizontalBlurFrameBuffer>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture verticalBlurFrameBuffer;
sampler VerticalBlurFrameBuffer =
sampler_state
{
	Texture = <verticalBlurFrameBuffer>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

texture sceneFrameBuffer;
sampler SceneFrameBuffer =
sampler_state
{
	Texture = <sceneFrameBuffer>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Input del Vertex Shader
struct Light
{
	float3 Position;
	float3 SpecularColor;
	float3 DiffuseColor;
	float3 AmbientColor;
};

float KAmbient; // Coeficiente de Ambient
float KDiffuse; // Coeficiente de Diffuse
float KSpecular; // Coeficiente de Specular
float shininess; //Exponente de specular
float3 eyePosition; //Posicion de la camara
float screen_dx, screen_dy; // 1 / proporcion de la pantalla

bool toneMapping;
bool bloom;
bool scene;

Light lights[3];

int lightCount;

float glowyDifference = 0.8;
float glowyLightRadius = 14.0;

static const int radius = 7;
static const int kernelSize = 15;
static const float kernel[kernelSize] =
{
	0.000489, 0.002403, 0.009246, 0.02784, 0.065602, 0.120999, 0.174697, 0.197448, 0.174697, 0.120999, 0.065602, 0.02784, 0.009246, 0.002403, 0.000489
};
//Input del Vertex Shader
struct BlinnVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct BlinnVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

//Input del Vertex Shader
struct GlowyVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct GlowyVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
};


//Input del Vertex Shader
struct SimpleVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct SimpleVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};



float3 ToneMap(float3 color)
{
	float gamma = 1.0 / 2.2;
	return color * pow(color, float3(gamma, gamma, gamma));
}

//Vertex Shader
BlinnVertexShaderOutput VSBlinn(BlinnVertexShaderInput input)
{
	BlinnVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	return output;
}

//Pixel Shader
float4 PSBlinn(BlinnVertexShaderOutput input) : COLOR0
{
	input.WorldNormal = normalize(input.WorldNormal);
	float3 viewDirection = normalize(eyePosition - input.WorldPosition);

	// Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);
	float4 color = float4(0, 0, 0, texelColor.a);
	
	for (int index = 0; index < lightCount; index++)
	{
		float3 lightDirection = normalize(lights[index].Position - input.WorldPosition);
		float3 halfVector = normalize(lightDirection + viewDirection);

		//Componente Diffuse: N dot L
		float3 NdotL = dot(input.WorldNormal, lightDirection);
		float3 diffuseLight = KDiffuse * lights[index].DiffuseColor * max(0.0, NdotL);

		//Componente Specular: (N dot H)^shininess
		float3 NdotH = dot(input.WorldNormal, halfVector);
		float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * lights[index].SpecularColor * pow(max(0.0, NdotH), shininess);

		color.rgb += float3(saturate(lights[index].AmbientColor * KAmbient + diffuseLight) * texelColor + specularLight);
	}

	return color;
}


//Vertex Shader
GlowyVertexShaderOutput VSGlowy(GlowyVertexShaderInput input)
{
	GlowyVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	output.WorldPosition = mul(input.Position, matWorld);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

//Pixel Shader
float4 PSGlowy(GlowyVertexShaderOutput input) : COLOR0
{
	float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);
	float4 finalColor = float4(0, 0, 0, 1);

	for (int index = 0; index < lightCount; index++)
	{
		float3 color = lights[index].DiffuseColor;
		float difference = distance(color, texelColor.rgb);

		float distanceLight = distance(lights[index].Position, input.WorldPosition);

		if (distanceLight < glowyLightRadius && difference < glowyDifference)
			finalColor = texelColor;
	}
	return finalColor;
}


//Vertex Shader
SimpleVertexShaderOutput VSSimplePropagate(SimpleVertexShaderInput input)
{
	SimpleVertexShaderOutput output;

	// Propagamos la position
	output.Position = input.Position;

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

//Pixel Shader
float4 PSHorizontalBlur(SimpleVertexShaderOutput input) : COLOR0
{
	float4 horizontalSum = float4(0, 0, 0, 1);
	for (int x = 0; x < kernelSize; x++)
	{
		float2 delta = float2((x - radius + 1) / screen_dx, 0);
		horizontalSum += tex2D(GlowyFrameBuffer, input.TextureCoordinates + delta) * kernel[x];
	}
	return horizontalSum;
}

//Pixel Shader
float4 PSVerticalBlur(SimpleVertexShaderOutput input) : COLOR0
{
	float4 verticalSum = float4(0, 0, 0, 1);
	for (int y = 0; y < kernelSize; y++)
	{
		float2 delta = float2(0, (y - radius + 1) / screen_dy);
		verticalSum += tex2D(GlowyFrameBuffer, input.TextureCoordinates + delta) * kernel[y];
	}
	return verticalSum;
}


//Pixel Shader
float4 PSBloom(SimpleVertexShaderOutput input) : COLOR0
{
	float4 bloomColor = tex2D(VerticalBlurFrameBuffer, input.TextureCoordinates);
	bloomColor *= bloom ? 1.5 : 0.0;
	
	float4 sceneColor = tex2D(SceneFrameBuffer, input.TextureCoordinates) * scene;
	float4 finalColor = sceneColor + bloomColor;

	if (toneMapping)
		finalColor.rgb = ToneMap(finalColor.rgb);

	return finalColor;
}

technique Blinn
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSBlinn();
		PixelShader = compile ps_3_0 PSBlinn();
	}
}

technique GlowyObjects
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSGlowy();
		PixelShader = compile ps_3_0 PSGlowy();
	}
}

technique HorizontalBlur
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSSimplePropagate();
		PixelShader = compile ps_3_0 PSHorizontalBlur();
	}
}

technique VerticalBlur
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSSimplePropagate();
		PixelShader = compile ps_3_0 PSVerticalBlur();
	}
}

technique Integrate
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSSimplePropagate();
		PixelShader = compile ps_3_0 PSBloom();
	}
}