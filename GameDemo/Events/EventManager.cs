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

            String path = Path.Combine(Content.RootDirectory, "json-sample.txt");
            String Text = File.ReadAllText(path);
            AllEventDialogue AllEventDialogue = JsonSerializer.Deserialize<AllEventDialogue>(Text);
            EventDialogue eventDialogue = AllEventDialogue.AllEvents["eventString"][0];

            //Setting up for event to be true (remove later)
            MainCharacter.Relationships = new Dictionary<string, int>();
            MainCharacter.Relationships.Add("blush", 1);
            MainCharacter.Relationships.Add("tomazzi", 1);
            MainCharacter.Stats = new Dictionary<string, int>();
            MainCharacter.Stats.Add("intelligence", 1);
            MainCharacter.Stats.Add("strength", 2);
            MainCharacter.EventFlags = new HashSet<string>();
            MainCharacter.EventFlags.Add("beginning");
            MainCharacter.EventFlags.Add("running");

            bool PlayEvent = true;

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

            if (PlayEvent)
            {
                TxtReader = new TxtReader(MainCharacter, Content, eventDialogue.Text);
            }
            else
            {
                Console.WriteLine("Cannot play event.");
            }

        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            int TextEnd = 0;
            if (CurrentTextObject == null)
            {
                CurrentTextObject = TxtReader.CurrentTxtObject();
            }

            switch (CurrentTextObject.GetType().Name)
            {
                case "Background":
                    Background = (Background)CurrentTextObject;
                    PriorCharacterAnimation = null;
                    TextEnd = NextTextObject();
                    break;

                case "CharacterAnimation":
                    CharacterAnimation CurrentAnimation = (CharacterAnimation) CurrentTextObject;
                    DefaultAnimation = CurrentAnimation;
                    TextEnd = NextTextObject();
                    break;

                case "LineOfDialogue":
                    Dialogue = (LineOfDialogue)CurrentTextObject;
                    EndOfLine = Dialogue.Complete();
                    break;

                default:
                    TextEnd = NextTextObject();
                    break;
            }

            if (CurrentTextObject.GetType().Name.Equals("LineOfDialogue"))
            {
                if (DefaultAnimation != null)
                {
                    Dialogue.SetSecondAnimation(DefaultAnimation, DESATURATION_PERCENT);
                }
            }

            if (PreviousButtonState == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released && EndOfLine)
            {
                CharacterAnimation CurrentCharacter = Dialogue.CharacterAnimation;

                TextEnd = NextTextObject();
                EndOfLine = false;

                CharacterAnimation NextCharacter = CurrentCharacterAnimation();

                if (CurrentCharacter != null && NextCharacter != null
                    && !NextCharacter.CharacterName.Equals(CurrentCharacter.CharacterName))
                {
                    PriorCharacterAnimation = CurrentCharacter;
                    DefaultAnimation = null;
                }
            }

            // event has ended - pop event off of game stack.
            if (TextEnd == 1)
            {
                gameEngine.Pop(false, false);
            }

            CurrentTextObject.Update(gameTime);
            PreviousButtonState = Mouse.GetState().LeftButton;
        }

        // Move to the next text object, return 1 if there is no more text
        private int NextTextObject()
        {
            int status = TxtReader.NextTxtObject();
            CurrentTextObject = TxtReader.CurrentTxtObject();
            return status;
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

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
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
