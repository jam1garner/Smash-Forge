#version 120
varying vec2 texCoord;

uniform sampler2D image;

uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;
uniform int preserveAspectRatio;
uniform int width;
uniform int height;

void main()
{
    gl_FragColor = vec4(0,0,0,1);
    vec4 textureColor = vec4(1);
    bool fill = false;
    // Perform texture aspect ratio scaling.
    float xCoord = texCoord.x;
    float yCoord = 1 - texCoord.y;

    float scale = 1;
    if (preserveAspectRatio == 1)
    {
        if (width > height)
        {
            scale = width / height;
            yCoord *= scale;
            if ((1 - texCoord.y) > (1 / scale))
                fill = true;
        }
        else
        {
            scale = height / width;
            xCoord *= scale;
            if (texCoord.x > (1 / scale))
                fill = true;
        }
    }

    textureColor = texture2D(image, vec2(xCoord, yCoord)).rgba;

    // Control what channels are displayed. Single channels are greyscale.
    if (renderR == 1)
    {
        gl_FragColor.r = textureColor.r;
        if (renderB == 0 && renderG == 0)
            gl_FragColor.rgb = textureColor.rrr;
    }
    if (renderG == 1)
    {
        gl_FragColor.g = textureColor.g;
        if (renderB == 0 && renderR == 0)
            gl_FragColor.rgb = textureColor.ggg;
    }
    if (renderB == 1)
    {
        gl_FragColor.b = textureColor.b;
        if (renderG == 0 && renderR == 0)
            gl_FragColor.rgb = textureColor.bbb;
    }
    if (renderAlpha == 1)
    {
        gl_FragColor.a = textureColor.a;
    }
    if (alphaOverride == 1)
        gl_FragColor = vec4(textureColor.aaa, 1);
    if (fill)
        gl_FragColor = vec4(1);
}
