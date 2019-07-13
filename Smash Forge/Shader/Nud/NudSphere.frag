#version 330

in vec2 quadTexCoord;

out vec4 fragColor;

// Textures are used instead of vertex attributes.
uniform sampler2D normalTex;
uniform sampler2D uvTex;
uniform sampler2D tanTex;
uniform sampler2D bitanTex;

// Constants
#define gamma 2.2
#define PI 3.14159

struct VertexAttributes {
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
};

// Defined in SmashShader.frag.
vec4 SmashShader(VertexAttributes vert);

void main() {
    fragColor = vec4(1);

    vec2 texCoord = vec2(quadTexCoord.x, 1 - quadTexCoord.y);
    vec2 meshTexCoord = texture(uvTex, quadTexCoord).xy * 1.15 + vec2(0.75, 0.0);

    // Create a struct for passing all the vertex attributes to other functions.
    VertexAttributes vert;
    vert.viewPosition = vec3(0);
    vert.objectPosition = vec3(0);
    vert.texCoord = meshTexCoord;
    vert.texCoord2 = meshTexCoord;
    vert.texCoord3 = meshTexCoord;
    vert.normaltexCoord = meshTexCoord;
    vert.vertexColor = vec4(1);
    vert.normal = texture(normalTex, texCoord).xyz * 2 - 1;
    vert.viewNormal = texture(normalTex, texCoord).xyz * 2 - 1;
    vert.tangent = texture(tanTex, texCoord).xyz * 2 - 1;
    vert.bitangent = texture(bitanTex, texCoord).xyz * 2 - 1;

    float alpha = texture(normalTex, texCoord).a;
    vec3 shadedColor = SmashShader(vert).rgb;
    vec3 backgroundColor = vec3(0);

    fragColor.rgb = shadedColor;
    fragColor.a = alpha;
}
