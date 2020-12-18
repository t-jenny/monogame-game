using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Components;
using GameDemo.Managers;
using GameDemo.Testimonies;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Notebook
{

    public class NotebookManager : IManager
    {
        private MainCharacter MainCharacter;
        private ContentManager Content;

        private const string NotebookPath = "notebook";
        private Background Background;

        private Texture2D ReturnIcon;
        private Rectangle ReturnIconRect;

        // Notebook tabs/sections
        private ClickableTexture PeopleTab;
        private ClickableTexture OptionsTab;
        private ClickableTexture StatsTab;
        private ClickableTexture TestimonyTab;

        // Selection Lists
        private AllCharacters AllChars;
        private OptionsList MainOptionsList;
        private TestimonyList TestimonyList;
        private OptionsList TopicOptionsList;

        private Button QuitButton;
        private Button SaveButton;
        private ConfirmMenu ConfirmQuitMenu;

        private Button SelectTestimonyButton;
        private ConfirmMenu ConfirmContradictMenu;

        //Make this part of notebook page class;
        //private Button NextPageButton;
        //private Button PrevPageButton;
        private int PageIndex = 0;

        // Text related variables
        private SpriteFont Arial;
        private SpriteFont JustBreathe;
        private SpriteFont JustBreathe25;
        private Vector2 TextOffset;
        private Vector2 Indent;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private NotebookState GState;
        private bool IsTransitioning;
        private bool SeekingTestimony;

        enum NotebookState
        {
            Returning,
            Stats,
            Testimonies,
            Profiles,
            Options,
            ClickedQuitGame,
            ConfirmedQuitGame
        }

        public NotebookManager(bool seekingTestimony)
        {
            SeekingTestimony = seekingTestimony;
        }

        // from Microsoft Docs
        private void SaveGame()
        {
            string path = Path.Combine(Content.RootDirectory, "savedata.txt");
            string json = JsonSerializer.Serialize<MainCharacter>(MainCharacter);
            File.WriteAllText(path, json);
        }

        private Dictionary<string, string> GetOptions()
        {
            switch (GState)
            {
                case NotebookState.Stats:
                    return new Dictionary<string, string> {
                        { "My Stats", "stats" },
                        { "Relationships", "relationships" }
                    };

                case NotebookState.Options:
                    return new Dictionary<string, string> {
                        { "Settings", "settings" },
                        { "Save & Quit", "savequit" }
                    };

                default:
                    // pull from people known to main character (should be a list not this dict)
                    Dictionary<string, string> Names = new Dictionary<string, string>();
                    foreach (string Name in MainCharacter.Relationships.Keys)
                    {
                        if (AllChars.AllChars.ContainsKey(Name))
                        {
                            Names[AllChars.AllChars[Name].Name] = Name;
                        }
                    }
                    return Names;

            }
        }

        private void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(10, 10));

            switch (GState)
            {
                case NotebookState.ClickedQuitGame:
                    if (ConfirmQuitMenu.IsConfirming(MouseClickRect))
                    {
                        GState = NotebookState.ConfirmedQuitGame;
                    }
                    if (ConfirmQuitMenu.IsCancelling(MouseClickRect))
                    {
                        GState = NotebookState.Options; // replace with whichever state exposes the quit button
                    }
                    break;

                default:
                    if (MouseClickRect.Intersects(StatsTab.Rect))
                    {
                        GState = NotebookState.Stats;
                        MainOptionsList = null;
                        TopicOptionsList = null;
                    }
                    if (MouseClickRect.Intersects(PeopleTab.Rect))
                    {
                        GState = NotebookState.Profiles;
                        MainOptionsList = null;
                        TopicOptionsList = null;
                    }
                    if (MouseClickRect.Intersects(OptionsTab.Rect))
                    {
                        GState = NotebookState.Options;
                        MainOptionsList = null;
                        TopicOptionsList = null;
                    }
                    if (MouseClickRect.Intersects(TestimonyTab.Rect))
                    {
                        GState = NotebookState.Testimonies;
                        MainOptionsList = null;
                        TopicOptionsList = null;
                    }
                    if (MouseClickRect.Intersects(ReturnIconRect))
                    {
                        GState = NotebookState.Returning;
                    }

                    if (MainOptionsList?.SelectedOption == "savequit")
                    {
                        if (QuitButton != null && MouseClickRect.Intersects(QuitButton.Rect))
                        {
                            GState = NotebookState.ClickedQuitGame;
                            string Query = "Are you sure you want to quit the game?";
                            ConfirmQuitMenu = new ConfirmMenu(Query, Content, Arial);
                        }
                        if (SaveButton != null && MouseClickRect.Intersects(SaveButton.Rect))
                        {
                            SaveGame();
                        }
                    }
                    break;
            }
        }


        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            //content.Unload();

            MainCharacter = mainCharacter;
            Content = content;

            // Load Characters
            String path = Path.Combine(Content.RootDirectory, "characters.txt");
            String CharJSON = File.ReadAllText(path);
            AllChars = JsonSerializer.Deserialize<AllCharacters>(CharJSON);

            // Load Testimonies
            path = Path.Combine(Content.RootDirectory, "testimonies.txt");
            String TestimonyJSON = File.ReadAllText(path);
            TestimonyList = JsonSerializer.Deserialize<TestimonyList>(TestimonyJSON);

            // Visual Elements
            Background = new Background(content, NotebookPath);
            ReturnIcon = Content.Load<Texture2D>("return-icon");
            QuitButton = null;
            SaveButton = null;

            // Always start by viewing stats.
            GState = NotebookState.Stats;
            MainOptionsList = null;
            TopicOptionsList = null;

            Arial = content.Load<SpriteFont>("Fonts/Arial");
            JustBreathe = content.Load<SpriteFont>("Fonts/JustBreathe20");
            JustBreathe25 = content.Load<SpriteFont>("Fonts/JustBreathe25");
            TextOffset = new Vector2(0.0f, JustBreathe.MeasureString("A").Y + 0.5f);
            Indent = new Vector2(Arial.MeasureString("A").X, 0.0f);

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            /*** Update Components ***/
            // Notebook sections
            PeopleTab?.Update();
            OptionsTab?.Update();
            StatsTab?.Update();
            TestimonyTab?.Update();

            MainOptionsList?.Update();
            TopicOptionsList?.Update();

            // Game Quit Components
            QuitButton?.Update();
            SaveButton?.Update();
            ConfirmQuitMenu?.Update(gameTime);

            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState);
            }

            PrevMouseState = MouseState;


            if (GState == NotebookState.Returning)
            {
                gameEngine.Pop(true, true);
                IsTransitioning = true;
            }

            if (GState == NotebookState.ConfirmedQuitGame)
            {
                gameEngine.PopAll(true, true);
                IsTransitioning = true;
            }

        }

        private void DrawCharacterEntry(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Character character, Vector2 textPos)
        {
            if (MainOptionsList?.SelectedOption == null) return;

            Texture2D CharPic = Content.Load<Texture2D>(character.ImagePath);
            Rectangle PicRect = new Rectangle((int)textPos.X, (int)textPos.Y, 100, 100);
            spriteBatch.Draw(CharPic, PicRect, Color.White);

            spriteBatch.DrawString(JustBreathe, character.Name, textPos += new Vector2(PicRect.Width + 5.0f, 0.0f), Color.Black);
            spriteBatch.DrawString(JustBreathe, "Age: " + character.Age, textPos += TextOffset, Color.Black);
            spriteBatch.DrawString(JustBreathe, "Personality: " + character.Personality, textPos += TextOffset, Color.Black);

            spriteBatch.DrawString(JustBreathe, "Occupation: " + character.Occupation,
                textPos += 2.5f * TextOffset - new Vector2(PicRect.Width + 5.0f, 0.0f), Color.Black);
            // Character Description:
            spriteBatch.DrawString(JustBreathe, "Description: ", textPos += 2.0f * TextOffset, Color.Black);
            Point DescBoxSize = new Point(400 - (int)Indent.X, 300);
            Rectangle DescBoxRect = new Rectangle(((textPos += TextOffset) + Indent).ToPoint(), DescBoxSize);
            string Description = DrawingUtils.WrappedString(JustBreathe, character.Description, DescBoxRect, 0.1f)[0];
            spriteBatch.DrawString(JustBreathe, Description, textPos + Indent, Color.Black);

            // Best Friends:
            spriteBatch.DrawString(JustBreathe, "Best Friends: ", textPos += 6 * TextOffset, Color.Black);
            foreach (string FriendName in character.BFFs)
            {
                spriteBatch.DrawString(JustBreathe, "- " + FriendName, (textPos += TextOffset) + Indent, Color.Black);
            }
        }

        private void DrawTestimonies(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, List<Testimony> testimonies, Vector2 textPos)
        {
            string Results = "";
            foreach (Testimony Testimony in testimonies)
            {
                Results += "Name: " + AllChars.AllChars[Testimony.CharacterKey].Name + "; ";
                Results += "Re: " + Testimony.TopicTag + " \n ";
                Results += Testimony.Text + " \n \n ";
            }
            Rectangle TestimonyRect = new Rectangle((int)(textPos + TextOffset).X, (int)(textPos + TextOffset).Y, 400, 600);

            List<string> ResultsPages = DrawingUtils.WrappedString(JustBreathe, Results, TestimonyRect, 0.1f);
            spriteBatch.DrawString(JustBreathe, ResultsPages[PageIndex], textPos, Color.Black);
        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background

            Background.Draw(spriteBatch, graphics);
            int ScreenWidth = graphics.GraphicsDevice.Viewport.Width;
            int ScreenHeight = graphics.GraphicsDevice.Viewport.Height;

            // Banner with Date
            string DateString = MainCharacter.GetDateTimeString();
            DrawingUtils.DrawTextBanner(spriteBatch, graphics, Arial, DateString, Color.Red, Color.Black);

            // Return-to-world icon
            if (ReturnIconRect.IsEmpty)
            {
                ReturnIconRect = new Rectangle(ScreenWidth - 100, 20, 70, 70);
            }
            spriteBatch.Draw(ReturnIcon, ReturnIconRect, Color.White);

            // Draw MainMainOptionsList (Lefthand Page)
            if (MainOptionsList == null)
            {
                Rectangle MainOptionsListRect = new Rectangle(150, 120 + 2 * JustBreathe25.LineSpacing, 300, 500);
                MainOptionsList = new OptionsList(GetOptions(), JustBreathe, MainOptionsListRect);
            }
            MainOptionsList.Draw(spriteBatch, graphics);

            /***** Draw notebook info (make this more modular *****/
            Vector2 TextPos = new Vector2(1.1f * ScreenWidth / 2, 120);

            TextPos += TextOffset;

            // Notebook Page Display
            switch (GState)
            {
                case NotebookState.Stats:
                    // Lefthand Page
                    spriteBatch.DrawString(JustBreathe25, "Stats:", new Vector2(150, 120), Color.Black);

                    // Righthand Page
                    if (MainOptionsList?.SelectedOption == "stats")
                    {
                        spriteBatch.DrawString(JustBreathe, "My Stats: ", TextPos, Color.Black);
                        foreach (string Stat in MainCharacter.Stats.Keys)
                        {
                            TextPos += TextOffset;
                            string StatString = Stat + ": " + MainCharacter.Stats[Stat];
                            spriteBatch.DrawString(JustBreathe, StatString, TextPos, Color.Black);
                        }
                    }
                    else if (MainOptionsList?.SelectedOption == "relationships")
                    {
                        spriteBatch.DrawString(JustBreathe, "Friendship Levels: ", TextPos, Color.Black);
                        List<string> RelationshipList = MainCharacter.Relationships.Keys.ToList();
                        RelationshipList.Sort();

                        foreach (string CharName in RelationshipList)
                        {
                            TextPos += TextOffset;
                            string RelString = CharName + ": " + MainCharacter.Relationships[CharName];
                            spriteBatch.DrawString(JustBreathe, RelString, TextPos, Color.Black);
                        }
                    }
                    break;

                case NotebookState.ClickedQuitGame:
                case NotebookState.Options:
                    // Lefthand Page
                    spriteBatch.DrawString(JustBreathe25, "Options:", new Vector2(150, 120), Color.Black);

                    // Righthand Page
                    if (MainOptionsList?.SelectedOption == "savequit")
                    {
                        if (QuitButton == null)
                        {
                            SaveButton = new Button("Save Game", Arial, TextPos);
                            QuitButton = new Button("Quit Game", Arial, TextPos + new Vector2(0.0f, 2 * SaveButton.Rect.Height));
                        }
                        QuitButton.Draw(spriteBatch, graphics);
                        SaveButton.Draw(spriteBatch, graphics);
                    }
                    break;

                case NotebookState.Profiles:
                    // Lefthand Page
                    spriteBatch.DrawString(JustBreathe25, "People:", new Vector2(150, 120), Color.Black);

                    if (MainOptionsList?.SelectedOption != null)
                    {
                        DrawCharacterEntry(spriteBatch, graphics, AllChars.AllChars[MainOptionsList.SelectedOption], TextPos);
                    }
                    break;

                case NotebookState.Testimonies:
                    // Add topics list to lefthand page
                    spriteBatch.DrawString(JustBreathe25, "Testimony:", new Vector2(150, 120), Color.Black);
                    spriteBatch.DrawString(JustBreathe25, "Person", new Vector2(150, 120 + JustBreathe25.LineSpacing), Color.Black);
                    spriteBatch.DrawString(JustBreathe25, "Topic", new Vector2(375, 120 + JustBreathe25.LineSpacing), Color.Black);
                    if (TopicOptionsList == null)
                    {
                        Rectangle TopicOptionsListRect = new Rectangle(375, 120 + 2 * JustBreathe25.LineSpacing, 300, 500);
                        TopicOptionsList = new OptionsList(TestimonyList.Topics, JustBreathe, TopicOptionsListRect);
                    }
                    TopicOptionsList.Draw(spriteBatch, graphics);


                    // must select a character and a topic to view testimony
                    if (TopicOptionsList?.SelectedOption != null && MainOptionsList?.SelectedOption != null)
                    {
                        List<Testimony> Testimonies = (from testimony in TestimonyList.Testimonies
                                                       where testimony.TopicTag == TopicOptionsList?.SelectedOption &&
                                                       testimony.CharacterKey == MainOptionsList?.SelectedOption
                                                       select testimony).ToList();
                        DrawTestimonies(spriteBatch, graphics, Testimonies, TextPos);
                    }
                    break;

                default:
                    break;

            }
            /***** End Notebook Page placeholder *****/

            // Draw Notebook Tabs
            if (PeopleTab == null)
            {
                PeopleTab = new ClickableTexture(Content.Load<Texture2D>("tab_people"),
                    new Vector2(120, 120));
                StatsTab = new ClickableTexture(Content.Load<Texture2D>("tab_stats"),
                    new Vector2(120, PeopleTab.Rect.Y + PeopleTab.Rect.Height));
                TestimonyTab = new ClickableTexture(Content.Load<Texture2D>("tab_testimony"),
                    new Vector2(120, StatsTab.Rect.Y + StatsTab.Rect.Height));
                OptionsTab = new ClickableTexture(Content.Load<Texture2D>("tab_options"),
                    new Vector2(120, TestimonyTab.Rect.Y + TestimonyTab.Rect.Height));
            }
            OptionsTab.Draw(spriteBatch, graphics);
            StatsTab.Draw(spriteBatch, graphics);
            PeopleTab.Draw(spriteBatch, graphics);
            TestimonyTab.Draw(spriteBatch, graphics);


            // Confirm Menu if quitting the game
            if (GState == NotebookState.ClickedQuitGame && ConfirmQuitMenu != null)
            {
                ConfirmQuitMenu.Draw(spriteBatch, graphics);
            }

        }
    }
}