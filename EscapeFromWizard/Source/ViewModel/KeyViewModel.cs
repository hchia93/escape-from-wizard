using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeFromWizard.ViewModel
{
    public class KeyViewModel
    {
        private Texture2D m_PreLootTexture;
        private Texture2D m_PostLootTexture;
        private bool m_IsLooted;
        private Vector2 m_Position;
        private Rectangle m_SourceRectangle;

        public KeyViewModel()
        {
            m_IsLooted = false;
            m_Position = Vector2.Zero;
            m_SourceRectangle = Rectangle.Empty;
        }

        public void SetPreLootTexture(Texture2D texture)
        {
            m_PreLootTexture = texture;
        }

        public void SetPostLootTexture(Texture2D texture)
        {
            m_PostLootTexture = texture;
        }

        public void SetPosition(Vector2 position)
        {
            m_Position = position;
        }

        public void SetSourceRectangle(Rectangle sourceRect)
        {
            m_SourceRectangle = sourceRect;
        }

        public void SetLooted(bool isLooted)
        {
            m_IsLooted = isLooted;
        }

        public void DrawView(SpriteBatch spriteBatch)
        {
            if (m_IsLooted)
            {
                if (m_PostLootTexture != null)
                {
                    spriteBatch.Draw(m_PostLootTexture, m_Position, m_SourceRectangle, Microsoft.Xna.Framework.Color.White);
                }
            }
            else
            {
                if (m_PreLootTexture != null)
                {
                    spriteBatch.Draw(m_PreLootTexture, m_Position, m_SourceRectangle, Microsoft.Xna.Framework.Color.White);
                }
            }
        }
    }
} 