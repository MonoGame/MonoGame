uniform vec3 FogColor;

varying vec4 Diffuse;
varying vec4 Specular;

void main()
{
    gl_FragColor = vec4(mix(Diffuse.rgb, FogColor * Diffuse.a, Specular.w), Diffuse.a);
}

