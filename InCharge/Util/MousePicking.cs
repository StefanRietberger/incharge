using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Indiefreaks.Xna.Input;
using Indiefreaks.Xna.Core;
using Microsoft.Xna.Framework.Graphics;
using Indiefreaks.Xna.Rendering.Camera;
using Microsoft.Xna.Framework.Input;

namespace InCharge.Util
{
    public static class MousePicking
    {
        /// <summary>
        /// Indiefreaks Framework Extension: Get a picking ray for the current camera view.
        /// </summary>
        /// <param name="mouseInputState"></param>
        /// <returns></returns>
        public static Ray GetPickingRay()
        {
            var ms = Application.Input.MouseState;

            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.            
            Viewport vp = Application.Graphics.GraphicsDevice.Viewport;
            var camera = Application.SunBurn.GetManager<ICameraManager>(true).ActiveCamera;
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(ms.X, ms.Y, 0), camera.SceneState.Projection, camera.SceneState.View, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(ms.X, ms.Y, 1), camera.SceneState.Projection, camera.SceneState.View, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);
            Ray pickingRay = new Ray(pos1, dir);
            return pickingRay;
        }
    }
}
