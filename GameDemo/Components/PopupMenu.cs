using System;
using System.Collections.Generic;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Components
{
    // A menu with explanatory text and two options (confirm or cancel)
    public class PopupMenu
    {
        protected int MenuWidth { get; set; } = 450;
        protected int MenuHeight { get; set; } = 250;
        protected string StaticText { get; set; } = "Are you sure?";
        protected Vector2 Position { get; set; } = new Vector2(400, 300); // center of screen
        protected Texture2D Menu { get; set; }
        protected SpriteFont Font { get; set; }

        private protected List<Button> Buttons;
        protected List<string> ButtonLabels;

        public string ConfirmButtonText { get; protected set; } = "Yes";
        public string CancelButtonText { get; protected set; } = "No";

        protected PopupMenu(ContentManager content, SpriteFont font)
        {
            Buttons = new List<Button>();
            ButtonLabels = new List<string>();
            Menu = content.Load<Texture2D>("parchment");
            Font = font;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Buttons.Count == 0) return;
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].Update();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Location Menu
            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);
            string WrappedText = DrawingUtils.WrappedString(Font, StaticText, MenuRect, 0.1f)[0]; // must be one page

            Vector2 TextSize = Font.MeasureString(WrappedText);
            float LineHeight = Font.MeasureString("A").Y;
            Vector2 TextPos = new Vector2(Position.X + (MenuWidth - TextSize.X) / 2, Position.Y + LineHeight);

            spriteBatch.DrawString(Font, WrappedText, TextPos, Color.Black);

            for (int i = 0; i < ButtonLabels.Count; i++)
            {
                if (i + 1 > Buttons.Count)
                {
                    Vector2 ButtonTextSize = Font.MeasureString(ButtonLabels[i]);
                    Vector2 ButtonPos = new Vector2(Position.X + (i + 1) * MenuWidth / (ButtonLabels.Count + 1) - ButtonTextSize.X / 2,
                        (TextPos + TextSize).Y + LineHeight / 4);
                    Buttons.Add(new Button(ButtonLabels[i], Font, ButtonPos));
                }
                Buttons[i].Draw(spriteBatch, graphics);
            }
        }

        public virtual string ClickedText(Rectangle mouseClickRect)
        {
            for (int i = 0; i < ButtonLabels.Count; i++)
            {
                if (mouseClickRect.Intersects(Buttons[i].Rect)) {
                    return ButtonLabels[i];
                }
            }
            return "No Selection";
        }

        public virtual bool IsConfirming(Rectangle mouseClickRect)
        {
            return ClickedText(mouseClickRect) == ConfirmButtonText;
        }

        public virtual bool IsCancelling(Rectangle mouseClickRect)
        {
            return ClickedText(mouseClickRect) == CancelButtonText;
        }

        public void DisableButton(string buttonText)
        {
            ButtonLabels.Remove(buttonText);
        }
    }

    public class ConfirmMenu : PopupMenu
    {
        public ConfirmMenu(string query, ContentManager content, SpriteFont font) : base(content, font)
        {
            StaticText = query;
            ButtonLabels.Add(ConfirmButtonText);
            ButtonLabels.Add(CancelButtonText);
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Color Overlay = Color.Lerp(Color.Transparent, Color.Black, 0.7f);
            GraphicsDevice GD = graphics.GraphicsDevice;
            DrawingUtils.DrawFilledRectangle(spriteBatch, graphics, GD.Viewport.Bounds, Overlay);
            base.Draw(spriteBatch, graphics);
        }
    }
}
