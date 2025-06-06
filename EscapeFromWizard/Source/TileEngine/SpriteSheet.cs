using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    public class SpriteSheet
    {
        static public int m_WidthPx = 32;
        static public int m_HeightPx = 32;
        public Texture2D m_Texture;
        static public Vector2 m_Origin = new Vector2(0, 0);

        public Rectangle GetSourceRectangle(int tileIndex)
        {
            int tileY = tileIndex / (m_Texture.Width / m_WidthPx); 
            int tileX = tileIndex % (m_Texture.Width / m_WidthPx);  

            return new Rectangle(tileX * m_WidthPx, tileY * m_HeightPx, m_WidthPx, m_HeightPx);
        }
    }
}
