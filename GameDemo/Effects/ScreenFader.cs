using System;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Effects
{
    public class ScreenFader
    {

        private Color CurrentColor;
        private Color InitialColor;
        private Color FinalColor;

        private TimeSpan CurrentTime;
        private TimeSpan EndTime;
        private Texture2D Overlay;

        private bool Fading;

        public ScreenFader()
        {
            // default fade colors to black for now, with
            // no RGB interpolation
            CurrentColor = Color.Transparent;
            InitialColor = Color.Transparent;
            FinalColor = Color.Transparent;
            CurrentTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;
            Fading = false;
        }

        public void BeginFade(Color finalColor, double duration)
        {
            Fading = true;
            InitialColor = CurrentColor;
            FinalColor = finalColor;
            CurrentTime = TimeSpan.Zero;
            EndTime = TimeSpan.FromMilliseconds(duration);
        }

        public bool IsFading()
        {
            return Fading;
        }

        public void Update(GameTime gameTime)
        {
            if (!Fading) return;

            // if we have exceeded duration, stop
            if (CurrentTime >= EndTime)
            {
                Fading = false;
                CurrentColor = FinalColor;
                return;
            }

            float PercentComplete = (float) CurrentTime.TotalMilliseconds / (float) EndTime.TotalMilliseconds;

            CurrentColor = Color.Lerp(InitialColor, FinalColor, PercentComplete);
            TimeSpan TS = gameTime.ElapsedGameTime;
            CurrentTime = CurrentTime.Add(TS);

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (CurrentColor.Equals(Color.Transparent)) return;
            GraphicsDevice GD = graphics.GraphicsDevice;
            DrawingUtils.DrawFilledRectangle(graphics, spriteBatch, GD.Viewport.Bounds, CurrentColor);         
        }
    }
}
