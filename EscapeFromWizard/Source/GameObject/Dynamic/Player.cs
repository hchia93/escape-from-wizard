using EscapeFromWizard.Map;
using EscapeFromWizard.Source.GameObject.Static;
using EscapeFromWizard.Source.Interface;
using Microsoft.Xna.Framework;
using System;
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
        private bool m_PickUpSomething;
        private bool m_HpIncreased;

        private Level m_ReferenceMapData;
        private Lock[] m_DoorLock;

        private int m_HP;
        private const int m_MaxHP = 8;

        PlayerInventory m_Inventory;

        private int m_QuestItemOnHand;
        private int m_MaxQuestItem = 3;

        private int m_CollectedStar;
        private int m_MaxStar = 15;

        // Sound callback functions
        public Action OnHitByMinion { get; set; }
        public Action OnHitByWizard { get; set; }

        public Player()
        {
            m_IsHiding = true;
            m_IsOnExit = false;
            m_Direction = PlayerDirection.STILL;
            m_MoveDelayTimer = 0.0f;
            m_StandDelayTimer = 0.0f;
            m_HideDelayTimer = 0.0f;
            m_HP = m_MaxHP;
            m_QuestItemOnHand = 0;
            m_CollectedStar = 0;
        }
        void Reset()
        {
            m_Direction = PlayerDirection.STILL;
        }

        public void SetWorld(Level level)
        {
            m_ReferenceMapData = level;
        }

        public void SetUpLockInformation(Lock[] doorLock)
        {
            m_DoorLock = doorLock;
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
            return m_PickUpSomething;
        }

        public bool IsStanding()
        {
            //if player didn't move at all.
            return m_Position == m_PositionLastFrame;
        }

        public void SetPickUpFlag(bool value)
        {
            m_PickUpSomething = value;
        }

        public void TakeDamage(int value)
        {
            m_HP -= value;
            if (m_HP <= 0)
            {
                m_HP = 0;
            }
        }

        public void TakeDamageFromMinion(int value)
        {
            TakeDamage(value);
            OnHitByMinion?.Invoke();
        }

        public void TakeDamageFromWizard(int value)
        {
            TakeDamage(value);
            OnHitByWizard?.Invoke();
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
        public void LootQuestItem()
        {
            m_QuestItemOnHand += 1;
        }

        public int GetCurrentNumOfQuestItem()
        {
            return m_QuestItemOnHand;
        }

        public int GetMaxNumOfQuestItem()
        {
            return m_MaxQuestItem;
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
            return m_MaxStar;
        }

        private void CheckCurrentTile(GameTime gameTime)
        {
            int maxRow = m_ReferenceMapData.GetTotalTileHeight();
            int maxColumn = m_ReferenceMapData.GetTotalTileWidth();
            int tileIdCurrentTile = m_ReferenceMapData.GetTileData(m_ReferenceMapData.ToTileIndex((int)m_Position.Y, (int)m_Position.X, maxRow, maxColumn));

            if (tileIdCurrentTile == (int)TileType.PATH)
            {
                m_IsHiding = false;
                m_HideDelayTimer = 0.0f;
            }

            if (tileIdCurrentTile == (int)TileType.EXIT_SIGN)
            {
                m_IsOnExit = true;
            }

            if (tileIdCurrentTile == (int)TileType.HIDE_TILE)
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

        private bool IsMoveToNextTilePossible() 
        {

            int totalRow = m_ReferenceMapData.GetTotalTileHeight();
            int totalCol = m_ReferenceMapData.GetTotalTileWidth();
            int nextTileTileID = 0;

            int nextTileX = 0;
            int nextTileY = 0;

            switch (m_Direction)
            {
                case PlayerDirection.UP:
                    nextTileY = (int) m_Position.Y - 1;
                    nextTileX = (int) m_Position.X;
                    break;
                case PlayerDirection.DOWN:
                    nextTileY = (int) m_Position.Y + 1;
                    nextTileX = (int) m_Position.X;
                    break;
                case PlayerDirection.LEFT:
                    nextTileY = (int) m_Position.Y;
                    nextTileX = (int) m_Position.X - 1;
                    break;
                case PlayerDirection.RIGHT:
                    nextTileY = (int) m_Position.Y;
                    nextTileX = (int) m_Position.X + 1;
                    break;
            }

            nextTileTileID = m_ReferenceMapData.GetTileData(m_ReferenceMapData.ToTileIndex(nextTileY, nextTileX, totalRow, totalCol));

            //Lock control
            if (CheckIfPlayerIsNextToLock(nextTileX, nextTileY))
            {
                if (nextTileTileID == (int)TileType.PATH)
                {
                    return true;
                }
                else if (nextTileTileID == (int)TileType.HIDE_TILE)
                {
                    m_PositionBeforeHideout = m_Position;
                    return true;
                }
                else if (nextTileTileID == (int)TileType.EXIT_SIGN)
                {
                    m_PositionBeforeExit = m_Position;
                    return true;
                }
                else
                {
                    return false;
                }
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
                        if (IsMoveToNextTilePossible())
                        {
                            m_Position.X += 1;
                        }
                        break;
                    case PlayerDirection.LEFT:
                        if (IsMoveToNextTilePossible())
                        {
                            m_Position.X -= 1;
                        }
                        break;
                    case PlayerDirection.DOWN:
                        if (IsMoveToNextTilePossible())
                        {
                            m_Position.Y += 1;
                        }
                        break;
                    case PlayerDirection.UP:
                        if (IsMoveToNextTilePossible())
                        {
                            m_Position.Y -= 1;
                        }
                        break;
                }
                //ResetTimer
                m_MoveDelayTimer = 0;
                CheckCurrentTile(gameTime);
            }
        }

        private bool CheckIfPlayerIsNextToLock(int playerNextPosX, int playerNextPosY)
        {
            for (int i = 0; i < m_DoorLock.Length; i++)
            {
                //if there is a lock beside player
                if (m_DoorLock[i].GetPosition() == new Vector2(playerNextPosX, playerNextPosY))
                {
                    //if the lock is unlocked
                    if (m_DoorLock[i].IsUnlocked())
                    {
                        //can walk
                        m_DoorLock[i].SetDestroyed(true);
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
