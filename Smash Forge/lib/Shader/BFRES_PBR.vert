#version 330

//------------------------------------------------------------------------------------
//
//      Viewport Camera/Lighting
//
//------------------------------------------------------------------------------------

uniform mat4 modelview;

//------------------------------------------------------------------------------------
//
//      Verex Attributes
//
//------------------------------------------------------------------------------------

in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec2 vUV0;
in vec2 vUV1;
in vec2 vUV2;
in vec4 vBone;
in vec4 vWeight;
in vec3 vTangent;
in vec3 vBitangent;

out vec2 f_texcoord0;
out vec2 f_texcoord1;
out vec2 f_texcoord2;
out vec2 f_texcoord3;
out vec3 normal;
out vec4 color;
out vec3 tangent;
out vec3 bitangent;
out vec3 FragPos;

out vec3 boneWeightsColored;

//------------------------------------------------------------------------------------
//
//      Shader Options
//
//------------------------------------------------------------------------------------

uniform vec4 gsys_bake_st0;
uniform vec4 gsys_bake_st1;

//------------------------------------------------------------------------------------
//
//      Skinning uniforms
//
//------------------------------------------------------------------------------------

uniform mat4 bones[100];
uniform int boneList[100];
uniform int NoSkinning;
uniform int RigidSkinning;
uniform int SingleBoneIndex;

uniform int selectedBoneIndex;

uniform sampler2D weightRamp1;
uniform sampler2D weightRamp2;

//------------------------------------------------------------------------------------
//
//      SRT Animation uniforms
//
//------------------------------------------------------------------------------------

uniform vec2 SRT_Scale;
uniform vec2 SRT_Translate;
uniform float SRT_Rotate;

uniform int debugOption;


vec3 skinNRM(vec3 nr, ivec4 index)
{
    vec3 nrmPos = vec3(0);

    if(vWeight.x != 0.0) nrmPos = mat3(bones[boneList[index.x]]) * nr * vWeight.x;
    if(vWeight.y != 0.0) nrmPos += mat3(bones[boneList[index.y]]) * nr * vWeight.y;
    if(vWeight.z != 0.0) nrmPos += mat3(bones[boneList[index.z]]) * nr * vWeight.z;
    if(vWeight.w < 1) nrmPos += mat3(bones[boneList[index.w]]) * nr * vWeight.w;

    return nrmPos;
}

vec2 rotateUV(vec2 uv, float rotation)
{
    float mid = 0.5;
    return vec2(
        cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
        cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

float BoneWeightDisplay(ivec4 index)
{
    int X = int(boneList[index.x]);
    int Y = int(boneList[index.y]);
    int Z = int(boneList[index.z]);
    int W = int(boneList[index.w]);

    float weight = 0;
    if (selectedBoneIndex == X)
        weight += vWeight.x;
    if (selectedBoneIndex == Y)
        weight += vWeight.y;
    if (selectedBoneIndex == Z)
        weight += vWeight.z;
    if (selectedBoneIndex == W)
        weight += vWeight.w;

		//For meshes that only use the skin index. Usually it defaults to root, but some are specific.
        if (selectedBoneIndex == SingleBoneIndex)
		    weight += 1;

    return weight;
}

vec3 BoneWeightColor(float weights)
{
	float rampInputLuminance = weights;
	rampInputLuminance = clamp((rampInputLuminance), 0.001, 0.999);
    if (debugOption == 1)
        return vec3(weights);
    else if (debugOption == 2)
	   return texture(weightRamp1, vec2(1 - rampInputLuminance, 0.50)).rgb;
    else
        return texture(weightRamp2, vec2(1 - rampInputLuminance, 0.50)).rgb;
}

void main()
{
    ivec4 index = ivec4(vBone);

    vec4 objPos = vec4(vPosition.xyz, 1.0);

    vec4 sampler2 = gsys_bake_st0;
    vec4 sampler3 = gsys_bake_st1;

    normal = vNormal;


	     if(vBone.x != -1){
         objPos = bones[boneList[index.x]] * vec4(vPosition, 1.0) * vWeight.x;
         objPos += bones[boneList[index.y]] * vec4(vPosition, 1.0) * vWeight.y;
         objPos += bones[boneList[index.z]] * vec4(vPosition, 1.0) * vWeight.z;
         if(vWeight.w < 1)
             objPos += bones[boneList[index.w]] * vec4(vPosition, 1.0) * vWeight.w;
     }


    gl_Position = modelview * vec4(objPos.xyz, 1.0);

    vec3 distance = (objPos.xyz + vec3(5, 5, 5))/2;

	 if(vBone.x != -1.0)
	 	normal = normalize((skinNRM(vNormal.xyz, ivec4(vBone))).xyz);
    
    
      if (RigidSkinning == 1){
	      gl_Position = modelview * bones[boneList[index.x]] * vec4(vPosition, 1.0);
          normal = vNormal;
	 	 normal = mat3(bones[boneList[index.x]]) * vNormal.xyz * 1;
	 }
	 if (NoSkinning == 1){
	     gl_Position = modelview * bones[SingleBoneIndex] * vec4(vPosition, 1.0);
	     normal = mat3(bones[SingleBoneIndex]) * vNormal.xyz * 1;
	 	normal = normalize(normal);
	 }

    FragPos = objPos.xyz;

    //gl_TexCoord[0] = vUV0;
    //gl_TexCoord[1] = vUV1;

	if (sampler2.x + sampler2.y != 0) //BOTW has scale values to 0 if unused so set them to 1
        f_texcoord1 = vec2((vUV1 * sampler2.xy) + sampler2.zw);
     else
        f_texcoord1 = vec2((vUV1 * vec2(1)) + sampler2.zw);

    f_texcoord2 = vec2((vUV1 * sampler3.xy) + sampler3.zw);
    f_texcoord3 = vUV2;

	f_texcoord0 = vec2(vUV0);

	//Set SRT values43
	if (SRT_Scale.x + SRT_Scale.y != 0)
	    f_texcoord0 = vec2((vUV0 * SRT_Scale) + SRT_Translate);


	f_texcoord0 = rotateUV(f_texcoord0, SRT_Rotate);

	tangent = vTangent;
	bitangent = vBitangent;

	 // fragment shader attributes
    float totalWeight = BoneWeightDisplay(ivec4(vBone));
    boneWeightsColored = BoneWeightColor(totalWeight).rgb;


    color = vColor;
}
