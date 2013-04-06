using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Indiefreaks.Xna.Core;

namespace InCharge.Rendering.Gui
{
    public class TwinkleStar: Control, IUpdate
    {
        private static Random random = new Random();
        private static readonly float interval = 20f;

        private readonly string _texturePathBig;
        private readonly string _texturePathMedium;
        private readonly string _texturePathSmall;
        private Texture2D _textureBig;
        private Texture2D _textureMedium;
        private Texture2D _textureSmall;
        private enum TwinkleState
        {
            Big,
            Medium,
            Small
        }
        private TwinkleState state;
        private float timeSinceLastTwinkle = (float)random.NextDouble() * interval;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="texturePath">The path to the texture to be used</param>
        public TwinkleStar(string texturePathBig, string texturePathMedium, string texturePathSmall)
        {
            this._texturePathBig = texturePathBig;
            this._texturePathMedium = texturePathMedium;
            this._texturePathSmall = texturePathSmall;
            Color = Color.White;
            this.state = TwinkleState.Small;
        }

        /// <summary>
        /// The color used to render the texture
        /// </summary>
        public Color Color { get; set; }

        #region Implementation of IContentHost

        /// <summary>
        ///   Load all XNA <see cref = "ContentManager" /> content
        /// </summary>
        /// <param name = "catalogue"></param>
        /// <param name = "manager">XNA content manage</param>
        public override void LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {
            this._textureBig = manager.Load<Texture2D>(_texturePathBig);
            this._textureMedium = manager.Load<Texture2D>(_texturePathMedium);
            this._textureSmall = manager.Load<Texture2D>(_texturePathSmall);
            
            ((IGuiElement) this).Refresh(Application.Graphics.GraphicsDevice);
        }

        /// <summary>
        ///   Unload all XNA <see cref = "ContentManager" /> content
        /// </summary>
        /// <param name = "catalogue"></param>
        public override void UnloadContent(IContentCatalogue catalogue)
        {
        }

        #endregion

        public override Control Clone()
        {
            var image = new TwinkleStar(this._texturePathBig, this._texturePathMedium, this._texturePathSmall) 
            {
                _textureBig = this._textureBig, _textureMedium = this._textureMedium, _textureSmall = this._textureSmall,
                Color = Color, Scale = Scale
            };

            return image;
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="spriteRenderer"></param>
        public override void Render(SpriteBatch spriteRenderer)
        {
            base.Render(spriteRenderer);
            var tex = CurrentTexture;
            if (tex != null)
            {
                spriteRenderer.Draw(tex, new Rectangle(0, 0, Width, Height), Color);
            }
        }

        private Texture2D CurrentTexture
        {
            get
            {
                Texture2D texture = null;
                switch (this.state)
                {
                    case TwinkleState.Big:
                        texture = this._textureBig;
                        break;
                    case TwinkleState.Medium:
                        texture = this._textureMedium;
                        break;
                    case TwinkleState.Small:
                        texture = this._textureSmall;
                        break;
                }
                return texture;
            }
        }

        /// <summary>
        /// Refreshes the control properties when it requires to be redrawn to the RenderTarget
        /// </summary>
        /// <param name="device"></param>
        public override void Refresh(GraphicsDevice device)
        {
            Width = CurrentTexture.Width;
            Height = CurrentTexture.Height;
            Width = (int)(Width * Scale.X);
            Height = (int)(Height * Scale.Y);
            //Width = Scale.X > 1 ? (int) Scale.X : 1;
            //Height = Scale.Y > 1 ? (int) Scale.Y : 1;
        }

        #region IUpdate Members

        public void Update(GameTime gameTime)
        {
            this.timeSinceLastTwinkle += gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            switch (this.state)
            {
                case TwinkleState.Small:
                    {
                        var t = 3f + (float)TwinkleStar.random.NextDouble() * TwinkleStar.interval;
                        if (this.timeSinceLastTwinkle >= t)
                        {
                            this.state = TwinkleState.Medium;
                            this.timeSinceLastTwinkle = 0.0f;
                            this.Invalidate();
                        }
                        break;
                    }
                case TwinkleState.Medium:
                    {
                        var t = 0.1f + (float)TwinkleStar.random.NextDouble() * 0.05f;
                        if (this.timeSinceLastTwinkle >= t)
                        {
                            if (TwinkleStar.random.Next(2) > 0)
                            {
                                this.state = TwinkleState.Small;
                                this.timeSinceLastTwinkle = 0.0f;
                                this.Invalidate();
                            }
                            else
                            {
                                this.state = TwinkleState.Big;
                                this.timeSinceLastTwinkle = 0.0f;
                                this.Invalidate();
                            }
                        }
                        break;
                    }
                case TwinkleState.Big:
                    {
                        var t = (float)TwinkleStar.random.NextDouble() * 0.05f;
                        if (this.timeSinceLastTwinkle >= t)
                        {
                            this.state = TwinkleState.Medium;
                            this.timeSinceLastTwinkle = 0.0f;
                            this.Invalidate();
                        }
                        break;
                    }
            }
        }

        #endregion
    }
}
