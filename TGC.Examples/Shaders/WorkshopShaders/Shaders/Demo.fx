// ---------------------------------------------------------
// demo shaders
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
float time = 0;

// Fresnel
float FBias = 0.4;
int FPower = 1;
float FEscala = 0.5;

// Agua
float2 vortice = float2(0, -100);
float canoa_x = 0;
float canoa_y = 0;
float fHeightMapScale = 0.2;

float k_la = 0.7;							// luz ambiente global
float k_ld = 0.4;							// luz difusa
float k_ls = 1.0;							// luz specular
float fSpecularPower = 16.84;

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3   g_vLightPos;  // posicion de la luz (en World Space) = pto que representa patch emisor Bj
float3   g_vLightDir;  // Direcion de la luz (en World Space) = normal al patch Bj

// Textura auxiliar:
texture aux_Tex;
sampler2D auxMap =
sampler_state
{
	Texture = (aux_Tex);
	ADDRESSU = MIRROR;
	ADDRESSV = MIRROR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

// enviroment map
texture  g_txCubeMap;
samplerCUBE g_samCubeMap =
sampler_state
{
	Texture = <g_txCubeMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

// enviroment map
texture  g_txCubeMapAgua;
samplerCUBE g_samCubeMapAgua =
sampler_state
{
	Texture = <g_txCubeMapAgua>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

// ------------------------------------
// Shadow map
#define SMAP_SIZE 512
#define EPSILON 0.000005f

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

/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position :        POSITION0;
	float2 Texcoord :        TEXCOORD0;
	float3 Norm :			TEXCOORD1;		// Normales
	float3 Pos :   			TEXCOORD2;		// Posicion real 3d
};

//Vertex Shader
VS_OUTPUT vs_main(float4 Pos:POSITION, float3 Normal : NORMAL, float2 Texcoord : TEXCOORD0)
{
	VS_OUTPUT Output;
	//Proyectar posicion
	Output.Position = mul(Pos, matWorldViewProj);
	//Propago  las coord. de textura
	Output.Texcoord = Texcoord;
	// Calculo la posicion real
	Output.Pos = mul(Pos, matWorld).xyz;
	// Transformo la normal y la normalizo
	Output.Norm = normalize(mul(Normal, matInverseTransposeWorld));
	//Output.Norm = normalize(mul(Normal,matWorld));
	return(Output);
}

//Pixel Shader
float4 ps_main(float2 Texcoord: TEXCOORD0, float3 N : TEXCOORD1,
	float3 Pos : TEXCOORD2) : COLOR0
{
	float ld = 0;		// luz difusa
	float le = 0;		// luz specular

	// corrijo la normal
	float aux = N.y;
	N.y = N.z;
	N.z = aux;
	N = normalize(N);

	// for(int =0;i<cant_ligths;++i)
	// 1- calculo la luz diffusa
	float3 LD = normalize(fvLightPosition - float3(Pos.x,Pos.y,Pos.z));
	ld += saturate(dot(N, LD))*k_ld;

	// 2- calcula la reflexion specular
	float3 D = normalize(float3(Pos.x,Pos.y,Pos.z) - fvEyePosition);
	float ks = saturate(dot(reflect(LD,N), D));
	ks = pow(ks,fSpecularPower);
	le += ks*k_ls;

	//Obtener el texel de textura
	float4 fvBaseColor = tex2D(diffuseMap, Texcoord);

	// suma luz diffusa, ambiente y especular
	float4 RGBColor = 0;
	RGBColor.rgb = saturate(fvBaseColor*(saturate(k_la + ld)) + le);
	return RGBColor;
}

technique RenderScene
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_main();
	}
}

/**************************************************************************************/
/* RenderCubeMap */
/**************************************************************************************/

void VSCubeMap(float4 Pos : POSITION,
	float3 Normal : NORMAL,
	float2 Texcoord : TEXCOORD0,
	out float4 oPos : POSITION,
	out float3 EnvTex : TEXCOORD0,
	out float2 Tex : TEXCOORD2,
	out float3 N : TEXCOORD1,
	out float3 EnvTex1 : TEXCOORD3,
	out float3 EnvTex2 : TEXCOORD4,
	out float3 EnvTex3 : TEXCOORD5,
	out float3 wPos : TEXCOORD6,
	out float  Fresnel : COLOR
)
{
	wPos = mul(Pos, matWorld);
	float3 vEyeR = normalize(wPos - fvEyePosition);

	// corrijo la normal (depende de la malla)
	//Normal*= -1;
	float3 vN = mul(Normal, (float3x3)matWorld);
	vN = normalize(vN);
	EnvTex = reflect(vEyeR, vN);

	// Refraccion de la luz
	EnvTex1 = refract(vEyeR, vN, 1.001);
	EnvTex2 = refract(vEyeR, vN, 1.009);
	EnvTex3 = refract(vEyeR, vN, 1.02);
	Fresnel = FBias + FEscala*pow(1 + dot(vEyeR, vN), FPower);

	// proyecto
	oPos = mul(Pos, matWorldViewProj);

	//Propago la textura
	Tex = Texcoord;

	//Propago la normal
	N = vN;
}

float4 PSCubeMap(float3 EnvTex: TEXCOORD0,
	float3 N : TEXCOORD1,
	float3 Texcoord : TEXCOORD2,
	float3 Tex1 : TEXCOORD3,
	float3 Tex2 : TEXCOORD4,
	float3 Tex3 : TEXCOORD5,
	float Fresnel : COLOR,
	float3 wPos : TEXCOORD6
) : COLOR0
{
	float ld = 0;		// luz difusa
	float le = 0;		// luz specular

	N = normalize(N);

	// 1- calculo la luz diffusa
	float3 LD = normalize(fvLightPosition - wPos);
	ld += saturate(dot(N, LD))*k_ld;

	// 2- calcula la reflexion specular
	float3 D = normalize(wPos - fvEyePosition);
	float ks = saturate(dot(reflect(LD,N), D));
	ks = pow(ks,fSpecularPower);
	le += ks*k_ls;

	//Obtener el texel de textura
	float k = 0.3;
	float4 fvBaseColor = k*texCUBE(g_samCubeMap, EnvTex) +
						(1 - k)*tex2D(diffuseMap, Texcoord);

	// suma luz diffusa, ambiente y especular
	fvBaseColor.rgb = saturate(fvBaseColor*(saturate(k_la + ld)) + le);
	fvBaseColor.a = 1;
	float4 color_reflejado = fvBaseColor;

	/*float4 color_refractado = float4(
		texCUBE( g_samCubeMap, Tex1).x,
		texCUBE( g_samCubeMap, Tex2).y,
		texCUBE( g_samCubeMap, Tex3).z,
		1);
	*/

	float4 color_refractado = texCUBE(g_samCubeMap, Tex1);

	//return color_reflejado*Fresnel + color_refractado*(1-Fresnel);
	//return color_refractado;
	return color_reflejado;
}

technique RenderCubeMap
{
	pass p0
	{
		VertexShader = compile vs_3_0 VSCubeMap();
		PixelShader = compile ps_3_0 PSCubeMap();
	}
}

/**************************************************************************************/
/* RenderAgua */
/**************************************************************************************/

void VSAgua(float4 Pos : POSITION,
	float2 Texcoord : TEXCOORD0,
	float3 normal : NORMAL,
	out float4 oPos : POSITION,
	out float2 Tex : TEXCOORD0,
	out float3 tsEye : TEXCOORD1,
	out float3 tsPos : TEXCOORD2,
	out float3 wsPos : TEXCOORD3,
	out float4 vPosLight : TEXCOORD4

)
{
	float4 VertexPositionWS = mul(Pos, matWorld);

	wsPos = VertexPositionWS.xyz;		// de paso devuelvo la Posicion en worldspace
	// y de paso propago la posicion del vertice en el espacio de proyeccion de la luz
	vPosLight = mul(VertexPositionWS, g_mViewLightProj);

	float3 E = fvEyePosition.xyz - wsPos;

	// calculo la tg y la binormal
	//float3 up = float3(0,0,1);
	//float3 tangent = cross(up,normal);
	//float3 binormal = cross(normal,tangent);

	normal = float3(0, 1, 0);
	float3 tangent = float3(0, 0, 1);
	float3 binormal = float3(1, 0, 0);

	float3x3 tangentToWorldSpace;
	tangentToWorldSpace[0] = mul(tangent, matWorld);
	tangentToWorldSpace[1] = mul(binormal, matWorld);
	tangentToWorldSpace[2] = mul(normal, matWorld);

	float3x3 worldToTangentSpace = transpose(tangentToWorldSpace);

	// proyecto
	oPos = mul(Pos, matWorldViewProj);
	//Propago la textura
	Tex = Texcoord;

	// devuelvo la pos. del ojo expresados en tangent space:
	tsEye = mul(E, worldToTangentSpace);
	// devuelvo la pos. del pto en tg space
	tsPos = mul(VertexPositionWS.xyz, worldToTangentSpace);
}

float4 PSAgua(float3 Pos: POSITION,
	float2 Texcoord : TEXCOORD0,
	float3 tsEye : TEXCOORD1,		// estan en tangent space!
	float3 tsPos : TEXCOORD2,
	float3 wsPos : TEXCOORD3,		// pos. en world space
	float4 vPosLight : TEXCOORD4		// pos. en Esp. Proyeccion de la luz

) : COLOR0
{
	//vortice.x = canoa_y;
	//vortice.y = canoa_x;
	float4 vCurrSample;
// uso una funcion de onda para computar la altura del heightmap
// calculos en Tangent space:
float k = 1;
float vel = -5;
float fCurrH = 0;
float x = tsPos.x / 250.0;
float y = tsPos.y / 250.0;
// ondulaciones globales
fCurrH += (0.5 + sin(k*(x + time*vel)) / 2 + 0.5 + cos(k*(y + time*vel)) / 2);
// onda esferica sobre el vortice
fCurrH += sin(k*distance(float2(x,y),vortice) + time*vel);
// onda esferica sobre la canoa
fCurrH += sin(k*distance(float2(x,y),float2(canoa_y,canoa_x)) + time*vel);

/*
// rastro de la canoa
float2 n = normalize(float2(canoa_y,-canoa_x));
float2 p = normalize(float2(y-canoa_x,x-canoa_y));
float dn = dot(n,p);
if(dn<-0.95)
{
	// perturbacion generada por el rastro de la canoa
	float2 pn = float2(canoa_x,canoa_y) + n*dn;
	float dist = distance(p,pn);
	float d2 = clamp(length(p),1,10000);
	//vN.x += 0.01*sin( 2*(dist+time*vel))/d2;
	//vN.y += 0.01*cos( 2*(dist+time*vel))/d2;
	//vN = normalize(vN);
	fCurrH += sin(50*(dist+time*vel));
}
*/

// 	ajusto segun el Hieghmap Scale y un cierto bias
// fCurrH <-- (fCurrH * HeightMapScale + Bias)/tsEye.z;
fCurrH = (fCurrH * 0.04 + 0.01) / tsEye.z;

Texcoord += tsEye.xy * fCurrH;
vCurrSample = tex2D(diffuseMap, Texcoord*0.1);

// enviroment map
float dist = distance(float2(x,y),vortice);
float fp = cos(k*dist + time*vel)*k / dist*0.01;
//float3 vN = float3(0,1,0);
float3 vN = normalize(float3(-fp*(x - vortice.x),1,-fp*(y - vortice.y)));

// necesito los valores en Worlds Space para acceder al cubemap
// Reflexion
float3 vEyeR = normalize(wsPos - fvEyePosition);
float3 EnvTex = reflect(vEyeR,vN);
float4 color_reflejado = texCUBE(g_samCubeMapAgua, EnvTex);
// Refraccion
float3 EnvTex1 = refract(vEyeR,-vN,1.001);
float3 EnvTex2 = refract(vEyeR,-vN,1.009);
float3 EnvTex3 = refract(vEyeR,-vN,1.02);
float4 color_refractado = float4(
		tex2D(auxMap, float2(EnvTex1.x + 1,-EnvTex1.z + 1)*0.5).x,
		tex2D(auxMap, float2(EnvTex2.x + 1,-EnvTex2.z + 1)*0.5).y,
		tex2D(auxMap, float2(EnvTex3.x + 1,-EnvTex3.z + 1)*0.5).z,
		1);

//Combino con el Enviroment map
float Fresnel = 0.1 + 0.1*pow(1 + abs(dot(vN,vEyeR)),7);
float4 fvBaseColor = color_reflejado*Fresnel + color_refractado*(1 - Fresnel);

k = 0.75;
fvBaseColor = k*fvBaseColor + (1 - k)*vCurrSample;
fvBaseColor.a = 0.5 + (1 - Fresnel)*0.5;

// combino con el shadow map
float I = 0.7;
float3 vLight = normalize(float3(wsPos - g_vLightPos));
float cono = dot(vLight, g_vLightDir);
if (cono > 0.001)
{
	// coordenada de textura CT
	float2 CT = 0.5 * vPosLight.xy / vPosLight.w + float2(0.5, 0.5);
	CT.y = 1.0f - CT.y;
	//I = (tex2D( g_samShadow, CT) + EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;

	float2 vecino = frac(CT*SMAP_SIZE);
	float prof = vPosLight.z / vPosLight.w;
	float s0 = (tex2D(g_samShadow, float2(CT)) + EPSILON < prof) ? 0.0f : 1.0f;
	float s1 = (tex2D(g_samShadow, float2(CT)+float2(1.0 / SMAP_SIZE,0))
						+ EPSILON < prof) ? 0.0f : 1.0f;
	float s2 = (tex2D(g_samShadow, float2(CT)+float2(0,1.0 / SMAP_SIZE))
						+ EPSILON < prof) ? 0.0f : 1.0f;
	float s3 = (tex2D(g_samShadow, float2(CT)+float2(1.0 / SMAP_SIZE,1.0 / SMAP_SIZE))
						+ EPSILON < prof) ? 0.0f : 1.0f;
	I += 0.3*lerp(lerp(s0, s1, vecino.x),lerp(s2, s3, vecino.x),vecino.y);
}

fvBaseColor.rgb *= I;

return fvBaseColor;
//return color_reflejado;
//return color_refractado;
//return float4(Fresnel,Fresnel,Fresnel,1);
}

technique RenderAgua
{
	pass p0
	{
		VertexShader = compile vs_3_0 VSAgua();
		PixelShader = compile ps_3_0 PSAgua();
	}
}

/**************************************************************************************/
/* RenderSceneShadows: Shadow map */
/**************************************************************************************/
//-----------------------------------------------------------------------------
// Vertex Shader que implementa un shadow map. Lo necesito para calcular
// el valor de la funcion de visibilidad de la ecuacion de radiosity
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
	//Color = (1-k)*500;
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
void VertSceneShadows(float4 iPos : POSITION,
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
float4 PixSceneShadows(float2 Tex : TEXCOORD0,
	float4 vPos : TEXCOORD1,
	float3 vNormal : TEXCOORD2,
	float4 vPosLight : TEXCOORD3
) :COLOR
{
	float3 vLight = normalize(float3(vPos - g_vLightPos));
	float cono = dot(vLight, g_vLightDir);
	float4 K = 0.0;
	if (cono > 0.1)
	{
		// coordenada de textura CT
		float2 CT = 0.5 * vPosLight.xy / vPosLight.w + float2(0.5, 0.5);
		CT.y = 1.0f - CT.y;

		float2 vecino = frac(CT*SMAP_SIZE);
		float prof = vPosLight.z / vPosLight.w;
		float s0 = (tex2D(g_samShadow, float2(CT)) + EPSILON < prof) ? 0.0f : 1.0f;
		float s1 = (tex2D(g_samShadow, float2(CT)+float2(1.0 / SMAP_SIZE,0))
							+ EPSILON < prof) ? 0.0f : 1.0f;
		float s2 = (tex2D(g_samShadow, float2(CT)+float2(0,1.0 / SMAP_SIZE))
							+ EPSILON < prof) ? 0.0f : 1.0f;
		float s3 = (tex2D(g_samShadow, float2(CT)+float2(1.0 / SMAP_SIZE,1.0 / SMAP_SIZE))
							+ EPSILON < prof) ? 0.0f : 1.0f;
		K = lerp(lerp(s0, s1, vecino.x),lerp(s2, s3, vecino.x),vecino.y);
	}

	float4 color_base = tex2D(diffuseMap, Tex);
	color_base.rgb *= 0.7 + 0.3*K;
	return color_base;
}

technique RenderSceneShadows
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertSceneShadows();
		PixelShader = compile ps_3_0 PixSceneShadows();
	}
}