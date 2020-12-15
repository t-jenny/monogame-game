using System;
using GameDemo.Utils;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Components
{
    public class OptionsList
    {
        public readonly struct Option
        {
            public Option(string label, SpriteFont font, Point coords)
            {
                Label = label;
                Rect = new Rectangle(coords, font.MeasureString(label).ToPoint());
            }
            public string Label { get; }
            public Rectangle Rect { get; }

        }
        private List<Option> Options;
        private SpriteFont Font;
        private Rectangle Rect;
        public string SelectedOption { get; private set; }

        public OptionsList(string[] options, SpriteFont font, Rectangle rect)
        {
            Font = font;
            Rect = rect;
            int Diff = 20;
            int LineHeight = (int) font.MeasureString("A").Y;
            Options = new List<Option>();
            for (int i = 0; i < options.Length; i++)
            {
                Point Coords = new Point(Rect.X + Diff, Rect.Y + LineHeight * i + Diff);
                Options.Add(new Option(options[i], font, Coords));
            }
        }

        public void Update()
        {
            MouseState MouseState = Mouse.GetState();
            Point MousePoint = new Point(MouseState.X, MouseState.Y);

            foreach (Option Opt in Options)
            {
                if (Opt.Rect.Contains(MousePoint) && MouseState.LeftButton == ButtonState.Pressed)
                {
                    SelectedOption = Opt.Label;
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            foreach (Option Opt in Options)
            {
                if (Opt.Label == SelectedOption)
                {
                    DrawingUtils.DrawUnderline(spriteBatch, graphics, Opt.Rect, Color.Purple);
                }
                spriteBatch.DrawString(Font, Opt.Label, new Vector2(Opt.Rect.X, Opt.Rect.Y), Color.Black);
            }
        }
    }
}
