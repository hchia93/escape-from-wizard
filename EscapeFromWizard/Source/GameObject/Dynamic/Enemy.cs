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
        private MapData m_MapData;
        private bool refPlayerHideFlag;
        private bool refPlayerWasHitFlag;
        private bool refPlayerExitFlag;

        private EBehaviorState m_Behavior;
       
        public Enemy()
        {
            refPlayerHideFlag = false;
            refPlayerWasHitFlag = false;
        }

        //=======================================================
        // Reference Functions
        //=======================================================
        public void SetBehavior(EBehaviorState state)
        {
            m_Behavior = state;
        }

        public void SetTargetPosition(int column, int row)
        {
            m_TargetPosition = new Vector2(column, row);
        }

        public void SetMapReference(MapData mapData)
        {
            m_MapData = mapData;
        }


        public void SetPlayerHideFlag(bool i_bool)
        {
            refPlayerHideFlag = i_bool;
        }


        public void SetPlayerWasHitFlag(bool i_bool)
        {
            refPlayerWasHitFlag = i_bool;
        }


        public void SetPlayerExitFlag(bool i_bool)
        {
            refPlayerExitFlag = i_bool;
        }


        public EBehaviorState GetBehavior() 
        {
            return m_Behavior;
        }

        public Vector2 GetTargetPosition()
        {
            return m_TargetPosition;
        }


        public MapData GetMapReference()
        {
            return m_MapData;
        }


        public bool GetPlayerHideFlag()
        {
            return refPlayerHideFlag;
        }


        public bool GetPlayerWasHitFlag()
        {
            return refPlayerWasHitFlag;
        }


        public bool GetPlayerExitFlag()
        {
            return refPlayerExitFlag;
        }

        //=======================================================
        // Condition Functions
        //=======================================================

        protected bool _PlayerInLineOfSightHorizontalFlag(Vector2 playerPosition)
        {
            int playerX = (int) playerPosition.X;
            int playerY = (int) playerPosition.Y;
            int wizardX = (int) m_TargetPosition.X;
            int wizardY = (int)m_TargetPosition.Y;
            int R = m_MapData.GetMapTileHeight();
            int C = m_MapData.GetMapTileWidth();
            int TileID = 0;

            //If same Row means different in X, player-wizard Horizontal
            if (wizardY == playerY)
            {
                int _d = playerX - wizardX;
                if (_d > 0) //Player on right side 
                {
                    while (_d != 0)
                    {
                        TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(wizardY, wizardX + 1, R, C));

                        if (TileID == 00)
                        {
                            wizardX++;
                            _d--;
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
                    while (_d != 0)
                    {
                        TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(wizardY, wizardX - 1, R, C));

                        if (TileID == 00)
                        {
                            wizardX--;
                            _d++;
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

        protected bool _PlayerInLineOfSightVerticalFlag(Vector2 playerPosition)
        {
            int playerX = (int) playerPosition.X;
            int playerY = (int) playerPosition.Y;
            int wizardX = (int) m_TargetPosition.X;
            int wizardY = (int) m_TargetPosition.Y;
            int R = m_MapData.GetMapTileHeight();
            int C = m_MapData.GetMapTileWidth();
            int TileID = 0;

            //If same Col means different in Y, player-wizard Vertical
            if (wizardX == playerX)
            {
                int _d = playerY - wizardY;
                if (_d > 0) //Player beneath Wizard 
                {
                    while (_d != 0)
                    {
                        TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(wizardY + 1, wizardX, R, C));

                        if (TileID == 00)
                        {
                            wizardY++;
                            _d--;
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
                    while (_d != 0)
                    {
                        TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(wizardY - 1, wizardX, R, C));

                        if (TileID == 00)
                        {
                            wizardY--;
                            _d++;
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
            return _PlayerInLineOfSightHorizontalFlag(playerPosition) || _PlayerInLineOfSightVerticalFlag(playerPosition);
        }

        protected bool _IsAdjacentTileMovable(Vector2 i_CurrentTile, EDirection i_Direction)
        {
            /* 
             * Return true - It is movable for wizard
             * Return false - It is an unmovable tile for wizard
             */
            int R = m_MapData.GetMapTileHeight();
            int C = m_MapData.GetMapTileWidth();
            int X = (int)i_CurrentTile.X;
            int Y = (int)i_CurrentTile.Y;
            int TileID = 0;

            switch (i_Direction)
            {
                case EDirection.UP:
                    TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(Y - 1, X, R, C));
                    break;
                case EDirection.DOWN:
                    TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(Y + 1, X, R, C));
                    break;
                case EDirection.LEFT:
                    TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(Y, X - 1, R, C));
                    break;
                case EDirection.RIGHT:
                    TileID = m_MapData.GetMapTileData(m_MapData.ConvertToMapIdex(Y, X + 1, R, C));
                    break;
            }
            if (TileID == 0)
                return true;

            return false;
        }

        protected bool _IsDestinationValid(Vector2 i_Location)
        {
            int R = m_MapData.GetMapTileHeight();
            int C = m_MapData.GetMapTileWidth();
            int X = (int)i_Location.X;
            int Y = (int)i_Location.Y;
            int CurrentTileID = GetMapReference().GetMapTileData(GetMapReference().ConvertToMapIdex(Y, X, R, C));

            if (CurrentTileID != 0)
                return false;

            return true;
        }


        //=======================================================
        // Computation Functions
        //=======================================================

        protected Vector2 _GetNextTileVector(Vector2 i_CurrentTile, EDirection i_Direction)
        {
            /* 
            * Return a new vector2 values according to relative direction of wizard
            * Return original vector2 values if the function failed to get.
            * Potential Error: Does not check on invalid index on map.
            */
            switch (i_Direction)
            {
                case EDirection.UP:
                    i_CurrentTile.Y -= 1;
                    return i_CurrentTile;
                case EDirection.DOWN:
                    i_CurrentTile.Y += 1;
                    return i_CurrentTile;
                case EDirection.LEFT:
                    i_CurrentTile.X -= 1;
                    return i_CurrentTile;
                case EDirection.RIGHT:
                    i_CurrentTile.X += 1;
                    return i_CurrentTile;
            }
            return i_CurrentTile;
        }

        protected int _ComputeDistance(Vector2 i_Pos1, Vector2 i_Pos2)
        {
            //Heuristic implement here.
            int offsetX = (int)MathHelper.Distance(i_Pos1.X, i_Pos2.X);
            int offsetY = (int)MathHelper.Distance(i_Pos1.Y, i_Pos2.Y);
            return offsetX + offsetY;
        }

   
        //=======================================================
        // Pathing Functions
        //=======================================================

        protected void _GetMinFScoreObject(ref List<Tuple<Vector2, Vector2, int>> refList, ref int minima, ref int mindex)
        {
            for (int i = 0; i < refList.Count; ++i)
                if (refList[i].Item3 <= minima)
                {
                    minima = refList[i].Item3;
                    mindex = i;
                }
                else
                {
                    //Reset if not match.
                    minima = 999;
                    mindex = 0;
                }

        }

        //=======================================================
        // Overritable Functions
        //=======================================================
        protected virtual void _Chase(Vector2 i_playerPosVector){ }

        protected virtual void _LineOfSight(Vector2 i_playerPosVector){ }

        protected virtual void _Wonder() { }

        protected virtual void _Decision(Vector2 i_playerPosVectror) { }

        public virtual void UpdateMovement(GameTime gameTime, Vector2 i_playerPosVector){ }
      

    }
}
