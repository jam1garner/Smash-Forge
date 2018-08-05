#version 330

//------------------------------------------------------------------------------------
//
//      Viewport Camera/Lighting
//
//------------------------------------------------------------------------------------

uniform mat4 mvpMatrix;
uniform vec3 specLightDirection;
uniform vec3 difLightDirection;

uniform int enableCellShading;

const float levels = 3.0;

//------------------------------------------------------------------------------------
//
//      Verex Attributes
//
//------------------------------------------------------------------------------------

in vec2 f_texcoord0;
in vec2 f_texcoord1;
in vec2 f_texcoord2;
in vec2 f_texcoord3;
in vec3 normal;
in vec4 color;
in vec3 tangent;
in vec3 bitangent;

in vec3 boneWeightsColored;

//------------------------------------------------------------------------------------
//
//      Viewport Settings
//
//------------------------------------------------------------------------------------

uniform int uvChannel;
uniform int renderType;
uniform int useNormalMap;
uniform vec4 colorSamplerUV;
uniform int renderVertColor;
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform int colorOverride;

// Channel Toggles
uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;

//------------------------------------------------------------------------------------
//
//      Texture Samplers
//
//------------------------------------------------------------------------------------

uniform sampler2D tex0;
uniform sampler2D BakeShadowMap;
uniform sampler2D spl;
uniform sampler2D nrm;
uniform sampler2D BakeLightMap;
uniform sampler2D UVTestPattern;
uniform sampler2D TransparencyMap;
uniform sampler2D EmissionMap;
uniform sampler2D SpecularMap;
uniform sampler2D DiffuseLayer;

//------------------------------------------------------------------------------------
//
//      Shader Params
//
//------------------------------------------------------------------------------------

uniform float normal_map_weight;
uniform float ao_density;
uniform float emission_intensity;
uniform vec4 fresnelParams;
uniform vec4 base_color_mul_color;
uniform vec3 emission_color;


//------------------------------------------------------------------------------------
//
//      Shader Options
//
//------------------------------------------------------------------------------------

uniform float uking_texture2_texcoord;
uniform float bake_shadow_type;
uniform float enable_fresnel;
uniform float enable_emission;


//------------------------------------------------------------------------------------
//
//      Texture Map Toggles
//
//------------------------------------------------------------------------------------

uniform int HasNormalMap;
uniform int HasSpecularMap;
uniform int HasShadowMap;
uniform int HasAmbientOcclusionMap;
uniform int HasLightMap;
uniform int HasTransparencyMap;
uniform int HasEmissionMap;
uniform int HasDiffuseLayer;
uniform int isTransparent;



out vec4 fragColor;

#define gamma 2.2

// Defined in Utility.frag.
float Luminance(vec3 rgb);

vec2 displayTexCoord =  f_texcoord0;

float AmbientOcclusionBlend()
{
    float aoMap = texture(BakeShadowMap, f_texcoord1).r;
    return mix(aoMap, 1, ao_density);
}

vec3 SpecularPass(vec3 I, vec3 normal)
{
    if (HasSpecularMap == 0)
        return vec3(0);

    // TODO: Different games use the channels for separate textures.
    vec3 specularColor = texture(SpecularMap, f_texcoord0).rrr;

    float specBrdf = max(dot(I, normal), 0);
    float exponent = 8;

    vec3 result = specularColor * pow(specBrdf, exponent);
    float intensity = 0.3;
    return result * intensity;
}

vec3 CalcBumpedNormal(vec3 inputNormal) //Currently reused some bits from nud shader.
{
    // If there's no normal map, then return just the normal.
    if (useNormalMap == 0 || HasNormalMap == 0)
	   return inputNormal;

    float normalIntensity = 1;

	//if (normal_map_weight != 0) //MK8 and splatoon 1/2 uses this param
	//      normalIntensity = normal_map_weight;

    // Calculate the resulting normal map and intensity.
	vec3 normalMapColor = vec3(1);
	if (uking_texture2_texcoord == 1)
        normalMapColor = vec3(texture(nrm, f_texcoord1).rg, 1);
    else
        normalMapColor = vec3(texture(nrm, displayTexCoord).rg, 1);
    normalMapColor = mix(vec3(0.5, 0.5, 1), normalMapColor, normalIntensity);

    // Remap the normal map to the correct range.
    vec3 normalMapNormal = 2.0 * normalMapColor - vec3(1);

    // TBN Matrix.
    vec3 T = tangent;
    vec3 B = bitangent;
    if (Luminance(B) < 0.01)
        B = normalize(cross(T, normal));
    mat3 tbnMatrix = mat3(T, B, normal);

    vec3 newNormal = tbnMatrix * normalMapNormal;
    return normalize(newNormal);
}

vec3 EmissionPass()
{
    vec3 result = vec3(0);
    float emissionIntensity2 = emission_intensity * emission_intensity;

    if (emission_intensity > 0.1)
    {
        vec3 emission = vec3(emissionIntensity2);
        result += emission;
    }

    if (HasEmissionMap == 1 || enable_emission == 1)
    {
        // BOTW somtimes uses second uv channel for emission map
        vec3 emission = vec3(1);
        if (uking_texture2_texcoord == 1)
            emission = texture2D(EmissionMap, f_texcoord1).rgb * vec3(1);
        else
            emission = texture2D(EmissionMap, displayTexCoord).rgb * emission_color;

        // If tex is empty then use full brightness.
        //Some emissive mats have emission but no texture
        if (Luminance(emission.rgb) < 0.01)
            result += vec3(emission_intensity) * emission_color;
        else
            result += emission.rgb;
    }

    return result;
}

void main()
{
    fragColor = vec4(vec3(0), 1);

    if (uvChannel == 2)
        displayTexCoord = f_texcoord1;
    if (uvChannel == 3)
        displayTexCoord = f_texcoord3;

    // Wireframe color.
    if (colorOverride == 1)
    {
		if (renderVertColor == 1)
            fragColor = vec4(color);
		else
            fragColor = vec4(1);

        return;
    }

    // Calculate shading vectors.
    vec3 I = vec3(0,0,-1) * mat3(mvpMatrix);
	vec3 bumpMapNormal = CalcBumpedNormal(normal);

    // Light Map
    vec4 LightMapColor = texture(BakeLightMap, f_texcoord2);

    // Shadow Map
    vec3 ShadowDepth = texture(BakeShadowMap, f_texcoord1).ggg;
	float shadow_intensity = LightMapColor.a;

    // Diffuse lighting.
	float brightness = 0;
	if (enableCellShading == 1)
	{
        // Higher blend values make the dark region smoother and larger.
        float lambert = max(dot(bumpMapNormal, difLightDirection), 0);
        float smoothness = 0.1;
        float center = 0.5;
        float edgeL = center;
        float edgeR = center + (smoothness * 0.5);
        float smoothLambert = smoothstep(edgeL, edgeR, lambert);

        float ambient = 0.6;
        smoothLambert = clamp(smoothLambert + ambient, 0, 1);

		brightness = smoothLambert * 3;
	}
	else
	{
        float halfLambert = dot(difLightDirection, bumpMapNormal) * 0.5 + 0.5;
		brightness = halfLambert;
	}

    //Texture Overlay (Like an emblem in mk8)
    if (HasDiffuseLayer == 1)
        fragColor += vec4(texture(DiffuseLayer, f_texcoord3).rgb, 1) * vec4(1);

    // Default Shader
    vec4 alpha = texture2D(tex0, displayTexCoord).aaaa;

    if (HasTransparencyMap == 1)
    {
        // TODO: ???
        alpha = texture2D(TransparencyMap, displayTexCoord).rgba;
        alpha *= 0.5;
    }

	vec4 diffuseMapColor = vec4(texture(tex0, displayTexCoord).rgb, 1);
    diffuseMapColor *= brightness;
    fragColor.rgb += diffuseMapColor.rgb;

    // Render Passes
    fragColor.rgb += EmissionPass();
    fragColor.rgb += SpecularPass(I, bumpMapNormal);

    if (HasAmbientOcclusionMap == 1)
    {
         vec4 A0Tex = vec4(1);

         if (uking_texture2_texcoord == 1)
               A0Tex = vec4(texture(BakeShadowMap, f_texcoord1).rrr, 1);
		 else
		      A0Tex = vec4(texture(BakeShadowMap, displayTexCoord).rrr, 1);

	    fragColor *= A0Tex;
    }
	if (HasShadowMap == 1)
	{
	     if (bake_shadow_type == 0)
		 {
		       vec4 A0Tex = vec4(texture(BakeShadowMap, f_texcoord1).rrr, 1);
		       fragColor *= A0Tex;
		 }
         if (bake_shadow_type == 2)
		 {
		       vec4 A0Tex = vec4(texture(BakeShadowMap, f_texcoord1).rrr, 1);
		       fragColor *= A0Tex;

			   //For this it will need a frame buffer to be used
			   vec4 ShadowTex = vec4(texture(BakeShadowMap, f_texcoord1).ggg, 1);
		 }
	}

   //Mario Odyssey uses this. Often for fur colors
   if (base_color_mul_color != vec4(0))
       fragColor *= min(base_color_mul_color, 1);

    if (renderVertColor == 1)
	    fragColor *= min(color, vec4(1));

    // Fragment alpha calculations.
    fragColor.a *= texture(tex0, displayTexCoord).a;

    if (renderType == 1) // normals vertexColor
    {
        vec3 displayNormal = (bumpMapNormal * 0.5) + 0.5;
        fragColor = vec4(displayNormal,1);
    }
    else if (renderType == 2) // Lighting
    {
        float halfLambert = dot(difLightDirection, bumpMapNormal) * 0.5 + 0.5;
        fragColor = vec4(vec3(halfLambert), 1);
    }
	else if (renderType == 4) //Display Normal
	{
		if (uking_texture2_texcoord == 1)
            fragColor.rgb = texture(nrm, f_texcoord1).rgb;
		else
            fragColor.rgb = texture(nrm, displayTexCoord).rgb;
	}
	else if (renderType == 3) //DiffuseColor
	    fragColor = vec4(texture(tex0, displayTexCoord).rgb, 1);
    else if (renderType == 5) // vertexColor
        fragColor = color;
	else if (renderType == 6) //Display Ambient Occlusion
	{
	    if (HasShadowMap == 1)
        {
            float ambientOcclusionBlend = AmbientOcclusionBlend();
            fragColor = vec4(vec3(ambientOcclusionBlend), 1);
        }
		else
        {
            fragColor = vec4(1);
        }
	}
    else if (renderType == 7) // uv coords
        fragColor = vec4(displayTexCoord.x, displayTexCoord.y, 1, 1);
    else if (renderType == 8) // uv test pattern
	{
        fragColor = vec4(texture(UVTestPattern, displayTexCoord).rgb, 1);
	}
    else if (renderType == 9) //Display tangents
    {
        vec3 displayTangent = (tangent * 0.5) + 0.5;
        if (dot(tangent, vec3(1)) == 0)
            displayTangent = vec3(0);

        fragColor = vec4(displayTangent,1);
    }
    else if (renderType == 10) //Display bitangents
    {
        vec3 displayBitangent = (bitangent * 0.5) + 0.5;
        if (dot(bitangent, vec3(1)) == 0)
            displayBitangent = vec3(0);

        fragColor = vec4(displayBitangent,1);
    }
    else if (renderType == 11) //Display lights from second bake map if exists
	{
		vec4 AmbientOcc = vec4(texture(BakeShadowMap, f_texcoord1).rrr, 1);
		vec4 ColorMain = vec4(((33,33,33 * alpha * diffuseMapColor)).xyz, 1.0);

		vec3 LightHDRScale = vec3(1);

		vec3 LColor = vec3(1);

		if (LightMapColor.a > 0.9)
		{
		    LColor = vec3(1);
		}
		else
		{
		    LColor = LightMapColor.rgb;
		}

	    if (HasLightMap == 1)
		{
		    vec3 LightMap = LightHDRScale * LColor * LightMapColor.a;
		    fragColor = AmbientOcc * ColorMain * vec4(LightMap, 1) ;
		}

        if (HasEmissionMap == 1)
	    {
	        vec3 emission = texture2D(EmissionMap, displayTexCoord).rgb * vec3(1);
	        fragColor.rgb += emission.rgb;
	    }
	}

    // Toggles rendering of individual color channels for all render modes.
    fragColor.rgb *= vec3(renderR, renderG, renderB);
    if (renderR == 1 && renderG == 0 && renderB == 0)
        fragColor.rgb = fragColor.rrr;
    else if (renderG == 1 && renderR == 0 && renderB == 0)
        fragColor.rgb = fragColor.ggg;
    else if (renderB == 1 && renderR == 0 && renderG == 0)
        fragColor.rgb = fragColor.bbb;

    if (renderAlpha != 1 || isTransparent != 1)
        fragColor.a = 1;

    if (renderType == 12)
        fragColor.rgb = boneWeightsColored;
}
