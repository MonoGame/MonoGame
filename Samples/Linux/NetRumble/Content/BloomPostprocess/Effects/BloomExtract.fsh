uniform sampler2D TextureSampler;
uniform float BloomThreshold;

void main()
{
  vec4 c = texture2D(TextureSampler, gl_TexCoord[0].xy);
  gl_FragColor = clamp((c - BloomThreshold )/ (1.0-BloomThreshold),0.0, 1.0);
}

