uniform sampler2D Texture;

// Camera parameters.
uniform mat4 View;
uniform mat4 Projection;
uniform vec2 ViewportScale;


// The current time, in seconds.
uniform float CurrentTime;


// Parameters describing how the particles animate.
uniform float Duration;
uniform float DurationRandomness;
uniform vec3 Gravity;
uniform float EndVelocity;
uniform vec4 MinColor;
uniform vec4 MaxColor;


// These float2 parameters describe the min and max of a range.
// The actual value is chosen differently for each particle,
// interpolating between x and y by some random amount.
uniform vec2 RotateSpeed;
uniform vec2 StartSize;
uniform vec2 EndSize;

void main()
{
	mat4 view = View;
	mat4 projection = Projection;
	vec2 viewportScale = ViewportScale;
	
	float currentTime = CurrentTime;
	
	float duration = Duration;
	float durationRandomness = DurationRandomness;
	vec3 gravity = Gravity;
	float endVelocity = EndVelocity;
	vec4 minColor = MinColor;
	vec4 maxColor = MaxColor;
	
	vec2 rotateSpeed = RotateSpeed;
	vec2 startSize = StartSize;
	vec2 endSize = EndSize;
	
	gl_FrontColor = gl_Color;
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
}
