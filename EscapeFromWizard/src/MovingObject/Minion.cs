using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DungeonRE
{
    public class Minion : Enemy
    {
        //Pathing variables
        private List<Tuple<Vector2, Vector2, int>> m_pathList;
        private double m_moveDelayTimer;
        private double m_updateTimer;
        private int m_pathListiterator;

        private Vector2 PatrolStartPos;
        private Vector2 PatrolEndPos;

        private bool m_foundPatrolPath;
        private bool m_reachDestinationFlag;

        private int coverageX;
        private int coverageY;

        private int minionId;

        public Minion()
        {


            m_pathList = new List<Tuple<Vector2, Vector2, int>>();
            m_pathListiterator = 0;

            m_foundPatrolPath = false;
            m_reachDestinationFlag = false;

            //Timers
            m_updateTimer = 1.0f;
            SetEnumState(EState.STOP);

        }

        //----------------------------------------------------------------------
        // Initialize
        //----------------------------------------------------------------------


        public void SetPatrolStartPos(int i_StartPosX, int i_StartPosY, int i_coverageX, int i_coverageY)
        {
            PatrolStartPos = new Vector2((int)i_StartPosX, (int)i_StartPosY);
            coverageX = i_coverageX;
            coverageY = i_coverageY;
        }

        public void SetPatrolStartPos(int[] i_PatrolData)
        {
            PatrolStartPos = new Vector2((int)i_PatrolData[0], (int)i_PatrolData[1]);
            coverageX = i_PatrolData[2];
            coverageY = i_PatrolData[3];

        }

        public void SetMinionId(int i_minionId)
        {
            minionId = i_minionId;
        }

        //----------------------------------------------------------------------
        // State Functions
        //----------------------------------------------------------------------

        private void _Patrol()
        {
            /* 
             * Given an initial position (patrolCentrePos) and specified coverage values X and Y,
             * find the furthest patrol point,
             * and then patrol back and forth between the initial position & end position.
             */

            if (!m_foundPatrolPath)
                for (int i = coverageX; i >= 0; i--)
                    for (int j = coverageY; j >= 0; j--)
                    {
                        PatrolEndPos.X = PatrolStartPos.X + i;
                        PatrolEndPos.Y = PatrolStartPos.Y + j;

                        if (_IsDestinationValid(PatrolEndPos))
                        {
                            m_foundPatrolPath = true;
                            break;
                        }
                    }


            //if minion has not reached the End point, go to destination
            //else, go back to the starting position
            if (!m_reachDestinationFlag)
            {
                _MoveToTargetTile((int)PatrolEndPos.X, (int)PatrolEndPos.Y);
            }
            else
            {
                _MoveToTargetTile((int)PatrolStartPos.X, (int)PatrolStartPos.Y);
            }
           

        }

      /*  private void _PatrolBetweenTwoPositions(Vector2 i_startPos, Vector2 i_endPos)
        {
            /* 
             * Given two defined X and Y positions, patrol back and forth between them
            
            //m_patrolFlag = true;

            //if minion has not reached the End point, go to destination
            //else, go back to the starting position
            if (!m_reachDestinationFlag)
                _MoveToTargetTile((int)i_endPos.X, (int)i_endPos.Y);
            else
                _MoveToTargetTile((int)i_startPos.X, (int)i_startPos.Y);

            m_updateTimer = 0.0f;
        }*/


        private void _MoveToTargetTile(int i_Column, int i_Row)
        {
            /* 
             *  Move Minion to Desire Location With A*. 
             */
            Vector2 Start = GetTilePositionVector();
            Vector2 Destination;
            Destination.X = i_Column;
            Destination.Y = i_Row;
            _A_STAR_ALGORITHM(Start, Destination);
            _GetMoveablePathFromList();
        }

        private void _GetMoveablePathFromList()
        {
            List<Tuple<Vector2, Vector2, int>> processedPathList = new List<Tuple<Vector2, Vector2, int>>();
            if (m_pathList.Count > 0)
            {
                Vector2 currentTilePos = m_pathList[m_pathList.Count - 1].Item1;
                Vector2 parentTilePos = m_pathList[m_pathList.Count - 1].Item2;
                processedPathList.Add(m_pathList[m_pathList.Count - 1]); //Always add this Tile. 

                for (int i = m_pathList.Count - 1; i > 0; --i)
                    if (parentTilePos == m_pathList[i - 1].Item1)
                    {
                        processedPathList.Add(m_pathList[i - 1]);
                        currentTilePos = m_pathList[i - 1].Item1;
                        parentTilePos = m_pathList[i - 1].Item2;
                    }

                _ClearPathList();
                m_pathList.AddRange(processedPathList);
                m_pathList.Reverse();
            }
        }

        private void _ClearPathList()
        {
            m_pathList.TrimExcess();
            m_pathList.Clear();
            //WP COMMENTED THE LINE BELOW : 28/9 TO FIX BUG
            //m_pathListiterator = 0;
        }

        private bool _A_STAR_ALGORITHM(Vector2 i_StartPosition, Vector2 i_PatrolEndPosition)
        {
            /* 
             *  Return True as if the Path was found, False as if there is a dead end.
             *  This function Manipulate the m_path information and its value.
             */

            //Reject Noob Request that request wrong location.
            if (!_IsDestinationValid(i_PatrolEndPosition))
                return false;

            //Node Pos, Parent Pos, f_cost
            List<Tuple<Vector2, Vector2, int>> OpenList = new List<Tuple<Vector2, Vector2, int>>();
            List<Tuple<Vector2, Vector2, int>> CloseList = new List<Tuple<Vector2, Vector2, int>>();

            //Cost Set Up
            int h_cost = _ComputeDistance(i_StartPosition, i_PatrolEndPosition);
            int g_cost = 0;
            int f_score = 0;
            f_score = g_cost + h_cost;

            //SetUp Current Pos and Parent Pos
            Vector2 CurrentPos = i_StartPosition;
            Vector2 ParentPos = i_StartPosition;
            Vector2 NextPos = i_StartPosition;

            //Index SetUp
            int minima = 999;
            int mindex = 0;

            //Add Start To OpenList
            OpenList.Add(new Tuple<Vector2, Vector2, int>(CurrentPos, ParentPos, f_score));

            //While Not Empty
            while (OpenList.Count != 0)
            {

                //Find the Minimum Index
                _GetMinFScoreObject(ref OpenList, ref minima, ref mindex);

                //Update CurrentPos
                CurrentPos = OpenList[mindex].Item1;

                //Add Min Into Close List
                CloseList.Add(OpenList[mindex]);

                //Remove Current Node From OpenList
                OpenList.Remove(OpenList[mindex]);

                //Pick Next Node that is Movable, ReCompute f_cost, Add Into OpenList
                foreach (EDirection direction in Enum.GetValues(typeof(EDirection)))
                {
                    if (_IsAdjacentTileMovable(CurrentPos, direction))
                    {
                        NextPos = _GetNextTileVector(CurrentPos, direction);
                        if (!CloseList.Any(Pos => Pos.Item1 == NextPos))
                        {
                            h_cost = _ComputeDistance(NextPos, i_PatrolEndPosition);
                            g_cost = _ComputeDistance(NextPos, i_StartPosition);
                            f_score = g_cost + h_cost;
                            OpenList.Add(new Tuple<Vector2, Vector2, int>(NextPos, CurrentPos, f_score));
                        }

                        //If current node is the target node
                        if (NextPos == i_PatrolEndPosition)
                        {
                            CloseList.Add(new Tuple<Vector2, Vector2, int>(i_PatrolEndPosition, CurrentPos, 0));
                            CloseList.Add(new Tuple<Vector2, Vector2, int>(i_PatrolEndPosition, i_PatrolEndPosition, 0));
                            m_pathList.AddRange(CloseList);
                            if (!m_reachDestinationFlag)
                                m_reachDestinationFlag = true;
                            else
                                m_reachDestinationFlag = false;
                            return true;
                        }
                    }
                }
            }

            if (OpenList.Count == 0)
                return false;
            else
                return true;
        }

        //=======================================================
        // Overritable Functions
        //=======================================================


        override protected void _LineOfSight(Vector2 i_playerPosVector)
        {
            _ClearPathList();
            int playerPosX = (int)i_playerPosVector.X;
            int playerPosY = (int)i_playerPosVector.Y;
            _MoveToTargetTile(playerPosX, playerPosY);
            m_updateTimer = 0.0f;
        }

        override protected void _Decision(Vector2 i_playerPosVector)
        {
            switch (GetEnumState())
            {
                case EState.WONDER:
                    _Wonder(); //This function is not overrided. It used the super class version
                    return;
                case EState.CHASE:
                    _Chase(i_playerPosVector); //This function is not overrided. It used the super class version
                    return;
                case EState.LINEOFSIGHT:
                    _LineOfSight(i_playerPosVector);
                    return;
                case EState.STOP:
                    _ClearPathList();
                    return;
                case EState.PATROL:
                    _Patrol();
                    return;
            }
        }

        override public void UpdateMovement(GameTime gameTime, Vector2 i_playerPosVector)
        {
            //Setup References
            int playerPosX = (int)i_playerPosVector.X;
            int playerPosY = (int)i_playerPosVector.Y;
            int minionX = (int)GetTilePositionVector().X;
            int minionY = (int)GetTilePositionVector().Y;
            m_moveDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_updateTimer += gameTime.ElapsedGameTime.TotalSeconds;


            //Outside of Timer Scope.
            if (minionX == playerPosX && minionY == playerPosY)
                SetPlayerWasHitFlag(true);

            //Timer Scope: Increased to make it slower to calculate next move.
            if (m_updateTimer > 1.50f && GetEnumState() != EState.PATROL)
            {
                SetEnumState(EState.PATROL);
                
                if (GetPlayerHideFlag() == true)
                    SetEnumState(EState.WONDER);

                if (GetPlayerExitFlag() == true )
                    SetEnumState(EState.STOP);

                //WP COMMENTED THE LINE BELOW : 28/9 TO FIX BUG (2)
                //if (_PlayerInLineOfSight(i_playerPosVector))
                   // SetEnumState(EState.LINEOFSIGHT);
                
                _Decision(i_playerPosVector);
            }

            if (m_pathList.Count != 0 && m_pathListiterator < m_pathList.Count && !GetPlayerWasHitFlag())
            {
                //Move Update Speed
                if (m_moveDelayTimer >= 0.165f)
                {
                    Vector2 updatePosition = m_pathList[m_pathListiterator].Item1;
                    SetTilePosition((int)updatePosition.X, (int)updatePosition.Y);
                    m_pathListiterator++;
                    m_moveDelayTimer = 0.0f;
                }

                if (m_pathListiterator == m_pathList.Count - 1)
                {
                    SetEnumState(EState.STOP);
                }

            }
        }
    }
}
