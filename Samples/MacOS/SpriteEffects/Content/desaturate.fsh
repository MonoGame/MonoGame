uniform sampler2D TextureSampler;

vec4 Desaturate(vec3 color, float Desaturation)
{
	vec3 grayXfer = vec3(0.3, 0.59, 0.11);
	vec3 gray = vec3(dot(grayXfer, color));

	//return vec4(mix(color, gray, Desaturation), 1.0);
	return vec4(mix(gray, color, Desaturation), 1.0);
}


void main()
{
	vec4 tex = texture2D(TextureSampler, gl_TexCoord[0].xy);
	vec4 color = Desaturate(tex.rgb, gl_Color.a * 4.0);
	
	gl_FragColor = color;

}
