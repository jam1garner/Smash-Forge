#version 330

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

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

void main() {
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
        EmitVertex();
    }

    EndPrimitive();
}
