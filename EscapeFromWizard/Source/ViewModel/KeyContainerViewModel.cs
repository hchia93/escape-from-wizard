using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeFromWizard.Source.Interface;
using System.Collections.Generic;

namespace EscapeFromWizard.ViewModel
{
    public class KeyTextureIDResolver
    {
        int[] m_KeyTextureIDs = new int[] { 17, 18, 15, 21, // Pre-Loot key texture id
            22, 23, 20, 16 }; // Post-Loot key texture id

        public int GetKeyTextureID(int i)
        {
            return m_KeyTextureIDs[i];
        }
    }
    public class KeyContainerViewModel : IWidget
    {
        private List<KeyViewModel> m_KeyViewModels;
        private Vector2 m_WidgetPosition;
        private Vector2 m_PaddingInPX;
        private int m_WidgetCount = 0;
        private KeyTextureIDResolver m_TextureResolver = new KeyTextureIDResolver();

        public KeyContainerViewModel()
        {
            m_KeyViewModels = new List<KeyViewModel>();
            m_PaddingInPX = new Vector2(32, 0); // Default spacing of 32 pixels between keys
            SetDefaultPosition();
        }

        private void SetDefaultPosition()
        {
            // Position the key container at 4 tiles from the right edge, on the bottom row
            Rectangle keyViewRect = GameSettings.CreateTileRectangleAt(GameSettings.m_TilePerRow - 4, GameSettings.m_TilePerColumn - 1);
            m_WidgetPosition = keyViewRect.Location.ToVector2();
        }

        public void AddKeyViewModel(KeyViewModel keyViewModel)
        {
            int preLootID = m_TextureResolver.GetKeyTextureID(m_WidgetCount + 4);
            int postLoodID = m_TextureResolver.GetKeyTextureID(m_WidgetCount);
            m_WidgetCount++;
            keyViewModel.SetPreLootRectangleFromID(preLootID);
            keyViewModel.SetPostLootRectangleFromID(postLoodID);
            m_KeyViewModels.Add(keyViewModel);
            UpdateKeyPositions();
        }

        public void SetWidgetPosition(Vector2 position)
        {
            m_WidgetPosition = position;
            UpdateKeyPositions();
        }

        private void UpdateKeyPositions()
        {
            Vector2 currentPosition = m_WidgetPosition;
            foreach (var keyViewModel in m_KeyViewModels)
            {
                keyViewModel.SetWidgetPosition(currentPosition);
                currentPosition += m_PaddingInPX;
            }
        }

        public void DrawWidget(SpriteBatch spriteBatch)
        {
            foreach (var keyViewModel in m_KeyViewModels)
            {
                keyViewModel.DrawWidget(spriteBatch);
            }
        }

        public KeyViewModel GetKeyViewModel(int index)
        {
            if (index >= 0 && index < m_KeyViewModels.Count)
            {
                return m_KeyViewModels[index];
            }
            return null;
        }
    }
} 