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
out vec3 tangent;
out vec3 bitangent;
out vec4 vertexColor;
out vec2 uv1;
out vec2 uv2;
out vec2 uv3;

uniform mat4 mvpMatrix;

void main() {
    uv1 = vUV;
    uv2 = vUV2;
    uv3 = vUV3;
    tangent = vTangent;
    bitangent = vBiTangent;
    normal = vNormal;
    vertexColor = vColor;

    gl_Position = mvpMatrix * vec4(vPosition, 1);
}
