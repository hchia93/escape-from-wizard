using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    public class Tile
    {
        public Texture2D TileSetTexture;
        static public int TileWidth = 32;
        static public int TileHeight = 32;
        static public Vector2 TileOriginPoint = new Vector2(0, 0);

        public Rectangle GetSourceRectangle(int tileIndex)
        {
            int tileY = tileIndex / (TileSetTexture.Width / TileWidth); 
            int tileX = tileIndex % (TileSetTexture.Width / TileWidth);  

            return new Rectangle(tileX * TileWidth, tileY * TileHeight, TileWidth, TileHeight);
        }
    }
}
