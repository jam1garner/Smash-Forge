#version 330

in vec3 viewPosition;
in vec3 objectPosition;
in vec2 texCoord;
in vec2 texCoord2;
in vec2 texCoord3;
in vec2 normaltexCoord;
in vec4 vertexColor;
in vec3 normal;
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

// Da Flags
uniform uint flags;
uniform int isTransparent;

uniform int lightSet;
uniform int isStage;
uniform int renderStageLighting;

uniform int uvChannel;
uniform int debug1;
uniform int debug2;
uniform int drawWireframe;

// NU_ Material Properties
uniform vec4 colorOffset;
uniform vec4 aoMinGain;
uniform vec4 fresnelColor;
uniform vec4 specularColor;
uniform vec4 specularColorGain;
uniform vec4 diffuseColor;
uniform vec4 colorGain;
uniform vec4 finalColorGain;
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

// render settings
uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderFresnel;
uniform int renderReflection;
uniform int renderType;
uniform int renderLighting;
uniform int renderVertColor;
uniform int useNormalMap;

uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;

uniform float diffuseIntensity;
uniform float ambientIntensity;
uniform float specularIntensity;
uniform float fresnelIntensity;
uniform float reflectionIntensity;

// character lighting
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform vec3 difLightDirection;
uniform vec3 fresGroundColor;
uniform vec3 fresSkyColor;
uniform vec3 specLightColor;
uniform vec3 specLightDirection;
uniform vec3 refLightColor;

// stage light 1
uniform int renderStageLight1;
uniform vec3 stageLight1Color;
uniform vec3 stageLight1Direction;

// stage light 2
uniform int renderStageLight2;
uniform vec3 stageLight2Color;
uniform vec3 stageLight2Direction;

// stage light 3
uniform int renderStageLight3;
uniform vec3 stageLight3Color;
uniform vec3 stageLight3Direction;

// stage light 4
uniform int renderStageLight4;
uniform vec3 stageLight4Color;
uniform vec3 stageLight4Direction;

// light_set fog
uniform int renderFog;
uniform vec3 stageFogColor;

uniform mat4 mvpMatrix;
uniform vec3 lightPosition;
uniform vec3 lightDirection;
uniform sampler2D shadowMap;

uniform int selectedBoneIndex;
uniform vec3 NSC;
uniform float elapsedTime;

// Constants
#define gamma 2.2
#define PI 3.14159

// Tools
vec3 RGB2HSV(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 HSV2RGB(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float Luminance(vec3 rgb)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    return dot(rgb, W);
}

vec3 CalculateTintColor(vec3 inputColor, float colorAlpha)
{
    float intensity = colorAlpha * 0.4;
    vec3 inputHSV = RGB2HSV(inputColor);
    float outSaturation = min((inputHSV.y * intensity),1); // cant have color with saturation > 1
    vec3 outColorTint = HSV2RGB(vec3(inputHSV.x,outSaturation,1));
    return outColorTint;
}

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    return shadow;
}

vec3 CalcBumpedNormal(vec3 inputNormal)
{
    // if no normal map, then return just the normal
    if(hasNrm == 0 || useNormalMap == 0)
	   return inputNormal;

    float normalIntensity = normalParams.x;
    vec3 BumpMapNormal = texture(normalMap, normaltexCoord).xyz;
    vec3 BumpMapNormal2 = texture(normalMap, vec2(normaltexCoord.x + dualNormalScrollParams.x, normaltexCoord.y + dualNormalScrollParams.y)).xyz;
    if(hasDualNormal == 1)
        BumpMapNormal = normalize(BumpMapNormal + BumpMapNormal2);
    BumpMapNormal = mix(vec3(0.5, 0.5, 1), BumpMapNormal, normalIntensity); // probably a better way to do this
    BumpMapNormal = 2.0 * BumpMapNormal - vec3(1);

    vec3 NewNormal;
    vec3 Normal = normalize(normal);
    mat3 TBN = mat3(tangent, bitangent, Normal);
    NewNormal = TBN * BumpMapNormal;
    NewNormal = normalize(NewNormal);

    return NewNormal;
}

vec3 CalculateLighting(vec3 N, float halfLambert)
{
    vec3 lighting = vec3(1);

    if (isStage == 1) // stage lighting
    {
        // should this be half lambert?
        vec3 stageLight1 = stageLight1Color * max((dot(N, stageLight1Direction)), 0);
        vec3 stageLight2 = stageLight2Color * max((dot(N, stageLight2Direction)), 0);
        vec3 stageLight3 = stageLight3Color * max((dot(N, stageLight3Direction)), 0);
        vec3 stageLight4 = stageLight4Color * max((dot(N, stageLight4Direction)), 0);

        lighting = vec3(0);
        lighting += (stageLight1 * renderStageLight1);
        lighting += (stageLight2 * renderStageLight2);
        lighting += (stageLight3 * renderStageLight3);
        lighting += (stageLight4 * renderStageLight4);
    }
    else // gradient based character lighting
    {
        lighting = mix(ambLightColor * ambientIntensity, difLightColor * diffuseIntensity, halfLambert);
    }

    if (((flags & 0x0F000000u) < 0x04000000u) || renderStageLighting != 1)
        lighting = vec3(1); // flags for "no lighting"

    return lighting;
}

vec3 CalculateFog(vec3 inputColor)
{
    float depth = viewPosition.z;
    depth = clamp((depth / fogParams.y), 0, 1);
    float fogIntensity = mix(fogParams.z, fogParams.w, depth);
    if(renderFog == 1 && renderStageLighting == 1)
        return mix(inputColor, pow((stageFogColor), vec3(gamma)), fogIntensity);
    else
        return inputColor;
}

void main()
{
    fragColor = vec4(0,0,0,1);

    // remap vectors for nicer visualization
    vec3 bumpMapNormal = CalcBumpedNormal(normal);
    vec3 displayNormal = (bumpMapNormal * 0.5) + 0.5;

    // diffuse calculations
    float halfLambert = dot(difLightDirection, bumpMapNormal) * 0.5 + 0.5;

    // still show <0,0,0> as black
    vec3 displayTangent = (tangent * 0.5) + 0.5;
    if (dot(tangent, vec3(1)) == 0)
        displayTangent = vec3(0);

    // still show <0,0,0> as black
    vec3 displayBitangent = (bitangent * 0.5) + 0.5;
    if (dot(bitangent, vec3(1)) == 0)
        displayBitangent = vec3(0);

    // zOffset correction
    gl_FragDepth = gl_FragCoord.z - (zOffset.x / 1500); // divide by far plane?

    // similar to du dv but uses just the normal map
    float offsetIntensity = 0;
    if(useNormalMap == 1)
        offsetIntensity = normalParams.z;
    vec2 textureOffset = 1 - texture(normalMap, normaltexCoord).xy;
    textureOffset = (textureOffset * 2) -1; // remap to -1 to 1?
    vec2 offsetTexCoord = texCoord + (textureOffset * offsetIntensity);

    // calculate diffuse map blending to use in Shaded and Diffuse Maps render modes
    vec4 diffuse1 = texture(dif, texCoord);
    vec4 diffuse2 = texture(dif2, texCoord2);
    vec4 diffuse3 = texture(dif3, texCoord3);

    vec3 aoBlend = vec3(1);
    float aoMap = texture(normalMap, texCoord).a;
    float maxAOBlendValue = 1.2;
    aoBlend = min((vec3(aoMap) + aoMinGain.rgb), vec3(maxAOBlendValue));
    vec3 aoGain = min((aoMap * (1 + aoMinGain.rgb)), vec3(1.0));
	vec3 c1 = RGB2HSV(aoBlend);
    vec3 c2 = RGB2HSV(aoGain);
	aoBlend.rgb = HSV2RGB(vec3(c1.x, c2.y, c1.z));

    vec4 resultingColor = vec4(1);

    // Sets which diffuse texture or UV coords to display based on UV channel.
    vec4 displayDiffuse = diffuse1;
    vec2 displayTexCoord = texCoord;
    if (uvChannel == 2)
    {
        displayTexCoord = texCoord2;
        displayDiffuse = diffuse2;
    }
    else if (uvChannel == 3)
    {
        displayTexCoord = texCoord3;
        displayDiffuse = diffuse3;
    }

    // render modes
    if (renderType == 1) // normals color
        resultingColor.rgb = displayNormal;
    else if (renderType == 2) // lighting
    {
        resultingColor.rgb = CalculateLighting(bumpMapNormal, halfLambert);
        resultingColor.rgb = CalculateFog(resultingColor.rgb);
    }
    else if (renderType == 3) // diffuse map
    {
        resultingColor.rgba = displayDiffuse;
    }
    else if (renderType == 4) // normal map
        resultingColor.rgb = texture(normalMap, normaltexCoord).rgb;
    else if (renderType == 5) // vertexColor
    {
        // The default range is [0,128].
        // Conversion from [0, 128] to [0, 1] is done prior to shader.
        // This allows values in range [0,2]
        resultingColor = vertexColor;
        if (debug1 == 1)
        {
            resultingColor *= 0.5;
        }
    }
    else if (renderType == 6) // ambient occlusion
    {
        resultingColor.rgb = texture(normalMap, texCoord).aaa;
        if (debug1 == 1)
            resultingColor.rgb = aoBlend.rgb;
    }

    else if (renderType == 7) // uv coords
		resultingColor.rgb = vec3(displayTexCoord, 1);
    else if (renderType == 8) // uv test pattern
        resultingColor.rgb = texture(UVTestPattern, displayTexCoord).rgb;
    else if (renderType == 9) // tangents
        resultingColor.rgb = displayTangent;
    else if (renderType == 10) // bitangents
        resultingColor.rgb = displayBitangent;
    else if (renderType == 11) // light set #
    {
        if (lightSet == 0)
            resultingColor.rgb = vec3(0,0,0);
        else if (lightSet == 1)
            resultingColor.rgb = vec3(0,1,1);
        else if (lightSet == 2)
            resultingColor.rgb = vec3(0,0,1);
        else if (lightSet == 3)
            resultingColor.rgb = vec3(1,1,0);
        else if (lightSet == 4)
            resultingColor.rgb = vec3(1,0,1);
        else if (lightSet == 5)
            resultingColor.rgb = vec3(0,1,0);
        else
            resultingColor.rgb = vec3(1);
    }
    else if (renderType == 12)
        resultingColor.rgb = boneWeightsColored;

    // Toggles rendering of individual color channels for all render modes.
    fragColor.rgb = resultingColor.rgb * vec3(renderR, renderG, renderB);
    if (renderR == 1 && renderG == 0 && renderB == 0)
        fragColor.rgb = resultingColor.rrr;
    else if (renderG == 1 && renderR == 0 && renderB == 0)
        fragColor.rgb = resultingColor.ggg;
    else if (renderB == 1 && renderR == 0 && renderG == 0)
        fragColor.rgb = resultingColor.bbb;

    if (renderAlpha == 1)
    {
        fragColor.a = resultingColor.a;
    }
    if (alphaOverride == 1)
        fragColor = vec4(resultingColor.aaa, 1);

    if (drawWireframe == 1)
    {
        float minDistance = min(min(edgeDistance.x, edgeDistance.y), edgeDistance.z);
        float smoothedDistance = exp2(-512.0 * minDistance * minDistance);
        vec3 edgeColor = vec3(1);
        fragColor.rgb = mix(fragColor.rgb, edgeColor, smoothedDistance);
    }
}
