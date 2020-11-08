using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Dialogue
{
    public class ChoiceDialogue : ITextObject
    {
        private Dictionary<String[], String[]> Choices;

        public ChoiceDialogue(Dictionary<String[], String[]> choices)
        {
            this.Choices = choices;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {

        }
    }
}
