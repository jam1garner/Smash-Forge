#version 330

in vec3 objectPosition;
in vec3 normal;
in vec2 UV0;

uniform sampler2D diffuseTex;
uniform sampler2D specularTex;

uniform int hasDiffuse;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform int enableSpecular;
uniform float glossiness;
uniform float transparency;

uniform int colorOverride;

uniform mat4 mvpMatrix;

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
	vec3 diffuseMap = texture2D(diffuseTex, UV0).rgb;
    if (hasDiffuse == 0)
        diffuseMap = vec3(1);
	vec3 diffuseTerm = diffuseMap * mix(ambientColor.rgb, diffuseColor.rgb, lambert);

	// Specular
	float phong = clamp(dot(normal, V), 0, 1);
	phong = pow(phong, glossiness);
	vec3 specularTerm = vec3(phong) * specularColor.rgb;

	// Render passes
	fragColor.rgb += diffuseTerm;
	fragColor.rgb += specularTerm * enableSpecular;
}
