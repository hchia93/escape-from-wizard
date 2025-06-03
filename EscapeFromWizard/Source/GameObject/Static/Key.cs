using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Key
    {
        int m_keyTilePositionX;
        int m_keyTilePositionY;
        KeyLockColor keyColour;
        bool keyLootFlag;

        public Key()
        {
            keyLootFlag = false;
        }

        public Key(int i_TileColumn, int i_TileRow)
        {
            keyLootFlag = false;
            m_keyTilePositionX = i_TileColumn;
            m_keyTilePositionY = i_TileRow;
        }

        public void SetKeyTilePosition(int i_TileColumn, int i_TileRow)
        {
            m_keyTilePositionX = i_TileColumn;
            m_keyTilePositionY = i_TileRow;
        }

        public int GetKeyTilePositionX()
        {
            return m_keyTilePositionX;
        }

        public int GetKeyTilePositionY()
        {
            return m_keyTilePositionY;
        }

        public void SetKeyColour(KeyLockColor i_keyColour)
        {
            keyColour = i_keyColour;
        }

        public int GetKeyColourIndex()
        {
            return (int)keyColour;
        }

        public bool isLooted()
        {
            return keyLootFlag;
        }

        public void SetLooted(bool i_keyLootFlag)
        {
            keyLootFlag = i_keyLootFlag;
        }

        public bool CheckPlayerPos(Vector2 i_playerPosVector)
        {
            /* 
             Return True if Key Position matched Player Position, else return false;
             */
            if (m_keyTilePositionX == (int)i_playerPosVector.X && m_keyTilePositionY == (int)i_playerPosVector.Y)
                if (!keyLootFlag)
                {
                    keyLootFlag = true;
                    return true;
                }
                    
            return false;
        }
    }
}
