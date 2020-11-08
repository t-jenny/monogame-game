using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Animations
{
    public class Animation
    {
        private ButtonState PreviousMouseState;
        List<AnimationFrame> Frames = new List<AnimationFrame>();

        public bool FastForward;
        public TimeSpan TimeIntoAnimation;
        public bool IsLooping;

        public TimeSpan Duration
        {
            get
            {
                double TotalSeconds = 0;
                foreach (var Frame in Frames)
                {
                    TotalSeconds += Frame.Duration.TotalSeconds;
                }

                return TimeSpan.FromSeconds(TotalSeconds);
            }
        }

        public Rectangle CurrentRectangle
        {
            get
            {
                AnimationFrame CurrentFrame = null;

                // See if we can find the frame
                TimeSpan AccumulatedTime = new TimeSpan();
                foreach (var Frame in Frames)
                {
                    if (AccumulatedTime + Frame.Duration >= TimeIntoAnimation)
                    {
                        CurrentFrame = Frame;
                        break;
                    }
                    else
                    {
                        AccumulatedTime += Frame.Duration;
                    }
                }

                // If no frame was found, then try the last frame, 
                // just in case timeIntoAnimation somehow exceeds Duration
                if (TimeIntoAnimation > this.Duration)
                {
                    CurrentFrame = Frames.FirstOrDefault();
                }

                // If we found a frame, return its rectangle, otherwise
                // return an empty rectangle (one with no width or height)
                if (CurrentFrame != null)
                {
                    return CurrentFrame.SourceRectangle;
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        public void AddFrame(Rectangle rectangle, TimeSpan duration)
        {
            AnimationFrame NewFrame = new AnimationFrame()
            {
                SourceRectangle = rectangle,
                Duration = duration
            };

            Frames.Add(NewFrame);
        }

        public void Update(GameTime gameTime)
        {
            double SecondsIntoAnimation = TimeIntoAnimation.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;

            if (Mouse.GetState().LeftButton == ButtonState.Released && PreviousMouseState == ButtonState.Pressed)
            {
                FastForward = true;
            }

            if (IsLooping)
            {
                if (!FastForward)
                {
                    double Remainder = SecondsIntoAnimation % Duration.TotalSeconds;
                    TimeIntoAnimation = TimeSpan.FromSeconds(Remainder);
                }

                else
                {
                    double End = this.Duration.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;
                    TimeIntoAnimation = TimeSpan.FromSeconds(End);
                }
            }

            else
            {
                TimeIntoAnimation = TimeSpan.FromSeconds(SecondsIntoAnimation);
            }

            PreviousMouseState = Mouse.GetState().LeftButton;
        }
    }
}
