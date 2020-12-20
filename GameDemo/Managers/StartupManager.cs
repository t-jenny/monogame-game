using System;
using System.Threading; // for loading
using GameDemo.Engine;
using GameDemo.Map;
using GameDemo.Locations;
using GameDemo.Managers;
using GameDemo.Characters;
using GameDemo.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;


namespace GameDemo.Startup
{
    public class StartupManager : IManager {

        private MainCharacter MainCharacter;
        private ContentManager Content;
        private SpriteFont Arial;

        private Texture2D StartButtonImg;
        private Texture2D ExitButtonImg;
        private ClickableTexture StartButton;
        private ClickableTexture ExitButton;
        private CountdownTimer CountdownTimer; // Added to test CountdownTimer

        private Texture2D LoadingTxt;
        private Vector2 LoadingTxtPos;

        private StartupState GState;
        private MouseState MouseState;
        private MouseState PrevMouseState;
        private KeyboardInputMenu KeyboardInputMenu;
        private bool IsLoading;

        private Thread BackgroundThread;

        enum StartupState
        {
            StartMenu,
            EnterName,
            Loading,
            Playing
        }

        /* MouseClick Handler for Start Menu */
        private void MouseClicked(int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 10, 10);

            switch(GState) {
                case StartupState.StartMenu:
                    bool ClickedPlay = StartButton.Rect.Contains(mouseClickRect);
                    if (ClickedPlay && MainCharacter.Name == null)
                    {
                        GState = StartupState.EnterName;
                        string Query = "Hi, what's your name?";
                        KeyboardInputMenu = new KeyboardInputMenu(Query, Content, Arial);
                    }
                    else if (ClickedPlay)
                    {
                        GState = StartupState.Loading;
                        IsLoading = false;
                    }
                    else if (ExitButton.Rect.Contains(mouseClickRect))
                    {
                        Game1.QuitGame();
                    }
                    break;

                case StartupState.EnterName:
                    if (KeyboardInputMenu.IsConfirming(mouseClickRect))
                    {
                        GState = StartupState.Loading;
                        MainCharacter.Name = KeyboardInputMenu.GetText();
                    }
                    if (KeyboardInputMenu.IsCancelling(mouseClickRect))
                    {
                        GState = StartupState.StartMenu;
                        KeyboardInputMenu = null;
                    }
                    break;

                default:
                    break;
            }
        }

        void LoadGame(GameEngine gameEngine)
        {
            gameEngine.Push(new CalendarManager(), true, true);

            Thread.Sleep(3000);
            GState = StartupState.Playing;
            IsLoading = true;
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            MainCharacter = mainCharacter;
            Content = content;
            Arial = content.Load<SpriteFont>("Fonts/Arial");

            // Initialize buttons
            StartButtonImg = Content.Load<Texture2D>("start");
            ExitButtonImg = Content.Load<Texture2D>("exit");
            LoadingTxt = Content.Load<Texture2D>("loading");

            float StartX = (Game1.GetWindowSize().X / 2) - (StartButtonImg.Width / 2);
            float StartY = Game1.GetWindowSize().Y / 3;
            float ExitX = (Game1.GetWindowSize().X / 2) - (ExitButtonImg.Width / 2);
            float ExitY = 2 * Game1.GetWindowSize().Y / 3;
            float LoadingX = (Game1.GetWindowSize().X / 2) - (LoadingTxt.Width / 2);
            float LoadingY = (Game1.GetWindowSize().Y / 2) - (LoadingTxt.Height / 2);

            StartButton = new ClickableTexture(StartButtonImg, new Vector2(StartX, StartY));
            ExitButton = new ClickableTexture(ExitButtonImg, new Vector2(ExitX, ExitY));
            LoadingTxtPos = new Vector2(LoadingX, LoadingY);

            // important to reset these components to null when the manager is reloaded
            KeyboardInputMenu = null;

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

            /*** Update Components ***/
            if (GState == StartupState.StartMenu)
            {
                StartButton?.Update();
                ExitButton?.Update();
            }
            KeyboardInputMenu?.Update(gameTime);

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

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {   
            if (GState != StartupState.Loading) {
                StartButton.Draw(spriteBatch, graphics);
                ExitButton.Draw(spriteBatch, graphics);
                if (GState == StartupState.EnterName && KeyboardInputMenu != null)
                {
                    KeyboardInputMenu.Draw(spriteBatch, graphics);
                }
            }
            else
            {
                spriteBatch.Draw(LoadingTxt, LoadingTxtPos, Color.YellowGreen);
            }
        }
    }
}