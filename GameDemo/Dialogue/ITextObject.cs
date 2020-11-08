using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Dialogue
{
    public interface ITextObject
    {
        public void Update(GameTime gameTime);
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics);
    }
}
