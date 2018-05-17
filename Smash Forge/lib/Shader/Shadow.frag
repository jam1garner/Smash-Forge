#version 330
//out vec4 outColor;
in vec4 outPosition;
void main()
{   
    float depth = outPosition.z;
    depth *= 0.01;
    //outColor = vec4(vec3(depth), 1);
}