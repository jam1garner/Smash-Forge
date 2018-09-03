#version 330

in vec3 objectPosition;
in vec3 normal;
in vec2 UV0;

uniform sampler2D diffuseTex;

uniform sampler2D UVTestPattern;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform float glossiness;
uniform float Transparency;

uniform int colorOverride;
uniform int renderType;

uniform mat4 mvpMatrix;

out vec4 fragColor;

void main()
{
	if (colorOverride == 1)
	{
		fragColor = vec4(1);
		return;
	}

	fragColor = vec4(0, 0, 0, 1);

    vec3 displayNormal = normal * 0.5 + 0.5;
    vec2 displayTexCoord = UV0;

    // render modes
    if (renderType == 1) // normals color
        fragColor.rgb = displayNormal;
    else if (renderType == 2)
    {
        // lighting
        vec3 V = vec3(0,0,-1) * mat3(mvpMatrix);
        float lambert = clamp(dot(normal, V), 0, 1);
        fragColor.rgb = mix(ambientColor.rgb, diffuseColor.rgb, lambert);
    }
    else if (renderType == 3)
    {
        // diffuse map
        fragColor.rgb = texture(diffuseTex, UV0).rgb;
    }
    else if (renderType == 4)
    {
        // normal map
    }
    else if (renderType == 5)
    {
        // vertex color
    }
    else if (renderType == 6)
    {
        // ambient occlusion

    } else if (renderType == 7)
    {
        // uv coords
        fragColor.rgb = vec3(displayTexCoord, 1);
    }
    else if (renderType == 8)
    {
        // uv test pattern
        fragColor.rgb = texture(UVTestPattern, displayTexCoord).rgb;
    }
    else if (renderType == 9)
    {
         // tangents
    }
    else if (renderType == 10)
    {
        // bitangents
    }
    else if (renderType == 11)
    {
        // light set #
    }
    else if (renderType == 12)
    {
        // bone weights colored
    }
}
