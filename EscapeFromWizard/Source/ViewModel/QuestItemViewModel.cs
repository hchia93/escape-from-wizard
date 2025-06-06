using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeFromWizard.Source.GameObject.Dynamic;
using TileEngine;
using System;

namespace EscapeFromWizard.ViewModel
{
    public enum QuestItemTextureIDResolver
    {
        FILLED_QUEST_ITEM = 22,
        EMPTY_QUEST_ITEM = 24,
    }
    
    public class QuestItemViewModel
    {
        private SpriteSheet m_SourceSpriteSheet;
        private Texture2D m_SpriteSheetTexture;
        private Rectangle m_EmptyQuestItemRectangle;
        private Rectangle m_FilledQuestItemRectangle;
        private Vector2 m_WidgetPosition;
        private int m_CurrentQuestItems;
        private int m_MaxQuestItems;
        private Player m_Player;

        public QuestItemViewModel(Player player)
        {
            m_Player = player;
            m_CurrentQuestItems = player.GetCurrentNumOfQuestItem();
            m_MaxQuestItems = player.GetMaxNumOfQuestItem();
            
            // Subscribe to player's quest item change event
            m_Player.OnQuestItemChanged += OnPlayerQuestItemChanged;
        }

        ~QuestItemViewModel()
        {
            // Unsubscribe from the event when the view model is destroyed
            if (m_Player != null)
            {
                m_Player.OnQuestItemChanged -= OnPlayerQuestItemChanged;
            }
        }

        private void OnPlayerQuestItemChanged(object sender, int newQuestItems)
        {
            m_CurrentQuestItems = newQuestItems;
        }

        public void SetSourceSpriteSheet(SpriteSheet spriteSheet)
        {
            m_SourceSpriteSheet = spriteSheet;
            m_SpriteSheetTexture = spriteSheet.m_Texture;
            UpdateQuestItemRectangles();
        }

        public void UpdateQuestItemRectangles()
        {
            m_EmptyQuestItemRectangle = m_SourceSpriteSheet.GetSourceRectangle((int)QuestItemTextureIDResolver.EMPTY_QUEST_ITEM);
            m_FilledQuestItemRectangle = m_SourceSpriteSheet.GetSourceRectangle((int)QuestItemTextureIDResolver.FILLED_QUEST_ITEM);
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_WidgetPosition = position;
        }

        public void DrawWidget(SpriteBatch spriteBatch)
        {
            if (m_SourceSpriteSheet != null)
            {
                for (int i = 0; i < m_MaxQuestItems; i++)
                {
                    Vector2 questItemPosition = m_WidgetPosition + new Vector2(i * 32, 0); // Using 32 as tile size
                    if (i < m_CurrentQuestItems)
                    {
                        spriteBatch.Draw(m_SpriteSheetTexture, questItemPosition, m_FilledQuestItemRectangle, Microsoft.Xna.Framework.Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(m_SpriteSheetTexture, questItemPosition, m_EmptyQuestItemRectangle, Microsoft.Xna.Framework.Color.White);
                    }
                }
            }
        }
    }
} 