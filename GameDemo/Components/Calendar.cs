using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDemo.Utils;

namespace GameDemo.Components
{
    public class Calendar
    {
        private string[] EntryArray = new string[5];
        private SpriteFont Font;
        private Rectangle WeekRect;
        private Rectangle TodayRect;
        private DateTime Monday;
        private bool Moving = false;

        public int DayIndex { get; private set; } = 0;

        public Calendar(Rectangle rect, SpriteFont font, DateTime monday)
        {
            WeekRect = rect;
            Font = font;
            Monday = monday;
            TodayRect = new Rectangle(rect.X - 4, rect.Y - 4, rect.Width / 6 + 8, rect.Height + 8);
        }

        public void AddEntry(string entry)
        {
            if (DayIndex > 4) return;
            EntryArray[DayIndex] = entry;
            DayIndex++;
        }

        public bool IsMoving()
        {
            return Moving;
        }

        public void MoveDay()
        {
            Moving = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!Moving) return;
            int MoveTime = 1000; // time in milliseconds
            int MoveDistance = WeekRect.Width / 6;

            if (TodayRect.X < WeekRect.X + MoveDistance * DayIndex - 4)
            {
                TimeSpan Increment = gameTime.ElapsedGameTime;
                float HalfwayX = WeekRect.X + MoveDistance * (DayIndex - 0.5f);
                float Scale = -2 * Math.Abs((TodayRect.X - HalfwayX) / MoveDistance) + 2;
                TodayRect.X += (int)(Scale * (Increment.TotalMilliseconds / MoveTime) * MoveDistance);
            }
            else
            {
                TodayRect.X = WeekRect.X + MoveDistance * DayIndex - 4;
                Moving = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            DrawingUtils.DrawFilledRectangle(spriteBatch, graphics, WeekRect, Color.Beige);
            string[] DayInits = new string[6] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

            for (int i = 0; i < 6; i++)
            {
                Rectangle HeaderRect = new Rectangle(WeekRect.X + i * WeekRect.Width / 6, WeekRect.Y, WeekRect.Width / 6, 50);
                DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, HeaderRect, Color.Black, 2);
                spriteBatch.DrawString(Font, DayInits[i] + " - " + Monday.AddDays(i).ToString("M/d"),
                    new Vector2(HeaderRect.X + 5.0f, HeaderRect.Y + 5.0f), Color.Black);

                Rectangle DayRect = new Rectangle(WeekRect.X + i * WeekRect.Width / 6, WeekRect.Y + 50, WeekRect.Width / 6, WeekRect.Height - 50);
                DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, DayRect, Color.Black, 2);

                if (i == 5)
                {
                    spriteBatch.DrawString(Font, "Investigate!", new Vector2(DayRect.X + 5.0f, DayRect.Y + 5.0f), Color.Black);
                }
                else if (EntryArray[i] != null)
                {
                    string EntryString = DrawingUtils.WrappedString(Font, EntryArray[i], DayRect, 0.05f)[0];
                    spriteBatch.DrawString(Font, EntryString, new Vector2(DayRect.X + 5.0f, DayRect.Y + 5.0f), Color.Black);
                }

            }
            DrawingUtils.DrawFadedRectangle(spriteBatch, graphics, TodayRect, Color.Purple, 0.2f);
            DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, TodayRect, Color.Tomato, 4);
        }
    }
}
