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
        private SpriteFont Arial;
        private TextBoxAnimation TextBoxAnimation;
        public TextBoxAnimation CurrentTextBoxAnimation;
        private SoundEffect SoundEffect;
        private String PreviousString;

        public TextBox(ContentManager content, String dialogue)
        {
            TextBoxAnimation = new TextBoxAnimation()
            {
                CharArray = dialogue.ToCharArray()
            };
            Arial = content.Load<SpriteFont>("Fonts/Arial");
            TextBoxAnimation.AppearRate = TimeSpan.FromSeconds(.1);
            SoundEffect = content.Load<SoundEffect>("Audio/sfx/sfx-blipmale");
            PreviousString = "";
        }

        public void Update(GameTime gameTime)
        {
            CurrentTextBoxAnimation = TextBoxAnimation;
            CurrentTextBoxAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 TopLeftofDialogue = new Vector2(100, 600);
            Color TintColor = Color.White;
            String CurrentString = CurrentTextBoxAnimation.CurrentString;

            if (!PreviousString.Equals(CurrentString) && !CurrentString.EndsWith(" "))
            {
                SoundEffect.Play();
                PreviousString = CurrentString;
            }

            Texture2D Rect = new Texture2D(graphics.GraphicsDevice, 1175, 200);

            Color[] Data = new Color[1175 * 200];
            for (int i = 0; i < Data.Length; ++i) Data[i] = Color.Black;
            Rect.SetData(Data);

            Vector2 Coor = new Vector2(50, 550);
            spriteBatch.Draw(Rect, Coor, Color.White);
            spriteBatch.DrawString(Arial, CurrentString, TopLeftofDialogue, TintColor);
        }
    }
}
