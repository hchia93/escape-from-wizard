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

        /* 
         *  Return True if the Path was found, False if there is no path.
         *  This function manipulates the m_PathList information and its value.
         */
        private bool AStarAlgorithm(Vector2 startPosition, Vector2 endPosition)
        {
            // Validate destination
            if (!IsDestinationValid(endPosition))
            {
                return false;
            }

            // If already at destination, no pathfinding needed
            if (startPosition == endPosition)
            {
                return true;
            }

            // Node structure: Position, Parent Position, F-Cost
            List<Tuple<Vector2, Vector2, int>> openList = new List<Tuple<Vector2, Vector2, int>>();
            List<Tuple<Vector2, Vector2, int>> closedList = new List<Tuple<Vector2, Vector2, int>>();

            // Initialize starting node
            int hCost = ComputeDistance(startPosition, endPosition);
            int gCost = 0;
            int fScore = gCost + hCost;

            // Add start node to open list
            openList.Add(new Tuple<Vector2, Vector2, int>(startPosition, startPosition, fScore));

            // Safety counter to prevent infinite loops
            int maxIterations = 1000;
            int iterations = 0;

            // Main A* loop
            while (openList.Count > 0 && iterations < maxIterations)
            {
                iterations++;

                // Find node with lowest F-cost
                int minima = int.MaxValue;
                int minIndex = 0;
                GetMinFScoreObject(ref openList, ref minima, ref minIndex);

                // Current node becomes the one with lowest F-cost
                var currentNode = openList[minIndex];
                Vector2 currentPos = currentNode.Item1;
                Vector2 currentParent = currentNode.Item2;

                // Move current node from open to closed list
                closedList.Add(currentNode);
                openList.RemoveAt(minIndex);

                // Check if we reached the goal
                if (currentPos == endPosition)
                {
                    // Reconstruct path by adding all nodes from closed list
                    m_PathList.AddRange(closedList);
                    return true;
                }

                // Explore all adjacent tiles
                foreach (EDirection direction in System.Enum.GetValues(typeof(EDirection)))
                {
                    if (IsAdjacentTileMovable(currentPos, direction))
                    {
                        Vector2 neighborPos = GetNextTileVector(currentPos, direction);

                        // Skip if neighbor is in closed list
                        if (closedList.Any(node => node.Item1 == neighborPos))
                        {
                            continue;
                        }

                        // Calculate costs for neighbor
                        int neighborGCost = ComputeDistance(neighborPos, startPosition);
                        int neighborHCost = ComputeDistance(neighborPos, endPosition);
                        int neighborFCost = neighborGCost + neighborHCost;

                        // Check if neighbor is already in open list
                        int existingNodeIndex = -1;
                        for (int i = 0; i < openList.Count; i++)
                        {
                            if (openList[i].Item1 == neighborPos)
                            {
                                existingNodeIndex = i;
                                break;
                            }
                        }

                        if (existingNodeIndex >= 0) // Found existing node
                        {
                            // If new path to neighbor is better, update it
                            if (neighborFCost < openList[existingNodeIndex].Item3)
                            {
                                openList[existingNodeIndex] = new Tuple<Vector2, Vector2, int>(neighborPos, currentPos, neighborFCost);
                            }
                        }
                        else
                        {
                            // Add neighbor to open list
                            openList.Add(new Tuple<Vector2, Vector2, int>(neighborPos, currentPos, neighborFCost));
                        }
                    }
                }
            }

            // No path found
            return false;
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
