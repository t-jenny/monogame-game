using System;
using System.IO;
using System.Text.Json;
using GameDemo.Characters;
using GameDemo.Events;
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
        private EventManager EventManager;
        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 780;


        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = DEFAULT_WIDTH;
            Graphics.PreferredBackBufferHeight = DEFAULT_HEIGHT;
            Graphics.ApplyChanges();

            MainCharacter = new MainCharacter();
            String path = Path.Combine(Content.RootDirectory, "json-sample.txt");
            String Text = File.ReadAllText(path);
            AllEventDialogue AllEventDialogue = JsonSerializer.Deserialize<AllEventDialogue>(Text);
            EventDialogue EventDialogue = AllEventDialogue.AllEvents["eventString"][0];

            EventManager = new EventManager(this.MainCharacter, EventDialogue, this.Content);

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

            EventManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();
            EventManager.Draw(SpriteBatch, Graphics);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
