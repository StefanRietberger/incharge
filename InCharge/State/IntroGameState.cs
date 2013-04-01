using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Rendering;
using SynapseGaming.LightingSystem.Rendering;

namespace InCharge.State
{
    public class IntroGameState : GameState
    {
        private SunBurnSplashScreen _sunburnSplashScreen;

        public IntroGameState(Application application)
            : base("Intro", application)
        { 

        }

        public override void Initialize()
        {
            // we add the IGF SunBurnSplashScreen layer which takes care for you of handling the SunBurn SplashScreen component
            _sunburnSplashScreen = new SunBurnSplashScreen(this, true);
            AddLayer(_sunburnSplashScreen);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (SplashScreen.DisplayComplete && _sunburnSplashScreen.IsVisible)
            {
                _sunburnSplashScreen.IsVisible = false;
                _sunburnSplashScreen.UnloadContent(this.Content);

                Application.LoadGameState(new MainMenuState(Application));
            }
        }
    }
}
