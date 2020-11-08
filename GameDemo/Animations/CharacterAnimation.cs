using System;
using System.Collections.Generic;
using GameDemo.Dialogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Animations
{
    public class CharacterAnimation : ITextObject
    {
        private const float LAYER_DEPTH = 0.1f;
        private const int NAME_INDEX = 1;
        private const int DIRECTION_INDEX = 0;
        private const String LEFT = "l";
        private const String RIGHT = "r";

        private AnimationSets AnimationSet = new AnimationSets();
        private String Direction;
        private Animation CurrentAnimation;

        public bool Desaturated { get; private set; }
        public String CharacterName { get; private set; }
        public Animation Animation { get; private set; }

        public Texture2D CharacterSheet;

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

        public CharacterAnimation(ContentManager content, String character, String animation)
        {
            CharacterName = character.Substring(NAME_INDEX);
            Direction = character.Substring(DIRECTION_INDEX, NAME_INDEX);
            String AnimationName = animation;

            if (AnimationName == null) {
                AnimationName = AnimationSet.CharacterDefault[CharacterName];
            }

            String FilePath = "Characters/" + character + "/" + AnimationName;

            if (CharacterSheet == null || !CharacterSheet.ToString().Equals(FilePath))
            {
                CharacterSheet = content.Load<Texture2D>(FilePath);
            }

            Animation = new Animation();

            if (!AnimationSet.NoLoop.Contains(AnimationName))
            {
                Animation.IsLooping = true;
            }

            Dictionary<String, Rectangle> AnimationMap =
                content.Load<Dictionary<String, Rectangle>>("Characters/" + character + "/" + AnimationName + "Map");

            foreach (KeyValuePair<String, Rectangle> Entry in AnimationMap)
            {
                Animation.AddFrame(Entry.Value, TimeSpan.FromSeconds(.15));
            }

            Desaturated = false;
        }

        public void Update(GameTime gameTime)
        {
            CurrentAnimation = Animation;
            CurrentAnimation.Update(gameTime);
        }

        public void Desaturate(GraphicsDeviceManager graphics, double percentage)
        {
            Color[] data = new Color[CharacterSheet.Width * CharacterSheet.Height];
            CharacterSheet.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                double f = percentage;
                double L = 0.3 * data[i].R + 0.6 * data[i].G + 0.1 * data[i].B;
                data[i].R = Convert.ToByte(data[i].R + f * (L - data[i].R));
                data[i].G = Convert.ToByte(data[i].G + f * (L - data[i].G));
                data[i].B = Convert.ToByte(data[i].B + f * (L - data[i].B));
            }

            CharacterSheet = new Texture2D(graphics.GraphicsDevice, CharacterSheet.Width, CharacterSheet.Height);
            CharacterSheet.SetData(data);
            Desaturated = true;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (Direction.Equals(LEFT))
            {
                this.X = -200;
            }
            else if (Direction.Equals(RIGHT))
            {
                this.X = 300;
            }
            this.Y = 50;

            Vector2 TopLeftOfSprite = new Vector2(this.X, this.Y);
            Color TintColor = Color.White;
            var SourceRectangle = CurrentAnimation.CurrentRectangle;

            spriteBatch.Draw(CharacterSheet, TopLeftOfSprite, SourceRectangle,
                TintColor, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, LAYER_DEPTH);
        }
    }
}
