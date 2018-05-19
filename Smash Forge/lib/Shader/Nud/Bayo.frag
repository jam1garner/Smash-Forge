# version 330

// Defined in SmashShader.frag.
float AnisotropicSpecExponent(vec3 halfAngle, float width, float height, vec3 tangent, vec3 bitangent);

vec3 BayoHairDiffuse(vec3 diffuseMap, vec4 colorOffset, vec4 colorGain, vec4 alphaBlendParams) {
    vec3 diffuseColor = (colorOffset.rgb + diffuseMap.rrr) * colorGain.rgb;
    diffuseColor *= alphaBlendParams.w; // #justbayothings
    return diffuseColor;
}

vec3 BayoHairSpecular(vec3 diffuseMap, vec3 halfAngle, vec4 reflectionParams, vec3 tangent, vec3 bitangent) {
    float specMask = diffuseMap.b;

    float exponent = AnisotropicSpecExponent(halfAngle, reflectionParams.z, reflectionParams.w, tangent, bitangent);
    float specularTerm = pow(dot(bitangent.xyz, halfAngle), exponent);

    // TODO: Find proper constants.
    vec3 specularColorTotal = vec3(1) * specularTerm * specMask;
    return specularColorTotal;
}
