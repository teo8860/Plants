#version 330

in vec2 fragTexCoord;

uniform vec3 customColor;
uniform sampler2D texture0;


void main()
{
	vec4 realPixel = texture2D(texture0, fragTexCoord);

	float brightness = (realPixel.r + realPixel.g + realPixel.b) / 3.0;

	if(brightness < 0.25)
		realPixel.r = 1.0;
		
	if(brightness > 0.9)
		realPixel.b = 1.0;

	gl_FragColor = realPixel;
}
