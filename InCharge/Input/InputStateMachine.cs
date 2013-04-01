using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using InCharge.State;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Input;

namespace InCharge.Input
{
    /// <summary>
    /// Handles input based on current input states
    /// </summary>
    public class InputStateMachine
    {
        private class StateNode
        {
            public InputState State { get; private set; }
            public List<StateNode> ChildrenStates { get; private set; }
            public Dictionary<Keys, InputAction> KeyActions {get; private set; }
            public Dictionary<MouseInput, InputAction> MouseActions { get; private set; } 

            public StateNode(InputState state)
            {
                this.State = state;
                this.ChildrenStates = new List<StateNode>();
                this.KeyActions = new Dictionary<Keys, InputAction>();
                this.MouseActions = new Dictionary<MouseInput, InputAction>();
            }
        }
        /// <summary>
        /// Reference to complete state hierarchy
        /// </summary>
        private StateNode stateHierarchy;
        /// <summary>
        /// Current state node
        /// </summary>
        private StateNode currentState;
        /// <summary>
        /// Current input state
        /// </summary>
        public InputState CurrentState { get { return this.currentState.State; } }

        /// <summary>
        /// Defines a game action triggered by user input 
        /// </summary>
        /// <param name="gameTime">Current game time</param>
        /// <param name="down">Indicates if the trigger is still pressed, false if this is the release event</param>
        public delegate void InputAction(Button buttonState, GameTime gameTime);

        public InputStateMachine()
        {
            // set up state hierarchy
            this.stateHierarchy = new StateNode(InputState.Free);
            this.currentState = this.stateHierarchy;

            var terraformState = new StateNode(InputState.Terraform);
            this.stateHierarchy.ChildrenStates.Add(terraformState);
            terraformState.ChildrenStates.Add(new StateNode(InputState.Dig));
            terraformState.ChildrenStates.Add(new StateNode(InputState.Dump));

            this.stateHierarchy.ChildrenStates.Add(new StateNode(InputState.Build));
        }

        private StateNode GetNodeForInputState(InputState inputState, StateNode node)
        {
            if (node.State == inputState)
            {
                return node;
            }
            else
            {
                foreach (StateNode n in node.ChildrenStates)
                {
                    var recResult = this.GetNodeForInputState(inputState, n);
                    if (recResult.State == inputState) return recResult;
                }
            }
            // if state not found, return root node
            return this.stateHierarchy;
        }

        public void RegisterKeyboardActionForState(Keys key, InputState state, InputAction action) 
        {
            var currNode = this.GetNodeForInputState(state, this.stateHierarchy);
            currNode.KeyActions.Add(key, action);
        }

        public void RegisterMouseActionForState(MouseInput mouse, InputState state, InputAction action)
        {
            var currNode = this.GetNodeForInputState(state, this.stateHierarchy);
            currNode.MouseActions.Add(mouse, action);
        }        

        public void ChangeState(InputState newState)
        {
            var newNode = this.GetNodeForInputState(newState, this.stateHierarchy);
            this.currentState = newNode;
        }

        public void ProcessInput(GameTime gameTime)
        {
            foreach (KeyValuePair<Keys, InputAction> ki in this.currentState.KeyActions)
            {
                // get key button state
                var button = Application.Input.KeyboardState.GetKey(ki.Key);
                // call delegate
                ki.Value(button, gameTime);
            }

            foreach (KeyValuePair<MouseInput, InputAction> mi in this.currentState.MouseActions)
            {
                // get mouse button state
                Button button;

                switch (mi.Key)
                {
                    default:
                    case MouseInput.LeftButton: button = Application.Input.MouseState.LeftButton; break;
                    case MouseInput.RightButton: button = Application.Input.MouseState.RightButton; break;
                    case MouseInput.MiddleButton: button = Application.Input.MouseState.MiddleButton; break;                    
                    case MouseInput.XButton1: button = Application.Input.MouseState.XButton1; break;
                    case MouseInput.XButton2: button = Application.Input.MouseState.XButton2; break;
                }

                // call delegate
                mi.Value(button, gameTime);
            }
        }
    }
}
