# version 330

// A struct is used for what would normally be attributes from the vert/geom shader.
struct VertexAttributes
{
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
uniform int drawWireframe;
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

uniform MaterialProperties
{
    vec4 colorOffset;
    vec4 aoMinGain;
    vec4 fresnelColor;
    vec4 specularColor;
    vec4 specularColorGain;
    vec4 diffuseColor;
    vec4 characterColor;
    vec4 colorGain;
    vec4 finalColorGain;
    vec4 finalColorGain2;
    vec4 finalColorGain3;
    vec4 reflectionColor;
    vec4 fogColor;
    vec4 effColorGain;
    vec4 zOffset;
    vec4 colorSamplerUV;
    vec4 colorSampler2UV;
    vec4 colorSampler3UV;
    vec4 normalSamplerAUV;
    vec4 normalSamplerBUV;

    vec4 fresnelParams;
    vec4 specularParams;
    vec4 reflectionParams;
    vec4 fogParams;
    vec4 normalParams;
    vec4 angleFadeParams;
    vec4 dualNormalScrollParams;
    vec4 alphaBlendParams;
    vec4 softLightingParams;
    vec4 customSoftLightParams;
    vec4 effUniverseParam;
};

// Defined in SmashShader.frag.
float AnisotropicSpecExponent(vec3 halfAngle, float width, float height, vec3 tangent, vec3 bitangent);

vec3 BayoHairDiffuse(vec3 diffuseMap, vec4 colorOffset, vec4 colorGain, vec4 alphaBlendParams) {
    vec3 diffuseColor = (colorOffset.rgb + diffuseMap.rrr) * colorGain.rgb;
    diffuseColor *= alphaBlendParams.w; // #justbayothings
    return diffuseColor;
}

vec3 BayoHairSpecular(vec3 diffuseMap, vec3 I, vec3 specLightDirection, vec4 reflectionParams, VertexAttributes vert) {
    float specMask = diffuseMap.g;

    vec3 halfAngle = normalize(I + specLightDirection);
    float exponent = AnisotropicSpecExponent(halfAngle, reflectionParams.z, reflectionParams.w, vert.tangent, vert.bitangent);
    float specularTerm = pow(dot(vert.bitangent.xyz, halfAngle), exponent);
    specularTerm = 1;

    // This is some sort of BRDF map.
    float u = dot(vert.bitangent, halfAngle) * 0.5 + 0.5;
    float v = dot(vert.tangent, halfAngle) *  0.5 + 0.5;
    vec3 specColor = texture(dummyRamp, vec2(u, v)).rgb;
    // TODO: Find proper constants.
    float intensity = alphaBlendParams.z * 0.1;
    vec3 specularColorTotal = specColor * specularTerm * specMask * intensity;
    specularColorTotal *= colorGain.rgb; // #justbayothings
    specularColorTotal = min(specularColorTotal, vec3(1));
    return specularColorTotal;
}
