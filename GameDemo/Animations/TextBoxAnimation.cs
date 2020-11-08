using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Animations
{
    public class TextBoxAnimation
    {
        TimeSpan TimeIntoAnimation;
        ButtonState PreviousMouseState;

        public bool FastForward { get; private set; }
        public bool TextComplete { get; private set; }

        public char[] CharArray { get; set; }
        public TimeSpan AppearRate { get; set; }

        public TimeSpan Duration
        {
            get
            {
                double TotalSeconds = 0;
                for (int i = 0; i < CharArray.Length; i++)
                {
                    TotalSeconds += GetAlteredRate(AppearRate, i).TotalSeconds;
                }

                return TimeSpan.FromSeconds(TotalSeconds);
            }
        }

        private TimeSpan GetAlteredRate(TimeSpan rate, int i)
        {
            if (!Char.IsLetterOrDigit(CharArray[i]) && !Char.IsLetterOrDigit(CharArray[i - 1]))
            {
                return rate + rate;
            }

            else return rate;
        }

        public String CurrentString
        {
            get
            {
                int CurrentCharIndex = 0;

                // See if we can find the frame
                TimeSpan AccumulatedTime = new TimeSpan();
                for (int i = 0; i < CharArray.Length; i++)
                {
                    if (AccumulatedTime + GetAlteredRate(AppearRate, i) >= TimeIntoAnimation)
                    {
                        CurrentCharIndex = i;
                        break;
                    }
                    else
                    {
                        AccumulatedTime += GetAlteredRate(AppearRate, i);
                    }
                }

                // in case timeIntoAnimation exceeds Duration
                // return full string

                if (TimeIntoAnimation > this.Duration)
                {
                    TextComplete = true;
                    CurrentCharIndex = CharArray.Length;
                }

                // If we found a frame, return its rectangle, otherwise
                // return an empty rectangle (one with no width or height)
                if (CurrentCharIndex != 0)
                {
                    return new string(CharArray, 0, CurrentCharIndex);
                }
                else
                {
                    return "";
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            double SecondsIntoAnimation =
                TimeIntoAnimation.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;

            if (Mouse.GetState().LeftButton == ButtonState.Released && PreviousMouseState == ButtonState.Pressed)
            {
                FastForward = true;
            }

            if (!FastForward)
            {
                TimeIntoAnimation = TimeSpan.FromSeconds(SecondsIntoAnimation);
            }

            else
            {
                double End = this.Duration.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;
                TimeIntoAnimation = TimeSpan.FromSeconds(End);
            }

            PreviousMouseState = Mouse.GetState().LeftButton;
        }
    }
}
