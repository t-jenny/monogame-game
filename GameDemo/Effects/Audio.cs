using System;
using GameDemo.Dialogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GameDemo.Effects
{
    public class Audio : ITextObject
    {
        private const int AUDIO_TYPE_INDEX = 0;

        public Audio(ContentManager content, String audioTitle)
        {
            String[] AudioInfo = audioTitle.Split("/");
            String path = "Audio/" + audioTitle;

            if (AudioInfo[AUDIO_TYPE_INDEX].Equals("sfx"))
            {
                SoundEffect SoundEffect = content.Load<SoundEffect>(path);
                SoundEffect.Play();
            } else
            {
                Song Song = content.Load<Song>("Audio/" + audioTitle);
                MediaPlayer.Play(Song);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {

        }
    }
}
