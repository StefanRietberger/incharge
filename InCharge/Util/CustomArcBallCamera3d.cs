using System;
using Indiefreaks.Xna.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SynapseGaming.LightingSystem.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Indiefreaks.Xna.Core;

namespace Indiefreaks.Xna.Rendering.Camera
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomArcBallCamera3d : CustomCamera3D
    {
        private const float VerticalRotationMin = 0.01f;
        private const float VerticalRotationMax = MathHelper.PiOver2 * 0.5f;

        private readonly float _maxRadius;
        private readonly float _minRadius;

        public float MaxRadius { get { return _maxRadius; } }
        public float MinRadius { get { return _minRadius; } }

        // Should we force these initial values to be supplied in the constructor ??
        private float _horizontalRotation;
        private float _radius = 30f;
        private float _verticalRotation = MathHelper.PiOver4;

        private const float baseFovAngle = MathHelper.PiOver4; // 90°
        private const float pseudoOrthoAngle = MathHelper.Pi * 0.03f;
        private float baseMoveScale = 0.5f;
        private float frameMoveScale;
        private int lastWheelValue = 0;

        private int[] mousePosBeforeRotate = new int[2];
        private bool isRotating = false;
        private Matrix orthoMatrix;

        private int width;
        private int height;
        private RenderTarget2D cameraRenderTarget;

        /// <summary>
        /// Creates a new instance of the camera
        /// </summary>
        /// <param name = "aspectRatio"></param>
        /// <param name = "fieldOfView"></param>
        /// <param name = "nearPlaneDistance"></param>
        /// <param name = "farPlaneDistance"></param>
        /// <param name="minRadius"></param>
        /// <param name="maxRadius"></param>
        public CustomArcBallCamera3d(float aspectRatio, float fieldOfView, float nearPlaneDistance, float farPlaneDistance, float minRadius, float maxRadius, int width, int height, RenderTarget2D renderTarget)
            : base(aspectRatio, fieldOfView, nearPlaneDistance, farPlaneDistance)
        {
            this.cameraRenderTarget = renderTarget;
            this.width = width;
            this.height = height;
            TargetPosition = Vector3.Forward;
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            this.orthoMatrix = Matrix.CreateOrthographic(width >> 1, height >> 1, nearPlaneDistance, farPlaneDistance);
        }

        public ISceneEntity TargetEntity { get; set; }

        /// <summary>
        /// Distance of camera from target
        /// </summary>
        public float Radius
        {
            get { return _radius; }
            set { _radius = MathHelper.Clamp(value, _minRadius, _maxRadius); }
        }

        /// <summary>
        /// Rotation of camera around vertical axis (Y)
        /// </summary>
        public float HorizontalRotation
        {
            get { return _horizontalRotation; }
            set { _horizontalRotation = value % MathHelper.TwoPi; }
        }

        /// <summary>
        /// Rotation of camera around horizontal axis (X)
        /// </summary>
        public float VerticalRotation
        {
            get { return _verticalRotation; }
            set { _verticalRotation = MathHelper.Clamp(value, VerticalRotationMin, VerticalRotationMax); }
        }

        /// <summary>
        /// Override this method to catch input events and act on the camera
        /// </summary>
        /// <param name = "input">The current input instance</param>
        protected override void UpdateInput(InputManager input)
        {
            bool hasMoved = false;
            MouseState mouse = Mouse.GetState();

            // strafe left
            if (input.KeyboardState.GetKey(Keys.A).IsDown || input.KeyboardState.GetKey(Keys.Left).IsDown)
            {
                hasMoved = true;
                Vector3 translate = Position - TargetPosition;
                translate.Normalize();
                float x = translate.X;
                float z = translate.Z;
                translate.Y = 0;
                translate.X = -z;
                translate.Z = x;
                this.TargetPosition += translate;
            }
            // strafe right
            if (input.KeyboardState.GetKey(Keys.D).IsDown || input.KeyboardState.GetKey(Keys.Right).IsDown)
            {
                hasMoved = true;
                Vector3 translate = Position - TargetPosition;
                translate.Normalize();
                float x = translate.X;
                float z = translate.Z;
                translate.Y = 0;
                translate.X = z;
                translate.Z = -x;
                this.TargetPosition += translate;
            }
            // strafe up
            if (input.KeyboardState.GetKey(Keys.W).IsDown || input.KeyboardState.GetKey(Keys.Up).IsDown)
            {
                hasMoved = true;
                Vector3 translate = Position - TargetPosition;
                translate.Normalize();
                translate.Y = 0;
                this.TargetPosition -= translate;
            }
            // strafe down
            if (input.KeyboardState.GetKey(Keys.S).IsDown || input.KeyboardState.GetKey(Keys.Down).IsDown)
            {
                hasMoved = true;
                Vector3 translate = Position - TargetPosition;
                translate.Normalize();
                translate.Y = 0;
                this.TargetPosition += translate;
            }

            // rotate on middle button
            if (mouse.MiddleButton == ButtonState.Pressed)
            {
                hasMoved = true;
                if (!isRotating)
                {
                    mousePosBeforeRotate[0] = mouse.X;
                    mousePosBeforeRotate[1] = mouse.Y;
                }
                else
                {
                    var deltaX = mousePosBeforeRotate[0] - mouse.X;
                    var deltaY = mousePosBeforeRotate[1] - mouse.Y;

                    this.HorizontalRotation += deltaX * frameMoveScale;
                    this.VerticalRotation += deltaY * frameMoveScale;

                    Mouse.SetPosition(mousePosBeforeRotate[0], mousePosBeforeRotate[1]);
                }
                isRotating = true;
            }
            else if (mouse.MiddleButton == ButtonState.Released)
            {
                isRotating = false;
            }

            // check mouse wheel for zoom
            int wheelDelta = this.lastWheelValue - mouse.ScrollWheelValue;
            if (Math.Abs(wheelDelta) > 0.01)
            {
                this.Radius += wheelDelta * 0.02f;
            }
            this.lastWheelValue = mouse.ScrollWheelValue;

            /*// adjust perspective distortion based on camera movement
            if (hasMoved)
            {
                var newViewAngle = this.FieldOfView + 0.05f;
                this.FieldOfView = MathHelper.Clamp(newViewAngle, pseudoOrthoAngle, baseFovAngle);
            }
            else
            {
                var newViewAngle = this.FieldOfView - 0.05f;
                if (newViewAngle < pseudoOrthoAngle)
                {
                    this.FieldOfView = pseudoOrthoAngle;
                }
                else
                {
                    this.FieldOfView = newViewAngle;
                }
            }*/
        }

        protected override Matrix UpdateViewMatrix(GameTime gameTime)
        {
            float timescale = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float rotatescale = 3.0f * timescale;
            this.frameMoveScale = timescale * this.baseMoveScale;

            // update camera position
            Vector3 cameraPosition = Vector3.Multiply(Vector3.Up, Radius);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationX(this.VerticalRotation));
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(this.HorizontalRotation));
            Position = TargetPosition + cameraPosition;
            
            // update view matrix
            float perspFactor = baseFovAngle / this.FieldOfView;
            float currDist = this.Radius * perspFactor;
            Vector3 targetToView = Position - TargetPosition;
            targetToView.Normalize();
            Vector3 perspViewPosition = TargetPosition + (targetToView * currDist);
            return Matrix.CreateLookAt(perspViewPosition, TargetPosition, Vector3.Up);
                    
        }

        protected override Matrix UpdateProjectionMatrix()
        {
            //if (!this.isOrtho)
                return base.UpdateProjectionMatrix();
            //else
            //    return this.orthoMatrix;
        }

        public override void BeginFrameRendering(GameTime gameTime, SynapseGaming.LightingSystem.Core.FrameBuffers frameBuffers)
        {
            Application.Graphics.GraphicsDevice.SetRenderTarget(this.cameraRenderTarget);
            Application.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Application.Graphics.GraphicsDevice.Clear(Color.White);
            base.BeginFrameRendering(gameTime, frameBuffers);
        }
    }
}