//-----------------------------------------------
// Synapse Gaming - SunBurn Lighting System
// Copyright © Synapse Gaming 2008
//-----------------------------------------------


//-----------------------------------------------
// include the SunBurn deferred helper objects.
//

#include <..\ShaderLibrary\Deferred\DeferredHelper.fx>


//-----------------------------------------------
// main scene transforms - bound as common semantics to
// automatically receive data from SunBurn.
//

float4x4 _World : WORLD;
float4x4 _View : VIEW;
float4x4 _Projection : PROJECTION;
float4x4 _WorldView : WORLDVIEW;


//-----------------------------------------------
// lighting parameters - bound to SAS address and
// custom SunBurn semantics to automatically receive
// information from SunBurn, including the deferred
// lighting diffuse and specular textures.
//

float3 AmbientLighting
<
string SasBindAddress = "Sas.AmbientLight[0].Color";
> = 0;

texture2D SceneLightingDiffuseMap : SCENELIGHTINGDIFFUSEMAP;
texture2D SceneLightingSpecularMap : SCENELIGHTINGSPECULARMAP;

sampler SceneLightingDiffuseSampler = sampler_state
{
    Texture = <SceneLightingDiffuseMap>;
	MipFilter = NONE;
	AddressU  = Clamp;
    AddressV  = Clamp;
};

sampler SceneLightingSpecularSampler = sampler_state
{
    Texture = <SceneLightingSpecularMap>;
	MipFilter = NONE;
	AddressU  = Clamp;
    AddressV  = Clamp;
};


//-----------------------------------------------
// variables that control the blend effect - not bound to
// semantics or SAS addresses, making them visible to the
// editor.
//

// base texture
texture2D BaseTexture;

texture2D BlendNorth;
texture2D BlendEast;
texture2D BlendSouth;
texture2D BlendWest;

texture2D NormalMap;

sampler NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
};

sampler BlendNorthSampler = sampler_state
{
    Texture = <BlendNorth>;
};

sampler BlendEastSampler = sampler_state
{
    Texture = <BlendEast>;
};

sampler BlendSouthSampler = sampler_state
{
    Texture = <BlendSouth>;
};

sampler BlendWestSampler = sampler_state
{
    Texture = <BlendWest>;
};

sampler BaseSampler = sampler_state
{
    Texture = <BaseTexture>;
};


//-----------------------------------------------
// shader structures - for passing data between the model,
// vertex shader, and pixel shader.
//

struct InputData
{
	float4 position		: POSITION;
	float3 normal		: NORMAL;
	float4 uvCoord		: TEXCOORD0;
	float3 tangent		: TANGENT0;
	float3 binormal		: BINORMAL0;

	// atlas tex coords for blending tiles
	float4 atlasBlend12					: TEXCOORD1;
	float4 atlasBlend34					: TEXCOORD2;	
};

struct ShaderLink
{
	// used by the gpu for rasterizing the geometry.
    float4 position						: POSITION0;
	
	// used to sample diffuse textures.
	float4 uvCoord						: TEXCOORD0;
	
	// used to calculate depth and fog.
	float4 viewPosition 				: TEXCOORD1;
	
	// used by the g-buffer for lighting.
	float3 viewNormal					: TEXCOORD2;
	
	// used to multisample the deferred lighting textures.
	float4 projectionPosition 			: TEXCOORD3;
	float4 projectionPositionCentroid	: TEXCOORD4_centroid;

	// atlas tex coords for blending tiles
	float4 atlasBlend12					: TEXCOORD5;
	float4 atlasBlend34					: TEXCOORD6;

	float3x3 tangentToView				: TEXCOORD7;
};


//-----------------------------------------------
// common vertex shader for the techniques.
//

ShaderLink BlendMapVS(InputData input)
{
    ShaderLink output;
	
	
	// calculate data for the pixel shader.
	
	output.position = mul(input.position, _World);
	output.position = mul(output.position, _View);
	
	output.viewPosition = output.position;
	
	output.position = mul(output.position, _Projection);
	
	output.projectionPosition = output.position;
	output.projectionPositionCentroid = output.position;
	
	output.viewNormal = mul(input.normal, (float3x3)_World);
	output.viewNormal = mul(output.viewNormal, (float3x3)_View);	
	
	output.uvCoord = input.uvCoord;

	// pass on data for blending
	output.atlasBlend12 = input.atlasBlend12;
	output.atlasBlend34 = input.atlasBlend34;	
	
	float3x3 tangenttoobject = float3x3(input.tangent, input.binormal, input.normal.xyz);
	output.tangentToView = mul(tangenttoobject, (float3x3)_WorldView);	
    return output;
}


//-----------------------------------------------
// pixel shader used for fast z-pass rendering, which
// boosts performance of subsequent rendering.
//
// See the Depth technique below for more details.
//

float4 BlendMapDepthPassPS(ShaderLink input) : COLOR
{
	return 0;
}


//-----------------------------------------------
// pixel shader used for writing all data necessary for
// deferred rendering to the g-buffers.
//
// See the GBuffer technique below for more details.
//

SceneMRTData BlendMapGBufferPassPS(ShaderLink input)
{

	// deferred data.
	
	float depth = input.viewPosition.z / _FarClippingDistance;	

	float3 normalmap = tex2D(NormalMapSampler, input.uvCoord.xy);
	normalmap = mul(normalmap, input.tangentToView);
	float3 viewnormal = normalize(normalmap);
	
	
	// no spec/fres.
	
	float specpower = 0.0f;
	float3 fresnel_bias_offset_microfacet = 0.0f;
	
	
	// use the shader helper function to automatically pack deferred
	// data into the correct formats and g-buffers.
	
	return SaveSceneData(viewnormal, depth, specpower, fresnel_bias_offset_microfacet.xyz);
}


//-----------------------------------------------
// pixel shader used for rendering the final composed
// object to the scene.  This includes rendering material
// data, fog, and applying the pre-generated lighting (supplied
// by SunBurn's deferred rendering, and multisampled for
// anti-aliasing).
//
// See the Final technique below for more details.
//

float4 BlendMapFinalPassPS(ShaderLink input) : COLOR
{	
	// shader specific material calculations - generate the diffuse
	// using blend mapping.	
	
	float4 blendAlpha;
	float blendFactor = 1.0f;	
	float3 diffuse = float3(0.0f, 0.0f, 0.0f);

	// blend north neighbor
	if (input.atlasBlend12.x + input.atlasBlend12.y > 0)
	{
		blendAlpha = tex2D(BlendNorthSampler, input.uvCoord.zw);
		diffuse += blendAlpha.w * tex2D(BaseSampler, input.atlasBlend12.xy);
		blendFactor = saturate(blendFactor - blendAlpha.w);
	}
	
	// blend east neighbor
	if (input.atlasBlend12.z + input.atlasBlend12.w > 0)
	{
		blendAlpha = tex2D(BlendEastSampler, input.uvCoord.zw);
		diffuse += blendAlpha.w * tex2D(BaseSampler, input.atlasBlend12.zw) * blendFactor;
		blendFactor = saturate(blendFactor - blendAlpha.w);
	}
		
	// blend south neighbor
	if (input.atlasBlend34.x + input.atlasBlend34.y > 0)
	{
		blendAlpha = tex2D(BlendSouthSampler, input.uvCoord.zw);
		diffuse += blendAlpha.w * tex2D(BaseSampler, input.atlasBlend34.xy) * blendFactor;
		blendFactor = saturate(blendFactor - blendAlpha.w);
	}
		
	// blend west neighbor
	if (input.atlasBlend34.z + input.atlasBlend34.w > 0)
	{
		blendAlpha = tex2D(BlendWestSampler, input.uvCoord.zw);
		diffuse += blendAlpha.w * tex2D(BaseSampler, input.atlasBlend34.zw) * blendFactor;
		blendFactor = saturate(blendFactor - blendAlpha.w);
	}
			
	// base texture
	diffuse += tex2D(BaseSampler, input.uvCoord.xy) * blendFactor;
	
	// calculate the screen-space uv coordinates used to sample
	// from the full-screen deferred lighting textures.
	
	float2 screenuvlinear = GetScreenUV(input.projectionPosition);
	float2 screenuvcentroid = GetScreenUV(input.projectionPositionCentroid);
	
	
	// sample the pre-generated deferred lighting textures - use SunBurn's
	// multisampling helper function for anti-aliasing (only recommended for
	// deferred buffers).
	
	LightingMRTData data;
	data.lightingDiffuse = MultiSampleGBuffer(SceneLightingDiffuseSampler, screenuvlinear, screenuvcentroid);
	data.lightingSpecular = MultiSampleGBuffer(SceneLightingSpecularSampler, screenuvlinear, screenuvcentroid);
	
	
	// unpack the lighting data using SunBurn's helper function.
	
	float3 lightingdiffuse = 0.0f;
	float3 lightingspecular = 0.0f;
	
	LoadLightingData(data, lightingdiffuse, lightingspecular);
	
	
	// apply the unpacked lighting and SunBurn's automatic fog to the
	// diffuse, and return the full material color.
	
	float4 material = 0.0f;
	material.xyz = LightMaterial(diffuse, float3(0.0f, 0.0f, 0.0f), lightingdiffuse + AmbientLighting, lightingspecular);
	material.xyz = FogMaterial(material.xyz, input.viewPosition);
	
	return material;
}


//-----------------------------------------------
// technique used to apply a fast z-fill pass in order to boost
// performance of other deferred rendering passes.
//
// this technique does *not* need to render any particular output
// however it *does* need to perform alpha clipping (using
// SunBurn's "ClipTransparency" help function) on materials
// that utilize this feature.
//

technique BlendMap_Depth_Technique
{
    pass P0
    {
        VertexShader = compile vs_3_0 BlendMapVS();
        PixelShader  = compile ps_3_0 BlendMapDepthPassPS();
    }
}


//-----------------------------------------------
// technique used to write all data required for deferred rendering
// to the g-buffers (using SunBurn's "SaveSceneData" helper
// function for packing).
//

technique BlendMap_GBuffer_Technique
{
    pass P0
    {
        VertexShader = compile vs_3_0 BlendMapVS();
        PixelShader  = compile ps_3_0 BlendMapGBufferPassPS();
    }
}


//-----------------------------------------------
// technique used for rendering the final composed
// object to the scene.
//
// this technique renders basic material data (such
// as sampled diffuse textures), fog, and applies the
// pre-generated lighting (supplied by SunBurn's deferred
// rendering, and multisampled for anti-aliasing).
//

technique BlendMap_Final_Technique
{
    pass P0
    {
        VertexShader = compile vs_3_0 BlendMapVS();
        PixelShader  = compile ps_3_0 BlendMapFinalPassPS();
    }
}

