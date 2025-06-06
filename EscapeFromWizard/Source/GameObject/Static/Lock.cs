using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Lock
    {
        Vector2 m_Position;
        private Color m_Color;
        private bool m_IsUnlocked;
        private bool m_IsDestroyed;

        public Lock()
        {
            m_IsUnlocked = false;
            m_IsDestroyed = false;
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

        public bool IsUnlocked()
        {
            return m_IsUnlocked;
        }

        public void SetUnlocked(bool value)
        {
            m_IsUnlocked = value;
        }

        //When destroyedFlag is true, the lock will not be drawn in main loop
        public void SetDestroyed(bool isDestroyed)
        {
            m_IsDestroyed = isDestroyed;
        }

        public bool IsDestroyed()
        {
            return m_IsDestroyed;
        }
    }
}
