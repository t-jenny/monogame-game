using System;
using System.Collections.Generic;
using GameDemo.Characters;
using GameDemo.Effects;
using GameDemo.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GameDemo.Engine
{
    public class GameEngine
    {

        private bool FadeOut;
        private bool FadeOutFinished;
        private bool FadeIn;
        private bool StateChange;
        private Stack<IManager> GameStack;
        private Stack<IManager> PushStack;
        private int StackSize;
        private int PushCount;
        private ScreenFader ScreenFader;

        public GameEngine()
        {
            FadeOut = false;
            FadeOutFinished = false;
            FadeIn = false;

            GameStack = new Stack<IManager>();
            PushStack = new Stack<IManager>();
            StackSize = 0;
            PushCount = 0;
            ScreenFader = new ScreenFader();
        }

        ~GameEngine()
        {
            Console.WriteLine("ModeEngine destroyed");
        }

        public void Update(GameTime gameTime, MainCharacter mainCharacter, ContentManager content)
        {
            if (FadeOut && !ScreenFader.IsFading())
            {
                FadeOut = false;
                FadeOutFinished = true;
            }

            if (FadeOutFinished && StateChange)
            {
                while (PushCount > 0)
                {
                    GameStack.Push(PushStack.Pop());
                    PushCount--;
                    StackSize++;
                }

                while (PushCount < 0)
                {
                    GameStack.Pop();
                    PushCount++;
                    StackSize--;
                }

                GameStack.Peek().Reset(this, mainCharacter, content);
                StateChange = false;

                if (FadeIn)
                {
                    ScreenFader.BeginFade(Color.Transparent, 500);
                }
            }

            if (StackSize == 0)
            {
                Console.WriteLine("Gamestack is empty. Exiting...");
                Game1.QuitGame();
            }
            GameStack.Peek().Update(this, gameTime);

            // Update the screenfader
            ScreenFader.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (StackSize == 0) return;
            GameStack.Peek().Draw(this, spriteBatch, graphics);

            // For fades
            ScreenFader.Draw(spriteBatch, graphics);
        }

        public void Push(IManager mm, bool fadeIn, bool fadeOut)
        {

            StateChange = true;

#if DEBUG
            Console.WriteLine("Pushing IManager of type " + mm.GetType());
#endif

            FadeIn = fadeIn;
            if (fadeOut)
            {
                FadeOut = true;
                FadeOutFinished = false;
                ScreenFader.BeginFade(Color.Black, 200);
            }
            else
            {
                FadeOut = false;
                FadeOutFinished = true;
            }
            PushCount++;
            PushStack.Push(mm);
        }

        public void Pop(bool fadeIn, bool fadeOut)
        {
            FadeIn = fadeIn;
            StateChange = true;

#if DEBUG
            Console.WriteLine("Popping IManager");
#endif

            FadeIn = fadeIn;
            if (fadeOut)
            {
                FadeOut = true;
                FadeOutFinished = false;
                ScreenFader.BeginFade(Color.Black, 200);
            }
            else
            {
                FadeOut = false;
                FadeOutFinished = true;
            }
            PushCount--;
        }
    }
}
