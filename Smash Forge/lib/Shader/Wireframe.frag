#version 330

float WireframeIntensity(vec3 distanceToEdges) {
    float minDistance = min(min(distanceToEdges.x, distanceToEdges.y), distanceToEdges.z);
    float smoothedDistance = exp2(-512.0 * minDistance * minDistance);
    return smoothedDistance;
}
