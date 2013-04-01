float4x4 _World : WORLD;
float4x4 _View : VIEW;
float4x4 _Projection : PROJECTION;
float4x4 _WorldView : WORLDVIEW;

float2 Viewport;

sampler image : register(s0);

struct VertexOutput
{
	float4 position: POSITION0;
	float4 color: COLOR0;

	float2 c00: TEXCOORD1; 
    float2 c01: TEXCOORD2; 
    float2 c02: TEXCOORD3;
    float2 c10: TEXCOORD4;
    float2 c11: TEXCOORD5; 
    float2 c12: TEXCOORD6; 
    float2 c20: TEXCOORD7;
    float2 c21: NORMAL0; 
    float2 c22: TANGENT0;
};

VertexOutput vertex(float4 position	: POSITION, float4 color: COLOR0, float2 tex : TEXCOORD)
{
	VertexOutput output;
	
	output.position = position;

	// Half pixel offset for correct texel centering.
	output.position.xy -= 0.5;

	// Viewport adjustment.
	output.position.xy /= Viewport;
	output.position.xy *= float2(2, -2);
	output.position.xy -= float2(1, -1);

	output.color = color;

	float2 delta = 0.5 / Viewport;
	float dx = delta.x;
	float dy = delta.y;

    output.c00 = tex + float2(-dx, -dy);
    output.c01 = tex + float2(-dx, 0);
    output.c02 = tex + float2(-dx, dy);
    output.c10 = tex + float2(0, -dy);
    output.c11 = tex + float2(0, 0);
    output.c12 = tex + float2(0, dy);
    output.c20 = tex + float2(dx, -dy);
    output.c21 = tex + float2(dx, 0);
    output.c22 = tex + float2(dx, dy);

	return output;
}

const float mx = 0.325;      // start smoothing wt.
const float k = -0.250;      // wt. decrease factor
const float max_w = 0.25;    // max filter weigth
const float min_w = -0.05;    // min filter weigth
const float lum_add = 0.25;  // effects smoothing

float4 pixel(VertexOutput input) : COLOR0
{
   float3 c00 = tex2D(image, input.c00).xyz;
   float3 c01 = tex2D(image, input.c01).xyz;
   float3 c02 = tex2D(image, input.c02).xyz;
   float3 c10 = tex2D(image, input.c10).xyz;
   float3 c11 = tex2D(image, input.c11).xyz;
   float3 c12 = tex2D(image, input.c12).xyz;
   float3 c20 = tex2D(image, input.c20).xyz;
   float3 c21 = tex2D(image, input.c21).xyz;
   float3 c22 = tex2D(image, input.c22).xyz;
   float3 dt = float3(1.0, 1.0, 1.0);

   float md1 = dot(abs(c00 - c22), dt);
   float md2 = dot(abs(c02 - c20), dt);

   float w1 = dot(abs(c22 - c11), dt) * md2;
   float w2 = dot(abs(c02 - c11), dt) * md1;
   float w3 = dot(abs(c00 - c11), dt) * md2;
   float w4 = dot(abs(c20 - c11), dt) * md1;

   float t1 = w1 + w3;
   float t2 = w2 + w4;
   float ww = max(t1, t2) + 0.0001;

   c11 = (w1 * c00 + w2 * c20 + w3 * c22 + w4 * c02 + ww * c11) / (t1 + t2 + ww);

   float lc1 = k / (0.12 * dot(c10 + c12 + c11, dt) + lum_add);
   float lc2 = k / (0.12 * dot(c01 + c21 + c11, dt) + lum_add);

   w1 = clamp(lc1 * dot(abs(c11 - c10), dt) + mx, min_w, max_w);
   w2 = clamp(lc2 * dot(abs(c11 - c21), dt) + mx, min_w, max_w);
   w3 = clamp(lc1 * dot(abs(c11 - c12), dt) + mx, min_w, max_w);
   w4 = clamp(lc2 * dot(abs(c11 - c01), dt) + mx, min_w, max_w);

   return float4(w1 * c10 + w2 * c21 + w3 * c12 + w4 * c01 + (1.0 - w1 - w2 - w3 - w4) * c11, 1.0);   
}

technique Scale
{
    pass P0
    {
		// shaders
		VertexShader = compile vs_3_0 vertex();
		PixelShader  = compile ps_3_0 pixel();
    }  
}
