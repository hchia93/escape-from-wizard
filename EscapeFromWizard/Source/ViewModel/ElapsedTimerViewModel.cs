using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeFromWizard.Source;
using EscapeFromWizard.Source.Interface;

namespace EscapeFromWizard.ViewModel
{
    public class ElapsedTimerViewModel : IWidget
    {
        private SpriteFont m_Font;
        private Vector2 m_Position;
        private GameStates m_GameStates;

        public ElapsedTimerViewModel(GameStates gameStates)
        {
            m_GameStates = gameStates;
            m_Position = Vector2.Zero;
        }

        public void SetFont(SpriteFont font)
        {
            m_Font = font;
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_Position = position;
        }

        public void DrawWidget(SpriteBatch spriteBatch)
        {
            if (m_Font != null && m_GameStates != null)
            {
                string gameTimeStr = m_GameStates.GetFormattedGameTime();
                Vector2 textSize = m_Font.MeasureString(gameTimeStr);
                Vector2 centeredPosition = new Vector2(m_Position.X - textSize.X / 2, m_Position.Y);
                
                spriteBatch.DrawString(m_Font, gameTimeStr, centeredPosition, Microsoft.Xna.Framework.Color.Black);
            }
        }
    }
} 