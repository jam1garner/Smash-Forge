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

uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;

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

	vec4 resultingColor = vec4(0, 0, 0, 1);

    // render modes
    if (renderType == 1) // normals color
        resultingColor.rgb = displayNormal;
    else if (renderType == 2)
    {
        // lighting
        vec3 V = vec3(0,0,-1) * mat3(mvpMatrix);
        float lambert = clamp(dot(normal, V), 0, 1);
        resultingColor.rgb = mix(ambientColor.rgb, diffuseColor.rgb, lambert);
    }
    else if (renderType == 3)
    {
        // diffuse map
        resultingColor.rgba = texture(diffuseTex, UV0).rgba;
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
        resultingColor.rgb = vec3(displayTexCoord, 1);
    }
    else if (renderType == 8)
    {
        // uv test pattern
        resultingColor.rgb = texture(UVTestPattern, displayTexCoord).rgb;
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

	// Toggles rendering of individual color channels for all render modes.
	fragColor.rgb = resultingColor.rgb * vec3(renderR, renderG, renderB);
	if (renderR == 1 && renderG == 0 && renderB == 0)
		fragColor.rgb = resultingColor.rrr;
	else if (renderG == 1 && renderR == 0 && renderB == 0)
		fragColor.rgb = resultingColor.ggg;
	else if (renderB == 1 && renderR == 0 && renderG == 0)
		fragColor.rgb = resultingColor.bbb;

	if (renderAlpha == 1) {
		fragColor.a = resultingColor.a;
	}
	
	if (alphaOverride == 1)
		fragColor = vec4(resultingColor.aaa, 1);
}
