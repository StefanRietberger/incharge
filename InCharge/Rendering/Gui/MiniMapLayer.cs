using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering;
using SynapseGaming.LightingSystem.Rendering;
using Indiefreaks.Xna.Core;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;
using Microsoft.Xna.Framework;
using SynapseGaming.LightingSystem.Effects;
using Indiefreaks.Xna.Rendering.Camera;

namespace InCharge.Rendering.Gui
{
    public class MiniMapLayer : Layer
    {
        private Texture2D texCompass;
        private Effect compassQuadEffect;
        private SpriteBatch spriteBatch;
        private Vector2 compassOrigin;
        private Vector2 compassScreenPosition;

        private CustomArcBallCamera3d camera;

        public MiniMapLayer(GameState gameState, CustomArcBallCamera3d camera)
            : base(gameState)
        {
            this.texCompass = this.GameState.Application.Content.Load<Texture2D>("Textures/Gui/compass");
            this.compassOrigin = new Vector2(this.texCompass.Width / 2, this.texCompass.Height / 2);
            this.compassQuadEffect = new SunBurnBasicEffect(this.GameState.Application.GraphicsDevice);
            this.compassQuadEffect.Parameters["Texture"].SetValue(texCompass);

            this.camera = camera;
            this.spriteBatch = new SpriteBatch(gameState.Application.GraphicsDevice);

            this.compassScreenPosition = new Vector2(960, 60);
        }        

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.texCompass != null)
            {
                this.spriteBatch.Begin();
                this.spriteBatch.Draw(
                    this.texCompass, 
                    this.compassScreenPosition,
                    null, 
                    Color.White, 
                    this.camera.HorizontalRotation, 
                    this.compassOrigin,
                    1.0f, 
                    SpriteEffects.None, 0);
                this.spriteBatch.End();
            }
        }
    }
}
