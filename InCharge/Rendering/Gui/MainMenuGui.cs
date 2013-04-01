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
        private readonly Button exitMenuItem;
        private readonly Button optionsMenuItem;
        private readonly Button newGameMenuItem;
        private readonly Button continueMenuItem;
        private readonly Button creditsMenuItem;
        private readonly Image logoImage;

        private ISaveStateManager saveStateManager = Application.SunBurn.GetManager<ISaveStateManager>(true);

        public MainMenuGui()
            : base(false, true, Application.Input.PlayerOne)
        {
            this.logoImage = new Image("Textures/Gui/logo")
            {
                IsVisible = true,                
                Y = -90
            };            
            this.Add(this.logoImage);

            // start with the buttons at this Y offset:
            int buttonHeight = 280;

            // we create a first button that will let us move to the game itself
            this.newGameMenuItem = new Button("Fonts/MainMenuFont", "NEW GAME") 
            {                
                Y = buttonHeight,
            };            
            this.newGameMenuItem.Normal.TextColor = Color.Gold;

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
            this.Add(this.newGameMenuItem);
            if (this.continueMenuItem != null) this.Add(this.continueMenuItem);
            this.Add(this.optionsMenuItem);
            this.Add(this.creditsMenuItem);
            this.Add(this.exitMenuItem);
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
                continueMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.continueMenuItem.Width / 2;
            }
            this.optionsMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.optionsMenuItem.Width / 2;
            this.creditsMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.creditsMenuItem.Width / 2;
            this.exitMenuItem.X = Application.Graphics.GraphicsDevice.Viewport.Width / 4 - this.exitMenuItem.Width / 2;
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

            this.DoRelativePositioning();
        }        

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.newGameMenuItem);
            if (this.continueMenuItem != null) catalogue.Remove(this.continueMenuItem);
            catalogue.Remove(this.optionsMenuItem);
            catalogue.Remove(this.exitMenuItem);
            catalogue.Remove(this.logoImage);
        }

        #endregion
    }
}
