using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeFromWizard.ViewModel
{
    public class KeyViewModel
    {
        private Texture2D m_SourceTexture;
        private bool m_IsLooted;
        private Vector2 m_Position;
        private Rectangle m_PreLootedRectangle;
        private Rectangle m_PostLootedRectangle;
        public KeyViewModel()
        {
            m_IsLooted = false;
            m_Position = Vector2.Zero;
            m_PreLootedRectangle = Rectangle.Empty;
            m_PostLootedRectangle = Rectangle.Empty;
        }

        public void SetSourceTexture(Texture2D texture)
        {
            m_SourceTexture = texture;
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_Position = position;
        }

        public void SetPreLootedRectangle(Rectangle rectangle)
        {
            m_PreLootedRectangle = rectangle;
        }

        public void SetPostLootedRectangle(Rectangle rectangle)
        {
            m_PostLootedRectangle = rectangle;
        }

        public void SetLooted(bool isLooted)
        {
            m_IsLooted = isLooted;
        }

        public void DrawView(SpriteBatch spriteBatch)
        {
            if ( m_SourceTexture != null)
            {
                if (m_IsLooted)
                {
                    spriteBatch.Draw(m_SourceTexture, m_Position, m_PostLootedRectangle, Microsoft.Xna.Framework.Color.White);  
                }
                else
                {
                    spriteBatch.Draw(m_SourceTexture, m_Position, m_PreLootedRectangle, Microsoft.Xna.Framework.Color.White);
                }
            }
            
        }
    }
} 