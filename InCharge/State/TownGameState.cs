using System;
using InCharge.Input;
using InCharge.Logic;
using InCharge.Procedural.Terrain;
using InCharge.Rendering;
using InCharge.UI;
using Indiefreaks.Xna.Core;
using Indiefreaks.Xna.Input;
using Indiefreaks.Xna.Rendering;
using Indiefreaks.Xna.Rendering.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using InCharge.Persistence;
using SynapseGaming.LightingSystem.Shadows.Deferred;
using InCharge.Rendering.Gui;
using InCharge.Rendering.Model;
using InCharge.Logic.Character;
using SynapseGaming.LightingSystem.Rendering;
using Indiefreaks.Xna.Physics;
using Indiefreaks.Xna.Logic;
using Indiefreaks.Xna.Logic.Steering;
using Indiefreaks.Xna.Sessions;
using System.Collections.Generic;

namespace InCharge.State
{
    class TownGameState : GameState, IGameController
    {
        private PlayerAgent singlePlayerAgent;

        private ISaveStateManager saveStateManager;

        /// <summary>
        /// Sprite batch for simple texture drawing
        /// </summary>
        private SpriteBatch spriteBatch;

        // Effects        
        private Effect celShaderEffect;
        private Effect outlineEffect;
        private Effect scaleEffect;
        /// <summary>
        /// Render target for filter pass 1
        /// </summary>
        private RenderTarget2D rtPass1;
        /// <summary>
        /// Rendertarget for filter pass 2
        /// </summary>
        private RenderTarget2D rtPass2;
        /// <summary>
        /// Rendertarget for filter pass 3
        /// </summary>
        private RenderTarget2D rtPass3;
        /// <summary>
        /// Camera reference
        /// </summary>
        private CustomArcBallCamera3d camera;
        /// <summary>
        /// Scene graph holding all town stuff
        /// </summary>
        private IScene townScene;
        private ISceneEntityGroup terrainSceneGroup;
        private ISceneEntityGroup characterSceneGroup;

        /// <summary>
        /// Sun light source
        /// </summary>
        SynapseGaming.LightingSystem.Lights.DirectionalLight sunLight;
        private float sunPhase = MathHelper.Pi + MathHelper.PiOver2 + MathHelper.PiOver4 / 2;

        /// <summary>
        /// Terrain reference
        /// </summary>
        private TerrainMap terrain;

        private TownSelectionLayer selectionLayer;
        private MiniMapLayer miniMapLayer;

        #region Input state variables
        private const float outlineThresholdFactor = 3;
        private float nearPlaneDistance = 1f;
        private const float farPlaneDistance = 20000f;
        //private float outlineThreshold = 0.00025f; // at farplane 20000
        private float outlineThreshold = outlineThresholdFactor / farPlaneDistance;
        /// <summary>
        /// Render console flag
        /// </summary>
        private bool showConsole = false;
        /// <summary>
        /// Input state machine
        /// </summary>
        private InputStateMachine inputStateMachine = new InputStateMachine();
        /// <summary>
        /// Currently picked terrain block
        /// </summary>
        private TerrainMap.TerrainPickResult? terrainPickResult;
        /// <summary>
        /// Mining area
        /// </summary>
        private DigArea digArea;
        private DigAreaPresentation digAreaPresentation;

        private TerrainBlock currentDumpingBlock;

        private DumpArea dumpArea;
        private DumpAreaPresentation dumpAreaPresentation;
        /// <summary>
        /// Layer for all GUI related drawing and input handling
        /// </summary>
        private TownGuiLayer guiLayer;
        #endregion Input state variables

        public TownGameState(Application application)
            : base("TownState", application)
        {
            this.saveStateManager = SunBurn.GetManager<ISaveStateManager>(true);

            Application.ClearDeviceColor = Color.CornflowerBlue;
        }

        public override void Initialize()
        {
            this.singlePlayerAgent = SessionManager.CurrentSession.CreatePlayerAgent(SessionManager.LocalPlayers[PlayerIndex.One]);

            // In order to render SunBurn SceneObject instances, we need to add a SunBurn Layer (this isn't required if you're not using SunBurn rendering like in the
            // main menu or introduction)
            this.AddLayer(new SunBurnLayer(this));

            var prefs = new SystemPreferences();
            prefs.TextureSampling = SamplingPreference.Point;
            SunBurn.ApplyPreferences(prefs);

            SunBurn.ObjectManager.AutoOptimize = true;

            // we add a basic lighting setup so that we can see something in the scene
            SunBurn.LightManager.SubmitStaticAmbientLight(Color.White.ToVector3(), 0.1f);
            sunLight = new SynapseGaming.LightingSystem.Lights.DirectionalLight();
            sunLight.DiffuseColor = new Vector3(1f, 1f, 0.9f);
            sunLight.Direction = new Vector3(0f, 10f, 0f);
            sunLight.Intensity = 0.8f;
            sunLight.LightingType = LightingType.RealTime;
            SunBurn.LightManager.Submit(sunLight);

            this.rtPass1 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width >> 1, this.Application.GraphicsDevice.Viewport.Height >> 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            this.rtPass2 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            this.rtPass3 = new RenderTarget2D(this.Application.GraphicsDevice, this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // SunBurn.GetManager<CameraManager>(true).Submit(new FreeCamera3D(Application.GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.1f, 1000f));
            // create a camera which observes a given object or target position
            this.camera = new CustomArcBallCamera3d(this.Application.GraphicsDevice.Viewport.AspectRatio, MathHelper.PiOver4, nearPlaneDistance, farPlaneDistance, 5, 120,
                this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height, this.rtPass1)
            {
                Position = new Vector3(0f, 80f, 0f),
                TargetPosition = new Vector3(0, 48, 32),

                SceneEnvironment = // each camera in IGF contains a SceneEnvironment instance that will be used to control the SunBurn rendering
                {
                    Gravity = 9.81f,
                    FogEnabled = false,
                    ShadowFadeStartDistance = 64,
                    ShadowFadeEndDistance = 128,
                    VisibleDistance = 256f
                }
            };
            SunBurn.GetManager<ICameraManager>(true).Submit(this.camera);

            this.spriteBatch = new SpriteBatch(this.Application.GraphicsDevice);

            // we create our asteroid factory and ask the GameState to load its content
            this.terrain = new TerrainMap(this.Application.Content, this.Application.GraphicsDevice, new Random().Next(int.MaxValue));

            PreLoad(this.terrain);

            // on loading completion, we add the terrain to the ObjectManager
            this.townScene = new Scene()
            {
                Name = "Town",
                AffectedInCode = true
            };
            this.terrainSceneGroup = new SceneEntityGroup()
            {
                Name = "Terrain",
                AffectedInCode = true
            };
            this.characterSceneGroup = new SceneEntityGroup()
            {
                Name = "Characters",
                AffectedInCode = true
            };
            this.townScene.EntityGroups.Add(this.terrainSceneGroup);
            this.townScene.EntityGroups.Add(this.characterSceneGroup);
            SunBurn.ObjectManager.Submit(this.townScene);

            LoadingCompleted += delegate
            {
                foreach (var so in terrain.SurfaceSceneObjects)
                {
                    this.terrainSceneGroup.Add(so);
                }
                this.townScene.Apply();
            };

            this.celShaderEffect = this.Application.Content.Load<Effect>("Effects/CelShader");
            this.celShaderEffect.Parameters["CelMap"].SetValue(this.Application.Content.Load<Texture2D>("Effects/Toon"));
            this.outlineEffect = this.Application.Content.Load<Effect>("Effects/OutlineShader");
            this.outlineEffect.Parameters["Thickness"].SetValue(0.5f);
            this.outlineEffect.Parameters["Threshold"].SetValue(this.outlineThreshold);
            this.outlineEffect.Parameters["ScreenSize"].SetValue(
                new Vector2(this.FrameBuffers.Width >> 1, this.FrameBuffers.Height >> 1));
            this.scaleEffect = this.Application.Content.Load<Effect>("Effects/hq2x");
            this.scaleEffect.Parameters["Viewport"].SetValue(new Vector2(this.Application.GraphicsDevice.Viewport.Width, this.Application.GraphicsDevice.Viewport.Height));

            // create and add layers
            this.selectionLayer = new TownSelectionLayer(this, this.camera);
            this.selectionLayer.IsVisible = false;
            this.AddLayer(this.selectionLayer);

            this.guiLayer = new TownGuiLayer(this, this);
            this.guiLayer.IsVisible = false; // suppress automatic drawing
            this.AddLayer(this.guiLayer);

            this.miniMapLayer = new MiniMapLayer(this, this.camera);
            this.miniMapLayer.IsVisible = false; // suppress automatic drawing
            this.AddLayer(this.miniMapLayer);

            this.digArea = new DigArea();
            this.digAreaPresentation = new DigAreaPresentation(digArea, this.Application.Content, this.Application.GraphicsDevice);

            this.dumpArea = new DumpArea(this.digArea);
            this.dumpAreaPresentation = new DumpAreaPresentation(dumpArea, this.Application.Content, this.Application.GraphicsDevice);

            this.SetUpControls();
        }



        /// <summary>
        /// Calculate the mouse picking ray
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        private Ray GetPickingRay()
        {
            var ms = Application.Input.MouseState;

            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            Viewport vp = this.Application.GraphicsDevice.Viewport;
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(ms.X, ms.Y, 0), this.camera.SceneState.Projection, this.camera.SceneState.View, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(ms.X, ms.Y, 1), this.camera.SceneState.Projection, this.camera.SceneState.View, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);
            Ray pickingRay = new Ray(pos1, dir);
            return pickingRay;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SunBurn.ObjectManager.Update(gameTime);

            this.inputStateMachine.ProcessInput(gameTime);
            this.ProcessInput(gameTime);

            // check picks
            Ray pickRay = this.GetPickingRay();
            this.terrainPickResult = this.terrain.GetPickedTerrainBlock(pickRay);

            switch (this.inputStateMachine.CurrentState)
            {
                case InputState.Dump:
                case InputState.Dig:
                case InputState.Terraform:
                    this.selectionLayer.UpdatePickResult(terrainPickResult);

                    break;
            }

            this.AdjustOutlineThreshold();

            //this.sunPhase += MathHelper.Pi * 0.01f * gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            this.sunPhase %= MathHelper.Pi * 2;
            this.sunLight.Direction = new Vector3((float)Math.Cos(this.sunPhase), (float)Math.Sin(this.sunPhase), -0.6f);            
        }

        private void AdjustOutlineThreshold()
        {
            var depthRange = farPlaneDistance - nearPlaneDistance;
            var one = TerrainBlock.BLOCK_DIAMETER / depthRange;
            var depth = one * 5;
            var zoomFactor = 1 + (this.camera.Radius - this.camera.MinRadius) / (this.camera.MaxRadius - this.camera.MinRadius);

            this.outlineThreshold = zoomFactor * depth;

            this.outlineEffect.Parameters["Threshold"].SetValue(this.outlineThreshold);
        }

        private void ProcessInput(GameTime gameTime)
        {
            if (Application.Input.KeyboardState.KeyState.Escape.IsReleased)
            {
                Application.Exit();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // at this point, all the layers have been drawn onto the render target given to the camera! (rtPass1)
            // apply filters               

            // cel shading
            /*this.Application.GraphicsDevice.SetRenderTarget(this.rtPass2);            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.celShaderEffect);                    
            spriteBatch.Draw(this.rtPass1, Vector2.Zero, Color.White);
            spriteBatch.End();*/

            // outline shader
            this.Application.GraphicsDevice.SetRenderTarget(this.rtPass2);
            // assign current depth map to outline filter
            this.outlineEffect.Parameters["depthMap"].SetValue(this.camera.SceneState.FrameBuffers.GetBuffer(FrameBufferType.DeferredDepthAndSpecularPower, false));
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.outlineEffect);
            spriteBatch.Draw(this.rtPass1, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();

            // scale and render to back buffer
            this.Application.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, null, this.scaleEffect);
            spriteBatch.Draw(this.rtPass2, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            spriteBatch.End();

            // draw GUI
            this.guiLayer.BeginDraw(gameTime);
            this.guiLayer.Draw(gameTime);
            this.guiLayer.EndDraw(gameTime);
            this.miniMapLayer.BeginDraw(gameTime);
            this.miniMapLayer.Draw(gameTime);
            this.miniMapLayer.EndDraw(gameTime);

            if (this.showConsole)
            {
                SystemConsole.Render(SystemStatisticCategory.Performance, true, true, Vector2.Zero, Vector2.One, Color.White, gameTime);
            }
        }

        #region Controls
        /// <summary>
        /// Sets up controls and actions
        /// </summary>
        private void SetUpControls()
        {
            // state switches
            // TODO: check if its allowed to switch!
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.T, InputState.Free, new InputStateMachine.InputAction(SwitchToTerraformMode));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.G, InputState.Terraform, new InputStateMachine.InputAction(SwitchToDigMode));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.P, InputState.Terraform, new InputStateMachine.InputAction(SwitchToDumpMode));

            this.inputStateMachine.RegisterKeyboardActionForState(Keys.F, InputState.Dig, new InputStateMachine.InputAction(SwitchToFreeMode));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.F, InputState.Dump, new InputStateMachine.InputAction(SwitchToFreeMode));

            this.inputStateMachine.RegisterKeyboardActionForState(Keys.B, InputState.Free, new InputStateMachine.InputAction(SwitchToBuildMode));

            this.inputStateMachine.RegisterKeyboardActionForState(Keys.F, InputState.Terraform, new InputStateMachine.InputAction(SwitchToFreeMode));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.F, InputState.Build, new InputStateMachine.InputAction(SwitchToFreeMode));

            // game actions           
            //this.inputStateMachine.RegisterMouseActionForState(MouseInput.LeftButton, InputState.Dig, new InputStateMachine.InputAction(OnClickDig));
            //this.inputStateMachine.RegisterMouseActionForState(MouseInput.LeftButton, InputState.Dump, new InputStateMachine.InputAction(OnClickDump));

            // debug actions
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.C, InputState.Free, new InputStateMachine.InputAction(ToggleConsole));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.Add, InputState.Free, new InputStateMachine.InputAction(IncreaseDebugValue));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.Subtract, InputState.Free, new InputStateMachine.InputAction(DecreaseDebugValue));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.D1, InputState.Free, new InputStateMachine.InputAction(DebugAddCharacter));
            this.inputStateMachine.RegisterKeyboardActionForState(Keys.D2, InputState.Free, new InputStateMachine.InputAction(DebugAddNpc));
            //this.inputStateMachine.RegisterKeyboardActionForState(Keys.OemQuestion, InputState.Free, new InputStateMachine.InputAction(SaveGame));
            //this.inputStateMachine.RegisterKeyboardActionForState(Keys.S | Keys.RightControl, InputState.Free, new InputStateMachine.InputAction(SaveGame));
        }

        private void SaveGame(Button ctrlS, GameTime gameTime)
        {
            if (this.saveStateManager.CurrentSaveState == null)
            {
                this.saveStateManager.SaveNewState();
            }
            this.saveStateManager.CurrentSaveState.Put(this.terrain);
            this.saveStateManager.WriteCurrentSaveState();
        }

        private void DebugAddNpc(Button two, GameTime gameTime)
        {
            if (two.IsReleased)
            {
                if (this.terrainPickResult.HasValue)
                {
                    var newChar = new Character(string.Format("testNPC{0}", gameTime.TotalGameTime), Application.ContentManager, Application.GraphicsDevice);
                    var pos = this.terrainPickResult.Value.BlockPosition.WorldPosition;
                    newChar.Position = pos;

                    var behavior = new HumanIdleBehavior(this.terrain);                                       

                    var agent = SessionManager.CurrentSession.CreateNonPlayerAgent();
                    newChar.Components.Add(agent);
                    agent.Add(behavior);

                    this.characterSceneGroup.Add(newChar);
                    Application.SunBurn.ObjectManager.Submit(newChar);
                }
            }
        }

        private void DebugAddCharacter(Button one, GameTime gameTime)
        {
            if (one.IsReleased)
            {
                if (this.terrainPickResult.HasValue)
                {
                    var newChar = new Character("testChar", Application.ContentManager, Application.GraphicsDevice);
                    var pos = this.terrainPickResult.Value.BlockPosition.WorldPosition;
                    newChar.Position = pos;

                    var behavior = new PlayerClickBehavior(this.terrain);
                    newChar.Components.Add(this.singlePlayerAgent);
                    this.singlePlayerAgent.Add(behavior);
                    
                    this.characterSceneGroup.Add(newChar);
                    Application.SunBurn.ObjectManager.Submit(newChar);
                }
            }
        }

        private void IncreaseDebugValue(Button plus, GameTime gameTime)
        {
            if (plus.IsReleased)
            {

            }
        }

        private void DecreaseDebugValue(Button minus, GameTime gameTime)
        {
            if (minus.IsReleased)
            {

            }
        }

        private void ToggleConsole(Button c, GameTime gameTime)
        {
            if (c.IsReleased)
            {
                this.showConsole = !this.showConsole;
            }
        }

        private void SwitchToTerraformMode(Button T, GameTime gt)
        {
            if (T.IsReleased)
            {
                this.ChangeInputState(InputState.Dig);
            }
        }

        private void SwitchToBuildMode(Button b, GameTime gt)
        {
            if (b.IsReleased)
            {
                this.ChangeInputState(InputState.Build);
            }
        }

        private void SwitchToFreeMode(Button b, GameTime gt)
        {
            if (b.IsReleased)
            {
                this.ChangeInputState(InputState.Free);
            }
        }

        private void SwitchToDigMode(Button b, GameTime gt)
        {
            if (b.IsReleased)
            {
                this.ChangeInputState(InputState.Dig);
            }
        }

        private void SwitchToDumpMode(Button b, GameTime gt)
        {
            if (b.IsReleased)
            {
                this.ChangeInputState(InputState.Dump);
            }
        }

        #endregion Controls

        #region IGameController Members
        /// <summary>
        /// Changes input state from outside event
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeInputState(InputState newState)
        {
            switch (newState)
            {
                case InputState.Dump:
                case InputState.Dig:
                case InputState.Terraform:
                    this.selectionLayer.IsVisible = true;
                    break;
                default:
                case InputState.Free:
                    this.selectionLayer.IsVisible = false;
                    break;
            }

            this.inputStateMachine.ChangeState(newState);
        }

        /// <summary>
        /// Returns the current mouse pick context for the GUI
        /// </summary>
        public MousePickContext CurrentMousePickContext
        {
            get
            {
                //TODO: evaluate all pick results to find the nearest and put it here
                return MousePickContext.Ground;
            }
        }

        public InputState CurrentInputState
        {
            get { return this.inputStateMachine.CurrentState; }
        }

        /// <summary>
        /// Accepts the current input
        /// </summary>
        public void AcceptInput()
        {
            switch (this.inputStateMachine.CurrentState)
            {
                default: this.ChangeInputState(InputState.Free); break;
            }
        }

        /// <summary>
        /// Cancels the current input
        /// </summary>
        public void CancelInput()
        {
            switch (this.inputStateMachine.CurrentState)
            {
                default: this.ChangeInputState(InputState.Free); break;
            }
        }

        /// <summary>
        /// Handles a left click event caught by the GUI
        /// </summary>
        public void OnLeftClick()
        {
            // digging
            if (this.inputStateMachine.CurrentState == InputState.Dig)
            {
                if (this.terrainPickResult.HasValue)
                {
                    foreach (var so in this.terrain.SurfaceSceneObjects)
                    {
                        this.terrainSceneGroup.Remove(so);
                    }

                    this.terrain.RemoveBlock(this.terrainPickResult.Value);

                    foreach (var so in this.terrain.SurfaceSceneObjects)
                    {
                        this.terrainSceneGroup.Add(so);
                    }
                    this.townScene.Apply();
                }
            } // dumping
            else if (this.inputStateMachine.CurrentState == InputState.Dump)
            {
                if (this.terrainPickResult.HasValue)
                {
                    // TODO: check if position is already dumped upon, increase dump stack
                    var vecBlockPos = new Vector3(
                        this.terrainPickResult.Value.BlockPosition.X + this.terrainPickResult.Value.Region.OriginX,
                        this.terrainPickResult.Value.BlockPosition.Y,
                        this.terrainPickResult.Value.BlockPosition.Z + this.terrainPickResult.Value.Region.OriginZ);
                    var dumpPosition = vecBlockPos + (Vector3.Up * (1 / TerrainBlock.BLOCK_HEIGHT)) + Vector3.Up;
                    var intDumpPosition = new TerrainBlock.IntVector3()
                    {
                        X = (int)dumpPosition.X,
                        Y = (int)dumpPosition.Y,
                        Z = (int)dumpPosition.Z
                    };

                    this.dumpArea.AddDumpSite(new DumpArea.DumpSite()
                    {
                        Location = intDumpPosition,
                        OriginalBlock = this.currentDumpingBlock
                    });

                    var prevMesh = this.dumpAreaPresentation.SceneObject;
                    var areaMesh = this.dumpAreaPresentation.BuildSceneObject();

                    if (prevMesh != areaMesh)
                    {
                        if (prevMesh != null)
                        {
                            this.SunBurn.ObjectManager.Remove(prevMesh);
                        }
                        this.SunBurn.ObjectManager.Submit(areaMesh);
                    }
                }
            }
        }

        #endregion
    }
}
