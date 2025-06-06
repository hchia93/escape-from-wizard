using Microsoft.Xna.Framework;

namespace EscapeFromWizard
{
    public static class GameSettings
    {
        // Tile and Screen Settings
        public const int PixelHeightPerTile = 32;
        public const int PixelWidthPerTile = 32;
        public const int SquaresAcross = 18; // How many columns on screen
        public const int SquaresDown = 18; // How many rows on screen
        
        // Game Object Counts
        public const int NumOfKeys = 4;
        public const int NumOfLocks = 4;
        public const int NumOfMinions = 5;
        
        // Combat Settings
        public const int MinionHitDamage = 1;
        public const double HitDetectionTimerDefault = 5.0;
        
        // Screen Dimensions (calculated properties)
        public static int ScreenWidth => PixelWidthPerTile * SquaresAcross;
        public static int ScreenHeight => PixelHeightPerTile * SquaresDown;
        public static Vector2 ScreenCenter => new Vector2(ScreenWidth / 2, ScreenHeight / 2);
        
        // Minion Initial Patrol Data
        public static readonly int[][] MinionsInitialPatrolData = new int[NumOfMinions][]
        {
            new int[] {4, 1, 5, 6},
            new int[] {4, 15, 5, 6},
            new int[] {16, 1, 10, 10},
            new int[] {20, 15, 4, 5},
            new int[] {11, 8, 7, 7}
        };
        
        // Utility Functions
        public static Vector2 CalculateCameraBoundary(int mapTileWidth, int mapTileHeight)
        {
            int maxX = (mapTileWidth - SquaresAcross) * PixelWidthPerTile;
            int maxY = (mapTileHeight - SquaresDown) * PixelHeightPerTile;
            return new Vector2(maxX, maxY);
        }
        
        public static Rectangle CreateTileRectangle(int tileX, int tileY)
        {
            return new Rectangle(
                tileX * PixelWidthPerTile,
                tileY * PixelHeightPerTile,
                PixelWidthPerTile,
                PixelHeightPerTile
            );
        }
        
        public static Rectangle CreateTileRectangle(Vector2 tilePosition)
        {
            return CreateTileRectangle((int)tilePosition.X, (int)tilePosition.Y);
        }
        
        public static Vector2 TileToPixelPosition(int tileX, int tileY)
        {
            return new Vector2(tileX * PixelWidthPerTile, tileY * PixelHeightPerTile);
        }
        
        public static Vector2 TileToPixelPosition(Vector2 tilePosition)
        {
            return TileToPixelPosition((int)tilePosition.X, (int)tilePosition.Y);
        }
        
        public static Vector2 PixelToTilePosition(Vector2 pixelPosition)
        {
            return new Vector2(
                (int)(pixelPosition.X / PixelWidthPerTile),
                (int)(pixelPosition.Y / PixelHeightPerTile)
            );
        }
    }
} 