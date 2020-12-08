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

        private Dictionary<String, Rectangle> ClickableBoxes;
        private Dictionary<String, String> ClickableInfo;
        private bool IsTransitioning;

        private Button EventButton;

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
                    if (MouseClickRect.Intersects(NotebookRect))
                    {
                        GState = LocationState.ToNotebook;
                    }
                    if (MouseClickRect.Intersects(EventButton.Rect))
                    {
                        GState = LocationState.Confirmed;
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
            //ClickableBoxes = new Dictionary<String, Rectangle>();
            //ClickableInfo = new Dictionary<String, String>();
            Notebook = Content.Load<Texture2D>("notebook_icon");
            GState = LocationState.Normal;

            Arial = content.Load<SpriteFont>("Fonts/Arial");
            EventButton = null;

            //Dictionary<String, Vector2> Locations = new Dictionary<String, Vector2>();

            // would probabily read in from json
            //Locations.Add("Kaiville", new Vector2(800, 200));
            //Locations.Add("Jennyland", new Vector2(400, 300));
            //LocationInfo.Add("Kaiville", "A happy place");
            //LocationInfo.Add("Jennyland", "Lots of cool cats");

            // need to construct list of locations based on main character stat
            // use json with file extension and coordinates of rectangle
            //foreach (String Name in Locations.Keys)
            //{
            //    Vector2 TextSize = Arial.MeasureString(Name);
            //    Rectangle LocBox = new Rectangle((int)Locations[Name].X,
            //        (int)Locations[Name].Y,
            //        (int)TextSize.X,
            //        (int)TextSize.Y);
            //    LocationBoxes.Add(Name, LocBox);
            //}

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            if (EventButton != null) EventButton.Update();

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

            // Banner
            int WindowWidth = (int)graphics.GraphicsDevice.Viewport.Width;
            int WindowHeight = (int)graphics.GraphicsDevice.Viewport.Height;
            Texture2D Banner = DrawingUtils.FilledRectangle(graphics, WindowWidth, 100, Color.Red);
            spriteBatch.Draw(Banner, new Vector2(0.0f, 0.0f), Color.White);

            // Current Date
            DateTime CurrentDate = MainCharacter.GetDate();
            String DateString = CurrentDate.ToString("dddd, MMMM dd") + " - Carpe Diem!";
            spriteBatch.DrawString(Arial, DateString, new Vector2(10.0f, 30.0f), Color.Black);

            // Event trigger (for now)
            if (EventButton == null)
            {
                EventButton = new Button("Click for Event", Arial, 300, 300);
            }
            EventButton.Draw(spriteBatch, graphics);

            // Notebook
            if (NotebookRect.IsEmpty)
            {
                NotebookRect = new Rectangle(WindowWidth - 100, WindowHeight - 100, 70, 70);
            }
            spriteBatch.Draw(Notebook, NotebookRect, Color.White);
        }
    }
}
