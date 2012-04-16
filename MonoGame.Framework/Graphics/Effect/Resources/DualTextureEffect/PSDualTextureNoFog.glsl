uniform lowp sampler2D Texture;
uniform lowp sampler2D Texture2;

varying lowp vec4 Diffuse;
varying mediump vec2 TexCoord;
varying mediump vec2 TexCoord2;

void main()
{
    lowp vec4 color = texture2D(Texture, TexCoord);
    lowp vec4 overlay = texture2D(Texture2, TexCoord2);
    color.rgb *= vec3(2);
    color *= overlay * Diffuse;
    gl_FragColor = color;
}