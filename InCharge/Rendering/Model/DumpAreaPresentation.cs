using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Logic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Effects.Deferred;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Core;
using InCharge.Procedural.Terrain;
using Microsoft.Xna.Framework;

namespace InCharge.Rendering.Model
{
    /// <summary>
    /// Represents a dump area
    /// </summary>
    public class DumpAreaPresentation : IPresentation
    {
        /// <summary>
        /// Dumping area reference
        /// </summary>
        private DumpArea area;
        /// <summary>
        /// Graphics device reference
        /// </summary>
        private GraphicsDevice device;
        /// <summary>
        /// Content manager reference
        /// </summary>
        private ContentManager content;
        /// <summary>
        /// Geometry texture
        /// </summary>
        private Texture2D texture;

        private SceneObject sceneObject;
        /// <summary>
        /// Scene object representation
        /// </summary>
        public SceneObject SceneObject
        {
            get { return sceneObject; }
        }
        /// <summary>
        /// Geometry effect
        /// </summary>
        private Effect effect;

        public DumpAreaPresentation(DumpArea area, ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.area = area;
            this.device = graphicsDevice;
            this.content = content;

            this.texture = content.Load<Texture2D>("Textures/Plan/dump_area");

            var eff = new DeferredObjectEffect(this.device);
            eff.DiffuseMapTexture = this.texture;
            eff.SpecularPower = 0;
            eff.TransparencyMode = TransparencyMode.Clip;
            eff.NormalMapTexture = null;
            eff.Skinned = false;
            eff.AddressModeU = TextureAddressMode.Clamp;
            eff.AddressModeV = TextureAddressMode.Clamp;
            eff.AddressModeW = TextureAddressMode.Clamp;
            this.effect = eff;
        }

        #region IPresentation Members

        public SceneObject BuildSceneObject()
        {
            List<VertexPositionNormalTextureBump> vertices = new List<VertexPositionNormalTextureBump>();
            List<int> indices = new List<int>();

            foreach (TerrainBlock.IntVector3 location in this.area.DumpLocations)
            {
                /*// create north face
                if (block.GetNeighbor(WorldOrientation.North) == null)
                {
                    SelectionAreaVertexCreator.CreateVertices(vertices, indices, block, WorldOrientation.North);
                }
                // create east face
                if (block.GetNeighbor(WorldOrientation.East) == null)
                {
                    SelectionAreaVertexCreator.CreateVertices(vertices, indices, block, WorldOrientation.East);
                }
                // create south face
                if (block.GetNeighbor(WorldOrientation.South) == null)
                {
                    SelectionAreaVertexCreator.CreateVertices(vertices, indices, block, WorldOrientation.South);
                }
                // create west face
                if (block.GetNeighbor(WorldOrientation.West) == null)
                {
                    SelectionAreaVertexCreator.CreateVertices(vertices, indices, block, WorldOrientation.West);
                }
                // create top face
                if (block.GetNeighbor(WorldOrientation.Up) == null)
                {
                    SelectionAreaVertexCreator.CreateVertices(vertices, indices, block, WorldOrientation.Up);
                }*/
            }

            var indicesArray = indices.ToArray();
            var verticesArray = vertices.ToArray();

            // calc more stuff
            VertexPositionNormalTextureBump.BuildTangentSpaceDataForTriangleList(indicesArray, verticesArray);

            MeshData md = new MeshData();
            md.VertexBuffer = new VertexBuffer(this.device, VertexPositionNormalTextureBump.VertexDeclaration, verticesArray.Length, BufferUsage.None);
            md.VertexBuffer.SetData(verticesArray);
            md.IndexBuffer = new IndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, indicesArray.Length, BufferUsage.None);
            md.IndexBuffer.SetData(indicesArray);
            md.MeshToObject = Matrix.Identity;
            md.Effect = this.effect;
            md.PrimitiveCount = verticesArray.Length >> 1;
            md.VertexCount = verticesArray.Length;
            md.InfiniteBounds = true; // always show selection            
            // ...
            var so = new SceneObject(md, "DumpingSelection");
            so.StaticLightingType = SynapseGaming.LightingSystem.Lights.StaticLightingType.Custom;
            so.CustomStaticLightingColor = Color.White.ToVector3();
            so.HullType = HullType.Box;
            so.AffectedByGravity = false;
            so.CollisionType = SynapseGaming.LightingSystem.Collision.CollisionType.None;

            this.area.HasChanged = false;
            return so;
        }

        #endregion

        #region IPresentation Members

        public SceneObject PresentationSceneObject
        {
            get 
            {
                // return cached object, otherwise (re-)build it
                if (this.area.HasChanged || this.sceneObject == null)
                {
                    this.sceneObject = this.BuildSceneObject();                    
                }
                return this.sceneObject;
            }
        }

        #endregion
    }
}
