using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Sessions;
using Microsoft.Xna.Framework.Content;
using InCharge.Persistence;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace InCharge.Rendering.Gui
{
    public class MainMenuGui : Screen, IContentHost
    {
        #region Controls
        private Button exitMenuItem;
        private Button optionsMenuItem;
        private Button newGameMenuItem;
        private Button continueMenuItem;
        private Button creditsMenuItem;
        private Image logoImage;
        private CloudLayer clouds1, clouds2;
        private Vector2 clouds1Pos, clouds2Pos;
        private List<TwinkleStar> stars = new List<TwinkleStar>();
        #endregion Controls

        private ISaveStateManager saveStateManager = Application.SunBurn.GetManager<ISaveStateManager>(true);

        public MainMenuGui()
            : base(false, true, Application.Input.PlayerOne,
            Application.Graphics.GraphicsDevice.Viewport.Width, Application.Graphics.GraphicsDevice.Viewport.Height, 2.0f,
            new Color(0, 0, 51, 255))
        {
            this.AddStars();
            this.AddClouds();
            this.AddLogo();
            this.AddButtons();
        }

        private void AddButtons()
        {
            // start with the buttons at this Y offset:
            int buttonHeight = 280;

            // we create a first button that will let us move to the game itself
            this.newGameMenuItem = new Button("Fonts/MainMenuFont", "NEW GAME")
            {
                Y = buttonHeight,
            };

            // we catch the play button event to start a new game session
            this.newGameMenuItem.Clicked += new System.EventHandler(OnPlayMenuItemClicked);


            if (saveStateManager.HasSaves)
            {
                buttonHeight += 32;
                this.continueMenuItem = new Button("Fonts/MainMenuFont", "CONTINUE")
                {
                    Y = buttonHeight
                };
                this.continueMenuItem.Clicked += new EventHandler(continueMenuItem_Clicked);
            }

            // we create an options button
            buttonHeight += 32;
            this.optionsMenuItem = new Button("Fonts/MainMenuFont", "OPTIONS")
            {
                Y = buttonHeight
            };

            // Credits screen
            buttonHeight += 32;
            this.creditsMenuItem = new Button("Fonts/MainMenuFont", "CREDITS")
            {
                Y = buttonHeight
            };

            // we then create a second button that will let us quit the game
            buttonHeight += 32;
            this.exitMenuItem = new Button("Fonts/MainMenuFont", "EXIT")
            {
                Y = buttonHeight
            }; // we set the Y position depending on the previous menu item and the height of the SpriteFont used

            // for now, we'll simply hook the Exit menu item clicked event to quit the game
            this.exitMenuItem.Clicked += delegate { Application.Instance.Exit(); };

            // we then add the menu items to the root main menu screen
            // the fonts will be loaded automatically when added.
            var buttonList = new List<Button>();

            this.Add(this.newGameMenuItem);
            buttonList.Add(this.newGameMenuItem);
            if (this.continueMenuItem != null)
            {
                this.Add(this.continueMenuItem);
                buttonList.Add(this.continueMenuItem);
            }
            this.Add(this.optionsMenuItem);
            buttonList.Add(this.optionsMenuItem);
            this.Add(this.creditsMenuItem);
            buttonList.Add(this.creditsMenuItem);
            this.Add(this.exitMenuItem);
            buttonList.Add(this.exitMenuItem);

            foreach (var button in buttonList)
            {
                button.Normal.TextColor = Color.Goldenrod;
                button.Selected.TextColor = Color.White;
            }
        }

        private void AddLogo()
        {
            this.logoImage = new Image("Textures/Gui/logo")
            {
                IsVisible = true,
                Y = 0
            };
            this.Add(this.logoImage);
        }

        private void AddClouds()
        {
            this.clouds1 = new CloudLayer("Textures/Gui/clouds_1");
            this.clouds1.Color *= 0.6f;
            this.clouds1.Scale = new Vector2(2.43f, 1.89f);
            this.Add(this.clouds1);

            this.clouds2 = new CloudLayer("Textures/Gui/clouds_1");
            this.clouds2.Color *= 0.6f;
            this.clouds2.Scale = new Vector2(6.43f, 2.89f);
            this.Add(this.clouds2);
        }

        private void AddStars()
        {
            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                var star = new TwinkleStar("Textures/Gui/star_big", "Textures/Gui/star_medium", "Textures/Gui/star_small")
                {
                    X = rand.Next(5, Application.Graphics.GraphicsDevice.Viewport.Width / 2 - 5),
                    Y = rand.Next(5, Application.Graphics.GraphicsDevice.Viewport.Height / 2 - 5),
                    Scale = Vector2.One * (0.1f + (float)rand.NextDouble() * 0.75f)
                };

                this.stars.Add(star);
                this.Add(star);
            }
        }

        void continueMenuItem_Clicked(object sender, EventArgs e)
        {
            // load the most recent save game if possible
            this.saveStateManager.LoadMostRecentSave();
            Application.SunBurn.GetManager<ISessionManager>(true).CreateSinglePlayerSession();
        }

        private void OnPlayMenuItemClicked(object sender, EventArgs e)
        {
            // in here, we just have to ask the SessionManager to create a new session (this is a single player session since we are using the LocalSessionManager,
            // if you try to create another session type, you'll get an exception at runtime.
            // This won't happen if you use the LiveSessionManager for instance on Xbox or later the LidgrenSessionManager which will work on Windows.
            Application.SunBurn.GetManager<ISessionManager>(true).CreateSinglePlayerSession();
        }

        private void DoRelativePositioning()
        {
            this.logoImage.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.logoImage.Width / 2;
            this.newGameMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.newGameMenuItem.Width / 2;
            if (this.continueMenuItem != null)
            {
                this.continueMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.continueMenuItem.Width / 2;
            }
            this.optionsMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.optionsMenuItem.Width / 2;
            this.creditsMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.creditsMenuItem.Width / 2;
            this.exitMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.exitMenuItem.Width / 2;


            this.clouds1.X = -this.clouds1.Width / 3;
            this.clouds1.Y = -this.clouds1.Height / 3;
            this.clouds1Pos = new Vector2(this.clouds1.X, this.clouds1.Y);

            this.clouds2.X = -this.clouds2.Width / 3;
            this.clouds2.Y = -this.clouds2.Height / 3;
            this.clouds2Pos = new Vector2(this.clouds2.X, this.clouds2.Y);
        }

        #region Implementation of IContentHost

        public void LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {
            // we add here to the catalogue (an IGF class) to add the button instances to the elements to load either synchronously or asynchronously depending on the
            // GameState loading system used
            catalogue.Add(this.newGameMenuItem);
            if (this.continueMenuItem != null) catalogue.Add(this.continueMenuItem);
            catalogue.Add(this.optionsMenuItem);
            catalogue.Add(this.exitMenuItem);
            catalogue.Add(this.logoImage);
            catalogue.Add(this.creditsMenuItem);
            catalogue.Add(this.clouds1);
            catalogue.Add(this.clouds2);

            foreach (var star in this.stars)
            {
                catalogue.Add(star);
            }

            this.DoRelativePositioning();
        }

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.newGameMenuItem);
            if (this.continueMenuItem != null) catalogue.Remove(this.continueMenuItem);
            catalogue.Remove(this.optionsMenuItem);
            catalogue.Remove(this.exitMenuItem);
            catalogue.Remove(this.logoImage);
            catalogue.Remove(this.clouds1);
            catalogue.Remove(this.clouds2);

            foreach (var star in this.stars)
            {
                catalogue.Remove(star);
            }
        }

        #endregion

        #region IUpdate Members

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update stars to let them twinkle
            foreach (var s in this.stars)
            {
                s.Update(gameTime);
            }

            //update clouds to get movement
            this.clouds1Pos.X += gameTime.ElapsedGameTime.Milliseconds * 0.02f;
            this.clouds1Pos.Y += gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            this.clouds2Pos.X += gameTime.ElapsedGameTime.Milliseconds * 0.004f;
            this.clouds2Pos.Y += gameTime.ElapsedGameTime.Milliseconds * 0.0006f;

            // do wrap around
            if (this.clouds1.X >= 0)
            {
                this.clouds1Pos.X -= this.clouds1.Width / 3;
            }

            if (this.clouds1.Y >= 0)
            {
                this.clouds1Pos.Y -= this.clouds1.Height / 3;
            }

            if (this.clouds2.X >= 0)
            {
                this.clouds2Pos.X -= this.clouds2.Width / 3;
            }

            if (this.clouds2.Y >= 0)
            {
                this.clouds2Pos.Y -= this.clouds2.Height / 3;
            }

            this.clouds1.X = (int)this.clouds1Pos.X;
            this.clouds1.Y = (int)this.clouds1Pos.Y;
            this.clouds2.X = (int)this.clouds2Pos.X;
            this.clouds2.Y = (int)this.clouds2Pos.Y;
        }

        #endregion
    }
}
