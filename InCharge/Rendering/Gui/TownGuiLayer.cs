using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering;
using Indiefreaks.Xna.Core;
using Microsoft.Xna.Framework.Content;
using InCharge.Logic;
using Indiefreaks.Xna.Rendering.Gui;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using InCharge.UI;

namespace InCharge.Rendering.Gui
{
    public class TownGuiLayer : Layer, IContentHost
    {
        private IGameController controller;
        private TownGui gui;
        private SpriteBatch spriteBatch;

        public TownGuiLayer(GameState gameState, IGameController controller)
            : base(gameState)
        {
            this.controller = controller;
        }

        public override void Initialize()
        {
            base.Initialize();
            this.spriteBatch = new SpriteBatch(Application.Graphics.GraphicsDevice);

            this.gui = new TownGui(Application.Input.PlayerOne, controller);

            Application.SunBurn.GetManager<IGuiManager>(true).AddScreen(this.gui);
            this.gui.SetFocus();
            GameState.PreLoad(this.gui);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.gui.Texture != null)
            {
                this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                this.spriteBatch.Draw(this.gui.Texture, Vector2.Zero, Color.White);
                this.spriteBatch.End();
            }
        }


        #region IContentHost Members

        public void LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {
            catalogue.Add(this.gui);
        }

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.gui);
        }

        #endregion
    }
}
