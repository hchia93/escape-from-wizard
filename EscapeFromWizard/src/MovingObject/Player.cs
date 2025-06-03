using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DungeonRE.StaticObject;

namespace DungeonRE
{
    public enum PlayerDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        STILL
    };

    public class Player
    {
        private PlayerDirection m_direction;
        private int m_playerTilePositionX;
        private int m_playerTilePositionY;
        private Vector2 m_playerPreviousPosition;
        private Vector2 m_playerPreviousPositionForHideout;
        private Vector2 m_playerPreviousPositionForExit;
       
        private double m_movingDelayTimer;
        private double m_standingDelayTimer;
        private double m_hidingTimer;

        //Player State
        private bool m_onHideTile;
        private bool m_onExitTile;
        private bool m_pickUpSomething;
        private bool m_hpIncreased;

        private MapData m_referenceMapData;
        private Lock[] doorLock;

        private int m_HP;
        private const int maxHP = 8;

        private int m_questItemOnHand;
        private int m_maxQuestItem = 3;

        private int m_starOnHand;
        private int m_maxStar = 15;

        public Player()
        {
            m_onHideTile = true;
            m_onExitTile = false;
            m_direction = PlayerDirection.STILL;
            m_movingDelayTimer = 0.0f;
            m_standingDelayTimer = 0.0f;
            m_hidingTimer = 0.0f;
            m_HP = maxHP;
            m_questItemOnHand = 0;
            m_starOnHand = 0;
        }

        //----------------------------------------------------------------------
        // Initialize
        //----------------------------------------------------------------------

        public void SetMapReference(MapData i_MapData)
        {
            m_referenceMapData = i_MapData;
        }

        public void SetUpLockInformation(Lock[] i_doorLock)
        {
            doorLock = i_doorLock;
        }


        //----------------------------------------------------------------------
        // Positioning Functions
        //----------------------------------------------------------------------

        public void SetTilePosition(int i_TileColumn, int i_TileRow)
        {
            m_playerTilePositionX = i_TileColumn;
            m_playerTilePositionY = i_TileRow;
        }

        public Vector2 GetTilePositionVector()
        {
            return new Vector2(m_playerTilePositionX, m_playerTilePositionY);
        }

        public PlayerDirection GetMovingDirection()
        {
            return m_direction;
        }

        public void ComputeMovingDirection(Vector2 i_Vector)
        {
            if (i_Vector.X == 0 && i_Vector.Y == 0)
                m_direction = PlayerDirection.STILL;

            if (i_Vector.X > 0)
                m_direction = PlayerDirection.RIGHT;

            if (i_Vector.X < 0)
                m_direction = PlayerDirection.LEFT;

            if (i_Vector.Y > 0)
                m_direction = PlayerDirection.DOWN;

            if (i_Vector.Y < 0)
                m_direction = PlayerDirection.UP;
        }


        //----------------------------------------------------------------------
        // State Flag Functions
        //----------------------------------------------------------------------

        public bool IsHiding()
        {
            return m_onHideTile;
        }

        public bool IsOnExit()
        {
            return m_onExitTile;
        }

        public void SetOnExit(bool i_onExitTile)
        {
            m_onExitTile = i_onExitTile;
        }

        public void ResetPositionIfQuestUncompleted()
        {
            SetTilePosition((int)m_playerPreviousPositionForExit.X, (int)m_playerPreviousPositionForExit.Y);
        }

        public bool IsLootedSomething()
        {
            return m_pickUpSomething;
        }

        public bool IsStanding()
        {
            //if player didn't move at all.
            if (m_playerTilePositionX == m_playerPreviousPosition.X && m_playerTilePositionY == m_playerPreviousPosition.Y)
                return true;
            else 
                return false;
        }

        public void SetPickUpFlag(bool i_bool)
        {
            m_pickUpSomething = i_bool;
        }

        //----------------------------------------------------------------------
        // HP Functions
        //----------------------------------------------------------------------

        public void DecreaseHP(int i_decrement)
        {
            if(m_HP > 0)
                m_HP = m_HP - i_decrement;
        }

        public void IncreaseHP(int i_increment)
        {
            for (int i = 0; i < i_increment; i++)
            {
                if (m_HP < maxHP)
                    m_HP = m_HP + 1;
            }
        }

        public void RestoreHP()
        {
            m_HP = maxHP;
        }

        public int GetHP()
        {
            return m_HP;
        }

        public int GetMaxHP()
        {
            return maxHP;
        }

        public bool IsHPIncreased()
        {
            return m_hpIncreased;
        }

        public void SetIsHPIncreased(bool i_hpIncreased)
        {
            m_hpIncreased = i_hpIncreased;
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
            m_starOnHand += 1;
        }

        public int GetCurrentNumOfStar()
        {
            return m_starOnHand;
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
            int _totalRow = m_referenceMapData.GetMapTileHeight();
            int _totalCol = m_referenceMapData.GetMapTileWidth();
            int tileID_CurrentTIle = m_referenceMapData.GetMapTileData(m_referenceMapData.ConvertToMapIdex(m_playerTilePositionY, m_playerTilePositionX, _totalRow, _totalCol));

            if (tileID_CurrentTIle == (int)TileType.PATH)
            {
                m_onHideTile = false;
                m_hidingTimer = 0.0f;
            }

            if (tileID_CurrentTIle == (int)TileType.EXIT_SIGN)
            {
                m_onExitTile = true;
            }

            if (tileID_CurrentTIle == (int)TileType.HIDE_TILE)
            {
                m_hidingTimer += (gameTime.ElapsedGameTime.TotalSeconds*7);
                if (m_hidingTimer <= 5.0f)
                {
                    m_onHideTile = true;
                }
                else
                {
                    m_hidingTimer = 0.0f;
                    m_onHideTile = false;
                    SetTilePosition((int)m_playerPreviousPositionForHideout.X, (int)m_playerPreviousPositionForHideout.Y);

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

            switch (m_direction)
            {
                case PlayerDirection.UP:
                    _nextTileY = m_playerTilePositionY - 1;
                    _nextTileX = m_playerTilePositionX;
                    break;
                case PlayerDirection.DOWN:
                    _nextTileY = m_playerTilePositionY + 1;
                    _nextTileX = m_playerTilePositionX;
                    break;
                case PlayerDirection.LEFT:
                    _nextTileY = m_playerTilePositionY;
                    _nextTileX = m_playerTilePositionX - 1;
                    break;
                case PlayerDirection.RIGHT:
                    _nextTileY = m_playerTilePositionY;
                    _nextTileX = m_playerTilePositionX + 1;
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
                        m_playerPreviousPositionForHideout.X = m_playerTilePositionX;
                        m_playerPreviousPositionForHideout.Y = m_playerTilePositionY;
                    return true;
                    }
                else if (_NextTileTileID == (int)TileType.EXIT_SIGN)
                {
                    m_playerPreviousPositionForExit.X = m_playerTilePositionX;
                    m_playerPreviousPositionForExit.Y = m_playerTilePositionY;
                    return true;
                }
                else
                    return false;
            }
            
            return false;
        }

        public void UpdateMovement(GameTime gameTime)
        {
            m_movingDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_standingDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
           
            //Set Previous Tile Value with Delay
            if(m_standingDelayTimer >= 0.1f)
            {
                m_playerPreviousPosition.X = m_playerTilePositionX;
                m_playerPreviousPosition.Y = m_playerTilePositionY;
                m_standingDelayTimer = 0.0f;
            }

            //Action with Delay
            if (m_movingDelayTimer >= 0.1f) 
            {
                switch (m_direction)
                {
                    case PlayerDirection.RIGHT:
                        if (_IsMoveToNextTilePossible())
                            m_playerTilePositionX += 1;
                        break;
                    case PlayerDirection.LEFT:
                        if (_IsMoveToNextTilePossible())
                            m_playerTilePositionX -= 1;
                        break;
                    case PlayerDirection.DOWN:
                        if (_IsMoveToNextTilePossible())
                            m_playerTilePositionY += 1;
                        break;
                    case PlayerDirection.UP:
                        if (_IsMoveToNextTilePossible())
                            m_playerTilePositionY -= 1;
                        break;
                }
                //ResetTimer
                m_movingDelayTimer = 0;
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
                if(doorLock[i].GetLockTilePositionX() == playerNextPosX
                    && doorLock[i].GetLockTilePositionY() == playerNextPosY)
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
