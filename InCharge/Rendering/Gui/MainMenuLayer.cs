using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace InCharge.Rendering.Gui
{
    public class MainMenuLayer : Layer, IContentHost
    {
        private Effect bloomExtract;
        private Effect bloomCombine;
        private Effect gaussianBlur;
        private readonly MainMenuGui rootMainMenu;        

        /// <summary>
        /// Sprite batch for simple texture drawing
        /// </summary>
        private SpriteBatch spriteBatch;

        // Effects
        private Effect scaleEffect;
        /// <summary>
        /// Render target for filter pass 1
        /// </summary>
        private RenderTarget2D rtPass1;
        /// <summary>
        /// Render target for filter pass 2
        /// </summary>
        private RenderTarget2D rtPass2;

        /// <summary>
        /// Changing bloom intensity
        /// </summary>
        private float bloomIntensity = 1.25f;
        private float bloomPhase = 0f;

        public MainMenuLayer(GameState gameState)
            : base(gameState)
        {
            this.spriteBatch = new SpriteBatch(Application.Graphics.GraphicsDevice);
            this.rtPass1 = new RenderTarget2D(
                Application.Graphics.GraphicsDevice,
                Application.Graphics.GraphicsDevice.Viewport.Width,
                Application.Graphics.GraphicsDevice.Viewport.Height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0,
                RenderTargetUsage.DiscardContents);
            this.rtPass2 = new RenderTarget2D(
                Application.Graphics.GraphicsDevice,
                Application.Graphics.GraphicsDevice.Viewport.Width,
                Application.Graphics.GraphicsDevice.Viewport.Height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0,
                RenderTargetUsage.DiscardContents);
            this.scaleEffect = Application.ContentManager.Load<Effect>("Effects/hq2x");
            this.scaleEffect.Parameters["Viewport"].SetValue(new Vector2(Application.Graphics.GraphicsDevice.Viewport.Width, Application.Graphics.GraphicsDevice.Viewport.Height));

            // we create our root main menu instance
            this.rootMainMenu = new MainMenuGui();            
            
            
            // we add it to the GuiManager so that it gets updated and rendered
            Application.SunBurn.GetManager<IGuiManager>(true).AddScreen(this.rootMainMenu);
            // and we set the input focus to it
            this.rootMainMenu.SetFocus();
            // Create render target on main menu which it renders to
            this.rootMainMenu.PrepareRenderTarget(Application.Graphics.GraphicsDevice);
        }

        #region Implementation of IContentHost

        public void LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {            
            // by adding the rootMainMenu instance to the catalogue, we'll get its inner buttons also added automatically. They'll all get loaded when required.
            catalogue.Add(this.rootMainMenu);

            this.bloomExtract = manager.Load<Effect>("Effects/BloomExtract");
            this.bloomCombine = manager.Load<Effect>("Effects/BloomCombine");
            this.gaussianBlur = manager.Load<Effect>("Effects/GaussianBlur");

            this.bloomExtract.Parameters["BloomThreshold"].SetValue(0.25f);
            this.bloomCombine.Parameters["BaseIntensity"].SetValue(1.0f);
            this.bloomCombine.Parameters["BloomSaturation"].SetValue(1.0f);
            this.bloomCombine.Parameters["BaseSaturation"].SetValue(1.0f);
        }

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.rootMainMenu);
            this.bloomExtract.Dispose();
            this.bloomCombine.Dispose();
            this.gaussianBlur.Dispose();
        }
        #endregion

        public override void BeginDraw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.BeginDraw(gameTime);

            // misc. update code:
            //this.rootMainMenu.Update(gameTime);
            this.bloomPhase += gameTime.ElapsedGameTime.Milliseconds * 0.004f;
            this.bloomPhase %= MathHelper.TwoPi;
            this.bloomIntensity = 1.25f + 0.25f * (float)Math.Sin(this.bloomPhase);           
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            // apply bloom                      
            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.rtPass1);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.bloomExtract);
            spriteBatch.Draw(this.rootMainMenu.Texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
          

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.

            SetBlurEffectParameters(1.0f / (float)this.rtPass1.Width, 0);
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.rtPass2);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.gaussianBlur);
            spriteBatch.Draw(this.rtPass1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();           

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)this.rtPass1.Height);
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.rtPass1);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.gaussianBlur);
            spriteBatch.Draw(this.rtPass2, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.rtPass2);
            Application.Graphics.GraphicsDevice.Textures[1] = this.rootMainMenu.Texture;
            this.bloomCombine.Parameters["BloomIntensity"].SetValue(this.bloomIntensity);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.bloomCombine);
            spriteBatch.Draw(this.rtPass1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();

            // scale and render to back buffer
            Application.Graphics.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, this.scaleEffect);
            spriteBatch.Draw(this.rtPass2, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = this.gaussianBlur.Parameters["SampleWeights"];
            offsetsParameter = this.gaussianBlur.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = 4.0f;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }        
    }    
}
