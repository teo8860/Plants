#version 330 compatibility

in vec2 fragTexCoord;
out vec4 outColor;
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

	outColor = realPixel;
}
