uniform lowp sampler2D Texture;

varying lowp vec4 Diffuse;
varying lowp vec4 Specular;
varying mediump vec2 TexCoord;

void main()
{
    lowp vec4 color = texture2D(Texture, TexCoord) * Diffuse;
    color.rgb += Specular.rgb * color.a;
    
    gl_FragColor = color;
}