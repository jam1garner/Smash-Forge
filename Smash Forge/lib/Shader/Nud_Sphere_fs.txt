#version 330
in vec2 quadTexCoord;

uniform sampler2D normalTex;
uniform sampler2D uvTex;
uniform sampler2D tanTex;
uniform sampler2D bitanTex;

vec3 viewPosition;
vec3 objectPosition;
vec2 texCoord;
vec2 texCoord2;
vec2 texCoord3;
vec2 normaltexCoord;
vec4 vertexColor;
vec3 normal;
vec3 viewNormal;
vec3 tangent;
vec3 bitangent;

#include MISC_UNIFORMS

#include STAGE_LIGHTING_UNIFORMS

#include NU_UNIFORMS

// Constants
#define gamma 2.2
#define PI 3.14159

#include SMASH_SHADER

out vec4 fragColor;

void main()
{
    fragColor = vec4(1,0,0,1);

    // Set the mesh attributes using textures instead. TODO: Fix texCoords.
    vec4 nrm = texture(normalTex, vec2(quadTexCoord.x, 1 - quadTexCoord.y));
    fragColor = nrm;

    normal = nrm.xyz * 2 - 1; // remap to [-1,1]
    tangent = texture(tanTex, vec2(quadTexCoord.x, 1 - quadTexCoord.y)).xyz * 2 - 1;
    bitangent = texture(bitanTex, vec2(quadTexCoord.x, 1 - quadTexCoord.y)).xyz * 2 - 1;
    vec2 meshTexCoord = texture(uvTex, quadTexCoord).xy;

    fragColor.rgb = texture(UVTestPattern, meshTexCoord).rgb;
    fragColor.rgb += vec3(Fresnel(vec3(0,0,1), normal));

    fragColor.rgb = mix(vec3(0), fragColor.rgb, nrm.a);
}
