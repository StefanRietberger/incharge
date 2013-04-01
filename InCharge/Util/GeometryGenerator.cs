using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SynapseGaming.LightingSystem.Core;
using InCharge.Procedural;

namespace InCharge.Util
{
    public class GeometryGenerator
    {
        private static int[] BOX_INDICES = {
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7,
            8, 9, 10, 8, 10, 11,
            12, 13, 14, 12, 14, 15,
            16, 17, 18, 16, 18, 19,
            20, 21, 22, 20, 22, 23            
            };

        public struct Box
        {
            public VertexPositionNormalTextureBump[] Vertices;
            public int[] Indices;

            public void SetTexCoords(WorldOrientation face, Vector2[] texCoords)
            {
                int offset = 0;
                switch (face)
                {
                    default:
                    case WorldOrientation.Up: offset = 0; break;
                    case WorldOrientation.North: offset = 4; break;
                    case WorldOrientation.East: offset = 8; break;
                    case WorldOrientation.South: offset = 12; break;
                    case WorldOrientation.West: offset = 16; break;
                    case WorldOrientation.Down: offset = 20; break;
                }

                for (int i = 0; i < 4; i++)
                {
                    this.Vertices[offset + i].TextureCoordinate = texCoords[i];
                }
            }
        }

        /// <summary>
        /// Creates vertices and normals for a box, all other vector components must be set by the caller
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public static Box CreateBox(Vector3 position, Vector3 dimensions)
        {
            Box box;
            box.Indices = BOX_INDICES;
            VertexPositionNormalTextureBump[] vertices = new VertexPositionNormalTextureBump[24];

            Vector3 tNW, tNE, tSW, tSE, bNW, bNE, bSW, bSE;
            tNW = position;
            tNE = new Vector3(position.X + dimensions.X, position.Y, position.Z);
            tSE = new Vector3(position.X + dimensions.X, position.Y, position.Z + dimensions.Z);
            tSW = new Vector3(position.X, position.Y, position.Z + dimensions.Z);
            bNW = new Vector3(position.X, position.Y - dimensions.Y, position.Z);
            bNE = new Vector3(position.X + dimensions.X, position.Y - dimensions.Y, position.Z);
            bSE = new Vector3(position.X + dimensions.X, position.Y - dimensions.Y, position.Z + dimensions.Z);
            bSW = new Vector3(position.X, position.Y - dimensions.Y, position.Z + dimensions.Z);
            // TODO: assign these points to correct vertices and indices

            // top face
            for (int i = 0; i < 4; i++) vertices[i].Normal = Vector3.Up;
            vertices[0].Position = tNW;
            vertices[1].Position = tNE;
            vertices[2].Position = tSE;
            vertices[3].Position = tSW;
            // north face
            for (int i = 4; i < 8; i++) vertices[i].Normal = -Vector3.UnitZ;
            vertices[4].Position = tNE;
            vertices[5].Position = tNW;
            vertices[6].Position = bNW;
            vertices[7].Position = bNE;
            // east face
            for (int i = 8; i < 12; i++) vertices[i].Normal = Vector3.UnitX;
            vertices[8].Position = tSE;
            vertices[9].Position = tNE;
            vertices[10].Position = bNE;
            vertices[11].Position = bSE;
            // south face
            for (int i = 12; i < 16; i++) vertices[i].Normal = Vector3.UnitZ;
            vertices[12].Position = tSW;
            vertices[13].Position = tSE;
            vertices[14].Position = bSE;
            vertices[15].Position = bSW;
            // west face
            for (int i = 16; i < 20; i++) vertices[i].Normal = -Vector3.UnitX;
            vertices[16].Position = tNW;
            vertices[17].Position = tSW;
            vertices[18].Position = bSW;
            vertices[19].Position = bNW;
            // bottom face
            for (int i = 20; i < 24; i++) vertices[i].Normal = Vector3.Down;
            vertices[20].Position = bSE;
            vertices[21].Position = bSW;
            vertices[22].Position = bNW;
            vertices[23].Position = bNE;            

            for (int i = 0; i < 6; i++)
            {
                vertices[i * 4].TextureCoordinate = Vector2.Zero;
                vertices[i * 4 + 1].TextureCoordinate = Vector2.UnitX;
                vertices[i * 4 + 2].TextureCoordinate = Vector2.One;
                vertices[i * 4 + 3].TextureCoordinate = Vector2.UnitY;
            }

            box.Vertices = vertices;
            return box;
        }
    }

    public struct IntVector3
    {
        public int X, Y, Z;

        public IntVector3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3 AsFloatVector()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        /// Returns an integer vector containing the floored values of a given float vector
        /// </summary>
        /// <param name="floatVector"></param>
        /// <returns></returns>
        public static IntVector3 FromFloatVector(Vector3 floatVector)
        {
            return new IntVector3((int)floatVector.X, (int)floatVector.Y, (int)floatVector.Z);
        }
    }
}
