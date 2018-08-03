#version 330

in vec3 vPosition;
in vec4 vBone;
in vec4 vWeight;
in int vSelected;

flat out vec4 color;

uniform mat4 mvpMatrix;

uniform int selected[];
uniform vec3 col1;
uniform vec3 col2;

uniform bones
{
    mat4 transforms[200];
} bones_;

vec4 skin(vec3 po, ivec4 index)
{
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;

    return oPos;
}
void main()
{
    // vertex skinning
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    if(vBone.x != -1.0)
       objPos = skin(vPosition, ivec4(vBone));

	color = vec4(col1,0.5);
	if (vSelected == 1)
		color = vec4(col2, 1);

    //objPos.z *= zScale;
    objPos = mvpMatrix * vec4(objPos.xyz, 1.0);
    gl_Position = objPos;
}
