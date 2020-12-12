using System;
using System.IO;
using System.Text.Json;
using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Components;
using GameDemo.Managers;
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
        private OptionsList OptionsList;

        private Button QuitButton;
        private ConfirmMenu ConfirmQuitMenu;

        private SpriteFont Arial;
        private SpriteFont JustBreathe;
        private SpriteFont JustBreathe25;
        private Vector2 TextOffset;
        private Vector2 Indent;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private NotebookState GState;
        private bool IsTransitioning;

        enum NotebookState
        {
            Returning,
            Stats,
            Testimonies,
            Profiles,
            Locations,
            Options,
            ClickedQuitGame,
            ConfirmedQuitGame
        }

        private string[] GetOptions()
        {
            switch (GState)
            {
                case NotebookState.Profiles:
                    // pull from people known to main character
                    return new string[] { "Diana", "Jenny", "Kai" };

                case NotebookState.Stats:
                    return new string[] { "My Stats", "Relationships" };

                case NotebookState.Options:
                    return new string[] { "Settings", "Save & Quit" };

                default:
                    return new string[] { };

            }
        }

        private void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(50, 50));

            switch (GState)
            {
                case NotebookState.ClickedQuitGame:
                    if (ConfirmQuitMenu.IsConfirming(MouseClickRect))
                    {
                        GState = NotebookState.ConfirmedQuitGame;
                    }
                    if (ConfirmQuitMenu.IsCancelling(MouseClickRect))
                    {
                        GState = NotebookState.Stats; // replace with whichever state exposes the quit button
                    }
                    break;

                default:
                    if (MouseClickRect.Intersects(StatsTab.Rect))
                    {
                        GState = NotebookState.Stats;
                        OptionsList = null;
                    }
                    if (MouseClickRect.Intersects(PeopleTab.Rect))
                    {
                        GState = NotebookState.Profiles;
                        OptionsList = null;
                    }
                    if (MouseClickRect.Intersects(OptionsTab.Rect))
                    {
                        GState = NotebookState.Options;
                        OptionsList = null;
                    }
                    if (MouseClickRect.Intersects(ReturnIconRect))
                    {
                        GState = NotebookState.Returning;
                    }

                    if (OptionsList?.SelectedOption == "Save & Quit" && MouseClickRect.Intersects(QuitButton.Rect))
                    {
                        GState = NotebookState.ClickedQuitGame;
                        string Query = "Are you sure you want to quit the game?";
                        ConfirmQuitMenu = new ConfirmMenu(Query, Content);
                    }
                    break;
            }
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            MainCharacter = mainCharacter;
            Content = content;

            // Visual Elements
            Background = new Background(content, NotebookPath);
            ReturnIcon = Content.Load<Texture2D>("return-icon");
            QuitButton = null;

            // Always start by viewing stats.
            GState = NotebookState.Stats;
            OptionsList = null;

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
            OptionsList?.Update();

            // Game Quit Components
            QuitButton?.Update();
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

        private void DrawCharacterEntry(SpriteBatch spriteBatch, Character character, Vector2 textPos)
        {
            Texture2D CharPic = Content.Load<Texture2D>(character.ImagePath);
            Rectangle PicRect = new Rectangle((int)textPos.X, (int)textPos.Y, 100, 100);
            spriteBatch.Draw(CharPic, PicRect, Color.White);

            spriteBatch.DrawString(JustBreathe, character.Name, textPos += new Vector2(PicRect.Width + 5.0f, 0.0f), Color.Black);
            spriteBatch.DrawString(JustBreathe, "Age: " + character.Age, textPos += TextOffset, Color.Black);
            spriteBatch.DrawString(JustBreathe, "Personality: " + character.Personality, textPos += TextOffset, Color.Black);

            // Character Description:
            spriteBatch.DrawString(JustBreathe, "Description: ",
                textPos += 2.5f * TextOffset - new Vector2(PicRect.Width + 5.0f, 0.0f),
                Color.Black);
            Point DescBoxSize = new Point(300, 300);
            string Description = DrawingUtils.WrappedString(JustBreathe,
                character.Description,
                new Rectangle(((textPos+=TextOffset) + Indent).ToPoint(), DescBoxSize),
                0.1f);
            spriteBatch.DrawString(JustBreathe, Description, textPos + Indent, Color.Black);

            // Best Friends:
            spriteBatch.DrawString(JustBreathe, "Best Friends: ", textPos += 4 * TextOffset, Color.Black);
            foreach (string FriendName in character.BFFs)
            {
                spriteBatch.DrawString(JustBreathe, "- " + FriendName, (textPos += TextOffset) + Indent, Color.Black);
            }
        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background
            Background.Draw(spriteBatch, graphics);
            int ScreenWidth = graphics.GraphicsDevice.Viewport.Width;

            // Banner with Date
            DateTime CurrentDate = MainCharacter.GetDate();
            String DateString = CurrentDate.ToString("dddd, MMMM dd");
            spriteBatch.DrawString(Arial, DateString, new Vector2(10.0f, 30.0f), Color.Black);
            DrawingUtils.DrawTextBanner(graphics, spriteBatch, Arial, DateString, Color.Red, Color.Black);

            // Return-to-world icon
            if (ReturnIconRect.IsEmpty)
            {
                ReturnIconRect = new Rectangle(ScreenWidth - 100, 20, 70, 70);
            }
            spriteBatch.Draw(ReturnIcon, ReturnIconRect, Color.White);

            /***** Draw notebook info (make this more modular *****/
            Vector2 TextPos = new Vector2(1.1f * ScreenWidth / 2, 120);

            //spriteBatch.DrawString(JustBreathe, MainCharacter.Name + "\'s Notebook", TextPos, Color.Black);
            //Point NameSize = JustBreathe.MeasureString(MainCharacter.Name + "\'s Notebook").ToPoint();
            //Rectangle NameRect = new Rectangle(TextPos.ToPoint(), NameSize);
            //DrawingUtils.DrawUnderline(graphics, spriteBatch, NameRect, Color.Black);
            TextPos += TextOffset;

            // Notebook Page Display
            switch (OptionsList?.SelectedOption)
            {
                case "My Stats":
                    spriteBatch.DrawString(JustBreathe, "My Stats: ", TextPos, Color.Black);
                    foreach (string Stat in MainCharacter.Stats.Keys)
                    {
                        TextPos += TextOffset;
                        string StatString = Stat + ": " + MainCharacter.Stats[Stat];
                        spriteBatch.DrawString(JustBreathe, StatString, TextPos, Color.Black);
                    }
                    break;

                case "Relationships":
                    spriteBatch.DrawString(JustBreathe, "Friendship Levels: ", TextPos, Color.Black);
                    foreach (string CharName in MainCharacter.Relationships.Keys)
                    {
                        TextPos += TextOffset;
                        string RelString = CharName + ": " + MainCharacter.Relationships[CharName];
                        spriteBatch.DrawString(JustBreathe, RelString, TextPos, Color.Black);
                    }
                    break;


                case "Save & Quit":
                    if (QuitButton == null)
                    {
                        QuitButton = new Button("Quit Game", Arial,
                            (int)TextPos.X,
                            (int)(TextPos + 2 * TextOffset).Y + 5);
                    }
                    QuitButton.Draw(spriteBatch, graphics);
                    break;

                case "Settings":
                    break;

                // Defaults to character info
                default:
                    if (OptionsList?.SelectedOption != null)
                    {
                        String path = Path.Combine(Content.RootDirectory, "characters.txt");
                        String CharJSON = File.ReadAllText(path);
                        AllCharacters CharList = JsonSerializer.Deserialize<AllCharacters>(CharJSON);
                        DrawCharacterEntry(spriteBatch, CharList.AllChars[OptionsList.SelectedOption], TextPos);
                    }
                    break;

            }
            /***** End Notebook Page placeholder *****/

            // Draw OptionsList
            if (OptionsList == null)
            {
                Rectangle OptionsListRect = new Rectangle(150, 120, 300, 500);
                OptionsList = new OptionsList(GetOptions(), JustBreathe25, OptionsListRect);
            }
            OptionsList.Draw(graphics, spriteBatch);

            // Draw Notebook Tabs
            if (PeopleTab == null)
            {
                PeopleTab = new ClickableTexture(Content.Load<Texture2D>("tab_people"),
                    new Vector2(ScreenWidth / 2, 95.0f));
                StatsTab = new ClickableTexture(Content.Load<Texture2D>("tab_stats"),
                    new Vector2(PeopleTab.Rect.X + PeopleTab.Rect.Width, 95.0f));
                OptionsTab = new ClickableTexture(Content.Load<Texture2D>("tab_options"),
                    new Vector2(StatsTab.Rect.X + StatsTab.Rect.Width, 95.0f));
            }
            OptionsTab.Draw(spriteBatch, graphics);
            StatsTab.Draw(spriteBatch, graphics);
            PeopleTab.Draw(spriteBatch, graphics);


            // Confirm Menu if quitting the game
            if (GState == NotebookState.ClickedQuitGame && ConfirmQuitMenu != null)
            {
                ConfirmQuitMenu.Draw(spriteBatch, Arial, graphics);
            }

        }
    }
}