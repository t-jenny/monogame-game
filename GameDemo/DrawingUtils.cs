using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Utils
{
    // static utilities for drawing
    public static class DrawingUtils
    {
        public static void DrawFilledRectangle(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Color[] Colors = new Color[rect.Width * rect.Height];
            for (int i = 0; i < Colors.Length; ++i) Colors[i] = color;
            Texture2D FullRect = new Texture2D(graphics.GraphicsDevice, rect.Width, rect.Height);
            FullRect.SetData(Colors);
            spriteBatch.Draw(FullRect, rect, color);
            return;
        }

        public static void DrawOpenRectangle(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D border = new Texture2D(graphics.GraphicsDevice, 1, 1);
            border.SetData(new[] { Color.White });
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Top, 2, rect.Height), color);
            spriteBatch.Draw(border, new Rectangle(rect.Right, rect.Top, 2, rect.Height), color);
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Top, rect.Width, 2), color);
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Bottom, rect.Width + 2, 2), color);
            return;
        }

        public static void DrawTextBanner(GraphicsDeviceManager graphics,
            SpriteBatch spriteBatch,
            SpriteFont font,
            string text,
            Color bannerCol,
            Color textCol)
        {
            int WindowWidth = (int)graphics.GraphicsDevice.Viewport.Width;
            Rectangle BannerRect = new Rectangle(0, 0, WindowWidth, 100);
            DrawFilledRectangle(graphics, spriteBatch, BannerRect, bannerCol);

            spriteBatch.DrawString(font, text, new Vector2(10.0f, 30.0f), textCol);
        }
    }
}
