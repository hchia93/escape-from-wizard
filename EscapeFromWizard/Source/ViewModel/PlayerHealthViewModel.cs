using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeFromWizard.Source.GameObject.Dynamic;
using TileEngine;
using System;

namespace EscapeFromWizard.ViewModel
{
    public enum HeartRectangleIDResolver
    {
        EMPTY_HEART = 10,
        FULL_HEART = 11,
        HALF_HEART = 12,
    }
    public class PlayerHealthViewModel
    {
        private SpriteSheet m_SourceSpriteSheet;
        private Texture2D m_SpriteSheetTexture;
        private Rectangle m_EmptyHeartRectangle;
        private Rectangle m_FilledHeartRectangle;
        private Vector2 m_WidgetPosition;
        private int m_HP;
        private int m_MaxHP;
        private Player m_Player;

        public PlayerHealthViewModel(Player player)
        {
            m_Player = player;
            m_HP = player.GetHP();
            m_MaxHP = player.GetMaxHP();
            
            // Subscribe to player's HP change event
            m_Player.OnHPChanged += OnPlayerHPChanged;
        }

        ~PlayerHealthViewModel()
        {
            // Unsubscribe from the event when the view model is destroyed
            if (m_Player != null)
            {
                m_Player.OnHPChanged -= OnPlayerHPChanged;
            }
        }

        private void OnPlayerHPChanged(object sender, int newHP)
        {
            m_HP = newHP;
        }

        public void SetSourceSpriteSheet(SpriteSheet spriteSheet)
        {
            m_SourceSpriteSheet = spriteSheet;
            m_SpriteSheetTexture = spriteSheet.m_Texture;
            UpdateHeartRectangles();
        }

        public void UpdateHeartRectangles()
        {
            m_EmptyHeartRectangle = m_SourceSpriteSheet.GetSourceRectangle((int) HeartRectangleIDResolver.EMPTY_HEART);
            m_FilledHeartRectangle = m_SourceSpriteSheet.GetSourceRectangle((int) HeartRectangleIDResolver.FULL_HEART);
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_WidgetPosition = position;
        }

        public void DrawView(SpriteBatch spriteBatch)
        {
            if (m_SourceSpriteSheet != null)
            {
                for (int i = 0; i < m_MaxHP; i++)
                {
                    Vector2 heartPosition = m_WidgetPosition + new Vector2(i * 32, 0); // Using 32 as tile size
                    if (i < m_HP)
                    {
                        spriteBatch.Draw(m_SpriteSheetTexture, heartPosition, m_FilledHeartRectangle, Microsoft.Xna.Framework.Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(m_SpriteSheetTexture, heartPosition, m_EmptyHeartRectangle, Microsoft.Xna.Framework.Color.White);
                    }
                }
            }
        }
    }
} 