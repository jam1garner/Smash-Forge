#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec3 vBitan;
in vec3 vTan;
in vec4 vColor;
in vec2 vUV0;
in vec4 vBone;
in vec4 vWeight;

out vec3 objectPosition;
out vec3 normal;
out vec3 bitangent;
out vec3 tangent;
out vec4 color;
out vec2 UV0;

uniform int BoneIndex;
uniform mat4 mvpMatrix;

uniform Bones
{
    mat4 transforms[200];
    mat4 binds[200];
};

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
	bitangent = normalize(vBitan);
	tangent = normalize(vTan);
	UV0 = vUV0;
	color = vColor;

	vec4 position = vec4(vPosition, 1.0);

	if(vBone.y == 0 && vBone.x != 0)
	{
		position = transforms[int(vBone.x)] * position;
		normal = (inverse(transpose(transforms[int(vBone.x)])) * vec4(vNormal, 0)).xyz;
	}
	position.xyz = (transforms[int(BoneIndex)] * vec4(position.xyz, 1.0)).xyz;

	position = Skin(position, ivec4(vBone));

	objectPosition = position.xyz;

    gl_Position = mvpMatrix * vec4(position.xyz, 1);
}
