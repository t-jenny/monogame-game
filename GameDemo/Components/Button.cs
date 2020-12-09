using System;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Components
{
    public class Button
    {
        private readonly string Text;
        private SpriteFont Font;

        private bool IsHovered = false;
        private bool IsClicked = false;

        public Rectangle Rect { get; set; }

        public Button(String text, SpriteFont font, int x, int y)
        { 
            Text = text;
            Font = font;
            Vector2 TextSize = Font.MeasureString(text);

            Rect = new Rectangle(x - ((int)TextSize.X - 10) / 2, y,
                (int)TextSize.X + 10,
                (int)TextSize.Y);
        }

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
            Color ButtonCol = Color.LightGray;
            Color TextCol = Color.Black;
            if (IsHovered)
            {
                ButtonCol = Color.DarkGray;
            }
            if (IsClicked)
            {
                ButtonCol = Color.Gray;
                TextCol = Color.DarkGray;
            }

            // draw button 
            DrawingUtils.DrawFilledRectangle(graphics, spriteBatch, Rect, ButtonCol);
            Vector2 ButtonPos = new Vector2(Rect.X + 5, Rect.Y);
            spriteBatch.DrawString(Font, Text, ButtonPos, TextCol);

            // draw border
            DrawingUtils.DrawOpenRectangle(graphics, spriteBatch, Rect, Color.Black);
        }
    }
}
