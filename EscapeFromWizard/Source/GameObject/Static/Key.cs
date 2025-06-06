using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.Interface;
using Microsoft.Xna.Framework;
using System;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Key : ICollectibles
    {
        private Vector2 m_Position;
        private Color m_Color;
        private bool m_IsLooted;

        // Event that fires when the key's looted state changes
        public event EventHandler<bool> OnLootedStateChanged;

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
            if (m_IsLooted != value)
            {
                m_IsLooted = value;
                OnLootedStateChanged?.Invoke(this, m_IsLooted);
            }
        }

        public bool IsOverlapped(Vector2 playerPosition)
        {
            /// Todo: check precision
            if (m_Position == playerPosition)
            {
                /// function has side effect
                if (!m_IsLooted)
                {
                    SetLooted(true);
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
