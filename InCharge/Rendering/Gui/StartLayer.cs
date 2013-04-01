using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Rendering;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Sessions;

namespace InCharge.Rendering.Gui
{
    public class StartLayer: Layer
    {
        private readonly Screen pressStartScreen;
        private readonly Label pressStartLabel;
        private readonly Image gameTitleImage;

        public StartLayer(GameState state)
            : base(state)
        {
            // we first create our GUI Screen instance used to render GUI elements.
            this.pressStartScreen = new Screen(true, false, null);
            // we now create the Label instance that will display the "Press Start" message.
            this.pressStartLabel = new Label("Fonts/GuiFont", "Press Start");
            // we create our game title picture
            this.gameTitleImage = new Image("Textures/gametitle")
            {
                X = Application.Graphics.GraphicsDevice.Viewport.Width / 2 - 300,   // we set the X position of the game title in the middle of the screen
                Y = 100,                                                          // and the Y position at a fixed height
                Scale = new Vector2(1, 1)                                    // the Scale property is used to define the destination rectangle used by a SpriteBatch
            };

            // we ask the GameState to add the Label instance to the list of elements that will get their LoadContent method called (works for synchronous and asynchronous
            // content loading.
            GameState.PreLoad(this.pressStartLabel);
            // we also ask it to load the Image instance
            GameState.PreLoad(this.gameTitleImage);

            // we add the Label and the Image to the Screen so it gets rendered.
            this.pressStartScreen.Add(this.pressStartLabel);
            this.pressStartScreen.Add(this.gameTitleImage);

            // we now hook the GameState.LoadingCompleted event so that we can place our Label correctly and render it all.
            GameState.LoadingCompleted += OnGameStateLoadingCompleted;
        }

        private void OnGameStateLoadingCompleted(object sender, EventArgs e)
        {
            // we first position our label in the center of the window
            this.pressStartLabel.X = Application.Graphics.GraphicsDevice.Viewport.Width / 2 - this.pressStartLabel.Width / 2;
            this.pressStartLabel.Y = Application.Graphics.GraphicsDevice.Viewport.Height / 2 - this.pressStartLabel.Height / 2;

            // we now add the Screen to the GuiManager: remember the Label is already added to the Screen instance.
            Application.SunBurn.GetManager<IGuiManager>(true).AddScreen(this.pressStartScreen);

            // we retrieve the current SessionManager instance (note that you don't have to care if it is a LocalSessionManager or any other implementation: it is meant to work
            // seemlessly).
            var sessionManager = Application.SunBurn.GetManager<ISessionManager>(true);
            // we now ask the SessionManager to start listening to player input events to identify players
            sessionManager.BeginPlayerIdentification();
            // and we hook the PlayerLogin event so that we can react when a player actually presses the Start button.
            sessionManager.PlayerLogin += delegate
            {
                // we hide the current layer as well as the Screen instance to avoid it being processed
                // and tell the SessionManager to end the player identification process.
                IsVisible = false;
                this.pressStartScreen.IsVisible = false;
                sessionManager.EndPlayerIdentification();
            };
        }
    }
}
