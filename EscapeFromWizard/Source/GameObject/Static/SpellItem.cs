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

        public void SetItemType(SpellItems i_itemType)
        {
            m_Type = i_itemType;
        }

        public bool isLooted()
        {
            return m_IsItemLooted;
        }

        public void SetLooted(bool i_itemLootFlag)
        {
            m_IsItemLooted = i_itemLootFlag;
        }

        public bool CheckPlayerPos(Vector2 i_playerPosVector)
        {
            /* 
             Return True if item Position matched Player Position, else return false;
             */
            if (m_PositionX == (int)i_playerPosVector.X && m_PositionY == (int)i_playerPosVector.Y)
                if (!m_IsItemLooted)
                {
                    m_IsItemLooted = true;
                    return true;
                }
                    
            return false;
        }
    }
}
