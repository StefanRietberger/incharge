using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace InCharge.Procedural.Terrain
{
    /// <summary>
    /// Stores information about a possbible block position, contains region relative coordinates and region origins
    /// </summary>
    [Serializable]
    public struct BlockPosition : IEquatable<BlockPosition>
    {
        public readonly int X, Y, Z, 
            regionX, regionZ;

        private readonly Vector3 worldPos;
        private readonly BoundingBox boundingBox;

        private static readonly Vector3 blockBound = new Vector3(
            TerrainBlock.BLOCK_DIAMETER,
            TerrainBlock.BLOCK_HEIGHT,
            TerrainBlock.BLOCK_DIAMETER);

        public BlockPosition(int X, int Y, int Z, int regionX, int regionZ)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.regionX = regionX;
            this.regionZ = regionZ;
            this.worldPos = new Vector3(
                (regionX + X) * TerrainBlock.BLOCK_DIAMETER,
                Y * TerrainBlock.BLOCK_HEIGHT,
                (regionZ + Z) * TerrainBlock.BLOCK_DIAMETER);
            var boundMax = Vector3.Add(this.worldPos, BlockPosition.blockBound);
            this.boundingBox = new BoundingBox(this.worldPos, boundMax);
        }

        public Vector3 WorldPosition
        {
            get { return this.worldPos; }
        }

        public BoundingBox BoundBox
        {
            get { return this.boundingBox; }
        }

        public override int GetHashCode()
        {
            return (int)(this.X ^ 0xf00f00f0 + this.Z ^ 0x0f00f00f + this.Y ^ 0x00f00f00);
        }

        public override bool Equals(object other)
        {
            return other is BlockPosition ? Equals((BlockPosition)other) : false;

        }

        #region IEquatable<BlockPosition> Members

        public bool Equals(BlockPosition other)
        {
            // equal if all world coordinates are the same
            return other.X == this.X && other.Z == this.Z && other.Y == this.Y;
        }

        #endregion
    }
}
