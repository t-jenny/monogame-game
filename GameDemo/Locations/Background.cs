using System;
using GameDemo.Dialogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Locations
{
    public class Background
    {
        static Texture2D background;
        const float LAYER_DEPTH = 0.0f;
        const float ORIGIN = 0.0f;

        public Background(ContentManager content, String location)
        {
            if (background == null)
            {
                background = content.Load<Texture2D>("Locations/phoenix/" + location);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeftOfSprite = new Vector2(ORIGIN, ORIGIN);
            Color tintColor = Color.White;

            spriteBatch.Draw(background, topLeftOfSprite, null,
                tintColor, 0.0f, Vector2.Zero, new Vector2(5), SpriteEffects.None, LAYER_DEPTH);
        }
    }
}
