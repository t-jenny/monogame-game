using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Animations
{
    public class Animation
    {
        List<AnimationFrame> frames = new List<AnimationFrame>();
        public bool fastForward;
        public TimeSpan timeIntoAnimation;
        private ButtonState previousMouseState;
        public bool isLooping { get; set; }

        public TimeSpan Duration
        {
            get
            {
                double totalSeconds = 0;
                foreach (var frame in frames)
                {
                    totalSeconds += frame.Duration.TotalSeconds;
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
        }

        public Rectangle CurrentRectangle
        {
            get
            {
                AnimationFrame currentFrame = null;

                // See if we can find the frame
                TimeSpan accumulatedTime = new TimeSpan();
                foreach (var frame in frames)
                {
                    if (accumulatedTime + frame.Duration >= timeIntoAnimation)
                    {
                        currentFrame = frame;
                        break;
                    }
                    else
                    {
                        accumulatedTime += frame.Duration;
                    }
                }

                // If no frame was found, then try the last frame, 
                // just in case timeIntoAnimation somehow exceeds Duration
                if (timeIntoAnimation > this.Duration)
                {
                    currentFrame = frames.FirstOrDefault();
                }

                // If we found a frame, return its rectangle, otherwise
                // return an empty rectangle (one with no width or height)
                if (currentFrame != null)
                {
                    return currentFrame.SourceRectangle;
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        public void AddFrame(Rectangle rectangle, TimeSpan duration)
        {
            AnimationFrame newFrame = new AnimationFrame()
            {
                SourceRectangle = rectangle,
                Duration = duration
            };

            frames.Add(newFrame);
        }

        public void Update(GameTime gameTime)
        {
            double secondsIntoAnimation =
                timeIntoAnimation.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed &&
                previousMouseState == ButtonState.Released &&
                secondsIntoAnimation > 0.1)
            {
                fastForward = true;
            }

            if (isLooping)
            {
                if (!fastForward)
                {
                    double remainder = secondsIntoAnimation % Duration.TotalSeconds;
                    timeIntoAnimation = TimeSpan.FromSeconds(remainder);
                }

                else
                {
                    double end = this.Duration.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;
                    timeIntoAnimation = TimeSpan.FromSeconds(end);
                }
            }

            else
            {
                timeIntoAnimation = TimeSpan.FromSeconds(secondsIntoAnimation);
            }

            previousMouseState = Mouse.GetState().LeftButton;
        }
    }
}
