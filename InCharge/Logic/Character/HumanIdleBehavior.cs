using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Procedural.Terrain;
using Indiefreaks.Xna.Logic;
using Microsoft.Xna.Framework;

namespace InCharge.Logic.Character
{
    /// <summary>
    /// Lets characters do some idle stuff more or less in place when there is nothing else to do
    /// or they need a break.
    /// </summary>
    public class HumanIdleBehavior : NpcBaseBehavior
    {
        private Random random = new Random();
        private float timeSinceLastAction = float.MaxValue;

        public HumanIdleBehavior(TerrainMap map)
            : base(map)
        {
            this.AddCondition(new Condition(IsCharacterIdle));
            this.AddLocalCommand(
                new Command.ClientCommand(Idle), 
                ExecutionFrequency.PartialUpdate10Hz);
        }

        private bool IsCharacterIdle()
        {
            // TODO: track state of character
            return true;
        }

        private object Idle(Command command)
        {
            if (this.timeSinceLastAction > 0)
            {
                // do something in random intervals
                var randTime = this.random.Next(1500, 5000) / 1000f;
                if (this.timeSinceLastAction > randTime)
                {
                    // step one block at random
                    var blocks = Pathfinder.GetPassableNeighborBlocksForCharacter(this.map, this.character);
                    var cnt = blocks.Count();
                    if (cnt > 0)
                    { 
                        var index = random.Next(cnt);
                        var block = blocks.ElementAt(index);
                        var distance = (block.Block.WorldPosition - this.character.Position).Length();
                        this.character.StepTo(block.Block.WorldPosition, distance, distance);

                        this.timeSinceLastAction = 0;
                    }
                }
            }

            // update elapsed time
            this.timeSinceLastAction += 1f / 10f; // updating at 10hz
            return null;
        }
    }
}
