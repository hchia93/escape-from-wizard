using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class SpellItem
    {
        int m_PositionX;
        int m_PositionY;
        SpellItems m_Type;
        bool m_IsItemLooted;

        public SpellItem()
        {
            m_IsItemLooted = false;
        }

        public SpellItem(int column, int row)
        {
            m_IsItemLooted = false;
            m_PositionX = column;
            m_PositionY = row;
        }

        public void SetPosition(int column, int row)
        {
            m_PositionX = column;
            m_PositionY = row;
        }

        public int GetItemTilePositionX()
        {
            return m_PositionX;
        }

        public int GetItemTilePositionY()
        {
            return m_PositionY;
        }

        public int GetItemTypeIndex()
        {
            return (int)m_Type;
        }

        public SpellItems GetItemType()
        {
            return m_Type;
        }

        public void SetItemType(SpellItems itemType)
        {
            m_Type = itemType;
        }

        public bool isLooted()
        {
            return m_IsItemLooted;
        }

        public void SetLooted(bool itemLootFlag)
        {
            m_IsItemLooted = itemLootFlag;
        }

        public bool CheckPlayerPos(Vector2 playerPosVector)
        {
            /* 
             Return True if item Position matched Player Position, else return false;
             */
            if (m_PositionX == (int)playerPosVector.X && m_PositionY == (int)playerPosVector.Y)
            {
                if (!m_IsItemLooted)
                {
                    m_IsItemLooted = true;
                    return true;
                }
            }
                    
            return false;
        }
    }
}
