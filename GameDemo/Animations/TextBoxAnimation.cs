using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Animations
{
    public class TextBoxAnimation
    {
        public char[] charArray { get; set; }
        TimeSpan timeIntoAnimation;
        ButtonState previousMouseState;
        public bool fastForward;
        public TimeSpan appearRate { get; set; }

        public TimeSpan Duration
        {
            get
            {
                double totalSeconds = 0;
                for (int i = 0; i < charArray.Length; i++)
                {
                    totalSeconds += getAlteredRate(appearRate, i).TotalSeconds;
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
        }

        private TimeSpan getAlteredRate(TimeSpan rate, int i)
        {
            if (!Char.IsLetterOrDigit(charArray[i]) && !Char.IsLetterOrDigit(charArray[i - 1]))
            {
                return rate + rate;
            }

            else return rate;
        }

        public String currentString
        {
            get
            {
                int currentCharIndex = 0;

                // See if we can find the frame
                TimeSpan accumulatedTime = new TimeSpan();
                for (int i = 0; i < charArray.Length; i++)
                {
                    if (accumulatedTime + getAlteredRate(appearRate, i) >= timeIntoAnimation)
                    {
                        currentCharIndex = i;
                        break;
                    }
                    else
                    {
                        accumulatedTime += getAlteredRate(appearRate, i);
                    }
                }

                // in case timeIntoAnimation exceeds Duration
                // return full string

                if (timeIntoAnimation > this.Duration)
                {
                    currentCharIndex = charArray.Length - 1;
                }

                // If we found a frame, return its rectangle, otherwise
                // return an empty rectangle (one with no width or height)
                if (currentCharIndex != 0)
                {
                    return new string(charArray, 0, currentCharIndex);
                }
                else
                {
                    return "";
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            double secondsIntoAnimation =
                timeIntoAnimation.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed &&
                previousMouseState == ButtonState.Released &&
                secondsIntoAnimation > appearRate.TotalSeconds)
            {
                fastForward = true;
            }

            if (!fastForward)
            {
                timeIntoAnimation = TimeSpan.FromSeconds(secondsIntoAnimation);
            }

            else
            {
                double end = this.Duration.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;
                timeIntoAnimation = TimeSpan.FromSeconds(end);
            }

            previousMouseState = Mouse.GetState().LeftButton;
        }
    }
}
