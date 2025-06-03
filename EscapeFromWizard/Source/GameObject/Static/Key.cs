using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.Interface;
using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Key : ICollectibles
    {
        Vector2 m_Position;
        Color m_Color;
        bool isLooted;

        public Key()
        {
            isLooted = false;
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
            return isLooted;
        }

        public void SetLooted(bool value)
        {
            this.isLooted = value;
        }

        public bool IsOverlapped(Vector2 playerPosition)
        {
            /// Todo: check precision
            if (m_Position == playerPosition)
            {
                /// function has side effect
                if (!isLooted)
                {
                    isLooted = true;
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
