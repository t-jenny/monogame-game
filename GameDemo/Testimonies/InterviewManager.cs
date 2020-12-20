using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Components;
using GameDemo.Dialogue;
using GameDemo.Engine;
using GameDemo.Events;
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
        private const double DESATURATION_PERCENT = 0.85;

        private bool EndOfLine;
        private bool TextEnd;

        private MainCharacter MainCharacter;
        private string CharacterKey;
        private TestimonyList TestimonyList;
        private ContentManager Content;
        private TxtReader TxtReader;
        private ButtonState PreviousButtonState;
        private MouseState MouseState;
        private MouseState PrevMouseState;
        private ITextObject CurrentTextObject;
        private CharacterAnimation PriorCharacterAnimation;
        private CharacterAnimation DefaultAnimation;

        private SpriteFont Arial;
        private SpriteFont JustBreathe;
        private Background Background;
        private EventDialogue EventDialogue;
        private LineOfDialogue Dialogue;
        private ClickableTexture ContradictButton;
        private List<Button> TopicButtons;

        private string SelectedTopic;
        private int[] IdContradict = new int[] { -1 }; // Id of contradicting testimony from notebook
        private int TestimonyId; // Id of current selected Testimony
        private bool IsContradicted;

        //private Button NextButton;

        private InterviewState GState;
        bool IsTransitioning;

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
                    string Text = "Shucks, you got me! @redgeworth #disgusted\n+n: 100";
                    TxtReader = new TxtReader(MainCharacter, Content, Text);
                    IsContradicted = true;
                }
                GState = InterviewState.PlayText;
                Background = new Background(content, "witnessempty");
                IsTransitioning = false;
                return;
            }
            else
            {
                GState = InterviewState.TopicChoice;
            }

            content.Unload();
            Background = new Background(content, "witnessempty");

            this.MainCharacter = mainCharacter;
            this.Content = content;

            IsTransitioning = false;
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
            JustBreathe = Content.Load<SpriteFont>("Fonts/JustBreathe20");
            ContradictButton = null;
            TopicButtons = new List<Button>();

            this.EndOfLine = false;
            this.TextEnd = false;

            // Load Testimonies
            String path = Path.Combine(Content.RootDirectory, "testimonies.txt");
            String TestimonyJSON = File.ReadAllText(path);
            TestimonyList = JsonSerializer.Deserialize<TestimonyList>(TestimonyJSON);

        }

        private void MouseClicked(GameTime gameTime, int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 10, 10);

            switch (GState)
            {
                case InterviewState.TopicChoice:
                    foreach (Button Button in TopicButtons)
                    {
                        // Don't add this to button list
                        if (mouseClickRect.Intersects(Button.Rect) && Button.Text == "Bye!")
                        {
                            GState = InterviewState.Exiting;
                        }
                        else if (mouseClickRect.Intersects(Button.Rect))
                        {
                            GState = InterviewState.PlayText;
                            SelectedTopic = TestimonyList.Topics[Button.Text];
                        }
                    }
                    break;

                case InterviewState.PlayText:
                    if (mouseClickRect.Intersects(ContradictButton.Rect))
                    {
                        GState = InterviewState.Contradiction;
                        TxtReader = null;
                    }
                    break;

                default:
                    break;
            }
        }

        public void HandleText(GameTime gameTime)
        {

            if (CurrentTextObject == null)
            {
                CurrentTextObject = TxtReader.NextTxtObject();
            }

            if (Dialogue != null)
            {
                EndOfLine = Dialogue.Complete();
            }

            switch (CurrentTextObject.GetType().Name)
            {
                case "Background":
                    Background = (Background)CurrentTextObject;
                    PriorCharacterAnimation = null;
                    CurrentTextObject = TxtReader.NextTxtObject();
                    break;

                case "CharacterAnimation":
                    CharacterAnimation CurrentAnimation = (CharacterAnimation)CurrentTextObject;
                    DefaultAnimation = CurrentAnimation;
                    CurrentTextObject = TxtReader.NextTxtObject();
                    break;

                case "LineOfDialogue":
                    Dialogue = (LineOfDialogue)CurrentTextObject;
                    if (DefaultAnimation != null)
                    {
                        Dialogue.SetSecondAnimation(DefaultAnimation, DESATURATION_PERCENT);
                    }

                    if (PreviousButtonState == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released && EndOfLine)
                    {
                        CharacterAnimation CurrentCharacter = Dialogue.CharacterAnimation;
                        CurrentTextObject = TxtReader.NextTxtObject();
                        EndOfLine = false;

                        CharacterAnimation NextCharacter = null;

                        if (CurrentTextObject != null)
                        {
                            NextCharacter = CurrentCharacterAnimation();
                        }

                        if (CurrentCharacter != null && NextCharacter != null
                            && !NextCharacter.CharacterName.Equals(CurrentCharacter.CharacterName))
                        {
                            PriorCharacterAnimation = CurrentCharacter;
                            DefaultAnimation = null;
                        }
                    }

                    break;

                default:
                    CurrentTextObject = TxtReader.NextTxtObject();
                    break;
            }

            TextEnd = TxtReader.IsEmpty();

            if (TextEnd) // Player should actively choose to end testimony
            {
                GState = (!IsContradicted) ? InterviewState.TopicChoice : InterviewState.Exiting;
                TxtReader = null;
            }
            else
            {
                CurrentTextObject.Update(gameTime);
                PreviousButtonState = Mouse.GetState().LeftButton;
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
                    if (TxtReader == null)
                    {
                        List<Testimony> Testimony = (from testimony in TestimonyList.Testimonies
                                               where testimony.TopicTag == SelectedTopic &&
                                               testimony.CharacterKey == CharacterKey
                                               select testimony).ToList();

                        string Text = "%ost/gumshoe\nCan I talk to you about something? @lphoenix #talking\nI don't have any information. @redgeworth #disgusted\nOk no problem. @lphoenix #talking\n+n: 100";
                        TestimonyId = -1;

                        if (Testimony.Count > 0)
                        {
                            TestimonyId = Testimony[0].Id;
                        }

                        if (Testimony.Count > 0 && !Testimony[0].SpokenText.Equals(string.Empty)) {
                            Text = Testimony[0].SpokenText;
                        }

                        TxtReader = new TxtReader(MainCharacter, Content, Text);
                    }
                    HandleText(gameTime);
                    break;

                default:
                    break;
            }
            PrevMouseState = MouseState;
        }

        private CharacterAnimation CurrentCharacterAnimation()
        {
            CharacterAnimation Animation = null;

            if (CurrentTextObject.GetType().Name.Equals("LineOfDialogue"))
            {
                LineOfDialogue Dialogue = (LineOfDialogue)CurrentTextObject;
                Animation = Dialogue.CharacterAnimation;
            }

            return Animation;
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

                Vector2 TopicPos = new Vector2(500, 450);
                if (TopicButtons.Count == 0)
                {
                    foreach (string Topic in TestimonyList.Topics.Keys)
                    {
                        TopicButtons.Add(new Button(Topic, Arial, TopicPos));
                        TopicPos.Y += 75;
                    }
                    TopicButtons.Add(new Button("Bye!", Arial, TopicPos));
                }
                foreach (Button TopicButton in TopicButtons)
                {
                    TopicButton.Draw(spriteBatch, graphics);
                }
            }

            if (GState == InterviewState.PlayText)
            {
                if (CurrentTextObject == null)
                {
                    return;
                }

                if (ContradictButton == null)
                {
                    ContradictButton = new ClickableTexture(Content.Load<Texture2D>("notebook_icon"),
                        new Vector2(graphics.GraphicsDevice.Viewport.Width - 100, 20));
                }
                ContradictButton.Draw(spriteBatch, graphics);

                if (PriorCharacterAnimation != null)
                {
                    if (!PriorCharacterAnimation.Desaturated)
                    {
                        PriorCharacterAnimation.Desaturate(graphics, DESATURATION_PERCENT);
                    }

                    PriorCharacterAnimation.Draw(spriteBatch, graphics);
                }

                if (!CurrentTextObject.GetType().Name.Equals("CharacterAnimation"))
                {
                    CurrentTextObject.Draw(spriteBatch, graphics);
                }
            }
        }
    }
}
