using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Utils
{
    // static utilities for drawing
    public static class DrawingUtils
    {
        public static Texture2D FilledRectangle(GraphicsDeviceManager graphics, int wd, int ht, Color color)
        {
            Color[] Colors = new Color[wd * ht];
            for (int i = 0; i < Colors.Length; ++i) Colors[i] = color;
            Texture2D FullRect = new Texture2D(graphics.GraphicsDevice, wd, ht);
            FullRect.SetData(Colors);
            return FullRect;
        }
    }
}
