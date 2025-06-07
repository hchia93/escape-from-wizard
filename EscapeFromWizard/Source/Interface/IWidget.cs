using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeFromWizard.Source.Interface
{
    public interface IWidget
    {
        void SetWidgetPosition(Vector2 position);
        void DrawWidget(SpriteBatch spriteBatch);
    }
} 