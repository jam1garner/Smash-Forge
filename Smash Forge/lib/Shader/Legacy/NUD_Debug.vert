#version 120
attribute vec3 vPosition;
attribute vec4 vColor;
attribute vec3 vNormal;
attribute vec3 vTangent;
attribute vec3 vBiTangent;
attribute vec2 vUV;
attribute vec2 vUV2;
attribute vec2 vUV3;
attribute vec4 vBone;
attribute vec4 vWeight;
attribute vec4 vBoneHash;

varying vec3 viewPosition;
varying vec3 objectPosition;
varying vec2 texCoord;
varying vec2 texCoord2;
varying vec2 texCoord3;
varying vec2 normaltexCoord;
varying vec4 vertexColor;
varying vec3 normal;
varying vec3 viewNormal;
varying vec3 tangent;
varying vec3 bitangent;
varying vec3 boneWeightsColored;

uniform vec4 colorSamplerUV;
uniform vec4 colorSampler2UV;
uniform vec4 colorSampler3UV;
uniform vec4 normalSamplerAUV;
uniform vec4 normalSamplerBUV;

uniform mat4 mvpMatrix;
uniform mat4 nscMatrix;
uniform mat4 sphereMapMatrix;

// uniform uint flags;
uniform float zScale;
uniform vec4 zOffset;

uniform float elapsedTime;
uniform int useDirectUVTime;
uniform int selectedBoneIndex;

uniform int hasNrmSamplerAUV;
uniform int hasNrmSamplerBUV;

uniform sampler2D boneWeight;
uniform int debug1;
uniform int debug2;

uniform int renderType;

float BoneWeightDisplay()
{
    float weight = 0;
    if (selectedBoneIndex == vBone.x)
        weight += vWeight.x;
    if (selectedBoneIndex == vBone.y)
        weight += vWeight.y;
    if (selectedBoneIndex == vBone.z)
        weight += vWeight.z;
    if (selectedBoneIndex == vBone.w)
        weight += vWeight.w;

    return weight;
}

vec3 BoneWeightRamp(vec3 weights)
{
	float rampInputLuminance = weights.x;
	rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
	return texture2D(boneWeight, vec2(1 - rampInputLuminance, 0.50)).rgb;
}

void main()
{
    // vertex skinning
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    // if(vBone.x != -1.0)
    //    objPos = skin(vPosition, ivec4(vBone));

    objPos.z *= zScale;
    objPos = nscMatrix * objPos;
    objPos = mvpMatrix * vec4(objPos.xyz, 1.0);
    gl_Position = objPos;

    vec4 sampler1 = colorSamplerUV;
    vec4 sampler2 = colorSampler2UV;
    if (hasNrmSamplerBUV == 1)
        sampler2 = normalSamplerBUV;
    vec4 sampler3 = colorSampler3UV;
    vec4 nrmSampler = colorSamplerUV;
    if (hasNrmSamplerAUV == 1)
        nrmSampler = normalSamplerAUV;

    if (useDirectUVTime == 1)
    {
        texCoord = vec2((vUV * sampler1.xy) + (sampler1.zw * elapsedTime));
        texCoord2 = vec2((vUV2 * sampler2.xy) + (sampler2.zw * elapsedTime));
        texCoord3 = vec2((vUV3 * sampler3.xy) + (sampler3.zw * elapsedTime));
        normaltexCoord = vec2((vUV * nrmSampler.xy) + (nrmSampler.zw * elapsedTime));
    }
    else
    {
        texCoord = vec2((vUV * sampler1.xy) + sampler1.zw);
        texCoord2 = vec2((vUV2 * sampler2.xy) + (sampler2.zw));
        texCoord3 = vec2((vUV3 * sampler3.xy) + (sampler3.zw));
        normaltexCoord = vec2((vUV * nrmSampler.xy) + nrmSampler.zw);
    }

    tangent.xyz = vTangent.xyz;
    bitangent.xyz = vBiTangent.xyz;
    viewPosition = vec3(vPosition * mat3(mvpMatrix));

    // fragment shader attributes
    float totalWeight = BoneWeightDisplay();
    boneWeightsColored = vec3(totalWeight);
    if (debug1 == 1)
        boneWeightsColored = BoneWeightRamp(vec3(boneWeightsColored)).rgb;

    vertexColor = vColor;
    objectPosition = vPosition.xyz;

    normal = vNormal;
	// if(vBone.x != -1.0)
	// 	normal = normalize((skinNRM(vNormal.xyz, ivec4(vBone))).xyz);

    viewNormal = mat3(sphereMapMatrix) * normal.xyz;
    viewNormal = viewNormal * 0.5 + 0.5;
}
