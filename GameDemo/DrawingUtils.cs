using System;
using System.Collections.Generic;
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

        public static Texture2D FilledRectangle(GraphicsDeviceManager graphics, Rectangle rect, Color color)
        {
            Color[] Colors = new Color[rect.Width * rect.Height];
            for (int i = 0; i < Colors.Length; ++i) Colors[i] = color;
            Texture2D FullRect = new Texture2D(graphics.GraphicsDevice, rect.Width, rect.Height);
            FullRect.SetData(Colors);
            return FullRect;
        }

        public static void DrawFilledRectangle(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Rectangle rect, Color color)
        {
            Texture2D FullRect = FilledRectangle(graphics, rect, color);
            spriteBatch.Draw(FullRect, rect, color);
            return;
        }

        public static void DrawGradientRectangle(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Rectangle rect, Color color)
        {
            Texture2D GradTex = GradientTexture(graphics, rect.Width, rect.Height, color);
            spriteBatch.Draw(GradTex, rect, color);
        }

        public static void DrawOpenRectangle(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Rectangle rect, Color color)
        {
            Texture2D border = new Texture2D(graphics.GraphicsDevice, 1, 1);
            border.SetData(new[] { Color.White });
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Top, 2, rect.Height), color);
            spriteBatch.Draw(border, new Rectangle(rect.Right, rect.Top, 2, rect.Height), color);
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Top, rect.Width, 2), color);
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Bottom, rect.Width + 2, 2), color);
            return;
        }

        public static void DrawUnderline(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Rectangle rect, Color color)
        {
            Texture2D border = new Texture2D(graphics.GraphicsDevice, 1, 1);
            border.SetData(new[] { Color.White });
            spriteBatch.Draw(border, new Rectangle(rect.Left, rect.Bottom, rect.Width + 2, 2), color);
            return;
        }

        public static void DrawTextBanner(SpriteBatch spriteBatch, GraphicsDeviceManager graphics,
            SpriteFont font,
            string text,
            Color bannerCol,
            Color textCol)
        {
            int WindowWidth = (int)graphics.GraphicsDevice.Viewport.Width;
            Rectangle BannerRect = new Rectangle(0, 0, WindowWidth, 100);
            DrawGradientRectangle(spriteBatch, graphics, BannerRect, bannerCol);

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
                data[i].R = Convert.ToByte(data[i].R * (1.0f - percentage));
                data[i].G = Convert.ToByte(data[i].G * (1.0f - percentage));
                data[i].B = Convert.ToByte(data[i].B * (1.0f - percentage));
            }
            NewTexture.SetData(data);
            spriteBatch.Draw(NewTexture, rect, Color.White);
        }

        public static List<string> WrappedString(SpriteFont font, string text, Rectangle rect, float padding)
        {
            List<string> Pages = new List<string>();
            string NewString = string.Empty;
            string[] Words = text.Split(" ");
            float LineWidth = 0.0f;
            float LineHeight = font.MeasureString("A").Y;
            float TextHeight = LineHeight;

            for (int i = 0; i < Words.Length; i++)
            {
                float WordLength = font.MeasureString(Words[i]).X;
                LineWidth += WordLength;
                string Space = " ";
                if (LineWidth > (1.0f - 2.0f * padding) * rect.Width)
                {
                    TextHeight += LineHeight;
                    if (TextHeight > (1.0f - 2.0f * padding) * rect.Height)
                    {
                        Pages.Add(NewString);
                        NewString = string.Empty;
                        TextHeight = 0.0f;
                    }
                    else
                    {
                        NewString += Environment.NewLine;
                    }
                    LineWidth = WordLength;
                }

                // There's a subtle bug here...
                if (Words[i].Equals("\n"))
                {
                    LineWidth = 0.0f;
                    Space = "";
                    TextHeight += LineHeight;
                }

                NewString += Words[i] + Space;
            }
            Pages.Add(NewString);
            return Pages;
        }

    }
}
