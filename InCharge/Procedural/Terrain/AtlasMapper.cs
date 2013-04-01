using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InCharge.Rendering;

namespace InCharge.Procedural.Terrain
{
    class AtlasMapper
    {
        private const float tileFactor = 1f / (256f / 32f);
        private static float one = 1.00f * tileFactor;
        private static Random random = new Random();

        private static Vector2[] GetCoordsForTilePosition(int col, int row)
        {
            float tileCol = col * tileFactor;
            float tileRow = row * tileFactor;

            Vector2[] texCoords = new Vector2[4];

            texCoords[0] = new Vector2(tileCol, tileRow);
            texCoords[1] = new Vector2(tileCol + one, tileRow);
            texCoords[2] = new Vector2(tileCol, tileRow + one);
            texCoords[3] = new Vector2(tileCol + one, tileRow + one);

            return texCoords;
        }

        private static void RemoveNonOverlappingNeighbors(byte center, byte[] neighbors)
        {
            byte centerPriority = BlockTypes.OverlappingPriority[center];
            for (int i = 0; i < 4; i++)
            {
                if (centerPriority >= BlockTypes.OverlappingPriority[neighbors[i]])
                {
                    neighbors[i] = BlockTypes.None;
                }
            }
        }

        /// <summary>
        /// Assigns texture coordinates to vertices for atlas mapping and tile transition blending
        /// </summary>
        /// <param name="region"></param>
        /// <param name="blockPosition"></param>
        /// <param name="neighbors"></param>
        /// <param name="surfaceOrientation"></param>
        /// <param name="vertices"></param>
        public static void AssignTextureCoords(Region region, BlockPosition blockPosition, byte[] neighbors, WorldOrientation surfaceOrientation, VertexAtlas[] vertices)
        {
            Vector2[] baseTexCoords;
            
            var blockType = region.GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z);

            byte[] neighborBlockTypes = new byte[4]; // north, east, south, west (up, right, down, left for side faces)
            switch (surfaceOrientation)
            {
                case WorldOrientation.Up:
                    {
                        // assign relevant neighbors, indices based on designed array order
                        for (int i = 0; i < 4; i++)
                        {
                            var upperNeighborIndex = 6 + i * 2;
                            var sameLevelNeighborIndex = 2 + i;
                            if (neighbors[upperNeighborIndex] != BlockTypes.None)
                            {
                                neighborBlockTypes[i] = neighbors[upperNeighborIndex];
                            }
                            else
                            {
                                neighborBlockTypes[i] = neighbors[sameLevelNeighborIndex];
                            }
                        }
                                                                   
                        break;
                    }
                // side surfaces:
                case WorldOrientation.East:                   
                    neighborBlockTypes[0] = neighbors[8];
                    neighborBlockTypes[1] = neighbors[2];
                    neighborBlockTypes[2] = neighbors[9];
                    neighborBlockTypes[3] = neighbors[4];
                    break;
                case WorldOrientation.South:
                    neighborBlockTypes[0] = neighbors[10];
                    neighborBlockTypes[1] = neighbors[3];
                    neighborBlockTypes[2] = neighbors[11];
                    neighborBlockTypes[3] = neighbors[5];
                    break;
                case WorldOrientation.West:
                    neighborBlockTypes[0] = neighbors[12];
                    neighborBlockTypes[1] = neighbors[4];
                    neighborBlockTypes[2] = neighbors[13];
                    neighborBlockTypes[3] = neighbors[6];
                    break;
                case WorldOrientation.North:
                    neighborBlockTypes[0] = neighbors[6];
                    neighborBlockTypes[1] = neighbors[5];
                    neighborBlockTypes[2] = neighbors[7];
                    neighborBlockTypes[3] = neighbors[3];                   
                    break;
            }

            // grass is special
            switch (surfaceOrientation)
            {
                case WorldOrientation.North:
                case WorldOrientation.East:
                case WorldOrientation.West:
                case WorldOrientation.South:
                    if (neighborBlockTypes[1] == BlockTypes.Grass) // grass block to the right
                    {
                        neighborBlockTypes[1] = BlockTypes.Soil;
                    }
                    if (neighborBlockTypes[3] == BlockTypes.Grass) // grass block to the left
                    {
                        neighborBlockTypes[3] = BlockTypes.Soil;
                    }
                    break;
            }            
            if (blockType == BlockTypes.Grass && surfaceOrientation != WorldOrientation.Up)
            {
                neighborBlockTypes[0] = BlockTypes.Grass; // grass fringe on upper side               
                blockType = BlockTypes.Soil;
            }            

            AtlasMapper.RemoveNonOverlappingNeighbors(blockType, neighborBlockTypes);
            
            // base texture coordinates and blend map texture coordinates:
            baseTexCoords = AtlasMapper.GetCoordsForBlockType(blockType);
            vertices[0].TextureCoordinate = new Vector4(baseTexCoords[0], 0, 0);
            vertices[1].TextureCoordinate = new Vector4(baseTexCoords[1], 1, 0);
            vertices[2].TextureCoordinate = new Vector4(baseTexCoords[2], 0, 1);
            vertices[3].TextureCoordinate = new Vector4(baseTexCoords[3], 1, 1);

            // overlap texture coordinates
            var texCoords1 = AtlasMapper.GetCoordsForBlockType(neighborBlockTypes[0]); // north
            var texCoords2 = AtlasMapper.GetCoordsForBlockType(neighborBlockTypes[1]); // north
            var texCoords3 = AtlasMapper.GetCoordsForBlockType(neighborBlockTypes[2]); // north
            var texCoords4 = AtlasMapper.GetCoordsForBlockType(neighborBlockTypes[3]); // north

            for (int i = 0; i < 4; i++)
            {
                vertices[i].AtlasBlendCoordinate12 = new Vector4(texCoords1[i].X, texCoords1[i].Y, texCoords2[i].X, texCoords2[i].Y);
                vertices[i].AtlasBlendCoordinate34 = new Vector4(texCoords3[i].X, texCoords3[i].Y, texCoords4[i].X, texCoords4[i].Y);
            }
        }

        private static Vector2[] GetCoordsForBlockType(byte blockType)
        {
            switch (blockType)
            {
                default:
                case BlockTypes.None:
                    return new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
                case BlockTypes.Grass:
                    return AtlasMapper.GetCoordsForTilePosition(0, 0);
                case BlockTypes.Soil:
                    return AtlasMapper.GetCoordsForTilePosition(2, 0);
                case BlockTypes.Rock:
                    return AtlasMapper.GetCoordsForTilePosition(1, 0);
                case BlockTypes.Clay:
                    return AtlasMapper.GetCoordsForTilePosition(3, 0);
                case BlockTypes.Sand:
                    return AtlasMapper.GetCoordsForTilePosition(4, 0);
            }
        }
    }
}
