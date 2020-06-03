float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float4x4 matQuad;

//Textura para Albedo
texture baseColorTexture;
sampler2D baseColorSampler = sampler_state
{
    Texture = (baseColorTexture);
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
texture positionTexture;
sampler2D positionSampler = sampler_state
{
    Texture = (positionTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texDiffuseMap;
sampler2D diffuseSampler = sampler_state
{
    Texture = (texDiffuseMap);
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

int lightCount = 1;
Light lights[200];

float time = 0;

float3 eyePosition; //Posicion de la camara

const float PI = 3.14159265359;

float KAmbient = 0.04;
float KSpecular = 0.3;
float KDiffuse = 0.7;
float shininess = 16.0;

//Input del Vertex Shader
struct DefaultVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct DefaultVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};



//Vertex Shader
DefaultVertexShaderOutput VSDefault(DefaultVertexShaderInput input)
{
    DefaultVertexShaderOutput output;

	// Proyectamos la position
    output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
    output.WorldNormal = /*mul(*/input.
Normal /*, matInverseTransposeWorld).xyz*/;

	// Usamos la matriz de world para proyectar la posicion
    output.WorldPosition = mul(input.Position, matWorld);

    return output;
}

float4 PSDefault(DefaultVertexShaderOutput input) : COLOR0
{
    input.WorldNormal = normalize(input.WorldNormal);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition);
    float4 texelColor = tex2D(diffuseSampler, input.TextureCoordinates);
    
    float3 accumulatedDiffuse = float3(0, 0, 0);
    float3 accumulatedSpecular = float3(0, 0, 0);
    
    for (int index = 0; index < 50; index++)
    {
        float3 lightVector = lights[index].Position - input.WorldPosition;
        float3 lightDirection = normalize(lightVector);
        float3 halfVector = normalize(lightDirection + viewDirection);
        float3 lightColor = lights[index].Color;
        
	    //Componente Diffuse: N dot L
        float3 NdotL = dot(input.WorldNormal, lightDirection);
        float3 diffuseLight = KDiffuse * lightColor * max(0.0, NdotL);

	    //Componente Specular: (N dot H)^shininess
        float3 NdotH = dot(input.WorldNormal, halfVector);
        float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * lightColor * pow(max(0.0, NdotH), shininess);
        
        float scaled = 20.0 / length(lightVector);
        
        accumulatedDiffuse += scaled * (lightColor * KAmbient + diffuseLight);
        accumulatedSpecular += scaled * specularLight;
    }
    return float4(accumulatedDiffuse, 1) * texelColor + float4(accumulatedSpecular, 1);
}



//Vertex Shader
DefaultVertexShaderOutput VSDeferred(DefaultVertexShaderInput input)
{
    DefaultVertexShaderOutput output;

	// Proyectamos la position
    output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
    output.WorldNormal = /*mul(*/input.Normal /*, matInverseTransposeWorld).xyz*/;

	// Usamos la matriz de world para proyectar la posicion
    output.WorldPosition = mul(input.Position, matWorld);

    return output;
}

struct DeferredPixelShaderOutput
{
    float4 position : COLOR0;
    float4 normals : COLOR1;
    float4 baseColor : COLOR2;
};

DeferredPixelShaderOutput PSDeferred(DefaultVertexShaderOutput input)
{
    DeferredPixelShaderOutput output;
    
    float4 texelColor = tex2D(diffuseSampler, input.TextureCoordinates);
    
    output.normals = float4(normalize(input.WorldNormal), 1);
    output.position = float4(input.WorldPosition, 1);
    output.baseColor = texelColor;
    
    return output;
}

//Input del Vertex Shader
struct FullScreenQuadVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Input del Vertex Shader
struct FullScreenQuadVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

FullScreenQuadVertexShaderOutput VSFullScreenQuad(FullScreenQuadVertexShaderInput input)
{
    FullScreenQuadVertexShaderOutput output;
    
    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
    
    return output;
}

float4 PSIntegrate(FullScreenQuadVertexShaderOutput input) : COLOR0
{
    float3 position = tex2D(positionSampler, input.TextureCoordinates);
    float3 normal = tex2D(normalSampler, input.TextureCoordinates);
    float4 baseColor = tex2D(baseColorSampler, input.TextureCoordinates);
    
    float3 viewDirection = normalize(eyePosition - position);
    
    float3 accumulatedDiffuse = float3(0, 0, 0);
    float3 accumulatedSpecular = float3(0, 0, 0);
    
    for (int index = 0; index < 50; index++)
    {
        float3 lightVector = lights[index].Position - position;
        float3 lightDirection = normalize(lightVector);
        float3 halfVector = normalize(lightDirection + viewDirection);
        float3 lightColor = lights[index].Color;
        
	    //Componente Diffuse: N dot L
        float3 NdotL = dot(normal, lightDirection);
        float3 diffuseLight = KDiffuse * lightColor * max(0.0, NdotL);

	    //Componente Specular: (N dot H)^shininess
        float3 NdotH = dot(normal, halfVector);
        float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * lightColor * pow(max(0.0, NdotH), shininess);
        
        float scaled = 20.0 / length(lightVector);
        
        accumulatedDiffuse += scaled * (lightColor * KAmbient + diffuseLight);
        accumulatedSpecular += scaled * specularLight;
    }
    return float4(accumulatedDiffuse, 1) * baseColor + float4(accumulatedSpecular, 1);
}

struct RenderTargetVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct RenderTargetVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

int target = 0;

/*
sampler2D samplerFromTarget()
{
    return (target == 0) ? positionSampler : ((target == 1) ? normalSampler : baseColorSampler);
}*/

RenderTargetVertexShaderOutput VSRenderTargets(RenderTargetVertexShaderInput input)
{
    RenderTargetVertexShaderOutput output;
    
    output.Position = mul(input.Position, matQuad);
    output.TextureCoordinates = input.TextureCoordinates;
    
    return output;
}

float4 PSRenderTargets(RenderTargetVertexShaderOutput input) : COLOR0
{
    float4 color = (target == 0) ? tex2D(positionSampler, input.TextureCoordinates) : ((target == 1) ? tex2D(normalSampler, input.TextureCoordinates) : tex2D(baseColorSampler, input.TextureCoordinates));
    return color;
}



technique RenderTargets
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VSRenderTargets();
        PixelShader = compile ps_3_0 PSRenderTargets();
    }
}

technique Default
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VSDefault();
        PixelShader = compile ps_3_0 PSDefault();
    }
}

technique Deferred
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VSDeferred();
        PixelShader = compile ps_3_0 PSDeferred();
    }
}

technique IntegrateDeferred
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VSFullScreenQuad();
        PixelShader = compile ps_3_0 PSIntegrate();
    }
}