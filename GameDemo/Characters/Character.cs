using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameDemo.Animations;
using Microsoft.Xna.Framework.Input;
using GameDemo.Dialogue;

namespace GameDemo.Characters
{
    public class Character
    {
        static Texture2D characterSheet;
        Animation animation;
        Animation currentAnimation;
        const float LAYER_DEPTH = 0.1f;

        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public Character(ContentManager content, String[] dialogue, NoLoopSet noLoopSet)
        {
            string characterName = dialogue[1];
            string characterAnimation = dialogue[2];
            string filePath = "Characters/" + characterName + "/" + characterAnimation;

            if (characterSheet == null || !characterSheet.ToString().Equals(filePath))
            {
                characterSheet = content.Load<Texture2D>(filePath);
            }

            animation = new Animation();

            if (!noLoopSet.animations.Contains(characterAnimation))
            {
                animation.isLooping = true;
            }

            Dictionary<String, Rectangle> animationMap =
                content.Load<Dictionary<String, Rectangle>>("Characters/" + characterName + "/" + characterAnimation + "Map");

            foreach (KeyValuePair<String, Rectangle> entry in animationMap)
            {
                animation.AddFrame(entry.Value, TimeSpan.FromSeconds(.15));
            }

        }

        public bool endOfLine(TextBox textBox)
        {
            if (animation.timeIntoAnimation.TotalSeconds >
                Math.Max(animation.Duration.TotalSeconds, textBox.currentTextBoxAnimation.Duration.TotalSeconds))
            {
                return true;
            }

            if (animation.fastForward && textBox.currentTextBoxAnimation.fastForward)
            {
                return true;
            }

            return false;
        }

        public void Update(GameTime gameTime)
        {
            currentAnimation = animation;
            currentAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            this.X = 150;
            this.Y = 50;
            Vector2 topLeftOfSprite = new Vector2(this.X, this.Y);
            Color tintColor = Color.White;
            var sourceRectangle = currentAnimation.CurrentRectangle;

            spriteBatch.Draw(characterSheet, topLeftOfSprite, sourceRectangle,
                tintColor, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, LAYER_DEPTH);
        }
    }
}
