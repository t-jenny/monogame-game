﻿using System;
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
        protected const int MenuWidth = 450;
        protected const int MenuHeight = 250;
        protected string StaticText { get; set; } = "Are you sure?";
        protected Vector2 Position { get; set; } = new Vector2(400, 300);
        protected Texture2D Menu { get; set; }

        private protected List<Button> Buttons;
        protected List<string> ButtonLabels;

        public string ConfirmButtonText { get; protected set; } = "Yes";
        public string CancelButtonText { get; protected set; } = "No";

        protected PopupMenu(ContentManager content)
        {
            Buttons = new List<Button>();
            ButtonLabels = new List<string>();
            Menu = content.Load<Texture2D>("parchment");
        }

        public virtual void Update()
        {
            if (Buttons.Count == 0) return;
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].Update();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            // Location Menu
            Vector2 TextSize = font.MeasureString(StaticText);

            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);
            spriteBatch.DrawString(font, StaticText, new Vector2(Position.X + (MenuWidth - TextSize.X) / 2, Position.Y + MenuHeight / 10), Color.Black);

            for (int i = 0; i < ButtonLabels.Count; i++)
            {
                if (i + 1 > Buttons.Count)
                {
                    Buttons.Add(new Button(ButtonLabels[i], font,
                    (int)Position.X + (i + 1) * MenuWidth / (ButtonLabels.Count + 1),
                    (int)Position.Y + MenuHeight / 2));
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
        public ConfirmMenu(string query, ContentManager content) : base(content)
        {
            StaticText = query;
            ButtonLabels.Add(ConfirmButtonText);
            ButtonLabels.Add(CancelButtonText);
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