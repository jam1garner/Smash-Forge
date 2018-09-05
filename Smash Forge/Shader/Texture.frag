#version 330

in vec2 texCoord;

uniform sampler2D image;

// TODO: Make vector 3
uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;
uniform int preserveAspectRatio;
uniform float width;
uniform float height;
uniform int currentMipLevel;

uniform float intensity;

out vec4 fragColor;

void main() {
    fragColor = vec4(0,0,0,1);

    bool fill = false;
    // Perform texture aspect ratio scaling.
    float xCoord = texCoord.x;
    float yCoord = 1 - texCoord.y;

    float scale = 1;
    if (preserveAspectRatio == 1) {
        if (width > height) {
            scale = width / height;
            yCoord *= scale;
            if ((1 - texCoord.y) > (1 / scale))
                fill = true;
        } else {
            scale = height / width;
            xCoord *= scale;
            if (texCoord.x > (1 / scale))
                fill = true;
        }
    }

    vec4 textureColor = textureLod(image, vec2(xCoord, yCoord), currentMipLevel).rgba;

    // Toggles rendering of individual color channels for all render modes.
    fragColor.rgb = textureColor.rgb * vec3(renderR, renderG, renderB);
    if (renderR == 1 && renderG == 0 && renderB == 0)
        fragColor.rgb = textureColor.rrr;
    else if (renderG == 1 && renderR == 0 && renderB == 0)
        fragColor.rgb = textureColor.ggg;
    else if (renderB == 1 && renderR == 0 && renderG == 0)
        fragColor.rgb = textureColor.bbb;

    if (renderAlpha == 1)
        fragColor.a = textureColor.a;
    if (alphaOverride == 1)
        fragColor = vec4(textureColor.aaa, 1);
    if (fill)
        fragColor = vec4(1);

    fragColor.rgb *= intensity;
}
