#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec2 vUV0;
in vec4 vBone;
in vec4 vWeight;

out vec3 V;
out vec3 N;
out vec2 UV0;

uniform vec2 UV0Scale;
uniform vec3 BonePosition;
uniform mat4 mvpMatrix;
uniform mat4 bones[100];

void main() {

	N = vNormal;
	UV0 = vUV0 * UV0Scale;

	vec4 Pos = vec4(vPosition, 1.0);

	if(vBone.y == 0 && vBone.x != 0)
	{
		Pos = bones[int(vBone.x)] * Pos;
		N = (inverse(transpose(bones[int(vBone.x)])) * vec4(vNormal, 0)).xyz;
	}
	Pos.xyz += BonePosition;

	V = Pos.xyz;
	
    gl_Position = mvpMatrix * Pos;
}
