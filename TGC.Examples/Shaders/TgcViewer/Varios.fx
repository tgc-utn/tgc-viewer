/*
* Shader con tecnicas varias utilizadas por diversas herramientas del framework,
* como: TgcBox, TgcArrow, TgcPlaneWall, TgcBoundingBox, TgcBoundingSphere, etc.
* Hay varias Techniques, una para cada combinación utilizada en el framework de formato de vertice:
*	- PositionColoredTextured
*	- PositionTextured
*	- PositionColored
*	- PositionColoredAlpha
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

//Factor de translucidez
float alphaValue = 1;


/**************************************************************************************/
/* PositionColoredTextured */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_PositionColoredTextured
{
   float4 Position : POSITION0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_PositionColoredTextured
{
   float4 Position : POSITION0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};


//Vertex Shader
VS_OUTPUT_PositionColoredTextured vs_PositionColoredTextured(VS_INPUT_PositionColoredTextured input)
{
	VS_OUTPUT_PositionColoredTextured output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_PositionColoredTextured
{
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;   
};

//Pixel Shader
float4 ps_PositionColoredTextured(PS_INPUT_PositionColoredTextured input) : COLOR0
{      
	return input.Color * tex2D(diffuseMap, input.Texcoord);
}

/*
* Technique PositionColoredTextured
*/
technique PositionColoredTextured
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_PositionColoredTextured();
	  PixelShader = compile ps_2_0 ps_PositionColoredTextured();
   }
}



/**************************************************************************************/
/* PositionTextured */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_PositionTextured
{
   float4 Position : POSITION0;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_PositionTextured
{
   float4 Position : POSITION0;
   float2 Texcoord : TEXCOORD0;
};


//Vertex Shader
VS_OUTPUT_PositionTextured vs_PositionTextured(VS_INPUT_PositionTextured input)
{
	VS_OUTPUT_PositionTextured output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_PositionTextured
{
	float2 Texcoord : TEXCOORD0;   
};

//Pixel Shader
float4 ps_PositionTextured(PS_INPUT_PositionTextured input) : COLOR0
{      
	return tex2D(diffuseMap, input.Texcoord);
}

/*
* Technique PositionTextured
*/
technique PositionTextured
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_PositionTextured();
	  PixelShader = compile ps_2_0 ps_PositionTextured();
   }
}


/**************************************************************************************/
/* PositionColored */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_PositionColored
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
};

//Output del Vertex Shader
struct VS_OUTPUT_PositionColored
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
};


//Vertex Shader
VS_OUTPUT_PositionColored vs_PositionColored(VS_INPUT_PositionColored input)
{
	VS_OUTPUT_PositionColored output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_PositionColored
{
	float4 Color : COLOR0;   
};

//Pixel Shader
float4 ps_PositionColored(PS_INPUT_PositionColored input) : COLOR0
{      
	return input.Color;
}

/*
* Technique PositionColored
*/
technique PositionColored
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_PositionColored();
	  PixelShader = compile ps_2_0 ps_PositionColored();
   }
}




/**************************************************************************************/
/* PositionColoredAlpha */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_PositionColoredAlpha
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
};

//Output del Vertex Shader
struct VS_OUTPUT_PositionColoredAlpha
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
};


//Vertex Shader
VS_OUTPUT_PositionColoredAlpha vs_PositionColoredAlpha(VS_INPUT_PositionColoredAlpha input)
{
	VS_OUTPUT_PositionColoredAlpha output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_PositionColoredAlpha
{
	float4 Color : COLOR0;   
};

//Pixel Shader
float4 ps_PositionColoredAlpha(PS_INPUT_PositionColoredAlpha input) : COLOR0
{      
	return float4(input.Color.rgb, alphaValue);
}

/*
* Technique PositionColoredAlpha
*/
technique PositionColoredAlpha
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_PositionColoredAlpha();
	  PixelShader = compile ps_2_0 ps_PositionColoredAlpha();
   }
}

