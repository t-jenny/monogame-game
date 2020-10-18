using System;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Dialogue;
using GameDemo.Locations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Scene
{
    public class SceneManager
    {
        ContentManager content;
        TxtReader txtReader;
        GraphicsDeviceManager graphics;
        Background background;
        Character character1;
        TextBox textBox;
        NoLoopSet noLoopSet;
        ButtonState previousMouseState;
        private const String BACKGROUND = "//";
        bool endLine;
        private SpriteFont arial;
        String[] dialogue;

        public SceneManager(ContentManager content, GraphicsDeviceManager graphics, TxtReader txtReader)
        {
            this.content = content;
            this.txtReader = txtReader;
            this.graphics = graphics;
            this.noLoopSet = new NoLoopSet();
            this.endLine = false;
            this.arial = content.Load<SpriteFont>("Fonts/Arial"); 
        }

        public void Update(GameTime gameTime)
        {
            if (background == null ||(endLine &&
                (previousMouseState == ButtonState.Released &&
                Mouse.GetState().LeftButton == ButtonState.Pressed)))
            {
                if (txtReader.currentLine().Contains(BACKGROUND))
                {
                    background = new Background(content, txtReader.getLocation());
                    txtReader.nextLine();
                }

                dialogue = txtReader.getDialogue();
                character1 = new Character(content, dialogue, noLoopSet);
                textBox = new TextBox(content, dialogue[0]);
                endLine = false;
                txtReader.nextLine();
            }

            character1.Update(gameTime);
            textBox.Update(gameTime);

            previousMouseState = Mouse.GetState().LeftButton;
            endLine = character1.endOfLine(textBox);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);
            character1.Draw(spriteBatch);

            Vector2 topLeftofDialogue = new Vector2(175, 505);
            Color tintColor = Color.White;

            Texture2D rect = new Texture2D(graphics.GraphicsDevice, 200, 50);

            Color[] data = new Color[200 * 50];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Black;
            rect.SetData(data);

            Vector2 coor = new Vector2(150, 500);
            spriteBatch.Draw(rect, coor, Color.White);
            spriteBatch.DrawString(arial, dialogue[1].Substring(0, 1).ToUpper() + dialogue[1].Substring(1), topLeftofDialogue, tintColor);

            textBox.Draw(spriteBatch, graphics);
        }
    }
}
