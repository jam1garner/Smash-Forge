#version 330
in vec2 texCoord;

uniform vec3 topColor;
uniform vec3 bottomColor;

out vec4 FragColor;

void main() {
	FragColor = vec4(1);
	FragColor.rgb = mix(bottomColor, topColor, texCoord.y);
}
