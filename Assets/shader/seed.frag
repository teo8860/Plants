#version 330 compatibility

in vec2 fragTexCoord;

uniform sampler2D texture0;

uniform float time;
uniform vec3 color;
uniform sampler2D noise;

void main()
{
	vec4 realPixel = texture2D(texture0, fragTexCoord);
	vec4 noisePixel = texture2D(noise, fragTexCoord);

	if(realPixel.a < 0.3)
		discard;

	noisePixel.a = 0.9;

	realPixel *= noisePixel;
	realPixel.rgb += color;
	
	

	gl_FragColor = realPixel;
}
