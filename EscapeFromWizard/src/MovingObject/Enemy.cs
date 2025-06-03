using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonRE
{
    public enum EDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    public enum EState
    {
        CHASE,
        WONDER,
        LINEOFSIGHT,
        STOP,
        PATROL,
    };


    public class Enemy
    {
        //EnemyPosition References
        private int refPosX;
        private int refPosY;

        //External References
        private MapData refMap;
        private bool refPlayerHideFlag;
        private bool refPlayerWasHitFlag;
        private bool refPlayerExitFlag;

        private EState m_state;
       
        public Enemy()
        {
            refPlayerHideFlag = false;
            refPlayerWasHitFlag = false;
        }

        //=======================================================
        // Reference Functions
        //=======================================================
        public void SetEnumState(EState i_state)
        {
            m_state = i_state;
        }

        public void SetTilePosition(int i_TileColumn, int i_TileRow)
        {
            refPosX = i_TileColumn;
            refPosY = i_TileRow;
        }


        public void SetMapReference(MapData i_MapData)
        {
            refMap = i_MapData;
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


        public EState GetEnumState() 
        {
            return m_state;
        }

        public Vector2 GetTilePositionVector()
        {
            return new Vector2(refPosX, refPosY);
        }


        public MapData GetMapReference()
        {
            return refMap;
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

        protected bool _PlayerInLineOfSightHorizontalFlag(Vector2 i_playerPosVector)
        {
            int playerX = (int)i_playerPosVector.X;
            int playerY = (int)i_playerPosVector.Y;
            int wizardX = refPosX;
            int wizardY = refPosY;
            int R = refMap.GetMapTileHeight();
            int C = refMap.GetMapTileWidth();
            int TileID = 0;

            //If same Row means different in X, player-wizard Horizontal
            if (wizardY == playerY)
            {
                int _d = playerX - wizardX;
                if (_d > 0) //Player on right side 
                {
                    while (_d != 0)
                    {
                        TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(wizardY, wizardX + 1, R, C));

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
                        TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(wizardY, wizardX - 1, R, C));

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

        protected bool _PlayerInLineOfSightVerticalFlag(Vector2 i_playerPosVector)
        {
            int playerX = (int)i_playerPosVector.X;
            int playerY = (int)i_playerPosVector.Y;
            int wizardX = refPosX;
            int wizardY = refPosY;
            int R = refMap.GetMapTileHeight();
            int C = refMap.GetMapTileWidth();
            int TileID = 0;

            //If same Col means different in Y, player-wizard Vertical
            if (wizardX == playerX)
            {
                int _d = playerY - wizardY;
                if (_d > 0) //Player beneath Wizard 
                {
                    while (_d != 0)
                    {
                        TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(wizardY + 1, wizardX, R, C));

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
                        TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(wizardY - 1, wizardX, R, C));

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

        protected bool _PlayerInLineOfSight(Vector2 i_playerPosVector)
        {
            return _PlayerInLineOfSightHorizontalFlag(i_playerPosVector) || _PlayerInLineOfSightVerticalFlag(i_playerPosVector);
        }

        protected bool _IsAdjacentTileMovable(Vector2 i_CurrentTile, EDirection i_Direction)
        {
            /* 
             * Return true - It is movable for wizard
             * Return false - It is an unmovable tile for wizard
             */
            int R = refMap.GetMapTileHeight();
            int C = refMap.GetMapTileWidth();
            int X = (int)i_CurrentTile.X;
            int Y = (int)i_CurrentTile.Y;
            int TileID = 0;

            switch (i_Direction)
            {
                case EDirection.UP:
                    TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(Y - 1, X, R, C));
                    break;
                case EDirection.DOWN:
                    TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(Y + 1, X, R, C));
                    break;
                case EDirection.LEFT:
                    TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(Y, X - 1, R, C));
                    break;
                case EDirection.RIGHT:
                    TileID = refMap.GetMapTileData(refMap.ConvertToMapIdex(Y, X + 1, R, C));
                    break;
            }
            if (TileID == 0)
                return true;

            return false;
        }

        protected bool _IsDestinationValid(Vector2 i_Location)
        {
            int R = refMap.GetMapTileHeight();
            int C = refMap.GetMapTileWidth();
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
