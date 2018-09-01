#version 330

in vec3 V;
in vec3 N;
in vec2 UV0;

out vec4 fragColor;
uniform sampler2D TEX0;

uniform int colorOverride;

uniform vec4 AMB;
uniform vec4 DIF;
uniform vec4 SPC;

uniform int Flags;
uniform float Glossiness;
uniform float Transparency;

uniform vec3 Eye;

const vec3 LightPosition = vec3(10, 30, 40);

void main() {
	vec3 L = normalize(LightPosition - V);
	vec3 R = normalize(-reflect(L, N));

	// Diffuse
	float diff = max(dot(N, L), 0.0);  
	diff = clamp(diff, 0.0, 1.0); 

	// Specular
	float spec = pow(max(dot(R, normalize(-Eye)),0.0), 0.3);
	spec = clamp(spec, 0.0, 1.0); 

	// Final Elements
	vec3 FAMB = vec3(0.3);//AMB.rgb * 0.3;
	vec3 FDIF = diff * texture2D(TEX0, UV0).rgb * DIF.rgb;
	vec3 FSPC = spec * SPC.rgb;

	if((Flags & 0xF) == 0xC)
		fragColor = vec4(texture2D(TEX0, UV0).rgb, 1);
	else
		fragColor = vec4(texture2D(TEX0, UV0).rgb, 1);
	if(colorOverride == 1)
		fragColor.rgb = vec3(1);
}
