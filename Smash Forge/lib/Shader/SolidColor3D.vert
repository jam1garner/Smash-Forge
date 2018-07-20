#version 330

in vec3 position;

uniform vec3 center;
uniform vec3 scale;

uniform mat4 mvpMatrix;

void main() {
    vec3 finalPosition = (position + center) * scale;
    gl_Position = mvpMatrix * vec4(finalPosition, 1);
}
