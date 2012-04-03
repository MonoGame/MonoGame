varying vec4 Diffuse;
varying vec4 Specular;

void main()
{
    gl_FragColor = vec4(Diffuse.rgb + Specular.rgb * Diffuse.a, Diffuse.a);
}