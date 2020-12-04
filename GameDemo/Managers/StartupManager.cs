using System.Threading; // for loading
using GameDemo.Engine;
using GameDemo.Events;
using GameDemo.Managers;
using GameDemo.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;


namespace GameDemo.Startup
{
    public class StartupManager : IManager {


        private MainCharacter MainCharacter;
        private ContentManager Content;

        /* StartMenu variables */
        private Texture2D StartButton;
        private Texture2D ExitButton;
        private Texture2D LoadingTxt;

        private Vector2 StartButtonPos;
        private Vector2 ExitButtonPos;
        private Vector2 LoadingTxtPos;
        private StartupState GState;
        private MouseState MouseState;
        private MouseState PrevMouseState;
        private bool IsLoading;

        private Thread BackgroundThread;

        enum StartupState
        {
            StartMenu,
            Playing,
            Loading
        }

        /* MouseClick Handler for Start Menu */
        private void MouseClicked(int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 20, 20);

            if (GState == StartupState.StartMenu)
            {
                // Collision doesn't work very well.
                Rectangle startButtonRect = new Rectangle((int)StartButtonPos.X,
                    (int)StartButtonPos.Y, 100, 50);
                Rectangle exitButtonRect = new Rectangle((int)ExitButtonPos.X,
                    (int)ExitButtonPos.Y, 100, 50);

                if (mouseClickRect.Intersects(startButtonRect))
                {
                    GState = StartupState.Loading;
                    IsLoading = false;
                }
                else if (mouseClickRect.Intersects(exitButtonRect))
                {
                    Game1.QuitGame();
                }
            }
        }

        void LoadGame(GameEngine gameEngine)
        {
            // for now this is an eventmanager, in general it may be more like
            // a calendarmanager or a brief intro screen to the calendar.
            gameEngine.Push(new EventManager(), false, false);

            Thread.Sleep(3000);
            GState = StartupState.Playing;
            IsLoading = true;

        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();
            this.MainCharacter = mainCharacter;
            this.Content = content;

            /* Should go in StartMenuManager */
            StartButton = Content.Load<Texture2D>("start");
            ExitButton = Content.Load<Texture2D>("exit");
            LoadingTxt = Content.Load<Texture2D>("loading");
            GState = StartupState.StartMenu;

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (GState == StartupState.Loading && !IsLoading)
            {
                BackgroundThread = new Thread(()=>LoadGame(gameEngine));
                IsLoading = true;

                BackgroundThread.Start();
            }

            MouseState = Mouse.GetState();
            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState.X, MouseState.Y);
            }

            PrevMouseState = MouseState;

            if (GState == StartupState.Playing && IsLoading)
            {
                LoadGame(gameEngine);

                IsLoading = false;
            }
        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Start Button, Exit Button, and Loading Text
            float startX = (graphics.GraphicsDevice.Viewport.Width / 2) - (StartButton.Width / 2);
            float startY = graphics.GraphicsDevice.Viewport.Height / 3;
            float exitX = (graphics.GraphicsDevice.Viewport.Width / 2) - (ExitButton.Width / 2);
            float exitY = 2 * graphics.GraphicsDevice.Viewport.Height / 3;
            float loadingX = (graphics.GraphicsDevice.Viewport.Width / 2) - (LoadingTxt.Width / 2);
            float loadingY = (graphics.GraphicsDevice.Viewport.Height / 2) - (LoadingTxt.Height / 2);

            StartButtonPos = new Vector2(startX, startY);
            ExitButtonPos = new Vector2(exitX, exitY);
            LoadingTxtPos = new Vector2(loadingX, loadingY);

            if (GState == StartupState.StartMenu)
            {
                spriteBatch.Draw(StartButton, StartButtonPos, Color.White);
                spriteBatch.Draw(ExitButton, ExitButtonPos, Color.White);
            }

            if (GState == StartupState.Loading)
            {
                spriteBatch.Draw(LoadingTxt, LoadingTxtPos, Color.YellowGreen);
            }
        }

    }
}