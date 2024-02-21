#version 330

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

// Basic directional light.
struct StageLight {
    int enabled;
    vec3 color;
    vec3 direction;
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


// Defined in Utility.frag.
float Luminance(vec3 rgb);
vec3 SrgbToLinear(vec3 color);

// Defined in Bayo.frag.
vec3 BayoHairDiffuse(vec3 diffuseMap, vec4 colorOffset, vec4 colorGain, vec4 alphaBlendParams);
vec3 BayoHairSpecular(vec3 diffuseMap, vec3 I, vec3 specLightDirection, vec4 reflectionParams, VertexAttributes vert);

vec3 TintColor(vec3 diffuseColor, float tintAmount) {
    // Approximates an overlay blend mode. Cheaper than converting to HSV/HSL.
    // Normalize the color to avoid color tint making the model darker.
    if (Luminance(diffuseColor) < 0.01)
        return vec3(1);
    vec3 colorTint = mix(vec3(1), (normalize(diffuseColor) * 2), min(tintAmount * 0.5, 1));
    return colorTint;
}

vec3 RampColor(float rampCoord, sampler2D ramp, int hasRamp) {
    // TODO: Vertical component is always 0?
	rampCoord = clamp(rampCoord, 0.01, 0.99);
	return pow(texture(ramp, vec2(1 - rampCoord)).rgb, vec3(2.2)) * hasRamp;
}

vec3 SphereMapColor(vec3 viewNormal, sampler2D spheremap) {
    // Calculate UVs based on view space normals.
    vec2 sphereTexcoord = vec2(viewNormal.x, (1 - viewNormal.y));
    return texture(spheremap, sphereTexcoord).rgb;
}

// probably not needed
vec3 ShiftTangent(vec3 tangent, vec3 normal, float shift)  {
    vec3 shiftedT = tangent + shift * normal;
    return normalize(shiftedT);
}

float Fresnel(vec3 I, vec3 N) {
    return max(1 - max(dot(I, N), 0), 0);
}

vec3 BumpMapNormal(sampler2D normalMap, VertexAttributes vert, vec4 dualNormalScrollParams,
    int hasDualNormal, vec4 normalParams) {
    // TODO: How does NU_dualNormalScrollParams work?
    vec3 bumpMapNormal = texture(normalMap, vert.normaltexCoord).xyz;
    vec3 bumpMapNormal2 = texture(normalMap, vec2(vert.normaltexCoord.x + dualNormalScrollParams.x, vert.normaltexCoord.y + dualNormalScrollParams.y)).xyz;
    if (hasDualNormal == 1)
        bumpMapNormal = normalize(bumpMapNormal + bumpMapNormal2);

    // Change normal map intensity.
    bumpMapNormal = mix(vec3(0.5, 0.5, 1), bumpMapNormal, normalParams.x);

    bumpMapNormal = 2.0 * bumpMapNormal - vec3(1);
    mat3 tbnMatrix = mat3(vert.tangent, vert.bitangent, vert.normal);
    vec3 newNormal = tbnMatrix * bumpMapNormal;
    return normalize(newNormal);
}

vec3 ColorOffsetGain(vec3 diffuseMap, int hasBayoHair,vec4 colorOffset, vec4 colorGain, vec4 alphaBlendParams) {
    if (hasBayoHair == 1)
        return BayoHairDiffuse(diffuseMap, colorOffset, colorGain, alphaBlendParams);

    // Offset the shadows first before adjusting the gain.
    vec3 resultingColor = vec3(Luminance(diffuseMap));
    resultingColor += colorOffset.rgb;
    resultingColor *= colorGain.rgb;
    return resultingColor;
}

vec3 FresnelPass(vec3 N, vec3 I, vec4 diffuseMap, float aoBlend, vec3 tintColor) {
    // hemisphere fresnel with fresnelParams exponent
    float hemiBlendSky = dot(N, fresSkyDirection) * 0.5 + 0.5;
    vec3 hemiColorSky = mix(vec3(0), fresSkyColor, hemiBlendSky);

    float hemiBlendGround = dot(N, fresGroundDirection) * 0.5 + 0.5;
    vec3 hemiColorGround = mix(vec3(0), fresGroundColor, hemiBlendGround);

    vec3 hemiColorTotal = hemiColorSky + hemiColorGround;

    // TODO: Find a more accurate value.
    float exponentOffset = 2.75;
    float fresnelExponent =  exponentOffset + fresnelParams.x;

    float fresnelTerm = clamp(pow(Fresnel(I, N), fresnelExponent), 0, 1);

    vec3 fresnelPass = hemiColorTotal * fresnelColor.rgb * fresnelTerm;
    fresnelPass *= aoBlend * fresnelIntensity * tintColor;
    return fresnelPass;
}

vec3 ReflectionPass(vec3 N, vec3 I, vec4 diffuseMap, float aoBlend, vec3 tintColor, VertexAttributes vert) {
    vec3 reflectionPass = vec3(0);
	// cubemap reflection
	vec3 R = reflect(I, N);
	R.y *= -1.0;
	vec3 stageCubeColor = texture(stagecube, R).rgb;
    vec3 cubeColor = texture(cube, R).rgb;

    // TODO: Cubemaps from model.nut currently just use the miiverse cubemap.
	if (hasCube == 1)
		reflectionPass += diffuseMap.aaa * cubeColor * tintColor * reflectionParams.x;

    // TODO: Stage cubemaps currently just use the miiverse cubemap.
    if (hasStage == 1)
       reflectionPass += reflectionColor.rgb * stageCubeColor.rgb * tintColor;

    vec3 sphereMapColor = SphereMapColor(vert.viewNormal, spheremap) * reflectionColor.xyz * tintColor;
	reflectionPass += sphereMapColor * hasSphereMap;

    // Very crude energy conservation approximation.
    reflectionPass -= 0.5 * Luminance(diffuseMap.rgb);
    reflectionPass = max(reflectionPass, vec3(0));

    reflectionPass *= aoBlend;
    reflectionPass *= refLightColor;
    reflectionPass *= reflectionIntensity;

    if (useDifRefMask == 1)
        reflectionPass *= diffuseMap.a;

    // TODO: Why is this gamma corrected.
    reflectionPass = pow(reflectionPass, vec3(2.2));
    return reflectionPass;
}

float AnisotropicSpecExponent(vec3 halfAngle, float width, float height, vec3 tangent, vec3 bitangent) {
    // Blinn-phong with some anistropic bits stolen from an anistropic GGX BRDF.
    vec3 X = normalize(tangent);
    vec3 Y = normalize(bitangent);
    float xComponent = max(pow((dot(halfAngle, X) / width), 2), 0);
    float yComponent = max(pow((dot(halfAngle, Y) / height), 2), 0);

    return xComponent + yComponent;
}

vec3 SpecularPass(vec3 N, vec3 I, vec4 diffuseMap, float aoBlend, vec3 tintColor, vec3 diffusePass, VertexAttributes vert) {
    vec3 specularPass = vec3(0);

    // Only uses the anisotropic exponent for mats without NU_specularParams.
    vec3 halfAngle = normalize(I + specLightDirection);
    float exponent = AnisotropicSpecExponent(halfAngle, reflectionParams.z, reflectionParams.w, vert.tangent, vert.bitangent);
    if (hasSpecularParams == 1)
        exponent = specularParams.y;

    float blinnPhongSpec = dot(halfAngle, N);
    blinnPhongSpec = pow(blinnPhongSpec, exponent);

    vec3 specColorTotal = specularColor.rgb * blinnPhongSpec;

    if (hasBayoHair == 1)
        specColorTotal = BayoHairSpecular(diffuseMap.rgb, I, specLightDirection, reflectionParams, vert);

    if (hasColorGainOffset == 1)
        specColorTotal *= specularColorGain.rgb;

    // TODO: Not sure how this works. Specular works differently for eye mats.
    // Check for eye mats and Mega Man/final smash eyes.
    if (((flags & 0x00E10000u) == 0x00E10000u) || ((flags & 0xFFFFFFFFu) == 0x92F01125u))
        specColorTotal *= diffuseMap.rgb;

    specularPass += specColorTotal;

    // Very crude energy conservation approximation.
    specularPass -= 0.5 * Luminance(diffusePass.rgb);
    specularPass = max(specularPass, vec3(0));

    specularPass *= aoBlend;
    specularPass *= tintColor;
    specularPass *= specLightColor;
    specularPass *= specularIntensity;

    return specularPass;
}

float AngleFade(vec3 N, vec3 I, vec4 angleFadeParams) {
    float fresnelBlend = Fresnel(I, N);
    float angleFadeAmount = mix(angleFadeParams.x, angleFadeParams.y, fresnelBlend);
    return max((1 - angleFadeAmount), 0);
}

vec3 UniverseColor(float universeParam, vec3 objectPosition, vec3 cameraPosition, mat4 modelViewMatrix, sampler2D dif) {
    // TODO: Doesn't work for all angles (viewing from below).
    vec3 projVec = normalize(objectPosition.xyz - cameraPosition.xyz);
    projVec = mat3(modelViewMatrix) * projVec;
    // Cheap approximations of trig functions.
    float uCoord = projVec.x * 0.5 + 0.5;
    float vCoord = projVec.y * 0.5 + 0.5;
    // Gamma correct again to make the texture dark enough.
    return pow(texture(dif, vec2(uCoord * universeParam, 1 - vCoord)).rgb, vec3(2.2));
}

vec3 CharacterDiffuseLighting(float halfLambert, vec3 ambLightColor, vec3 difLightColor) {
    vec3 rampContribution = difLightColor * RampColor(halfLambert, ramp, hasRamp);
    vec3 dummyRampContribution = difLightColor * RampColor(halfLambert, dummyRamp, hasDummyRamp);
    // TODO: How do ramps work?
    return mix(ambLightColor, difLightColor * (rampContribution + dummyRampContribution), halfLambert);
}

// Defined in StageLighting.frag.
vec3 StageDiffuseLighting(vec3 N, StageLight[4] lights);
vec3 FogColor(vec3 inputColor, vec4 fogParams, float depth, vec3 stageFogColor);

vec3 Lighting(vec3 N, float halfLambert) {
    // Flags for ignoring stage lighting. TODO: Replace bitwise operations.
    if (((flags & 0x0F000000u) < 0x04000000u) || renderStageLighting != 1)
        return vec3(1);

    if (isStage == 1) {
        // Stage Lighting
        StageLight light1 = StageLight(renderStageLight1, stageLight1Color, stageLight1Direction);
        StageLight light2 = StageLight(renderStageLight2, stageLight2Color, stageLight2Direction);
        StageLight light3 = StageLight(renderStageLight3, stageLight3Color, stageLight3Direction);
        StageLight light4 = StageLight(renderStageLight4, stageLight4Color, stageLight4Direction);
        StageLight[4] lights = StageLight[4](light1, light2, light3, light4);
        return StageDiffuseLighting(N, lights);
    } else {
        return CharacterDiffuseLighting(halfLambert, ambLightColor * ambientIntensity, difLightColor * diffuseIntensity);
    }

    return vec3(1);
}

vec3 SoftLighting(vec3 diffuse, vec4 params, float lambert) {
    float smoothLambert = clamp(lambert / params.z, 0, 1);
    float darkenMultiplier = 0.5; // TODO: some sort of uniform is used in game.
    // TODO: How is tint color done in the in game shaders?
    vec3 ambientTint = mix(vec3(Luminance(diffuse)), diffuse, params.x);
    float ambientGain = max(1 - (darkenMultiplier * params.y), 0);
    vec3 ambientColorFinal = ambLightColor * ambientGain * ambientTint;
    return CharacterDiffuseLighting(smoothLambert, ambientColorFinal * ambientIntensity, difLightColor * diffuseIntensity) * diffuse;
}

vec3 DiffuseAOBlend(float aoMap, vec4 aoMinGain) {
    // TODO: AO with no normal map?
    // Calculate the effect of NU_aoMinGain on the ambient occlusion map.
    if (hasNrm == 1)
        return clamp(aoMap + aoMinGain.rgb, 0, 1);
    else
        return clamp(1.0 + aoMinGain.rgb, 0, 1);
}

float AmbientOcclusionBlend(vec4 diffuseMap, float aoMap, vec4 aoMinGain, VertexAttributes vert) {
    // The diffuse map for colorGain/Offset materials does a lot of things.
    if (hasColorGainOffset == 1 || useDiffuseBlend == 1)
        aoMap = Luminance(pow(diffuseMap.rgb, vec3(1 / 2.2)));

    float aoMixIntensity = aoMinGain.w;
    if (useDiffuseBlend == 1) // aomingain but no ao map (mainly for trophies)
        aoMixIntensity = 0;

    return clamp(aoMap + aoMixIntensity, 0, 1);
}

vec3 DiffusePass(vec3 N, vec4 diffuseMap, float aoMap, VertexAttributes vert) {
    if (hasUniverseParam == 1)
        return UniverseColor(effUniverseParam.x, vert.objectPosition, cameraPosition, modelViewMatrix, dif);

    vec3 diffusePass = vec3(0);
    
    vec3 diffuseColorFinal = vec3(0); // result of diffuse map, aoBlend, and some NU_values
    if (hasColorGainOffset == 1) {
        diffuseColorFinal = ColorOffsetGain(diffuseMap.rgb, hasBayoHair, colorOffset, colorGain, alphaBlendParams);
    } else {
        vec3 aoBlend = DiffuseAOBlend(aoMap, aoMinGain);
        diffuseColorFinal = diffuseMap.rgb * aoBlend * diffuseColor.rgb;
    }

    // Diffuse uses a half lambert for softer lighting.
    float lambert = max(dot(difLightDirection, N), 0.0);
    float halfLambert = dot(difLightDirection, N) * 0.5 + 0.5;
    if (hasSoftLight == 1)
        diffusePass = SoftLighting(diffuseColorFinal, softLightingParams, lambert);
    else if (hasCustomSoftLight == 1)
        diffusePass = SoftLighting(diffuseColorFinal, customSoftLightParams, lambert);
    else
        diffusePass = diffuseColorFinal * Lighting(N, halfLambert);

    // Flags used for brightening diffuse for softlightingparams.
    if (softLightBrighten == 1)
        diffusePass *= 1.5;

    return diffusePass;
}

vec2 DistortedUvCoords(VertexAttributes vert, sampler2D normalMap, vec4 normalParams, int useNormalMap) {
    // Similar effect to du dv but uses just the normal map.
    // TODO: does du dv affect other UV channels?
    float offsetIntensity = 0;
    if(useNormalMap == 1)
        offsetIntensity = normalParams.z;
    vec2 textureOffset = 1 - texture(normalMap, vert.normaltexCoord).xy;
    textureOffset = (textureOffset * 2) - 1; // Remap [0,1] to [-1,1]
    return vert.texCoord + (textureOffset * offsetIntensity);
}

vec4 DiffuseMapTotal(VertexAttributes vert) {
    vec4 diffuseMapTotal = vec4(0);

    // Some stage water materials have NU_diffuseColor but no diffuse map.
    if (hasDif != 1)
        return diffuseColor;

    vec2 offsetTexCoord = DistortedUvCoords(vert, normalMap, normalParams, useNormalMap);

    // Blends all of the different diffuse textures together.
    if (hasDif == 1) {
        vec4 diffuse1 = texture(dif, offsetTexCoord) * finalColorGain.rgba;
        diffuseMapTotal = diffuse1;

        // TODO: 2nd diffuse texture. doesnt work properly with stages.
        if (hasDif2 == 1 && hasDif3 != 1) {
            vec4 diffuse2 = texture(dif2, vert.texCoord2) * finalColorGain2.rgba;
            diffuseMapTotal = mix(diffuse2, diffuse1, diffuse1.a);
            diffuseMapTotal.a = 1.0;

            if (hasDif3 == 1) {
                vec4 diffuse3 = texture(dif3, vert.texCoord3) * finalColorGain3.rgba;
                diffuseMapTotal = mix(diffuse3, diffuseMapTotal, diffuse3.a);
            }
        }
    }

    // TODO: Create uniform for flags comparison.
    if (hasAo == 1) {
        vec3 difAO = texture(ao, vert.texCoord2).rgb;
        float normalBlend = vert.normal.y * 0.5 + 0.5;

        if ((flags & 0x00410000u) == 0x00410000u)
            diffuseMapTotal.rgb = mix(difAO, diffuseMapTotal.rgb, normalBlend);
        else
            diffuseMapTotal.rgb *= difAO.rgb;
    }

    // Diffuse is sRGB.
    return pow(diffuseMapTotal, vec4(vec3(2.2), 1));
}

vec3 RenderPasses(vec4 diffuseMap, vec3 N, vec3 I, VertexAttributes vert) {
    float aoMap = pow(texture(normalMap, vert.texCoord).a, 2.2);
    // Not all materials have an ambient occlusion map.
    if (hasNrm != 1)
        aoMap = 1;

    // Separate render pass calculations.
    vec3 diffusePass = DiffusePass(N, diffuseMap, aoMap, vert);

    // Use total diffuse pass instead of just diffuse map color for tint.
    vec3 specTintColor = TintColor(diffusePass, specularColor.a);
    vec3 fresTintColor = TintColor(diffusePass, fresnelColor.a);
    vec3 reflTintColor = TintColor(diffusePass, reflectionColor.a);

    // The ambient occlusion calculations for diffuse are done separately.
    float ambientOcclusionBlend = AmbientOcclusionBlend(diffuseMap, aoMap, aoMinGain, vert);
    vec3 specularPass = SpecularPass(N, I, diffuseMap, ambientOcclusionBlend, specTintColor, diffusePass, vert);
    vec3 fresnelPass = FresnelPass(N, I, diffuseMap, ambientOcclusionBlend, fresTintColor);
	vec3 reflectionPass = ReflectionPass(N, I, diffuseMap, ambientOcclusionBlend, reflTintColor, vert);

	vec3 resultingColor = vec3(0);

	if(renderLighting == 1) {
        // Prevent negative colors for some GPUs.
    	resultingColor += max((diffusePass * renderDiffuse), 0);
    	resultingColor += max((fresnelPass * renderFresnel), 0);
    	resultingColor += max((specularPass * renderSpecular), 0);
    	resultingColor += max((reflectionPass * renderReflection), 0);

        // light_set_param.bin fog
        if (renderFog == 1 && renderStageLighting == 1)
            resultingColor = FogColor(resultingColor, fogParams, vert.viewPosition.z, stageFogColor);
	} else {
        resultingColor = diffusePass;

    }

    resultingColor = pow(resultingColor, vec3(1 / 2.2));
    return resultingColor;
}

vec4 SmashShader(VertexAttributes vert)
{
    vec4 resultingColor = vec4(0,0,0,1);

    // Transform view vector to world space.
    vec3 I = vec3(0,0,-1) * mat3(mvpMatrix);

    vec3 N = vert.normal;
    if (hasNrm == 1 && useNormalMap == 1)
        N = BumpMapNormal(normalMap, vert, dualNormalScrollParams, hasDualNormal, normalParams);

    // zOffset correction
    // TODO: Divide by far plane?
    gl_FragDepth = gl_FragCoord.z - (zOffset.x / 1500) - zBufferOffset;

    // Calculate diffuse map blending.
    vec4 diffuseMapTotal = DiffuseMapTotal(vert);

    // TODO: Research how mii colors work.
    diffuseMapTotal *= characterColor.rgba;
    // Material lighting done in SmashShader
    resultingColor.rgb = RenderPasses(diffuseMapTotal, N, I, vert);

    // TODO: Max vertex color value?
    if (renderVertColor == 1 || hasFinalColorGain == 1)
        resultingColor *= min(vert.vertexColor, vec4(1));

    // Universe mats are weird...
    if (hasUniverseParam != 1)
        resultingColor.rgb *= effColorGain.rgb;

    // Adjust the alpha.
    // TODO: Is this affected by multiple diffuse maps?
    resultingColor.a *= texture(dif, vert.texCoord).a;
    resultingColor.a *= finalColorGain.a;
    resultingColor.a *= effColorGain.a;
    resultingColor.a *= AngleFade(N, I, angleFadeParams);
    resultingColor.a += alphaBlendParams.x; // TODO: Ryu works differently.

    // Alpha override for render settings. Fixes alpha for renders.
    if (renderAlpha != 1 || isTransparent != 1)
        resultingColor.a = 1;

    return resultingColor;
}
