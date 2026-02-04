#version 330 compatibility

#define NUM_OCTAVES 6

in vec2 fragTexCoord;

uniform sampler2D texture0;

uniform vec3 color;
uniform float time;
uniform int type;

uniform sampler2D tex_noise1;
uniform sampler2D tex_noise2;
uniform sampler2D tex_noise3;
uniform sampler2D tex_noise4;
uniform sampler2D tex_noise5;
uniform sampler2D tex_noise6;



float avg(vec4 color) 
{
    return (color.r + color.g + color.b)/3.0;
}

float hash1( float n ) { return fract(sin(n)*43758.5453); }
vec2  hash2( vec2  p ) { p = vec2( dot(p,vec2(127.1,311.7)), dot(p,vec2(269.5,183.3)) ); return fract(sin(p)*43758.5453); }

// The parameter w controls the smoothness
vec4 voronoi( in vec2 x, float w ,float di, float _time)
{
    vec2 n = floor( x );
    vec2 f = fract( x );
	vec4 m = vec4( 8.0, 0.0, 0.0, 0.0 );
    for( int j=-2; j<=2; j++ )
    for( int i=-2; i<=2; i++ )
    {
        vec2 g = vec2( float(i),float(j) );
        vec2 o = hash2( n + g );	
		// animate
        o =0.5*sin( _time + 6.2831*o );
        // distance to cell		
		float d = 1.2*length(g - f + o);
		
        // cell color
		vec3 col = 0.2*vec3(1,2,7)-0.2*vec3(1,0,0);
        // in linear space
        col = col;
        
        // do the smooth min for colors and distances		
		float h = smoothstep( -1.5, 0.3, (m.x-di*d)/w );
	    m.x   = mix( m.x,     d, h ) - h*(1.0-h)*w/(1.0+3.0*w); // distance
		m.yzw = mix( m.yzw, col, h ) + h*(1.0-h)*w/(1.0+3.0*w); // color
    }
	
	return m;
}

// random2 function by Patricio Gonzalez
vec2 random2( vec2 p )
{
    return fract(sin(vec2(dot(p,vec2(127.1,311.7)),dot(p,vec2(269.5,183.3))))*43758.5453);
}

// Value Noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/lsf3WH
float noise(vec2 st) 
{
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ), 
                     dot( random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ), 
                     dot( random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

vec3 magmaFunc(vec3 color, vec2 uv, float detail, float power, float colorMul, float glowRate, bool animate, float noiseAmount)
{
    vec3 rockColor = vec3(0.09 + abs(sin(time * .75)) * .03, 0.02, .02);
    float minDistance = 1.;
    uv *= detail;
    
    vec2 cell = floor(uv);
    vec2 frac = fract(uv);
    
    for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <= 1; j++) {
        	vec2 cellDir = vec2(float(i), float(j));
            vec2 randPoint = random2(cell + cellDir);
            randPoint += noise(uv) * noiseAmount;
            randPoint = animate ? 0.5 + 0.5 * sin(time * .35 + 6.2831 * randPoint) : randPoint;
            minDistance = min(minDistance, length(cellDir + randPoint - frac));
        }
    }
    	
    float powAdd = sin(uv.x * 2. + time * glowRate) + sin(uv.y * 2. + time * glowRate);
	vec3 outColor = vec3(color * pow(minDistance, power + powAdd * .95) * colorMul);
    outColor.rgb = mix(rockColor, outColor.rgb, minDistance);
    return outColor;
}


float fbm(vec2 p){
    float v=0.,a=0.5;
    vec2 s=vec2(100.);
    mat2 r=mat2(cos(0.5),sin(0.5),-sin(0.5),cos(0.5));
    for(int i=0;i<NUM_OCTAVES;i++){
        float d=mod(float(i),2.)>0.5?1.:-1.;
        v+=a*noise(p-0.05*d*time);
        p=r*p*2.+s;
        a*=0.5;
    }
    return v;
}


void main()
{
	vec4 realPixel = texture2D(texture0, fragTexCoord);
	
	if(realPixel.a < 0.3)
		discard;

	if(type == 0) // NORMALE
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);
		realPixel.rgb *= baseColor;
	}

	if(type == 1) // PODEROSO
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);

		vec4 noiseTex = texture2D(tex_noise1, fragTexCoord);
		noiseTex.rgb = vec3(step(noiseTex.r, 0.5));

		realPixel.rgb *= baseColor;
		realPixel.rgb = mix(realPixel.rgb, noiseTex.rgb, 0.2);
	}
	

	if(type == 2) // FLUVIALE
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);

		vec2  p = fragTexCoord;
		float c = 1.0;
		float scale = 4.0;
		vec4 v1 = voronoi( 0.7*scale*p, 0.3,1.5, time );
		vec4 v2 = voronoi( 2.*scale*p, 0.3,0.8, time  );
		vec4 v3 = voronoi( 4.*scale*p, 0.3,0.4, time  );
		vec4 v = (2.*v1+0.5*v2+ 0.5*v3)/3.;

		vec3 col = sqrt(v.yzw);

		if ((col.g > 0.64) && (col.g < 0.655))
			col = 1.0*vec3( 0.239, 0.714, 0.984);
		else if (col.g >= 0.655) 
			col = 1.45*vec3( 0.239, 0.714, 0.984);    
	
		realPixel = mix(realPixel, vec4( col, 1.0 ), 0.4);
	}
	

	if(type == 3) // GLACIALE
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);

		vec2  p = fragTexCoord;
		float c = 1.0;
		float scale = 8;
		vec4 v1 = voronoi( 0.7*scale*p, 0.3,1.5, 0 );
		vec4 v2 = voronoi( 2.*scale*p, 0.3,0.8, 0  );
		vec4 v3 = voronoi( 4.*scale*p, 0.3,0.4, 0  );
		vec4 v = (2.*v1+0.5*v2+ 0.5*v3)/3.;

		vec3 col = sqrt(v.yzw);

		if ((col.g > 0.64) && (col.g < 0.655))
			col = 1.0*vec3( 0.239, 0.714, 0.984);
		else if (col.g >= 0.655) 
			col = 1.45*vec3( 0.239, 0.714, 0.984);    
	
		realPixel = mix(realPixel, vec4( col, 1.0 ), 0.6);
	}
	

	if(type == 4) // MAGMATICO
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);
		vec2 uv = fragTexCoord;
		uv.x += time * .01;
		vec4 magmaPixel = vec4(0.);
		magmaPixel.rgb += magmaFunc(vec3(1.5, .45, 0.), uv, 3.,  2.5, 1.15, 1.5, false, 1.5);
		magmaPixel.rgb += magmaFunc(vec3(1.5, 0., 0.), uv, 6., 3., .4, 1., false, 0.);
		magmaPixel.rgb += magmaFunc(vec3(1.2, .4, 0.), uv, 8., 4., .2, 1.9, true, 0.5);
		magmaPixel.a = 1.0;

		realPixel = mix(realPixel, magmaPixel, 0.8);
	}
	

	if(type == 5) // PURO
	{
		vec3 baseColor = vec3(1.710, 1.588, 1.588);
		vec4 noiseTex = texture2D(tex_noise4, fragTexCoord);

		realPixel.rgb *= baseColor;

		realPixel.rgb = mix(realPixel.rgb, noiseTex.rgb, 0.5);
	}
	

	if(type == 6) // FLORIDO
	{
		vec3 baseColor = vec3(0.267, 0.761, 0.349);
		vec4 noiseTex = texture2D(tex_noise5, fragTexCoord);

		realPixel.rgb *= baseColor;

		realPixel.rgb = mix(realPixel.rgb, noiseTex.rgb, 0.3);
	}
	

	if(type == 7) // RAPIDO
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);
		realPixel.rgb *= baseColor;
	}
	

	if(type == 8) // ANTICO
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);
		realPixel.rgb *= baseColor;
	}
	

	if(type == 9) // COSMICO
	{
		vec3 baseColor = vec3(0.710, 0.588, 0.588);
		realPixel.rgb *= baseColor;

		vec2 p=fragTexCoord-vec2(12.,0.);
		vec2 q=vec2(fbm(p),fbm(p+vec2(1.))),r=vec2(fbm(p+q+vec2(1.7,1.2)+0.15),fbm(p+q+vec2(8.3,2.8)+0.126));
		float f=fbm(p+r);
		vec3 c=mix(vec3(1.,1.,2.),vec3(1.),clamp(f*f*5.5,1.2,15.5));
		c=mix(c,vec3(1.),clamp(length(q),2.,2.));
		c=mix(c,vec3(0.3,0.2,1.),clamp(r.x,0.,5.));
		c=(f*f*f+0.9*f)*c;
		vec2 uv=fragTexCoord;
		float alpha=50.-max(pow(100.*distance(uv.x,-1.),0.),pow(2.*distance(uv.y,0.5),5.));
		vec4 cosmicColor = realPixel+(vec4(c,alpha*c.r)* vec4(0.682, 0.439, 0.878, 1.0));

		realPixel = mix(realPixel, cosmicColor, 0.5);
	}


	gl_FragColor = realPixel;
}
