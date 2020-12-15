using System;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Components
{
    public class ClickableTexture
    {
        private bool IsHovered = false;
        private bool IsClicked = false;
        private Texture2D BaseTexture;

        public Rectangle Rect { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public ClickableTexture(Texture2D texture, Vector2 position)
        {
            X = (int) position.X;
            Y = (int) position.Y;
            BaseTexture = texture;
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Rect = new Rectangle(X, Y, Width, Height);
        }

        // Same code as Button.Update
        public void Update()
        {
            MouseState MouseState = Mouse.GetState();
            Point MousePoint = new Point(MouseState.X, MouseState.Y);

            if (Rect.Contains(MousePoint))
            {
                IsHovered = true;
                IsClicked = MouseState.LeftButton == ButtonState.Pressed;
            }
            else
            {
                IsHovered = false;
                IsClicked = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (IsClicked)
            {
                DrawingUtils.DrawShadedTexture(graphics, spriteBatch, BaseTexture, 0.6f, Rect);
            }
            else if (IsHovered)
            {
                DrawingUtils.DrawShadedTexture(graphics, spriteBatch, BaseTexture, 0.3f, Rect);
            }
            else
            {
                spriteBatch.Draw(BaseTexture, Rect, Color.White);
            }
        }
    }
}
