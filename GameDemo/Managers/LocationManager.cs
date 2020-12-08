using System;
using System.Collections.Generic;
using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Events;
using GameDemo.Locations;
using GameDemo.Managers;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Locations
{
    public class SpeechMenu
    {
        private const int MenuWidth = 450;
        private const int MenuHeight = 250;
        private readonly string Greeting;
        private Vector2 Position;
        private Texture2D Menu;

        private Button TalkButton;
        private Button IgnoreButton;

        public SpeechMenu(string greeting, Rectangle person, ContentManager content)
        {
            Greeting = greeting;
            Menu = content.Load<Texture2D>("speech");
            
            Position = new Vector2(person.X, person.Y - MenuHeight);
        }

        // Update the button on the location menu
        public void Update()
        {
            if (TalkButton == null) return;
            TalkButton.Update();
            IgnoreButton.Update();
        }

        public bool IsExiting(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(IgnoreButton.Rect);
        }

        public bool IsConfirming(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(TalkButton.Rect);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            // Location Menu
            Vector2 GreetingSize = font.MeasureString(Greeting);

            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);
            spriteBatch.DrawString(font, Greeting, new Vector2(Position.X + MenuWidth / 2 - GreetingSize.X / 2, Position.Y + 20), Color.Black);

            // draw border (for testing)
            //DrawingUtils.DrawOpenRectangle(graphics, spriteBatch, MenuRect, Color.Black);

            // Explore Button
            if (TalkButton == null)
            {
                TalkButton = new Button("Talk", font,
                    (int)Position.X + MenuWidth / 3,
                    (int)Position.Y + MenuHeight / 2);
            }
            TalkButton.Draw(spriteBatch, graphics);

            if (IgnoreButton == null)
            {
                IgnoreButton = new Button("Ignore", font,
                    (int)Position.X + 2 * MenuWidth / 3,
                    (int)Position.Y + MenuHeight / 2);
            }
            IgnoreButton.Draw(spriteBatch, graphics);
        }

    }

    public class LocationManager : IManager
    {
        private MainCharacter MainCharacter;
        private ContentManager Content;

        private string BGImagePath;
        private Background Background;
        private Texture2D Notebook;
        private Rectangle NotebookRect;

        private SpriteFont Arial;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private LocationState GState;

        private string SelectedPersonName;
        private Dictionary<string, Vector2> CharCoords;
        private Dictionary<string, Texture2D> CharPics;
        private Dictionary<string, string> Greetings;
        private bool IsTransitioning;

        private SpeechMenu SpeechMenu;

        enum LocationState
        {
            Normal,
            Selected,
            Confirmed,
            ToNotebook
        }

        private void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(50, 50));

            switch (GState)
            {
                // If nothing selected, check whether location was selected
                case LocationState.Normal:
                    foreach (string CharName in CharPics.Keys)
                    {
                        Rectangle CharRect = new Rectangle((int)CharCoords[CharName].X,
                            (int)CharCoords[CharName].Y, CharPics[CharName].Width, CharPics[CharName].Height);
                        if (MouseClickRect.Intersects(CharRect))
                        {
                            GState = LocationState.Selected;
                            SpeechMenu = new SpeechMenu(Greetings[CharName], CharRect, Content);
                            SelectedPersonName = CharName;
                        }
                    }
                    if (MouseClickRect.Intersects(NotebookRect))
                    {
                        GState = LocationState.ToNotebook;
                    }
                    break;

                case LocationState.Selected:
                    if (SpeechMenu.IsExiting(MouseClickRect))
                    {
                        GState = LocationState.Normal;
                        SpeechMenu = null;
                    }
                    else if (SpeechMenu.IsConfirming(MouseClickRect))
                    {
                        GState = LocationState.Confirmed;
                        SpeechMenu = null;
                    }
                    break;
            }
        }

        public LocationManager(string pathName)
        {
            BGImagePath = pathName;
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            MainCharacter = mainCharacter;
            Content = content;
            IsTransitioning = false;

            // Visual Elements
            Background = new Background(content, BGImagePath);
            CharCoords = new Dictionary<string, Vector2>();
            CharPics = new Dictionary<string, Texture2D>();
            Greetings = new Dictionary<string, string>();

            Notebook = Content.Load<Texture2D>("notebook_icon");
            GState = LocationState.Normal;

            Arial = content.Load<SpriteFont>("Fonts/Arial");
            SpeechMenu = null;

            /***** Replace this with JSON load *****/
            if (BGImagePath == "Jennyland")
            {
                CharCoords.Add("jenny", new Vector2(500, 400));
                Greetings.Add("jenny", "Wassup!");
            }
            if (BGImagePath == "Kaiville")
            {
                CharCoords.Add("kai", new Vector2(140, 415));
                Greetings.Add("kai", "Howdy!");
            }

            foreach(string CharName in CharCoords.Keys)
            {
                CharPics.Add(CharName, Content.Load<Texture2D>("Characters/" + CharName));
            }
            /***** End Replace *****/

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            if (SpeechMenu != null) SpeechMenu.Update();

            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState);
            }

            PrevMouseState = MouseState;

            if (GState == LocationState.Confirmed)
            {
                gameEngine.Push(new EventManager(), true, true);
                IsTransitioning = true;
            }

            if (GState == LocationState.ToNotebook)
            {
                gameEngine.Pop(true, true);
                IsTransitioning = true;
            }

            
        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background
            Background.Draw(spriteBatch, graphics);

            // Banner with Date
            DateTime CurrentDate = MainCharacter.GetDate();
            String DateString = CurrentDate.ToString("dddd, MMMM dd") + " - " + BGImagePath;
            DrawingUtils.DrawTextBanner(graphics, spriteBatch, Arial, DateString, Color.Red, Color.Black);

            foreach (String CharName in CharCoords.Keys)
            {
                // replace with a box sprite
                spriteBatch.Draw(CharPics[CharName], CharCoords[CharName], Color.White);
            }

            // Notebook
            if (NotebookRect.IsEmpty)
            {
                NotebookRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 100,
                    graphics.GraphicsDevice.Viewport.Height - 100, 70, 70);
            }
            spriteBatch.Draw(Notebook, NotebookRect, Color.White);

            // Location Info Menu if place is clicked
            if (GState == LocationState.Selected && SpeechMenu != null)
            {
                SpeechMenu.Draw(spriteBatch, Arial, graphics);
            }
        }
    }
}
