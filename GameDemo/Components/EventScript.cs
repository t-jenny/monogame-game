using System;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Dialogue;
using GameDemo.Locations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Components
{
    public class EventScript
    {
        private const double DESATURATION_PERCENT = 0.85;

        private ITextObject CurrentTextObject;
        private CharacterAnimation PriorCharacterAnimation;
        private CharacterAnimation DefaultAnimation;
        private ButtonState PreviousButtonState;

        private bool EndOfLine;
        private bool TextEnd;
        private TxtReader TxtReader;
        private Background Background;
        private LineOfDialogue Dialogue;

        public EventScript(MainCharacter mainCharacter, ContentManager content, string text)
        {
            this.EndOfLine = false;
            this.TextEnd = false;

            TxtReader = new TxtReader(mainCharacter, content, text);
        }

        public void Update(GameTime gameTime)
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
            if (!TextEnd)
            {
                CurrentTextObject.Update(gameTime);
                PreviousButtonState = Mouse.GetState().LeftButton;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (CurrentTextObject == null)
            {
                return;
            }

            Background?.Draw(spriteBatch, graphics);

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

        public bool IsFinished()
        {
            return TextEnd;
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

    }
}
