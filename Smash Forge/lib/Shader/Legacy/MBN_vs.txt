#version 120

attribute vec3 pos;
attribute vec4 col;
attribute vec3 nrm;
attribute vec2 tx0;
attribute vec2 bone;
attribute vec2 weight;

varying vec2 texcoord;
varying vec4 vertexColor;
varying vec3 normal;
varying vec4 position;
varying vec3 boneWeightsColored;

uniform mat4 modelview;
uniform int selectedBoneIndex;
uniform int boneList[100];

uniform bones
{
    mat4 transforms[100];
} bones_;

float BoneWeightDisplay()
{
    float w = 0;
    if (selectedBoneIndex == bone.x)
        w += weight.x;
    if (selectedBoneIndex == bone.y)
        w += weight.y;

    return w;
}

void main()
{
    //float totalWeight = BoneWeightDisplay();
    boneWeightsColored = vec3(0);
    // if (debug1 == 1)
    //     boneWeightsColored = BoneWeightRamp(vec3(boneWeightsColored)).rgb;

    //ivec4 index = ivec4(vBone);
    vec4 objPos = vec4(pos.xyz, 1.0);
    ivec2 index = ivec2(bone);

    position = modelview * vec4(objPos.xyz, 1.0);
    texcoord = vec2(tx0.x, 1-tx0.y);
    vertexColor = vec4(1);
    normal.xyz = nrm.xyz;

    if (bone.x != -1.0)
    {
        objPos = bones_.transforms[boneList[index.x]] * vec4(pos, 1.0) * weight.x;
        objPos += bones_.transforms[boneList[index.y]] * vec4(pos, 1.0) * weight.y;
    }

    gl_Position = modelview * vec4(objPos.xyz, 1.0);
}
