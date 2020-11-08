using System;
using System.Collections.Generic;
using GameDemo.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDemo.Managers
{
    public class GameplayManager
    {
        /**
        private string CurrentGamplay = "event";
        private IManager CurrentManager;
        private Dictionary<string, IManager> GameplayDictionary;

        public GameplayManager(EventManager eventManager)
        {
            GameplayDictionary.Add("event", eventManager);
            //GameplayDictionary.Add("schedule, scheduleManager);
            //GameplayDictionary.Add("question, questionManager);
        }

        public void Update(GameTime gameTime)
        {
            if (GameplayDictionary.ContainsKey(CurrentGamplay))
            {
                CurrentManager = GameplayDictionary[CurrentGamplay];
                CurrentManager.Update(gameTime);
            }
            else
            {
                throw new ArgumentException(
                    String.Format("%s is not a valid gameplay type.", CurrentGamplay));
            }

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            CurrentManager.Draw(spriteBatch);
        }
        **/
    }
}
