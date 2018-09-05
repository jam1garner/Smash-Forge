#version 330

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

// Attributes from vertex shader.
in vec3 geomNormal[];
in vec3 geomViewNormal[];
in vec3 geomTangent[];
in vec3 geomBitangent[];
in vec4 geomFragPos[];
in vec4 geomFragPosLightSpace[];
in vec3 geomViewPosition[];
in vec3 geomObjectPosition[];

in vec4 geomVertexColor[];

in vec2 geomTexCoord[];
in vec2 geomTexCoord2[];
in vec2 geomTexCoord3[];
in vec2 geomNormaltexCoord[];

in vec3 geomBoneWeightsColored[];

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
out vec4 fragPos;
out vec4 fragPosLightSpace;
out vec3 boneWeightsColored;
noperspective out vec3 edgeDistance;

// Defined in EdgeDistance.geom.
vec3 EdgeDistances();

void main()
{
    vec3 distances = EdgeDistances();

    // Create a triangle and assign the vertex attributes.
    for (int i = 0; i < 3; i++)
    {
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
        fragPos = geomFragPos[i];
        fragPosLightSpace = geomFragPosLightSpace[i];
        boneWeightsColored = geomBoneWeightsColored[i];

        // The distance from a point to each of the edges.
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
