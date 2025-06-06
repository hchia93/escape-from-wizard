using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EscapeFromWizard.ViewModel
{
    public class KeyContainerViewModel
    {
        private List<KeyViewModel> m_KeyViewModels;
        private Vector2 m_BaseWidgetPosition;
        private Vector2 m_PaddingInPX;

        public KeyContainerViewModel()
        {
            m_KeyViewModels = new List<KeyViewModel>();
            m_BaseWidgetPosition = Vector2.Zero;
            m_PaddingInPX = new Vector2(32, 0); // Default spacing of 32 pixels between keys
        }

        public void AddKeyViewModel(KeyViewModel keyViewModel)
        {
            m_KeyViewModels.Add(keyViewModel);
            UpdateKeyPositions();
        }

        public void SetBaseWidgetPosition(Vector2 position)
        {
            m_BaseWidgetPosition = position;
            UpdateKeyPositions();
        }
        private void UpdateKeyPositions()
        {
            Vector2 currentPosition = m_BaseWidgetPosition;
            foreach (var keyViewModel in m_KeyViewModels)
            {
                keyViewModel.SetWidgetPosition(currentPosition);
                currentPosition += m_PaddingInPX;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var keyViewModel in m_KeyViewModels)
            {
                keyViewModel.DrawView(spriteBatch);
            }
        }

        public void UpdateKeyState(int index, bool isLooted)
        {
            if (index >= 0 && index < m_KeyViewModels.Count)
            {
                m_KeyViewModels[index].SetLooted(isLooted);
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