using System;
using System.Collections.Generic;
using GameDemo.Characters;
using GameDemo.Engine;
using GameDemo.Locations;
using GameDemo.Components;
using GameDemo.Managers;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Notebook
{
    public class NotebookManager : IManager
    {
        private MainCharacter MainCharacter;
        private ContentManager Content;

        private const string NotebookPath = "notebook";
        private Background Background;

        private Texture2D ReturnIcon;
        private Rectangle ReturnIconRect;
        private Button QuitButton;
        private ConfirmMenu ConfirmQuitMenu;

        private SpriteFont Arial;

        private MouseState MouseState;
        private MouseState PrevMouseState;

        private NotebookState GState;
        private bool IsTransitioning;

        enum NotebookState
        {
            Returning,
            Stats,
            Testimonies,
            Profiles,
            Locations,
            Options,
            ClickedQuitGame,
            ConfirmedQuitGame
        }

        public void MouseClicked(MouseState mouseState)
        {
            Point MouseClick = new Point(mouseState.X, mouseState.Y);
            Rectangle MouseClickRect = new Rectangle(MouseClick, new Point(50, 50));

            switch (GState)
            {
                case NotebookState.Returning:
                    break;

                case NotebookState.ClickedQuitGame:
                    if (ConfirmQuitMenu.IsConfirming(MouseClickRect))
                    {
                        GState = NotebookState.ConfirmedQuitGame;
                    }
                    if (ConfirmQuitMenu.IsCanceling(MouseClickRect))
                    {
                        GState = NotebookState.Stats; // replace with whichever state exposes the quit button
                    }
                    break;

                default:
                    if (MouseClickRect.Intersects(ReturnIconRect))
                    {
                        GState = NotebookState.Returning;
                    }

                    if (MouseClickRect.Intersects(QuitButton.Rect))
                    {
                        GState = NotebookState.ClickedQuitGame;
                        string Query = "Are you sure you want" + Environment.NewLine + "to quit the game?";
                        ConfirmQuitMenu = new ConfirmMenu(Query, Content);
                    }
                    break;
            }
        }

        public void Reset(GameEngine gameEngine, MainCharacter mainCharacter, ContentManager content)
        {
            content.Unload();

            MainCharacter = mainCharacter;
            Content = content;

            // Visual Elements
            Background = new Background(content, NotebookPath);
            ReturnIcon = Content.Load<Texture2D>("return-icon");
            QuitButton = null;

            // Always start by viewing stats.
            GState = NotebookState.Stats;

            Arial = content.Load<SpriteFont>("Fonts/Arial");

            MouseState = Mouse.GetState();
            PrevMouseState = MouseState;
        }

        public void Update(GameEngine gameEngine, GameTime gameTime)
        {
            if (IsTransitioning) return;
            MouseState = Mouse.GetState();
            if (QuitButton != null) QuitButton.Update();
            if (ConfirmQuitMenu != null) ConfirmQuitMenu.Update();

            if (PrevMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(MouseState);
            }

            PrevMouseState = MouseState;


            if (GState == NotebookState.Returning)
            {
                gameEngine.Pop(true, true);
                IsTransitioning = true;
            }

            if (GState == NotebookState.ConfirmedQuitGame)
            {
                gameEngine.PopAll(true, true);
                IsTransitioning = true;
            }

        }

        public void Draw(GameEngine gameEngine, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            // Background
            Background.Draw(spriteBatch, graphics);

            // Banner with Date
            DateTime CurrentDate = MainCharacter.GetDate();
            String DateString = CurrentDate.ToString("dddd, MMMM dd");
            spriteBatch.DrawString(Arial, DateString, new Vector2(10.0f, 30.0f), Color.Black);
            DrawingUtils.DrawTextBanner(graphics, spriteBatch, Arial, DateString, Color.Red, Color.Black);

            // Return-to-world icon
            if (ReturnIconRect.IsEmpty)
            {
                ReturnIconRect = new Rectangle(graphics.GraphicsDevice.Viewport.Width - 100, 20, 70, 70);
            }
            spriteBatch.Draw(ReturnIcon, ReturnIconRect, Color.White);

            /***** Draw notebook info (all on one page for now) *****/
            Vector2 TextPos = new Vector2(650, 110);

            spriteBatch.DrawString(Arial, MainCharacter.Name, TextPos, Color.Black);
            Point NameSize = Arial.MeasureString(MainCharacter.Name).ToPoint();
            Rectangle NameRect = new Rectangle(TextPos.ToPoint(), NameSize);
            DrawingUtils.DrawUnderline(graphics, spriteBatch, NameRect, Color.Black);

            Vector2 TextOffset = new Vector2(0.0f, Arial.MeasureString("[Your Name]").Y + 5.0f);

            foreach (string Stat in MainCharacter.Stats.Keys)
            {
                TextPos += TextOffset;
                string StatString = Stat + ": " + MainCharacter.Stats[Stat];
                spriteBatch.DrawString(Arial, StatString, TextPos, Color.Black);
            }

            if (QuitButton == null)
            {
                QuitButton = new Button("Quit Game", Arial,
                    (int)TextPos.X + (int) Arial.MeasureString("[Your Name]").X / 2,
                    (int)(TextPos + TextOffset).Y + 5);
            }
            QuitButton.Draw(spriteBatch, graphics);

            /***** End Notebook Page placeholder *****/

            // Confirm Menu if quitting the game
            if (GState == NotebookState.ClickedQuitGame && ConfirmQuitMenu != null)
            {
                ConfirmQuitMenu.Draw(spriteBatch, Arial, graphics);
            }

        }
    }
}