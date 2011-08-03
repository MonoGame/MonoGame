// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.
uniform sampler2D TextureSampler;
uniform vec2 SampleOffsets[15];
uniform float SampleWeights[15];


void main()
{
     vec4 c = vec4(0);
     for(int i=0; i < 1; i++) 
     {
          c += texture2D(TextureSampler, gl_TexCoord[0].xy + SampleOffsets[i]) * SampleWeights[i];
     }
	 gl_FragColor = c;
}

