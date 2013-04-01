using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InCharge.Logic
{
    /// <summary>
    /// Defines context for context menu
    /// </summary>
    public enum MousePickContext
    {
        /// <summary>
        /// Terrain
        /// </summary>
        Ground,
        /// <summary>
        /// Any constructed building
        /// </summary>
        Building,
        /// <summary>
        /// Regular villager
        /// </summary>
        Villager,
        /// <summary>
        /// Hero character
        /// </summary>
        Hero,
    }
}
