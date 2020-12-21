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
        private Case Case;

        private const string NotebookPath = "notebook";
        private Background Background;

        private Texture2D ReturnIcon;
        private Rectangle ReturnIconRect;

        // Notebook tabs/sections
        private ClickableTexture PeopleTab;
        private ClickableTexture OptionsTab;
        private ClickableTexture StatsTab;
        private ClickableTexture TestimonyTab;
        private Rectangle OpenNotebookRect;

        // Selection Lists
        private AllCharacters AllChars;
        private OptionsList MainOptionsList;
        private TestimonyList TestimonyList;
        private OptionsList TopicOptionsList;

        // Page-Specific Components
        private Button QuitButton;
        private Button SaveButton;
        private ConfirmMenu ConfirmQuitMenu;
        private Button SelectTestimonyButton;
        private ConfirmMenu ConfirmContradictMenu;

        //Make this part of notebook page class;
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
        private int[] TestimonyId;

        enum NotebookState
        {
            Returning,
            Stats,
            Testimonies,
            Profiles,
            Options,
            ClickedQuitGame,
            ConfirmedQuitGame,
            SelectedTestimony
        }

        public NotebookManager(bool seekingTestimony, ref int[] testimonyId)
        {
            SeekingTestimony = seekingTestimony;
            TestimonyId = testimonyId;
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            MainCharacter = mainCharacter;
            Content = content;

            // Load Characters
            String path = Path.Combine(Content.RootDirectory, "characters.txt");
            String CharJSON = File.ReadAllText(path);
            AllChars = JsonSerializer.Deserialize<AllCharacters>(CharJSON);

            // Load Case
            String CasePath = Path.Combine(Content.RootDirectory, "case" + MainCharacter.CurrentCase + ".txt");
            String CaseJSON = File.ReadAllText(CasePath);
            Case = JsonSerializer.Deserialize<Case>(CaseJSON);

            // Load Testimonies
            path = Path.Combine(Content.RootDirectory, "testimonies.txt");
            String TestimonyJSON = File.ReadAllText(path);
            TestimonyList = JsonSerializer.Deserialize<TestimonyList>(TestimonyJSON);

            // Always start by viewing stats.
            GState = NotebookState.Stats;
            Point WindowSize = Game1.GetWindowSize();

            // Fixed Visual Elements
            Background = new Background(content, NotebookPath);
            ReturnIcon = Content.Load<Texture2D>("return-icon");
            ReturnIconRect = new Rectangle(WindowSize.X - 100, 20, 70, 70);

            int TabOffset = WindowSize.X / 10 - 8;
            PeopleTab = new ClickableTexture(Content.Load<Texture2D>("tab_people"), new Vector2(TabOffset, TabOffset));
            StatsTab = new ClickableTexture(Content.Load<Texture2D>("tab_stats"),
                new Vector2(TabOffset, PeopleTab.Rect.Y + PeopleTab.Rect.Height));
            TestimonyTab = new ClickableTexture(Content.Load<Texture2D>("tab_testimony"),
                new Vector2(TabOffset, StatsTab.Rect.Y + StatsTab.Rect.Height));
            OptionsTab = new ClickableTexture(Content.Load<Texture2D>("tab_options"),
                new Vector2(TabOffset, TestimonyTab.Rect.Y + TestimonyTab.Rect.Height));

            // Variable Visual Elements
            MainOptionsList = null;
            TopicOptionsList = null;
            QuitButton = null;
            SaveButton = null;

            // Fonts and Text
            Arial = content.Load<SpriteFont>("Fonts/Arial");
            JustBreathe = content.Load<SpriteFont>("Fonts/JustBreathe20");
            JustBreathe25 = content.Load<SpriteFont>("Fonts/JustBreathe25");
            TextOffset = new Vector2(0.0f, JustBreathe.MeasureString("A").Y + 0.5f);
            Indent = new Vector2(Arial.MeasureString("A").X, 0.0f);

            OpenNotebookRect = new Rectangle((int) (0.12f * WindowSize.X),
                (int) (0.15f * WindowSize.Y + TextOffset.Y),
                (int) (0.75f * WindowSize.X),
                (int) (0.71f * WindowSize.Y));

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();

            /*** Update Components ***/
            // Notebook sections (Constant)
            PeopleTab.Update();
            OptionsTab.Update();
            StatsTab.Update();
            TestimonyTab.Update();

            // Tab-specific components (Variable)
            MainOptionsList?.Update();
            TopicOptionsList?.Update();
            QuitButton?.Update();
            SaveButton?.Update();
            SelectTestimonyButton?.Update();
            ConfirmQuitMenu?.Update(gameTime);
            ConfirmContradictMenu?.Update(gameTime);

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

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background
            Background.Draw(spriteBatch, graphics);

            // Banner with Date
            string DateString = MainCharacter.GetDateTimeString();
            DrawingUtils.DrawTextBanner(spriteBatch, graphics, Arial, DateString, Color.Red, Color.Black);
            spriteBatch.Draw(ReturnIcon, ReturnIconRect, Color.White);

            // Notebook Pages
            DrawLeftPage(spriteBatch, graphics);
            DrawRightPage(spriteBatch, graphics);

            // Draw Notebook Tabs
            OptionsTab.Draw(spriteBatch, graphics);
            StatsTab.Draw(spriteBatch, graphics);
            PeopleTab.Draw(spriteBatch, graphics);
            TestimonyTab.Draw(spriteBatch, graphics);

            // Confirm Menu if quitting the game
            if (GState == NotebookState.ClickedQuitGame)
            {
                ConfirmQuitMenu.Draw(spriteBatch, graphics);
            }

            // Confirm Menu if selected testimony
            if (GState == NotebookState.SelectedTestimony)
            {
                ConfirmContradictMenu.Draw(spriteBatch, graphics);
            }

        }

        // from Microsoft Docs
        private void SaveGame()
        {
            string path = Path.Combine(Content.RootDirectory, "savedata.txt");
            string json = JsonSerializer.Serialize<MainCharacter>(MainCharacter);
            File.WriteAllText(path, json);
        }

        private void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(10, 10));

            switch (GState)
            {
                case NotebookState.SelectedTestimony:
                    if (ConfirmContradictMenu.IsConfirming(MouseClickRect))
                    {
                        GState = NotebookState.Returning;
                    }
                    if (ConfirmContradictMenu.IsCancelling(MouseClickRect))
                    {
                        GState = NotebookState.Testimonies;
                    }
                    break;

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

                    if (SeekingTestimony && SelectTestimonyButton != null)
                    {
                        if (MouseClickRect.Intersects(SelectTestimonyButton.Rect))
                        {
                            GState = NotebookState.SelectedTestimony;
                            string Query = "Are you sure you want to contradict?";
                            ConfirmContradictMenu = new ConfirmMenu(Query, Content, Arial);
                        }
                    }
                    break;
            }
        }

        private void DrawLeftPage(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 TextPosLeft = new Vector2(OpenNotebookRect.X, OpenNotebookRect.Y);
            Vector2 ColOffset = new Vector2(OpenNotebookRect.Width / 4, 0);
            if (MainOptionsList == null) {
                MainOptionsList = new OptionsList(GetMainOptions(), JustBreathe, (TextPosLeft + 2 * TextOffset).ToPoint());
            }
            MainOptionsList.Draw(spriteBatch, graphics);

            switch (GState)
            {
                case NotebookState.Stats:
                    spriteBatch.DrawString(JustBreathe25, "Stats:", TextPosLeft, Color.Black);
                    break;

                case NotebookState.ClickedQuitGame:
                case NotebookState.Options:
                    spriteBatch.DrawString(JustBreathe25, "Options:", TextPosLeft, Color.Black);
                    break;

                case NotebookState.Profiles:
                    // Lefthand Page
                    spriteBatch.DrawString(JustBreathe25, "People:", TextPosLeft, Color.Black);
                    break;

                case NotebookState.SelectedTestimony:
                case NotebookState.Testimonies:
                    // Add topics list to lefthand page
                    spriteBatch.DrawString(JustBreathe25, "Testimony:", TextPosLeft, Color.Black);
                    spriteBatch.DrawString(JustBreathe25, "Person", TextPosLeft += TextOffset, Color.Black);
                    spriteBatch.DrawString(JustBreathe25, "Topic", TextPosLeft += ColOffset, Color.Black);

                    if (TopicOptionsList == null)
                    {
                        TopicOptionsList = new OptionsList(TestimonyList.Topics, JustBreathe, (TextPosLeft += TextOffset).ToPoint());
                    }
                    TopicOptionsList.Draw(spriteBatch, graphics);
                    break;

                default:
                    break;

            }
        }

        private void DrawRightPage(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 TextPosRight = new Vector2(OpenNotebookRect.X + 0.55f * OpenNotebookRect.Width, OpenNotebookRect.Y);
            switch (GState)
            {
                case NotebookState.Stats:
                    Rectangle StatTableRect = new Rectangle((TextPosRight + TextOffset).ToPoint(),
                        new Point(OpenNotebookRect.Width / 5, OpenNotebookRect.Height / 2));
                    if (MainOptionsList.SelectedOption == "stats")
                    {
                        spriteBatch.DrawString(JustBreathe, "My Stats: ", TextPosRight, Color.Black);
                        string[] Aspects = new string[6] { "charm", "courage", "empathy", "intelligence", "strength", "money" };
                        InfoTable StatTable = new InfoTable(MainCharacter.Stats, Aspects, StatTableRect, JustBreathe);
                        StatTable.Draw(spriteBatch, graphics);
                    }
                    else if (MainOptionsList.SelectedOption == "relationships")
                    {
                        spriteBatch.DrawString(JustBreathe, "Friendship Levels: ", TextPosRight, Color.Black);
                        string[] People = Case.Suspects.Concat(Case.TestimonyOnly).ToArray();
                        InfoTable RelTable = new InfoTable(MainCharacter.Relationships, People, StatTableRect, JustBreathe);
                        RelTable.Draw(spriteBatch, graphics);
                    }
                    break;

                case NotebookState.ClickedQuitGame:
                case NotebookState.Options:
                    if (MainOptionsList.SelectedOption == "savequit")
                    {
                        if (QuitButton == null)
                        {
                            SaveButton = new Button("Save Game", Arial, TextPosRight);
                            QuitButton = new Button("Quit Game", Arial, TextPosRight + new Vector2(0.0f, 2 * SaveButton.Rect.Height));
                        }
                        QuitButton.Draw(spriteBatch, graphics);
                        SaveButton.Draw(spriteBatch, graphics);
                    }
                    break;

                case NotebookState.Profiles:
                    if (MainOptionsList.SelectedOption != null)
                    {
                        DrawCharacterEntry(spriteBatch, graphics, AllChars.AllChars[MainOptionsList.SelectedOption], TextPosRight);
                    }
                    break;

                case NotebookState.Testimonies:
                case NotebookState.SelectedTestimony:

                    // must select a character and a topic to view testimony
                    if (TopicOptionsList.SelectedOption != null && MainOptionsList?.SelectedOption != null)
                    {
                        List<Testimony> Testimonies = (from testimony in TestimonyList.Testimonies
                                                       where testimony.TopicTag == TopicOptionsList?.SelectedOption &&
                                                       testimony.CharacterKey == MainOptionsList?.SelectedOption
                                                       select testimony).ToList();

                        if (Testimonies.Count > 0)
                        {
                            TestimonyId[0] = Testimonies[0].IdContradict;
                        }

                        DrawTestimonies(spriteBatch, graphics, Testimonies, TextPosRight);

                        if (SeekingTestimony)
                        {
                            SelectTestimonyButton = new Button("Select", JustBreathe25,
                                TextPosRight + new Vector2(0, OpenNotebookRect.Height - 50));
                            SelectTestimonyButton.Draw(spriteBatch, graphics);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private Dictionary<string, string> GetMainOptions()
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
            Rectangle TestimonyRect = new Rectangle((int)(textPos + TextOffset).X,
                (int)(textPos + TextOffset).Y,
                (int)(0.45f * OpenNotebookRect.Width),
                (int)(OpenNotebookRect.Height - TextOffset.Y));

            List<string> ResultsPages = DrawingUtils.WrappedString(JustBreathe, Results, TestimonyRect, 0.1f);
            spriteBatch.DrawString(JustBreathe, ResultsPages[PageIndex], textPos, Color.Black);
        }

    }
}