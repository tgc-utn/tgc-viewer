// ---------------------------------------------------------
// Ejemplo de shaders basicos:
// ---------------------------------------------------------

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

// Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matViewProj; // Matriz View * Proj
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float time = 0;
float factor;
float4 eyePosition;
float4 effectVector;
float4 center;

// Textura para DiffuseMap
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

// Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

// Output del Vertex Shader (e Input del Pixel Shader)
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 Texcoord : TEXCOORD0;
    float4 Color : COLOR0;
};

// Helper para encontrar que tan paralelos son dos vectores
float dotNormalized(float4 vectorOne, float4 vectorTwo)
{
    return dot(normalize(vectorOne), normalize(vectorTwo));
}

float dotNormalized(float3 vectorOne, float3 vectorTwo)
{
    return dot(normalize(vectorOne), normalize(vectorTwo));
}

VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

	// Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);

    // Proyectamos la normal
    Output.Normal = Input.Normal;

	// Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	// Propago el color x vertice
    Output.Color = Input.Color;

    return Output;
}

VS_OUTPUT vs_expansion(VS_INPUT Input)
{
    VS_OUTPUT Output;

	// Proyectar posicion
    Output.Position = mul(Input.Position, matWorld);
    Output.Position = lerp(Output.Position, center, sin(time) - 1);
    Output.Position = mul(Output.Position, matViewProj);

    // Proyectamos la normal
    Output.Normal = Input.Normal;

	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	//Propago el color x vertice
    Output.Color = Input.Color;

    return Output;
}

VS_OUTPUT vs_extrude(VS_INPUT Input)
{
    VS_OUTPUT Output;

    float pos = 1 + clamp(abs(dotNormalized(Input.Normal, effectVector)), factor, 10);
	
    Input.Position.x *= pos;
    Input.Position.y *= pos;
    Input.Position.z *= pos;

    Output.Position = mul(Input.Position, matWorldViewProj);;
	
    Output.Normal = Input.Normal;

    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return Output;
}

VS_OUTPUT vs_identity_plane_extrude(VS_INPUT Input)
{
    VS_OUTPUT Output;

    float timeFactor = time * 10 * factor;
    float3 planeNormal = float3(cos(timeFactor), 1, sin(timeFactor));

    float parallel = dot(Input.Position, planeNormal);
    float extrude = 1 + (parallel > -0.1 && parallel < 0.1) * 0.1;

    Input.Position.x *= extrude;
    Input.Position.y *= extrude;
    Input.Position.z *= extrude;

    Output.Position = mul(Input.Position, matWorldViewProj);;

    Output.Normal = Input.Normal;

    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return Output;
}

VS_OUTPUT vs_planar_extrude(VS_INPUT Input)
{
    VS_OUTPUT Output;
	
    float parallel = dotNormalized(Input.Position, effectVector);

    float extrude = 1 + (parallel > -0.1 && parallel < 0.1) * 0.2;

    Input.Position.x *= extrude;
    Input.Position.y *= extrude;
    Input.Position.z *= extrude;

    Output.Position = mul(Input.Position, matWorldViewProj);;

    Output.Normal = Input.Normal;

    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return Output;
}

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    return tex2D(diffuseMap, Input.Texcoord);
}

float4 ps_texture_cycling(VS_OUTPUT Input) : COLOR0
{
    Input.Texcoord.x += sin(time * 2) * 0.1;
    Input.Texcoord.y += cos(time * 2) * 0.1;
    return ps_main(Input);
}

float4 ps_color_cycling(VS_OUTPUT Input) : COLOR0
{
    float3 rotated = float3(cos(time), 1, sin(time));
    float3 normal = float3(sin(time), 1, cos(time));
    float3 other = float3(tan(time), 1, -tan(time));

    float r = abs(dotNormalized(rotated, Input.Normal));
    float g = abs(dotNormalized(normal, Input.Normal));
    float b = abs(dotNormalized(other, Input.Normal));

    return float4(r, g, b, 1);
}

struct VS_LIGHT_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float4 InterpolatedPosition : TEXCOORD1;
};

VS_LIGHT_OUTPUT vs_lightstruck(VS_INPUT Input)
{
    VS_LIGHT_OUTPUT Output;

    Output.Position = mul(Input.Position, matWorldViewProj);
	
    Output.InterpolatedPosition = Input.Position;

    Output.Texcoord = Input.Texcoord;

    return Output;
}

float4 ps_inner_light(VS_LIGHT_OUTPUT Input) : COLOR0
{
    float4 textureColor = tex2D(diffuseMap, Input.Texcoord);
    float innerLight = clamp(dotNormalized(Input.InterpolatedPosition, effectVector), 0.1, 1);

    return textureColor * innerLight;
}

struct VS_EXTRUDED_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float InRange : TEXCOORD1;
};




VS_EXTRUDED_OUTPUT vs_identity_plane_extrude_with_position(VS_LIGHT_OUTPUT Input)
{
    VS_EXTRUDED_OUTPUT Output;

    float timeFactor = time * 10 * factor;
    float3 planeNormal = float3(cos(timeFactor), 1, sin(timeFactor));

    float parallel = dot(Input.Position, planeNormal);
    float extrude = (parallel > -0.1 && parallel < 0.1) * 0.1;

    Output.InRange = extrude;

    float4 pos = Input.Position * (1 + extrude);
    pos.w = 1;

    Output.Position = mul(pos, matWorldViewProj);;
	
    Output.Texcoord = Input.Texcoord;

    return Output;
}

float4 ps_extrude(VS_EXTRUDED_OUTPUT Input) : COLOR0
{
    return tex2D(diffuseMap, Input.Texcoord) + float4(0, 0, Input.InRange * 10 * (sin(time * 2) + 1.5), 1);
}

//Output del Vertex Shader
struct VS_PHONG_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};


VS_PHONG_OUTPUT vs_phong(VS_INPUT Input)
{
    VS_PHONG_OUTPUT Output;
	
    Output.Position = mul(Input.Position, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Output.WorldPosition = mul(Input.Position, matWorld).xyz;
	
    Output.WorldNormal = mul(Input.Normal, matWorld).xyz;

    return Output;
}

float4 ps_phong(VS_PHONG_OUTPUT Input) : COLOR0
{
    float4 textureFragment = tex2D(diffuseMap, Input.Texcoord);
	
    Input.WorldNormal = normalize(Input.WorldNormal);

    float ambientAmount = 0.2;
    float diffuseAmount = 0.7;
    float specularAmount = factor;
    float ambient = ambientAmount;

    float3 lightDirection = normalize(effectVector + center - Input.WorldPosition);
    float diffuse = saturate(dotNormalized(Input.WorldNormal, lightDirection)) * 0.7;

    float3 eyeDirection = normalize(Input.WorldPosition - eyePosition);
    float specular = saturate(dotNormalized(reflect(lightDirection, Input.WorldNormal), eyeDirection));
    float power = 16.84;
    specular = pow(specular, power);
    specular *= specularAmount;

    textureFragment.rgb = saturate(textureFragment * (saturate(ambient + diffuse)) + specular);
    return textureFragment;
}


technique RenderMesh
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}

technique Expansion
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_expansion();
        PixelShader = compile ps_3_0 ps_main();
    }
};

technique Extrude
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_extrude();
        PixelShader = compile ps_3_0 ps_main();
    }
};

technique PlanarExtrude
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_planar_extrude();
        PixelShader = compile ps_3_0 ps_main();
    }
};

technique IdentityPlaneExtrude
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_identity_plane_extrude();
        PixelShader = compile ps_3_0 ps_main();
    }
};

technique TextureCycling
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_texture_cycling();
    }
};

technique ColorCycling
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_color_cycling();
    }
};

technique InnerLight
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_lightstruck();
        PixelShader = compile ps_3_0 ps_inner_light();
    }
};

technique ExtrudeCombined
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_identity_plane_extrude_with_position();
        PixelShader = compile ps_3_0 ps_extrude();
    }
};

technique Phong
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_phong();
        PixelShader = compile ps_3_0 ps_phong();
    }
};