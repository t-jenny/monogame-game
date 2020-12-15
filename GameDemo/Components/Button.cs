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

        public Button(String text, SpriteFont font, Vector2 position)
        { 
            Text = text;
            Font = font;
            Vector2 ButtonDims = Font.MeasureString(text);
            ButtonDims += new Vector2(10.0f, 0.0f);

            Rect = new Rectangle(position.ToPoint(), ButtonDims.ToPoint());
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
            DrawingUtils.DrawGradientRectangle(spriteBatch, graphics, Rect, ButtonCol);
            Vector2 ButtonPos = new Vector2(Rect.X + 5, Rect.Y);
            spriteBatch.DrawString(Font, Text, ButtonPos, TextCol);

            // draw border
            DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, Rect, Color.Gray);
        }
    }
}
