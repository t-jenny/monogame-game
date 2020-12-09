using System;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Components
{
    // A menu with explanatory text and two options (confirm or cancel)
    public class PopupMenu
    {
        protected const int MenuWidth = 450;
        protected const int MenuHeight = 250;
        protected string StaticText { get; set; }
        protected Vector2 Position { get; set; }
        protected Texture2D Menu { get; set; }

        private protected Button ConfirmButton;
        protected string ConfirmButtonText { get; set; }
        private protected Button CancelButton;
        protected string CancelButtonText { get; set; }

        protected PopupMenu()
        {
        }

        public virtual void Update()
        {
            if (ConfirmButton == null || CancelButton == null) return;
            ConfirmButton.Update();
            CancelButton.Update();
        }

        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            // Location Menu
            Vector2 TextSize = font.MeasureString(StaticText);

            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);
            spriteBatch.DrawString(font, StaticText, new Vector2(Position.X + (MenuWidth - TextSize.X) / 2, Position.Y + MenuHeight / 10), Color.Black);

            if (ConfirmButton == null)
            {
                ConfirmButton = new Button(ConfirmButtonText, font,
                    (int)Position.X + MenuWidth / 3,
                    (int)Position.Y + MenuHeight / 2);
            }
            ConfirmButton.Draw(spriteBatch, graphics);

            if (CancelButton == null)
            {
                CancelButton = new Button(CancelButtonText, font,
                    (int)Position.X + 2 * MenuWidth / 3,
                    (int)Position.Y + MenuHeight / 2);
            }
            CancelButton.Draw(spriteBatch, graphics);
        }

        public virtual bool IsCanceling(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(CancelButton.Rect);
        }

        public bool IsConfirming(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(ConfirmButton.Rect);
        }
    }

    public class ConfirmMenu : PopupMenu
    {
        public ConfirmMenu(string query, ContentManager content) : base()
        {
            StaticText = query;
            Menu = content.Load<Texture2D>("parchment");
            Position = new Vector2(400, 300);
            ConfirmButtonText = "Yes";
            CancelButtonText = "No";
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            Color Overlay = Color.Lerp(Color.Transparent, Color.Black, 0.7f);
            GraphicsDevice GD = graphics.GraphicsDevice;
            DrawingUtils.DrawFilledRectangle(graphics, spriteBatch, GD.Viewport.Bounds, Overlay);
            base.Draw(spriteBatch, font, graphics);
        }
    }
}
