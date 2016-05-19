/*
* Shader para ejemplo de HDR
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

//Parametros de la Luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float3 materialAmbientColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Vector para pasar de RGB a Luminance
static const float3 LUMINANCE_VECTOR = float3(0.2125f, 0.7154f, 0.0721f);

//Umbral de brillo minimo para el BrightPass
static const float BRIGHT_PASS_THRESHOLD = 5.0f;

//Offset for BrightPass filter
static const float BRIGHT_PASS_OFFSET = 10.0f;

//Textura de escena original con HDR
texture texSceneRT;
sampler SceneRT = sampler_state
{
	Texture = (texSceneRT);
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
};

//Textura de 1x1 con valor de Luminance de la escena
texture texLuminanceRT;
sampler LuminanceRT = sampler_state
{
	Texture = (texLuminanceRT);
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
};

//Textura de BrightPass
texture texBrightPassRT;
sampler BrightPassRT = sampler_state
{
	Texture = (texBrightPassRT);
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
};

//Textura de Bloom
texture texBloomRT;
sampler BloomRT = sampler_state
{
	Texture = (texBloomRT);
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
};

//Offsets y weights de Gaussian Blur
static const int MAX_SAMPLES = 15;
float2 gauss_offsets[MAX_SAMPLES];
float gauss_weights[MAX_SAMPLES];

//Offsets generales para hacer varios casos de down-sampling. Como maximo es de 4x4
float sampleOffsets[16];

//Valor para hacer bright-pass
float middleGray;

/**************************************************************************************/
/* LightPass */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
	float3 LightVec	: TEXCOORD3;
	float3 ViewVec : TEXCOORD4;
};

//Vertex Shader para render inicial del mesh
VS_OUTPUT_DIFFUSE_MAP vs_LightPass(VS_INPUT_DIFFUSE_MAP input)
{
	VS_OUTPUT_DIFFUSE_MAP output;

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

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
	output.LightVec = lightPosition.xyz - output.WorldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara. Usado en Specular
	output.ViewVec = eyePosition.xyz - output.WorldPosition;

	return output;
}

//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
	float3 LightVec	: TEXCOORD3;
	float3 ViewVec : TEXCOORD4;
};

//Pixel Shader para render inicial del mesh
float4 ps_LightPass(PS_DIFFUSE_MAP input) : COLOR0
{
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);

	//Alpha-test
	if (texelColor.a < 0.1f)
	{
		discard;
	}

	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Ln = normalize(input.LightVec);
	float3 Vn = normalize(input.ViewVec);

	//Componente Diffuse: N dot L
	float n_dot_l = max(0.0, dot(Nn, Ln));
	float fDiffuse = saturate(n_dot_l);

	//Componente Specular: (N dot H)^exp
	float3 vReflection = reflect(-Ln, Nn);
	float n_dot_h = max(0.0, dot(Vn, vReflection));
	float fSpecular = pow(saturate(n_dot_h), materialSpecularExp);

	//Final color
	float3 lightIntensityColor = float3(lightIntensity, lightIntensity, lightIntensity);
	float4 finalColor = float4((materialAmbientColor + (fDiffuse + fSpecular) * lightIntensityColor) * texelColor.rgb, 1);

	return finalColor;
}

/*
* Technique LightPass
*/
technique LightPass
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_LightPass();
		PixelShader = compile ps_3_0 ps_LightPass();
	}
}

/**************************************************************************************/
/* DrawLightSource */
/**************************************************************************************/

//Pixel Shader renderizar la fuente de luz
float4 vs_DrawLightSource(float4 position : POSITION0) : POSITION0
{
	//Solo proyectar posicion
	return mul(position, matWorldViewProj);
}

//Pixel Shader renderizar la fuente de luz
float4 ps_DrawLightSource() : COLOR0
{
	//Solo devolvemos el color de la intensidad de la luz
	return float4(lightIntensity, lightIntensity, lightIntensity, 1);
}

/*
* Technique DrawLightSource
*/
technique DrawLightSource
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_DrawLightSource();
		PixelShader = compile ps_3_0 ps_DrawLightSource();
	}
}

/**************************************************************************************/
/* DefaultTechnique */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 ScreenPos : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_DEFAULT vs_default(VS_INPUT_DEFAULT Input)
{
	VS_OUTPUT_DEFAULT Output;
	Output.Position = float4(Input.Position.xy, 0, 1);
	Output.ScreenPos = Input.Texcoord;
	return Output;
}

//Input del Pixel Shader
struct PS_INPUT_DEFAULT
{
	float2 ScreenPos : TEXCOORD0;
};

//Pixel Shader
float4 ps_default(PS_INPUT_DEFAULT Input) : COLOR0
{
	return tex2D(SceneRT, Input.ScreenPos);
}

technique DefaultTechnique
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_default();
	}
}

/**************************************************************************************/
/* DownScale4x4 */
/**************************************************************************************/

float4 ps_DownScale4x4(PS_INPUT_DEFAULT Input) : COLOR
{
	float4 sample = 0.0f;

	for (int i = 0; i < 16; i++)
	{
		sample += tex2D(SceneRT, Input.ScreenPos + sampleOffsets[i]);
	}
	return sample / 16;
}

technique DownScale4x4
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_DownScale4x4();
	}
}

/**************************************************************************************/
/* SampleAvgLuminance_Init */
/**************************************************************************************/

//Hacer un downsampling de 3x3 con un promedio de log(luminance)
float4 ps_SampleLumInitial(PS_INPUT_DEFAULT Input) : COLOR
{
	float3 vSample = 0.0f;
	float  fLogLumSum = 0.0f;

	for (int iSample = 0; iSample < 9; iSample++)
	{
		// Compute the sum of log(luminance) throughout the sample points
		vSample = tex2D(SceneRT, Input.ScreenPos + sampleOffsets[iSample]);
		fLogLumSum += log(dot(vSample, LUMINANCE_VECTOR) + 0.0001f);
	}

	// Divide the sum to complete the average
	fLogLumSum /= 9;
	return float4(fLogLumSum, fLogLumSum, fLogLumSum, 1.0f);
}

technique SampleAvgLuminance_Init
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_SampleLumInitial();
	}
}

/**************************************************************************************/
/* SampleAvgLuminance_Intermediate */
/**************************************************************************************/

//Downsampling de 4x4 de luminance
float4 ps_SampleLumIterative(PS_INPUT_DEFAULT Input) : COLOR
{
	float fResampleSum = 0.0f;

	for (int iSample = 0; iSample < 16; iSample++)
	{
		// Compute the sum of luminance throughout the sample points
		fResampleSum += tex2D(SceneRT, Input.ScreenPos + sampleOffsets[iSample]);
	}

	// Divide the sum to complete the average
	fResampleSum /= 16;
	return float4(fResampleSum, fResampleSum, fResampleSum, 1.0f);
}

technique SampleAvgLuminance_Intermediate
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_SampleLumIterative();
	}
}

/**************************************************************************************/
/* SampleAvgLuminance_End */
/**************************************************************************************/

//Downsampling de 4x4 final y le aplicamos exp al resultado para obtener el luminance promedio de toda la escena
float4 ps_SampleLumFinal(PS_INPUT_DEFAULT Input) : COLOR
{
	float fResampleSum = 0.0f;

	for (int iSample = 0; iSample < 16; iSample++)
	{
		// Compute the sum of luminance throughout the sample points
		fResampleSum += tex2D(SceneRT, Input.ScreenPos + sampleOffsets[iSample]);
	}

	// Divide the sum to complete the average, and perform an exp() to complete
	// the average luminance calculation
	fResampleSum = exp(fResampleSum / 16);
	return float4(fResampleSum, fResampleSum, fResampleSum, 1.0f);
}

technique SampleAvgLuminance_End
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_SampleLumFinal();
	}
}

/**************************************************************************************/
/* BrightPass */
/**************************************************************************************/

//Quedarse con los pixels que superan un umbral de brillo
float4 ps_BrightPassFilter(PS_INPUT_DEFAULT Input) : COLOR
{
	float4 vSample = tex2D(SceneRT, Input.ScreenPos);
	float  fAdaptedLum = tex2D(LuminanceRT, float2(0.5f, 0.5f));

	// Determine what the pixel's value will be after tone-mapping occurs
	vSample.rgb *= middleGray / (fAdaptedLum + 0.001f);

	// Subtract out dark pixels
	vSample.rgb -= BRIGHT_PASS_THRESHOLD;

	// Clamp to 0
	vSample = max(vSample, 0.0f);

	// Map the resulting value into the 0 to 1 range. Higher values for
	// BRIGHT_PASS_OFFSET will isolate lights from illuminated scene
	// objects.
	vSample.rgb /= (BRIGHT_PASS_OFFSET + vSample);

	return vSample;
}

technique BrightPass
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_BrightPassFilter();
	}
}

/**************************************************************************************/
/* BloomPass */
/**************************************************************************************/

//Pasada de GaussianBlur horizontal o vertical para generar efecto de Bloom
float4 ps_bloom_pass(PS_INPUT_DEFAULT Input) : COLOR0
{
	float4 vSample = 0.0f;
	float4 vColor = 0.0f;

	float2 vSamplePosition;

	// Perform a one-directional gaussian blur
	for (int iSample = 0; iSample < MAX_SAMPLES; iSample++)
	{
		vSamplePosition = Input.ScreenPos + gauss_offsets[iSample];
		vColor = tex2D(BloomRT, vSamplePosition);
		vSample += gauss_weights[iSample] * vColor;
	}

	return vSample;
}

technique BloomPass
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_bloom_pass();
	}
}

/**************************************************************************************/
/* FinalRender */
/**************************************************************************************/

//Juntar todo en el render final
float4 ps_finalPass(PS_INPUT_DEFAULT Input, uniform float toneMapping) : COLOR0
{
	float4 origColor = tex2D(SceneRT, Input.ScreenPos);
	float4 bloomColor = tex2D(BloomRT, Input.ScreenPos);
	float avgLum = tex2D(LuminanceRT, float2(0.5f, 0.5f));

	//Tone mapping
	if (toneMapping > 0)
	{
		origColor.rgb *= middleGray / (avgLum + 0.001f);
		origColor.rgb /= (1.0f + origColor);
	}

	//Bloom
	origColor += bloomColor;

	return origColor;
}

technique FinalRender
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_finalPass(1);
	}
}

technique FinalRenderNoToneMapping
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_finalPass(0);
	}
}