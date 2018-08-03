#version 330

// Adapted from code in David Wolff's "OpenGL 4.0 Shading Language Cookbook"
// https://gamedev.stackexchange.com/questions/136915/geometry-shader-wireframe-not-rendering-correctly-glsl-opengl-c
vec3 EdgeDistances() {
    float a = length(gl_in[1].gl_Position.xyz - gl_in[2].gl_Position.xyz);
    float b = length(gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz);
    float c = length(gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz);

    float alpha = acos((b*b + c*c - a*a) / (2.0*b*c));
    float beta = acos((a*a + c*c - b*b) / (2.0*a*c));
    float ha = abs(c * sin(beta));
    float hb = abs(c * sin(alpha));
    float hc = abs(b * sin(alpha));
    return vec3(ha, hb, hc);
}
