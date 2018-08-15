#version 330

in vec3 vPosition;
in vec2 vUV;
in vec2 vUV2;
in vec2 vUV3;

uniform mat4 mvpMatrix;

void main()
{
    gl_Position = mvpMatrix * vec4(vUV, 1.0, 1.0);
}
