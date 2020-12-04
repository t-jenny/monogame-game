using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDemo.Characters;
using GameDemo.Engine;
using Microsoft.Xna.Framework.Content;

namespace GameDemo.Managers
{
    public interface IManager
    {
        public abstract void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager contentManager);
        public abstract void Update(GameEngine gameEngine, GameTime gameTime);
        public abstract void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics);
    }
}
