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
a
out vec3 normal;
out vec3 viewNormal;
out vec3 tangent;
out vec3 bitangent;
out vec3 fragpos;
out vec3 viewPosition;
out vec3 objectPosition;

out vec4 vertexColor;

out vec2 texCoord;
out vec2 texCoord2;
out vec2 texCoord3;
out vec2 normaltexCoord;

uniform vec4 colorSamplerUV;
uniform vec4 colorSampler2UV;
uniform vec4 colorSampler3UV;
uniform vec4 normalSamplerAUV;
uniform vec4 normalSamplerBUV;

uniform mat4 mvpMatrix;
uniform mat4 nscMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 sphereMapMatrix;

uniform uint flags;
uniform float zScale;
uniform vec4 zOffset;

uniform float elapsedTime;
uniform int useDirectUVTime;
uniform int selectedBoneIndex;

uniform int hasNrmSamplerAUV;
uniform int hasNrmSamplerBUV;

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
    if(vBone.x != -1.0)
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

    texCoord = vec2(sampler1.xy * (vUV + sampler1.zw));
    texCoord2 = vec2(sampler2.xy * (vUV2 + sampler2.zw));
    texCoord3 = vec2(sampler3.xy * (vUV3 + sampler3.zw));
    normaltexCoord = vec2(nrmSampler.xy * (vUV + nrmSampler.zw));

    // Vertex attributes for fragment shader.
    vertexColor = vColor;
	fragpos = objPos.xyz;
    objectPosition = vPosition.xyz;
    tangent.xyz = vTangent.xyz;
    bitangent.xyz = vBiTangent.xyz;
    viewPosition = vec3(vPosition * mat3(mvpMatrix));

    normal = vNormal;
	if(vBone.x != -1.0)
		normal = normalize((skinNRM(vNormal.xyz, ivec4(vBone))).xyz);

    viewNormal = mat3(sphereMapMatrix) * normal.xyz;
    viewNormal = viewNormal * 0.5 + 0.5;
}
