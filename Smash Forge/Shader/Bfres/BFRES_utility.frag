#version 330

// A struct is used for what would normally be attributes from the vert/geom shader.
struct VertexAttributes
{
    vec3 objectPosition;
    vec2 texCoord;
    vec2 texCoord2;
    vec2 texCoord3;
    vec4 vertexColor;
    vec3 normal;
    vec3 viewNormal;
    vec3 tangent;
    vec3 bitangent;
};

// Defined in Utility.frag.
float Luminance(vec3 rgb);

vec3 SpecularPass(vec3 I, vec3 normal, int HasSpecularMap, sampler2D SpecularMap, vec3 SpecColor, VertexAttributes vert, float texcoord2)
{
    float specBrdf = max(dot(I, normal), 0);
    float exponent = 8;



	if (SpecColor == vec3(0)) //Color shouldn't be black unless it's not set
	    SpecColor = vec3(1);

    if (HasSpecularMap == 0)
	{
        return 0.1 * SpecColor * pow(specBrdf, exponent);
	}

    // TODO: Different games use the channels for separate textures.
	vec3 specularTex = vec3(1);
	if (texcoord2 == 1)
	    specularTex = texture(SpecularMap, vert.texCoord2).rrr;
	else
	    specularTex = texture(SpecularMap, vert.texCoord).rrr;

    vec3 result = specularTex * SpecColor * pow(specBrdf, exponent);
	result *= SpecColor.rgb;

    float intensity = 0.3;
    return result * intensity;
}

vec3 EmissionPass(sampler2D EmissionMap, float emission_intensity, VertexAttributes vert, float texCoordIndex, vec3 emission_color)
{
    vec3 result = vec3(0);
    float emissionIntensity2 = emission_intensity * emission_intensity;

    if (emission_intensity > 0.1)
    {
        vec3 emission = vec3(emissionIntensity2);
        result += emission;
    }

    
        // BOTW somtimes uses second uv channel for emission map
        vec3 emission = vec3(1);
        if (texCoordIndex == 1)
            emission = texture2D(EmissionMap, vert.texCoord2).rgb * vec3(1);
        else
            emission = texture2D(EmissionMap, vert.texCoord).rgb * emission_color;

        // If tex is empty then use full brightness.
        //Some emissive mats have emission but no texture
        if (Luminance(emission.rgb) < 0.01)
            result += vec3(emission_intensity) * emission_color;
        else
            result += emission.rgb;
    

    return result;
}

float AmbientOcclusionBlend(sampler2D BakeShadowMap, VertexAttributes vert, float ao_density)
{
    float aoMap = texture(BakeShadowMap, vert.texCoord2).r;
    return mix(aoMap, 1, ao_density);
}

vec3 CalcBumpedNormal(vec3 inputNormal, sampler2D normalMap, VertexAttributes vert, float texCoordIndex)
{
    float normalIntensity = 1;

	//if (normal_map_weight != 0) //MK8 and splatoon 1/2 uses this param
	//      normalIntensity = normal_map_weight;

    // Calculate the resulting normal map and intensity.
	vec3 normalMapColor = vec3(1);
	if (texCoordIndex == 1)
        normalMapColor = vec3(texture(normalMap, vert.texCoord2).rg, 1);
    else
        normalMapColor = vec3(texture(normalMap, vert.texCoord).rg, 1);
    normalMapColor = mix(vec3(0.5, 0.5, 1), normalMapColor, normalIntensity);

    // Remap the normal map to the correct range.
    vec3 normalMapNormal = 2.0 * normalMapColor - vec3(1);

    // TBN Matrix.
    vec3 T = vert.tangent;
    vec3 B = vert. bitangent;
    if (Luminance(B) < 0.01)
        B = normalize(cross(T,  vert.normal));
    mat3 tbnMatrix = mat3(T, B,  vert.normal);

    vec3 newNormal = tbnMatrix * normalMapNormal;
    return normalize(newNormal);
}