using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Managers
{
    public interface IManager
    {
        public void Update(GameTime gameTime);
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics);
    }
}
