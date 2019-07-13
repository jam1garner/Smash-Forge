#version 330

in vec3 viewPosition;
in vec3 objectPosition;
in vec2 texCoord;
in vec2 texCoord2;
in vec2 texCoord3;
in vec2 normaltexCoord;
in vec4 vertexColor;
in vec3 normal;
in vec3 viewNormal;
in vec3 tangent;
in vec3 bitangent;
in vec4 fragPos;
in vec4 fragPosLightSpace;
noperspective in vec3 edgeDistance;

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec4 fragColorBright;

// Wireframe Rendering
uniform int drawWireFrame;
uniform float lineWidth;

uniform int drawSelection;

uniform float bloomThreshold;

// Shadow Mapping
uniform int drawShadow;
uniform sampler2D depthMap;
mat4 lightMatrix;

// Polygon ID for viewport selection.
uniform int drawId;
uniform vec3 colorId;

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

// Defined in Wireframe.frag.
float WireframeIntensity(vec3 distanceToEdges);

// Defined in Utility.frag.
float Luminance(vec3 rgb);

// Defined in SmashShader.frag.
vec4 SmashShader(VertexAttributes vert);

float CalculateShadow(float shadowBrightness)
{
    // Shadow calculations
    vec3 projectionCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projectionCoords = projectionCoords * 0.5 + 0.5;
    float closestDepth = texture(depthMap, projectionCoords.xy).r;
    float currentDepth = projectionCoords.z;
    float shadowBias = 0.00085;
    if ((currentDepth - shadowBias) > closestDepth)
        return shadowBrightness;
    else
        return 1.0;
}

void main() {
    if (drawSelection == 1) {
        fragColor = vec4(1);
        return;
    }

    // Create a struct for passing all the vertex attributes to other functions.
    VertexAttributes vert;
    vert.viewPosition = viewPosition;
    vert.objectPosition = objectPosition;
    vert.texCoord = texCoord;
    vert.texCoord2 = texCoord2;
    vert.texCoord3 = texCoord3;
    vert.normaltexCoord = normaltexCoord;
    vert.vertexColor = vertexColor;
    vert.normal = normal;
    vert.viewNormal = viewNormal;
    vert.tangent = tangent;
    vert.bitangent = bitangent;

    fragColor = SmashShader(vert);

    // Separate bright and regular color for bloom calculations.
    fragColorBright = vec4(vec3(0), 1);
    if (Luminance(fragColor.rgb) > bloomThreshold)
        fragColorBright.rgb = fragColor.rgb;

    if (drawWireFrame == 1)
    {
        vec3 edgeColor = vec3(1);
        float intensity = WireframeIntensity(edgeDistance);
        fragColor.rgb = mix(fragColor.rgb, edgeColor, intensity);
    }

    if (drawId == 1) {
        // Draw a color ID map.
        fragColor.rgb = colorId;
    }

    // Shadow calculations
    if (drawShadow == 1)
        fragColor.rgb *= CalculateShadow(0.5);
}
