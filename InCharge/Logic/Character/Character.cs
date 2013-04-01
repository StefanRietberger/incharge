using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Effects.Deferred;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using InCharge.Procedural;
using SynapseGaming.LightingSystem.Core;
using Indiefreaks.Xna.Logic;
using InCharge.Util;
using InCharge.Procedural.Terrain;

namespace InCharge.Logic.Character
{
    public class Character : SceneObject
    {
        private static DeferredObjectEffect effect;

        private ContentManager content;
        private GraphicsDevice device;

        private Vector3 position;
        /// <summary>
        /// Character position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                this.World = Matrix.CreateTranslation(value);
            }
        }

        /// <summary>
        /// Character's facing direction
        /// </summary>
        public float Direction { get; set; }
        /// <summary>
        /// Character health, falling to 0 means death.
        /// </summary>
        public float Health { get; set; }
        /// <summary>
        /// Jump height capabilities of character as number of blocks
        /// </summary>
        public int JumpHeight { get; set; }

        /// <summary>
        /// Minimum height needed as number of blocks to fit 
        /// </summary>
        public int BoundHeight { get; set; }

        /// <summary>
        /// Movement status
        /// </summary>
        public bool IsMoving { get; set; }
        private Vector3 moveOrigin;
        private Vector3 moveTarget;
        private float moveTime;
        private MovementType moveType;
        /// <summary>
        /// Maximum walking speed in blocks per second
        /// </summary>
        private float maxWalkSpeed = 1f;
        /// <summary>
        /// Maximum running speed in blocks per second
        /// </summary>
        private float maxRunSpeed = 4f;

        public Character(string name, ContentManager content, GraphicsDevice device)
            : base(name, false)
        {
            this.content = content;
            this.device = device;

            this.JumpHeight = 2;

            this.HullType = SynapseGaming.LightingSystem.Rendering.HullType.Box;
            this.AffectedByGravity = false; // set to true dynamically when necessary
            this.Mass = 60f;
            this.CollisionType = SynapseGaming.LightingSystem.Collision.CollisionType.Collide;
            //this.StaticLightingType = SynapseGaming.LightingSystem.Lights.StaticLightingType.Composite;
            this.Visibility = ObjectVisibility.RenderedAndCastShadows | ObjectVisibility.RenderedInEditor;
            this.UpdateType = UpdateType.Automatic;
            this.World = Matrix.Identity;

            this.LoadModel();

            this.CalculateBounds();
        }

        /// <summary>
        /// Loads and assigns the mesh/model for a character
        /// </summary>
        private void LoadModel()
        {
            if (Character.effect == null)
            {
                var eff = new DeferredObjectEffect(this.device);
                eff.DiffuseMapTexture = this.content.Load<Texture2D>("Textures/Character/placeholder");
                eff.SpecularPower = 0;
                eff.TransparencyMode = TransparencyMode.Clip;
                eff.NormalMapTexture = null;
                eff.Skinned = false;
                eff.AddressModeU = TextureAddressMode.Clamp;
                eff.AddressModeV = TextureAddressMode.Clamp;
                eff.AddressModeW = TextureAddressMode.Clamp;
                Character.effect = eff;
            }

            var data = PrimitivesHelper.CreateBox(0.5f, 2f, 0.5f);

            var mesh = new RenderableMesh();

            var indexBuffer = new IndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, data.Indices.Length, BufferUsage.None);
            indexBuffer.SetData(data.Indices);

            var vertexBuffer = new VertexBuffer(this.device, VertexPositionNormalTextureBump.VertexDeclaration, data.Vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(data.Vertices);

            mesh.Build(this,
                Character.effect,
                Matrix.Identity,
                new BoundingSphere(), // empty
                //new BoundingBox(new Vector3(0, 0, 0), new Vector3(0.5f, 1.8f, 0.5f)),
                new BoundingBox(new Vector3(0.5f, 2.0f, 0.5f), new Vector3(0, 0, 0)),
                indexBuffer,
                vertexBuffer,
                0,
                PrimitiveType.TriangleList,
                data.Vertices.Length / 2,
                0,
                data.Vertices.Length,
                0,
                false);

            this.Add(mesh);
        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);

            if (this.IsMoving)
            {
                var movePath = this.moveTarget - this.moveOrigin;
                this.moveTime += (float)(gametime.ElapsedGameTime.TotalSeconds);
                // set the property to update world translation
                var partialMovement = movePath * this.moveTime * this.maxRunSpeed;
                this.Position = this.moveOrigin + partialMovement;

                // check if target was reached
                if (partialMovement.LengthSquared() >= movePath.LengthSquared())
                {
                    this.IsMoving = false;
                    // set position to exact target
                    this.Position = this.moveTarget;
                }
                Indiefreaks.Xna.Core.Application.SunBurn.ObjectManager.Move(this);
            }
        }

        public void StepTo(Vector3 blockDestination, float totalPathLength, float remainingPathLength)
        {
            this.moveOrigin = this.position;
            this.moveTarget = this.GetBlockCenteredPosition(blockDestination);

            var moveHeight = this.moveTarget.Y - this.moveOrigin.Y;

            if (moveHeight > 0.01f)
            {
                this.moveType = MovementType.JumpUp;
            }
            else if (moveHeight < -0.01f)
            {
                this.moveType = MovementType.JumpDown;
            }
            else
            {
                this.moveType = MovementType.Go;
            }

            this.IsMoving = true;
            this.moveTime = 0;
        }

        /// <summary>
        /// Returns a position relative to the given block position's center
        /// </summary>
        /// <param name="blockPosition"></param>
        /// <returns></returns>
        public Vector3 GetBlockCenteredPosition(Vector3 blockPosition)
        {
            return blockPosition + new Vector3(TerrainBlock.BLOCK_DIAMETER / 2 - 0.25f, 0, TerrainBlock.BLOCK_DIAMETER - 0.25f);
        }

        public override void OnCollisionReact(IMovableObject collider, IMovableObject collidee, SynapseGaming.LightingSystem.Collision.CollisionPoint worldcollisionpoint, ref bool collisionhandled)
        {
            base.OnCollisionReact(collider, collidee, worldcollisionpoint, ref collisionhandled);
        }

        public override void OnCollisionTrigger(IMovableObject collider, IMovableObject trigger)
        {
            base.OnCollisionTrigger(collider, trigger);
        }
    }
}
