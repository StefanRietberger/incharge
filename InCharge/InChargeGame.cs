//-----------------------------------------------
// Synapse Gaming - SunBurn Starter Kit
//-----------------------------------------------
//
// Provides an empty solution for creating new SunBurn based games and
// projects.
// 
// To use:
//   -Run the solution from Visual Studio
//   -When running press F11 to open the in-game editor
//   -Import new models into the content repository (using the Scene Object tab)
//   -Drag models from the repository into the scene tree-view
//   -Add lights, adjust materials, the environment, and more
//
// Please see the included Readme.htm for details and documentation.
//
//-----------------------------------------------------------------------------

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

// Include the necessary SunBurn namespaces.
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Editor;
using SynapseGaming.LightingSystem.Rendering;
using InCharge.Procedural.Terrain;
using SynapseGaming.LightingSystem.Effects;
using InCharge.Util;
using Indiefreaks.Xna.Rendering.Camera;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Sessions.Local;
using Indiefreaks.Xna.Rendering.Gui;
using Indiefreaks.Xna.Physics;
using Indiefreaks.Xna.Rendering;
using InCharge.State;
using System.Windows.Forms;
using InCharge.Persistence;
using SynapseGaming.LightingSystem.Collision.Legacy;
#endregion


namespace InCharge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class InChargeGame : Indiefreaks.Xna.Core.Application
    {
        // Scene related members.
        ContentRepository contentRepository;

        public InChargeGame() : base("In Charge", "Content")
        {
            // set resolution to double SNES max native resolution
            this.GraphicsDeviceManager.PreferredBackBufferWidth = 1024;
            this.GraphicsDeviceManager.PreferredBackBufferHeight = 896;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();            

            // since the InputManager isn't yet initialized, we only can retrieve PlayerInput instances using the physical playerindex input using the following method:
            var playerInputOne = Input.GetPlayerInput(PlayerIndex.One);

            // to activate the Keyboard & Mouse input system
            playerInputOne.UseKeyboardMouseInput = true;            

            this.SunBurnSystemPreferences.ShadowDetail = DetailPreference.High;
            this.SunBurnSystemPreferences.ShadowQuality = 1f;
            this.SunBurnSystemPreferences.LightingDetail = DetailPreference.High;

            // if you want to change the virtual gamepad mapping:
            playerInputOne.VirtualGamePadMapping.LeftStickLeft = Microsoft.Xna.Framework.Input.Keys.Left;
            playerInputOne.VirtualGamePadMapping.LeftStickForward = Microsoft.Xna.Framework.Input.Keys.Up;
            playerInputOne.VirtualGamePadMapping.LeftStickRight = Microsoft.Xna.Framework.Input.Keys.Right;
            playerInputOne.VirtualGamePadMapping.LeftStickBackward = Microsoft.Xna.Framework.Input.Keys.Down;            

            // We first start by loading our IntroductionGameState
            LoadGameState(new IntroGameState(this));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the content repository, which stores all assets imported via the editor.
            // This must be loaded before any other assets.
            contentRepository = Content.Load<ContentRepository>("Content");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here            
        }        

        #region Main entry point
#if !WINDOWS_PHONE
        static class Program
        {
            /// <summary>
            /// The main entry point for the application.
            /// </summary>
            [STAThread]
            static void Main(string[] args)
            {
#if WINDOWS
                // Improved ui.
                System.Windows.Forms.Application.EnableVisualStyles();
#endif

                using (InChargeGame game = new InChargeGame())
                    game.Run();
            }
        }
#endif
        #endregion

        protected override void InitializeSunBurn()
        {
            // we add the local only SessionManager instance.
            SunBurn.AddManager(new LocalSessionManager(Indiefreaks.Xna.Core.Application.SunBurn));
            // we add the manager which handles the GUI for us.
            SunBurn.AddManager(new GuiManager(Indiefreaks.Xna.Core.Application.SunBurn));
            // we'll use IGF BEPUPhysics implementation to handle the game physics
            //SunBurn.AddManager(new BEPUPhysicsManager(Indiefreaks.Xna.Core.Application.SunBurn));
            SunBurn.AddManager(new CollisionManager(Indiefreaks.Xna.Core.Application.SunBurn));
            
            // instantiate the save game manager
            SunBurn.AddManager(new SaveStateManager(Indiefreaks.Xna.Core.Application.SunBurn));

            SunBurn.AddManager(new SynapseGaming.LightingSystem.Editor.SunBurnEditor(Indiefreaks.Xna.Core.Application.SunBurn));
            // Set the in-editor hotkey.
            Indiefreaks.Xna.Core.Application.SunBurn.Editor.LaunchKey = Microsoft.Xna.Framework.Input.Keys.F11;

            // Helper method found in Application class to create the Rendering Managers for you
            this.CreateRenderer(Renderers.Deferred);

            // enable keyboard and mouse input
            Indiefreaks.Xna.Core.Application.Input.IsMouseVisible = true;
            Indiefreaks.Xna.Core.Application.Input.UseKeyboardAndMouse = true;            

            var cursor = NativeMethods.LoadCustomCursor(@"Content\Textures\Cursor\hand_cursor.cur");
            Form winForm = (Form)Form.FromHandle(this.Window.Handle);
            winForm.Cursor = cursor;
        }
    }
}
