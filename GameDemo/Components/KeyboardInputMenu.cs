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

        // Update pressed keys
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
                    if (Key == Keys.Back && TextString.Length > 0) // backspace
                        TextString = TextString.Remove(TextString.Length - 1, 1);
                    else if (TextString.Length > 16)
                        break;
                    else if (Key == Keys.Space)
                        TextString = TextString.Insert(TextString.Length, " ");
                    else if (Key == Keys.OemQuotes) // apostrophe
                        TextString = TextString.Insert(TextString.Length, "\'");
                    else if (Key.ToString().Length == 1)
                        TextString += Key.ToString();
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (IsActive) UpdateInput();
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

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, SpriteFont font)
        {
            Color FieldColor = IsActive? Color.White : Color.LightGray;
            DrawingUtils.DrawFilledRectangle(spriteBatch, graphics, Rect, FieldColor);
            spriteBatch.DrawString(font, TextString, new Vector2(Rect.X + 5, Rect.Y + 5), Color.Black);
            DrawingUtils.DrawUnderline(spriteBatch, graphics, Rect, Color.Black);
        }

    }

    public class KeyboardInputMenu : PopupMenu
    {
        private InputTextField InputTextField;

        public KeyboardInputMenu(string query, ContentManager content, SpriteFont font):base(content, font)
        {
            // Adjust defaults
            MenuWidth = 600;
            Position = new Vector2(350, 300);

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
            if (InputTextField != null) InputTextField.Update(gameTime);
        }

        // Overrides the entire PopupMenu Draw method due to InputTextField
        public override void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            float Padding = 0.1f;
            float LineHeight = Font.MeasureString(StaticText).Y;
            Rectangle MenuRect = new Rectangle((int)Position.X, (int)Position.Y, MenuWidth, MenuHeight);
            spriteBatch.Draw(Menu, MenuRect, Color.White);

            string WrappedText = DrawingUtils.WrappedString(Font, StaticText, MenuRect, Padding)[0];
            Vector2 TextSize = Font.MeasureString(WrappedText);

            spriteBatch.DrawString(Font, WrappedText,
                new Vector2(Position.X + (MenuWidth - TextSize.X) / 2, Position.Y + MenuHeight / 10), Color.Black);

            // Draw Field for input text
            if (InputTextField == null)
            {
                InputTextField = new InputTextField(new Rectangle((int) (Position.X + MenuWidth * Padding),
                    (int) (Position.Y + Font.MeasureString(WrappedText).Y + Font.LineSpacing),
                    (int) ((1.0f - 2*Padding) * MenuWidth), (int) LineHeight));
            }
            InputTextField.Draw(spriteBatch, graphics, Font);

            for (int i = 0; i < ButtonLabels.Count; i++)
            {
                if (i + 1 > Buttons.Count)
                {
                    Vector2 ButtonTextSize = Font.MeasureString(ButtonLabels[i]);
                    Vector2 CenteredButtonPos = new Vector2(Position.X + (i + 1) * MenuWidth / (ButtonLabels.Count + 1) - ButtonTextSize.X / 2,
                    Position.Y + TextSize.Y + LineHeight + 2 * MenuHeight / 10);
                    Buttons.Add(new Button(ButtonLabels[i], Font, CenteredButtonPos));
                }
                Buttons[i].Draw(spriteBatch, graphics);
            }

        }
    }
}
