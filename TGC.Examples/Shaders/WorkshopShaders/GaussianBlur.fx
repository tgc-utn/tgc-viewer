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

float screen_dx; // tamaño de la pantalla en pixels
float screen_dy;
float KLum = 1; // factor de luminancia

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
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture g_GlowMap;
sampler GlowMap =
sampler_state
{
    Texture = <g_GlowMap>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};


// textura de 1 x 1 que tiene el valor promedio de luminance
texture g_Luminance;
sampler Luminance =
sampler_state
{
    Texture = <g_Luminance>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
};

// textura de 1 x 1 que tiene el valor promedio de luminance
texture g_Luminance_ant;
sampler Luminance_ant =
sampler_state
{
    Texture = <g_Luminance_ant>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
};

// Depth of field
float zn = 1; // near plane
float zf = 10000; // far plane
float zfoco = 300; // focus plane
float blur_k = 0.5; // factor de desenfoque

texture g_BlurFactor;
sampler BlurFactor =
sampler_state
{
    Texture = <g_BlurFactor>;
    MipFilter = Point;
    MinFilter = Point;
    MagFilter = Point;
};

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
	//Obtener el texel de textura
    float4 fvBaseColor = tex2D(diffuseMap, Texcoord);
	// aplico el factor de luminancia
    fvBaseColor.rgb *= KLum;
    return fvBaseColor;
}

technique DefaultTechnique
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}

// dibuja negro (tiene que ocultar lo que esta oscuro)
float4 PSOscuros(float3 Texcoord : TEXCOORD0) : COLOR0
{
    float4 fvBaseColor = tex2D(diffuseMap, Texcoord);
    return float4(0, 0, 0, fvBaseColor.a);
}

technique DibujarObjetosOscuros
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 PSOscuros();
    }
}

void VSCopy(float4 vPos : POSITION, float2 vTex : TEXCOORD0, out float4 oPos : POSITION, out float2 oScreenPos : TEXCOORD0)
{
    oPos = vPos;
    oScreenPos = vTex;
    oPos.w = 1;
}

// Gaussian Blur

static const int kernel_r = 6;
static const int kernel_size = 13;
static const float Kernel[kernel_size] =
{
    0.002216, 0.008764, 0.026995, 0.064759, 0.120985, 0.176033, 0.199471, 0.176033, 0.120985, 0.064759, 0.026995, 0.008764, 0.002216,
};

void Blur(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        for (int j = 0; j < kernel_size; ++j)
            Color += tex2D(RenderTarget, screen_pos + float2((float) (i - kernel_r) / screen_dx, (float) (j - kernel_r) / screen_dy)) * Kernel[i] * Kernel[j];
    Color.a = 1;
}

technique GaussianBlur
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 Blur();
    }
}

void BlurH(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        Color += tex2D(RenderTarget, screen_pos + float2((float) (i - kernel_r) / screen_dx, 0)) * Kernel[i];
    Color.a = 1;
}

void BlurV(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        Color += tex2D(RenderTarget, screen_pos + float2(0, (float) (i - kernel_r) / screen_dy)) * Kernel[i];
    Color.a = 1;
}

technique GaussianBlurSeparable
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 BlurH();
    }
    pass Pass_1
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 BlurV();
    }
}

float4 PSDownFilter4(in float2 Tex : TEXCOORD0) : COLOR0
{
    float4 Color = 0;
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            Color += tex2D(RenderTarget, Tex + float2((float) i / screen_dx, (float) j / screen_dy));

    return Color / 16;
}

technique DownFilter4
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSDownFilter4();
    }
}

float4 PSGrayScale(in float2 Tex : TEXCOORD0, in float2 vpos : VPOS) : COLOR0
{
    float4 ColorBase = tex2D(RenderTarget, Tex);
    float4 ColorBrillante = tex2D(GlowMap, Tex + float2((float) 16 / screen_dx, (float) 16 / screen_dy));
	// Y = 0.2126 R + 0.7152 G + 0.0722 B
    float Yb = 0.2126 * ColorBase.r + 0.7152 * ColorBase.g + 0.0722 * ColorBase.b;
    float Yk = 0.2126 * ColorBrillante.r + 0.7152 * ColorBrillante.g + 0.0722 * ColorBrillante.b;
    if (round(vpos.y / 2) % 2 == 0)
    {
        Yb *= 0.85;
        Yk *= 0.85;
    }

    return float4(Yk * 0.75, Yb * 0.6 + Yk * 4, Yk * 0.75, 1);
}

technique GrayScale
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSGrayScale();
    }

}

static const float4 LUM_VECTOR = float4(.299, .587, .114, 0);
static const float MIDDLE_GRAY = 0.72f;
static const float LUM_WHITE = 1.5f;
float pupila_time = 0;
bool glow = true;
int tone_mapping_izq = 1;
int tone_mapping_der = 0;
bool pantalla_completa = true;

float4 PSToneMapping(in float2 Tex : TEXCOORD0, in float2 vpos : VPOS) : COLOR0
{
    int tone_mapping;
    if (vpos.x < screen_dx / 2)
    {
		// Pantalla izquierda
        tone_mapping = tone_mapping_izq;
    }
    else
    {
		// Pantalla derecha
        if (pantalla_completa)
        {
			// Pantalla completa, 
            tone_mapping = tone_mapping_izq;
        }
        else
        {
			// Pantalla dividida
            tone_mapping = tone_mapping_der;
            Tex.x -= 0.5;
        }
    }
	
    float vLum = dot(tex2D(Luminance, float2(0, 0)), LUM_VECTOR);
    float vLumAnt = dot(tex2D(Luminance_ant, float2(0, 0)), LUM_VECTOR);
    float Yk = lerp(vLumAnt, vLum, pupila_time);
    float4 ColorBase = tex2D(RenderTarget, Tex);
    float4 ColorBrillante = glow && tone_mapping ? tex2D(GlowMap, Tex + float2((float) 16 / screen_dx, (float) 16 / screen_dy)) : 0;


	// Tone mapping
    if (tone_mapping == 1)
    {
		// Reinhard 
        ColorBase.rgb = ColorBase.rgb / (1 + ColorBase.rgb);
        ColorBase.rgb *= MIDDLE_GRAY / (Yk + 0.001f);
    }
    else if (tone_mapping == 2)
    {
		// Modified Reinhard
        Yk *= 1.5;
        ColorBase.rgb = (ColorBase.rgb * (1 + ColorBase.rgb / Yk * Yk)) / (1 + ColorBase.rgb);
        ColorBase.rgb *= MIDDLE_GRAY / (Yk + 0.001f);
    }
    else if (tone_mapping == 3)
    {
		// logaritmico
        ColorBase.rgb = log(ColorBase.rgb + 1) / log(1.2 * Yk + 1);
    }
    else if (tone_mapping == 4)
    {
		// falta averiguar el nombre
        ColorBase.rgb *= MIDDLE_GRAY / (Yk + 0.001f);
        ColorBase.rgb *= (1.0f + ColorBase / LUM_WHITE);
        ColorBase.rgb /= (1.0f + ColorBase);
    }

	// combino con glow 
    float4 rta = float4(ColorBase.rgb + 2.6 * ColorBrillante.rgb, 1);
    return rta;
}


technique ToneMapping
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSToneMapping();
    }

}

// --------------------------------------------------------------------------------
struct VS_OUTPUT2
{
    float4 Position : POSITION; // vertex position 
    float4 Pos : TEXCOORD0; // distancia a la camara
};

VS_OUTPUT2 BlurFactorVS(float4 vPos : POSITION, float3 vNormal : NORMAL,
                         float4 vTexCoord0 : TEXCOORD0)
{
    VS_OUTPUT2 Output;
    Output.Pos = Output.Position = mul(vPos, matWorldViewProj);
    return Output;
}

// Pixel shader que genera el blur map
float4 BlurFactorPS(float4 Pos : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);
    float z = Pos.w;
    float k = clamp(blur_k * abs(z - zfoco) / (zf - zn), 0, 1);
    color.rgb = k;
    return color;
}

// 
technique RenderBlurFactor
{
    pass P0
    {
        VertexShader = compile vs_3_0 BlurFactorVS();
        PixelShader = compile ps_3_0 BlurFactorPS();
    }
}

// Gaussian blur 
float4 PSBlur(float2 TextureUV : TEXCOORD0) : COLOR0
{
    int blur_factor = tex2Dlod(BlurFactor, float4(TextureUV.xy, 0, 0)).r * 255;
    int r = clamp(blur_factor, 0, 5);
    float4 p = 0;
    int cant_muestras = 0;
    for (int i = -r; i <= r; ++i)
        for (int j = -r; j <= r; ++j)
        {
		// Obtengo el blur factor de la muestra
            float4 CT = float4((TextureUV + float2(i / screen_dx, j / screen_dy)).xy, 0, 0);
            int blur_factor_muestra = tex2Dlod(BlurFactor, CT).r * 255;
            int rm = clamp(blur_factor_muestra, 0, 5);
		
		// Para poder utilizar este punto como muestra valida, su radio de influencia
		// tiene que ser mayor o igual a la distancia con respecto al centro
            if (rm * rm >= i * i + j * j)
            {
			// tomo el color de la muestra:
                p += tex2Dlod(RenderTarget, CT);
                ++cant_muestras;
            }
        }

    p /= (float) cant_muestras;

    return float4(p.xyz, 1);
}

technique DepthOfField
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSBlur();
    }
}