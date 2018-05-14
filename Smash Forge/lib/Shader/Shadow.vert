#version 330
in vec3 vPosition;
out vec4 outPosition;
uniform mat4 lightSpaceMatrix;
uniform mat4 modelMatrix;
uniform mat4 eyeview; // modelview matrix

void main()
{
    gl_Position = lightSpaceMatrix * modelMatrix * vec4(vPosition, 1.0f); //lightSpaceMatrix * eyeview * vec4(vPosition, 1.0f);
    outPosition = gl_Position;
}