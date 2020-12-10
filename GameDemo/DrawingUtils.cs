using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Utils
{
    // static utilities for drawing
    public static class DrawingUtils
    {
        private static Texture2D GradientTexture(GraphicsDeviceManager graphics, int width, int height, Color color)
        {
            Texture2D GradTex = new Texture2D(graphics.GraphicsDevice, width, height);
            Color[] GradientCols = new Color[height * width];
            Color InitialCol = Color.Lerp(Color.White, color, 0.3f);
            Color FinalCol = Color.Lerp(Color.Black, color, 0.8f);
            int GradientThickness = 2;

            for (int i = 0; i < height; i++)
            {
                float Percent = (float) (i / GradientThickness) / (float) ((height - 1) / GradientThickness);
                Color RowCol = Color.Lerp(InitialCol, FinalCol, Percent);
                for (int j = 0; j < width; j++)
                {
                    GradientCols[(i * width) + j] = RowCol;
                }
            
            }
            GradTex.SetData(GradientCols);
            return GradTex;
        }

        public static Texture2D FilledRectangle(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Color[] Colors = new Color[rect.Width * rect.Height];
            for (int i = 0; i < Colors.Length; ++i) Colors[i] = color;
            Texture2D FullRect = new Texture2D(graphics.GraphicsDevice, rect.Width, rect.Height);
            FullRect.SetData(Colors);
            return FullRect;
        }

        public static void DrawFilledRectangle(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D FullRect = FilledRectangle(graphics, spriteBatch, rect, color);
            spriteBatch.Draw(FullRect, rect, color);
            return;
        }

        public static void DrawGradientRectangle(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D GradTex = GradientTexture(graphics, rect.Width, rect.Height, color);
            spriteBatch.Draw(GradTex, rect, color);
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

        public static void DrawUnderline(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D border = new Texture2D(graphics.GraphicsDevice, 1, 1);
            border.SetData(new[] { Color.White });
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
            DrawGradientRectangle(graphics, spriteBatch, BannerRect, bannerCol);

            spriteBatch.DrawString(font, text, new Vector2(10.0f, 30.0f), textCol);
        }

        public static void DrawShadedTexture(GraphicsDeviceManager graphics,
            SpriteBatch spriteBatch, Texture2D texture, float percentage, Rectangle rect)
        {
            Texture2D NewTexture = new Texture2D(graphics.GraphicsDevice, rect.Width, rect.Height);
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Lerp(data[i], Color.Black, percentage);
            }
            NewTexture.SetData(data);
            spriteBatch.Draw(NewTexture, rect, Color.White);
        }

        public static string WrappedString(SpriteFont font, string text, Rectangle rect, float padding)
        {
            string NewString = string.Empty;
            string[] Words = text.Split(" ");
            float LineLength = 0.0f;
            for (int i = 0; i < Words.Length; i++)
            {
                LineLength += font.MeasureString(Words[i] + " ").X;
                string Space = " ";
                if (LineLength > (1.0f - 2.0f * padding) * rect.Width)
                {
                    NewString += Environment.NewLine;
                    LineLength = 0.0f;
                }
                if (Words[i].Equals("\n"))
                {
                    LineLength = 0.0f;
                    Space = "";
                }

                NewString += Words[i] + Space;
            }
            return NewString;
        }

    }
}
