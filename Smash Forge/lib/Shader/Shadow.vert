#version 330

in vec3 vPosition;
in vec4 vBone;
in vec4 vWeight;
in vec4 vBoneHash;

uniform mat4 lightMatrix;

uniform int useBones;

uniform bones {
    mat4 transforms[200];
} bones_;

vec4 skin(vec3 po, ivec4 index) {
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;

    return oPos;
}

vec3 skinNRM(vec3 nr, ivec4 index) {
    vec3 nrmPos = vec3(0);

    if(vWeight.x != 0.0) nrmPos = mat3(bones_.transforms[index.x]) * nr * vWeight.x;
    if(vWeight.y != 0.0) nrmPos += mat3(bones_.transforms[index.y]) * nr * vWeight.y;
    if(vWeight.z != 0.0) nrmPos += mat3(bones_.transforms[index.z]) * nr * vWeight.z;
    if(vWeight.w != 0.0) nrmPos += mat3(bones_.transforms[index.w]) * nr * vWeight.w;

    return nrmPos;
}

void main() {
    // Vertex Skinning
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    if (useBones == 1 && vBone.x != -1)
       objPos = skin(vPosition, ivec4(vBone));

    objPos = lightMatrix * vec4(objPos.xyz, 1.0);
    gl_Position = objPos;
}
