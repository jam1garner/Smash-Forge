#version 330

in vec3 position;

out vec2 texCoord;

// Convert the positions of a triangle to UV coordinates.
// The triangle extends past the borders of the screen.
//    -1.0, -1.0, 0.0,
//     3.0, -1.0, 0.0,
//    -1.0,  3.0, 0.0
void main() {
    texCoord.xy = (position.xy + vec2(1.0)) * 0.5;
    gl_Position = vec4(position, 1);
}
