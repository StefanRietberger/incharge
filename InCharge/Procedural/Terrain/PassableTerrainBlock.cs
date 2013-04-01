using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InCharge.Procedural.Terrain
{
    /// <summary>
    /// Decorator for terrain block which holds information about valid passable neighbor blocks
    /// </summary>
    public class PassableTerrainBlock
    {
        private TerrainBlock block;
        /// <summary>
        /// Decorated block reference
        /// </summary>
        public TerrainBlock Block
        {
            get { return block; }
            set { block = value; }
        }

        private Dictionary<WorldOrientation, PassableTerrainBlock> passableNeighbors = new Dictionary<WorldOrientation,PassableTerrainBlock>();
        /// <summary>
        /// Contains neighbors which can be reached by a character from this block
        /// </summary>
        public Dictionary<WorldOrientation, PassableTerrainBlock> PassableNeighbors
        {
            get { return passableNeighbors; }
            set { passableNeighbors = value; }
        }

        public PassableTerrainBlock(TerrainBlock block)
        {
            this.block = block;

            this.passableNeighbors.Add(WorldOrientation.Up, null);
            this.passableNeighbors.Add(WorldOrientation.Down, null);
            this.passableNeighbors.Add(WorldOrientation.North, null);
            this.passableNeighbors.Add(WorldOrientation.East, null);
            this.passableNeighbors.Add(WorldOrientation.South, null);
            this.passableNeighbors.Add(WorldOrientation.West, null);
        }
    }
}
