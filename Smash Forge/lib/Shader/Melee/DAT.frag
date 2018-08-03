#version 330

in vec2 texcoord;
in vec4 vertexColor;
in vec3 normal;
in vec3 boneWeightsColored;

out vec4 fragColor;

uniform vec3 difLightDirection;
uniform vec3 difLightColor;
uniform vec3 ambLightColor;

uniform int renderType;
uniform int renderVertColor;

uniform sampler2D tex;
uniform sampler2D UVTestPattern;
uniform vec2 uvscale;
uniform mat4 modelview;

void main() {
    vec3 displayNormal = normal * 0.5 + 0.5;

    vec4 alpha = texture(tex, texcoord * uvscale).aaaa;
    fragColor = vec4 ((vertexColor * alpha * texture(tex, texcoord*uvscale)).xyz, alpha.a * vertexColor.w);

    vec4 diffuseColor = texture(tex, texcoord * uvscale).rgba;

    float halfLambert = dot(difLightDirection, normal.xyz);
    halfLambert = (halfLambert + 1) / 2;
    vec3 lighting = mix(ambLightColor, difLightColor, halfLambert); // gradient based lighting

    fragColor = vec4(1);
    vec3 resultingColor = diffuseColor.rgb * lighting.rgb;
    if (renderVertColor == 1)
        resultingColor *= vertexColor.rgb;

    if (renderType == 1) // normals vertexColor
        fragColor = vec4(displayNormal,1);
    else if (renderType == 2)
    {
        // normals black & white
        float normalBnW = dot(vec4(normal * mat3(modelview), 1.0), vec4(0.15,0.15,0.15,1.0));
        fragColor.rgb = vec3(normalBnW);
    }
    else if (renderType == 3)
        fragColor = vec4(diffuseColor.rgb, 1);
    else if (renderType == 4) // normal maps
        fragColor = vec4(vec3(0), 1);
    else if (renderType == 5) // vertexColor
        fragColor = vertexColor;
    else if (renderType == 6) // ao
        fragColor = vec4(vec3(1), 1);
    else if (renderType == 7) // uv coords
        fragColor = vec4(texcoord.x, texcoord.y, 1, 1);
    else if (renderType == 8) // uv test pattern
        fragColor = vec4(texture(UVTestPattern, texcoord).rgb, 1);
    else if (renderType == 9)
        fragColor = vec4(vec3(0), 1);
    else if (renderType == 10)
        fragColor = vec4(vec3(0), 1);
    else if (renderType == 11)
        fragColor = vec4(vec3(0), 1);
    else if (renderType == 12)
        fragColor = vec4(boneWeightsColored, 1);
    else
        fragColor = vec4(resultingColor, 1);
}
