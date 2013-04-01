using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InCharge.Util;

namespace InCharge.Procedural.Terrain
{
    [Serializable]
    public class Region
    {
        public const int RegionWidth = 16;
        public const int RegionLength = 16;
        public const int RegionHeight = 128;

        public const int BlockIndexingFactorX = RegionLength * RegionHeight;
        public const int BlockIndexingFactorZ = RegionHeight;

        /// <summary>
        /// Change flag for representations
        /// </summary>
        public bool HasChanged { get; set; }

        private byte[] blocks;
        /// <summary>
        /// Terrainblocks in this region
        /// </summary>
        public byte[] Blocks
        {
            get { return blocks; }
            set { blocks = value; }
        }        

        /// <summary>
        /// Creates a new instance of region
        /// </summary>
        public Region(int originX, int originZ)
        {
            this.OriginX = originX;
            this.OriginZ = originZ;

            this.Neighbors = new Dictionary<WorldOrientation, Region>(4);
            this.Neighbors.Add(WorldOrientation.North, null);
            this.Neighbors.Add(WorldOrientation.East, null);
            this.Neighbors.Add(WorldOrientation.South, null);
            this.Neighbors.Add(WorldOrientation.West, null);
        }
        /// <summary>
        /// World coordinate of X origin of this region
        /// </summary>
        public int OriginX { get; set; }
        /// <summary>
        /// World coordinate of Z origin of this region
        /// </summary>
        public int OriginZ { get; set; }       

        /// <summary>
        /// Collection of blocks which are directly accessible (are not completely encased in other blocks)
        /// </summary>
        public SparseMatrix3D<BlockPosition> AccessibleBlocks { get; private set; }
        /// <summary>
        /// Neighboring regions
        /// </summary>
        public Dictionary<WorldOrientation, Region> Neighbors { get; set; }
        
        /// <summary>
        /// Returns an array of the neighboring block types for a given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="blockIndex">Array of byte, containing the neighbor types in the order of Up, Down, North, East, South, West</param>
        /// <returns></returns>
        public byte[] GetBlockNeighbors(int x, int y, int z, bool onlyDirectNeighbors)
        {
            var blockIndex = GetBlockIndexForCoords(x, y, z);
            byte up, down, north, east, south, west;

            int index = 0;

            // up
            index = y < Region.RegionHeight-1 ? Region.GetBlockIndexForCoords(x, y + 1, z) : -1;
            up = index < 0 ? BlockTypes.None : this.blocks[index];

            // down
            index = y > 0 ? Region.GetBlockIndexForCoords(x, y - 1, z) : -1;
            down = index < 0 ? BlockTypes.None : this.blocks[index];

            // north
            var region = this;
            index = Region.GetBlockIndexForCoords(ref region, x, y, z - 1);
            north = index < 0 ? BlockTypes.None : region.blocks[index];

            // east
            region = this;
            index = Region.GetBlockIndexForCoords(ref region, x + 1, y, z);
            east = index < 0 ? BlockTypes.None : region.blocks[index];

            // south
            region = this;
            index = Region.GetBlockIndexForCoords(ref region, x, y, z + 1);
            south = index < 0 ? BlockTypes.None : region.blocks[index];

            // west
            region = this;
            index = Region.GetBlockIndexForCoords(ref region, x - 1, y, z);
            west = index < 0 ? BlockTypes.None : region.blocks[index];

            // stop here if only direct neighbors are required
            if (onlyDirectNeighbors)
            {
                var result = new byte[6];
                result[0] = up;
                result[1] = down;
                result[2] = north;
                result[3] = east;
                result[4] = south;
                result[5] = west;

                return result;
            }
            // additional neighbors:
            byte northUp, northDown, eastUp, eastDown, southUp, southDown, westUp, westDown;

            // north up
            region = this;
            index = y < Region.RegionHeight-1 ? Region.GetBlockIndexForCoords(ref region, x, y + 1, z - 1): -1;
            northUp = index < 0 ? BlockTypes.None : region.blocks[index];

            // north down
            region = this;
            index = y > 0 ?Region.GetBlockIndexForCoords(ref region, x, y - 1, z - 1): -1;
            northDown = index < 0 ? BlockTypes.None : region.blocks[index];

            // east up
            region = this;
            index = y < Region.RegionHeight-1 ? Region.GetBlockIndexForCoords(ref region, x + 1, y + 1, z) : 0;
            eastUp = index < 0 ? BlockTypes.None : region.blocks[index];

            // east down
            region = this;
            index = y > 0 ?Region.GetBlockIndexForCoords(ref region, x + 1, y - 1, z) : -1;
            eastDown = index < 0 ? BlockTypes.None : region.blocks[index];

            // south up
            region = this;
            index = y < Region.RegionHeight-1 ? Region.GetBlockIndexForCoords(ref region, x, y + 1, z + 1) : -1;
            southUp = index < 0 ? BlockTypes.None : region.blocks[index];

            // south down
            region = this;
            index = y > 0 ?Region.GetBlockIndexForCoords(ref region, x, y - 1, z + 1) : -1;
            southDown = index < 0 ? BlockTypes.None : region.blocks[index];

            // west up
            region = this;
            index = y < Region.RegionHeight-1 ? Region.GetBlockIndexForCoords(ref region, x - 1, y + 1, z): -1;
            westUp = index < 0 ? BlockTypes.None : region.blocks[index];

            // west down
            region = this;
            index = y > 0 ?Region.GetBlockIndexForCoords(ref region, x - 1, y - 1, z) : -1;
            westDown = index < 0 ? BlockTypes.None : region.blocks[index];

            var resultLong = new byte[14];
            resultLong[0] = up;
            resultLong[1] = down;
            resultLong[2] = north;
            resultLong[3] = east;
            resultLong[4] = south;
            resultLong[5] = west;
            resultLong[6] = northUp;
            resultLong[7] = northDown;
            resultLong[8] = eastUp;
            resultLong[9] = eastDown;
            resultLong[10] = southUp;
            resultLong[11] = southDown;
            resultLong[12] = westUp;
            resultLong[13] = westDown;            

            return resultLong;
        }

        public bool HasSunlightOnBlock(int x, int y, int z)
        {
            var blockIndex = GetBlockIndexForCoords(x, y, z);
            for (int i = blockIndex + 1, height = y; height < Region.RegionHeight - 1; i++, height++)
            {
                if (this.blocks[i] != BlockTypes.None) // block index can simply be incremented because of the data layout
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates the block index for the given relative region coordinates. Coordinates
        /// exceeding the current region will be looked up in neighbor regions, where possible.
        /// </summary>
        /// <param name="region">Base region for relative lookup of block index, changes referentially to absolute region for
        /// which the given index is valid</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>Block index in the region which is referentially changed to accept the index or -1 if the coordinates are invalid</returns>
        private static int GetBlockIndexForCoords(ref Region region, int x, int y, int z)
        {
            while (x < 0 && region != null)
            {
                region = region.Neighbors[WorldOrientation.West];
                x += Region.RegionWidth;
            }
            while (x >= Region.RegionWidth && region != null)
            {
                region = region.Neighbors[WorldOrientation.East];
                x -= Region.RegionWidth;
            }
            while (z < 0 && region != null)
            {
                region = region.Neighbors[WorldOrientation.North];
                z += Region.RegionLength;
            }
            while (z >= Region.RegionLength && region != null)
            {
                region = region.Neighbors[WorldOrientation.South];
                z -= Region.RegionLength;
            }
            if (region != null)
            {
                var blockIndex = x * Region.BlockIndexingFactorX + z * Region.BlockIndexingFactorZ + y;
                return blockIndex;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns the block index for a block with the given relative coordinates within any single region
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static int GetBlockIndexForCoords(int x, int y, int z)
        {
            var blockIndex = x * Region.BlockIndexingFactorX + z * Region.BlockIndexingFactorZ + y;
            return blockIndex;
        }



        /// <summary>
        /// Sets a block at a given position
        /// </summary>
        /// <param name="blockType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetBlock(byte blockType, int x, int y, int z)
        {
            var index = Region.GetBlockIndexForCoords(x, y, z);
            this.blocks[index] = blockType;
        }
        
        /// <summary>
        /// Get a block from the given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetBlock(int x, int y, int z)
        {
            var index = Region.GetBlockIndexForCoords(x, y, z);
            return this.blocks[index];
        }

        /// <summary>
        /// Finds all accessible blocks
        /// </summary>
        public void CalculateAccessibleBlocks()
        {
            // set loop range to cut off the outermost blocks of the terrain
            int startX = this.Neighbors[WorldOrientation.West] != null ? 0 : 1;
            int endX = this.Neighbors[WorldOrientation.East] != null ? RegionWidth : RegionWidth - 1;
            int startZ = this.Neighbors[WorldOrientation.North] != null ? 0 : 1;
            int endZ = this.Neighbors[WorldOrientation.South] != null ? RegionLength : RegionLength - 1;

            SparseMatrix3D<BlockPosition> accBlocks = new SparseMatrix3D<BlockPosition>();
            for (int x = startX; x < endX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    for (int y = 0; y < Region.RegionHeight; y++) // calculate from bottom up 
                    {
                        var blockIndex = Region.GetBlockIndexForCoords(x, y, z);

                        var block = this.blocks[blockIndex];
                        if (block == BlockTypes.None) continue;

                        var blockNeighbors = this.GetBlockNeighbors(x, y, z, true);

                        bool isAccessible = false;
                        isAccessible |= blockNeighbors[0] == BlockTypes.None; // up
                        isAccessible |= blockNeighbors[2] == BlockTypes.None; // north
                        isAccessible |= blockNeighbors[3] == BlockTypes.None; // east
                        isAccessible |= blockNeighbors[4] == BlockTypes.None; // south
                        isAccessible |= blockNeighbors[5] == BlockTypes.None; // west

                        if (isAccessible)
                        {
                            // change to grass if soil block has sunlight
                            if (block == BlockTypes.Soil && this.HasSunlightOnBlock(x, y, z))
                            {
                                this.blocks[blockIndex] = BlockTypes.Grass;
                            }

                            BlockPosition pos = new BlockPosition(x, y, z, this.OriginX, this.OriginZ);
                            // add block position
                            accBlocks.SetAt(x, y, z, pos);
                            //accBlocks.Add(pos);
                        }
                    }
                }
            }

            this.AccessibleBlocks = accBlocks;
        }

        /// <summary>
        /// Connect the adjacent blocks of regions to each other
        /// </summary>
        /// <param name="north">North neighbor region</param>
        /// <param name="east">East neighbor region</param>
        /// <param name="south">South neighbor region</param>
        /// <param name="west">West neighbor region</param>
        public void ConnectRegions(Region east, Region south)
        {
            if (east != null)
            {
                this.Neighbors[WorldOrientation.East] = east;
                east.Neighbors[WorldOrientation.West] = this;
            }
            if (south != null)
            {
                this.Neighbors[WorldOrientation.South] = south;
                south.Neighbors[WorldOrientation.North] = this;
            }
        }
    }
}
