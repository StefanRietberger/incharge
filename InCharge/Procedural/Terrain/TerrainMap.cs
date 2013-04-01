using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Effects;
using SynapseGaming.LightingSystem.Effects.Forward;
using SynapseGaming.LightingSystem.Core;
using InCharge.Util;
using Indiefreaks.Xna.Core;
using System.Runtime.Serialization;
using InCharge.Rendering.Model;

namespace InCharge.Procedural.Terrain
{
    [Serializable]
    public class TerrainMap : IContentHost
    {
        /// <summary>
        /// Minimum of free height above a surface for it to be passable
        /// </summary>
        public const float PASSABLE_HEIGHT = 2.0f;
        /// <summary>
        /// Minimum of free height above a surface for it to be passable, expressed in number of blocks
        /// </summary>
        public static int PASSABLE_BLOCK_HEIGHT = (int)Math.Ceiling(PASSABLE_HEIGHT / TerrainBlock.BLOCK_HEIGHT);
        /// <summary>
        /// Hover height of selection overlay over terrain
        /// </summary>
        public const float SELECTION_OVERLAY_FLOAT_HEIGHT = 0.06f;
        /// <summary>
        /// Hover height of movement overlay over terrain
        /// </summary>
        public const float MOVEMENT_OVERLAY_FLOAT_HEIGHT = 0.04f;
        /// <summary>
        /// Inward sloping of blocks without neighbors
        /// </summary>
        public const float EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING = 0.2f;
        /// <summary>
        /// Displacement map maximum amplitude
        /// </summary>
        public const double DISPLACEMENT_FACTOR = 0.1;

        public struct TerrainPickResult
        {
            public Region Region;
            public BlockPosition BlockPosition;
            public int FreeBlocksAbove;
        }

        /// <summary>
        /// Content manager reference for textures and models
        /// </summary>
        [NonSerialized]
        private ContentManager content;

        /// <summary>
        /// seed value for terrain generation
        /// </summary>
        private int seed;

        /// <summary>
        /// Noise generator
        /// </summary>
        [NonSerialized]
        private PerlinNoise heightMapNoise;
        [NonSerialized]
        private PerlinNoise rockNoise;
        [NonSerialized]
        private PerlinNoise clayNoise;
        [NonSerialized]
        private PerlinNoise soilNoise;

        private SparseMatrix<Region> regions;
        /// <summary>
        /// Sparse matrix of created regions
        /// </summary>
        public SparseMatrix<Region> Regions
        {
            get { return regions; }
            set { regions = value; }
        }

        [NonSerialized]
        private IEnumerable<SceneObject> surfaceSceneObjects;

        public IEnumerable<SceneObject> SurfaceSceneObjects
        {
            get { return surfaceSceneObjects; }
            set { surfaceSceneObjects = value; }
        }

        /// <summary>
        /// List of currently drawable region representations
        /// </summary>
        [NonSerialized]
        private Dictionary<Region, RegionPresentation> representations = new Dictionary<Region, RegionPresentation>();

        /// <summary>
        /// Seeded random instance
        /// </summary>
        [NonSerialized]
        private Random random;

        /// <summary>
        /// Graphics device reference
        /// </summary>
        [NonSerialized]
        private GraphicsDevice graphics;

        private Rectangle worldBounds;


        /// <summary>
        /// Creates a new instance of TerrainFactory
        /// </summary>
        /// <param name="content"></param>
        public TerrainMap(ContentManager content, GraphicsDevice graphics, int seed)
        {
            this.content = content;
            this.graphics = graphics;
            this.seed = seed;
            this.heightMapNoise = new PerlinNoise(seed, 8, 0.1, 0.0004, 1);
            this.rockNoise = new PerlinNoise(seed, 6, 0.20, 0.0060, 1);
            this.clayNoise = new PerlinNoise(seed, 8, 0.20, 0.0005, 1);
            this.soilNoise = new PerlinNoise(seed, 10, 0.11, 0.0001, 1);

            this.random = new Random(seed);
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static TerrainMap()
        {

        }

        /// <summary>
        /// Returns the region which contains the given world coordinates or null
        /// if the coordinates are outside of the map.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public Region GetRegionForWorldCoordinates(Vector3 coordinates)
        {
            var regionX = (int)(coordinates.X / Region.RegionWidth);
            var regionZ = (int)(coordinates.Z / Region.RegionLength);
            return this.regions[regionZ, regionX];
        }

        public BlockPosition GetBlockPositionForWorldCoordinates(Vector3 coordinates)
        {
            var region = this.GetRegionForWorldCoordinates(coordinates);
            int x = (int)(coordinates.X / TerrainBlock.BLOCK_DIAMETER) % Region.RegionWidth;
            int y = (int)(coordinates.Y / TerrainBlock.BLOCK_HEIGHT);
            int z = (int)(coordinates.Z / TerrainBlock.BLOCK_DIAMETER) % Region.RegionLength;
            BlockPosition result = new BlockPosition(x, y, z, region.OriginX, region.OriginZ);
            return result;
        }

        /// <summary>
        /// Builds a region
        /// </summary>
        /// <param name="chunkOriginX">X origin of the region</param>
        /// <param name="chunkOriginZ">Z origin of the region</param>
        private void BuildRegion(Region region)
        {
            byte[] blocks = new byte[Region.RegionWidth * Region.RegionHeight * Region.RegionLength];
            for (int x = 0; x < Region.RegionWidth; x++)
            {
                for (int z = 0; z < Region.RegionLength; z++)
                {
                    var rock = rockNoise.Noise(
                        (x + region.OriginX),
                        (z + region.OriginZ));
                    rock = rock * rock * rock * rock * (5 - 4 * rock);
                    rock = rock * Region.RegionHeight + 16; // offset rock sediment

                    var clay = clayNoise.Noise(
                        (x + region.OriginX),
                        (z + region.OriginZ)) * Region.RegionHeight - 40; // offset clay sediment

                    var soil = soilNoise.Noise(
                        (x + region.OriginX),
                        (z + region.OriginZ)) * Region.RegionHeight;

                    var maxHeight = Math.Max(rock, Math.Max(clay, soil));

                    for (int y = 0; y < Region.RegionHeight; y++)
                    {
                        var blockIndex = x * Region.BlockIndexingFactorX + z * Region.BlockIndexingFactorZ + y;

                        byte blockType = BlockTypes.None;

                        // fill up lowest layer with bedrock
                        if (y == 0)
                        {
                            blocks[blockIndex] = BlockTypes.BedRock;
                            continue;
                        }
                        else if (y <= rock)
                        {
                            blockType = BlockTypes.Rock;
                        }
                        else if (y <= clay)
                        {
                            blockType = BlockTypes.Clay;
                        }
                        else if (y <= soil)
                        {
                            blockType = BlockTypes.Soil;
                        }

                        blocks[blockIndex] = blockType;
                    }
                }
            }

            // store blocks in region
            region.Blocks = blocks;
        }

        /// <summary>
        /// Generate a new random battlefield
        /// </summary>
        /// <param name="width">Initial width in regions</param>
        /// <param name="length">Initian length in regions</param>
        public void GenerateRandomWorld(int width, int length)
        {
            this.regions = new SparseMatrix<Region>();
            
            this.worldBounds = new Rectangle(0, 0, width * Region.RegionWidth, length * Region.RegionLength);

            // build regions
            this.BuildAllRegions(width, length);
            this.ConnectAllRegions();
            foreach (var region in this.regions)
            {
                region.CalculateAccessibleBlocks();
            }

            //this.AddWater(worldBounds);

            // build mesh data and put into scene objects
            foreach (var region in this.regions)
            {               
                var rep = new RegionPresentation(region, this.worldBounds, this.content, this.graphics);
                this.representations.Add(region, rep);
            }
            this.UpdateSceneObjects();                  
        }

        /// <summary>
        /// Rebuilds terrain scene objects
        /// </summary>
        private void UpdateSceneObjects()
        {
            List<SceneObject> sceneObjects = new List<SceneObject>();
            foreach (var rep in this.representations.Values)
            {
                sceneObjects.Add(rep.PresentationSceneObject);
            }

            this.surfaceSceneObjects = sceneObjects;
        }

        /// <summary>
        /// Adds rivers and generic water bodies to the map
        /// </summary>
        /// <param name="worldBounds"></param>
        private void AddWater(Rectangle worldBounds)
        {
            // identify flow points

            // find highest points at map edge
            int inflows = 20;
            LinkedList<BlockPosition> highestBlocks = new LinkedList<BlockPosition>();
            for (int i = 0; i < inflows; i++)
            {
                highestBlocks.AddFirst(new BlockPosition(0, 0, Region.RegionHeight, 0, 0));
            }


            for (int z = this.regions.RowMin; z <= this.regions.RowMax; z++)
            {
                for (int x = this.regions.ColMin; x <= this.regions.ColMax; x++)
                {
                    if (z > this.regions.RowMin && z < this.regions.RowMax &&
                        x > this.regions.ColMin && x < this.regions.ColMax) continue;

                    var region = this.regions.GetAt(z, x);
                    if (region != null)
                    {
                        foreach (BlockPosition blockPos in region.AccessibleBlocks)
                        {
                            if ((z == 0 && blockPos.Z <= 1) || (z == this.regions.RowMax && z >= Region.RegionLength - 2) ||
                                (x == 0 && blockPos.X <= 1) || (x == this.regions.ColMax && x >= Region.RegionWidth - 2))
                            {
                                var highest = highestBlocks.First.Value;
                                if (blockPos.Y > highest.Y)
                                {
                                    var bpos = new BlockPosition(blockPos.X, blockPos.Y, blockPos.Z, x, z);

                                    highestBlocks.AddFirst(bpos);
                                    highestBlocks.RemoveLast();
                                }
                            }
                        }
                    }
                }
            }

            // DEBUG
            foreach (BlockPosition bpos in highestBlocks)
            {
                var region = this.regions.GetAt(bpos.regionZ, bpos.regionX);
                region.SetBlock(BlockTypes.Sand, bpos.X, bpos.Y, bpos.Z);
            }

            // find path towards the center of the map and downwards

            // carve riverbed by erosion
        }

        /// <summary>
        /// Builds regions and their representations (=meshes)
        /// </summary>
        private void BuildAllRegions(int width, int length)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    var reg = new Region(x * Region.RegionWidth, z * Region.RegionLength);
                    this.regions.SetAt(z, x, reg);

                    this.BuildRegion(reg);
                }
            }
        }

        private void RebuildRegionRepresentation(Region region)
        {
            region.CalculateAccessibleBlocks();
            var rep = new RegionPresentation(region, this.worldBounds, this.content, this.graphics);
            this.representations[region] = rep;
        }

        /// <summary>
        /// Connects blocks of regions adjacent to each other
        /// </summary>
        private void ConnectAllRegions()
        {
            for (int z = this.regions.RowMin; z <= this.regions.RowMax; z++)
            {
                for (int x = this.regions.ColMin; x <= this.regions.ColMax; x++)
                {
                    var region = this.regions.GetAt(z, x);
                    if (region != null)
                    {
                        // check each neighbor region, connect if non-null
                        var south = this.regions.GetAt(z + 1, x);
                        var east = this.regions.GetAt(z, x + 1);

                        region.ConnectRegions(east, south);                        
                    }
                }
            }
        }

        /// <summary>
        /// Get ray picked terrain block
        /// </summary>
        /// <param name="pickingRay"></param>
        /// <returns></returns>
        public TerrainPickResult? GetPickedTerrainBlock(Ray pickingRay)
        {
            var hits = new Dictionary<float, TerrainPickResult>();
            // TODO: only test regions which can be hit by ray
            for (int z = this.regions.RowMin; z <= this.regions.RowMax; z++)
            {
                for (int x = this.regions.ColMin; x <= this.regions.ColMax; x++)
                {
                    var region = this.regions.GetAt(z, x);
                    if (region != null)
                    {
                        // only test blocks from regions which are intersected by ray
                        var rbox = new BoundingBox()
                        {
                            Min = new Vector3()
                            {
                                X = region.OriginX,
                                Y = 0,
                                Z = region.OriginZ
                            },
                            Max = new Vector3()
                            {
                                X = region.OriginX + Region.RegionWidth,
                                Y = Region.RegionHeight,
                                Z = region.OriginZ + Region.RegionLength
                            }
                        };
                        var regionIntersect = pickingRay.Intersects(rbox);
                        if (regionIntersect.HasValue)
                        {
                            var accBlocks = region.AccessibleBlocks;

                            foreach (var blockPosition in accBlocks)
                            {
                                float? intersectResult;
                                var bbox = blockPosition.BoundBox;
                                // intersection test
                                pickingRay.Intersects(ref bbox, out intersectResult);

                                // if the ray went through the block, add it to hits
                                if (intersectResult.HasValue && !hits.ContainsKey(intersectResult.Value))
                                {
                                    hits.Add(intersectResult.Value, new TerrainPickResult { BlockPosition = blockPosition, Region = region });
                                }
                            }
                        }
                    }
                }
            }

            if (hits.Count > 0)
            {
                // order hits to get the nearest hit
                var ordered = hits.OrderBy(k => k.Key);
                var pick = ordered.First().Value;

                /*// calculate free blocks above picked block
                TerrainBlock upper = pick.Block;
                int freeBlockCount = 0;
                while ((upper = upper.GetNeighbor(WorldOrientation.Up)) != null) freeBlockCount++;
                pick.FreeBlocksAbove = freeBlockCount;*/

                hits.Clear();
                return pick;
            }

            return null;
        }

        #region IContentHost Members

        void IContentHost.LoadContent(IContentCatalogue catalogue, ContentManager manager)
        {
            this.GenerateRandomWorld(4, 4);
        }

        void IContentHost.UnloadContent(IContentCatalogue catalogue)
        {

        }

        #endregion

        internal void RemoveBlock(TerrainPickResult pickResult)
        {
            var blockPosition = pickResult.BlockPosition;
            pickResult.Region.SetBlock(BlockTypes.None, blockPosition.X, blockPosition.Y, blockPosition.Z);

            // rebuild neighbors first if removed block is on border
            if (pickResult.BlockPosition.X == 0 && pickResult.Region.Neighbors[WorldOrientation.West] != null)
            {
                this.RebuildRegionRepresentation(pickResult.Region.Neighbors[WorldOrientation.West]);
            }
            if (pickResult.BlockPosition.X == Region.RegionWidth-1 && pickResult.Region.Neighbors[WorldOrientation.East] != null)
            {
                this.RebuildRegionRepresentation(pickResult.Region.Neighbors[WorldOrientation.East]);
            }
            if (pickResult.BlockPosition.Z == 0 && pickResult.Region.Neighbors[WorldOrientation.North] != null)
            {
                this.RebuildRegionRepresentation(pickResult.Region.Neighbors[WorldOrientation.North]);
            }
            if (pickResult.BlockPosition.Z == Region.RegionLength-1 && pickResult.Region.Neighbors[WorldOrientation.South] != null)
            {
                this.RebuildRegionRepresentation(pickResult.Region.Neighbors[WorldOrientation.South]);
            }
            this.RebuildRegionRepresentation(pickResult.Region);
            this.UpdateSceneObjects();
        }
    }
}
