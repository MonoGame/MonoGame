uniform vec3 FogColor;

varying vec4 Diffuse;
varying vec4 Specular;

void main()
{
    vec4 color = Diffuse;
    color.rgb += Specular.rgb * color.a;
    color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
    
    gl_FragColor = color;
}