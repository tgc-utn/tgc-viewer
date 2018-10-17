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

// variable de fogs
float4 ColorFog;
float4 CameraPos;
float StartFogDistance;
float EndFogDistance;
float Density;

//Input del Vertex Shader
struct VS_INPUT_VERTEX
{
    float4 Position : POSITION0;
    float3 Texture : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
    float4 PosView : COLOR0;
};

//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
    VS_OUTPUT_VERTEX output;

	//Proyectar posicion
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texture;
    output.PosView = mul(input.Position, matWorldView);
    return output;
}

//Input del Pixel Shader
struct PS_INPUT_PIXEL
{
    float2 Texture : TEXCOORD0;
    float1 Fog : FOG;
};

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
    float zn = StartFogDistance;
    float zf = EndFogDistance;

    float4 fvBaseColor = tex2D(diffuseMap, input.Texture);
    if (input.PosView.z < zn)
        return fvBaseColor;
    else if (input.PosView.z > zf)
    {
        fvBaseColor = ColorFog;
        return fvBaseColor;
    }
    else
    {
		// combino fog y textura
        float1 total = zf - zn;
        float1 resto = input.PosView.z - zn;
        float1 proporcion = resto / total;
        fvBaseColor = lerp(fvBaseColor, ColorFog, proporcion);
        return fvBaseColor;
    }
}

// ------------------------------------------------------------------
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}