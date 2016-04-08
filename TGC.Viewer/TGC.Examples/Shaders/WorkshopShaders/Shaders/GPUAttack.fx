#define MAX_DS 512

texture g_RenderTarget;              // Render target

int a = 33;
int c = 213; 
int m = 251;

int4 hash_buscado;

sampler RenderTarget = 
sampler_state
{
    Texture = <g_RenderTarget>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};




void Hash(int4 clave,out int4 buffer)
{
	int k = (clave[0] + clave[1] + clave[2] + clave[3]) % 256;
	for(int i=0;i<4;++i)
	{
		k = round(round(a*k + c) % m);
		buffer[i] = k;
		k  = round(k + clave[i]);
	}
}


float4 PSComputeHash( float2 pos : VPOS) : COLOR0
{
	int4 hash;
	int4 clave;
	int group_x = floor((float)pos.x / 32.0);
	int x = floor((float)pos.x % 32.0);
	int group_y = floor((float)pos.y / 32.0);
	int y = floor((float)pos.y % 32.0);

	clave[0] = 'A' + group_x;
	clave[1] = 'A' + group_y;
	clave[2] = 'A' + x;
	clave[3] = 'A' + y;
	Hash(clave,hash);

	// compara con el hash buscado
	if(distance(hash,hash_buscado)==0)
		return float4(1,1,1,1);
	else
		return float4(0,0,0,1);

	// retorna el hash pp dicho
	//return float4((float)hash[0]/255.0,(float)hash[1]/255.0,(float)hash[2]/255.0,(float)hash[3]/255.0);
}

void vs_copy( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}



technique ComputeHash
{
    pass P0
    {          
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader  = compile ps_3_0 PSComputeHash(); 
    }
}



void Hash6(int clave[6],out int buffer[6])
{
	int k = round(clave[0] + clave[1] + clave[2] + clave[3]+ clave[4]+ clave[5]) % 256;
	for(int i=0;i<6;++i)
	{
		k = round(round(a*k + c) % m);
		buffer[i] = k;
		k  = round(k + clave[i]);
	}
}

int prefix_x = 0;
int prefix_y = 0;
int hash_buscado6[6];

float4 PSComputeHash6( float2 pos : VPOS) : COLOR0
{
	int hash[6];
	int clave[6];
	int group_x = floor((float)pos.x / 32.0);
	int x = floor((float)pos.x % 32.0);
	int group_y = floor((float)pos.y / 32.0);
	int y = floor((float)pos.y % 32.0);

	clave[0] = 'A' + prefix_x;
	clave[1] = 'A' + prefix_y;
	clave[2] = 'A' + group_x;
	clave[3] = 'A' + group_y;
	clave[4] = 'A' + x;
	clave[5] = 'A' + y;
	Hash6(clave,hash);

	
	// compara con el hash buscado
	float dist = 0;
	for(int i=0;i<6;++i)
		dist += abs(hash[i] - hash_buscado6[i]);

	if(dist<0.01)
		return float4(1,1,1,1);
	else
		return float4(0,0,0,1);

/*	clave[0] = 'A';
	clave[1] = 'A';
	clave[2] = 'W';
	clave[3] = 'W';
	clave[4] = 'W';
	clave[5] = 'W';
	Hash6(clave,hash);
	return float4((float)hash[0]/255.0,(float)hash[1]/255.0,(float)hash[2]/255.0,(float)hash[3]/255.0);
	*/

}

technique ComputeHash6
{
    pass P0
    {          
		VertexShader = compile vs_3_0 vs_copy();
        PixelShader  = compile ps_3_0 PSComputeHash6(); 
    }
}

