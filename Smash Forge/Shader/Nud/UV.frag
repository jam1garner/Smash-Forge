#version 330

noperspective in vec3 edgeDistance;

out vec4 fragColor;

// Defined in Wireframe.frag.
float WireframeIntensity(vec3 distanceToEdges);

void main() {
    vec3 edgeColor = vec3(1);
    float intensity = WireframeIntensity(edgeDistance);
    fragColor.rgb = edgeColor;
    fragColor.a = intensity;
}
