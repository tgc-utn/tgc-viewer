
float time = 0;

float3 g_LightDir = float3(0,-1,0);
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float g_fSpecularExponent = 3;
float k_ld = 0.5;		// luz difusa
float k_le = 0.6;		// luz specular
float k_la = 0.4;		// luz ambiente
bool phong_lighting = true;

float min_cant_samples = 10;
float max_cant_samples = 50;

// Transformaciones
float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matInverseTransposeWorld;	

float fHeightMapScale = 0.1;
float fTexScale = 10;

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


// Textura basica:
texture base_Tex;
sampler2D baseMap =
sampler_state
{
   Texture = (base_Tex);
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture aux_Tex;
sampler2D auxMap =
sampler_state
{
   Texture = <aux_Tex>;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

// Heighmap
texture height_map;
sampler2D heightMap =
sampler_state
{
   Texture = <height_map>;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};



// calcula la iluminaciond dinamica
float4 Phong( float2 texCoord, float3 vLightTS, float3 vViewTS,float dx,float dy)
{
	// Color Basico
	float4 cBaseColor = tex2Dgrad( auxMap, texCoord,dx,dy );
	if(phong_lighting)
	{
		// Busco el vector normal en la textura Normal-Height Map  (esta en Tangent Space)
		float3 vNormalTS = normalize( tex2Dgrad( heightMap, texCoord ,dx,dy) * 2 - 1 );
	   
	   
		// Color difuso
		float3 vLightTSAdj = float3( vLightTS.x, -vLightTS.y, vLightTS.z );
		float cDiffuse = saturate( dot( vNormalTS, vLightTSAdj)) * k_ld;
	   
		// Color specular
		float4 cSpecular = 0;
		float3 vReflectionTS = normalize( 2 * dot( vViewTS, vNormalTS ) * vNormalTS - vViewTS );
		float fRdotL = saturate( dot( vReflectionTS, vLightTSAdj ));
		cSpecular = saturate( pow( fRdotL, g_fSpecularExponent))*k_le;

		// Retorno color difuso	+ luz especular
		cBaseColor = (k_la+cDiffuse)*cBaseColor + cSpecular;  
	}
	return cBaseColor;
}   


void RenderSceneVS( float4 Pos : POSITION,
             float2 Texcoord : TEXCOORD0,
             float3 normal : NORMAL,
             out float4 oPos : POSITION,
             out float2 Tex : TEXCOORD0,
             out float3 tsView : TEXCOORD1,
			 out float3 tsLight : TEXCOORD3,
             out float3 wsNormal : TEXCOORD2,
             out float3 wsView : TEXCOORD4
			)
{

	// Vector View = desde el ojo a la pos del vertice
	float4 VertexPositionWS = mul( Pos,matWorld );
	wsView = fvEyePosition.xyz - VertexPositionWS.xyz;

	
	// calculo la tg y la binormal ?
	float3 up = float3(0,0,1);
	if(abs(normal.z-1)<=0.0001)
		up = float3(0,1,0);
	float3 tangent = cross(normal,up);
	float3 binormal = cross(normal,tangent);

	/*
	// o la dejo fija? 
	normal = float3(0,1,0);
	float3 tangent = float3(1,0,0);
	float3 binormal = float3(0,0,1);
	*/
	
	float3x3 tangentToWorldSpace;
	tangentToWorldSpace[0] = mul( tangent, matWorld );
	tangentToWorldSpace[1] = mul( binormal, matWorld);
	tangentToWorldSpace[2] = mul( normal, matWorld);

	// tangentToWorldSpace es una matriz ortogonal, su inversa = a su transpuesta
	// A es OrtoNorm <=> A-1 == At
	float3x3 worldToTangentSpace = transpose(tangentToWorldSpace);
	
	// proyecto
    oPos = mul( Pos, matWorldViewProj );
    //Propago la textura
    Tex = Texcoord*fTexScale;

	tsView = mul( wsView, worldToTangentSpace );		// Vector View en TangentSpace
    tsLight = mul( g_LightDir, worldToTangentSpace);	// Vector Light en TangentSpace
    
    // propago el vector normal en Worldspace
	wsNormal = normal;
	// tambien devuelve el vector view en worldspace wsView
	
 }

 
// ws = worldspace    ts = tangentspace
float4 PSParallaxOcclusion(	float2 Texcoord: TEXCOORD0, 
				float3 Pos: POSITION,
				float3 tsView: TEXCOORD1,
				float3 wsNormal: TEXCOORD2,
				float3 tsLight: TEXCOORD3,
				float3 wsView: TEXCOORD4
			) : COLOR0
{   
	// normalizo todo lo que interpola el PS
	wsView = normalize(wsView);
	tsView = normalize(tsView);
	tsLight = normalize(tsLight);
	wsNormal = normalize(wsNormal);
	
	// POM Algoritmo Standard 
	float fParallaxLimit = length(tsView.xy) / tsView.z;
	fParallaxLimit *= fHeightMapScale;
	float2 vOffset = normalize( -tsView.xy);
	vOffset = vOffset * fParallaxLimit;
	// interpola entre un min y un max, proporcionalmente al angulo de vision
	int nNumSamples = (int)lerp( min_cant_samples ,max_cant_samples,
		 abs(dot(wsView,wsNormal)));
	float fStepSize = 1.0 / (float)nNumSamples;
	
	float2 dx, dy;
	dx = ddx( Texcoord );
	dy = ddy( Texcoord );
	
	// Ray casting: 
	float2 vOffsetStep = fStepSize * vOffset;
	float2 vCurrOffset = float2( 0, 0 );
	float2 vLastOffset = float2( 0, 0 );

	float fCurrH = 0;
	float fLastH = 0;

	float stepHeight = 1.0;
	int nCurrSample = 0;
	float4 vCurrSample;
	float4 vLastSample;
	
	while ( nCurrSample < nNumSamples )
	{
		vCurrSample = tex2Dgrad( heightMap, Texcoord + vCurrOffset, dx, dy );
		fCurrH = vCurrSample.a;		
		if ( fCurrH > stepHeight)
		{
			float Ua = 0;
			float X = ( fStepSize + (fCurrH - fLastH));
			if(X!=0.0f)
				Ua = ((stepHeight+fStepSize)- fLastH) / X;
			vCurrOffset = vLastOffset + Ua* vOffsetStep;
			nCurrSample = nNumSamples + 1;
	   }
	   else
	   {
		  nCurrSample++;
		  stepHeight -= fStepSize;
		  vLastOffset = vCurrOffset;
		  vCurrOffset += vOffsetStep;
		  vLastSample = vCurrSample;
		  fLastH = fCurrH;
	   }
	}

	return Phong( Texcoord + vCurrOffset, tsLight, -tsView,dx,dy);
}

// Parallax oclussion
technique ParallaxOcclusion
{
    pass p0
    {
        VertexShader = compile vs_3_0 RenderSceneVS();
        PixelShader = compile ps_3_0 PSParallaxOcclusion();
    }
}

float4 PSBumpMap(float2 Texcoord: TEXCOORD0, 
				float3 Pos: POSITION,
				float3 tsView: TEXCOORD1,
				float3 wsNormal: TEXCOORD2,
				float3 tsLight: TEXCOORD3
			) : COLOR0
{
	return Phong( Texcoord, tsLight, -tsView,ddx( Texcoord),ddy( Texcoord ));
}


technique BumpMap
{
    pass p0
    {
        VertexShader = compile vs_3_0 RenderSceneVS();
        PixelShader = compile ps_3_0 PSBumpMap();
    }
}

