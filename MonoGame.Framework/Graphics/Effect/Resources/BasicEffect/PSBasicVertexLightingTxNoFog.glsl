uniform sampler2D Texture;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

void main()
{
    vec4 color = texture2D(Texture, TexCoord) * Diffuse;
    color.rgb += Specular.rgb * color.a;
    
    gl_FragColor = color;
}