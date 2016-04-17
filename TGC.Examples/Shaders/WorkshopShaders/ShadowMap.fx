// ---------------------------------------------------------
// Sombras en el image space con la tecnica de Shadows Map
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

#define SMAP_SIZE 1024
#define EPSILON 0.05f

float time = 0;

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3   g_vLightPos;  // posicion de la luz (en World Space) = pto que representa patch emisor Bj
float3   g_vLightDir;  // Direcion de la luz (en World Space) = normal al patch Bj

texture  g_txShadow;	// textura para el shadow map
sampler2D g_samShadow =
sampler_state
{
	Texture = <g_txShadow>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position :        POSITION0;
	float2 Texcoord :        TEXCOORD0;
	float3 Norm :			TEXCOORD1;		// Normales
	float3 Pos :   			TEXCOORD2;		// Posicion real 3d
};

//-----------------------------------------------------------------------------
// Vertex Shader que implementa un shadow map
//-----------------------------------------------------------------------------
void VertShadow(float4 Pos : POSITION,
	float3 Normal : NORMAL,
	out float4 oPos : POSITION,
	out float2 Depth : TEXCOORD0)
{
	// transformacion estandard
	oPos = mul(Pos, matWorld);					// uso el del mesh
	oPos = mul(oPos, g_mViewLightProj);		// pero visto desde la pos. de la luz

	// devuelvo: profundidad = z/w
	Depth.xy = oPos.zw;
}

//-----------------------------------------------------------------------------
// Pixel Shader para el shadow map, dibuja la "profundidad"
//-----------------------------------------------------------------------------
void PixShadow(float2 Depth : TEXCOORD0, out float4 Color : COLOR)
{
	// parche para ver el shadow map
	//float k = Depth.x/Depth.y;
	//Color = (1-k);
	Color = Depth.x / Depth.y;
}

technique RenderShadow
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertShadow();
		PixelShader = compile ps_3_0 PixShadow();
	}
}

//-----------------------------------------------------------------------------
// Vertex Shader para dibujar la escena pp dicha con sombras
//-----------------------------------------------------------------------------
void VertScene(float4 iPos : POSITION,
	float2 iTex : TEXCOORD0,
	float3 iNormal : NORMAL,
	out float4 oPos : POSITION,
	out float2 Tex : TEXCOORD0,
	out float4 vPos : TEXCOORD1,
	out float3 vNormal : TEXCOORD2,
	out float4 vPosLight : TEXCOORD3
)
{
	// transformo al screen space
	oPos = mul(iPos, matWorldViewProj);

	// propago coordenadas de textura
	Tex = iTex;

	// propago la normal
	vNormal = mul(iNormal, (float3x3)matWorldView);

	// propago la posicion del vertice en World space
	vPos = mul(iPos, matWorld);
	// propago la posicion del vertice en el espacio de proyeccion de la luz
	vPosLight = mul(vPos, g_mViewLightProj);
}

//-----------------------------------------------------------------------------
// Pixel Shader para dibujar la escena
//-----------------------------------------------------------------------------
float4 PixScene(float2 Tex : TEXCOORD0,
	float4 vPos : TEXCOORD1,
	float3 vNormal : TEXCOORD2,
	float4 vPosLight : TEXCOORD3
) :COLOR
{
	float3 vLight = normalize(float3(vPos - g_vLightPos));
	float cono = dot(vLight, g_vLightDir);
	float4 K = 0.0;
	if (cono > 0.7)
	{
		// coordenada de textura CT
		float2 CT = 0.5 * vPosLight.xy / vPosLight.w + float2(0.5, 0.5);
		CT.y = 1.0f - CT.y;

		// sin ningun aa. conviene con smap size >= 512
		float I = (tex2D(g_samShadow, CT) + EPSILON < vPosLight.z / vPosLight.w) ? 0.0f : 1.0f;

		// interpolacion standard bi-lineal del shadow map
		// CT va de 0 a 1, lo multiplico x el tamaño de la textura
		// la parte fraccionaria indica cuanto tengo que tomar del vecino
		// conviene cuando el smap size = 256
		// leo 4 valores
		/*float2 vecino = frac( CT*SMAP_SIZE);
		float prof = vPosLight.z / vPosLight.w;
		float s0 = (tex2D( g_samShadow, float2(CT)) + EPSILON < prof)? 0.0f: 1.0f;
		float s1 = (tex2D( g_samShadow, float2(CT) + float2(1.0/SMAP_SIZE,0))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float s2 = (tex2D( g_samShadow, float2(CT) + float2(0,1.0/SMAP_SIZE))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float s3 = (tex2D( g_samShadow, float2(CT) + float2(1.0/SMAP_SIZE,1.0/SMAP_SIZE))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float I = lerp( lerp( s0, s1, vecino.x ),lerp( s2, s3, vecino.x ),vecino.y);
		*/

		/*
		// anti-aliasing del shadow map
		float I = 0;
		float r = 2;
		for(int i=-r;i<=r;++i)
			for(int j=-r;j<=r;++j)
				I += (tex2D( g_samShadow, CT + float2((float)i/SMAP_SIZE, (float)j/SMAP_SIZE) ) + EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;
		I /= (2*r+1)*(2*r+1);
		*/

		if (cono < 0.8)
			I *= 1 - (0.8 - cono) * 10;

		K = I;
	}

	float4 color_base = tex2D(diffuseMap, Tex);
	color_base.rgb *= 0.5 + 0.5*K;
	return color_base;
}

technique RenderScene
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertScene();
		PixelShader = compile ps_3_0 PixScene();
	}
}