using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Input;
using InCharge.Logic;

namespace InCharge.UI
{
    public interface IGameController
    {
        /// <summary>
        /// Set input state (e.g. because of GUI button click)
        /// </summary>
        /// <param name="newState"></param>
        void ChangeInputState(InputState newState);
        /// <summary>
        /// Get current input state
        /// </summary>
        InputState CurrentInputState { get; }
        /// <summary>
        /// Retrieve the current mouse pick context
        /// </summary>
        MousePickContext CurrentMousePickContext { get; }

        /// <summary>
        /// Send confirmation of current input
        /// </summary>
        void AcceptInput();
        /// <summary>
        /// Send cancel call of current input
        /// </summary>
        void CancelInput();
        /// <summary>
        /// Handles a left click event from the GUI
        /// </summary>
        void OnLeftClick();
    }
}
