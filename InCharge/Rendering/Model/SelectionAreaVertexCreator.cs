using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynapseGaming.LightingSystem.Core;
using InCharge.Procedural.Terrain;
using Microsoft.Xna.Framework;
using InCharge.Procedural;

namespace InCharge.Rendering.Model
{
    class SelectionAreaVertexCreator
    {
        private static float selectionOffset = 0.1f;
        /// <summary>
        /// Base indices for a quad
        /// </summary>
        private static int[] baseQuadIndices = { 0, 1, 2, 2, 1, 3 }; 

        /// <summary>
        /// Creates vertices for a block surface
        /// </summary>
        /// <param name="TerrainFactory.QuadIndices"></param>
        /// <param name="surfaces"></param>
        /// <param name="tb"></param>
        /// <param name="orientation"></param>
        public static void CreateVertices(List<VertexPositionNormalTextureBump> vertexList, List<int> indexList, TerrainBlock tb, WorldOrientation orientation)
        {
            var vertices = new VertexPositionNormalTextureBump[4];

            switch (orientation)
            {
                case WorldOrientation.Up:
                    {
                        vertices[0].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[1].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[2].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        for (int i = 0; i < vertices.Length; i++) vertices[i].Normal = Vector3.Up;
                        break;
                    }
                case WorldOrientation.North:
                    {
                        vertices[0].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[1].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[2].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z);
                        vertices[3].Position = new Vector3(tb.Position.X, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z);
                        for (int i = 0; i < vertices.Length; i++) vertices[i].Normal = Vector3.UnitZ;
                        break;
                    }
                case WorldOrientation.East:
                    {
                        vertices[0].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[1].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[2].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z);
                        for (int i = 0; i < vertices.Length; i++) vertices[i].Normal = Vector3.UnitX;
                        break;
                    }
                case WorldOrientation.South:
                    {
                        vertices[0].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[1].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[2].Position = new Vector3(tb.Position.X, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[3].Position = new Vector3(tb.Position.X + TerrainBlock.BLOCK_DIAMETER, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        for (int i = 0; i < vertices.Length; i++) vertices[i].Normal = Vector3.UnitZ * -1f;
                        break;
                    }
                case WorldOrientation.West:
                    {
                        vertices[0].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z);
                        vertices[1].Position = new Vector3(tb.Position.X, tb.Position.Y + selectionOffset, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        vertices[2].Position = new Vector3(tb.Position.X, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z);
                        vertices[3].Position = new Vector3(tb.Position.X, tb.Position.Y - TerrainBlock.BLOCK_HEIGHT, tb.Position.Z + TerrainBlock.BLOCK_DIAMETER);
                        for (int i = 0; i < vertices.Length; i++) vertices[i].Normal = Vector3.UnitX * -1f;
                        break;
                    }
            }

            // move edges outward at volume border
            var padding = GetSelectionOffset(tb, orientation);
            for (int i = 0; i < 4; i++) vertices[i].Position += padding[i];

            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].TextureCoordinate = new Vector2(0, 1);
            vertices[3].TextureCoordinate = new Vector2(1, 1);

            var indices = (from i in baseQuadIndices
                           select i + vertexList.Count).ToArray();

            indexList.AddRange(indices);
            vertexList.AddRange(vertices);
        }

        /// <summary>
        /// Get selection offset vector for each corner
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private static Vector3[] GetSelectionOffset(TerrainBlock tb, WorldOrientation orientation)
        {
            var result = new Vector3[4];

            if (orientation == WorldOrientation.Up)
            {
                if (tb.GetNeighbor(WorldOrientation.North) == null)
                {
                    if (tb.GetNeighbor(WorldOrientation.West) == null)
                        result[0].Z -= selectionOffset;
                    if (tb.GetNeighbor(WorldOrientation.East) == null)
                        result[1].Z -= selectionOffset;
                }
                if (tb.GetNeighbor(WorldOrientation.South) == null)
                {
                    if (tb.GetNeighbor(WorldOrientation.West) == null)
                        result[2].Z += selectionOffset;
                    if (tb.GetNeighbor(WorldOrientation.East) == null)
                        result[3].Z += selectionOffset;
                }
                if (tb.GetNeighbor(WorldOrientation.West) == null)
                {
                    if (tb.GetNeighbor(WorldOrientation.North) == null)
                        result[0].X -= selectionOffset;
                    if (tb.GetNeighbor(WorldOrientation.South) == null)
                        result[2].X -= selectionOffset;
                }
                if (tb.GetNeighbor(WorldOrientation.East) == null)
                {
                    if (tb.GetNeighbor(WorldOrientation.North) == null)
                        result[1].X += selectionOffset;
                    if (tb.GetNeighbor(WorldOrientation.South) == null)
                        result[3].X += selectionOffset;
                }
            }
            else
            {
                if (tb.GetNeighbor(WorldOrientation.Up) == null)
                {
                    switch (orientation)
                    {
                        case WorldOrientation.North:
                            if (tb.GetNeighbor(WorldOrientation.East) == null)
                            {
                                result[0].Z -= selectionOffset;
                                result[0].X += selectionOffset;
                            }
                            if (tb.GetNeighbor(WorldOrientation.West) == null)
                            {
                                result[1].Z -= selectionOffset;
                                result[1].X -= selectionOffset;
                            }
                            break;
                        case WorldOrientation.East:
                            if (tb.GetNeighbor(WorldOrientation.South) == null)
                            {
                                result[0].X += selectionOffset;
                                result[0].Z += selectionOffset;
                            }
                            if (tb.GetNeighbor(WorldOrientation.North) == null)
                            {
                                result[1].X += selectionOffset;
                                result[1].Z -= selectionOffset;
                            }
                            break;
                        case WorldOrientation.South:
                            if (tb.GetNeighbor(WorldOrientation.West) == null)
                            {
                                result[0].Z += selectionOffset;
                                result[0].X -= selectionOffset;
                            }
                            if (tb.GetNeighbor(WorldOrientation.East) == null)
                            {
                                result[1].Z += selectionOffset;
                                result[1].X += selectionOffset;
                            }
                            break;
                        case WorldOrientation.West:
                            if (tb.GetNeighbor(WorldOrientation.North) == null)
                            {
                                result[0].X -= selectionOffset;
                                result[0].Z -= selectionOffset;
                            }
                            if (tb.GetNeighbor(WorldOrientation.South) == null)
                            {
                                result[1].X -= selectionOffset;
                                result[1].Z += selectionOffset;
                            }
                            break;
                    }
                }
            }

            return result;
        }
    }
}
