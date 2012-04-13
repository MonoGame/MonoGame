uniform sampler2D Texture;
uniform sampler2D Texture2;
uniform vec3 FogColor;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;
varying vec2 TexCoord2;

void main()
{
    vec4 color = texture2D(Texture, TexCoord);
    vec4 overlay = texture2D(Texture2, TexCoord2);
    color.rgb *= vec3(2);
    color *= overlay * Diffuse;
    color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
    gl_FragColor = color;
}