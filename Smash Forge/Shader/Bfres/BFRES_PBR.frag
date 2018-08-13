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
uniform sampler2D spl;
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
uniform sampler2D BOTWSpecularMap;

uniform samplerCube irradianceMap;
uniform samplerCube prefilterMap;
uniform sampler2D brdfLUT;

// Shader Params
uniform float normal_map_weight;
uniform float ao_density;
uniform float emission_intensity;
uniform vec4 fresnelParams;
uniform vec4 base_color_mul_color;
uniform vec3 emission_color;

// Shader Options
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
uniform int HasBOTWSpecularMap;

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
const float PI = 3.14159265359;

// Defined in BFRES_Utility.frag.
vec3 CalcBumpedNormal(vec3 normal, sampler2D normalMap, VertexAttributes vert, float texCoordIndex);
float AmbientOcclusionBlend(sampler2D BakeShadowMap, VertexAttributes vert, float ao_density);
vec3 EmissionPass(sampler2D EmissionMap, float emission_intensity, VertexAttributes vert, float texCoordIndex, vec3 emission_color);

// Shader code adapted from learnopengl.com's PBR tutorial:
// https://learnopengl.com/PBR/Theory

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

vec3 saturation(vec3 rgb, float adjustment)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    vec3 intensity = vec3(dot(rgb, W));
    return mix(intensity, rgb, adjustment);
}

void main()
{
    fragColor = vec4(1);

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

    vec3 lightColor = vec3(10);

    // Wireframe color.
    if (colorOverride == 1)
    {
        fragColor = vec4(1);
        return;
    }

	vec3 albedo = vec3(1);
    if (HasDiffuse == 1)
        albedo = pow(texture(tex0, f_texcoord0).rgb, vec3(gamma));

	float metallic = DefaultMetalness;
    if (HasMetalnessMap == 1)
        metallic = texture(MetalnessMap, f_texcoord0).r;

	float roughness = DefaultRoughness;
    if (HasRoughnessMap == 1)
        roughness = texture(RoughnessMap, f_texcoord0).r;

	float ao = 1;
    if (HasShadowMap == 1 && UseAOMap == 1)
        ao = texture(BakeShadowMap, f_texcoord1).r;

	float shadow = 1;
    if (HasShadowMap == 1)
        shadow = texture(BakeShadowMap, f_texcoord1).g;

	float cavity = 1;

	vec3 emission = vec3(0);
   if (HasEmissionMap == 1 || enable_emission == 1) //Can be without texture map
		emission.rgb += EmissionPass(EmissionMap, emission_intensity, vert, 0, emission_color);

	vec3 lightMapColor = vec3(1);
	float lightMapIntensity = 0;
    if (HasLightMap == 1)
    {
        lightMapColor = texture(BakeLightMap, f_texcoord1).rgb;
        lightMapIntensity = texture(BakeLightMap, f_texcoord1).a;
    }

	float specIntensity = 1;

	if (HasMRA == 1) //Kirby Star Allies PBR map
	{
	    //Note KSA has no way to tell if one gets unused or not because shaders :(
		//Usually it's just metalness with roughness and works fine
		metallic = texture(MRA, f_texcoord0).r;
		roughness = texture(MRA, f_texcoord0).g;
		cavity = texture(MRA, f_texcoord0).b;
		ao = texture(MRA, f_texcoord0).a;
	}

    // Calculate shading vectors.
    vec3 I = vec3(0,0,-1) * mat3(mvpMatrix);
    vec3 N = normal;
	if (HasNormalMap == 1 && useNormalMap == 1)
		N = CalcBumpedNormal(normal, normalMap, vert, 0);

    vec3 V = normalize(I); //Eye View
	vec3 L = normalize(specLightDirection); //Light
	vec3 H = normalize(specLightDirection + I); //Half Angle

	// attenuation
    float A = 20.0 / dot(L - objectPosition, L - objectPosition);

    vec3 F0 = vec3(0.04); //0.04 for dielectric materials
    F0 = mix(F0, albedo, metallic);

	vec3 Lo = vec3(0.0);

	vec3 radiance = lightColor; //no attenuation

	//calculate Cook-Torrance BRDF
	float NDF = DistributionGGX(N, H, roughness);
	float G = GeometrySmith(N, V, L, roughness);
    vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

 	vec3 numerator = NDF * G * F;
	float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero.
	vec3 specular  = numerator / denominator * specIntensity;

	// kS is equal to Fresnel
    vec3 kS = F;

	vec3 kD = vec3(1.0) - kS;
	kD *= 1.0 - metallic;

	float NdotL = max(dot(N, L), 0.0);

    if (enableCellShading == 1)
	{
       // Higher blend values make the dark region smoother and larger.
        float smoothness = 0.1;
        float center = 0.5;
        float edgeL = center;
        float edgeR = center + (smoothness * 0.5);
        float smoothLambert = smoothstep(edgeL, edgeR, NdotL);

        float ambient = 0.6;
        smoothLambert = clamp(smoothLambert + ambient, 0, 1);

		NdotL = smoothLambert * 0.8;
	}

    // add to outgoing radiance Lo
    Lo = (kD * albedo / PI + specular) * radiance * NdotL;

    //Start IBL stuff

	vec3 color = vec3(1);

    // TODO: Renders as invisible on AMD even when useImageBasedLighting != 1.
	// if (useImageBasedLighting == 1)
	// {
    //     vec3 R = reflect(-V, N);
    //
    //     // ambient lighting (we now use IBL as the ambient term)
    //     F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);
    //     kS = F;
    //     kD = 1.0 - kS;
    //     kD *= 1.0 - metallic;
    //
    //     vec3 irradiance = texture(irradianceMap, N).rgb;
    //     vec3 diffuse = irradiance * albedo;
    //
    //     const float maxReflectionLod = 8.0;
    //     vec3 prefilteredColor = textureLod(prefilterMap, R,  roughness * maxReflectionLod).rgb;
    //
    //     vec2 envBRDF  = texture(brdfLUT, vec2(max(dot(N, V), 0.0), roughness)).rg;
    //     specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);
    //
    //     vec3 ambient = (kD * diffuse + specular);
    //
    //     color = ambient + Lo;
    //
    //     // HDR tonemapping. Raised high to fix brightness issues
    //     color = color / (color + vec3(10000.0));
    //
    //     // correct gamma
    //     color = pow(color, vec3(1.0/gamma));
	// }
	// else
	{
	    vec3 ambient = vec3(0.03) * albedo;
	    color = ambient + Lo;
	}

    color *= ao;
    color *= (0.6 + shadow);

    float cavityStrength = 1.0;
    color *= cavity * cavityStrength + (1.0-cavityStrength);

    color = pow(color, vec3(1.0 / gamma));

	float alpha = texture(tex0, f_texcoord0).a;

	fragColor = vec4(color, alpha);

    if (renderVertColor == 1)
        fragColor *= min(vertexColor, vec4(1));
}
