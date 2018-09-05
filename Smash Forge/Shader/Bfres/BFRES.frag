#version 330

in vec2 f_texcoord0;
in vec2 f_texcoord1;
in vec2 f_texcoord2;
in vec2 f_texcoord3;

in vec3 objectPosition;

in vec3 normal;
in vec3 viewNormal;
in vec4 vertexColor;
in vec3 tangent;
in vec3 bitangent;

in vec3 boneWeightsColored;

// Viewport Camera/Lighting
uniform mat4 mvpMatrix;
uniform vec3 specLightDirection;
uniform vec3 difLightDirection;
uniform mat4 projMatrix;
uniform mat4 normalMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 rotationMatrix;

uniform int useImageBasedLighting;
uniform int enableCellShading;

uniform vec3 camPos;

uniform vec3 light1Pos;

const float levels = 3.0;

// Viewport Settings
uniform int uvChannel;
uniform int renderType;
uniform int useNormalMap;
uniform vec4 colorSamplerUV;
uniform int renderVertColor;
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform int colorOverride;
uniform float DefaultMetalness;
uniform float DefaultRoughness;

// Channel Toggles
uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;

// Texture Samplers
uniform sampler2D tex0;
uniform sampler2D BakeShadowMap;
uniform sampler2D normalMap;
uniform sampler2D BakeLightMap;
uniform sampler2D UVTestPattern;
uniform sampler2D TransparencyMap;
uniform sampler2D EmissionMap;
uniform sampler2D SpecularMap;
uniform sampler2D DiffuseLayer;
uniform sampler2D MetalnessMap;
uniform sampler2D RoughnessMap;
uniform sampler2D MRA;
uniform sampler2D TeamColorMap;
uniform sampler2D SphereMap;

uniform samplerCube irradianceMap;
uniform samplerCube specularIbl;
uniform sampler2D brdfLUT;

// Shader Params
uniform float normal_map_weight;
uniform float ao_density;
uniform float emission_intensity;
uniform vec4 fresnelParams;
uniform vec4 base_color_mul_color;
uniform vec3 emission_color;
uniform vec3 specular_color;

// Shader Options
uniform float uking_texture2_texcoord;
uniform float bake_shadow_type;
uniform float enable_fresnel;
uniform float enable_emission;
uniform float cSpecularType;


// Texture Map Toggles
uniform int HasDiffuse;
uniform int HasNormalMap;
uniform int HasSpecularMap;
uniform int HasShadowMap;
uniform int HasAmbientOcclusionMap;
uniform int HasLightMap;
uniform int HasTransparencyMap;
uniform int HasEmissionMap;
uniform int HasDiffuseLayer;
uniform int HasMetalnessMap;
uniform int HasRoughnessMap;
uniform int HasMRA;

uniform int roughnessAmount;

uniform int UseAOMap;
uniform int UseCavityMap;
uniform int UseMetalnessMap;
uniform int UseRoughnessMap;

int isTransparent;

struct VertexAttributes {
    vec3 objectPosition;
    vec2 texCoord;
    vec2 texCoord2;
    vec2 texCoord3;
    vec4 vertexColor;
    vec3 normal;
    vec3 viewNormal;
    vec3 tangent;
    vec3 bitangent;
	};

out vec4 fragColor;

#define gamma 2.2

// Defined in Utility.frag.
float Luminance(vec3 rgb);

// Defined in BFRES_Utility.frag.
vec3 CalcBumpedNormal(vec3 normal, sampler2D normalMap, VertexAttributes vert, float texCoordIndex);
float AmbientOcclusionBlend(sampler2D BakeShadowMap, VertexAttributes vert, float ao_density);
vec3 EmissionPass(sampler2D EmissionMap, float emission_intensity, VertexAttributes vert, float texCoordIndex, vec3 emission_color);
vec3 SpecularPass(vec3 I, vec3 normal, int HasSpecularMap, sampler2D SpecularMap, vec3 specular_color, VertexAttributes vert, float texCoordIndex);
vec3 ReflectionPass(vec3 N, vec3 I, vec4 diffuseMap, float aoBlend, vec3 tintColor, VertexAttributes vert);

void main()
{
    fragColor = vec4(vec3(0), 1);

    // Create a struct for passing all the vertex attributes to other functions.
    VertexAttributes vert;
    vert.objectPosition = objectPosition;
    vert.texCoord = f_texcoord0;
    vert.texCoord2 = f_texcoord1;
    vert.texCoord3 = f_texcoord2;
    vert.vertexColor = vertexColor;
    vert.normal = normal;
	vert.viewNormal = viewNormal;
    vert.tangent = tangent;
    vert.bitangent = bitangent;

    // Wireframe color.
    if (colorOverride == 1)
    {
		if (renderVertColor == 1)
            fragColor = vec4(vertexColor);
		else
            fragColor = vec4(1);

        return;
    }

    // Calculate shading vectors.
    vec3 I = vec3(0,0,-1) * mat3(mvpMatrix);
    vec3 N = normal;
	if (HasNormalMap == 1 && useNormalMap == 1)
		N = CalcBumpedNormal(normal, normalMap, vert, 0);

    // Light Map
    vec4 LightMapColor = texture(BakeLightMap, f_texcoord2);

    // Shadow Map
    vec3 ShadowDepth = texture(BakeShadowMap, f_texcoord1).ggg;
	float shadow_intensity = LightMapColor.a;

    // Diffuse lighting.
    float halfLambert = dot(difLightDirection, N) * 0.5 + 0.5;

    //Texture Overlay (Like an emblem in mk8)
    if (HasDiffuseLayer == 1)
        fragColor += vec4(texture(DiffuseLayer, f_texcoord3).rgb, 1) * vec4(1);

    // Default Shader
    vec4 alpha = texture2D(tex0, f_texcoord0).aaaa;

    if (HasTransparencyMap == 1)
    {
        // TODO: ???
        alpha = texture2D(TransparencyMap, f_texcoord0).rgba;
        alpha *= 0.5;
    }

	vec4 diffuseMapColor = vec4(texture(tex0, f_texcoord0).rgb, 1);
    diffuseMapColor *= halfLambert;
    fragColor.rgb += diffuseMapColor.rgb;

	float aoBlend = 1;
	if (HasShadowMap == 1)
	{
	     if (bake_shadow_type == 0)
		 {
		       aoBlend = texture(BakeShadowMap, f_texcoord1).r;
		       fragColor *= aoBlend;
		 }
         if (bake_shadow_type == 2)
		 {
		       aoBlend = texture(BakeShadowMap, f_texcoord1).r;
		       // fragColor *= aoBlend;

			   //For this it will need a frame buffer to be used
			   vec4 ShadowTex = vec4(texture(BakeShadowMap, f_texcoord1).ggg, 1);
		 }
	}

	vec3 tintColor = vec3(1);

    // Render Passes
    if (HasEmissionMap == 1 || enable_emission == 1) //Can be without texture map
		fragColor.rgb += EmissionPass(EmissionMap, emission_intensity, vert, 0, emission_color);
    fragColor.rgb += SpecularPass(I, N, HasSpecularMap, SpecularMap, specular_color, vert, 0);
    fragColor.rgb += ReflectionPass(N, I, diffuseMapColor, aoBlend, tintColor, vert);


   //Mario Odyssey uses this. Often for fur colors
   if (base_color_mul_color != vec4(0))
       fragColor *= min(base_color_mul_color, 1);

   // TODO: Broken for Intel Integrated
   // if (renderVertColor == 1)
   //     fragColor *= min(vertexColor, vec4(1));

    // Fragment alpha calculations.
    fragColor.a *= texture(tex0, f_texcoord0).a;

	if (isTransparent != 1)
        fragColor.a = 1;
}
