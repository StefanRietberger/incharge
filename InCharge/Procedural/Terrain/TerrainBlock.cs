using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace InCharge.Procedural.Terrain
{
    public class TerrainBlock
    {
        public struct IntVector3
        {
            public IntVector3(int x, int y, int z) { X = x; Y = y; Z = z; }
            public int X;
            public int Y;
            public int Z;
        }

        public const float BLOCK_HEIGHT = 0.5f;
        public const float BLOCK_DIAMETER = 1.0f;
        public static float HEIGHT_FACTOR = 1f / BLOCK_HEIGHT;
        public static float DIAMETER_FACTOR = 1f / BLOCK_DIAMETER;

        private bool isOccupied;
        /// <summary>
        /// Determines whether this block has an actor occupying the space above it
        /// </summary>
        public bool IsOccupied
        {
            get { return isOccupied; }
            set { isOccupied = value; }
        }

        private bool hasSunlight;
        /// <summary>
        /// Determines whether this block receives sunlight
        /// </summary>
        public bool HasSunlight
        {
            get { return hasSunlight; }
            set { hasSunlight = value; }
        }

        private IntVector3 indexPosition;
        /// <summary>
        /// Index position in terrain collections
        /// </summary>
        public IntVector3 IndexPosition
        {
            get { return indexPosition; }
            set { indexPosition = value; }
        }

        private Vector3 position;
        /// <summary>
        /// Block position in world space
        /// </summary>
        public Vector3 Position
        {
            get { return position; }           
        }

        private TerrainBlock[] neighbors = new TerrainBlock[6];
        /// <summary>
        /// Find this block's neighbors
        /// </summary>
        public TerrainBlock[] Neighbors
        {
            get { return this.neighbors; }
        }

        private BoundingBox boundingBox;
        /// <summary>
        /// Terrain block bounding box
        /// </summary>
        public BoundingBox BoundBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }
        /// <summary>
        /// This block's material type
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Creates a new instance of TerrainBlock
        /// </summary>
        public TerrainBlock(int x, int y, int z, int regionX, int regionZ)
        {
            this.hasSunlight = true;            

            this.position = new Vector3(
                regionX + x * TerrainBlock.BLOCK_DIAMETER,
                y * TerrainBlock.BLOCK_HEIGHT,
                regionZ + z * TerrainBlock.BLOCK_DIAMETER);
            this.indexPosition = new IntVector3(x, y, z);
            this.boundingBox = new BoundingBox(
                this.Position, 
                new Vector3(
                    this.Position.X + TerrainBlock.BLOCK_DIAMETER,
                    this.Position.Y + TerrainBlock.BLOCK_HEIGHT,
                    this.Position.Z + TerrainBlock.BLOCK_DIAMETER));
        }

        public TerrainBlock GetNeighbor(WorldOrientation side)
        {
            switch (side)
            {
                default:
                case WorldOrientation.Up: return this.neighbors[0];
                case WorldOrientation.Down: return this.neighbors[1];
                case WorldOrientation.North: return this.neighbors[2];
                case WorldOrientation.East: return this.neighbors[3];
                case WorldOrientation.South: return this.neighbors[4];
                case WorldOrientation.West: return this.neighbors[5];
            }
        }

        public void SetNeighbor(WorldOrientation side, TerrainBlock neighbor)
        {
            switch (side)
            {
                default:
                case WorldOrientation.Up: this.neighbors[0] = neighbor; break;
                case WorldOrientation.Down: this.neighbors[1] = neighbor; break;
                case WorldOrientation.North: this.neighbors[2] = neighbor; break;
                case WorldOrientation.East: this.neighbors[3] = neighbor; break;
                case WorldOrientation.South: this.neighbors[4] = neighbor; break;
                case WorldOrientation.West: this.neighbors[5] = neighbor; break;
            }
        }
    }
}
