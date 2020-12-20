using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDemo.Characters;
using GameDemo.Engine;
using Microsoft.Xna.Framework.Content;

namespace GameDemo.Managers
{
    public interface IManager
    {
        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager contentManager);
        public void Update(GameEngine gameEngine, GameTime gameTime);
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics);
    }
}
