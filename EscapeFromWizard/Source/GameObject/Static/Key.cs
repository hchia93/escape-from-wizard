using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.Interface;
using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Key : ICollectibles
    {
        Vector2 m_Position;
        Color m_Color;
        bool m_IsLooted;

        public Key()
        {
            m_IsLooted = false;
        }

        public void SetPosition(int column, int row)
        {
            m_Position = new Vector2(column, row);
        }

        public Vector2 GetPosition()
        {
            return m_Position;
        }

        public void SetColor(Color color)
        {
            m_Color = color;
        }

        public Color GetColor()
        {
            return m_Color;
        }

        public bool IsLooted()
        {
            return m_IsLooted;
        }

        public void SetLooted(bool value)
        {
            this.m_IsLooted = value;
        }

        public bool IsOverlapped(Vector2 playerPosition)
        {
            /// Todo: check precision
            if (m_Position == playerPosition)
            {
                /// function has side effect
                if (!m_IsLooted)
                {
                    m_IsLooted = true;
                    return true;
                }
            }
            return false;
        }

        public void OnCollect(Player player)
        {
            // do something issit
        }
    }
}
