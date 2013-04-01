using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Logic;
using Microsoft.Xna.Framework.Input;
using InCharge.Procedural.Terrain;
using InCharge.Util;
using Microsoft.Xna.Framework;

namespace InCharge.Logic.Character
{
    public class PlayerClickBehavior : Behavior
    {
        /// <summary>
        /// PlayerAgent reference
        /// </summary>
        private PlayerAgent playerAgent;
        /// <summary>
        /// Target character to control by this behavior
        /// </summary>
        private Character character;
        /// <summary>
        /// Terrain map reference, must be known for navigation
        /// </summary>
        private TerrainMap map;
        /// <summary>
        /// Stores the current path
        /// </summary>
        private List<BlockPosition> currentPath;
        private float currentPathLength;
        private float remainingPathLength;

        public PlayerClickBehavior(TerrainMap map)
        {
            this.map = map;

            // validate click, position and path (will not be evaluated without clicking)
            this.AddCondition(new Condition(CheckPath));
            this.AddLocalCommand(
                new Command.ClientCommand(WalkCommand),
                ExecutionFrequency.FullUpdate60Hz);
        }

        /// <summary>
        /// Gets called after attachment of the behavior to an agent/object
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.playerAgent = this.Agent as PlayerAgent;
            this.character = playerAgent.ParentObject as Character;
        }

        private bool CheckPath()
        {
            if (this.playerAgent.Input.KeyboardMouseState.MouseState.LeftButton == ButtonState.Pressed)
            {
                var pickRay = MousePicking.GetPickingRay();
                var currentPickResult = this.map.GetPickedTerrainBlock(pickRay);

                if (currentPickResult.HasValue)
                {
                    // calculate path from start to target
                    var path = Pathfinder.FindCharacterPathToDestination(this.character, this.map, currentPickResult.Value.BlockPosition);
                    // check if path is valid
                    if (path != null && path.Count > 0)
                    {
                        this.currentPath = path;
                        this.currentPathLength = PlayerClickBehavior.CalculatePathLength(this.currentPath);
                    }
                }
            }
            return this.currentPath != null;
        }        

        private object WalkCommand(Command command)
        {            
            // move character accordingly
            if (this.currentPath.Count > 0 && !this.character.IsMoving)
            {
                var remainingPathLength = PlayerClickBehavior.CalculatePathLength(this.currentPath);

                this.character.StepTo(
                    this.currentPath.First().WorldPosition, 
                    this.currentPathLength, 
                    remainingPathLength);

                this.currentPath.RemoveAt(0);                
            }
            return null;
        }

        protected override void Process(float elapsed)
        {
            base.Process(elapsed);
        }

        private static float CalculatePathLength(List<BlockPosition> path)
        {
            BlockPosition last = path.First();
            float len = 0;
            foreach (var node in path)
            {
                len += Vector3.Distance(last.WorldPosition, node.WorldPosition);
                last = node;
            }
            return len;
        }
    }
}
