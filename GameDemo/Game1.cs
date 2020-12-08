using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Managers;
using GameDemo.Startup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace GameDemo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager Graphics;
        private SpriteBatch SpriteBatch;
        private MainCharacter MainCharacter;

        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 780;

        private static bool IsQuitting = false;

        private static GameEngine MainEngine = null;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            MainEngine = new GameEngine();

            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {

            Graphics.PreferredBackBufferWidth = DEFAULT_WIDTH;
            Graphics.PreferredBackBufferHeight = DEFAULT_HEIGHT;
            Graphics.ApplyChanges();

            MainCharacter = new MainCharacter();
            IsMouseVisible = true;

            MainEngine.Push(new StartupManager(), false, false);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (IsQuitting) Exit();

            MainEngine.Update(gameTime, MainCharacter, Content);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin();

            MainEngine.Draw(SpriteBatch, Graphics);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public static void QuitGame()
        {
            IsQuitting = true;
        }
    }
}