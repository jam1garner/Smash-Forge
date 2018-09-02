#version 330

in vec3 objectPosition;
in vec3 normal;
in vec2 UV0;

uniform sampler2D TEX0;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform float Glossiness;
uniform float Transparency;

uniform int colorOverride;

uniform mat4 mvpMatrix;

out vec4 fragColor;

void main()
{
	if(colorOverride == 1)
	{
		fragColor = vec4(1);
		return;
	}

	fragColor = vec4(0, 0, 0, 1);

	vec3 V = vec3(0,0,-1) * mat3(mvpMatrix);

	// Diffuse
	float lambert = clamp(dot(normal, V), 0, 1);
	vec3 diffuseColor = texture2D(TEX0, UV0).rgb;
	vec3 diffuseTerm = diffuseColor * mix(ambientColor.rgb, diffuseColor.rgb, lambert);

	// Specular
	float phong = clamp(dot(normal, V), 0, 1);
	phong = pow(phong, 8);
	vec3 specularTerm = vec3(phong) * specularColor.rgb;

	// Render passes
	fragColor.rgb += diffuseTerm;
	fragColor.rgb += specularTerm;
}
