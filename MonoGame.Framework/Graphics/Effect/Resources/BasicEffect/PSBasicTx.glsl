uniform lowp sampler2D Texture;
uniform lowp vec3 FogColor;

varying lowp vec4 Diffuse;
varying lowp vec4 Specular;
varying mediump vec2 TexCoord;

void main()
{
    lowp vec4 color = texture2D(Texture, TexCoord) * Diffuse;
    color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
    
    gl_FragColor = color;
}
