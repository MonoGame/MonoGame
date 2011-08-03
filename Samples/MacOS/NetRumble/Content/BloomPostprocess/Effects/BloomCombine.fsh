uniform sampler2D BloomSampler;
uniform sampler2D BaseSampler;

uniform float BloomIntensity;
uniform float BaseIntensity;

uniform float BloomSaturation;
uniform float BaseSaturation;

// Helper for modifying the saturation of a color.
vec4 AdjustSaturation(vec4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    //float grey = dot(color, vec3(0.3, 0.59, 0.11));

    //return lerp(grey, color, saturation);
	vec3 grayXfer = vec3(0.3, 0.59, 0.11);
	vec3 color3 = vec3(color);
	vec3 gray = vec3(dot(color3, grayXfer));
	//return vec4(mix(color, gray, Desaturation), 1.0);
	return vec4(mix(gray, color3, saturation), 1.0);
}

void main()
{
	// Look up the bloom and original base image colors.
	vec4 bloom = gl_Color * texture2D(BloomSampler, gl_TexCoord[0].xy);
	vec4 base = gl_Color * texture2D(BaseSampler, gl_TexCoord[0].xy);	
	// Adjust color saturation and intensity.
	bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
	base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
	
   // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    //base *= (1 - saturate(bloom));
	base *= clamp(bloom, 0.0, 1.0);
    // Combine the two images.
    gl_FragColor = base + bloom;
	//gl_FragColor = gl_Color * bloom; //texture2D(TextureSampler, 
}
