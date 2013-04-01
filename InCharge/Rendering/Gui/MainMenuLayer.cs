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

        public MainMenuLayer(GameState gameState)
            : base(gameState)
        {
            this.spriteBatch = new SpriteBatch(Application.Graphics.GraphicsDevice);
            this.rtPass1 = new RenderTarget2D(
                Application.Graphics.GraphicsDevice,
                Application.Graphics.GraphicsDevice.Viewport.Width >> 1,
                Application.Graphics.GraphicsDevice.Viewport.Height >> 1,
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
        }

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.rootMainMenu);
        }
        #endregion

        public override void BeginDraw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.rtPass1);
            base.BeginDraw(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {            
            base.Draw(gameTime);

            // scale and render to back buffer
            Application.Graphics.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.scaleEffect);
            spriteBatch.Draw(this.rootMainMenu.Texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
    }
}
