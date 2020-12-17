using System;
using System.Collections.Generic;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Components
{
    public class InfoTable
    {
        private Dictionary<string, int> InfoDict;
        private readonly string[] Aspects;
        private Rectangle Rect;
        private SpriteFont Font;

        public InfoTable(Dictionary<string, int> dict, string[] aspects, Rectangle rect, SpriteFont font)
        {
            InfoDict = dict;
            Aspects = aspects;
            Rect = rect;
            Font = font;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            DrawingUtils.DrawFilledRectangle(spriteBatch, graphics, Rect, Color.Beige);
            DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, Rect, Color.Black, 3);
            Rectangle StatRect = new Rectangle(Rect.X, Rect.Y, 4 * Rect.Width / 5, Rect.Height / Aspects.Length);
            Rectangle ValueRect = new Rectangle(Rect.X + StatRect.Width,
                Rect.Y, Rect.Width - StatRect.Width, Rect.Height / Aspects.Length);

            for (int i = 0; i < Aspects.Length; i++)
            {
                DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, StatRect, Color.Black, 2);
                spriteBatch.DrawString(Font, Aspects[i], new Vector2(StatRect.X + 5.0f, StatRect.Y + 5.0f), Color.Black);
                DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, ValueRect, Color.Black, 2);
                if (InfoDict.ContainsKey(Aspects[i]))
                {
                    spriteBatch.DrawString(Font, InfoDict[Aspects[i]].ToString(),
                        new Vector2(ValueRect.X + 5.0f, ValueRect.Y + 5.0f), Color.Black);
                }
                StatRect.Y += StatRect.Height;
                ValueRect.Y += ValueRect.Height;
            }
        }
    }
}
