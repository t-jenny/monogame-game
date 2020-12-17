using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using GameDemo.Engine;
using GameDemo.Map;
using GameDemo.Notebook;
using GameDemo.Locations;
using GameDemo.Characters;
using GameDemo.Components;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using System.Collections.Generic;

namespace GameDemo.Managers
{

    public class CalendarManager : IManager
    {
        private Background Background;
        private MainCharacter MainCharacter;
        private ContentManager Content;
        private SpriteFont JustBreathe;
        private SpriteFont Arial;

        private Button ConfirmButton;
        private OptionsList ActivitiesList;
        private OptionsList PeopleList;
        private ClickableTexture Notebook;
        private Calendar Calendar;
        private InfoTable StatsTable;
        private InfoTable RelTable;

        private Case Case;
        private DateTime ThisMonday;

        private CalendarState GState;
        private MouseState MouseState;
        private MouseState PrevMouseState;
        private bool IsTransitioning;

        enum CalendarState
        {
            ActivityChoice,
            ConfirmActivity,
            NextDay,
            ToWeekend,
            ToNotebook
        }

        /* MouseClick Handler for Start Menu */
        private void MouseClicked(int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 10, 10);

            switch (GState)
            {
                case CalendarState.ConfirmActivity:
                    if (mouseClickRect.Intersects(ConfirmButton.Rect))
                    {
                        // Later on there will be more complicated effects, keeping basic
                        if (!MainCharacter.Stats.ContainsKey(ActivitiesList.SelectedOption))
                        {
                            MainCharacter.Stats[ActivitiesList.SelectedOption] = 1;
                        }
                        else MainCharacter.Stats[ActivitiesList.SelectedOption]++;

                        if (!MainCharacter.Relationships.ContainsKey(PeopleList.SelectedOption))
                        {
                            MainCharacter.Relationships[PeopleList.SelectedOption] = 1;
                        }
                        else MainCharacter.Relationships[PeopleList.SelectedOption]++;

                        Calendar.AddEntry(ActivitiesList.SelectedOption + " with " + PeopleList.SelectedOption);
                        GState = CalendarState.NextDay;

                        MainCharacter.NextDay(); // increment internal timekeeper for maincharacter
                        Calendar.MoveDay();
                    }

                    if (mouseClickRect.Intersects(Notebook.Rect))
                    {
                        GState = CalendarState.ToNotebook;
                    }
                    break;

                case CalendarState.ActivityChoice:
                    if (mouseClickRect.Intersects(Notebook.Rect))
                    {
                        GState = CalendarState.ToNotebook;
                    }
                    break;

                default:
                    break;
            }
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();
            MainCharacter = mainCharacter;
 
            Content = content;
            JustBreathe = Content.Load<SpriteFont>("Fonts/JustBreathe20");
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
            IsTransitioning = false;

            // Load Case Info
            String CasePath = Path.Combine(Content.RootDirectory, "case" + MainCharacter.CurrentCase + ".txt");
            String CaseJSON = File.ReadAllText(CasePath);
            Case = JsonSerializer.Deserialize<Case>(CaseJSON);
            ThisMonday = MainCharacter.GetDate();

            // important to reset these components to null when the manager is reloaded
            ConfirmButton = null;
            ActivitiesList = null;
            PeopleList = null;
            StatsTable = null;
            RelTable = null;
            Calendar = null;
            Notebook = null;
            GState = CalendarState.ActivityChoice;

            Background = new Background(content, "bulletin");

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;

            /*** Update Components ***/
            if (GState == CalendarState.ActivityChoice || GState == CalendarState.ConfirmActivity)
            {
                ConfirmButton?.Update();
                PeopleList?.Update();
                ActivitiesList?.Update();
            }

            MouseState = Mouse.GetState();
            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState.X, MouseState.Y);
            }

            PrevMouseState = MouseState;

            switch(GState)
            {
                case CalendarState.NextDay:
                    if (Calendar.IsMoving())
                    {
                        Calendar.Update(gameTime);
                    }
                    else if (Calendar.DayIndex < 5)
                    {
                        GState = CalendarState.ActivityChoice;
                    }
                    else GState = CalendarState.ToWeekend;
                    // update position of box
                    break;

                case CalendarState.ToWeekend:
                    gameEngine.Push(new MapManager(), true, true);
                    IsTransitioning = true;
                    break;

                case CalendarState.ToNotebook:
                    gameEngine.Push(new NotebookManager(), true, true);
                    IsTransitioning = true;
                    break;

                case CalendarState.ConfirmActivity:
                    if (PeopleList?.SelectedOption == null || ActivitiesList?.SelectedOption == null)
                    {
                        GState = CalendarState.ActivityChoice;
                    }
                    break;

                case CalendarState.ActivityChoice:
                    if (PeopleList?.SelectedOption != null && ActivitiesList?.SelectedOption != null)
                    {
                        GState = CalendarState.ConfirmActivity;
                    }
                    break;

                default:
                    break;
            }

        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Background.Draw(spriteBatch, graphics);

            // Banner
            string WeekString = "Week of " + ThisMonday.ToString("M/d");

            DrawingUtils.DrawTextBanner(spriteBatch, graphics, Arial, WeekString, Color.Red, Color.Black);
            // Initialize Notebook Icon
            if (Notebook == null)
            {
                Notebook = new ClickableTexture(Content.Load<Texture2D>("notebook_icon"),
                    new Vector2(graphics.GraphicsDevice.Viewport.Width - 100, 20));
            }
            Notebook.Draw(spriteBatch, graphics);

            // create calendar
            if (Calendar == null)
            {
                Calendar = new Calendar(new Rectangle(110, 150, 960, 200), JustBreathe, ThisMonday);
            }
            Calendar.Draw(spriteBatch, graphics);

            // Confirm activity to move on to the next day
            Rectangle ActivityRect = new Rectangle(600, 400, 500, 300);
            DrawingUtils.DrawFilledRectangle(spriteBatch, graphics, ActivityRect, Color.Beige);
            DrawingUtils.DrawOpenRectangle(spriteBatch, graphics, ActivityRect, Color.DarkSlateBlue, 3);

            // Formulate Activity - "Today I will [blank] with [blank]"
            spriteBatch.DrawString(JustBreathe, "Today I will ...", new Vector2(ActivityRect.X, ActivityRect.Y), Color.Navy);

            if (ActivitiesList == null)
            {
                string[] Activities = new string[6] { "charm", "courage", "empathy", "intelligence", "strength", "money" };
                ActivitiesList = new OptionsList(Activities, JustBreathe,
                    new Rectangle(ActivityRect.X + 30,
                    ActivityRect.Y + 30,
                    ActivityRect.Width / 6,
                    ActivityRect.Height));
            }
            ActivitiesList.Draw(spriteBatch, graphics);

            spriteBatch.DrawString(JustBreathe, "with", ActivityRect.Center.ToVector2(), Color.Navy);

            if (PeopleList == null)
            {
                string[] People = Case.Suspects.Concat(Case.TestimonyOnly).ToArray();
                PeopleList = new OptionsList(People, JustBreathe,
                    new Rectangle(ActivityRect.X + 4 * ActivityRect.Width / 6,
                    ActivityRect.Y + 30,
                    ActivityRect.Width / 6,
                    ActivityRect.Height));
            }
            PeopleList.Draw(spriteBatch, graphics);

            // Add table for current Main Character stats
            if (StatsTable == null)
            {
                string[] Aspects = new string[6] { "charm", "courage", "empathy", "intelligence", "strength", "money" };
                StatsTable = new InfoTable(MainCharacter.Stats, Aspects, new Rectangle(110, 400, 200, 252), JustBreathe);
                string[] People = Case.Suspects.Concat(Case.TestimonyOnly).ToArray();
                RelTable = new InfoTable(MainCharacter.Relationships, People, new Rectangle(350, 400, 200, 252), JustBreathe);
            }
            StatsTable.Draw(spriteBatch, graphics);
            RelTable.Draw(spriteBatch, graphics);

            if (GState == CalendarState.ConfirmActivity)
            {
                if (ConfirmButton == null)
                {
                    ConfirmButton = new Button("Go!", JustBreathe,
                        new Vector2(ActivityRect.X + ActivityRect.Width / 2, ActivityRect.Y + ActivityRect.Height - 50));
                }
                ConfirmButton.Draw(spriteBatch, graphics);
            }
        }

    }
}