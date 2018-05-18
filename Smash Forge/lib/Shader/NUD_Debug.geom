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
out vec3 boneWeightsColored;
noperspective out vec3 edgeDistance;

// Adapted from code in David Wolff's "OpenGL 4.0 Shading Language Cookbook"
// https://gamedev.stackexchange.com/questions/136915/geometry-shader-wireframe-not-rendering-correctly-glsl-opengl-c
void main() {
    float a = length(gl_in[1].gl_Position.xyz - gl_in[2].gl_Position.xyz);
    float b = length(gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz);
    float c = length(gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz);

    float alpha = acos( (b*b + c*c - a*a) / (2.0*b*c) );
    float beta = acos( (a*a + c*c - b*b) / (2.0*a*c) );
    float ha = abs( c * sin( beta ) );
    float hb = abs( c * sin( alpha ) );
    float hc = abs( b * sin( alpha ) );

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
        boneWeightsColored = geomBoneWeightsColored[i];

        // The distance from a point to each of the edges.
        // This benefits from interpolation.
        if (i == 0)
            edgeDistance = vec3(ha, 0, 0);
        else if (i == 1)
            edgeDistance = vec3(0, hb, 0);
        else if (i == 2)
            edgeDistance = vec3(0, 0, hc);

        EmitVertex();
    }

    EndPrimitive();
}
