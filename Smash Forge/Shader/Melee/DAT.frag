#version 330

in vec3 objectPosition;
in vec3 normal;
in vec2 UV0;

uniform int hasDiffuse;
uniform sampler2D diffuseTex;
uniform vec2 diffuseScale;

uniform int hasSpecular;
uniform sampler2D specularTex;
uniform vec2 specularScale;

uniform int hasSphere;
uniform sampler2D sphereTex;
uniform vec2 sphereScale;

uniform int hasUnk2;
uniform sampler2D unk2Tex;
uniform vec2 unk2Scale;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform int enableSpecular;
uniform float glossiness;
uniform float transparency;

uniform int colorOverride;

uniform mat4 mvpMatrix;
uniform mat4 sphereMatrix;

uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderReflection;

out vec4 fragColor;

void main()
{
	if (colorOverride == 1)
	{
		fragColor = vec4(1);
		return;
	}

	fragColor = vec4(0, 0, 0, 1);

	vec3 V = vec3(0, 0, -1) * mat3(mvpMatrix);

	// Diffuse
	float lambert = clamp(dot(normal, V), 0, 1);
	vec4 diffuseMap = vec4(1);
    if (hasDiffuse == 1)
        diffuseMap = texture2D(diffuseTex, UV0 * diffuseScale).rgba;

	// Sphere maps
	if (hasSphere == 1)
	{
		vec3 viewNormal = mat3(sphereMatrix) * normal.xyz;
		vec2 sphereCoords = viewNormal.xy * 0.5 + 0.5;
		diffuseMap = texture(sphereTex, sphereCoords).rgba;
	}

	// TODO: This might also be a bump map.
	if (hasUnk2 == 1)
		diffuseMap.rgb *= texture(unk2Tex, UV0 * unk2Scale).rgb;

	vec3 diffuseTerm = diffuseMap.rgb * mix(ambientColor.rgb, diffuseColor.rgb, lambert);

	// Specular
	float phong = clamp(dot(normal, V), 0, 1);
	phong = pow(phong, glossiness);
	vec3 specularTerm = vec3(phong) * specularColor.rgb;
    if (hasSpecular == 1)
        specularTerm *= texture(specularTex, UV0 * specularScale).rgb;
	specularTerm *= enableSpecular;

	// Render passes
	fragColor.rgb += diffuseTerm * renderDiffuse;
	fragColor.rgb += specularTerm * renderSpecular;

	// Set alpha
    fragColor.a = diffuseMap.a * transparency;
}
