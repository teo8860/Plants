#version 330

in vec2 fragTexCoord;

uniform vec3 customColor;


void main()
{
	vec4 finalColor;

	if(fragTexCoord.x > 0.5 )
		finalColor = vec4(0.0, 1.0, 0.0, 1.0);
	else
		finalColor = vec4(1.0, 0.0, 0.0, 1.0);

	finalColor.a = 0.5;

	gl_FragColor = finalColor;
}
