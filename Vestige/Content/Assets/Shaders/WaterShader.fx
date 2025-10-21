#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_3_0_level_9_1
	#define PS_SHADERMODEL ps_3_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D BackgroundTexture;
float Time;
matrix ModelMatrix;
matrix View;
matrix Projection;
float2 screen_size;

static const float speed = 2.0;
static const float waves = 1000.0;
static const float horizontal_waves = 1000.0;
static const float amplitude = 0.69;
static const float horizontal_amplitude = 0.91;
static const float wave_offset = -0.04;
//TODO: need to set this in UpdateRenderTargets
static const float wave_amplitude = 0.03;
static const float wave_period = 0.005;

sampler2D SpriteTextureSampler
{
	Texture = <SpriteTexture>;
};

sampler2D BackgroundTextureSampler
{
    Texture = <BackgroundTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float Remap01(float value, float from, float to)
{
    return (value - from) / (to - from);
}

float whiteNoise(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float2 screenUV = input.Position.xy / screen_size;
    float4 fgColor = tex2D(SpriteTextureSampler, uv);
    float2 WorldSpace = mul(float4(input.Position.xy, 0.0, 1.0), ModelMatrix).xy;
    float2 transform = WorldSpace / screen_size;
    
    //float noise = whiteNoise(WorldSpace + float2(Time / 60.0, 0));
    //fgColor = float4(noise, noise, noise, 1.0);
    //TODO: add noise to the shader
    if (fgColor.g != 1.0)
    {
        float waves = uv.y + sin(transform.x / wave_period - Time * speed) * cos(0.2 * transform.x / wave_period + Time * speed) * wave_amplitude - wave_amplitude;
        if (waves > 0.1)
        {
            fgColor = float4(0.0, 0.6902, 1.0, 1.0) * input.Color;
        }
        else if (waves > 0.0)
        {
            fgColor = float4(1.0, 1.0, 1.0, 1.0) * input.Color;
        }
        else
        {
            fgColor = float4(0.0, 0.0, 0.0, 0.0) * input.Color;
        }
    }
    else
    {
        fgColor = float4(0.0, 0.6902, 1.0, 1.0) * input.Color;
    }
    
    float value = (sin(2.0 * (transform.x * waves / 10.0 - (Time * speed))) * 0.5 + cos(3.0 * transform.x * waves / 10.0 - (Time * speed)) * 0.5) * amplitude;
    screenUV.y += Remap01(value, -waves, waves) - 0.5;
    screenUV.x += Remap01(sin(transform.y * horizontal_waves / 10.0 - (Time * speed)) * horizontal_amplitude, -horizontal_waves, horizontal_waves) - 0.5;
	
    screenUV = frac(screenUV);
	
    float4 bgColor = tex2D(BackgroundTextureSampler, screenUV);

    if (bgColor.a == 0.0)
        bgColor = fgColor;
    
    return lerp(bgColor, fgColor, 0.5);
}

VertexShaderOutput MainVS(float4 position : POSITION0, float4 color : COLOR0, float2 texcoord : TEXCOORD0)
{
    VertexShaderOutput output;
    float4 viewPosition = mul(position, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = color;
    output.TextureCoordinates = texcoord;
    return output;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
technique PrimitiveDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};