using EscapeFromWizard.Map;
using EscapeFromWizard.Source.GameObject.Static;
using EscapeFromWizard.Source.Interface;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace EscapeFromWizard.Source.GameObject.Dynamic
{
    public enum PlayerDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        STILL
    };

    public class PlayerInventory : IReset
    {
        private int m_Stars;
        private int m_QuestItems;
        private List<Key> m_Keys;

        public void Reset()
        {
            m_Stars = 0;
            m_QuestItems = 0;
            m_Keys.Clear();
        }
    }

    public class Player
    {
        private PlayerDirection m_Direction;
        private Vector2 m_Position;
        private Vector2 m_PositionLastFrame;
        private Vector2 m_PositionBeforeHideout;
        private Vector2 m_PositionBeforeExit;
       
        private double m_MoveDelayTimer;
        private double m_StandDelayTimer;
        private double m_HideDelayTimer;

        //Player State
        private bool m_IsHiding;
        private bool m_IsOnExit;
        private bool m_pickUpSomething;
        private bool m_hpIncreased;

        private Level m_referenceMapData;
        private Lock[] doorLock;

        private int m_HP;
        private const int m_MaxHP = 8;

        PlayerInventory m_Inventory;

        private int m_questItemOnHand;
        private int m_maxQuestItem = 3;

        private int m_CollectedStar;
        private int m_maxStar = 15;

        public Player()
        {
            m_IsHiding = true;
            m_IsOnExit = false;
            m_Direction = PlayerDirection.STILL;
            m_MoveDelayTimer = 0.0f;
            m_StandDelayTimer = 0.0f;
            m_HideDelayTimer = 0.0f;
            m_HP = m_MaxHP;
            m_questItemOnHand = 0;
            m_CollectedStar = 0;
        }
        void Reset()
        {
            m_Direction = PlayerDirection.STILL;
        }

        public void SetMapReference(Level level)
        {
            m_referenceMapData = level;
        }

        public void SetUpLockInformation(Lock[] i_doorLock)
        {
            doorLock = i_doorLock;
        }

        public void SetPosition(int column, int row)
        {
            m_Position = new Vector2(column, row);
        }

        public Vector2 GetPosition()
        {
            return m_Position;
        }

        public PlayerDirection GetMovingDirection()
        {
            return m_Direction;
        }

        public void ProcessMovement(Vector2 movement)
        {
            if (movement.X == 0 && movement.Y == 0)
            {
                m_Direction = PlayerDirection.STILL;
            }

            if (movement.X > 0)
            {
                m_Direction = PlayerDirection.RIGHT;
            }

            if (movement.X < 0)
            {
                m_Direction = PlayerDirection.LEFT;
            }

            if (movement.Y > 0)
            {
                m_Direction = PlayerDirection.DOWN;
            }

            if (movement.Y < 0)
            {
                m_Direction = PlayerDirection.UP;
            }
        }

        public bool IsHiding()
        {
            return m_IsHiding;
        }

        public bool IsOnExit()
        {
            return m_IsOnExit;
        }

        public void SetOnExit(bool value)
        {
            m_IsOnExit = value;
        }

        public void RevertPositionOnIncompleteQuest()
        {
            SetPosition((int)m_PositionBeforeExit.X, (int)m_PositionBeforeExit.Y);
        }

        public bool IsLootedSomething()
        {
            return m_pickUpSomething;
        }

        public bool IsStanding()
        {
            //if player didn't move at all.
            return m_Position == m_PositionLastFrame;
        }

        public void SetPickUpFlag(bool value)
        {
            m_pickUpSomething = value;
        }

        //----------------------------------------------------------------------
        // HP Functions
        //----------------------------------------------------------------------

        public void TakeDamage(int value)
        {
            m_HP -= value;
            if (m_HP <= 0)
            {
                m_HP = 0;
            }
        }

        public void Heal(int value)
        {
            m_HP += value;
            if ( m_HP >= m_MaxHP)
            {
                m_HP = m_MaxHP;
            }
        }

        public int GetHP()
        {
            return m_HP;
        }

        public int GetMaxHP()
        {
            return m_MaxHP;
        }

        //----------------------------------------------------------------------
        // Checking Num Of Quest Item And Star On Hand Functions
        //----------------------------------------------------------------------

        public void LootQuestItem()
        {
            m_questItemOnHand += 1;
        }

        public int GetCurrentNumOfQuestItem()
        {
            return m_questItemOnHand;
        }

        public int GetMaxNumOfQuestItem()
        {
            return m_maxQuestItem;
        }

        public void LootStar()
        {
            m_CollectedStar += 1;
        }

        public int GetCurrentNumOfStar()
        {
            return m_CollectedStar;
        }

        public int GetMaxNumOfStar()
        {
            return m_maxStar;
        }

        //----------------------------------------------------------------------
        // Movement & Checking Functions
        //----------------------------------------------------------------------
        
        private void _CheckCurrentTile(GameTime gameTime)
        {
            int maxRow = m_referenceMapData.GetMapTileHeight();
            int maxColumn = m_referenceMapData.GetMapTileWidth();
            int tileID_CurrentTIle = m_referenceMapData.GetMapTileData(m_referenceMapData.ConvertToMapIdex((int)m_Position.Y, (int)m_Position.X, maxRow, maxColumn));

            if (tileID_CurrentTIle == (int)TileType.PATH)
            {
                m_IsHiding = false;
                m_HideDelayTimer = 0.0f;
            }

            if (tileID_CurrentTIle == (int)TileType.EXIT_SIGN)
            {
                m_IsOnExit = true;
            }

            if (tileID_CurrentTIle == (int)TileType.HIDE_TILE)
            {
                m_HideDelayTimer += (gameTime.ElapsedGameTime.TotalSeconds * 7);
                if (m_HideDelayTimer <= 5.0f)
                {
                    m_IsHiding = true;
                }
                else
                {
                    m_HideDelayTimer = 0.0f;
                    m_IsHiding = false;
                    SetPosition((int)m_PositionBeforeHideout.X, (int)m_PositionBeforeHideout.Y);

                }
            }
            
        }

        private bool _IsMoveToNextTilePossible() 
        {

            int _totalRow = m_referenceMapData.GetMapTileHeight();
            int _totalCol = m_referenceMapData.GetMapTileWidth();
            int _NextTileTileID = 0;

            int _nextTileX = 0;
            int _nextTileY = 0;

            switch (m_Direction)
            {
                case PlayerDirection.UP:
                    _nextTileY = (int) m_Position.Y - 1;
                    _nextTileX = (int) m_Position.X;
                    break;
                case PlayerDirection.DOWN:
                    _nextTileY = (int) m_Position.Y + 1;
                    _nextTileX = (int) m_Position.X;
                    break;
                case PlayerDirection.LEFT:
                    _nextTileY = (int) m_Position.Y;
                    _nextTileX = (int) m_Position.X - 1;
                    break;
                case PlayerDirection.RIGHT:
                    _nextTileY = (int) m_Position.Y;
                    _nextTileX = (int) m_Position.X + 1;
                    break;
            }

            _NextTileTileID = m_referenceMapData.GetMapTileData(m_referenceMapData.ConvertToMapIdex(_nextTileY,_nextTileX, _totalRow, _totalCol));

            //Lock control
            if (checkIfPlayerIsNextToLock(_nextTileX,_nextTileY))
            {
                if (_NextTileTileID == (int)TileType.PATH)
                {
                    return true;
                }
                else if (_NextTileTileID == (int)TileType.HIDE_TILE)
                {
                    m_PositionBeforeHideout = m_Position;
                    return true;
                }
                else if (_NextTileTileID == (int)TileType.EXIT_SIGN)
                {
                    m_PositionBeforeExit = m_Position;
                    return true;
                }
                else
                    return false;
            }
            
            return false;
        }

        public void UpdateMovement(GameTime gameTime)
        {
            m_MoveDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_StandDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
           
            //Set Previous Tile Value with Delay
            if(m_StandDelayTimer >= 0.1f)
            {
                m_PositionLastFrame = m_Position;
                m_StandDelayTimer = 0.0f;
            }

            //Action with Delay
            if (m_MoveDelayTimer >= 0.1f) 
            {
                switch (m_Direction)
                {
                    case PlayerDirection.RIGHT:
                        if (_IsMoveToNextTilePossible())
                            m_Position.X += 1;
                        break;
                    case PlayerDirection.LEFT:
                        if (_IsMoveToNextTilePossible())
                            m_Position.X -= 1;
                        break;
                    case PlayerDirection.DOWN:
                        if (_IsMoveToNextTilePossible())
                            m_Position.Y += 1;
                        break;
                    case PlayerDirection.UP:
                        if (_IsMoveToNextTilePossible())
                            m_Position.Y -= 1;
                        break;
                }
                //ResetTimer
                m_MoveDelayTimer = 0;
                _CheckCurrentTile(gameTime);
            }
        }

        //-------------------------------------------------------------
        // Check If Looted Key
        //-------------------------------------------------------------

        private bool checkIfPlayerIsNextToLock(int playerNextPosX, int playerNextPosY)
        {
            for (int i = 0; i < doorLock.Length; i++)
            {
                //if there is a lock beside player
                if (doorLock[i].GetPosition() == new Vector2(playerNextPosX, playerNextPosY))
                {
                    //if the lock is unlocked
                    if (doorLock[i].IsUnlocked())
                    {
                        //can walk
                        doorLock[i].SetDestroyed(true);
                        return true;
                    }
                    else
                    {
                        //cant walk
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
