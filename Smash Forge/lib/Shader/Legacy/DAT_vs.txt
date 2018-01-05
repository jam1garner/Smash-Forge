#version 120

attribute vec3 vPosition;
attribute vec4 vColor;
attribute vec3 vNormal;
attribute vec2 vUV;
attribute vec4 vBone;
attribute vec4 vWeight;

varying vec2 texcoord;
varying vec4 vertexColor;
varying vec3 normal;
varying vec3 boneWeightsColored;

uniform mat4 modelview;
uniform int selectedBoneIndex;

float BoneWeightDisplay()
{
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

void main()
{
    ivec4 index = ivec4(vBone);
    vec4 objPos = vec4(vPosition.xyz, 1.0);

    float totalWeight = BoneWeightDisplay();
    boneWeightsColored = vec3(totalWeight);
    // if (debug1 == 1)
    //     boneWeightsColored = BoneWeightRamp(vec3(boneWeightsColored)).rgb;

    gl_Position = modelview * vec4(objPos.xyz, 1.0);
    texcoord = vUV;
    vertexColor = vColor;
    normal = vNormal;
}
