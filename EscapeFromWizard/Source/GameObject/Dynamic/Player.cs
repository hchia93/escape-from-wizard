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

    public enum DamageSource
    {
        Minion,
        Wizard
    };

    public class PlayerInventory : IReset
    {
        private int m_Stars;
        private int m_QuestItems;
        private List<Key> m_Keys;
        private const int m_MaxQuestItems = 3;
        private const int m_MaxStars = 15;

        // Events for inventory changes
        public event EventHandler<int> OnQuestItemChanged;
        public event EventHandler<int> OnStarChanged;
        public event EventHandler<Key> OnKeyCollected;

        public PlayerInventory()
        {
            m_Stars = 0;
            m_QuestItems = 0;
            m_Keys = new List<Key>();
        }

        public void Reset()
        {
            m_Stars = 0;
            m_QuestItems = 0;
            m_Keys.Clear();
        }

        public void AddQuestItem()
        {
            int oldQuestItems = m_QuestItems;
            if (m_QuestItems < m_MaxQuestItems)
            {
                m_QuestItems++;
                if (oldQuestItems != m_QuestItems)
                {
                    OnQuestItemChanged?.Invoke(this, m_QuestItems);
                }
            }
        }

        public void AddStar()
        {
            int oldStars = m_Stars;
            if (m_Stars < m_MaxStars)
            {
                m_Stars++;
                if (oldStars != m_Stars)
                {
                    OnStarChanged?.Invoke(this, m_Stars);
                }
            }
        }

        public void AddKey(Key key)
        {
            if (!m_Keys.Contains(key))
            {
                m_Keys.Add(key);
                OnKeyCollected?.Invoke(this, key);
            }
        }

        public bool HasKey(Key key)
        {
            return m_Keys.Contains(key);
        }

        public int GetCurrentQuestItems()
        {
            return m_QuestItems;
        }

        public int GetMaxQuestItems()
        {
            return m_MaxQuestItems;
        }

        public int GetCurrentStars()
        {
            return m_Stars;
        }

        public int GetMaxStars()
        {
            return m_MaxStars;
        }

        public List<Key> GetKeys()
        {
            return m_Keys;
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

        private Level m_ReferenceMapData;
        private Lock[] m_DoorLock;

        private int m_HP;
        private const int m_MaxHP = 8;

        public PlayerInventory m_Inventory;

        // Sound callback functions
        public Action OnHitByMinion { get; set; }
        public Action OnHitByWizard { get; set; }
        public Action OnEnterOrExitHideState { get; set; }
        public Action OnHealed { get; set; }
        
        // State change events for minions and wizard
        public event EventHandler<bool> OnHideStateChanged;
        public event EventHandler<bool> OnExitStateChanged;
        public event EventHandler<int> OnHPChanged;

        public Player()
        {
            m_IsHiding = true;
            m_IsOnExit = false;
            m_Direction = PlayerDirection.STILL;
            m_MoveDelayTimer = 0.0f;
            m_StandDelayTimer = 0.0f;
            m_HideDelayTimer = 0.0f;
            m_HP = m_MaxHP;
            m_Inventory = new PlayerInventory();
        }

        public void SetLevel(Level level)
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

        public void SetPosition(Vector2 position)
        {
            m_Position = position;
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

        public bool GetIsHiding()
        {
            return m_IsHiding;
        }

        public bool IsOnExit()
        {
            return m_IsOnExit;
        }

        public void SetOnExit(bool value)
        {
            if (m_IsOnExit != value) // Only trigger event if state is changing
            {
                m_IsOnExit = value;
                OnExitStateChanged?.Invoke(this, m_IsOnExit);
            }
        }

        public void RevertPositionOnIncompleteQuest()
        {
            SetPosition((int)m_PositionBeforeExit.X, (int)m_PositionBeforeExit.Y);
        }

        public bool IsStanding()
        {
            //if player didn't move at all.
            return m_Position == m_PositionLastFrame;
        }

        public void TakeDamage(int value, DamageSource type)
        {
            int oldHP = m_HP;
            m_HP -= value;
            if (m_HP <= 0)
            {
                m_HP = 0;
            }
            if (oldHP != m_HP)
            {
                OnHPChanged?.Invoke(this, m_HP);
            }

            switch (type)
            {
                case DamageSource.Minion:
                    OnHitByMinion?.Invoke();
                    break;
                case DamageSource.Wizard:
                    OnHitByWizard?.Invoke();
                    break;
                default:
                    break;
            }
        }

        public void Heal(int value)
        {
            int oldHP = m_HP;
            m_HP += value;
            if (m_HP >= m_MaxHP)
            {
                m_HP = m_MaxHP;
            }
            if (oldHP != m_HP)
            {
                OnHPChanged?.Invoke(this, m_HP);
                // Trigger healing sound callback when player is actually healed
                OnHealed?.Invoke();
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
            m_Inventory.AddQuestItem();
        }

        public int GetCurrentNumOfQuestItem()
        {
            return m_Inventory.GetCurrentQuestItems();
        }

        public int GetMaxNumOfQuestItem()
        {
            return m_Inventory.GetMaxQuestItems();
        }

        public void LootStar()
        {
            m_Inventory.AddStar();
        }

        public int GetCurrentNumOfStar()
        {
            return m_Inventory.GetCurrentStars();
        }

        public int GetMaxNumOfStar()
        {
            return m_Inventory.GetMaxStars();
        }

        public void CollectKey(Key key)
        {
            m_Inventory.AddKey(key);
        }

        public bool HasKey(Key key)
        {
            return m_Inventory.HasKey(key);
        }

        private void UpdateState(GameTime gameTime)
        {
            int tileIdCurrentTile = m_ReferenceMapData.GetTileData(m_ReferenceMapData.ToTileIndex(m_Position, m_ReferenceMapData.GetTotalTileHeight(), m_ReferenceMapData.GetTotalTileWidth()));

            if (tileIdCurrentTile == (int)TileType.PATH)
            {
                if (m_IsHiding) // Only trigger event if state is changing
                {
                    m_IsHiding = false;
                    OnHideStateChanged?.Invoke(this, m_IsHiding);
                }
                m_HideDelayTimer = 0.0f;
            }

            if (tileIdCurrentTile == (int)TileType.EXIT_SIGN)
            {
                if (!m_IsOnExit) // Only trigger event if state is changing
                {
                    m_IsOnExit = true;
                    OnExitStateChanged?.Invoke(this, m_IsOnExit);
                }
            }

            if (tileIdCurrentTile == (int)TileType.HIDE_TILE)
            {
                m_HideDelayTimer += (gameTime.ElapsedGameTime.TotalSeconds * 7); // ????
                if (m_HideDelayTimer <= GameSettings.m_MaxHideTime)
                {
                    // Trigger hiding sound when player starts hiding
                    if (!m_IsHiding)
                    {
                        OnEnterOrExitHideState?.Invoke();
                        m_IsHiding = true;
                        OnHideStateChanged?.Invoke(this, m_IsHiding);
                    }
                }
                else
                {
                    m_HideDelayTimer = 0.0f;
                    if (m_IsHiding) // Only trigger event if state is changing
                    {
                        m_IsHiding = false;
                        OnHideStateChanged?.Invoke(this, m_IsHiding);
                    }
                    SetPosition(m_PositionBeforeHideout);
                    
                    // Trigger hiding sound when player is reset from hideout
                    OnEnterOrExitHideState?.Invoke();
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
                UpdateState(gameTime);
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
