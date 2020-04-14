float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

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

//Textura para Albedo
texture albedoTexture;
sampler2D albedoSampler = sampler_state
{
	Texture = (albedoTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Normals
texture normalTexture;
sampler2D normalSampler = sampler_state
{
	Texture = (normalTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Metallic
texture metallicTexture;
sampler2D metallicSampler = sampler_state
{
	Texture = (metallicTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Roughness
texture roughnessTexture;
sampler2D roughnessSampler = sampler_state
{
	Texture = (roughnessTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Ambient Occlusion
texture aoTexture;
sampler2D aoSampler = sampler_state
{
	Texture = (aoTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

// enviroment map
texture cubeMap;
samplerCUBE texCubeMap =
sampler_state
{
	Texture = <cubeMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture irradianceMap;
samplerCUBE texIrradianceMap =
sampler_state
{
	Texture = <irradianceMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture prefilterMap;
samplerCUBE texPrefilterMap =
sampler_state
{
	Texture = <prefilterMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture brdfLut;
sampler2D texbrdfLUT = sampler_state
{
	Texture = (brdfLut);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


//Input del Vertex Shader
struct Light
{
	float3 Position;
	float3 Color;
};

Light lights[4];

float3 eyePosition, albedoValue;

const float PI = 3.14159265359;

float time = 0.0, metallicValue = 0.0, roughnessValue = 0.0;

float passRoughness = 0.0;

bool textured = false;

//Input del Vertex Shader
struct PBRVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct PBRVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

//Input del Vertex Shader
struct CubeVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct CubeVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	float3 MeshPosition : TEXCOORD3;
};


//Input del Vertex Shader
struct FullQuadVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct FullQuadVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Vertex Shader
PBRVertexShaderOutput VSPBR(PBRVertexShaderInput input)
{
	PBRVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = input.Normal;

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	return output;
}
// ----------------------------------------------------------------------------
float3 fresnelSchlick(float cosTheta, float3 F0)
{
	return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}
// ----------------------------------------------------------------------------
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
	float antiRoughness = 1.0 - roughness;
	return F0 + (max(float3(antiRoughness, antiRoughness, antiRoughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}
// ----------------------------------------------------------------------------


float4 PSScene(PBRVertexShaderOutput input) : COLOR0
{
	float3 color = tex2D(diffuseMap, input.TextureCoordinates).rgb;
	float3 normal = normalize(input.WorldNormal);
	// ambient
	float3 ambient = 0.0 * color;
	// lighting
	float3 lighting = float3(0, 0, 0);
	for (int i = 0; i < 4; i++)
	{
		// diffuse
		float3 lightDir = normalize(lights[i].Position - input.WorldPosition);
		float diff = max(dot(lightDir, normal), 0.0);
		float3 diffuse = lights[i].Color * diff * color;
		float3 result = diffuse;
		// attenuation (use quadratic as we have gamma correction)
		float distance = length(input.WorldPosition - lights[i].Position);
		result *= 1.0 / (distance);
		lighting += result;
	}
	return float4(ambient + lighting, 1.0);
}



//Vertex Shader
CubeVertexShaderOutput VSCube(CubeVertexShaderInput input)
{
	CubeVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = /*mul(*/input.Normal/*, matInverseTransposeWorld).xyz*/;

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	// Propagamos la posicion
	output.MeshPosition = input.Position;

	return output;
}

float4 PSCubeIrradiance(CubeVertexShaderOutput input) : COLOR0
{
	float3 N = normalize(input.WorldPosition);
	float3 irradiance = float3(0, 0, 0);

	// tangent space calculation from origin point
	float3 up = float3(0.0, 1.0, 0.0);
	float3 right = cross(up, N);
	up = cross(N, right);

	float sampleDelta = 0.05;
	float nrSamples = 0.0f;

	float phi = 0.0f;
	float theta = 0.0f;
	while (phi < (2.0 * 3.14))
	{
		while (theta < (0.5 * 3.14))
		{
			// spherical to cartesian (in tangent space)
			float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
			// tangent space to world
			float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

			irradiance += texCUBE(texCubeMap, sampleVec).rgb * cos(theta) * sin(theta);
			nrSamples++;
			theta += sampleDelta;
		}
		phi += sampleDelta;
	}

	irradiance = PI * irradiance * (1.0 / float(nrSamples));
	return float4(irradiance, 1.0);

	//FragColor = float4(irradiance, 1.0);

	// Tomamos la posicion en mesh space del cubo unitario como coordenadas de textura
	// Esta posicion va perfectamente con las coordenadas de textura necesarias para el cubemap
	// Usamos texCUBE sin lod para tener la maxima resolucion de mipmap
	//float3 cubeMapSampled = texCUBE(texCubeMap, input.MeshPosition).rgb;
	//return float4(cubeMapSampled, 1.0);
}

float DistributionGGX(float3 N, float3 H, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.0);
	float NdotH2 = NdotH * NdotH;

	float nom = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;

	return nom / denom;
}

float VanDerCorpus(int n, int base)
{
	float invBase = 1.0 / float(base);
	float denom = 1.0;
	float result = 0.0;

	for (int i = 0; i < 32; ++i)
	{
		if (n > 0)
		{
			denom = float(n) % 2.0;
			result += denom * invBase;
			invBase = invBase / 2.0;
			n = int(float(n) / 2.0);
		}
	}

	return result;
}
// ----------------------------------------------------------------------------
float2 Hammersley(int i, int N)
{
	return float2(float(i) / float(N), VanDerCorpus(i, 2u));
}

// ----------------------------------------------------------------------------
float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
{
	float a = roughness * roughness;

	float phi = 2.0 * PI * Xi.x;
	float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
	float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

	// from spherical coordinates to cartesian coordinates - halfway vector
	float3 H;
	H.x = cos(phi) * sinTheta;
	H.y = sin(phi) * sinTheta;
	H.z = cosTheta;

	// from tangent-space H vector to world-space sample vector
	float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
	float3 tangent = normalize(cross(up, N));
	float3 bitangent = cross(N, tangent);

	float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
	return normalize(sampleVec);
}

float4 PSCubePrefilter(CubeVertexShaderOutput input) : COLOR0
{
	float3 N = normalize(input.WorldPosition);

	// make the simplyfying assumption that V equals R equals the normal 
	float3 R = N;
	float3 V = R;

	const int SAMPLE_COUNT = 128;
	float3 prefilteredColor = float3(0, 0, 0);
	float totalWeight = 0.0;

	for (int i = 0; i < SAMPLE_COUNT; ++i)
	{
		// generates a sample vector that's biased towards the preferred alignment direction (importance sampling).
		float2 Xi = Hammersley(i, SAMPLE_COUNT);
		float3 H = ImportanceSampleGGX(Xi, N, passRoughness);
		float3 L = normalize(2.0 * dot(V, H) * H - V);

		float NdotL = max(dot(N, L), 0.0);
		if (NdotL > 0.0)
		{
			// sample from the environment's mip level based on roughness/pdf
			float D = DistributionGGX(N, H, passRoughness);
			float NdotH = max(dot(N, H), 0.0);
			float HdotV = max(dot(H, V), 0.0);
			float pdf = D * NdotH / (4.0 * HdotV) + 0.0001;

			float resolution = 512.0; // resolution of source cubemap (per face)
			float saTexel = 4.0 * PI / (6.0 * resolution * resolution);
			float saSample = 1.0 / (float(SAMPLE_COUNT) * pdf + 0.0001);

			float mipLevel = passRoughness == 0.0 ? 0.0 : 0.5 * log2(saSample / saTexel);

			prefilteredColor += texCUBElod(texCubeMap, float4(L, mipLevel)).rgb * NdotL;
			totalWeight += NdotL;
		}
	}

	prefilteredColor = prefilteredColor / totalWeight;

	return float4(prefilteredColor, 1.0);
}

// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
	// note that we use a different k for IBL
	float a = roughness;
	float k = (a * a) / 2.0;

	float nom = NdotV;
	float denom = NdotV * (1.0 - k) + k;

	return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float ggx2 = GeometrySchlickGGX(NdotV, roughness);
	float ggx1 = GeometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

float2 IntegrateBRDF(float NdotV, float roughness)
{
	float3 V;
	V.x = sqrt(1.0 - NdotV * NdotV);
	V.y = 0.0;
	V.z = NdotV;

	float A = 0.0;
	float B = 0.0;

	float3 N = float3(0.0, 0.0, 1.0);

	const int SAMPLE_COUNT = 128;
	for (int i = 0; i < SAMPLE_COUNT; ++i)
	{
		// generates a sample vector that's biased towards the
		// preferred alignment direction (importance sampling).
		float2 Xi = Hammersley(i, SAMPLE_COUNT);
		float3 H = ImportanceSampleGGX(Xi, N, roughness);
		float3 L = normalize(2.0 * dot(V, H) * H - V);

		float NdotL = max(L.z, 0.0);
		float NdotH = max(H.z, 0.0);
		float VdotH = max(dot(V, H), 0.0);

		if (NdotL > 0.0)
		{
			float G = GeometrySmith(N, V, L, roughness);
			float G_Vis = (G * VdotH) / (NdotH * NdotV);
			float Fc = pow(1.0 - VdotH, 5.0);

			A += (1.0 - Fc) * G_Vis;
			B += Fc * G_Vis;
		}
	}
	A /= float(SAMPLE_COUNT);
	B /= float(SAMPLE_COUNT);
	return float2(A, B);
}

FullQuadVertexShaderOutput VSFullQuad(FullQuadVertexShaderInput input)
{
	FullQuadVertexShaderOutput output;
	output.Position = input.Position;
	output.TextureCoordinates = input.TextureCoordinates;
	return output;
}

float4 PSBDRFLUT(FullQuadVertexShaderOutput input) : COLOR0
{
	float2 integratedBRDF = IntegrateBRDF(input.TextureCoordinates.x, input.TextureCoordinates.y);
	return float4(integratedBRDF, 0.0, 1.0);
}

float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
	float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

	float3 Q1 = ddx(worldPosition);
	float3 Q2 = ddy(worldPosition);
	float2 st1 = ddx(textureCoordinates);
	float2 st2 = ddy(textureCoordinates);

	worldNormal = normalize(worldNormal);
	float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
	float3 B = -normalize(cross(worldNormal, T));
	float3x3 TBN = float3x3(T, B, worldNormal);

	return normalize(mul(tangentNormal, TBN));
}

//Pixel Shader
float4 PSPBR(PBRVertexShaderOutput input) : COLOR0
{

	float3 albedo = float3(1, 0, 0);
	float metallic = 0;
	float roughness = 0;
	float ao = 1.0;
	float3 N = input.WorldNormal;
	
	if (textured)
	{
		albedo = pow(tex2D(albedoSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
		metallic = tex2D(metallicSampler, input.TextureCoordinates).r;
		roughness = tex2D(roughnessSampler, input.TextureCoordinates).r;
		ao = 1.0;

		N = getNormalFromMap(input.TextureCoordinates, input.WorldPosition, N);
	}
	else
	{
		albedo = albedoValue;
		metallic = metallicValue;
		roughness = roughnessValue;
		N = normalize(N);
	}


	float3 V = normalize(eyePosition - input.WorldPosition);
	float3 R = reflect(-V, N);
	float3 positionView = normalize(input.WorldPosition - eyePosition);
	float3 reflection = float3(reflect(positionView, normalize(input.WorldNormal)));
	// calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
	// of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
	float3 F0 = float3(0.04, 0.04, 0.04);
	F0 = lerp(F0, albedo, metallic);

	// reflectance equation
	float3 Lo = float3(0, 0, 0);
	for (int i = 0; i < 4; ++i)
	{
		// calculate per-light radiance
		float3 L = normalize(lights[i].Position - input.WorldPosition);
		float3 H = normalize(V + L);
		float distance = length(lights[i].Position - input.WorldPosition);
		float attenuation = 1.0 / (distance);
		float3 radiance = lights[i].Color * attenuation;

		// Cook-Torrance BRDF
		float NDF = DistributionGGX(N, H, roughness);
		float G = GeometrySmith(N, V, L, roughness);
		float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

		float3 nominator = NDF * G * F;
		float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero.
		float3 specular = nominator / denominator;

		// kS is equal to Fresnel
		float3 kS = F;
		// for energy conservation, the diffuse and specular light can't
		// be above 1.0 (unless the surface emits light); to preserve this
		// relationship the diffuse component (kD) should equal 1.0 - kS.
		float3 kD = float3(1, 1, 1) - kS;
		// multiply kD by the inverse metalness such that only non-metals 
		// have diffuse lighting, or a linear blend if partly metal (pure metals
		// have no diffuse light).
		kD *= 1.0 - metallic;

		// scale light by NdotL
		float NdotL = max(dot(N, L), 0.0);

		// add to outgoing radiance Lo
		Lo += (kD * albedo / PI + specular) * radiance * NdotL; // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
	}

	// ambient lighting (we now use IBL as the ambient term)
	float3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);

	float3 kS = F;
	float3 kD = 1.0 - kS;
	kD *= 1.0 - metallic;

	float3 irradiance = texCUBE(texIrradianceMap, N).rgb;
	float3 diffuse = irradiance * albedo;

	// sample both the pre-filter map and the BRDF lut and combine them together as per the Split-Sum approximation to get the IBL specular part.
	const float MAX_REFLECTION_LOD = 5.0;
	float3 prefilteredColor = texCUBElod(texPrefilterMap, float4(R, roughness * MAX_REFLECTION_LOD)).rgb;
	float2 brdf = tex2D(texbrdfLUT, float2(max(dot(input.WorldNormal, V), 0.0), roughness)).rg;
	float3 specular = prefilteredColor * (F * brdf.x + brdf.y);

	float3 ambient = (kD * diffuse + specular) * ao;

	float3 color = ambient + Lo;

	// HDR tonemapping
	color = color / (color + float3(1, 1, 1));
	// gamma correct
	color = pow(color, float3(1.0 / 2.2, 1.0 / 2.2, 1.0 / 2.2));

	return float4(color, 1.0);
}


technique BDRFLUT
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VSFullQuad();
		PixelShader = compile ps_3_0 PSBDRFLUT();
	}
}


technique Scene
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VSPBR();
		PixelShader = compile ps_3_0 PSScene();
	}
}

technique Irradiance
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VSCube();
		PixelShader = compile ps_3_0 PSCubeIrradiance();
	}
}

technique Prefilter
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VSCube();
		PixelShader = compile ps_3_0 PSCubePrefilter();
	}
}

technique PBRIBL
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VSPBR();
		PixelShader = compile ps_3_0 PSPBR();
	}
}
