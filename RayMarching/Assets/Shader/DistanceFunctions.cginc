// Sphere
// s: radius
float sdSphere(float3 p, float s)
{
	return length(p) - s;
}

float maxSdSphere(float3 p, float s)
{
	return -length(p) + s;
}

// Box
// b: size of box in x/y/z
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) +
		length(max(d, 0.0));
}

float sdRoundBox(in float3 p, in float3 b, in float3 r)
{
    float3 q = abs(p) - b;
    return min(max(q.x, max(q.y, q.z)), 0.0) + length(max(q, 0.0)) - r;
}

float sdPlane(float3 p, float3 n, float h)
{
  // n must be normalized
  return dot(p,n) + h;
}

// BOOLEAN OPERATORS //

// Union
float4 opU(float4 d1, float4 d2)
{
	return (d1.w < d2.w ? d1 : d2);
}

float4 opUS( float4 d1, float4 d2, float k ) 
{
    float h = clamp( 0.5 + 0.5*(d2.w-d1.w)/k, 0.0, 1.0 );
    float3 color = lerp(d2.rgb, d1.rgb, h);
    float dist = lerp( d2.w, d1.w, h) - k*h*(1.0-h);
    return float4(color,dist);
}

// Subtraction
float opS(float d1, float d2)
{
	return max(-d1, d2);
}

float opSS( float d1, float d2, float k ) 
{
    float h = clamp( 0.5 - 0.5*(d2+d1)/k, 0.0, 1.0 );
    return lerp( d2, -d1, h ) + k*h*(1.0-h); 
}

// Intersection
float opI(float d1, float d2)
{
	return max(d1, d2);
}


float opIS( float d1, float d2, float k ) 
{
    float h = clamp( 0.5 - 0.5*(d2-d1)/k, 0.0, 1.0 );
    return lerp( d2, d1, h ) + k*h*(1.0-h); 
}

// Mod Position Axis
float pMod1 (inout float p, float size)
{
	float halfsize = size * 0.5;
	float c = floor((p+halfsize)/size);
	p = fmod(p+halfsize,size)-halfsize;
	p = fmod(-p+halfsize,size)-halfsize;
	return c;
}

