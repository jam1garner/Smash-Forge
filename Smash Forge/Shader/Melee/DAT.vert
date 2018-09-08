#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec3 vBitan;
in vec3 vTan;
in vec2 vUV0;
in vec4 vBone;
in vec4 vWeight;

out vec3 objectPosition;
out vec3 normal;
out vec3 bitan;
out vec3 tan;
out vec4 color;
out vec2 UV0;

uniform vec3 BonePosition;
uniform mat4 mvpMatrix;
uniform mat4 bones[100];
uniform mat4 binds[100];

vec4 Skin(vec4 P, ivec4 B)
{
	vec4 o = vec4(0, 0, 0, 1);
	if(B.x != 0) o += binds[B.x] * vec4(P.xyz, 1) * vWeight.x; else o = P;
	if(B.y != 0) o += binds[B.y] * vec4(P.xyz, 1) * vWeight.y;
	if(B.z != 0) o += binds[B.z] * vec4(P.xyz, 1) * vWeight.z;
	if(B.w != 0) o += binds[B.w] * vec4(P.xyz, 1) * vWeight.w;
	return o;
}

void main() {

	normal = vNormal;
	bitan = normalize(vBitan);
	tan = normalize(vTan);
	UV0 = vUV0;

	vec4 position = vec4(vPosition, 1.0);

	if(vBone.y == 0 && vBone.x != 0)
	{
		position = bones[int(vBone.x)] * position;
		normal = (inverse(transpose(bones[int(vBone.x)])) * vec4(vNormal, 0)).xyz;
	}
	position.xyz += BonePosition;

	position = Skin(position, ivec4(vBone));

	objectPosition = position.xyz;

    gl_Position = mvpMatrix * vec4(position.xyz, 1);
}
