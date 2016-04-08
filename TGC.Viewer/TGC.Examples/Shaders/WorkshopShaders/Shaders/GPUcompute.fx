#define MAX_DS 512

//Variables utilizadas por el Vertex Shader
float3 fvLightPosition = float3( -100.00, 100.00, -100.00 );
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float4x4 matViewProjection;
float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;

float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matInverseTransposeWorld;

float k_la = 0.3;							// luz ambiente global
float k_ld = 0.9;							// luz difusa
float k_ls = 0.4;							// luz specular
float fSpecularPower = 16.84;

float elapsedTime = 0;
float map_size = 64.0;
float map_desf = 0.5/64.0;
float Kp = 1;

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = MIRROR;
	ADDRESSV = MIRROR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


// mapa de Velocidad
texture g_pVelocidad;
sampler Velocidad = 
sampler_state
{
    Texture = <g_pVelocidad>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

// mapa de Posiciones
texture g_pPos;
sampler Posicion = 
sampler_state
{
    Texture = <g_pPos>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};


// Height map
texture height_map;
sampler2D heightMap =
sampler_state
{
   Texture = <height_map>;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = NONE;
};

float currentScaleXZ = 100;
float currentScaleY = 10;

// ------------------------------------
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 Norm :          TEXCOORD1;			// Normales
   float3 Pos :   		TEXCOORD2;		// Posicion real 3d
};

//Vertex Shader Trivial:
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;
   //Proyectar posicion
   Output.Position         = mul( Input.Position, matWorldViewProj);
   //Las Texcoord quedan igual
   Output.Texcoord         = Input.Texcoord;
   // Calculo la posicion real
   float4 pos_real = mul(Input.Position,matWorld);
   Output.Pos = float3(pos_real.x,pos_real.y,pos_real.z);
   // Transformo la normal y la normalizo
	Output.Norm = normalize(mul(Input.Normal,matInverseTransposeWorld));
   return( Output );
}

//Pixel Shader
float4 ps_main( float3 Texcoord: TEXCOORD0, float3 N:TEXCOORD1,
	float3 Pos: TEXCOORD2) : COLOR0
{      
	float ld = 0;		// luz difusa
	float le = 0;		// luz specular

	N = normalize(N);
	// 1- calculo la luz diffusa
	float3 LD = normalize(fvLightPosition-float3(Pos.x,Pos.y,Pos.z));
	ld += saturate(dot(N, LD))*k_ld;
	
	// 2- calcula la reflexion specular
	float3 D = normalize(float3(Pos.x,Pos.y,Pos.z)-fvEyePosition);
	float ks = saturate(dot(reflect(LD,N), D));
	ks = pow(ks,fSpecularPower);
	le += ks*k_ls;

	//Obtener el texel de textura
	float4 fvBaseColor      = tex2D( diffuseMap, Texcoord );

	// suma luz diffusa, ambiente y especular
	float4 RGBColor = 0;
	RGBColor.rgb = saturate(fvBaseColor*(saturate(k_la+ld)) + le);
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

// -----------------------------------------------

struct PS_OUTPUT
{
    float4 Velocity : COLOR0;
    float4 Position : COLOR1;
};

float CalcularAltura(float x,float y)
{
	float u = y/(currentScaleXZ*map_size) + 0.5;
	float v = x/(currentScaleXZ*map_size) + 0.5;
	float3 T0 = tex2D(heightMap,float2(u+map_desf,v+map_desf));
	float H0 = T0.r*0.299 + T0.g*0.587 + T0.b*0.114;
	return 255*H0*currentScaleY;
}

PS_OUTPUT PSInitVel( float2 Texcoord: TEXCOORD0)
{
	PS_OUTPUT Output;
	Output.Velocity = float4(0,0,0,0);
	
	/*
	float x0 = 	0.2 + 0.6*(Texcoord.x-0.5)*currentScaleXZ*map_size;
	float z0 =  0.2 + 0.6*(Texcoord.y-0.5)*currentScaleXZ*map_size;
	Output.Position = float4(x0,z0,220+CalcularAltura(x0, z0),0);	
	*/
	
	float an = 	200*Texcoord.x*3.1415;
	float z0 =  0.1*(Texcoord.y-0.5)*currentScaleXZ*map_size;
	Output.Position = float4(sin(an)*20,cos(an)*20-1000,2000+z0*50,0);	
	
	return Output;
}

PS_OUTPUT PSComputeVel( float2 Texcoord: TEXCOORD0)
{
	PS_OUTPUT Output;

	float4 Vel = tex2D( Velocidad, Texcoord );
	float vel_x = Vel.x;
	float vel_z = Vel.y;
	
	float4 Pos = tex2D( Posicion, Texcoord );
    float x0 = Pos.x;
    float z0 = Pos.y;
    float H = CalcularAltura(x0, z0);
	float ddx,ddz,alfa;
	if(Pos.z>H+10)
	{
		H = Pos.z - elapsedTime*500;
		ddx = ddz = 0;
		alfa = 0;
	}
	else
	{
		float dt = 20;
		ddx = (CalcularAltura(x0 + dt, z0) - H) / dt;
		ddz = (CalcularAltura(x0, z0 + dt) - H) / dt;
		vel_x -= elapsedTime * ddx * 150;
		vel_z -= elapsedTime * ddz * 150;
	    
		// rozamiento
		vel_x *= 1-elapsedTime/25;
		vel_z *= 1-elapsedTime/25;

		x0 += elapsedTime * vel_x;
		z0 += elapsedTime * vel_z;
    
		// calculo el angulo rodado:
		float dx = x0 - Pos.x;
		float dz = z0 - Pos.y;
		alfa = sqrt(dx*dx + dz*dz) / Kp;
	}
	
	
	Output.Velocity = float4(vel_x,vel_z,ddx,ddz);
	Output.Position = float4(x0,z0,H,alfa);	

	return Output;
}

float4 PSPreview( float2 Texcoord: TEXCOORD0) : COLOR0
{
	float4 fvVelPos = tex2D( Velocidad, Texcoord );
	float2 vel = fvVelPos.xy;
	float2 pos = fvVelPos.zw;
	return float4(1,1,1,1);
}

void vs_copy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


// Genera el mapa de velocidad
technique ComputeVel
{
    pass P0
    {          
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader  = compile ps_3_0 PSInitVel(); 
    }
    pass P1
    {          
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader  = compile ps_3_0 PSComputeVel(); 
    }
    
    pass P2
    {          
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader  = compile ps_3_0 PSPreview(); 
    }
    
}



