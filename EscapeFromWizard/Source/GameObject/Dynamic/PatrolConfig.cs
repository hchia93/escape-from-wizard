using Microsoft.Xna.Framework;

namespace EscapeFromWizard.Source.GameObject.Dynamic
{
    public struct PatrolConfig
    {
        public Vector2 m_StartPosition { get; }
        public int m_CoverageX { get; }
        public int m_CoverageY { get; }

        public PatrolConfig(int startPosX, int startPosY, int coverageX, int coverageY)
        {
            m_StartPosition = new Vector2(startPosX, startPosY);
            m_CoverageX = coverageX;
            m_CoverageY = coverageY;
        }

        public PatrolConfig(Vector2 startPosition, int coverageX, int coverageY)
        {
            m_StartPosition = startPosition;
            m_CoverageX = coverageX;
            m_CoverageY = coverageY;
        }

        public static PatrolConfig FromArray(int[] patrolData)
        {
            if (patrolData.Length < 4)
            {
                throw new System.ArgumentException("Patrol data array must contain at least 4 elements: startX, startY, coverageX, coverageY");
            } 
            
            return new PatrolConfig(patrolData[0], patrolData[1], patrolData[2], patrolData[3]);
        }
    }
} 