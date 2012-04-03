uniform sampler2D Texture;
uniform vec3 FogColor;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

void main()
{
    vec4 color = texture2D(Texture, TexCoord) * Diffuse;
    color.rgb += Specular.rgb * color.a;
    color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
    
    gl_FragColor = color;
}
