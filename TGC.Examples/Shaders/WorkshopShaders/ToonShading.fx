// ---------------------------------------------------------
// Ejemplo toon Shading
// ---------------------------------------------------------

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

float3 fvLightPosition = float3(-100.00, 100.00, -100.00);
float3 fvEyePosition = float3(0.00, 0.00, -100.00);
float k_la = 0.3; // luz ambiente global
float k_ld = 0.9; // luz difusa
float k_ls = 0.4; // luz specular
float fSpecularPower = 16.84;

float screen_dx; // tamaño de la pantalla en pixels
float screen_dy;

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

texture g_RenderTarget;
sampler RenderTarget =
sampler_state
{
    Texture = <g_RenderTarget>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

// mapa de normales
texture g_Normals; // mapa de normales
sampler Normales =
sampler_state
{
    Texture = <g_Normals>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

/**************************************************************************************/
/* DefaultTechnique */
/**************************************************************************************/

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Norm : TEXCOORD1; // Normales
    float3 Pos : TEXCOORD2; // Posicion real 3d
};

//Vertex Shader
VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);

	//Las Texcoord quedan igual
    Output.Texcoord = Input.Texcoord;

	// Calculo la posicion real
    float4 pos_real = mul(Input.Position, matWorld);
    Output.Pos = float3(pos_real.x, pos_real.y, pos_real.z);

	// Transformo la normal y la normalizo
	//Output.Norm = normalize(mul(Input.Normal,matInverseTransposeWorld));
    Output.Norm = normalize(mul(Input.Normal, matWorld));
    return (Output);
}

//Pixel Shader
float4 ps_main(float3 Texcoord : TEXCOORD0, float3 N : TEXCOORD1,
	float3 Pos : TEXCOORD2) : COLOR0
{
    float ld = 0; // luz difusa
    float le = 0; // luz specular

	// 	normalizo la normal
    N = normalize(N);

	// 1- calculo la luz diffusa
    float3 LD = normalize(fvLightPosition - float3(Pos.x, Pos.y, Pos.z));
    ld += saturate(dot(N, LD)) * k_ld;

	// 2- calcula la reflexion specular
    float3 D = normalize(float3(Pos.x, Pos.y, Pos.z) - fvEyePosition);
    float ks = saturate(dot(reflect(LD, N), D));
    ks = pow(ks, fSpecularPower);
    le += ks * k_ls;

	//Obtener el texel de textura
	//float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );
	//voy a usar un color listo:
    float4 fvBaseColor = float4(1, 0.5, 0.5, 1);

	// suma luz diffusa, ambiente y especular
    float4 RGBColor = 0;
    RGBColor.rgb = saturate(fvBaseColor * (saturate(k_la + ld)) + le);
    return RGBColor;
}

technique DefaultTechnique
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}

/**************************************************************************************/
/* CopyScreen */
/**************************************************************************************/

void VSCopy(float4 vPos : POSITION,
	float2 vTex : TEXCOORD0,
	out float4 oPos : POSITION,
	out float2 oScreenPos : TEXCOORD0)
{
    oPos = vPos;
    oScreenPos = vTex;
    oPos.w = 1;
}

// pixel copy
void PSCopy(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    float4 c0 = tex2D(RenderTarget, screen_pos);
    float4 c1 = tex2D(RenderTarget, screen_pos + float2(0, 15 / screen_dy));
    float4 c2 = tex2D(RenderTarget, screen_pos + float2(15 / screen_dx, 0));
    float4 c3 = tex2D(RenderTarget, screen_pos + float2(15 / screen_dx, 15 / screen_dy));

    float4 c4 = tex2D(RenderTarget, screen_pos);
    float4 c5 = tex2D(RenderTarget, screen_pos + float2(0, 35 / screen_dy));
    float4 c6 = tex2D(RenderTarget, screen_pos + float2(35 / screen_dx, 0));
    float4 c7 = tex2D(RenderTarget, screen_pos + float2(35 / screen_dx, 35 / screen_dy));

    Color = (c0 + c1 + c2 + c3 + c4 + c5 + c6 + c7) / 8;
    Color.a = 1;
}

technique CopyScreen
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSCopy();
    }
}

/**************************************************************************************/
/* NormalMap */
/**************************************************************************************/

// Pixel shader que genera el mapa de Normales
void NormalMapPS(float3 Norm : TEXCOORD1, out float4 Color : COLOR)
{
    Color.a = 1;
    Color.r = Norm.x;
    Color.g = Norm.y;
    Color.b = Norm.z;
}

// edge detect
void PSEdge(float2 screen_pos : TEXCOORD0,
	out float4 Color : COLOR)
{
    float4 pixel = tex2D(RenderTarget, screen_pos);

    float ep = 0.5;
    float4 c0 = tex2D(Normales, screen_pos);
    float4 c1 = tex2D(Normales, screen_pos + float2(0, 1 / screen_dy));
    int flag = 0;
    Color.a = 1;
    Color.rgb = 0;
    if (distance(c0, c1) > ep)
        flag = 1;
    else
    {
        c1 = tex2D(Normales, screen_pos + float2(1 / screen_dy, 0));
        if (distance(c0, c1) > ep)
            flag = 1;
        else
        {
            c1 = tex2D(Normales, screen_pos + float2(2 / screen_dy, 0));
            if (distance(c0, c1) > ep)
                flag = 2;
            else
            {
                c1 = tex2D(Normales, screen_pos + float2(0, 2 / screen_dy));
                if (distance(c0, c1) > ep)
                    flag = 2;
            }
        }
    }

    if (flag == 0)
        Color.rgb = round(pixel.rgb * 3) / 3;
    else if (flag == 1)
        Color.rgb = 1;
    else if (flag == 2)
        Color.r = 1;
}

// Genera el mapa de normales
technique NormalMap
{
    pass P0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 NormalMapPS();
    }
}

/**************************************************************************************/
/* EdgeDetect */
/**************************************************************************************/

technique EdgeDetect
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSEdge();
    }
}