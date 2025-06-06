using EscapeFromWizard.Source.GameObject.Dynamic;
using Microsoft.Xna.Framework;

namespace TileEngine
{
    public class Camera2D
    {
        private Vector2 m_Position = Vector2.Zero;
        private Vector4 m_Boundary;

        public Camera2D()
        {

        }

        public void UpdateMovement(PlayerDirection playerDirection, Vector2 playerPos, Vector2 screenCenter)
        {
            m_Position = playerPos*32 - screenCenter;
            // Clamping is not a must any more since it will follow character.
            m_Position.X = MathHelper.Clamp(m_Position.X, m_Boundary.X, m_Boundary.Y);
            m_Position.Y = MathHelper.Clamp(m_Position.Y, m_Boundary.Z, m_Boundary.W);
        }

        public Vector2 GetCameraPosition()
        {
            return m_Position;
        }

        public Matrix TransformMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-m_Position.X, -m_Position.Y, 0)) * Matrix.CreateRotationZ(0) * Matrix.CreateScale(1);// *
            //Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
        }
        public void SetBoundary(float xMin, float yMin, float xMax, float yMax)
        {
            m_Boundary.X = xMin;
            m_Boundary.Y = xMax;
            m_Boundary.Z = yMin;
            m_Boundary.W = yMax;
        }
    }
}
