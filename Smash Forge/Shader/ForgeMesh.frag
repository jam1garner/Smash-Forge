#version 330

in vec3 normal;

uniform vec4 color;

out vec4 fragColor;

void main() {
    fragColor = vec4(normal, 1);
}
