#version 330

in vec3 vPosition;

uniform mat4 mvpMatrix;

void main() {
    gl_Position = mvpMatrix * vec4(vPosition, 1.0);
}
