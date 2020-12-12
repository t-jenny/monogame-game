using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Components
{
    public class CountdownTimer
    {
        private TimeSpan TimeLeft;
        private readonly SpriteFont Font;
        private Vector2 Position;

        public CountdownTimer(SpriteFont font, int NumMinutes, int NumSeconds, Vector2 Position)
        {
            TimeLeft = new TimeSpan(0, NumMinutes, NumSeconds);
            Font = font;
        }

        public bool IsDone()
        {
            return TimeLeft.Equals(TimeSpan.Zero);
        }

        public void Update(GameTime gameTime)
        {
            if (TimeLeft > gameTime.ElapsedGameTime) TimeLeft -= gameTime.ElapsedGameTime;
            else TimeLeft = TimeSpan.Zero;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string TimeLeftString = TimeLeft.ToString(@"mm\:ss");
            if (TimeLeft.TotalMilliseconds > 10000)
            {
                spriteBatch.DrawString(Font, TimeLeftString, Position, Color.Black);
            }
            else if (!IsDone())
            {
                spriteBatch.DrawString(Font, TimeLeftString, Position, Color.Red);
            }
            else
            {
                spriteBatch.DrawString(Font, "Time's up!!", Position, Color.Black);
            }
        }
    }
}
