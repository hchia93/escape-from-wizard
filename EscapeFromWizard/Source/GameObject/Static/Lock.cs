namespace EscapeFromWizard.Source.GameObject.Static
{
    public class Lock
    {
        private int m_lockTilePositionX;
        private int m_lockTilePositionY;
        private KeyLockColor m_lockColour;
        private bool unlockedFlag;
        private bool destroyedFlag;

        public Lock()
        {
            unlockedFlag = false;
            destroyedFlag = false;
        }

        public Lock(int i_TileColumn, int i_TileRow)
        {
            unlockedFlag = false;
            m_lockTilePositionX = i_TileColumn;
            m_lockTilePositionY = i_TileRow;
        }

        public void SetLockTilePosition(int i_TileColumn, int i_TileRow)
        {
            m_lockTilePositionX = i_TileColumn;
            m_lockTilePositionY = i_TileRow;
        }

        public int GetLockTilePositionX()
        {
            return m_lockTilePositionX;
        }

        public int GetLockTilePositionY()
        {
            return m_lockTilePositionY;
        }

        public void SetLockColour(KeyLockColor i_lockColour)
        {
            m_lockColour = i_lockColour;
        }

        public int GetLockColourIndex()
        {
            return (int)m_lockColour;
        }

        public bool IsUnlocked(){
            return unlockedFlag;
        }

        public void SetUnlocked()
        {
            unlockedFlag = true;
        }

        public void CheckIfDoorLockIsUnlocked(bool i_keyLootFlag){
            unlockedFlag = i_keyLootFlag;
        }

        //When destroyedFlag is true, the lock will not be drawn in main loop
        public void SetDestroyed(bool i_lockDestroyed)
        {
            destroyedFlag = i_lockDestroyed;
        }

        public bool IsDestroyed()
        {
            return destroyedFlag;
        }

        
    }
}
