using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InCharge.Procedural.Terrain;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Effects.Deferred;
using Microsoft.Xna.Framework.Content;
using SynapseGaming.LightingSystem.Rendering;

namespace InCharge.Procedural.Model
{
    public class WorldMapPresentation : IPresentation
    {
        private WorldMap world;

        /// <summary>
        /// Base indices for a quad
        /// </summary>
        private static int[] baseQuadIndices = { 0, 1, 2, 2, 1, 3 };

        private Texture2D texTerrain;
        private Texture2D texTerrainNormals;
        private Texture2D texBlendNorth;
        private Texture2D texBlendEast;
        private Texture2D texBlendSouth;
        private Texture2D texBlendWest;
        private DeferredSasEffect effectTerrain;

        private readonly ContentManager content;
        private readonly GraphicsDevice device;

        public WorldMapPresentation(WorldMap world)
        {
            this.world = world;
        }



        #region IPresentation Members

        public SceneObject BuildSceneObject()
        {
            return null;
        }

        #endregion
    }
}
