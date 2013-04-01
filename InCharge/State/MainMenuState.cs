using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Core;
using InCharge.Rendering;
using Indiefreaks.Xna.Sessions;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Rendering.Gui;
using InCharge.Rendering.Gui;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;

namespace InCharge.State
{
    public class MainMenuState : GameState
    {
        private MainMenuLayer mainMenuLayer;
        private StartLayer startLayer;

        

        public MainMenuState(Application application)
            : base("Main Menu", application)
        {
            
        }

        public override void Initialize()
        {
            // we add our start screen layer to the GameState
            this.startLayer = new StartLayer(this);
            AddLayer(this.startLayer);

            // we need to tell the Application to FadeIn from Black since we FadeOut to Black on the IntroductionGameState
            this.LoadingCompleted += delegate 
            {
                //Application.FadeIn(Color.Black, 0.25f); 
            };

            // we retrieve the SessionManager instance and hook the PlayerIdentificationEnded event. For now, we'll simply exit the game.
            var sessionManager = Application.SunBurn.GetManager<ISessionManager>(true);
            sessionManager.PlayerIdentificationEnded += OnPlayerIdentificationEnded;
            // we need to catch when a session is created to start the session and load the Gameplay GameState.
            sessionManager.SessionCreated += OnSessionCreated;            
        }

        private void OnSessionCreated(object sender, EventArgs e)
        {
            // since we are only working on a SinglePlayer game, we just need to hook the SessionStarted event on the current session and actually start it.
            // you'll notice that we access the CurrentSession property from the SessionManager class statically since we can only have one session per game,
            // this is a practical way to retrieve the session instance without having to get through the SunBurn Manager calls.
            SessionManager.CurrentSession.Started += OnSessionStarted;

            SessionManager.CurrentSession.StartSession();
        }

        private void OnSessionStarted(object sender, EventArgs e)
        {
            // since we are loading a new scene, we need to clear all managers we've been using in the current GameState that are shared accross the application.
            // In this case, the GUIManager.
            Application.SunBurn.GetManager<IGuiManager>(true).Unload();

            // and when the session is started, we just load the GameplayGameState
            Application.LoadGameState(new TownGameState(Application));
        }

        private void OnPlayerIdentificationEnded(object sender, EventArgs e)
        {
            // since we are using the SessionManager.GetIdentifiedPlayer() method to retrieve the player which hit the Start button,
            // we need to wait for a player to hit the Start button to create our root main menu and add it to the GameState
            this.mainMenuLayer = new MainMenuLayer(this);
            // remember that when you add a layer in a GameState, if it implements IContentHost, the LoadContent method is automatically called by the framework
            AddLayer(this.mainMenuLayer);
        }       
    }
}
