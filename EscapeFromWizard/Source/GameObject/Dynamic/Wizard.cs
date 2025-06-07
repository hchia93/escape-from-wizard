using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Dynamic
{
    public class Wizard : Enemy
    {
        //Pathing variables
        private List<Tuple<Vector2,Vector2,int>> m_PathList;
        private double m_MoveDelayTimer;
        private double m_UpdateTimer;
        private int m_PathListIterator;
        private Random m_RandomStream;

        private Vector2 m_DestinationVector;

        public Wizard() 
        {
            SetBehavior(EBehaviorState.WONDER);
            m_PathList = new List<Tuple<Vector2,Vector2,int>>();
            m_PathListIterator = 0;

            m_RandomStream = new Random();

            //Timers
            m_UpdateTimer = 1.0f;
            m_DestinationVector = new Vector2();
        }

        //----------------------------------------------------------------------
        // State Functions
        //----------------------------------------------------------------------

        /* Move Wizard to Desire Location With A* */
        private void MoveToTargetTile(int column, int row)
        {
            Vector2 start = GetTargetPosition();
            m_DestinationVector.X = column;
            m_DestinationVector.Y = row;
            AStarAlgorithm(start, m_DestinationVector);
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
            m_PathListIterator = 0;
        }

        private bool AStarAlgorithm(Vector2 startPosition, Vector2 endPosition)
        {
            /* 
             *  Return True as if the Path was found, False as if there is a dead end.
             *  This function Manipulate the m_path information and its value.
             */

            //Reject Noob Request that request wrong location.
            if (!IsDestinationValid(endPosition))
            {
                return false;
            }
             

            //Node Pos, Parent Pos, f_cost
            List<Tuple<Vector2,Vector2, int>> openList = new List<Tuple<Vector2,Vector2,int>>();
            List<Tuple<Vector2, Vector2, int>> closeList = new List<Tuple<Vector2, Vector2, int>>();

            //Cost Set Up
            int hCost = ComputeDistance(startPosition, endPosition);
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
            while(openList.Count != 0) 
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
                        if (!closeList.Any(pos => pos.Item1 == nextPos))
                        {
                            hCost = ComputeDistance(nextPos, endPosition);
                            gCost = ComputeDistance(nextPos, startPosition);
                            fScore = gCost + hCost;
                            openList.Add(new Tuple<Vector2, Vector2, int>(nextPos, currentPos, fScore));
                        }

                        //If current node is the target node
                        if (nextPos == endPosition)
                        {
                            closeList.Add(new Tuple<Vector2, Vector2, int>(endPosition, currentPos, 0));
                            closeList.Add(new Tuple<Vector2, Vector2, int>(endPosition, endPosition, 0));
                            m_PathList.AddRange(closeList);
                            return true;
                        }
                    }
                }
            }

            return !(openList.Count == 0);
        }

        //=======================================================
        // Overritable Functions
        //=======================================================
        override protected void Wonder()
        {
            ClearPathList();
            m_DestinationVector.X = m_RandomStream.Next(0, 25);
            m_DestinationVector.Y = m_RandomStream.Next(0, 25);
            while (!IsDestinationValid(m_DestinationVector))
            {
                m_DestinationVector.X = m_RandomStream.Next(0, 25);
                m_DestinationVector.Y = m_RandomStream.Next(0, 25);
            }
            MoveToTargetTile((int)m_DestinationVector.X, (int)m_DestinationVector.Y);
            m_UpdateTimer = 0.0f;
        }

        override protected void Chase(Vector2 playerPosVector)
        {
            ClearPathList();
            int playerPosX = (int)playerPosVector.X;
            int playerPosY = (int)playerPosVector.Y;
            MoveToTargetTile(playerPosX, playerPosY);
            m_UpdateTimer = 0.0f;
        }

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
           switch(GetBehavior())
           {
               case EBehaviorState.WONDER:
                   Wonder();
                   return;
               case EBehaviorState.CHASE:
                   Chase(playerPosVector);
                   return;
               case EBehaviorState.HAS_LINE_OF_SIGHT:
                   LineOfSight(playerPosVector);
                   return;
               case EBehaviorState.STOP:
                   return;
           }
        }

        override public void UpdateMovement(GameTime gameTime, Vector2 playerPosVector)
        {
            //Setup References
            int playerPosX = (int)playerPosVector.X;
            int playerPosY = (int)playerPosVector.Y;
            int wizardX = (int)GetTargetPosition().X;
            int wizardY = (int)GetTargetPosition().Y;
            m_MoveDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_UpdateTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (wizardX == playerPosX && wizardY == playerPosY)
            {
                SetPlayerWasHitFlag(true);
            }

            if (IsPlayerInLineOfSight(playerPosVector))
            {
                SetBehavior(EBehaviorState.HAS_LINE_OF_SIGHT);
            }

            if (GetPlayerHideFlag() == false && GetPlayerExitFlag() == false )
            {
                SetBehavior(EBehaviorState.CHASE);
            }

            //Update per 2.5f
            if (m_UpdateTimer > 1.50f /*&& GetEnumState() != EState.CHASE*/)
            {
                if (GetPlayerHideFlag() == true )
                {
                    SetBehavior(EBehaviorState.WONDER); 
                }
                
                if (GetPlayerExitFlag() == true)
                {
                    SetBehavior(EBehaviorState.STOP); 
                }

                Decision(playerPosVector);
            }
            if (m_PathList.Count != 0 && m_PathListIterator < m_PathList.Count && !GetPlayerWasHitFlag())
            {
                //Movement Update Speed
                if (m_MoveDelayTimer >= 0.165f)
                {
                    Vector2 updatePosition = m_PathList[m_PathListIterator].Item1;
                    SetTargetPosition((int)updatePosition.X, (int)updatePosition.Y);
                    m_PathListIterator++;
                    m_MoveDelayTimer = 0.0f;
                }

                if(m_PathListIterator == m_PathList.Count -1)
                {
                    SetBehavior(EBehaviorState.STOP);
                }
            }
        } 
    }
}
