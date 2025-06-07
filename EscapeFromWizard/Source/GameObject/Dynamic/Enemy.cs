using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using EscapeFromWizard.Map;

namespace EscapeFromWizard.Source.GameObject.Dynamic
{
    public enum EDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    public enum EBehaviorState
    {
        CHASE,
        WONDER,
        HAS_LINE_OF_SIGHT,
        STOP,
        PATROL,
    };

    public class Enemy
    {
        //EnemyPosition References
        private Vector2 m_TargetPosition;

        //External References
        private Level m_LevelData;
        private bool m_RefPlayerHideFlag;
        private bool m_RefPlayerWasHitFlag;
        private bool m_RefPlayerExitFlag;

        private EBehaviorState m_Behavior;
       
        public Enemy()
        {
            m_RefPlayerHideFlag = false;
            m_RefPlayerWasHitFlag = false;
        }

        public void SetBehavior(EBehaviorState state)
        {
            m_Behavior = state;
        }

        public void SetTargetPosition(int column, int row)
        {
            m_TargetPosition = new Vector2(column, row);
        }

        public void SetLevel(Level level)
        {
            m_LevelData = level;
        }

        public void SetPlayerHideFlag(bool hideFlag)
        {
            m_RefPlayerHideFlag = hideFlag;
        }

        public void SetPlayerWasHitFlag(bool wasHitFlag)
        {
            m_RefPlayerWasHitFlag = wasHitFlag;
        }

        public void SetPlayerExitFlag(bool exitFlag)
        {
            m_RefPlayerExitFlag = exitFlag;
        }

        public EBehaviorState GetBehavior() 
        {
            return m_Behavior;
        }

        public Vector2 GetTargetPosition()
        {
            return m_TargetPosition;
        }

        public Level GetMapReference()
        {
            return m_LevelData;
        }

        public bool GetPlayerHideFlag()
        {
            return m_RefPlayerHideFlag;
        }

        public bool GetPlayerWasHitFlag()
        {
            return m_RefPlayerWasHitFlag;
        }

        public bool GetPlayerExitFlag()
        {
            return m_RefPlayerExitFlag;
        }
        
        protected bool PlayerInLineOfSightHorizontalFlag(Vector2 playerPosition)
        {
            int playerX = (int) playerPosition.X;
            int playerY = (int) playerPosition.Y;
            int wizardX = (int) m_TargetPosition.X;
            int wizardY = (int)m_TargetPosition.Y;
            int r = m_LevelData.GetTotalTileHeight();
            int c = m_LevelData.GetTotalTileWidth();
            int tileID = 0;

            //If same Row means different in X, player-wizard Horizontal
            if (wizardY == playerY)
            {
                int d = playerX - wizardX;
                if (d > 0) //Player on right side 
                {
                    while (d != 0)
                    {
                        tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(wizardY, wizardX + 1, r, c));

                        if (tileID == 00)
                        {
                            wizardX++;
                            d--;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else //Player on left side
                {
                    while (d != 0)
                    {
                        tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(wizardY, wizardX - 1, r, c));

                        if (tileID == 00)
                        {
                            wizardX--;
                            d++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        protected bool PlayerInLineOfSightVerticalFlag(Vector2 playerPosition)
        {
            int playerX = (int) playerPosition.X;
            int playerY = (int) playerPosition.Y;
            int wizardX = (int) m_TargetPosition.X;
            int wizardY = (int) m_TargetPosition.Y;
            int r = m_LevelData.GetTotalTileHeight();
            int c = m_LevelData.GetTotalTileWidth();
            int tileID = 0;

            //If same Col means different in Y, player-wizard Vertical
            if (wizardX == playerX)
            {
                int d = playerY - wizardY;
                if (d > 0) //Player beneath Wizard 
                {
                    while (d != 0)
                    {
                        tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(wizardY + 1, wizardX, r, c));

                        if (tileID == 00)
                        {
                            wizardY++;
                            d--;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    return true;
                }
                else //Player above Wizard
                {
                    while (d != 0)
                    {
                        tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(wizardY - 1, wizardX, r, c));

                        if (tileID == 00)
                        {
                            wizardY--;
                            d++;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    return true;
                }
            }
            return false;
        }

        protected bool IsPlayerInLineOfSight(Vector2 playerPosition)
        {
            return PlayerInLineOfSightHorizontalFlag(playerPosition) || PlayerInLineOfSightVerticalFlag(playerPosition);
        }

        protected bool IsAdjacentTileMovable(Vector2 currentTile, EDirection direction)
        {
            /* 
             * Return true - It is movable for wizard
             * Return false - It is an unmovable tile for wizard
             */
            int r = m_LevelData.GetTotalTileHeight();
            int c = m_LevelData.GetTotalTileWidth();
            int x = (int)currentTile.X;
            int y = (int)currentTile.Y;
            int tileID = 0;

            switch (direction)
            {
                case EDirection.UP:
                    tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(y - 1, x, r, c));
                    break;
                case EDirection.DOWN:
                    tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(y + 1, x, r, c));
                    break;
                case EDirection.LEFT:
                    tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(y, x - 1, r, c));
                    break;
                case EDirection.RIGHT:
                    tileID = m_LevelData.GetTileData(m_LevelData.ToTileIndex(y, x + 1, r, c));
                    break;
            }
            if (tileID == 0)
                return true;

            return false;
        }

        protected bool IsDestinationValid(Vector2 location)
        {
            int r = m_LevelData.GetTotalTileHeight();
            int c = m_LevelData.GetTotalTileWidth();
            int x = (int)location.X;
            int y = (int)location.Y;
            int currentTileID = GetMapReference().GetTileData(GetMapReference().ToTileIndex(y, x, r, c));

            if (currentTileID != 0)
                return false;

            return true;
        }

        //=======================================================
        // Computation Functions
        //=======================================================

        protected Vector2 GetNextTileVector(Vector2 currentTile, EDirection direction)
        {
            /* 
            * Return a new vector2 values according to relative direction of wizard
            * Return original vector2 values if the function failed to get.
            * Potential Error: Does not check on invalid index on map.
            */
            switch (direction)
            {
                case EDirection.UP:
                    currentTile.Y -= 1;
                    return currentTile;
                case EDirection.DOWN:
                    currentTile.Y += 1;
                    return currentTile;
                case EDirection.LEFT:
                    currentTile.X -= 1;
                    return currentTile;
                case EDirection.RIGHT:
                    currentTile.X += 1;
                    return currentTile;
            }
            return currentTile;
        }

        protected int ComputeDistance(Vector2 pos1, Vector2 pos2)
        {
            //Heuristic implement here.
            int offsetX = (int)MathHelper.Distance(pos1.X, pos2.X);
            int offsetY = (int)MathHelper.Distance(pos1.Y, pos2.Y);
            return offsetX + offsetY;
        }

   
        //=======================================================
        // Pathing Functions
        //=======================================================

        protected void GetMinFScoreObject(ref List<Tuple<Vector2, Vector2, int>> refList, ref int minima, ref int mindex)
        {
            if (refList.Count == 0) return;
            
            minima = refList[0].Item3;
            mindex = 0;
            
            for (int i = 1; i < refList.Count; ++i)
            {
                if (refList[i].Item3 < minima)
                {
                    minima = refList[i].Item3;
                    mindex = i;
                }
            }
        }

        //=======================================================
        // Overritable Functions
        //=======================================================
        protected virtual void Chase(Vector2 playerPosVector){ }

        protected virtual void LineOfSight(Vector2 playerPosVector){ }

        protected virtual void Wonder() { }

        protected virtual void Decision(Vector2 playerPosVectror) { }

        public virtual void UpdateMovement(GameTime gameTime, Vector2 playerPosVector){ }
      

    }
}
