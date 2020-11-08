using System;
using GameDemo.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Dialogue
{
    public class LineOfDialogue : ITextObject
    {
        private TextBox TextBox;
        private SpriteFont Arial;

        public CharacterAnimation CharacterAnimation { get; private set; }

        public LineOfDialogue(ContentManager content, String dialogue, CharacterAnimation characterAnimation)
        {
            this.CharacterAnimation = characterAnimation;
            this.Arial = content.Load<SpriteFont>("Fonts/Arial");

            TextBox = new TextBox(content, dialogue);
        }

        public bool Complete()
        {
            if (CharacterAnimation != null)
            {
                if (TextBox.CurrentTextBoxAnimation.TextComplete)
                {
                    return true;
                }

                if (CharacterAnimation.Animation.FastForward && TextBox.CurrentTextBoxAnimation.FastForward)
                {
                    return true;
                }
            } else if (TextBox.CurrentTextBoxAnimation.FastForward)
            {
                return true;
            }

            return false;
        }

        public void Update(GameTime gameTime)
        {
            if (CharacterAnimation != null)
            {
                CharacterAnimation.Update(gameTime);
                if (TextBox.CurrentTextBoxAnimation != null && TextBox.CurrentTextBoxAnimation.TextComplete)
                {
                    CharacterAnimation.Animation.FastForward = true;
                }
            }

            TextBox.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 topLeftofDialogue = new Vector2(75, 505);
            Color tintColor = Color.White;

            Texture2D rect = new Texture2D(graphics.GraphicsDevice, 250, 50);

            Color[] data = new Color[250 * 50];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Black;
            rect.SetData(data);

            Vector2 coor = new Vector2(50, 500);

            if (CharacterAnimation != null)
            {
                CharacterAnimation.Draw(spriteBatch, graphics);
            }

            spriteBatch.Draw(rect, coor, Color.White);

            if (CharacterAnimation != null)
            {
                spriteBatch.DrawString(Arial, CharacterAnimation.CharacterName.Substring(0, 1).ToUpper() + CharacterAnimation.CharacterName.Substring(1),
                topLeftofDialogue, tintColor);
            }

            //TextBox graphics is only required until we have a textbox graphic
            TextBox.Draw(spriteBatch, graphics);
        }
    }
}
