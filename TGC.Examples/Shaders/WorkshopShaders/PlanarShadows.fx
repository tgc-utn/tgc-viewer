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
float4x4 matViewProj; //View * Projection

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




float time = 0;

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3   g_vLightPos;  // posicion de la luz (en World Space) = pto que representa patch emisor Bj 
float3   g_vLightDir;  // Direcion de la luz (en World Space) = normal al patch Bj



//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 Norm :			TEXCOORD1;		// Normales
   float3 Pos :   			TEXCOORD2;		// Posicion real 3d
};



//-----------------------------------------------------------------------------
// Vertex Shader para dibujar la escena pp dicha con sombras
//-----------------------------------------------------------------------------
void VertScene( float4 iPos : POSITION,
                float2 iTex : TEXCOORD0,
                float3 iNormal : NORMAL,
				out float4 oPos : POSITION,                
                out float2 Tex : TEXCOORD0,
				out float4 vPos : TEXCOORD1,
                out float3 vNormal : TEXCOORD2
                )
{
    // transformo al screen space
    oPos = mul( iPos, matWorldViewProj );
        
	// propago coordenadas de textura 
    Tex = iTex;
    
	// propago la normal
    vNormal = mul( iNormal, (float3x3)matWorldView );
    
    // propago la posicion del vertice en World space
    vPos = mul( iPos, matWorld);
	
}


//-----------------------------------------------------------------------------
// Pixel Shader para dibujar la escena
//-----------------------------------------------------------------------------
float4 PixScene(	float2 Tex : TEXCOORD0,
					float4 vPos : TEXCOORD1,
					float3 vNormal : TEXCOORD2,
					float4 vPosLight : TEXCOORD3
					):COLOR
{
	float4 color_base = tex2D( diffuseMap, Tex);
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



//-----------------------------------------------------------------------------
void VertShadows( float4 iPos : POSITION , out float4 oPos : POSITION)
{
    float3 v = mul( iPos, matWorld);
	float k = v.y / (g_vLightPos.y-v.y);

	v = v + (v-g_vLightPos)*k;
	//v.y = 0;

    // transformo al screen space
    oPos = mul( float4(v , 1), matViewProj );

	
}


float4 PixShadows() :COLOR
{
	return float4(0,0,0,0.5);	
}	

technique RenderShadows
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertShadows();
        PixelShader = compile ps_3_0 PixShadows();
    }
}
