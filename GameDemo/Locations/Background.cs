using System;
using GameDemo.Dialogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Locations
{
    public class Background : ITextObject
    {
        static Texture2D Image;
        const float LAYER_DEPTH = 0.0f;
        const float ORIGIN = 0.0f;

        public Background(ContentManager content, String location)
        {
            if (Image == null)
            {
                Image = content.Load<Texture2D>("Locations/phoenix/" + location);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            Vector2 TopLeftOfSprite = new Vector2(ORIGIN, ORIGIN);
            Color TintColor = Color.White;

            spriteBatch.Draw(Image, TopLeftOfSprite, null,
                TintColor, 0.0f, Vector2.Zero, new Vector2(5), SpriteEffects.None, LAYER_DEPTH);
        }
    }
}
