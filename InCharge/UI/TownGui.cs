using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Sessions;
using Indiefreaks.Xna.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using InCharge.Logic;
using InCharge.Input;

namespace InCharge.UI
{
    public class TownGui : Screen, IContentHost
    {
        private const string FONT_PATH = "Fonts/GuiFont";
        

        /// <summary>
        /// Controller reference
        /// </summary>
        private IGameController controller;

        private Indiefreaks.Xna.Rendering.Gui.Button btnTerraform;
        private Indiefreaks.Xna.Rendering.Gui.Button btnBuild;
        private Indiefreaks.Xna.Rendering.Gui.Button btnDemolish;                

        /// <summary>
        /// Right-click context menu
        /// </summary>
        private ContextMenu contextMenu;

        /// <summary>
        /// Currently active placement menu controls, null if menu is inactive
        /// </summary>
        private List<Control> activePlaceMenuControls;
        /// <summary>
        /// A button spanning the whole screen, used to capture generic click events 
        /// and delegate them to the game state
        /// </summary>
        private Indiefreaks.Xna.Rendering.Gui.Button btnScreen;

        private Indiefreaks.Xna.Rendering.Gui.Button btnDig;
        private Indiefreaks.Xna.Rendering.Gui.Button btnDump;
        private Indiefreaks.Xna.Rendering.Gui.Button btnAccept;
        private Indiefreaks.Xna.Rendering.Gui.Button btnCancel;

        private Indiefreaks.Xna.Rendering.Gui.Button btnDumpSoil;
        private Indiefreaks.Xna.Rendering.Gui.Button btnDumpRock;

        /// <summary>
        /// status flag to check if any control other than the entire screen had been clicked
        /// </summary>
        private bool controlClicked = false;

        public TownGui(PlayerInput playerInput, IGameController controller)
            : base(false, true, playerInput)
        {
            this.controller = controller;

            this.btnBuild = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_build_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_build_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_build_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnBuild);

            this.btnDemolish = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_demolish_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_demolish_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_demolish_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnDemolish);

            this.btnTerraform = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_terraform_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_terraform_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_terraform_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnTerraform);
            this.btnTerraform.Clicked += new EventHandler(btnTerraform_Clicked);

            this.btnDig = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_dig_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_dig_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_dig_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnDig);
            this.btnDig.Clicked += new EventHandler(btnDig_Clicked);

            this.btnDump = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_dump_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_dump_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_dump_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnDump);
            this.btnDump.Clicked += new EventHandler(btnDump_Clicked);

            this.btnAccept = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_accept_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_accept_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_accept_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnAccept);
            this.btnAccept.Clicked += new EventHandler(btnAccept_Clicked);

            this.btnCancel = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_cancel_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_cancel_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_cancel_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnCancel);
            this.btnCancel.Clicked += new EventHandler(btnCancel_Clicked);

            this.btnDumpSoil = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_soil_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_soil_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_soil_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnDumpSoil);
            this.btnDumpSoil.Clicked += new EventHandler(btnDumpSoil_Clicked);

            this.btnDumpRock = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/btn_rock_normal", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/btn_rock_selected", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/btn_rock_pressed", FONT_PATH),
                IsVisible = false
            };
            this.Add(this.btnDumpRock);
            this.btnDumpRock.Clicked += new EventHandler(btnDumpRock_Clicked);

            // add full screen button to capture non-control left clicks, must be added last!
            this.btnScreen = new Indiefreaks.Xna.Rendering.Gui.Button(FONT_PATH, "")
            {
                Normal = new ButtonSkin("Textures/Gui/transparent", FONT_PATH),
                Selected = new ButtonSkin("Textures/Gui/transparent", FONT_PATH),
                Pressed = new ButtonSkin("Textures/Gui/transparent", FONT_PATH),
                X = 0,
                Y = 0,
                Scale = new Vector2(Application.Graphics.GraphicsDevice.Viewport.Width, Application.Graphics.GraphicsDevice.Viewport.Height)
            };
            this.Add(this.btnScreen);
            this.btnScreen.Clicked += new EventHandler(btnScreen_Clicked);

            // set up context visibility
            var contextVisibility = new Dictionary<Tuple<InputState, MousePickContext>, List<Control>>();
            foreach (InputState state in Enum.GetValues(typeof(InputState)))
            {
                foreach (MousePickContext pick in Enum.GetValues(typeof(MousePickContext)))
                {
                    contextVisibility.Add(new Tuple<InputState, MousePickContext>(state, pick), new List<Control>());
                }
            }

            var freeGround = contextVisibility[new Tuple<InputState, MousePickContext>(InputState.Free, MousePickContext.Ground)];
            freeGround.Add(this.btnTerraform);
            freeGround.Add(this.btnBuild);

            var terraGround = contextVisibility[new Tuple<InputState, MousePickContext>(InputState.Terraform, MousePickContext.Ground)];
            terraGround.Add(this.btnDig);
            terraGround.Add(this.btnDump);
            terraGround.Add(this.btnAccept);
            terraGround.Add(this.btnCancel);

            var digGround = contextVisibility[new Tuple<InputState, MousePickContext>(InputState.Dig, MousePickContext.Ground)];
            digGround.Add(this.btnDump);
            digGround.Add(this.btnAccept);
            digGround.Add(this.btnCancel);

            var dumpGround = contextVisibility[new Tuple<InputState, MousePickContext>(InputState.Dump, MousePickContext.Ground)];
            dumpGround.Add(this.btnDig);
            dumpGround.Add(this.btnAccept);
            dumpGround.Add(this.btnCancel);

            var freeBuilding = contextVisibility[new Tuple<InputState, MousePickContext>(InputState.Free, MousePickContext.Building)];
            freeBuilding.Add(this.btnBuild);
            // create context menu
            this.contextMenu = new ContextMenu(this.controller, contextVisibility);
        }

        void btnDumpRock_Clicked(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnDumpSoil_Clicked(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Generic click handler for any click not intended for a control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnScreen_Clicked(object sender, EventArgs e)
        {
            if (!this.controlClicked)
            {
                this.controller.OnLeftClick();
            }
        }

        void btnDump_Clicked(object sender, EventArgs e)
        {
            this.controlClicked = true;
            this.controller.ChangeInputState(InputState.Dump);
            this.contextMenu.CloseMenu();
        }

        void btnDig_Clicked(object sender, EventArgs e)
        {
            this.controlClicked = true;
            this.controller.ChangeInputState(InputState.Dig);
            this.contextMenu.CloseMenu();
        }

        void btnTerraform_Clicked(object sender, EventArgs e)
        {
            this.controlClicked = true;
            // switch directly to dig mode
            this.btnDig_Clicked(sender, e);
        }

        void btnCancel_Clicked(object sender, EventArgs e)
        {
            this.controlClicked = true;
            this.controller.CancelInput();
            this.contextMenu.CloseMenu();
        }

        void btnAccept_Clicked(object sender, EventArgs e)
        {
            this.controlClicked = true;
            this.controller.AcceptInput();
            this.contextMenu.CloseMenu();
        }

        public override void HandleInput(PlayerInput input, GameTime gameTime)
        {
            this.controlClicked = false;
            // handle base input
            base.HandleInput(input, gameTime);

            // on right click, toggle context menu if applicable
            if (input.KeyboardMouseState.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                this.contextMenu.ToggleMenu(new Point(input.KeyboardMouseState.MouseState.X, input.KeyboardMouseState.MouseState.Y));
            }            
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.contextMenu.Update(gameTime);
        }

        private void AnimateBottomMenu(GameTime gameTime)
        {
            //float timeScale = this.bottomAnimatingTime / openBottomAnimationLength;
        }
        

        #region IContentHost Members

        public void LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {
            catalogue.Add(this.btnScreen);
            catalogue.Add(this.btnBuild);
            catalogue.Add(this.btnTerraform);
            catalogue.Add(this.btnDemolish);
            catalogue.Add(this.btnDig);
            catalogue.Add(this.btnDump);
            catalogue.Add(this.btnAccept);
            catalogue.Add(this.btnCancel);
            catalogue.Add(this.btnDumpSoil);
            catalogue.Add(this.btnDumpRock);
        }

        public void UnloadContent(IContentCatalogue catalogue)
        {
            catalogue.Remove(this.btnScreen);
            catalogue.Remove(this.btnBuild);
            catalogue.Remove(this.btnTerraform);
            catalogue.Remove(this.btnDemolish);
            catalogue.Remove(this.btnDig);
            catalogue.Remove(this.btnDump);
            catalogue.Remove(this.btnAccept);
            catalogue.Remove(this.btnCancel);
            catalogue.Remove(this.btnDumpSoil);
            catalogue.Remove(this.btnDumpRock);
        }

        #endregion
    }
}
