#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec2 vUV0;
in vec4 vBone;
in vec4 vWeight;

out vec3 objectPosition;
out vec3 normal;
out vec2 UV0;

uniform vec2 UV0Scale;
uniform vec3 BonePosition;
uniform mat4 mvpMatrix;
uniform mat4 bones[100];

void main() {

	normal = vNormal;
	UV0 = vUV0 * UV0Scale;

	vec4 position = vec4(vPosition, 1.0);

	if(vBone.y == 0 && vBone.x != 0)
	{
		position = bones[int(vBone.x)] * position;
		normal = (inverse(transpose(bones[int(vBone.x)])) * vec4(vNormal, 0)).xyz;
	}
	position.xyz += BonePosition;

	objectPosition = position.xyz;

    gl_Position = mvpMatrix * position;
}
