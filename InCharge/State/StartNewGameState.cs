using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indiefreaks.Xna.Core;
using Microsoft.Xna.Framework.Graphics;
using Indiefreaks.Xna.Rendering.Camera;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Rendering;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using InCharge.Procedural.Terrain;

namespace InCharge.State
{
    public class StartNewGameState : GameState
    {
        /// <summary>
        /// Sprite batch for simple texture drawing
        /// </summary>
        SpriteBatch spriteBatch;

        // Effects        
        Effect celShaderEffect;
        Effect outlineEffect;
        /// <summary>
        /// Render target for filter pass 1
        /// </summary>
        RenderTarget2D rtPass1;
        /// <summary>
        /// Rendertarget for filter pass 2
        /// </summary>
        RenderTarget2D rtPass2;
        /// <summary>
        /// Rendertarget for filter pass 3
        /// </summary>
        RenderTarget2D rtPass3;
        /// <summary>
        /// Camera reference
        /// </summary>
        CustomArcBallCamera3d camera;

        SynapseGaming.LightingSystem.Lights.DirectionalLight sunLight;

        private const float outlineThresholdFactor = 3;
        private float nearPlaneDistance = 1f;
        private const float farPlaneDistance = 20000f;
        //private float outlineThreshold = 0.00025f; // at farplane 20000
        private float outlineThreshold = outlineThresholdFactor / farPlaneDistance;

        public StartNewGameState(Application application)
            : base("NewGameState", application)
        {
            Application.ClearDeviceColor = Color.CornflowerBlue;
        }

        public override void Initialize()
        {   
            // In order to render SunBurn SceneObject instances, we need to add a SunBurn Layer (this isn't required if you're not using SunBurn rendering like in the
            // main menu or introduction)
            this.AddLayer(new SunBurnLayer(this));            

            var prefs = new SystemPreferences();
            prefs.TextureSampling = SamplingPreference.Point;
            SunBurn.ApplyPreferences(prefs);

            SunBurn.ObjectManager.AutoOptimize = true;

            // we add a basic lighting setup so that we can see something in the scene
            SunBurn.LightManager.SubmitStaticAmbientLight(Color.White.ToVector3(), 0.6f);
            sunLight = new SynapseGaming.LightingSystem.Lights.DirectionalLight();
            sunLight.DiffuseColor = new Vector3(1, 1f, 1f);
            sunLight.Direction = new Vector3(0f, 10f, 0f);
            sunLight.Intensity = 1.5f;
            sunLight.LightingType = LightingType.RealTime;
            SunBurn.LightManager.Submit(sunLight);

            this.rtPass1 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width >> 1, this.Application.GraphicsDevice.Viewport.Height >> 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            this.rtPass2 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width >> 1, this.Application.GraphicsDevice.Viewport.Height >> 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            this.rtPass3 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // SunBurn.GetManager<CameraManager>(true).Submit(new FreeCamera3D(Application.GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.1f, 1000f));
            // create a camera which observes a given object or target position
            this.camera = new CustomArcBallCamera3d(this.Application.GraphicsDevice.Viewport.AspectRatio, MathHelper.PiOver4, nearPlaneDistance, farPlaneDistance, 10, 120, 
                this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height, this.rtPass1)
            {
                Position = new Vector3(0f, 80f, 0f),                
                TargetPosition = new Vector3(0, 64, 32),
                
                SceneEnvironment = // each camera in IGF contains a SceneEnvironment instance that will be used to control the SunBurn rendering
                {
                    Gravity = 9.81f,
                    FogEnabled = false,
                    ShadowFadeStartDistance = 2048,
                    ShadowFadeEndDistance = 2048,
                    VisibleDistance = float.MaxValue
                }                
            };
            SunBurn.GetManager<ICameraManager>(true).Submit(this.camera);

            this.spriteBatch = new SpriteBatch(this.Application.GraphicsDevice);                        

            this.celShaderEffect = this.Application.Content.Load<Effect>("Effects/CelShader");
            this.celShaderEffect.Parameters["CelMap"].SetValue(this.Application.Content.Load<Texture2D>("Effects/Toon"));
            this.outlineEffect = this.Application.Content.Load<Effect>("Effects/OutlineShader");
            this.outlineEffect.Parameters["Thickness"].SetValue(0.5f);
            this.outlineEffect.Parameters["Threshold"].SetValue(this.outlineThreshold);
            this.outlineEffect.Parameters["ScreenSize"].SetValue(
                new Vector2(this.FrameBuffers.Width >> 1, this.FrameBuffers.Height >> 1));       
     
            // create world map
            WorldMap wm = new WorldMap(1);
        }
    }
}
