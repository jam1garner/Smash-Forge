#version 330

in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBiTangent;
in vec2 vUV;
in vec2 vUV2;
in vec2 vUV3;
in vec4 vBone;
in vec4 vWeight;
in vec4 vBoneHash;

out vec3 normal;
out vec3 tangent;
out vec3 bitangent;
out vec3 fragpos;
out vec3 viewPosition;
out vec3 objectPosition;

out vec4 vertexColor;

out vec2 texCoord;

out vec3 boneWeightsColored;

// uniform vec4 colorSamplerUV;
// uniform vec4 colorSampler2UV;
// uniform vec4 colorSampler3UV;
// uniform vec4 normalSamplerAUV;
// uniform vec4 normalSamplerBUV;
uniform vec4 effScaleUV;
uniform vec4 effTransUV;
uniform vec4 effMaxUV;

uniform mat4 mvpMatrix;
uniform uint flags;
uniform vec3 NSC;
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


uniform bones
{
    mat4 transforms[200];
} bones_;

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

vec4 skin(vec3 po, ivec4 index)
{
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;

    return oPos;
}

vec3 skinNRM(vec3 nr, ivec4 index)
{
    vec3 nrmPos = vec3(0);

    if(vWeight.x != 0.0) nrmPos = mat3(bones_.transforms[index.x]) * nr * vWeight.x;
    if(vWeight.y != 0.0) nrmPos += mat3(bones_.transforms[index.y]) * nr * vWeight.y;
    if(vWeight.z != 0.0) nrmPos += mat3(bones_.transforms[index.z]) * nr * vWeight.z;
    if(vWeight.w != 0.0) nrmPos += mat3(bones_.transforms[index.w]) * nr * vWeight.w;

    return nrmPos;
}

vec3 BoneWeightRamp(vec3 weights)
{
	float rampInputLuminance = weights.x;
	rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
	return texture(boneWeight, vec2(1 - rampInputLuminance, 0.50)).rgb;
}

void main()
{
    // vertex skinning
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    if(vBone.x != -1.0)
       objPos = skin(vPosition, ivec4(vBone));

    //objPos.xyz += NSC;
    objPos.z *= zScale;

    objPos = mvpMatrix * vec4(objPos.xyz, 1.0);
    gl_Position = objPos;

    vec4 sampler1 = vec4(effScaleUV.xy, effTransUV.xy);


    texCoord = vec2((vUV * sampler1.xy) + (sampler1.zw * elapsedTime));


    tangent.xyz = vTangent.xyz;
    bitangent.xyz = vBiTangent.xyz;
    viewPosition = vec3(vPosition * mat3(mvpMatrix));

    // fragment shader attributes
    float totalWeight = BoneWeightDisplay();
    boneWeightsColored = vec3(totalWeight);
    if (debug1 == 1)
        boneWeightsColored = BoneWeightRamp(vec3(boneWeightsColored)).rgb;

    vertexColor = vColor;
	fragpos = objPos.xyz;
    objectPosition = vPosition.xyz;

    normal = vNormal;
	if(vBone.x != -1.0)
		normal = normalize((skinNRM(vNormal.xyz, ivec4(vBone))).xyz);
}
