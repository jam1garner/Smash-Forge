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

out vec3 normal;

uniform mat4 mvpMatrix;

void main() {
    normal = vNormal;
    gl_Position = mvpMatrix * vec4(vPosition, 1);
}
