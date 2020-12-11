using System;
using GameDemo.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDemo.Components
{

    public class InputTextField
    {
        private KeyboardState OldKeyboardState;
        private KeyboardState CurrentKeyboardState;
        public string TextString { get; private set; }
        public Rectangle Rect { get; private set; }
        public bool IsActive = false;

        public InputTextField(Rectangle rect)
        {
            Rect = rect;
            TextString = string.Empty;
        }

        // From https://www.gamedev.net/forums/topic/457783-xna-getting-text-from-keyboard/page__p__4040190
        private void UpdateInput()
        {
            OldKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            Keys[] PressedKeys;
            PressedKeys = CurrentKeyboardState.GetPressedKeys();

            foreach (Keys Key in PressedKeys)
            {
                if (OldKeyboardState.IsKeyUp(Key))
                {
                    if (Key == Keys.Back) // backspace key
                        TextString = TextString.Remove(TextString.Length - 1, 1);
                    else
                    if (Key == Keys.Space)
                        TextString = TextString.Insert(TextString.Length, " ");
                    else
                        TextString += Key.ToString();
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            UpdateInput();
            MouseState MouseState = Mouse.GetState();
            Point MousePoint = new Point(MouseState.X, MouseState.Y);

            if (!IsActive && Rect.Contains(MousePoint))
            {
                IsActive = MouseState.LeftButton == ButtonState.Pressed;
            }
            if (IsActive && !Rect.Contains(MousePoint))
            {
                IsActive = !(MouseState.LeftButton == ButtonState.Pressed);
            }
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, SpriteFont font)
        {
            Color FieldColor = IsActive? Color.LightGray : Color.White;
            DrawingUtils.DrawFilledRectangle(graphics, spriteBatch, Rect, FieldColor);
            spriteBatch.DrawString(font, TextString, new Vector2(Rect.X + 5, Rect.Y + 5), Color.Black);
            DrawingUtils.DrawUnderline(graphics, spriteBatch, Rect, Color.Black);
        }

    }

    public class KeyboardInputMenu : PopupMenu
    {
        private InputTextField InputTextField;
        private string TextString;

        public KeyboardInputMenu(string query, ContentManager content):base(content)
        {
            MenuWidth = 600;
            StaticText = query;
            ConfirmButtonText = "Enter";
            CancelButtonText = "Cancel";
            ButtonLabels.Add(CancelButtonText);
            ButtonLabels.Add(ConfirmButtonText);
        }

        public string GetText()
        {
            return InputTextField.TextString;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            InputTextField.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, GraphicsDeviceManager graphics)
        {
            float Padding = 0.1f;
            float LineHeight = font.MeasureString(StaticText).Y;
            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);

            string WrappedText = DrawingUtils.WrappedString(font, StaticText, MenuRect, Padding);
            Vector2 TextSize = font.MeasureString(WrappedText);

            spriteBatch.DrawString(font, WrappedText,
                new Vector2(Position.X + (MenuWidth - TextSize.X) / 2, Position.Y + MenuHeight / 10), Color.Black);

            // Draw Field for input text
            if (InputTextField == null)
            {
                InputTextField = new InputTextField(new Rectangle((int) (Position.X + MenuWidth * Padding),
                    (int) (Position.Y + font.MeasureString(WrappedText).Y + font.LineSpacing),
                    (int) ((1.0f - 2*Padding) * MenuWidth), (int) LineHeight));
            }
            InputTextField.Draw(graphics, spriteBatch, font);

            for (int i = 0; i < ButtonLabels.Count; i++)
            {
                if (i + 1 > Buttons.Count)
                {
                    Buttons.Add(new Button(ButtonLabels[i], font,
                    (int)(Position.X + (i + 1) * MenuWidth / (ButtonLabels.Count + 1)),
                    (int)(Position.Y + TextSize.Y + LineHeight + 2 * MenuHeight / 10)));
                }
                Buttons[i].Draw(spriteBatch, graphics);
            }

        }
    }
}
