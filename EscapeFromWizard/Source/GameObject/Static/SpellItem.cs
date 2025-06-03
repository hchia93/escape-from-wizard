using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class SpellItem
    {
        int m_itemTilePositionX;
        int m_itemTilePositionY;
        SpellItemType m_type;
        bool itemLootFlag;

        public SpellItem()
        {
            itemLootFlag = false;
        }

        public SpellItem(int i_TileColumn, int i_TileRow)
        {
            itemLootFlag = false;
            m_itemTilePositionX = i_TileColumn;
            m_itemTilePositionY = i_TileRow;
        }

        public void SetItemTilePosition(int i_TileColumn, int i_TileRow)
        {
            m_itemTilePositionX = i_TileColumn;
            m_itemTilePositionY = i_TileRow;
        }

        public int GetItemTilePositionX()
        {
            return m_itemTilePositionX;
        }

        public int GetItemTilePositionY()
        {
            return m_itemTilePositionY;
        }

        public int GetItemTypeIndex()
        {
            return (int)m_type;
        }

        public SpellItemType GetItemType()
        {
            return m_type;
        }

        public void SetItemType(SpellItemType i_itemType)
        {
            m_type = i_itemType;
        }

        public bool isLooted()
        {
            return itemLootFlag;
        }

        public void SetLooted(bool i_itemLootFlag)
        {
            itemLootFlag = i_itemLootFlag;
        }

        public bool CheckPlayerPos(Vector2 i_playerPosVector)
        {
            /* 
             Return True if item Position matched Player Position, else return false;
             */
            if (m_itemTilePositionX == (int)i_playerPosVector.X && m_itemTilePositionY == (int)i_playerPosVector.Y)
                if (!itemLootFlag)
                {
                    itemLootFlag = true;
                    return true;
                }
                    
            return false;
        }
    }
}
