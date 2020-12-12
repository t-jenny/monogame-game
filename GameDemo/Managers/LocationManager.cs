using System;
using System.Collections.Generic;
using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Events;
using GameDemo.Components;
using GameDemo.Managers;
using GameDemo.Notebook;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Locations
{
    public class SpeechMenu : PopupMenu
    {

        public SpeechMenu(string greeting, Rectangle person, ContentManager content, SpriteFont font) : base(content, font)
        {
            StaticText = greeting;
            Menu = content.Load<Texture2D>("speech");
            Position = new Vector2(person.X, person.Y - MenuHeight);

            ConfirmButtonText = "Talk";
            CancelButtonText = "Ignore";
            ButtonLabels.Add(ConfirmButtonText);
            ButtonLabels.Add(CancelButtonText);
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
        private Texture2D MapIcon;
        private Rectangle MapIconRect;

        private SpriteFont Arial;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private LocationState GState;

        private string SelectedPersonName;
        private Dictionary<string, Vector2> CharCoords;
        private Dictionary<string, ClickableTexture> CharPics;
        private Dictionary<string, string> Greetings;
        private Dictionary<string, bool> SpokenWith;
        private bool IsTransitioning;

        private SpeechMenu SpeechMenu;
        private ConfirmMenu ConfirmMenu;

        enum LocationState
        {
            Normal,
            ClickedPerson,
            ConfirmedPerson,
            ToNotebook,
            ClickedReturn,
            ConfirmedReturn
        }

        private void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(10, 10));

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
                            GState = LocationState.ClickedPerson;
                            SpeechMenu = new SpeechMenu(Greetings[CharName], CharRect, Content, Arial);
                            if (SpokenWith[CharName]) SpeechMenu.DisableButton(SpeechMenu.ConfirmButtonText);
                            SelectedPersonName = CharName;
                        }
                    }
                    if (MouseClickRect.Intersects(NotebookRect))
                    {
                        GState = LocationState.ToNotebook;
                    }
                    if (MouseClickRect.Intersects(MapIconRect))
                    {
                        GState = LocationState.ClickedReturn;
                        string query = "Are you sure you're done exploring for now?";
                        ConfirmMenu = new ConfirmMenu(query, Content, Arial);
                    }
                    break;

                case LocationState.ClickedPerson:
                    if (SpeechMenu.IsCancelling(MouseClickRect))
                    {
                        GState = LocationState.Normal;
                        SpeechMenu = null;
                    }
                    else if (SpeechMenu.IsConfirming(MouseClickRect))
                    {
                        GState = LocationState.ConfirmedPerson;
                        SpokenWith[SelectedPersonName] = true;
                        SpeechMenu = null;
                    }
                    break;

                case LocationState.ClickedReturn:
                    if (ConfirmMenu.IsCancelling(MouseClickRect))
                    {
                        GState = LocationState.Normal;
                        ConfirmMenu = null;
                    }
                    else if (ConfirmMenu.IsConfirming(MouseClickRect))
                    {
                        GState = LocationState.ConfirmedReturn;
                        ConfirmMenu = null;
                    }
                    break;

                default:
                    break;
            }
        }

        public LocationManager(string pathName)
        {
            BGImagePath = pathName;
            SpokenWith = new Dictionary<string, bool>(); 
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
            CharPics = new Dictionary<string, ClickableTexture>();
            Greetings = new Dictionary<string, string>();

            Notebook = Content.Load<Texture2D>("notebook_icon");
            MapIcon = Content.Load<Texture2D>("map-icon");
            GState = LocationState.Normal;

            Arial = content.Load<SpriteFont>("Fonts/Arial");
            SpeechMenu = null;

            /***** Replace this with JSON load *****/
            if (BGImagePath == "Jennyland")
            {
                CharCoords.Add("jenny", new Vector2(500, 400));
                Greetings.Add("jenny", "Wassup!");
                if (!SpokenWith.ContainsKey("jenny")) SpokenWith["jenny"] = false;
            }
            if (BGImagePath == "Kaiville")
            {
                CharCoords.Add("kai", new Vector2(140, 415));
                Greetings.Add("kai", "Howdy!");
                if (!SpokenWith.ContainsKey("kai")) SpokenWith["kai"] = false;
            }

            foreach(string CharName in CharCoords.Keys)
            {
                Texture2D CharTexture = Content.Load<Texture2D>("Characters/" + CharName);
                CharPics[CharName] = new ClickableTexture(CharTexture, CharCoords[CharName]);
            }
            /***** End Replace *****/

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            /*** Update components ***/
            SpeechMenu?.Update(gameTime);
            ConfirmMenu?.Update(gameTime);
            foreach (string CharName in CharPics.Keys)
            {
                CharPics[CharName].Update();
            }

            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState);
            }

            PrevMouseState = MouseState;

            switch (GState)
            {
                case (LocationState.ConfirmedPerson):
                    gameEngine.Push(new EventManager(), true, true);
                    IsTransitioning = true;
                    break;

                case (LocationState.ToNotebook):
                    gameEngine.Push(new NotebookManager(), true, true);
                    IsTransitioning = true;
                    break;

                case (LocationState.ConfirmedReturn):
                    gameEngine.Pop(true, true);
                    IsTransitioning = true;
                    break;

                default:
                    break;
            }

        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background
            Background.Draw(spriteBatch, graphics);

            // Banner
            String DateString = MainCharacter.GetDateTimeString() + " - " + BGImagePath;
            DrawingUtils.DrawTextBanner(spriteBatch, graphics, Arial, DateString, Color.Red, Color.Black);

            // Banner Icons
            if (NotebookRect.IsEmpty)
            {
                NotebookRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 100, 20, 70, 70);
            }
            spriteBatch.Draw(Notebook, NotebookRect, Color.White);

            if (MapIconRect.IsEmpty)
            {
                MapIconRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 200, 20, 70, 70);
            }
            spriteBatch.Draw(MapIcon, MapIconRect, Color.White);

            // Draw Characters
            foreach (string CharName in CharCoords.Keys)
            {
                CharPics[CharName].Draw(spriteBatch, graphics);
            }

            // Speech Menu if place is clicked
            if (GState == LocationState.ClickedPerson && SpeechMenu != null)
            {
                SpeechMenu.Draw(spriteBatch, graphics);
            }

            // Confirm Menu if returning to map
            if (GState == LocationState.ClickedReturn && ConfirmMenu != null)
            {
                ConfirmMenu.Draw(spriteBatch, graphics);
            }
        }
    }
}
