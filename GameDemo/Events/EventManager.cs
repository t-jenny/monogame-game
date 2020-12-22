using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using GameDemo.Animations;
using GameDemo.Characters;
using GameDemo.Components;
using GameDemo.Dialogue;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Events
{
    public class EventManager : IManager
    {
        private MainCharacter MainCharacter;
        private ContentManager Content;
        private EventScript EventScript;
        private bool IsTransitioning;

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            Console.WriteLine("hello");

            MainCharacter = mainCharacter;
            Content = content;

            String path = Path.Combine(Content.RootDirectory, "json-sample.txt");
            String Text = File.ReadAllText(path);
            AllEventDialogue AllEventDialogue = JsonSerializer.Deserialize<AllEventDialogue>(Text);
            EventDialogue eventDialogue = AllEventDialogue.AllEvents["eventString3"][0];

            MainCharacter.EventFlags = new HashSet<string>();
            MainCharacter.EventFlags.Add("beginning");
            MainCharacter.EventFlags.Add("running");

            bool PlayEvent = true;
            IsTransitioning = false;

            //Setting up for event to be true (remove later)
            if (MainCharacter.Relationships.ContainsKey("blush")) {
                MainCharacter.Relationships["blush"] = 1;
            }
            else {
                MainCharacter.Relationships.Add("blush", 1);
            }

            if (MainCharacter.Relationships.ContainsKey("tomazzi"))
            {
                MainCharacter.Relationships["tomazzi"] = 1;
            }
            else
            {
                MainCharacter.Relationships.Add("tomazzi", 1);
            }

            MainCharacter.Stats = new Dictionary<string, int>();
            MainCharacter.Stats.Add("intelligence", 1);
            MainCharacter.Stats.Add("strength", 2);

            //check maincharacter's attributes to see if the event should be played
            foreach (KeyValuePair<string, int> stat in eventDialogue.RequiredStats)
            {
                if (MainCharacter.Stats[stat.Key] < stat.Value)
                {
                    PlayEvent = false;
                }
            }

            foreach (KeyValuePair<string, int> relationship in eventDialogue.RequiredRelationships)
            {
                if (MainCharacter.Relationships[relationship.Key] < relationship.Value)
                {
                    PlayEvent = false;
                }
            }

            if (!eventDialogue.RequiredFlags.IsSubsetOf(MainCharacter.EventFlags))
            {
                PlayEvent = false;
            }

            if (MainCharacter.EventFlags.Contains(eventDialogue.EventName))
            {
                PlayEvent = false;
            }

            if (PlayEvent)
            {
                EventScript = new EventScript(MainCharacter, Content, eventDialogue.Text);
            }
            else
            {
                Console.WriteLine("Cannot play event.");
            }

            //event marked as seen
            MainCharacter.EventFlags.Add(eventDialogue.EventName);

        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning)
            {
                return;
            }
            EventScript.Update(gameTime);

            if (EventScript.IsFinished())
            {
                gameEngine.Pop(true, true);
                IsTransitioning = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            EventScript.Draw(spriteBatch, graphics);
        }
    }
}
