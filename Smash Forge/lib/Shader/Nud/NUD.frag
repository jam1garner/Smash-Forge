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

// Textures
uniform sampler2D dif;
uniform sampler2D dif2;
uniform sampler2D dif3;
uniform sampler2D ramp;
uniform sampler2D dummyRamp;
uniform sampler2D normalMap;
uniform sampler2D ao;
uniform samplerCube cube;
uniform samplerCube stagecube;
uniform sampler2D spheremap;
uniform samplerCube cmap;
uniform sampler2D UVTestPattern;

// flags tests
uniform int hasDif;
uniform int hasDif2;
uniform int hasDif3;
uniform int hasStage;
uniform int hasCube;
uniform int hasNrm;
uniform int hasRamp;
uniform int hasAo;
uniform int hasSphereMap;
uniform int hasDummyRamp;

// flags tests
uniform int hasColorGainOffset;
uniform int hasSpecularParams;
uniform int useDiffuseBlend;
uniform int hasDualNormal;
uniform int hasSoftLight;
uniform int hasCustomSoftLight;
uniform int hasFinalColorGain;
uniform int useDifRefMask;
uniform int hasBayoHair;
uniform int softLightBrighten;
uniform int hasUniverseParam;

// Da Flags
uniform uint flags;

// Check if src, dst, alpha function are non zero.
uniform int isTransparent;

// Render Settings
uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderFresnel;
uniform int renderReflection;
uniform int renderType;
uniform int renderLighting;
uniform int renderVertColor;
uniform int renderNormal;
uniform int useNormalMap;
// Wireframe Rendering
uniform int drawWireFrame;
uniform float lineWidth;

uniform int drawSelection;

// Channel Toggles
uniform int renderAlpha;

// Render Pass Intensities
uniform float diffuseIntensity;
uniform float ambientIntensity;
uniform float specularIntensity;
uniform float fresnelIntensity;
uniform float reflectionIntensity;

// Not found in game yet.
uniform vec3 refLightColor;

// Matrices
uniform mat4 mvpMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 sphereMapMatrix;
uniform mat4 rotationMatrix;

// Misc Mesh Attributes
uniform float zBufferOffset;

uniform vec3 cameraPosition;

uniform float bloomThreshold;

uniform sampler2D shadowMap;

// Stage Lighting
uniform int lightSet;
uniform int isStage;
uniform int renderStageLighting;

// Stage Light 1
uniform int renderStageLight1;
uniform vec3 stageLight1Color;
uniform vec3 stageLight1Direction;

// Stage Light 2
uniform int renderStageLight2;
uniform vec3 stageLight2Color;
uniform vec3 stageLight2Direction;

// Stage Light 3
uniform int renderStageLight3;
uniform vec3 stageLight3Color;
uniform vec3 stageLight3Direction;

// Stage Light 4
uniform int renderStageLight4;
uniform vec3 stageLight4Color;
uniform vec3 stageLight4Direction;

// light_set fog
uniform int renderFog;
uniform vec3 stageFogColor;

// Character Lighting
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform vec3 difLightDirection;

uniform vec3 difLight2Color;
uniform vec3 ambLight2Color;
uniform vec3 difLight2Direction;

uniform vec3 difLight3Color;
uniform vec3 ambLight3Color;
uniform vec3 difLight3Direction;

uniform vec3 specLightColor;
uniform vec3 specLightDirection;

// Shared by characters and stages.
uniform vec3 fresGroundColor;
uniform vec3 fresSkyColor;
uniform vec3 fresSkyDirection;
uniform vec3 fresGroundDirection;

// NU_ Material Properties
uniform vec4 colorOffset;
uniform vec4 aoMinGain;
uniform vec4 fresnelColor;
uniform vec4 specularColor;
uniform vec4 specularColorGain;
uniform vec4 diffuseColor;
uniform vec4 characterColor;
uniform vec4 colorGain;
uniform vec4 finalColorGain;
uniform vec4 finalColorGain2;
uniform vec4 finalColorGain3;
uniform vec4 reflectionColor;
uniform vec4 fogColor;
uniform vec4 effColorGain;
uniform vec4 zOffset;

// NU_ material params
uniform vec4 fresnelParams;
uniform vec4 specularParams;
uniform vec4 reflectionParams;
uniform vec4 fogParams;
uniform vec4 normalParams;
uniform vec4 angleFadeParams;
uniform vec4 dualNormalScrollParams;
uniform vec4 alphaBlendParams;
uniform vec4 softLightingParams;
uniform vec4 customSoftLightParams;
uniform vec4 effUniverseParam;

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
