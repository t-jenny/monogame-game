using System;
using GameDemo.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Dialogue
{
    public class TextBox
    {
        private SpriteFont arial;
        private TextBoxAnimation textBoxAnimation;
        public TextBoxAnimation currentTextBoxAnimation;
        private SoundEffect soundEffect;
        private String previousString;

        public TextBox(ContentManager content, String dialogue)
        {
            textBoxAnimation = new TextBoxAnimation()
            {
                charArray = dialogue.ToCharArray()
            };
            arial = content.Load<SpriteFont>("Fonts/Arial");
            textBoxAnimation.appearRate = TimeSpan.FromSeconds(.1);
            soundEffect = content.Load<SoundEffect>("Audio/sfx/sfx-blipmale");
            previousString = "";
        }

        public void Update(GameTime gameTime)
        {
            currentTextBoxAnimation = textBoxAnimation;
            currentTextBoxAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 topLeftofDialogue = new Vector2(200, 600);
            Color tintColor = Color.White;
            String currentString = currentTextBoxAnimation.currentString;


            if (!previousString.Equals(currentString) && !currentString.EndsWith(" "))
            {
                soundEffect.Play();
                previousString = currentString;
            }

            Texture2D rect = new Texture2D(graphics.GraphicsDevice, 1000, 200);

            Color[] data = new Color[1000 * 200];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Black;
            rect.SetData(data);

            Vector2 coor = new Vector2(150, 550);
            spriteBatch.Draw(rect, coor, Color.White);
            spriteBatch.DrawString(arial, currentString, topLeftofDialogue, tintColor);

        }
    }
}
