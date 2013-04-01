using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;
using Microsoft.Xna.Framework;

namespace InCharge.Procedural
{
    /// <summary>
    /// Provides creation methods for 3D primitives
    /// </summary>
    public class PrimitivesHelper
    {
        /// <summary>
        /// Base indices for a quad
        /// </summary>
        private static int[] baseQuadIndices = { 0, 1, 2, 2, 3, 0 };

        public struct PrimitiveData
        {
            public VertexPositionNormalTextureBump[] Vertices;
            public int[] Indices;
        }

        /// <summary>
        /// Creates vertices and indices for a box with the given dimensions
        /// </summary>
        /// <param name="widthX"></param>
        /// <param name="heightY"></param>
        /// <param name="lengthZ"></param>
        /// <returns></returns>
        public static PrimitiveData CreateBox(float widthX, float heightY, float lengthZ)
        {
            PrimitiveData result;
            result.Vertices = new VertexPositionNormalTextureBump[6 * 4]; // 6 sides, 4 corners
            result.Indices = new int[6 * 2 * 3]; // 6 sides, 2 triangles each, 3 points per triangle

            // set corners
            var corners = new Vector3[8];
            corners[0] = Vector3.Zero;                              // 0 left   bottom  front 
            corners[1] = new Vector3(widthX, 0, 0);                 // 1 right  bottom  front
            corners[2] = new Vector3(0, 0, -lengthZ);               // 2 left   bottom  back
            corners[3] = new Vector3(widthX, 0, -lengthZ);          // 3 right  bottom  back
            corners[4] = new Vector3(0, heightY, 0);                // 4 left   top     front
            corners[5] = new Vector3(widthX, heightY, 0);           // 5 right  top     front
            corners[6] = new Vector3(0, heightY, -lengthZ);         // 6 left   top     back
            corners[7] = new Vector3(widthX, heightY, -lengthZ);    // 7 right  top     back

            // top box side
            result.Vertices[0].Position = corners[6];
            result.Vertices[1].Position = corners[7];
            result.Vertices[2].Position = corners[5];
            result.Vertices[3].Position = corners[4];

            for (int i = 0; i < 4; i++) result.Vertices[i].Normal = Vector3.Up;

            // bottom box side
            result.Vertices[4].Position = corners[0];
            result.Vertices[5].Position = corners[1];
            result.Vertices[6].Position = corners[3];
            result.Vertices[7].Position = corners[2];

            for (int i = 4; i < 8; i++) result.Vertices[i].Normal = Vector3.Down;

            // left box side
            result.Vertices[8].Position = corners[6];
            result.Vertices[9].Position = corners[4];
            result.Vertices[10].Position = corners[0];
            result.Vertices[11].Position = corners[2];

            for (int i = 8; i < 12; i++) result.Vertices[i].Normal = Vector3.Left;

            // right box side
            result.Vertices[12].Position = corners[5];
            result.Vertices[13].Position = corners[7];
            result.Vertices[14].Position = corners[3];
            result.Vertices[15].Position = corners[1];

            for (int i = 12; i < 16; i++) result.Vertices[i].Normal = Vector3.Right;

            // front side
            result.Vertices[16].Position = corners[4];
            result.Vertices[17].Position = corners[5];
            result.Vertices[18].Position = corners[1];
            result.Vertices[19].Position = corners[0];

            for (int i = 16; i < 20; i++) result.Vertices[i].Normal = Vector3.Forward;

            // back side
            result.Vertices[20].Position = corners[7];
            result.Vertices[21].Position = corners[6];
            result.Vertices[22].Position = corners[2];
            result.Vertices[23].Position = corners[3];

            for (int i = 20; i < 24; i++) result.Vertices[i].Normal = Vector3.Backward;

            // assign texture coordinates and indices
            for (int i = 0; i < 6; i++)
            {
                var vertexOffset = i * 4;
                var indexOffset = i * 6;

                result.Vertices[vertexOffset].TextureCoordinate = Vector2.Zero;
                result.Vertices[vertexOffset + 1].TextureCoordinate = Vector2.UnitX;
                result.Vertices[vertexOffset + 2].TextureCoordinate = Vector2.One;
                result.Vertices[vertexOffset + 3].TextureCoordinate = Vector2.UnitY;

                result.Indices[indexOffset] = baseQuadIndices[0] + vertexOffset;
                result.Indices[indexOffset + 1] = baseQuadIndices[1] + vertexOffset;
                result.Indices[indexOffset + 2] = baseQuadIndices[2] + vertexOffset;
                result.Indices[indexOffset + 3] = baseQuadIndices[3] + vertexOffset;
                result.Indices[indexOffset + 4] = baseQuadIndices[4] + vertexOffset;
                result.Indices[indexOffset + 5] = baseQuadIndices[5] + vertexOffset;
            }

            // build tangent and bitangent data
            VertexPositionNormalTextureBump.BuildTangentSpaceDataForTriangleList<int>(result.Indices, result.Vertices);

            return result;
        }
    }
}
