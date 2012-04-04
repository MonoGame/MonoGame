uniform lowp vec3 FogColor;

varying lowp vec4 Diffuse;
varying lowp vec4 Specular;

void main()
{
    gl_FragColor = vec4(mix(Diffuse.rgb, FogColor * Diffuse.a, Specular.w), Diffuse.a);
}

