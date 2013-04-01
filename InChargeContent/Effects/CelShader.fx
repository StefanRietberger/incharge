/* ********************************************************
 * A Simple toon shader based on the work of Petri T. Wilhelmsen
 * found on his blog post XNA Shader Programming – Tutorial 7, Toon shading
 * http://digitalerr0r.wordpress.com/2009/03/22/xna-shader-programming-tutorial-7-toon-shading/
 *
 * Author: John Marquiss
 * Email: txg1152@gmail.com
 *  
 * This work by John Marquiss is licensed under a 
 * Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
 * http://creativecommons.org/licenses/by-nc-sa/3.0/
 */

sampler ColorMapSampler : register(s0);


texture CelMap;

/* Cel Shader effect map
 * Mapping to the cel shader texture is what
 * gives us that classic cel shaded effect
 */
sampler2D CelMapSampler = sampler_state
{
	Texture = <CelMap>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
};

struct VertexShaderOutput
{
    float2 Tex : TEXCOORD0;
};

/* getGray
 * a simple helper function to return a grey scale
 * value for a given pixel
 */
float getGray(float4 c)
{
	/* The closer a color is to a pure gray
	 * value the closer its dot product and gray
	 * will be to 0.
	 */
	return(dot(c.rgb,float3(0.3f, 0.59f, 0.11f)));
}

/* Shade each pixel
 * Get the current target pixle color from the ColorMap
 * Find the target shading:
 *   The dot product of two vectors ranges from 0 to 1, the
 *     greater the angel between the two vectors the closer the dot
 *     product is to 0.
 *   The cel shading texture is one pixel high so the cel texture
 *     coordinate will always be 0.
 *   Map the resulting cel texture coordinate onto the cel shading
 *     map to get the target gray value.
 * Once we have the target add the texture color multiplied by Ac*Ai
 * to the cel shading light color scaled by Di.
 */
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Get our target pixel color
	float4 Color = tex2D(ColorMapSampler, input.Tex);
	// Set our source color tinting (this should be an effect parameter)
	float Ai = 0.8f;	
	float4 Ac = float4(0.075, 0.075, 0.2, 1.0);

	// Shader scaling (should be an effect parameter)
	float Di = 1.0f;

	// Look up the cel shading light color
	float2 celTexCoord = float2(getGray(Color), 0.0f);
	float4 CelColor = tex2D(CelMapSampler, celTexCoord);

	// return the final pixel color
    return (Ai*Ac*Color)+(Color*Di*CelColor);
}

technique ToonShader
{
    pass Pass0
    {
        Sampler[0] = (ColorMapSampler);
		Sampler[1] = (CelMapSampler);

        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
