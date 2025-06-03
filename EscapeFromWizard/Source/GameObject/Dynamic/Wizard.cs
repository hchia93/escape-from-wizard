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
        private Random random_stream;

        private Vector2 m_DestinationVector;

        public Wizard() 
        {
            SetBehavior(EBehaviorState.WONDER);
            m_PathList = new List<Tuple<Vector2,Vector2,int>>();
            m_PathListIterator = 0;

            random_stream = new Random();

            //Timers
            m_UpdateTimer = 1.0f;
            m_DestinationVector = new Vector2();
        }

        //----------------------------------------------------------------------
        // State Functions
        //----------------------------------------------------------------------

        /* Move Wizard to Desire Location With A* */
        private void _MoveToTargetTile(int i_Column, int i_Row)
        {
            Vector2 Start = GetTargetPosition();
            m_DestinationVector.X = i_Column;
            m_DestinationVector.Y = i_Row;
            _A_STAR_ALGORITHM(Start, m_DestinationVector);
            _GetMoveablePathFromList();
        }

        private void _GetMoveablePathFromList()
        {
            List<Tuple<Vector2, Vector2, int>> processedPathList = new List<Tuple<Vector2, Vector2, int>>();
            Vector2 currentTilePos = m_PathList[m_PathList.Count - 1].Item1;
            Vector2 parentTilePos = m_PathList[m_PathList.Count - 1].Item2;
            processedPathList.Add(m_PathList[m_PathList.Count - 1]); //Always add this Tile. 

            for (int i = m_PathList.Count - 1; i > 0; --i)
                if (parentTilePos == m_PathList[i - 1].Item1)
                {
                    processedPathList.Add(m_PathList[i - 1]);
                    currentTilePos = m_PathList[i - 1].Item1;
                    parentTilePos = m_PathList[i - 1].Item2;
                }
                 
            _ClearPathList();
            m_PathList.AddRange(processedPathList);
            m_PathList.Reverse();
        }

        private void _ClearPathList()
        {
            m_PathList.TrimExcess();
            m_PathList.Clear();
            m_PathListIterator = 0;
        }

        private bool _A_STAR_ALGORITHM(Vector2 i_StartPosition, Vector2 i_EndPosition)
        {
            /* 
             *  Return True as if the Path was found, False as if there is a dead end.
             *  This function Manipulate the m_path information and its value.
             */

            //Reject Noob Request that request wrong location.
            if (!_IsDestinationValid(i_EndPosition))
            {
                return false;
            }
             

            //Node Pos, Parent Pos, f_cost
            List<Tuple<Vector2,Vector2, int>> OpenList = new List<Tuple<Vector2,Vector2,int>>();
            List<Tuple<Vector2, Vector2, int>> CloseList = new List<Tuple<Vector2, Vector2, int>>();

            //Cost Set Up
            int h_cost = _ComputeDistance(i_StartPosition, i_EndPosition);
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
            while(OpenList.Count != 0) 
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
                foreach (EDirection direction in System.Enum.GetValues(typeof(EDirection)))
                {
                    if (_IsAdjacentTileMovable(CurrentPos, direction))
                    {
                        NextPos = _GetNextTileVector(CurrentPos, direction);
                        if (!CloseList.Any(Pos => Pos.Item1 == NextPos))
                        {
                            h_cost = _ComputeDistance(NextPos, i_EndPosition);
                            g_cost = _ComputeDistance(NextPos, i_StartPosition);
                            f_score = g_cost + h_cost;
                            OpenList.Add(new Tuple<Vector2, Vector2, int>(NextPos, CurrentPos, f_score));
                        }

                        //If current node is the target node
                        if (NextPos == i_EndPosition)
                        {
                            CloseList.Add(new Tuple<Vector2, Vector2, int>(i_EndPosition, CurrentPos, 0));
                            CloseList.Add(new Tuple<Vector2, Vector2, int>(i_EndPosition, i_EndPosition, 0));
                            m_PathList.AddRange(CloseList);
                            return true;
                        }
                    }
                }
            }

            return !(OpenList.Count == 0);
        }

        //=======================================================
        // Overritable Functions
        //=======================================================
        override protected void _Wonder()
        {
            _ClearPathList();
            m_DestinationVector.X = random_stream.Next(0, 25);
            m_DestinationVector.Y = random_stream.Next(0, 25);
            while (!_IsDestinationValid(m_DestinationVector))
            {
                m_DestinationVector.X = random_stream.Next(0, 25);
                m_DestinationVector.Y = random_stream.Next(0, 25);
            }
            _MoveToTargetTile((int)m_DestinationVector.X, (int)m_DestinationVector.Y);
            m_UpdateTimer = 0.0f;
        }


        override protected void _Chase(Vector2 i_playerPosVector)
        {
            _ClearPathList();
            int playerPosX = (int)i_playerPosVector.X;
            int playerPosY = (int)i_playerPosVector.Y;
            _MoveToTargetTile(playerPosX, playerPosY);
            m_UpdateTimer = 0.0f;
        }


        override protected void _LineOfSight(Vector2 i_playerPosVector)
        {

            _ClearPathList();
            int playerPosX = (int)i_playerPosVector.X;
            int playerPosY = (int)i_playerPosVector.Y;
            _MoveToTargetTile(playerPosX, playerPosY);
            m_UpdateTimer = 0.0f;

        }


        override protected void _Decision(Vector2 i_playerPosVector)
        {
           switch(GetBehavior())
           {
               case EBehaviorState.WONDER:
                   _Wonder();
                   return;
               case EBehaviorState.CHASE:
                   _Chase(i_playerPosVector);
                   return;
               case EBehaviorState.HAS_LINE_OF_SIGHT:
                   _LineOfSight(i_playerPosVector);
                   return;
               case EBehaviorState.STOP:
                   _ClearPathList();
                   return;
           }
        }

        override public void UpdateMovement(GameTime gameTime, Vector2 i_playerPosVector)
        {
            //Setup References
            int playerPosX = (int)i_playerPosVector.X;
            int playerPosY = (int)i_playerPosVector.Y;
            int wizardX = (int)GetTargetPosition().X;
            int wizardY = (int)GetTargetPosition().Y;
            m_MoveDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            m_UpdateTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (wizardX == playerPosX && wizardY == playerPosY)
                SetPlayerWasHitFlag(true);

            if (IsPlayerInLineOfSight(i_playerPosVector))
                SetBehavior(EBehaviorState.HAS_LINE_OF_SIGHT);

            if (GetPlayerHideFlag() == false && GetPlayerExitFlag() == false )
                SetBehavior(EBehaviorState.CHASE);

            //Update per 2.5f
            if (m_UpdateTimer > 1.50f /*&& GetEnumState() != EState.CHASE*/)
            {
                if (GetPlayerHideFlag() == true )
                        SetBehavior(EBehaviorState.WONDER); 
                
                if (GetPlayerExitFlag() == true)
                    SetBehavior(EBehaviorState.STOP); 

                _Decision(i_playerPosVector);
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
                    SetBehavior(EBehaviorState.STOP);
            }
        } 
    }
}
