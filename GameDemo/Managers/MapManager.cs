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

namespace GameDemo.Map
{
    public class LocationMenu
    {
        private const int MenuWidth = 450;
        private const int MenuHeight = 250;
        private readonly string Name;
        private readonly string Info;
        private Vector2 Position;
        private Texture2D Menu;

        private Button ExploreButton;
        private Button ExitButton;

        public LocationMenu(string name, string info, ContentManager content)
        {
            Name = name;
            Info = info;

            Position = new Vector2(300, 300);

            Menu = content.Load<Texture2D>("parchment");
        }

        // Name of the location
        public string LocationName()
        {
            return Name;
        }

        // Update the button on the location menu
        public void Update()
        {
            if (ExploreButton == null) return;
            ExitButton.Update();
            ExploreButton.Update();
        }

        public bool IsExiting(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(ExitButton.Rect);
        }

        public bool IsConfirming(Rectangle mouseClickRect)
        {
            return mouseClickRect.Intersects(ExploreButton.Rect);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            // Location Menu
            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y - 50, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);
            spriteBatch.DrawString(font, Name, new Vector2(Position.X + 40, Position.Y), Color.Black);
            spriteBatch.DrawString(font, Info, new Vector2(Position.X + 40, Position.Y + 50), Color.Black);

            if (ExitButton == null)
            {
                ExitButton = new Button("x", font,
                    (int)Position.X + MenuWidth - 50,
                    (int)Position.Y - 50);
            }
            ExitButton.Draw(spriteBatch, graphics);

            // Explore Button
            if (ExploreButton == null)
            {
                ExploreButton = new Button("Explore", font,
                    (int)Position.X + MenuRect.Width / 2,
                    (int)Position.Y + MenuRect.Height / 2);
            }
            ExploreButton.Draw(spriteBatch, graphics);
        }

    }

    public class MapManager : IManager
    {
        private MainCharacter MainCharacter;
        private ContentManager Content;

        private string SelectedPlaceName;
        private const string MapPath = "fantasy-map";
        private Background Background;
        private Texture2D Notebook;
        private Rectangle NotebookRect;

        private SpriteFont Arial;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private MapState GState;

        private Dictionary<String, Rectangle> LocationBoxes;
        private Dictionary<String, String> LocationInfo;
        private LocationMenu LocationMenu;
        private bool IsTransitioning;

        enum MapState
        {
            Normal,
            Selected,
            Confirmed,
            ToNotebook
        }

        public void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(50, 50));

            switch (GState)
            {
                // If nothing selected, check whether location was selected
                case MapState.Normal:
                    foreach (String PlaceName in LocationBoxes.Keys)
                    {
                        if (MouseClickRect.Intersects(LocationBoxes[PlaceName]))
                        {
                            GState = MapState.Selected;
                            LocationMenu = new LocationMenu(PlaceName + ":", LocationInfo[PlaceName], Content);
                            SelectedPlaceName = PlaceName;
                        }
                    }
                    if (MouseClickRect.Intersects(NotebookRect))
                    {
                        GState = MapState.ToNotebook;
                    }
                    break;

                case MapState.Selected:
                    if (LocationMenu.IsExiting(MouseClickRect))
                    {
                        GState = MapState.Normal;
                        LocationMenu = null;
                    }
                    else if (LocationMenu.IsConfirming(MouseClickRect))
                    {
                        GState = MapState.Confirmed;
                        LocationMenu = null;
                    }
                    break;
            }
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();
            mainCharacter.NextDay(); // Increment the day next time returned to menu.

            MainCharacter = mainCharacter;
            Content = content;
            IsTransitioning = false;

            // Visual Elements
            Background = new Background(content, MapPath);
            LocationBoxes = new Dictionary<String, Rectangle>();
            LocationInfo = new Dictionary<String, String>();
            Notebook = Content.Load<Texture2D>("notebook_icon");
            GState = MapState.Normal;

            Arial = content.Load<SpriteFont>("Fonts/Arial");

            Dictionary<String, Vector2> Locations = new Dictionary<String, Vector2>();

            // would probabily read in from json
            Locations.Add("Kaiville", new Vector2(800, 200));
            Locations.Add("Jennyland", new Vector2(400, 300));
            LocationInfo.Add("Kaiville", "A happy place");
            LocationInfo.Add("Jennyland", "Lots of cool cats");

            // need to construct list of locations based on main character stat
            // use json with file extension and coordinates of rectangle
            foreach (String Name in Locations.Keys)
            {
                Vector2 TextSize = Arial.MeasureString(Name);
                Rectangle LocBox = new Rectangle((int)Locations[Name].X,
                    (int)Locations[Name].Y,
                    (int)TextSize.X,
                    (int)TextSize.Y);
                LocationBoxes.Add(Name, LocBox);
            }

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            if (LocationMenu != null) LocationMenu.Update();

            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState);
            }

            PrevMouseState = MouseState;

            if (GState == MapState.Confirmed)
            {
                gameEngine.Push(new LocationManager(SelectedPlaceName), true, true);
                IsTransitioning = true;
            }

            if (GState == MapState.ToNotebook)
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
            String DateString = CurrentDate.ToString("dddd, MMMM dd") + " - Carpe Diem!";
            spriteBatch.DrawString(Arial, DateString, new Vector2(10.0f, 30.0f), Color.Black);

            DrawingUtils.DrawTextBanner(graphics, spriteBatch, Arial, DateString, Color.Red, Color.Black);

            // Place Labels
            foreach (String PlaceName in LocationBoxes.Keys)
            {
                // replace with a box sprite
                DrawingUtils.DrawFilledRectangle(graphics, spriteBatch, LocationBoxes[PlaceName], Color.Brown);
                Vector2 LabelVec = new Vector2(LocationBoxes[PlaceName].X, LocationBoxes[PlaceName].Y);
                spriteBatch.DrawString(Arial, PlaceName, LabelVec, Color.White);
            }

            // Notebook
            if (NotebookRect.IsEmpty)
            {
                NotebookRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 100,
                    graphics.GraphicsDevice.Viewport.Height - 100, 70, 70);
            }
            spriteBatch.Draw(Notebook, NotebookRect, Color.White);

            // Location Info Menu if place is clicked
            if (GState == MapState.Selected && LocationMenu != null) {
                LocationMenu.Draw(spriteBatch, Arial, graphics);
            }
        }
    }
}
