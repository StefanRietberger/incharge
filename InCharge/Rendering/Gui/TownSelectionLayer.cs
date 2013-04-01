using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Rendering;
using Indiefreaks.Xna.Core;
using Microsoft.Xna.Framework;
using SynapseGaming.LightingSystem.Lights;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Effects;
using SynapseGaming.LightingSystem.Core;
using Indiefreaks.Xna.Rendering.Camera;
using InCharge.Procedural.Terrain;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Effects.Deferred;
using InCharge.Util;

namespace InCharge.Rendering.Gui
{
    /// <summary>
    /// Drawing layer for the town and contents selections
    /// </summary>
    public class TownSelectionLayer : Layer
    {
        private ICamera camera;
        private DeferredObjectEffect selectionTileEffect;
        private Texture2D texTileSelect;
        private TerrainMap.TerrainPickResult? currentPickResult;
        private PointLight selectionLight;
        private SceneObject selectionSurface;
       
        public TownSelectionLayer(GameState gameState, ICamera camera)
            : base(gameState)
        {
            this.camera = camera;
        }

        public override void Draw(GameTime gameTime)
        {
            
        }

        public void UpdatePickResult(TerrainMap.TerrainPickResult? pickResult)
        {
            this.currentPickResult = pickResult;
            if (pickResult.HasValue)
            {                
                Vector3 lightPos = pickResult.Value.BlockPosition.WorldPosition;
                lightPos.X += TerrainBlock.BLOCK_DIAMETER * 0.5f;
                lightPos.Z += TerrainBlock.BLOCK_DIAMETER * 0.5f;
                lightPos.Y += TerrainBlock.BLOCK_HEIGHT * 2;
                this.selectionLight.Position = lightPos;
                var surfaceTranslation = pickResult.Value.BlockPosition.WorldPosition;
                this.selectionSurface.World = Matrix.CreateTranslation(surfaceTranslation);
            }
        }

        public override void Initialize()
        {
            base.Initialize();           

            this.selectionLight = new PointLight();
            this.selectionLight.Enabled = false;
            this.selectionLight.FillLight = false;
            this.selectionLight.Intensity = 1f;
            this.selectionLight.LightingType = LightingType.RealTime;
            this.selectionLight.Radius = 5f;
            this.selectionLight.DiffuseColor = Color.CornflowerBlue.ToVector3();
            this.GameState.SunBurn.LightManager.Submit(this.selectionLight);

            this.texTileSelect = this.GameState.Application.Content.Load<Texture2D>("Textures/Plan/selection_frame");

            this.selectionTileEffect = new DeferredObjectEffect(this.GameState.Application.GraphicsDevice);
            this.selectionTileEffect.TransparencyMode = TransparencyMode.Clip;
            this.selectionTileEffect.EmissiveColor = Color.White.ToVector3();
            this.selectionTileEffect.World = Matrix.Identity;
            this.selectionTileEffect.DiffuseMapTexture = this.texTileSelect;

            this.selectionSurface = this.GetTileSelectionSurface(this.selectionTileEffect);
            this.selectionSurface.Visibility = ObjectVisibility.None;
            this.GameState.SunBurn.ObjectManager.Submit(this.selectionSurface);
        }

        public override bool IsVisible
        {
            get
            {
                return base.IsVisible;
            }
            set
            {
                this.OnSetIsVisible(value);
                base.IsVisible = value;                
            }
        }

        private void OnSetIsVisible(bool visible)
        {
            if (this.IsVisible != visible) // only react to actual state change
            {
                if (selectionLight != null)
                {
                    this.selectionLight.Enabled = visible;
                }
                if (this.selectionSurface != null)
                {
                    this.selectionSurface.Visibility = (visible ? ObjectVisibility.Rendered : ObjectVisibility.None);
                }
            }
        }

        /// <summary>
        /// Get the top surface of a given terrain tile
        /// </summary>
        /// <param name="pickingRay"></param>
        /// <returns></returns>
        public SceneObject GetTileSelectionSurface(Effect eff)
        {            
            var box = GeometryGenerator.CreateBox(new Vector3(-0.15f, 0.15f, -0.15f), new Vector3(1.3f, 0.8f, 1.3f));

            VertexPositionNormalTextureBump.BuildTangentSpaceDataForTriangleList(box.Indices, box.Vertices);

            MeshData md = new MeshData();
            md.VertexBuffer = new VertexBuffer(this.GameState.Application.GraphicsDevice, VertexPositionNormalTextureBump.VertexDeclaration, box.Vertices.Length, BufferUsage.None);
            md.VertexBuffer.SetData(box.Vertices);
            md.IndexBuffer = new IndexBuffer(this.GameState.Application.GraphicsDevice, IndexElementSize.ThirtyTwoBits, box.Indices.Length, BufferUsage.None);
            md.IndexBuffer.SetData(box.Indices);
            md.MeshToObject = Matrix.Identity;
            md.Effect = eff;
            md.PrimitiveCount = box.Vertices.Length / 2;
            md.VertexCount = box.Vertices.Length;
            md.ObjectSpaceBoundingBox = new BoundingBox(
                box.Vertices[0].Position,
                box.Vertices[23].Position);

            var so = new SceneObject(md);
            so.Visibility = ObjectVisibility.Rendered;
            so.CustomStaticLightingColor = Color.White.ToVector3();
            so.StaticLightingType = StaticLightingType.Custom;
            so.World = Matrix.Identity;
            so.HullType = HullType.Box;
            so.AffectedByGravity = false;
            so.CollisionType = CollisionType.None;

            return so;
        }
    }
}
