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

uniform mat4 mvpMatrix;

void main()
{
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    gl_Position = mvpMatrix * vec4(objPos.xyz, 1.0);
}
