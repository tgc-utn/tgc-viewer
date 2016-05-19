/*
* Shader para ejemplo de GaussianBlur
*/

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Scene Render Target
texture texSceneRT;
sampler SceneRT = sampler_state
{
	Texture = (texSceneRT);
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
};

//Offsets y weights de Gaussian Blur
static const int MAX_SAMPLES = 15;
float2 gauss_offsets[MAX_SAMPLES];
float gauss_weights[MAX_SAMPLES];

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

	//Proyectar posicion
	Output.Position = float4(Input.Position.xy, 0, 1);

	//La coordenada de textura del quad es la posicion en 2D
	Output.ScreenPos = Input.Texcoord;

	return(Output);
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
/* GaussianBlurPass */
/**************************************************************************************/

//Pasada de GaussianBlur horizontal o vertical
float4 ps_gaussian_blur_pass(PS_INPUT_DEFAULT Input) : COLOR0
{
	float4 vSample = 0.0f;
	float4 vColor = 0.0f;

	float2 vSamplePosition;

	// Perform a one-directional gaussian blur
	for (int iSample = 0; iSample < MAX_SAMPLES; iSample++)
	{
		vSamplePosition = Input.ScreenPos + gauss_offsets[iSample];
		vColor = tex2D(SceneRT, vSamplePosition);
		vSample += gauss_weights[iSample] * vColor;
	}

	return vSample;
}

technique GaussianBlurPass
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_default();
		PixelShader = compile ps_2_0 ps_gaussian_blur_pass();
	}
}