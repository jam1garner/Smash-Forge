#version 330

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

uniform vec2 windowSize;

// Attributes from vertex shader.
in vec3 geomNormal[];
in vec3 geomViewNormal[];
in vec3 geomTangent[];
in vec3 geomBitangent[];
in vec3 geomFragpos[];
in vec3 geomViewPosition[];
in vec3 geomObjectPosition[];

in vec4 geomVertexColor[];

in vec2 geomTexCoord[];
in vec2 geomTexCoord2[];
in vec2 geomTexCoord3[];
in vec2 geomNormaltexCoord[];

// Outputs to fragment shader.
out vec3 viewPosition;
out vec3 objectPosition;
out vec2 texCoord;
out vec2 texCoord2;
out vec2 texCoord3;
out vec2 normaltexCoord;
out vec4 vertexColor;
out vec3 normal;
out vec3 viewNormal;
out vec3 tangent;
out vec3 bitangent;
noperspective out vec3 edgeDistance;

void main() {
    vec2 p0 = windowSize * gl_in[0].gl_Position.xy / gl_in[0].gl_Position.w;
    vec2 p1 = windowSize * gl_in[1].gl_Position.xy / gl_in[1].gl_Position.w;
    vec2 p2 = windowSize * gl_in[2].gl_Position.xy / gl_in[2].gl_Position.w;
    vec2 v0 = p2 - p1;
    vec2 v1 = p2 - p0;
    vec2 v2 = p1 = p0;
    float area = abs(v1.x * v2.y - v1.y * v2.x);

    // Create a triangle and assign the vertex attributes.
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;
        viewPosition = geomViewPosition[i];
        objectPosition = geomObjectPosition[i];
        texCoord = geomTexCoord[i];
        texCoord2 = geomTexCoord2[i];
        texCoord3 = geomTexCoord3[i];
        normaltexCoord = geomNormaltexCoord[i];
        vertexColor = geomVertexColor[i];
        normal = geomNormal[i];
        viewNormal = geomViewNormal[i];
        tangent = geomTangent[i];
        bitangent = geomBitangent[i];

        // The distance from a point to each of the edges.
        // This will be correct after interpolation.
        if (i == 0)
            edgeDistance = vec3(area / length(v0), 0, 0);
        else if (i == 1)
            edgeDistance = vec3(0, area / length(v1), 0);
        else if (i == 2)
            edgeDistance = vec3(0, 0, area / length(v2));

        EmitVertex();
    }

    EndPrimitive();
}
