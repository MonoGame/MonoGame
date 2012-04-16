uniform lowp vec3 FogColor;

varying lowp vec4 Diffuse;
varying lowp vec4 Specular;

void main()
{
    lowp vec4 color = Diffuse;
    color.rgb += Specular.rgb * color.a;
    color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
    
    gl_FragColor = color;
}