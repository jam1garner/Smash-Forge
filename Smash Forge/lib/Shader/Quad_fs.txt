#version 330
in vec2 texCoord;
out vec4 outColor;

uniform sampler2D ShadowMap;
uniform sampler2D ScreenRender;
uniform sampler2D ScreenRenderBlur;

float LinearizeDepth(float depth)
{   float near_plane = 1.0;
    float far_plane = 500.0;
    float z = depth * 2.0 - 1.0; // Back to NDC
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}

void main()
{
    vec4 screenColor = texture(ScreenRenderBlur, texCoord).rgba;
    vec4 backgroundColor = mix(vec4(0), vec4(1), texCoord.y);
    //backgroundColor = vec4(1);
    float test = texture(ShadowMap, texCoord).r;
   // test = LinearizeDepth(test);
    vec3 depthVector = vec3(test);
    //outColor = mix(backgroundColor, vec4(depthVector,1), screenColor.a);


    // horizontal guassian blur
    float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
    vec2 tex_offset = 1.0 / textureSize(ScreenRender, 0); // gets size of single texel
    vec3 result = texture(ScreenRender, texCoord).rgb * weight[0]; // current fragment's contribution

        for(int i = 1; i < 5; ++i)
        {
            result += texture(ScreenRender, texCoord + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            result += texture(ScreenRender, texCoord - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        }
    //screenColor.rgb = result;

    result = screenColor.rgb;

    //outColor.rgb = mix(backgroundColor.rgb, screenColor.rgb, screenColor.a);
    outColor = vec4(result,1);
}
