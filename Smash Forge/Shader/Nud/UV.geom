#version 330

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

// Outputs to fragment shader.
noperspective out vec3 edgeDistance;

// Defined in EdgeDistance.geom.
vec3 EdgeDistances();

void main() {
    vec3 distances = EdgeDistances();

    // Create a triangle and assign the vertex attributes.
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;

        // The distance from a point to each of the edges.
        // This benefits from interpolation.
        if (i == 0)
            edgeDistance = vec3(distances.x, 0, 0);
        else if (i == 1)
            edgeDistance = vec3(0, distances.y, 0);
        else if (i == 2)
            edgeDistance = vec3(0, 0, distances.z);

        EmitVertex();
    }

    EndPrimitive();
}
