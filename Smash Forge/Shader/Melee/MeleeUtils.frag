#version 330

vec3 CalculateBumpMapNormal(vec3 normal, vec3 tangent, vec3 bitangent,
    int hasBump, sampler2D bumpMap, int width, int height, vec2 texCoords)
{
    if (hasBump != 1)
        return normal;

    // Compute a normal based on difference in height.
    float offsetX = 1.0 / width;
    float offsetY = 1.0 / height;
    float a = texture2D(bumpMap, texCoords).r;
    float b = texture2D(bumpMap, texCoords + vec2(offsetX, 0)).r;
    float c = texture2D(bumpMap, texCoords + vec2(0, offsetY)).r;
    vec3 bumpNormal = normalize(vec3(b-a, c-a, 0.1));

    mat3 tbnMatrix = mat3(tangent, bitangent, normal);
    vec3 newNormal = tbnMatrix * bumpNormal;
    return normalize(newNormal);
}
