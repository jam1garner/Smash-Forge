#version 330

in vec3 objectPosition;
in vec3 normal;
in vec3 bitangent;
in vec3 tangent;
in vec4 color;
in vec2 UV0;

uniform int hasSphere0;
uniform int hasDiffuse0;
uniform sampler2D diffuseTex0;
uniform vec2 diffuseScale0;

uniform int hasSphere1;
uniform int hasDiffuse1;
uniform sampler2D diffuseTex1;
uniform vec2 diffuseScale1;

uniform int hasSpecular;
uniform sampler2D specularTex;
uniform vec2 specularScale;

uniform int hasBumpMap;
uniform int bumpMapWidth;
uniform int bumpMapHeight;
uniform sampler2D bumpMapTex;
uniform vec2 bumpMapTexScale;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform int enableSpecular;
uniform int enableDiffuseLighting;

uniform float glossiness;
uniform float transparency;

uniform int colorOverride;

uniform mat4 mvpMatrix;
uniform mat4 sphereMatrix;

uniform int renderDiffuse;
uniform int renderSpecular;

uniform int renderAlpha;

uniform int renderNormalMap;

out vec4 fragColor;

vec2 GetSphereCoords(vec3 N)
{
    vec3 viewNormal = mat3(sphereMatrix) * normal.xyz;
    return viewNormal.xy * 0.5 + 0.5;
}

vec3 DiffusePass(vec3 N, vec3 V)
{
    // Diffuse
    float blend = 0.1; // TODO: Use texture's blend.
    float lambert = clamp(dot(N, V), 0, 1);

    vec4 diffuseMap = vec4(1);

    vec2 diffuseCoords0 = UV0;
    if (hasSphere0 == 1)
        diffuseCoords0 = GetSphereCoords(N);

    vec2 diffuseCoords1 = UV0;
    if (hasSphere1 == 1)
        diffuseCoords1 = GetSphereCoords(N);

    if (hasDiffuse0 == 1)
        diffuseMap = texture(diffuseTex0, diffuseCoords0 * diffuseScale0).rgba;
    if (hasDiffuse1 == 1)
        diffuseMap = mix(diffuseMap, texture(diffuseTex1, diffuseCoords1 * diffuseScale1), 0.1);

    vec3 diffuseTerm = diffuseMap.rgb;
    if (enableDiffuseLighting == 1)
        diffuseTerm *= mix(ambientColor.rgb, diffuseColor.rgb, lambert);

    return diffuseTerm;
}

vec3 SpecularPass(vec3 N, vec3 V)
{
    // Specular
    float phong = clamp(dot(normal, V), 0, 1);
    phong = pow(phong, glossiness);
    vec3 specularTerm = vec3(phong) * specularColor.rgb;
    if (hasSpecular == 1)
        specularTerm *= texture(specularTex, UV0 * specularScale).rgb;
    specularTerm *= enableSpecular;

    return specularTerm;
}

// Defined in MeleeUtils.frag
vec3 CalculateBumpMapNormal(vec3 normal, vec3 tangent, vec3 bitangent,
    int hasBump, sampler2D bumpMap, int width, int height, vec2 texCoords);

void main()
{
	if (colorOverride == 1)
	{
		fragColor = vec4(1);
		return;
	}

	fragColor = vec4(0, 0, 0, 1);

	vec3 V = vec3(0, 0, -1) * mat3(mvpMatrix);
    vec3 N = normal;

	// Render passes
	fragColor.rgb += DiffusePass(N, V) * renderDiffuse;
	fragColor.rgb += SpecularPass(N, V) * renderSpecular;

	fragColor.rgb *= color.rgb;

    if (renderNormalMap == 1 && hasBumpMap == 1)
    {
        // TODO: Bump mapping?
        vec2 tex0 = UV0 * bumpMapTexScale;
        vec2 tex1 = tex0 + vec2(dot(vec3(0,0,1), tangent.xyz), dot(vec3(0,0,1), bitangent.xyz));
        vec3 bump0 = texture(bumpMapTex, tex0).rgb;
        vec3 bump1 = texture(bumpMapTex, tex1).rgb;

        vec3 difference = bump0 - bump1;
        fragColor.rgb *= difference + 1.0;
    }

	// Set alpha
    if (renderAlpha == 1)
        fragColor.a = texture(diffuseTex0, UV0 * diffuseScale0).a * transparency;
}
