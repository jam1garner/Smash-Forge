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

// Outputs for geometry shader.
out vec3 geomNormal;
out vec3 geomViewNormal;
out vec3 geomTangent;
out vec3 geomBitangent;
out vec4 geomFragPos;
out vec4 geomFragPosLightSpace;
out vec3 geomViewPosition;
out vec3 geomObjectPosition;

out vec4 geomVertexColor;

out vec2 geomTexCoord;
out vec2 geomTexCoord2;
out vec2 geomTexCoord3;
out vec2 geomNormaltexCoord;

uniform vec4 colorSamplerUV;
uniform vec4 colorSampler2UV;
uniform vec4 colorSampler3UV;
uniform vec4 normalSamplerAUV;
uniform vec4 normalSamplerBUV;

uniform mat4 mvpMatrix;
uniform mat4 nscMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 sphereMapMatrix;
uniform mat4 lightMatrix;

uniform uint flags;
uniform float zScale;
uniform vec4 zOffset;

uniform float elapsedTime;
uniform int useDirectUVTime;
uniform int selectedBoneIndex;

uniform int hasNrmSamplerAUV;
uniform int hasNrmSamplerBUV;

uniform int useBones;

uniform bones
{
    mat4 transforms[200];
} bones_;


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

void main()
{
    // Vertex Skinning
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    if (useBones == 1 && vBone.x != -1)
       objPos = skin(vPosition, ivec4(vBone));

    objPos.z *= zScale;
    objPos = nscMatrix * objPos;
    objPos = mvpMatrix * vec4(objPos.xyz, 1.0);
    gl_Position = objPos;

    // Texture samplers.
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
        // UV translation animation.
        sampler1.zw *= elapsedTime;
        sampler2.zw *= elapsedTime;
        sampler3.zw *= elapsedTime;
        nrmSampler.zw *= elapsedTime;
    }

    // Vertex attributes for geometry shader.
    geomTexCoord = vec2(sampler1.xy * (vUV + sampler1.zw));
    geomTexCoord2 = vec2(sampler2.xy * (vUV2 + sampler2.zw));
    geomTexCoord3 = vec2(sampler3.xy * (vUV3 + sampler3.zw));
    geomNormaltexCoord = vec2(nrmSampler.xy * (vUV + nrmSampler.zw));

    geomVertexColor = vColor;
	geomFragPos = vec4(objPos.xyz, 1.0);
    geomFragPosLightSpace = lightMatrix * vec4(vPosition.xyz, 1.0);
    geomObjectPosition = vPosition.xyz;
    geomTangent.xyz = vTangent.xyz;
    geomBitangent.xyz = vBiTangent.xyz;
    geomViewPosition = vec3(vPosition * mat3(mvpMatrix));

    geomNormal = vNormal;
    if (useBones == 1 && vBone.x != -1)
		geomNormal = normalize((skinNRM(vNormal.xyz, ivec4(vBone))).xyz);

    geomViewNormal = mat3(sphereMapMatrix) * geomNormal.xyz;
    geomViewNormal = geomViewNormal * 0.5 + 0.5;
}
