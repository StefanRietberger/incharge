using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynapseGaming.LightingSystem.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InCharge.Rendering
{
    public struct VertexAtlas : IVertexType
    {
        //
        // Summary:
        //     The vertex position.
        public Vector3 Position;
        //
        // Summary:
        //     The vertex normal.
        public Vector3 Normal;
        //
        // Summary:
        //     The texture coordinates.
        public Vector4 TextureCoordinate;
        //
        // Summary:
        //     Tangent space tangent element used in bump / specular mapping.
        public Vector3 Tangent;
        // Summary:
        //     Tangent space binormal element used in bump / specular mapping.
        public Vector3 Binormal;                   
        
        /// <summary>
        /// Blend coordinates 1 and 2
        /// </summary>
        public Vector4 AtlasBlendCoordinate12;
        /// <summary>
        /// Blend coordinates 3 and 4
        /// </summary>
        public Vector4 AtlasBlendCoordinate34;        

        public static readonly VertexDeclaration VertexDecl = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(40, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(52, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(80, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2));

        public static void BuildTangentSpaceDataForTriangleList(int[] indicesArray, VertexAtlas[] verticesArray)
        {
            Vector3[] tan1 = new Vector3[verticesArray.Length];
            Vector3[] tan2 = new Vector3[verticesArray.Length];
    
            var triCount = indicesArray.Length / 3;

            for (int a = 0; a < triCount; a++)
            {
                var i1 = indicesArray[a * 3];
                var i2 = indicesArray[a * 3 + 1];
                var i3 = indicesArray[a * 3 + 2];
                
                var v1 = verticesArray[i1].Position;
                var v2 = verticesArray[i2].Position;
                var v3 = verticesArray[i3].Position;

                var w1 = verticesArray[i1].TextureCoordinate;
                var w2 = verticesArray[i2].TextureCoordinate;
                var w3 = verticesArray[i3].TextureCoordinate;                
        
                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;
        
                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;
        
                float r = 1.0F / (s1 * t2 - s2 * t1);
                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
                        (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,
                        (s1 * z2 - s2 * z1) * r);

                tan1[i1] = sdir;
                tan1[i2] = sdir;
                tan1[i3] = sdir;
        
                tan2[i1] = tdir;
                tan2[i2] = tdir;
                tan2[i3] = tdir;

            }
    
            for (int a = 0; a < verticesArray.Length; a++)
            {
                Vector3 n = verticesArray[a].Normal;
                Vector3 t = tan1[a];
        
                // Gram-Schmidt orthogonalize
                verticesArray[a].Tangent = (t - n * Vector3.Dot(n, t));
                verticesArray[a].Tangent.Normalize();
        
                // Calculate handedness
                var handedness = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0F) ? 1.0F : -1.0F;
                //verticesArray[a].Tangent.W = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F;

                // B = (N × T) · Tw
                // calc binormal
                verticesArray[a].Binormal = Vector3.Cross(n, t) * handedness;
                verticesArray[a].Binormal.Normalize();
            }
        }


        #region IVertexType Members
        
        public VertexDeclaration VertexDeclaration
        {
            get { return VertexAtlas.VertexDecl; }
        }

        #endregion
    }
}
