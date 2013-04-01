using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;

namespace InCharge.Rendering.Model
{
    public class TexturedSurface : IDisposable
    {
        private static int[] QuadIndices = new int[6] { 0, 1, 2, 2, 1, 3 };

        private VertexPositionNormalTextureBump[] vertices;

        public VertexPositionNormalTextureBump[] Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }
        private int vertexCount;
        /// <summary>
        /// Number of actual geometry vertices
        /// </summary>
        public int VertexCount
        {
            get { return vertexCount; }
            set { vertexCount = value; }
        }
        private int triangleCount;
        /// <summary>
        /// Number of actual triangles
        /// </summary>
        public int TriangleCount
        {
            get { return triangleCount; }
            set { triangleCount = value; }
        }
        private int[] indices;

        public int[] Indices
        {
            get { return indices; }
            set { indices = value; }
        }

        private Texture2D texture;

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }       

        /// <summary>
        /// Creates a textured surface object for a quad. Add vertices to complete.
        /// </summary>
        /// <returns></returns>
        public static TexturedSurface CreateQuad()
        {
            TexturedSurface ts = new TexturedSurface();
            ts.Vertices = new VertexPositionNormalTextureBump[4];
            ts.Indices = TexturedSurface.QuadIndices;

            ts.VertexCount = 4;
            ts.TriangleCount = 2;
            return ts;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}
