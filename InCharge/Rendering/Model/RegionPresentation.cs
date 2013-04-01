using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Effects.Forward;
using Microsoft.Xna.Framework.Content;
using InCharge.Procedural.Terrain;
using SynapseGaming.LightingSystem.Effects.Deferred;
using InCharge.Rendering;
using InCharge.Procedural;

namespace InCharge.Rendering.Model
{
    /// <summary>
    /// Scene-object creating decorator for terrain regions
    /// </summary>
    public class RegionPresentation : IPresentation
    {
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
        /// <summary>
        /// Displacement map for randomness on terrain
        /// </summary>
        private static Vector3[, ,] displacementMap;
        /// <summary>
        /// Base indices for a quad
        /// </summary>
        private static int[] baseQuadIndices = { 0, 1, 2, 2, 1, 3 };        

        private Texture2D texTerrain;
        private Texture2D texTerrainNormals;
        private Texture2D texBlendNorth;
        private Texture2D texBlendEast;
        private Texture2D texBlendSouth;
        private Texture2D texBlendWest;
        private DeferredSasEffect effectTerrain;

        private readonly Region region;
        private readonly ContentManager content;
        private readonly GraphicsDevice device;

        private SceneObject sceneObject;

        public Rectangle WorldBounds { private get; set; }

        static RegionPresentation()
        {          
            var width = Region.RegionWidth;
            var height = Region.RegionHeight;
            var length = Region.RegionLength;

            displacementMap = new Vector3[width, height, length];
            var random = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        displacementMap[x, y, z] = new Vector3(
                            (float)(random.NextDouble() * TerrainMap.DISPLACEMENT_FACTOR),
                            0,
                            (float)(random.NextDouble() * TerrainMap.DISPLACEMENT_FACTOR));
                    }
                }
            }
        }

        public RegionPresentation(Region region, Rectangle worldBounds, ContentManager content, GraphicsDevice device)
        {
            // TODO: check if textures get cached! Dispose or inject otherwise.
            this.texTerrain = content.Load<Texture2D>("Textures/Terrain/atlas_terrain");
            this.texBlendNorth = content.Load<Texture2D>("Textures/Terrain/blend_mask_tile_north");
            this.texBlendEast = content.Load<Texture2D>("Textures/Terrain/blend_mask_tile_east");
            this.texBlendSouth = content.Load<Texture2D>("Textures/Terrain/blend_mask_tile_south");
            this.texBlendWest = content.Load<Texture2D>("Textures/Terrain/blend_mask_tile_west");
            
            this.texTerrainNormals = content.Load<Texture2D>("Textures/Terrain/atlas_terrain_normals");
            this.effectTerrain = content.Load<DeferredSasEffect>("Effects/terrain_ground");

            this.effectTerrain.Parameters["BaseTexture"].SetValue(this.texTerrain);
            this.effectTerrain.Parameters["NormalMap"].SetValue(this.texTerrainNormals);
            this.effectTerrain.Parameters["BlendNorth"].SetValue(this.texBlendNorth);
            this.effectTerrain.Parameters["BlendEast"].SetValue(this.texBlendEast);
            this.effectTerrain.Parameters["BlendSouth"].SetValue(this.texBlendSouth);
            this.effectTerrain.Parameters["BlendWest"].SetValue(this.texBlendWest);

            this.region = region;
            this.content = content;
            this.device = device;
            this.WorldBounds = worldBounds;

            this.sceneObject = this.BuildSceneObject();
        }

        private SceneObject BuildSceneObject()
        {
            List<VertexAtlas> vertices = new List<VertexAtlas>();
            List<int> indices = new List<int>();

            foreach (var blockPos in this.region.AccessibleBlocks)
            {
                int wX = blockPos.X + this.region.OriginX;
                int wZ = blockPos.Z + this.region.OriginZ;                

                // get full list of neighbors
                var neighbors = this.region.GetBlockNeighbors(blockPos.X, blockPos.Y, blockPos.Z, false);

                // create north face
                if (neighbors[2] == BlockTypes.None && wZ > WorldBounds.Top)
                {
                    this.CreateVertices(vertices, indices, blockPos, neighbors, WorldOrientation.North);
                }
                // create east face
                if (neighbors[3] == BlockTypes.None && wX + TerrainBlock.BLOCK_DIAMETER < WorldBounds.Right)
                {
                    this.CreateVertices(vertices, indices, blockPos, neighbors, WorldOrientation.East);
                }
                // create south face
                if (neighbors[4] == BlockTypes.None && wZ + TerrainBlock.BLOCK_DIAMETER < WorldBounds.Bottom)
                {
                    this.CreateVertices(vertices, indices, blockPos, neighbors, WorldOrientation.South);
                }
                // create west face
                if (neighbors[5] == BlockTypes.None && wX > WorldBounds.Left)
                {
                    this.CreateVertices(vertices, indices, blockPos, neighbors, WorldOrientation.West);
                }
                // create top face
                if (neighbors[0] == BlockTypes.None)
                {
                    this.CreateVertices(vertices, indices, blockPos, neighbors, WorldOrientation.Up);
                }
            }            

            var indicesArray = indices.ToArray();
            var verticesArray = vertices.ToArray();

            // calc normals
            for (int i = 0; i < verticesArray.Length; i++)
                verticesArray[i].Normal = new Vector3(0, 0, 0);
             
            for (int i = 0; i < indicesArray.Length / 3; i++)
            {
                Vector3 firstvec = verticesArray[indicesArray[i * 3 + 1]].Position - verticesArray[indicesArray[i * 3]].Position;
                Vector3 secondvec = verticesArray[indicesArray[i * 3]].Position - verticesArray[indicesArray[i * 3 + 2]].Position;
                Vector3 normalreal = Vector3.Cross(firstvec, secondvec);
                normalreal.Normalize();
                verticesArray[indicesArray[i * 3]].Normal += normalreal;
                verticesArray[indicesArray[i * 3 + 1]].Normal += normalreal;
                verticesArray[indicesArray[i * 3 + 2]].Normal += normalreal;
            }
            for (int i = 0; i < verticesArray.Length; i++)
                verticesArray[i].Normal.Normalize();

            // calc tangent space stuff
            VertexAtlas.BuildTangentSpaceDataForTriangleList(indicesArray, verticesArray);

            MeshData md = new MeshData();
            md.VertexBuffer = new VertexBuffer(this.device, VertexAtlas.VertexDecl, verticesArray.Length, BufferUsage.None);
            md.VertexBuffer.SetData(verticesArray);
            md.IndexBuffer = new IndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, indicesArray.Length, BufferUsage.None);
            md.IndexBuffer.SetData(indicesArray);
            md.MeshToObject = Matrix.Identity;

            md.Effect = this.effectTerrain;
            md.PrimitiveCount = verticesArray.Length / 2;
            md.VertexCount = verticesArray.Length;
            md.InfiniteBounds = true;
            md.ObjectSpaceBoundingBox = new BoundingBox(
                new Vector3(0, 0, 0),
                new Vector3(Region.RegionWidth, Region.RegionHeight, Region.RegionLength));
            //md.ObjectSpaceBoundingSphere = new BoundingSphere(Vector3.Multiply(new Vector3(Region.RegionWidth, Region.RegionHeight, Region.RegionLength), 0.5f), Region.RegionHeight);
            // ...
            var so = new SceneObject(md, String.Format("Region_X:{0}_Z:{1}", this.region.OriginX, this.region.OriginZ));
            so.StaticLightingType = SynapseGaming.LightingSystem.Lights.StaticLightingType.Composite;
            //so.World = Matrix.CreateTranslation(this.region.OriginX, 0, this.region.OriginZ);
            so.HullType = HullType.Mesh;
            so.AffectedByGravity = false;
            so.CollisionType = SynapseGaming.LightingSystem.Collision.CollisionType.Collide;

            return so;
        }

        /// <summary>
        /// Creates vertices for a block surface
        /// </summary>
        /// <param name="TerrainFactory.QuadIndices"></param>
        /// <param name="surfaces"></param>
        /// <param name="blockPosition"></param>
        /// <param name="orientation"></param>
        private void CreateVertices(List<VertexAtlas> vertexList, List<int> indexList, BlockPosition blockPosition, byte[] blockNeighbors, WorldOrientation orientation)
        {
            var vertices = new VertexAtlas[4];

            var gPos = new Vector3(
                blockPosition.regionX + blockPosition.X * TerrainBlock.BLOCK_DIAMETER, 
                blockPosition.Y * TerrainBlock.BLOCK_HEIGHT, 
                blockPosition.regionZ + blockPosition.Z * TerrainBlock.BLOCK_DIAMETER);


            switch (orientation)
            {
                case WorldOrientation.Up:
                    {
                        vertices[0].Position = new Vector3(gPos.X, gPos.Y, gPos.Z);
                        vertices[1].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z);
                        vertices[2].Position = new Vector3(gPos.X, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);                       
                        break;
                    }
                case WorldOrientation.North:
                    {
                        vertices[0].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z);
                        vertices[1].Position = new Vector3(gPos.X, gPos.Y, gPos.Z);
                        vertices[2].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z);
                        vertices[3].Position = new Vector3(gPos.X, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z);

                        break;
                    }
                case WorldOrientation.East:
                    {
                        vertices[0].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[1].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z);
                        vertices[2].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z);

                        break;
                    }
                case WorldOrientation.South:
                    {
                        vertices[0].Position = new Vector3(gPos.X, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[1].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[2].Position = new Vector3(gPos.X, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(gPos.X + TerrainBlock.BLOCK_DIAMETER, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        break;
                    }
                case WorldOrientation.West:
                    {
                        vertices[0].Position = new Vector3(gPos.X, gPos.Y, gPos.Z);
                        vertices[1].Position = new Vector3(gPos.X, gPos.Y, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[2].Position = new Vector3(gPos.X, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z);
                        vertices[3].Position = new Vector3(gPos.X, gPos.Y - TerrainBlock.BLOCK_HEIGHT, gPos.Z + TerrainBlock.BLOCK_DIAMETER);
                        break;
                    }
            }

            AtlasMapper.AssignTextureCoords(this.region, blockPosition, blockNeighbors, orientation, vertices);            


            // geometry modifications
            var padding = this.GetPaddingForBlockSurfaceOrientation(blockNeighbors, orientation);
            
            for (int i = 0; i < 4; i++) 
            {
                // displace positions for a little variety
                vertices[i].Position = this.GetDisplacedVector(vertices[i].Position);
                // move edges inward for slopes
                vertices[i].Position += padding[i];                
            }            

            var indices = (from i in baseQuadIndices
                          select i + vertexList.Count).ToArray();

            indexList.AddRange(indices);
            vertexList.AddRange(vertices);
        }       

        /// <summary>
        /// Adds displacement
        /// </summary>
        /// <param name="origVector"></param>
        /// <returns></returns>
        private Vector3 GetDisplacedVector(Vector3 origVector)
        {
            int x = (int)(origVector.X * TerrainBlock.DIAMETER_FACTOR);
            int y = 1 + (int)(origVector.Y * TerrainBlock.HEIGHT_FACTOR);
            int z = (int)(origVector.Z * TerrainBlock.DIAMETER_FACTOR);

            return origVector + RegionPresentation.displacementMap[
                x % RegionPresentation.displacementMap.GetLength(0), 
                y % RegionPresentation.displacementMap.GetLength(1), 
                z % RegionPresentation.displacementMap.GetLength(2)];
        }

        /// <summary>
        /// Get surface inward padding reduction vector for each corner
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private Vector3[] GetPaddingForBlockSurfaceOrientation(byte[] neighbors, WorldOrientation orientation)
        {
            var result = new Vector3[4];           

            if (orientation == WorldOrientation.Up)
            {
                if (neighbors[2] == BlockTypes.None)
                {
                    if (neighbors[5] == BlockTypes.None)
                        result[0].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                    if (neighbors[3] == BlockTypes.None)
                        result[1].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                }
                if (neighbors[4] == BlockTypes.None)
                {
                    if (neighbors[5] == BlockTypes.None)
                        result[2].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                    if (neighbors[3] == BlockTypes.None)
                        result[3].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                }
                if (neighbors[5] == BlockTypes.None)
                {
                    if (neighbors[2] == BlockTypes.None)
                        result[0].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                    if (neighbors[4] == BlockTypes.None)
                        result[2].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                }
                if (neighbors[3] == BlockTypes.None)
                {
                    if (neighbors[2] == BlockTypes.None)
                        result[1].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                    if (neighbors[4] == BlockTypes.None)
                        result[3].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                }
            }
            else
            {
                if (neighbors[0] == BlockTypes.None)
                {
                    switch (orientation)
                    {
                        case WorldOrientation.North:
                            if (neighbors[3] == BlockTypes.None)
                            {
                                result[0].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[0].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            if (neighbors[5] == BlockTypes.None)
                            {
                                result[1].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[1].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            break;
                        case WorldOrientation.East:
                            if (neighbors[4] == BlockTypes.None)
                            {
                                result[0].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[0].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            if (neighbors[2] == BlockTypes.None)
                            {
                                result[1].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[1].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            break;
                        case WorldOrientation.South:
                            if (neighbors[5] == BlockTypes.None)
                            {
                                result[0].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[0].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            if (neighbors[3] == BlockTypes.None)
                            {
                                result[1].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[1].X -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            break;
                        case WorldOrientation.West:
                            if (neighbors[2] == BlockTypes.None)
                            {
                                result[0].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[0].Z += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            if (neighbors[4] == BlockTypes.None)
                            {
                                result[1].X += TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                                result[1].Z -= TerrainMap.EMPTY_NEIGHBOR_BLOCK_INWARD_PADDING;
                            }
                            break;
                    }
                }
            }

            return result;
        }

        #region IPresentation Members

        public SceneObject PresentationSceneObject
        {
            get { return this.sceneObject; }
        }

        #endregion
    }
}
