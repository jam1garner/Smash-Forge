#version 330

in vec3 position;
out vec2 texCoord;

void main() {
    texCoord.xy = (position.xy + vec2(1.0)) * 0.5;
    gl_Position = vec4(position, 1);
}
