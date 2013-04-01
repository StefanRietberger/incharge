using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Procedural.Terrain;
using Indiefreaks.Xna.Logic;

namespace InCharge.Logic.Character
{
    public abstract class NpcBaseBehavior: Behavior
    {
        /// <summary>
        /// Agent reference
        /// </summary>
        protected NonPlayerAgent npcAgent;
        /// <summary>
        /// Target character to control by this behavior
        /// </summary>
        protected Character character;
        /// <summary>
        /// Terrain map reference, must be known for navigation
        /// </summary>
        protected TerrainMap map;

        public NpcBaseBehavior(TerrainMap map)
        {
            this.map = map;
        }

        /// <summary>
        /// Gets called after attachment of the behavior to an agent/object
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.npcAgent = this.Agent as NonPlayerAgent;
            this.character = this.npcAgent.ParentObject as Character;
        }
    }
}
