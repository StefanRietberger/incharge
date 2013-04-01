using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering.Gui;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Core;
using InCharge.Input;
using InCharge.Logic;

namespace InCharge.UI
{
    public class ContextMenu : IUpdate
    {
        private static Point POINT_ZERO = new Point(0, 0);
        private const int MENU_DIAMETER = 100;
        private const int BUTTON_SIZE = 64;
        private const int HALF_BUTTON_SIZE = BUTTON_SIZE >> 1;
        
        private bool isContextMenuOpen = false;
        /// <summary>
        /// open/closed status flag of the context menu
        /// </summary>
        public bool IsContextMenuOpen
        {
            get { return isContextMenuOpen; }
        }
        
        private bool isContextAnimating = false;
        /// <summary>
        /// Indicates if the context menu is currently animating
        /// </summary>
        public bool IsContextAnimating
        {
            get { return isContextAnimating; }
        }

        private float contextAnimatingTime = 0.0f;
        private const float openContextAnimationLength = 0.3f;
        private const float closeContextAnimationLength = 0.1f;
        /// <summary>
        /// Currently active context menu controls, null if menu is closed
        /// </summary>
        private List<Control> activeContextMenuControls;        
        /// <summary>
        /// Center point of the menu
        /// </summary>
        private Point menuCenter;

        private InputState prevInputState;
        private MousePickContext prevPickContext;

        /// <summary>
        /// Game logic controller reference
        /// </summary>
        private IGameController controller;
        /// <summary>
        /// Control context visibility information
        /// </summary>
        private Dictionary<Tuple<InputState, MousePickContext>, List<Control>> contextVisibility;

        /// <summary>
        /// Creates a new context menu
        /// </summary>
        /// <param name="controller"></param>
        public ContextMenu(IGameController controller, Dictionary<Tuple<InputState, MousePickContext>, List<Control>> contextVisibility)
        {
            this.controller = controller;
            this.contextVisibility = contextVisibility;
        }

        public void CloseMenu()
        {
            this.ToggleMenu(POINT_ZERO, true);
        }

        public void ToggleMenu(Point center, bool forceClose = false)
        {
            // don't act if already animation
            if (this.isContextAnimating) return;

            // start animation
            this.isContextAnimating = true;
            this.contextAnimatingTime = 0.0f;

            // toggle open flag
            this.isContextMenuOpen = forceClose ? false : !this.isContextMenuOpen;
            if (this.isContextMenuOpen)
            {
                this.menuCenter = center;
                this.activeContextMenuControls = this.GetMenuControlsFromContext();
               
                foreach (Control c in this.activeContextMenuControls) c.IsVisible = true;
            }
            else
            {
                
            }
        }

        private List<Control> GetMenuControlsFromContext()
        {
            List<Control> controls;

            this.contextVisibility.TryGetValue(
                new Tuple<InputState, MousePickContext>(
                        this.controller.CurrentInputState,
                        this.controller.CurrentMousePickContext),
                        out controls);

            return controls != null ? controls : new List<Control>(0);
        }

        private void AnimateOpenMenu(GameTime gameTime)
        {
            // get radial angles for buttons
            float angle = MathHelper.Pi / (this.activeContextMenuControls.Count);
            // get factor of progression into animation
            float timeScale = this.contextAnimatingTime / openContextAnimationLength;
            //float drawScale = (float)(Math.Round(timeScale * 10) * 0.1);
            // get distance from center
            float dist = MENU_DIAMETER * timeScale;
            float centerOffset = HALF_BUTTON_SIZE * timeScale;

            // assign new positions
            for (int i = 0; i < this.activeContextMenuControls.Count; i++)
            {
                var button = this.activeContextMenuControls[i];
                button.X = this.menuCenter.X + (int)(dist * Math.Cos(angle * (i - 1)) - centerOffset);
                button.Y = this.menuCenter.Y + (int)(dist * Math.Sin(angle * (i - 1)) - centerOffset);
                button.Scale = new Vector2(timeScale, timeScale);
                button.Invalidate();
            }
        }

        private void AnimateClosingMenu(GameTime gameTime)
        {
            // get radial angles for buttons
            float angle = MathHelper.Pi / (this.activeContextMenuControls.Count);
            // get factor of progression into animation
            float timeScale = this.contextAnimatingTime / closeContextAnimationLength;
            // get distance from center
            float dist = MENU_DIAMETER * timeScale;
            float centerOffset = HALF_BUTTON_SIZE * (1 - timeScale);

            // assign new positions
            for (int i = 0; i < this.activeContextMenuControls.Count; i++)
            {
                var button = this.activeContextMenuControls[i];
                button.X = this.menuCenter.X + (int)((MENU_DIAMETER - dist) * Math.Cos(angle * (i - 1)) - centerOffset);
                button.Y = this.menuCenter.Y + (int)((MENU_DIAMETER - dist) * Math.Sin(angle * (i - 1)) - centerOffset);
                button.Scale = new Vector2(1 - timeScale, 1 - timeScale);
                button.Invalidate();
            }
        }

        #region IUpdate Members

        public void Update(GameTime gameTime)
        {
            if (this.isContextAnimating)
            {
                this.contextAnimatingTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;                 

                if (this.isContextMenuOpen)
                { // opening
                    if (this.contextAnimatingTime < openContextAnimationLength)
                    {
                        this.AnimateOpenMenu(gameTime);
                    }
                    else
                    {
                        this.isContextAnimating = false;
                    }
                }
                else
                { // closing
                    if (this.contextAnimatingTime < closeContextAnimationLength)
                    {
                        this.AnimateClosingMenu(gameTime);
                    }
                    else
                    {
                        foreach (Control c in this.activeContextMenuControls) c.IsVisible = false;
                        this.activeContextMenuControls = new List<Control>(0);
                        this.isContextAnimating = false;
                    }
                }
            }        
        }

        #endregion
    }
}
