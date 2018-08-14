#version 330

in vec2 uv1;
in vec2 uv2;
in vec2 uv3;
in vec3 normal;
in vec4 vertexColor;
in vec3 tangent;
in vec3 bitangent;

uniform int uvChannel;
uniform int renderType;

uniform sampler2D uvTestPattern;

out vec4 fragColor;

vec2 GetUvChannel(int channel)
{
    if (uvChannel == 1)
        return uv1;
    else if (uvChannel == 2)
        return uv2;
    else if (uvChannel == 3)
        return uv3;
    else
        return vec2(0);
}

void main()
{
    vec2 displayUv = GetUvChannel(uvChannel);

    fragColor = vec4(1);
    if (renderType == 0)
    {
        // shaded
        float halfLambert = dot(normal, vec3(0,0,1));
        fragColor.rgb = vec3(halfLambert);
    }
    else if (renderType == 1)
    {
        // normal
        fragColor.rgb = normal * 0.5 + 0.5;
    }
    else if (renderType == 2)
    {
        // lighting
    }
    else if (renderType == 3)
    {
        // diffuse map
    }
    else if (renderType == 4)
    {
        // normal maps
    }
    else if (renderType == 5)
    {
        fragColor.rgb = vertexColor.rgb;
    }
    else if (renderType == 6)
    {
        // ambient occlusion
    }
    else if (renderType == 7)
    {
        // uv
        fragColor.rgb = vec3(displayUv, 1);
    }
    else if (renderType == 8)
    {
        // uv pattern
        fragColor.rgb = texture(uvTestPattern, displayUv).rgb;
    }
    else if (renderType == 9)
    {
        // tangents
        fragColor.rgb = tangent;
    }
    else if (renderType == 10)
    {
        // bitangents
        fragColor.rgb = bitangent;
    }
}
