#version 330

in vec2 f_texcoord0;
in vec2 f_texcoord1;
in vec2 f_texcoord2;
in vec2 f_texcoord3;
in vec3 normal;
in vec4 color;
in vec3 tangent;
in vec3 bitangent;

uniform int uvChannel;
uniform vec3 difLightDirection;
uniform int renderType;
uniform int useNormalMap;
uniform vec4 colorSamplerUV;
uniform int renderVertColor;
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform int colorOverride;


//Texture Samplers
uniform sampler2D tex0;
uniform sampler2D BakeShadowMap;
uniform sampler2D spl;
uniform sampler2D nrm;
uniform sampler2D BakeLightMap;
uniform sampler2D UVTestPattern;
uniform sampler2D TransparencyMap;
uniform sampler2D EmissionMap;
uniform sampler2D DiffuseLayer;
uniform sampler2D MetalnessMap;
uniform sampler2D RoughnessMap;

uniform mat4 mvpMatrix;

//Shader Params via BFRES
uniform float normal_map_weight;
uniform float ao_density;
uniform float emission_intensity;


//Shader Options via BFRES
uniform float uking_texture2_texcoord;

// Check if src, dst, alpha function are non zero.
uniform int isTransparent;

// Channel Toggles
uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;

//Map toggles
uniform int HasNormalMap;
uniform int HasSpecularMap;
uniform int HasShadowMap;
uniform int HasLightMapMap;
uniform int HasTransparencyMap;
uniform int HasEmissionMap;
uniform int HasDiffuseLayer;
uniform int HasMetalnessMap;
uniform int HasRoughnessMap;

uniform int bake_shadow_type;

out vec4 FragColor;

#define gamma 2.2

vec2 displayTexCoord =  f_texcoord0;



void main()
{
    if (uvChannel == 2)
        displayTexCoord = f_texcoord1;
    if (uvChannel == 3)
        displayTexCoord = f_texcoord3;

    if (colorOverride == 1)
    {
        // Wireframe color.

		if (renderVertColor == 1)
		{
            FragColor = vec4(color);
		}
		else
		{
            FragColor = vec4(1);
		}
        return;
    }

	vec4 MetalnessTex = vec4(texture(MetalnessMap, displayTexCoord).rrr, 1);
	vec4 RoughnessTex = vec4(texture(RoughnessMap, displayTexCoord).rrr, 1);

    FragColor = vec4(0);

	if (HasMetalnessMap == 1)
        FragColor = vec4(MetalnessTex);

}