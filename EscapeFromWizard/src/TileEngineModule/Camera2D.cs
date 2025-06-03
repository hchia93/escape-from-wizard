using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DungeonRE;
using Microsoft.Xna.Framework;

namespace TileEngine
{
    public class Camera2D
    {
        private Vector2 m_position = Vector2.Zero;
        private Vector4 m_boundary;

        public Camera2D()
        {

        }

        public void UpdateMovement(DungeonRE.PlayerDirection i_playerDirection, Vector2 i_playerPos, Vector2 i_screenCenter)
        {
            m_position = i_playerPos*32 - i_screenCenter;
            //Clamping is not a must any more since it will follow character.
            m_position.X = MathHelper.Clamp(m_position.X, m_boundary.X, m_boundary.Y);
            m_position.Y = MathHelper.Clamp(m_position.Y, m_boundary.Z, m_boundary.W);
        }

        public Vector2 GetCameraPosition()
        {
            return m_position;
        }

        public Matrix TransformMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-m_position.X, -m_position.Y, 0)) *
                Matrix.CreateRotationZ(0) *
                Matrix.CreateScale(1);// *
            //Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
        }
        public void SetBoundary(float i_XMin, float i_YMin, float i_XMax, float i_YMax)
        {
            m_boundary.X = i_XMin;
            m_boundary.Y = i_XMax;
            m_boundary.Z = i_YMin;
            m_boundary.W = i_YMax;
        }
    }
}
