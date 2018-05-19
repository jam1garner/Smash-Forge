#version 330

// Basic directional light.
struct StageLight {
    int enabled;
    vec3 color;
    vec3 direction;
};

vec3 FogColor(vec3 inputColor, vec4 fogParams, float depth, vec3 stageFogColor) {
    depth = clamp((depth / fogParams.y), 0, 1);
    float fogIntensity = mix(fogParams.z, fogParams.w, depth);
    return mix(inputColor, pow((stageFogColor), vec3(2.2)), fogIntensity);
}

vec3 StageDiffuseLighting(vec3 N, StageLight[4] lights) {
    vec3 lighting = vec3(0);
    for (int i = 0; i < 4; i++) {
        // TODO: Do stages use half lambert?
        vec3 lightColor = lights[i].color * max((dot(N, lights[i].direction)), 0);
        lighting += lightColor * lights[i].enabled;
    }
    return lighting;
}
