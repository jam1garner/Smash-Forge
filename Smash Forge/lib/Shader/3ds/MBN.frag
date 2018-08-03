#version 330
in vec2 texcoord;
in vec4 vertexColor;
in vec3 normal;
in vec4 position;
in vec3 boneWeightsColored;

uniform vec3 difLightDirection;
uniform vec3 difLightColor;
uniform vec3 ambLightColor;

uniform sampler2D tex;
uniform sampler2D UVTestPattern;

uniform int renderType;
uniform int renderVertColor;
uniform mat4 modelview;

out vec4 FragColor;
void main()
{
    vec3 displayNormal = normal.xyz;
    displayNormal = (displayNormal + 1) / 2;

    vec4 diffuseColor = texture(tex, texcoord);

    float halfLambert = dot(difLightDirection, normal.xyz);
    halfLambert = (halfLambert + 1) / 2;
    vec3 lighting = mix(ambLightColor, difLightColor, halfLambert); // gradient based lighting

    FragColor = vec4(1);
    vec3 resultingColor = diffuseColor.rgb * lighting.rgb;
    if (renderVertColor == 1)
        FragColor *= vertexColor.rgba;

    if (renderType == 1) // normals color
        FragColor = vec4(displayNormal,1);
    else if (renderType == 2) // lighting
    {
        FragColor.rgb = lighting;
    }
    else if (renderType == 3)
        FragColor = vec4(diffuseColor.rgb, 1);
    else if (renderType == 4)
        FragColor = vec4(vec3(0), 1);
    else if (renderType == 5) // vertexColor
        FragColor = vertexColor;
    else if (renderType == 6) // ambient occlusion
        FragColor = vec4(vec3(1), 1);
    else if (renderType == 7) // uv coords
        FragColor = vec4(texcoord.x, texcoord.y, 1, 1);
    else if (renderType == 8) // uv test pattern
        FragColor = vec4(texture(UVTestPattern, texcoord).rgb, 1);
    else if (renderType == 9)
        FragColor = vec4(vec3(0), 1);
    else if (renderType == 10)
        FragColor = vec4(vec3(0), 1);
    else if (renderType == 11)
        FragColor = vec4(vec3(0), 1);
    else if (renderType == 12)
        FragColor.rgb = boneWeightsColored;
    else
        FragColor.rgb = resultingColor;
}
