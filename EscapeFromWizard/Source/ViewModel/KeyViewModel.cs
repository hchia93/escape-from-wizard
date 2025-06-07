using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeFromWizard.Source.GameObject.Static;
using EscapeFromWizard.Source.Interface;
using System;
using TileEngine;

namespace EscapeFromWizard.ViewModel
{
    public class KeyViewModel : IWidget
    {
        private SpriteSheet m_SourceSpriteSheet;
        private Texture2D m_SpriteSheetTexture;
        private bool m_IsLooted;
        private Vector2 m_WidgetPosition;
        private Rectangle m_PreLootedRectangle;
        private Rectangle m_PostLootedRectangle;
        private Key m_Key;

        public KeyViewModel(Key key)
        {
            m_Key = key;
            m_IsLooted = key.IsLooted();
            m_WidgetPosition = Vector2.Zero;
            m_PreLootedRectangle = Rectangle.Empty;
            m_PostLootedRectangle = Rectangle.Empty;

            // Subscribe to the key's loot state changes
            m_Key.OnLootedStateChanged += OnKeyLootedStateChanged;
        }
        ~KeyViewModel()
        {
            // Unsubscribe from the event when the view model is destroyed
            if (m_Key != null)
            {
                m_Key.OnLootedStateChanged -= OnKeyLootedStateChanged;
            }
        }

        private void OnKeyLootedStateChanged(object sender, bool isLooted)
        {
            m_IsLooted = isLooted;
        }

        public void SetPreLootRectangleFromID(int id)
        {
            m_PreLootedRectangle = m_SourceSpriteSheet.GetSourceRectangle(id);
        }

        public void SetPostLootRectangleFromID(int id)
        {
            m_PostLootedRectangle = m_SourceSpriteSheet.GetSourceRectangle(id);
        }

        public void SetSourceSpriteSheet(SpriteSheet spriteSheet)
        {
            m_SourceSpriteSheet = spriteSheet;
            m_SpriteSheetTexture = spriteSheet.m_Texture;
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_WidgetPosition = position;
        }

        public void DrawWidget(SpriteBatch spriteBatch)
        {
            if (m_SourceSpriteSheet != null)
            {
                if (m_IsLooted)
                {
                    spriteBatch.Draw(m_SpriteSheetTexture, m_WidgetPosition, m_PostLootedRectangle, Microsoft.Xna.Framework.Color.White);  
                }
                else
                {
                    spriteBatch.Draw(m_SpriteSheetTexture, m_WidgetPosition, m_PreLootedRectangle, Microsoft.Xna.Framework.Color.White);
                }
            }
        }
    }
} 