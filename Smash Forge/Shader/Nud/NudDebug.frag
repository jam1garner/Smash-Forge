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
in vec3 boneWeightsColored;
noperspective in vec3 edgeDistance;

out vec4 fragColor;

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
uniform int hasDummyRamp;
uniform int hasColorGainOffset;
uniform int hasSpecularParams;
uniform int useDiffuseBlend;
uniform int hasDualNormal;
uniform int hasSoftLight;
uniform int hasCustomSoftLight;

uniform vec3 lightSetColor;

uniform int uvChannel;
uniform int debug1;
uniform int debug2;
uniform int drawWireFrame;

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

// render settings
uniform int renderType;
uniform int useNormalMap;

uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;

// character lighting
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform vec3 difLightDirection;

uniform vec3 stageFogColor;

uniform int drawSelection;

// Polygon ID for viewport selection.
uniform int drawId;
uniform vec3 colorId;

// Constants
#define gamma 2.2
#define PI 3.14159

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

// Defined in SmashShader.frag.
vec3 DiffuseAOBlend(float aoMap, vec4 aoMinGain);
vec3 BumpMapNormal(sampler2D normalMap, VertexAttributes vert, vec4 dualNormalScrollParams,
                                                               int hasDualNormal, vec4 normalParams);

// Defined in Utility.frag.
float WireframeIntensity(vec3 distanceToEdges);
float Luminance(vec3 rgb);

// Defined in StageLighting.frag.
vec3 Lighting(vec3 N, float halfLambert);
vec3 FogColor(vec3 inputColor, vec4 fogParams, float depth, vec3 stageFogColor);

void main() {
    fragColor = vec4(0,0,0,1);

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

    // Remap vectors to visible range, but still show <0,0,0> as black.
    // Normals
    vec3 bumpMapNormal = BumpMapNormal(normalMap, vert, dualNormalScrollParams, hasDualNormal, normalParams);
    vec3 displayNormal = (bumpMapNormal * 0.5) + 0.5;
    if (hasNrm == 0 || useNormalMap == 0)
        displayNormal = vert.normal * 0.5 + 0.5;
    if (dot(bumpMapNormal, vec3(1)) == 0)
        displayNormal = vec3(0);

    // Tangents
    vec3 displayTangent = (tangent * 0.5) + 0.5;
    if (dot(tangent, vec3(1)) == 0)
        displayTangent = vec3(0);

    // Bitangents
    vec3 displayBitangent = (bitangent * 0.5) + 0.5;
    if (dot(bitangent, vec3(1)) == 0)
        displayBitangent = vec3(0);

    // Diffuse calculations
    float halfLambert = dot(difLightDirection, bumpMapNormal) * 0.5 + 0.5;

    // zOffset correction
    gl_FragDepth = gl_FragCoord.z - (zOffset.x / 1500); // divide by far plane?

    // similar to du dv but uses just the normal map
    float offsetIntensity = 0;
    if(useNormalMap == 1)
        offsetIntensity = normalParams.z;
    vec2 textureOffset = 1 - texture(normalMap, normaltexCoord).xy;
    textureOffset = (textureOffset * 2) - 1; // remap to -1 to 1?
    vec2 offsetTexCoord = texCoord + (textureOffset * offsetIntensity);

    // calculate diffuse map blending to use in Shaded and Diffuse Maps render modes
    vec4 diffuse1 = texture(dif, texCoord);
    vec4 diffuse2 = texture(dif2, texCoord2);
    vec4 diffuse3 = texture(dif3, texCoord3);

    // Ambient occlusion map and NU_aoMinGain
    vec3 aoBlend = vec3(1);
    if (hasNrm == 1)
        aoBlend = DiffuseAOBlend(texture(normalMap, vert.texCoord).a, aoMinGain);
    else
        aoBlend = DiffuseAOBlend(1.0, aoMinGain);

    vec4 resultingColor = vec4(1);

    // Sets which diffuse texture or UV coords to display based on UV channel.
    vec4 displayDiffuse = diffuse1;
    vec2 displayTexCoord = texCoord;
    if (uvChannel == 2) {
        displayTexCoord = texCoord2;
        displayDiffuse = diffuse2;
    } else if (uvChannel == 3) {
        displayTexCoord = texCoord3;
        displayDiffuse = diffuse3;
    }

    // render modes
    if (renderType == 1) // normals color
        resultingColor.rgb = displayNormal;
    else if (renderType == 2) {
        // lighting
        resultingColor.rgb = Lighting(bumpMapNormal, halfLambert);
        resultingColor.rgb = FogColor(resultingColor.rgb, fogParams, vert.viewPosition.z, stageFogColor);
    } else if (renderType == 3) {
        // diffuse map
        resultingColor.rgba = displayDiffuse;
    } else if (renderType == 4) {
        // normal map
        resultingColor.rgb = texture(normalMap, normaltexCoord).rgb;
    } else if (renderType == 5) {
        // vertexColor. The default range is [0,128].
        // Conversion from [0, 128] to [0, 1] is done prior to shader. This allows value range [0,2]
        resultingColor = vertexColor;
        if (debug1 == 1) {
            resultingColor *= 0.5;
        }
    } else if (renderType == 6) {
        // ambient occlusion
        resultingColor.rgb = pow(texture(normalMap, texCoord).aaa, vec3(1));
        if (debug1 == 1)
            resultingColor.rgb = pow(aoBlend.rgb, vec3(1 / 2.2));
    } else if (renderType == 7) {
        // uv coords
        resultingColor.rgb = vec3(displayTexCoord, 1);
    } else if (renderType == 8) {
        // uv test pattern
        resultingColor.rgb = texture(UVTestPattern, displayTexCoord).rgb;
    } else if (renderType == 9) {
         // tangents
         resultingColor.rgb = displayTangent;
    }
    else if (renderType == 10) {
        // bitangents
        resultingColor.rgb = displayBitangent;
    } else if (renderType == 11) {
        // light set #
        resultingColor.rgb = lightSetColor;
    } else if (renderType == 12) {
        resultingColor.rgb = boneWeightsColored;
    }

    // Toggles rendering of individual color channels for all render modes.
    fragColor.rgb = resultingColor.rgb * vec3(renderR, renderG, renderB);
    if (renderR == 1 && renderG == 0 && renderB == 0)
        fragColor.rgb = resultingColor.rrr;
    else if (renderG == 1 && renderR == 0 && renderB == 0)
        fragColor.rgb = resultingColor.ggg;
    else if (renderB == 1 && renderR == 0 && renderG == 0)
        fragColor.rgb = resultingColor.bbb;

    if (renderAlpha == 1) {
        fragColor.a = resultingColor.a;
    }
    if (alphaOverride == 1)
        fragColor = vec4(resultingColor.aaa, 1);

    // Rendering overrides.
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
}
