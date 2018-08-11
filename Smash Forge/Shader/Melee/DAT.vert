#version 330

in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec2 vUV;
in vec4 vBone;
in vec4 vWeight;

out vec2 texcoord;
out vec4 vertexColor;
out vec3 normal;
out vec3 boneWeightsColored;

uniform mat4 modelview;
uniform int selectedBoneIndex;

uniform bones {
    mat4 transforms[200];
} bones_;

float BoneWeightDisplay() {
    float weight = 0;
    if (selectedBoneIndex == vBone.x)
        weight += vWeight.x;
    if (selectedBoneIndex == vBone.y)
        weight += vWeight.y;
    if (selectedBoneIndex == vBone.z)
        weight += vWeight.z;
    if (selectedBoneIndex == vBone.w)
        weight += vWeight.w;

    return weight;
}

void
main() {
    /*
    // if (debug1 == 1)
    //     boneWeightsColored = BoneWeightRamp(vec3(boneWeightsColored)).rgb;
	*/

    vec4 objPos = vec4(vPosition.xyz, 1.0);
	ivec4 index = ivec4(vBone);
	if (vBone.x != -1) {
        objPos = bones_.transforms[index.x] * vec4(vPosition, 1.0) * vWeight.x;
        objPos += bones_.transforms[index.y] * vec4(vPosition, 1.0) * vWeight.y;
        objPos += bones_.transforms[index.z] * vec4(vPosition, 1.0) * vWeight.z;
        objPos += bones_.transforms[index.w] * vec4(vPosition, 1.0) * vWeight.w;
    }

    gl_Position = modelview * vec4(objPos.xyz, 1.0);
    texcoord = vUV;
    vertexColor = vColor;
    normal = vNormal;
    float totalWeight = BoneWeightDisplay();
    boneWeightsColored = vec3(totalWeight);
}
