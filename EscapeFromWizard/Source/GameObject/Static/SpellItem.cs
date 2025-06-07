using Microsoft.Xna.Framework;
using System;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class SpellItem
    {
        Vector2 m_Position;
        SpellItems m_Type;
        bool m_IsLooted;
        
        // Callback for pickup sound
        public Action OnPickedUp { get; set; }

        public SpellItem()
        {
            m_IsLooted = false;
        }

        public SpellItem(int column, int row)
        {
            m_IsLooted = false;
            m_Position = new Vector2(column, row);
        }

        public void SetPosition(int column, int row)
        {
            m_Position = new Vector2(column, row);
        }

        public Vector2 GetPosition()
        {
            return m_Position;
        }

        public int GetItemTypeIndex()
        {
            return (int) m_Type;
        }

        public SpellItems GetItemType()
        {
            return m_Type;
        }

        public void SetItemType(SpellItems itemType)
        {
            m_Type = itemType;
        }

        public bool GetIsLooted()
        {
            return m_IsLooted;
        }

        public void SetLooted(bool itemLootFlag)
        {
            if (m_IsLooted != itemLootFlag)
            {
                m_IsLooted = itemLootFlag;
                
                // Trigger pickup sound when item is collected
                if (m_IsLooted)
                {
                    OnPickedUp?.Invoke();
                }
            }
        }

        public bool CheckPlayerPos(Vector2 playerPosVector)
        {
            /* 
             Return True if item Position matched Player Position, else return false;
             */
            if (m_Position.X == (int) playerPosVector.X && m_Position.Y == (int) playerPosVector.Y)
            {
                if (!m_IsLooted)
                {
                    SetLooted(true);
                    return true;
                }
            }

            return false;
        }
    }
}
