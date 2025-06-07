using Microsoft.Xna.Framework;

namespace EscapeFromWizard
{
    public static class GameSettings
    {
        // Each tile is 32x32 pixels.
        public const int m_TileHeightInPx = 32;
        public const int m_TileWidthInPx = 32;
        
        // Number of tiles visible on screens.
        public const int m_TilePerRow = 18;
        public const int m_TilePerColumn = 18; 
        
        // Game Object Counts
        public const int m_MaxKeyCount = 4;
        public const int m_MaxLockCount = 4;
        public const int m_MaxMinionCount = 5;

        public const int m_MaxHideTime = 5;

        // Combat Settings
        public const int m_MinionHitDamage = 1;
        public const int m_WizardHitDamage = 3;
       
        public const double HitDetectionTimerDefault = 5.0;
        
        // Viewport Dimensions
        public static Vector2 m_ViewportSize => new Vector2(m_TileWidthInPx * m_TilePerRow, m_TileHeightInPx * m_TilePerColumn);
    
        public static Vector2 m_ViewportCenter => new Vector2(m_ViewportSize.X / 2, m_ViewportSize.Y / 2);
        
        // Minion Initial Patrol Data
        public static readonly int[][] MinionsInitialPatrolData = new int[m_MaxMinionCount][]
        {
            new int[] {4, 1, 5, 6},
            new int[] {4, 15, 5, 6},
            new int[] {16, 1, 10, 10},
            new int[] {20, 15, 4, 5},
            new int[] {11, 8, 7, 7}
        };
        
        // Utility Functions
        public static Vector2 CalculateCameraBound(int levelWidth, int levelHeight)
        {
            int maxX = (levelWidth - m_TilePerRow) * m_TileWidthInPx;
            int maxY = (levelHeight - m_TilePerColumn) * m_TileHeightInPx;
            return new Vector2(maxX, maxY);
        }
        
        public static Rectangle CreateTileRectangleAt(int tileIndexX, int tileIndexY)
        {
            return new Rectangle(
                tileIndexX * m_TileWidthInPx,
                tileIndexY * m_TileHeightInPx,
                m_TileWidthInPx,
                m_TileHeightInPx
            );
        }
        
        public static Rectangle CreateTileRectangleAt(Vector2 tileIndex)
        {
            return CreateTileRectangleAt((int)tileIndex.X, (int)tileIndex.Y);
        }
        
        public static Vector2 TileToPixelPosition(int tileX, int tileY)
        {
            return new Vector2(tileX * m_TileWidthInPx, tileY * m_TileHeightInPx);
        }
        
        public static Vector2 TileToPixelPosition(Vector2 tilePosition)
        {
            return TileToPixelPosition((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static Vector2 PixelToTilePosition(Vector2 pixelPosition)
        {
            return new Vector2(
                (int)(pixelPosition.X / m_TileWidthInPx),
                (int)(pixelPosition.Y / m_TileHeightInPx)
            );
        }
    }
} 