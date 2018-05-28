#version 330
in vec2 texCoord;

uniform int renderBloom;
uniform float bloomIntensity;

uniform sampler2D image0;
uniform sampler2D image1;

uniform vec3 backgroundTopColor;
uniform vec3 backgroundBottomColor;

uniform float weight[5] = float[] (0.1784, 0.210431, 0.222338, 0.210431, 0.1784);

out vec4 fragColor;

void main()
{
    fragColor = vec4(0,0,0,1);
    vec4 textureColor0 = texture(image0, texCoord).rgba;

    vec2 texCoord1 = vec2(texCoord.x, 1 - texCoord.y);
    vec4 textureColor1 = texture(image1, texCoord1).rgba;

    // Used for pixel offset
    vec2 textureOffset = 1.0 / textureSize(image1, 0);

    // Gaussian blur.
    vec3 blurResult = textureColor1.rgb * weight[0];
    int blurRadius = 5;
    // Horizontal Blur
    for (int i = 0; i < blurRadius; i++)
    {
        blurResult += texture(image1, texCoord1 + vec2(textureOffset.x * i, 0)).rgb * weight[i];
        blurResult += texture(image1, texCoord1 - vec2(textureOffset.x * i, 0)).rgb * weight[i];
    }
    // Vertical Blur
    for (int i = 0; i < blurRadius; i++)
    {
        blurResult += texture(image1, texCoord1 + vec2(0, textureOffset.y * i)).rgb * weight[i];
        blurResult += texture(image1, texCoord1 - vec2(0, textureOffset.y * i)).rgb * weight[i];
    }

    // Background Color
    vec3 backgroundTotalColor = mix(backgroundBottomColor, backgroundTopColor, texCoord.y);

    vec3 bloomColor = blurResult * bloomIntensity * renderBloom;

    fragColor.rgb = textureColor0.rgb;
    fragColor.rgb = mix(backgroundTotalColor, fragColor.rgb, min(1, textureColor0.a));
    fragColor.rgb += bloomColor;
}
