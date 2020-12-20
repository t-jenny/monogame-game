using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Dialogue;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Events
{
    public class EventManager : IManager
    {
        private const double DESATURATION_PERCENT = 0.85;

        private bool EndOfLine;
        private bool TextEnd;

        private MainCharacter MainCharacter;
        private ContentManager Content;
        private TxtReader TxtReader;
        private ButtonState PreviousButtonState;
        private ITextObject CurrentTextObject;
        private CharacterAnimation PriorCharacterAnimation;
        private CharacterAnimation DefaultAnimation;

        private Background Background;
        private LineOfDialogue Dialogue;

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            this.MainCharacter = mainCharacter;
            this.Content = content;
            this.EndOfLine = false;
            this.TextEnd = false;

            String path = Path.Combine(Content.RootDirectory, "json-sample.txt");
            String Text = File.ReadAllText(path);
            AllEventDialogue AllEventDialogue = JsonSerializer.Deserialize<AllEventDialogue>(Text);
            EventDialogue eventDialogue = AllEventDialogue.AllEvents["eventString"][0];

            //Setting up for event to be true (remove later)
            //if (MainCharacter.Relationships.ContainsKey("blush")) {
            //    MainCharacter.Relationships["blush"] = 1;
            //}
            //else {
            //    MainCharacter.Relationships.Add("blush", 1);
            //}

            //if (MainCharacter.Relationships.ContainsKey("tomazzi"))
            //{
            //    MainCharacter.Relationships["tomazzi"] = 1;
            //}
            //else
            //{
            //    MainCharacter.Relationships.Add("tomazzi", 1);
            //}

            //MainCharacter.Stats = new Dictionary<string, int>();
            //MainCharacter.Stats.Add("intelligence", 1);
            //MainCharacter.Stats.Add("strength", 2);

            MainCharacter.EventFlags = new HashSet<string>();
            MainCharacter.EventFlags.Add("beginning");
            MainCharacter.EventFlags.Add("running");

            bool PlayEvent = true;

            //check maincharacter's attributes to see if the event should be played
            foreach (KeyValuePair<string, int> stat in eventDialogue.RequiredStats)
            {
                if (MainCharacter.Stats[stat.Key] < stat.Value)
                {
                    PlayEvent = false;
                }
            }

            foreach (KeyValuePair<string, int> relationship in eventDialogue.RequiredRelationships)
            {
                if (MainCharacter.Relationships[relationship.Key] < relationship.Value)
                {
                    PlayEvent = false;
                }
            }

            if (!eventDialogue.RequiredFlags.IsSubsetOf(MainCharacter.EventFlags))
            {
                PlayEvent = false;
            }

            if (MainCharacter.EventFlags.Contains(eventDialogue.EventName))
            {
                PlayEvent = false;
            }

            if (PlayEvent)
            {
                TxtReader = new TxtReader(MainCharacter, Content, eventDialogue.Text);
            }
            else
            {
                Console.WriteLine("Cannot play event.");
            }

            //event marked as seen
            MainCharacter.EventFlags.Add(eventDialogue.EventName);

        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (TextEnd)
            {
                return;
            }

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
                    CharacterAnimation CurrentAnimation = (CharacterAnimation) CurrentTextObject;
                    DefaultAnimation = CurrentAnimation;
                    CurrentTextObject = TxtReader.NextTxtObject();
                    break;

                case "LineOfDialogue":
                    Dialogue = (LineOfDialogue) CurrentTextObject;
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

            if (TextEnd)
            {
                gameEngine.Pop(true, true);
                return;

            }

            CurrentTextObject.Update(gameTime);
            PreviousButtonState = Mouse.GetState().LeftButton;
        }

        private CharacterAnimation CurrentCharacterAnimation()
        {
            CharacterAnimation Animation = null;

            if (CurrentTextObject.GetType().Name.Equals("LineOfDialogue"))
            {
                LineOfDialogue Dialogue = (LineOfDialogue) CurrentTextObject;
                Animation = Dialogue.CharacterAnimation;
            }

            return Animation;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (CurrentTextObject == null)
            {
                return;
            }

            Background.Draw(spriteBatch, graphics);

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
