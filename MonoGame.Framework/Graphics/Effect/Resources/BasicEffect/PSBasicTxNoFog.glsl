uniform lowp sampler2D Texture;

varying lowp vec4 Diffuse;
varying mediump vec2 TexCoord;

void main()
{
    gl_FragColor = texture2D(Texture, TexCoord) * Diffuse;
}