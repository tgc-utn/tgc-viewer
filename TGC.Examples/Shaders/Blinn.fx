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

//Parametros de la Luz
float3 ambientColor; //Color RGB para Ambient de la luz
float3 diffuseColor; //Color RGB para Diffuse de la luz
float3 specularColor; //Color RGB para Specular de la luz
float KAmbient; // Coeficiente de Ambient
float KDiffuse; // Coeficiente de Diffuse
float KSpecular; // Coeficiente de Specular
float shininess; //Exponente de specular
float3 lightPosition; //Posicion de la luz
float3 eyePosition; //Posicion de la camara

//Input del Vertex Shader
struct BlinnVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct BlinnVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};



//Vertex Shader
BlinnVertexShaderOutput VSBlinn(BlinnVertexShaderInput input)
{
	BlinnVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	return output;
}

//Pixel Shader
float4 PSBlinn(BlinnVertexShaderOutput input) : COLOR0
{
	input.WorldNormal = normalize(input.WorldNormal);

	float3 lightDirection = normalize(lightPosition - input.WorldPosition);
	float3 viewDirection = normalize(eyePosition - input.WorldPosition);
	float3 halfVector = normalize(lightDirection + viewDirection);

	// Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);

	//Componente Diffuse: N dot L
	float3 NdotL = dot(input.WorldNormal, lightDirection);
	float3 diffuseLight = KDiffuse * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (N dot H)^shininess
	float3 NdotH = dot(input.WorldNormal, halfVector);
	float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * specularColor * pow(max(0.0, NdotH), shininess);

	float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor + specularLight, texelColor.a);
	return finalColor;
}


//Input del Vertex Shader
struct PhongVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct PhongVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

//Vertex Shader
PhongVertexShaderOutput VSPhong(PhongVertexShaderInput input)
{
	PhongVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	return output;
}

//Pixel Shader
float4 PSPhong(PhongVertexShaderOutput input) : COLOR0
{
	input.WorldNormal = normalize(input.WorldNormal);

	float3 lightDirection = normalize(lightPosition - input.WorldPosition);
	float3 viewDirection = normalize(eyePosition - input.WorldPosition);

	// Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);

	//Componente Diffuse: N dot L
	float3 NdotL = dot(input.WorldNormal, lightDirection);
	float3 diffuseLight = KDiffuse * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (R dot V)^shininess
	float3 reflection = reflect(-lightDirection, input.WorldNormal);
	float3 specularLight = pow(max(0.0, dot(viewDirection, reflection)), shininess) * KSpecular * specularColor;


	float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor + specularLight, texelColor.a);
	return finalColor;
}






//Input del Vertex Shader
struct WallVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct WallVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Vertex Shader
WallVertexShaderOutput VSWall(WallVertexShaderInput input)
{
	WallVertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

float3 AdjustContrast(float3 color, float contrast)
{
	return saturate(lerp(float3(0.5, 0.5, 0.5), color, contrast));
}

//Pixel Shader
float4 PSWall(WallVertexShaderOutput input) : COLOR0
{
	float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);
	return float4(AdjustContrast(texelColor.rgb * 0.4, 2.8), texelColor.a);
}



technique Phong
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSPhong();
		PixelShader = compile ps_3_0 PSPhong();
	}
}

technique Blinn
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSBlinn();
        PixelShader = compile ps_3_0 PSBlinn();
    }
}

technique Wall
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSWall();
		PixelShader = compile ps_3_0 PSWall();
	}
}