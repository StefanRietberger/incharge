using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace InCharge.Util
{
    /// <summary>
    /// Scales a Texture using the Scale2X algorithm and returns the rescaled version.
    /// For pixel art it tends to produce much better results than bilinear filtering.
    /// Can be run over an image twice for Scale4X or three times for Scale8X, etc.
    /// For sample images and more information the website of the algorithm is:
    /// http://scale2x.sourceforge.net/
    /// </summary>
    public static class Texture2DExtensions
    {
        public static Texture2D CelShade(this Texture2D texture, GraphicsDevice graphics)
        {
            Texture2D newTex = new Texture2D(graphics, texture.Width, texture.Height, true, SurfaceFormat.Color);

            Color[] newData = new Color[texture.Width * texture.Height];
            texture.GetData(newData);

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    int index = y * texture.Width + x;

                    Color c = newData[index];
                    Color gray = new Color(c.R * 0.3f, c.G * 0.59f, c.B * 0.11f);
                    newData[index] = gray;
                }
            }

            return newTex;
        }

        public static Texture2D Scale2X(this Texture2D texture, GraphicsDevice graphics)
        {
            int oldWidth = texture.Width;
            int oldHeight = texture.Height;
            int newWidth = oldWidth * 2;
            int newHeight = oldHeight * 2;

            Texture2D newTex = new Texture2D(graphics, newWidth, newHeight, true, SurfaceFormat.Color);

            Color[] textureData = new Color[oldWidth * oldHeight];
            Color[] cNT = new Color[newWidth * newHeight];
            texture.GetData(textureData);

            Color blank = Color.Transparent;

            Color B, D, E, F, H;

            //iterate over every pixel in the source image
            for (int oldY = 0; oldY < oldHeight; ++oldY)
            {
                int nY0 = (oldY * 2 + 0) * newWidth;
                int nY1 = (oldY * 2 + 1) * newWidth;

                int yIndex = oldY * oldWidth;
                int yIndexPrev = (oldY - 1) * oldWidth;
                int yIndexNext = (oldY + 1) * oldWidth;

                for (int oldX = 0; oldX < oldWidth; ++oldX)
                {
                    //this is the center pixel
                    E = textureData[oldX + yIndex];

                    //swap blank for E to use the algorithm as described
                    //I find blank works better for this type of content
                    B = oldY > 0 ? textureData[oldX + yIndexPrev] : blank;

                    D = oldX > 0 ? textureData[oldX - 1 + yIndex] : blank;
                    F = oldX < oldWidth - 1 ? textureData[oldX + 1 + yIndex] : blank;

                    H = oldY < oldHeight - 1 ? textureData[oldX + yIndexNext] : blank;

                    int nX0 = oldX * 2;
                    int nX1 = nX0 + 1;

                    //set the output pixels
                    if (B != H && D != F)
                    {
                        cNT[nX0 + nY0] = D == B ? D : E;
                        cNT[nX1 + nY0] = B == F ? F : E;

                        cNT[nX0 + nY1] = D == H ? D : E;
                        cNT[nX1 + nY1] = H == F ? F : E;
                    }
                    else
                    {
                        cNT[nX0 + nY0] = E; cNT[nX1 + nY0] = E;
                        cNT[nX0 + nY1] = E; cNT[nX1 + nY1] = E;
                    }
                }
            }

            newTex.SetData(cNT);

            return newTex;
        }
    }
}