uniform sampler2D TextureSampler;

uniform vec2 Position;

void main()
{
    vec4 texCoord = vec4(gl_TexCoord[0].xy, 0, 0);

    texCoord.x  += (Position.x  * 0.000250000);
    texCoord.y  += (Position.y  * 0.000250000);
    texCoord *= 0.500000;
    vec4 results = (vec4( 0.000000, 0.000000, 1.00000, 0.250000) * texture2D( TextureSampler, texCoord.xy));
    texCoord.x  += ((Position.x  * 0.000250000) + 0.250000);
    texCoord.y  += ((Position.y  * 0.000250000) - 0.150000);
    texCoord *= 0.400000;
    results += (vec4( 0.000000, 1.00000, 0.000000, 0.150000) * texture2D( TextureSampler, texCoord.xy));
    gl_FragColor = results;
}