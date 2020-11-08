using System;
using System.Collections.Generic;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Dialogue;
using GameDemo.Locations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Events
{
    public class EventManager
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

        public EventManager(MainCharacter mainCharacter, EventDialogue eventDialogue, ContentManager content)
        {
            this.MainCharacter = mainCharacter;
            this.Content = content;
            this.EndOfLine = false;

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

        public void Update(GameTime gameTime)
        {
            if (CurrentTextObject == null)
            {
                CurrentTextObject = TxtReader.CurrentTxtObject();
            }

            switch (CurrentTextObject.GetType().Name)
            {
                case "Background":
                    Background = (Background)CurrentTextObject;
                    PriorCharacterAnimation = null;
                    NextTextObject();
                    break;

                case "CharacterAnimation":
                    CharacterAnimation CurrentAnimation = (CharacterAnimation) CurrentTextObject;
                    DefaultAnimation = CurrentAnimation;
                    NextTextObject();
                    break;

                case "LineOfDialogue":
                    Dialogue = (LineOfDialogue)CurrentTextObject;
                    EndOfLine = Dialogue.Complete();
                    break;

                default:
                    NextTextObject();
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

                TxtReader.NextTxtObject();
                EndOfLine = false;
                CurrentTextObject = TxtReader.CurrentTxtObject();

                CharacterAnimation NextCharacter = CurrentCharacterAnimation();

                if (CurrentCharacter != null && NextCharacter != null
                    && !NextCharacter.CharacterName.Equals(CurrentCharacter.CharacterName))
                {
                    PriorCharacterAnimation = CurrentCharacter;
                    DefaultAnimation = null;
                }
            }

            CurrentTextObject.Update(gameTime);
            PreviousButtonState = Mouse.GetState().LeftButton;
        }

        private void NextTextObject()
        {
            TxtReader.NextTxtObject();
            CurrentTextObject = TxtReader.CurrentTxtObject();
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
