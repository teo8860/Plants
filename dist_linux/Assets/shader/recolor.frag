#version 330 compatibility

in vec2 fragTexCoord;
out vec4 outColor;

uniform vec3 customColor;


void main()
{
	vec4 finalColor;

	if(fragTexCoord.x > 0.5 )
		finalColor = vec4(0.0, 1.0, 0.0, 1.0);
	else
		finalColor = vec4(1.0, 0.0, 0.0, 1.0);

	finalColor.a = 0.5;

	outColor = finalColor;
}
