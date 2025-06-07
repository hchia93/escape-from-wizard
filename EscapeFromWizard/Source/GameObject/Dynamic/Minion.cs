using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Dynamic
{
    public class Minion : Enemy
    {
        //Pathing variables
        private List<Tuple<Vector2, Vector2, int>> m_PathList;
        private double m_MoveDelayTimer;
        private double m_UpdateTimer;
        private int m_PathListIterator;

        private Vector2 m_PatrolStartPos;
        private Vector2 m_PatrolEndPos;

        private bool m_FoundPatrolPath;
        private bool m_ReachDestinationFlag;

        private int m_CoverageX;
        private int m_CoverageY;

        private int m_MinionId;

        public Minion()
        {
            m_PathList = new List<Tuple<Vector2, Vector2, int>>();
            m_PathListIterator = 0;

            m_FoundPatrolPath = false;
            m_ReachDestinationFlag = false;

            //Timers
            m_UpdateTimer = 1.0f;
            SetBehavior(EBehaviorState.STOP);

        }

        public void SetPatrolConfig(PatrolConfig config)
        {
            m_PatrolStartPos = config.m_StartPosition;
            m_CoverageX = config.m_CoverageX;
            m_CoverageY = config.m_CoverageY;
        }

        public void SetMinionId(int minionId)
        {
            m_MinionId = minionId;
        }

        private void Patrol()
        {
            /* 
             * Given an initial position (patrolCentrePos) and specified coverage values X and Y,
             * find the furthest patrol point,
             * and then patrol back and forth between the initial position & end position.
             */

            if (!m_FoundPatrolPath)
            {
                for (int i = m_CoverageX; i >= 0; i--)
                {
                    for (int j = m_CoverageY; j >= 0; j--)
                    {
                        m_PatrolEndPos.X = m_PatrolStartPos.X + i;
                        m_PatrolEndPos.Y = m_PatrolStartPos.Y + j;

                        if (IsDestinationValid(m_PatrolEndPos))
                        {
                            m_FoundPatrolPath = true;
                            break;
                        }
                    }
                }
            }

            //if minion has not reached the End point, go to destination
            //else, go back to the starting position
            if (!m_ReachDestinationFlag)
            {
                MoveToTargetTile((int)m_PatrolEndPos.X, (int)m_PatrolEndPos.Y);
            }
            else
            {
                MoveToTargetTile((int)m_PatrolStartPos.X, (int)m_PatrolStartPos.Y);
            }
        }

        private void MoveToTargetTile(int column, int row)
        {
            /* 
             *  Move Minion to Desire Location With A*. 
             */
            Vector2 start = GetTargetPosition();
            Vector2 destination;
            destination.X = column;
            destination.Y = row;
            AStarAlgorithm(start, destination);
            GetMoveablePathFromList();
        }

        private void GetMoveablePathFromList()
        {
            List<Tuple<Vector2, Vector2, int>> processedPathList = new List<Tuple<Vector2, Vector2, int>>();
            if (m_PathList.Count > 0)
            {
                Vector2 currentTilePos = m_PathList[m_PathList.Count - 1].Item1;
                Vector2 parentTilePos = m_PathList[m_PathList.Count - 1].Item2;
                processedPathList.Add(m_PathList[m_PathList.Count - 1]); //Always add this Tile. 

                for (int i = m_PathList.Count - 1; i > 0; --i)
                {
                    if (parentTilePos == m_PathList[i - 1].Item1)
                    {
                        processedPathList.Add(m_PathList[i - 1]);
                        currentTilePos = m_PathList[i - 1].Item1;
                        parentTilePos = m_PathList[i - 1].Item2;
                    }
                }

                ClearPathList();
                m_PathList.AddRange(processedPathList);
                m_PathList.Reverse();
            }
        }

        private void ClearPathList()
        {
            m_PathList.TrimExcess();
            m_PathList.Clear();
            //WP COMMENTED THE LINE BELOW : 28/9 TO FIX BUG
            //m_PathListIterator = 0;
        }

        private bool AStarAlgorithm(Vector2 startPosition, Vector2 patrolEndPosition)
        {
            /* 
             *  Return True as if the Path was found, False as if there is a dead end.
             *  This function Manipulate the m_path information and its value.
             */

            //Reject Noob Request that request wrong location.
            if (!IsDestinationValid(patrolEndPosition))
            {
                return false;
            }

            //Node Pos, Parent Pos, f_cost
            List<Tuple<Vector2, Vector2, int>> openList = new List<Tuple<Vector2, Vector2, int>>();
            List<Tuple<Vector2, Vector2, int>> closeList = new List<Tuple<Vector2, Vector2, int>>();

            //Cost Set Up
            int hCost = ComputeDistance(startPosition, patrolEndPosition);
            int gCost = 0;
            int fScore = 0;
            fScore = gCost + hCost;

            //SetUp Current Pos and Parent Pos
            Vector2 currentPos = startPosition;
            Vector2 parentPos = startPosition;
            Vector2 nextPos = startPosition;

            //Index SetUp
            int minima = 999;
            int mindex = 0;

            //Add Start To OpenList
            openList.Add(new Tuple<Vector2, Vector2, int>(currentPos, parentPos, fScore));

            //While Not Empty
            while (openList.Count != 0)
            {

                //Find the Minimum Index
                GetMinFScoreObject(ref openList, ref minima, ref mindex);

                //Update CurrentPos
                currentPos = openList[mindex].Item1;

                //Add Min Into Close List
                closeList.Add(openList[mindex]);

                //Remove Current Node From OpenList
                openList.Remove(openList[mindex]);

                //Pick Next Node that is Movable, ReCompute f_cost, Add Into OpenList
                foreach (EDirection direction in System.Enum.GetValues(typeof(EDirection)))
                {
                    if (IsAdjacentTileMovable(currentPos, direction))
                    {
                        nextPos = GetNextTileVector(currentPos, direction);
                        if (!closeList.Any(Pos => Pos.Item1 == nextPos))
                        {
                            hCost = ComputeDistance(nextPos, patrolEndPosition);
                            gCost = ComputeDistance(nextPos, startPosition);
                            fScore = gCost + hCost;
                            openList.Add(new Tuple<Vector2, Vector2, int>(nextPos, currentPos, fScore));
                        }

                        //If current node is the target node
                        if (nextPos == patrolEndPosition)
                        {
                            closeList.Add(new Tuple<Vector2, Vector2, int>(patrolEndPosition, currentPos, 0));
                            closeList.Add(new Tuple<Vector2, Vector2, int>(patrolEndPosition, patrolEndPosition, 0));
                            m_PathList.AddRange(closeList);
                            if (!m_ReachDestinationFlag)
                                m_ReachDestinationFlag = true;
                            else
                                m_ReachDestinationFlag = false;
                            return true;
                        }
                    }
                }
            }

            if (openList.Count == 0)
                return false;
            else
                return true;
        }

        //=======================================================
        // Overritable Functions
        //=======================================================


        override protected void LineOfSight(Vector2 playerPosVector)
        {
            ClearPathList();
            int playerPosX = (int)playerPosVector.X;
            int playerPosY = (int)playerPosVector.Y;
            MoveToTargetTile(playerPosX, playerPosY);
            m_UpdateTimer = 0.0f;
        }

        override protected void Decision(Vector2 playerPosVector)
        {
            switch (GetBehavior())
            {
                case EBehaviorState.WONDER:
                    Wonder(); //This function is not overrided. It used the super class version
                    return;
                case EBehaviorState.CHASE:
                    Chase(playerPosVector); //This function is not overrided. It used the super class version
                    return;
                case EBehaviorState.HAS_LINE_OF_SIGHT:
                    LineOfSight(playerPosVector);
                    return;
                case EBehaviorState.STOP:
                    ClearPathList();
                    return;
                case EBehaviorState.PATROL:
                    Patrol();
                    return;
            }
        }

        override public void UpdateMovement(GameTime gameTime, Vector2 playerPosVector)
        {
            //Setup References
            int playerPosX = (int)playerPosVector.X;
            int playerPosY = (int)playerPosVector.Y;
            int minionX = (int)GetTargetPosition().X;
            int minionY = (int)GetTargetPosition().Y;
            m_MoveDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_UpdateTimer += gameTime.ElapsedGameTime.TotalSeconds;


            //Outside of Timer Scope.
            if (minionX == playerPosX && minionY == playerPosY)
                SetPlayerWasHitFlag(true);

            //Timer Scope: Increased to make it slower to calculate next move.
            if (m_UpdateTimer > 1.50f && GetBehavior() != EBehaviorState.PATROL)
            {
                SetBehavior(EBehaviorState.PATROL);
                
                if (GetPlayerHideFlag() == true)
                    SetBehavior(EBehaviorState.WONDER);

                if (GetPlayerExitFlag() == true )
                    SetBehavior(EBehaviorState.STOP);

                //WP COMMENTED THE LINE BELOW : 28/9 TO FIX BUG (2)
                //if (_PlayerInLineOfSight(playerPosVector))
                   // SetEnumState(EState.LINEOFSIGHT);
                
                Decision(playerPosVector);
            }

            if (m_PathList.Count != 0 && m_PathListIterator < m_PathList.Count && !GetPlayerWasHitFlag())
            {
                //Move Update Speed
                if (m_MoveDelayTimer >= 0.165f)
                {
                    Vector2 updatePosition = m_PathList[m_PathListIterator].Item1;
                    SetTargetPosition((int)updatePosition.X, (int)updatePosition.Y);
                    m_PathListIterator++;
                    m_MoveDelayTimer = 0.0f;
                }

                if (m_PathListIterator == m_PathList.Count - 1)
                {
                    SetBehavior(EBehaviorState.STOP);
                }

            }
        }
    }
}
