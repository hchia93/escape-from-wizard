using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace EscapeFromWizard.Source
{
    public struct ProcessedInput
    {
        public Vector2 MovementOffset;
        public bool IsEscapePressed;
        public bool IsEnterPressed;
        public bool IsMouseClicked;
    }

    public class GameInput : IDisposable
    {
        private KeyboardState m_CurrentKeyboardState;
        private KeyboardState m_PreviousKeyboardState;
        private MouseState m_CurrentMouseState;
        private MouseState m_PreviousMouseState;

        // Callback bindings
        public Action OnF1Pressed { get; set; }
        public Action OnF2Pressed { get; set; }
        public Action OnF3Pressed { get; set; }
        public Action OnEscapePressed { get; set; }
        public Action OnEnterPressed { get; set; }
        public Action OnMouseClicked { get; set; }

        public GameInput()
        {
            m_CurrentKeyboardState = Keyboard.GetState();
            m_PreviousKeyboardState = m_CurrentKeyboardState;
            m_CurrentMouseState = Mouse.GetState();
            m_PreviousMouseState = m_CurrentMouseState;
        }

        public void Update(GameTime gameTime)
        {
            // Store previous states
            m_PreviousKeyboardState = m_CurrentKeyboardState;
            m_PreviousMouseState = m_CurrentMouseState;

            // Get current states
            m_CurrentKeyboardState = Keyboard.GetState();
            m_CurrentMouseState = Mouse.GetState();

            // Process callback bindings (only trigger on key press, not hold)
            if (m_CurrentKeyboardState.IsKeyDown(Keys.F1) && !m_PreviousKeyboardState.IsKeyDown(Keys.F1))
            {
                OnF1Pressed?.Invoke();
            }

            if (m_CurrentKeyboardState.IsKeyDown(Keys.F2) && !m_PreviousKeyboardState.IsKeyDown(Keys.F2))
            {
                OnF2Pressed?.Invoke();
            }

            if (m_CurrentKeyboardState.IsKeyDown(Keys.F3) && !m_PreviousKeyboardState.IsKeyDown(Keys.F3))
            {
                OnF3Pressed?.Invoke();
            }

            if (m_CurrentKeyboardState.IsKeyDown(Keys.Escape) && !m_PreviousKeyboardState.IsKeyDown(Keys.Escape))
            {
                OnEscapePressed?.Invoke();
            }

            if (m_CurrentKeyboardState.IsKeyDown(Keys.Enter) && !m_PreviousKeyboardState.IsKeyDown(Keys.Enter))
            {
                OnEnterPressed?.Invoke();
            }

            if (m_CurrentMouseState.LeftButton == ButtonState.Pressed && m_PreviousMouseState.LeftButton == ButtonState.Released)
            {
                OnMouseClicked?.Invoke();
            }
        }

        public ProcessedInput GetProcessedInput()
        {
            ProcessedInput input = new ProcessedInput();

            // Process movement input (WASD and Arrow keys)
            int yDown = (m_CurrentKeyboardState.IsKeyDown(Keys.S) || m_CurrentKeyboardState.IsKeyDown(Keys.Down)) ? -1 : 0;
            int yUp = (m_CurrentKeyboardState.IsKeyDown(Keys.W) || m_CurrentKeyboardState.IsKeyDown(Keys.Up)) ? 1 : 0;
            int xLeft = (m_CurrentKeyboardState.IsKeyDown(Keys.A) || m_CurrentKeyboardState.IsKeyDown(Keys.Left)) ? -1 : 0;
            int xRight = (m_CurrentKeyboardState.IsKeyDown(Keys.D) || m_CurrentKeyboardState.IsKeyDown(Keys.Right)) ? 1 : 0;

            int finalVertical = yDown + yUp;
            int finalHorizontal = xLeft + xRight;

            input.MovementOffset = new Vector2(finalHorizontal, -finalVertical);

            // Process other input states
            input.IsEscapePressed = m_CurrentKeyboardState.IsKeyDown(Keys.Escape);
            input.IsEnterPressed = m_CurrentKeyboardState.IsKeyDown(Keys.Enter);
            input.IsMouseClicked = m_CurrentMouseState.LeftButton == ButtonState.Pressed;

            return input;
        }

        public bool IsKeyPressed(Keys key)
        {
            return m_CurrentKeyboardState.IsKeyDown(key) && !m_PreviousKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return m_CurrentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return !m_CurrentKeyboardState.IsKeyDown(key);
        }

        public KeyboardState GetCurrentKeyboardState()
        {
            return m_CurrentKeyboardState;
        }

        public KeyboardState GetPreviousKeyboardState()
        {
            return m_PreviousKeyboardState;
        }

        public MouseState GetCurrentMouseState()
        {
            return m_CurrentMouseState;
        }

        public MouseState GetPreviousMouseState()
        {
            return m_PreviousMouseState;
        }

        public void ClearCallbacks()
        {
            OnF1Pressed = null;
            OnF2Pressed = null;
            OnF3Pressed = null;
            OnEscapePressed = null;
            OnEnterPressed = null;
            OnMouseClicked = null;
        }

        public void Dispose()
        {
            // Clear all callback references to prevent memory leaks
            ClearCallbacks();
        }
    }
} 