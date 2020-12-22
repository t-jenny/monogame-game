using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

using GameDemo.Characters;
using GameDemo.Components;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Notebook;
using GameDemo.Managers;
using GameDemo.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Testimonies
{
    public class InterviewManager : IManager
    {
        private MainCharacter MainCharacter;
        private string CharacterKey;
        private TestimonyList TestimonyList;
        private ContentManager Content;
        private EventScript EventScript;
        private MouseState MouseState;
        private MouseState PrevMouseState;
        private SpriteFont Arial;
        private Background Background;
        private ClickableTexture ContradictButton;
        private List<Button> TopicButtons;

        private string SelectedTopic;
        private int[] IdContradict = new int[] { -1 }; // Id of contradicting testimony from notebook
        private int TestimonyId; // Id of current selected Testimony
        private bool IsContradicted;

        private InterviewState GState;
        private bool IsTransitioning;

        enum InterviewState
        {
            TopicChoice,
            PlayText,
            Contradiction,
            Exiting
        }

        public InterviewManager(string charKey)
        {
            CharacterKey = charKey;
            IsTransitioning = false;
            IsContradicted = false;
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            if (IsTransitioning)
            {
                if (TestimonyId > -1 && TestimonyId == IdContradict[0])
                {
                    Testimony NextTestimony = (from testimony in TestimonyList.Testimonies
                                               where testimony.PrevId == TestimonyId
                                               select testimony).ToList()[0];
                    string Text = NextTestimony.SpokenText;
                    EventScript = new EventScript(MainCharacter, Content, Text);
                    TestimonyId = NextTestimony.Id;
                    IsContradicted = true;
                }
                else if (IdContradict[0] > -1)
                {
                    Console.WriteLine("hello");
                    string Text = "%ost/gumshoe\nExcuse me, what are you suggesting?? @redgeworth #disgusted\n+n: 100";
                    EventScript = new EventScript(MainCharacter, Content, Text);
                    IdContradict[0] = -1;
                }
                GState = InterviewState.PlayText;
                Background = new Background(Content, "castle");
                IsTransitioning = false;
                return;
            }
            else
            {
                GState = InterviewState.TopicChoice;
            }

            content.Unload();

            // Common to all Modes
            Background = new Background(content, "castle");
            MainCharacter = mainCharacter;
            Content = content;
            IsTransitioning = false;
            Arial = Content.Load<SpriteFont>("Fonts/Arial");

            // Load Testimonies
            String path = Path.Combine(Content.RootDirectory, "testimonies.txt");
            String TestimonyJSON = File.ReadAllText(path);
            TestimonyList = JsonSerializer.Deserialize<TestimonyList>(TestimonyJSON);

            ContradictButton = new ClickableTexture(Content.Load<Texture2D>("notebook_icon"),
                new Vector2(Game1.GetWindowSize().X - 100, 20));
            TopicButtons = new List<Button>();
            Vector2 TopicPos = new Vector2(500, 450);
            HashSet<string> TopicTags = (from testimony in TestimonyList.Testimonies
                                         where testimony.IsInitial ||
                                         (MainCharacter.TestimonyIds.Contains(testimony.Id) && testimony.CharacterKey == CharacterKey)
                                         select testimony.TopicTag).ToHashSet();
            Dictionary<string, string> Topics = (from topic in TestimonyList.Topics
                                                 where TopicTags.Contains(topic.Value)
                                                 select topic).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (string Topic in Topics.Keys)
            {
                TopicButtons.Add(new Button(Topic, Arial, TopicPos));
                TopicPos.Y += 75;
            }
            TopicButtons.Add(new Button("Bye!", Arial, TopicPos));

        }

        private void MouseClicked(GameTime gameTime, int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 10, 10);

            switch (GState)
            {
                case InterviewState.TopicChoice:
                    foreach (Button Button in TopicButtons)
                    {
                        if (mouseClickRect.Intersects(Button.Rect) && Button.Text == "Bye!")
                        {
                            GState = InterviewState.Exiting;
                        }
                        else if (mouseClickRect.Intersects(Button.Rect))
                        {
                            GState = InterviewState.PlayText;
                            SelectedTopic = TestimonyList.Topics[Button.Text];
                            List<Testimony> Testimony = (from testimony in TestimonyList.Testimonies
                                                         where testimony.TopicTag == SelectedTopic &&
                                                         testimony.CharacterKey == CharacterKey &&
                                                         (testimony.IsInitial || MainCharacter.TestimonyIds.Contains(testimony.Id))
                                                         select testimony).ToList();

                            string Text = "%ost/gumshoe\nCan I talk to you about something? @lphoenix #talking\nI don't have any information. @redgeworth #disgusted\nOk no problem. @lphoenix #talking\n+n: 100";
                            TestimonyId = -1;

                            if (Testimony.Count > 0 && !Testimony[0].SpokenText.Equals(string.Empty))
                            {
                                Text = Testimony[0].SpokenText;
                                TestimonyId = Testimony[0].Id;
                            }
                            EventScript = new EventScript(MainCharacter, Content, Text);
                        }
                    }
                    break;

                case InterviewState.PlayText:
                    if (mouseClickRect.Intersects(ContradictButton.Rect))
                    {
                        GState = InterviewState.Contradiction;
                    }
                    break;

                default:
                    break;
            }
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning)
            {
                return;
            }

            MouseState = Mouse.GetState();
            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(gameTime, MouseState.X, MouseState.Y);
            }

            switch (GState)
            {
                case InterviewState.Contradiction:
                    gameEngine.Push(new NotebookManager(true, ref IdContradict), true, true);
                    IsTransitioning = true;
                    break;

                case InterviewState.Exiting:
                    gameEngine.Pop(true, true);
                    IsTransitioning = true;
                    break;

                case InterviewState.TopicChoice:
                    foreach (Button TopicButton in TopicButtons)
                    {
                        TopicButton.Update();
                    }
                    break;

                case InterviewState.PlayText:
                    ContradictButton?.Update();
                    EventScript.Update(gameTime);
                    if (EventScript.IsFinished())
                    {
                        GState = (!IsContradicted) ? InterviewState.TopicChoice : InterviewState.Exiting;
                        EventScript = null;
                        MainCharacter.TestimonyIds.Add(TestimonyId);
                    }
                    break;

                default:
                    break;

            }
            PrevMouseState = MouseState;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Background.Draw(spriteBatch, graphics);

            // Banner
            string DayString = MainCharacter.GetDateTimeString();
            DrawingUtils.DrawTextBanner(spriteBatch, graphics, Arial, DayString, Color.Red, Color.Black);

            if (GState == InterviewState.TopicChoice)
            {
                // draw placeholders for now
                Texture2D Phoenix = Content.Load<Texture2D>("Characters/blush");
                spriteBatch.Draw(Phoenix, new Vector2(100, 300), Color.White);
                Texture2D Edgeworth = Content.Load<Texture2D>("Characters/tomazzi");
                spriteBatch.Draw(Edgeworth, new Vector2(900, 300), Color.White);
                Texture2D Thought = Content.Load<Texture2D>("thought");
                spriteBatch.Draw(Thought, new Rectangle(350, 300, 500, 500), Color.White);

                foreach (Button TopicButton in TopicButtons)
                {
                    TopicButton.Draw(spriteBatch, graphics);
                }
            }

            if (GState == InterviewState.PlayText)
            {
                // Contradict icon is just notebook icon for now
                ContradictButton.Draw(spriteBatch, graphics);
                EventScript.Draw(spriteBatch, graphics);
            }
        }
    }
}
