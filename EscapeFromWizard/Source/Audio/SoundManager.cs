using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EscapeFromWizard.Source.Audio
{
    public class SoundManager
    {
        // Sound effects
        public SoundEffect m_ButtonSFX;
        public SoundEffect m_FootStepSFX;
        public SoundEffect m_OnItemLootedSFX;
        public SoundEffect m_OnUnlockDoorSFX;
        public SoundEffect m_OnMinorDamageSFX;
        public SoundEffect m_OnMajorDamageSFX;
        public SoundEffect m_OnHealSFX;
        public SoundEffect m_BGMSound;
        public SoundEffect m_GameVictory;
        public SoundEffect m_GameOver;
        
        // Sound effect instances
        public SoundEffectInstance m_BGMSoundInstance;
        public SoundEffectInstance m_GameOverSoundInstance;

        // Internal timing and "play once" flags
        private bool m_PlayGameBGMOnlyOnce = true;
        private bool m_PlayGameOverOnlyOnce = true;
        private bool m_PlayButtonOnlyOnce = true;
        private bool[] m_PlayUnlockDoorOnlyOnce;
        private double m_FootStepTimer = 0.0f;
        private const double m_FootStepPlayInterval = 0.2;

        public SoundManager(ContentManager Content)
        {
            m_ButtonSFX = Content.Load<SoundEffect>(@"Resource\Audio\ButtonSFX");
            m_FootStepSFX = Content.Load<SoundEffect>(@"Resource\Audio\FootstepSFX");
            m_BGMSound = Content.Load<SoundEffect>(@"Resource\Audio\Tombi_Dwarf_Forest_BGM");
            m_GameOver = Content.Load<SoundEffect>(@"Resource\Audio\GameOverSFX");
            m_GameVictory = Content.Load<SoundEffect>(@"Resource\Audio\Winning");
            m_OnItemLootedSFX = Content.Load<SoundEffect>(@"Resource\Audio\GetItemSFX");
            m_OnUnlockDoorSFX = Content.Load<SoundEffect>(@"Resource\Audio\UnlockDoorSFX");
            m_OnMajorDamageSFX = Content.Load<SoundEffect>(@"Resource\Audio\HitByWizardSFX");
            m_OnMinorDamageSFX = Content.Load<SoundEffect>(@"Resource\Audio\HitByMinionSFX");
            m_OnHealSFX = Content.Load<SoundEffect>(@"Resource\Audio\HPRecoverySFX");

            // Initialize unlock door flags based on number of locks
            m_PlayUnlockDoorOnlyOnce = new bool[GameSettings.NumOfLocks];
            ResetPlayOnceFlags();
        }

        public void Update(GameTime gameTime)
        {
            // Update footstep timer
            m_FootStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void ResetPlayOnceFlags()
        {
            m_PlayGameBGMOnlyOnce = true;
            m_PlayGameOverOnlyOnce = true;
            m_PlayButtonOnlyOnce = true;
            m_FootStepTimer = 0.0f;
            
            for (int i = 0; i < m_PlayUnlockDoorOnlyOnce.Length; i++)
            {
                m_PlayUnlockDoorOnlyOnce[i] = true;
            }
        }

        // Smart play methods that handle "play once" logic internally
        public void TryPlayBGM()
        {
            if (m_PlayGameBGMOnlyOnce)
            {
                PlayBGM();
                m_PlayGameBGMOnlyOnce = false;
            }
        }

        public void TryPlayGameOver()
        {
            if (m_PlayGameOverOnlyOnce)
            {
                OnGameOver();
                m_PlayGameOverOnlyOnce = false;
            }
        }

        public void TryPlayFootstepSound(bool isPlayerMoving)
        {
            if (isPlayerMoving && m_FootStepTimer > m_FootStepPlayInterval)
            {
                PlayFootstepSound();
                m_FootStepTimer = 0.0f;
                m_PlayButtonOnlyOnce = true; // Reset button sound flag when moving
            }
        }
        public void TryPlayUnlockDoorSound(int lockIndex)
        {
            if (lockIndex >= 0 && lockIndex < m_PlayUnlockDoorOnlyOnce.Length && m_PlayUnlockDoorOnlyOnce[lockIndex])
            {
                PlayUnlockDoorSound();
                m_PlayUnlockDoorOnlyOnce[lockIndex] = false;
            }
        }
        public void TryPlayHidingSound()
        {
            if (m_PlayButtonOnlyOnce)
            {
                PlayHidingSound();
                m_PlayButtonOnlyOnce = false;
            }
        }

        // Original play methods (now used internally)
        public void OnGameFinished()
        {
            m_BGMSoundInstance?.Stop(); 
            m_GameVictory.Play();
        }

        public void OnGameOver()
        {
            m_BGMSoundInstance?.Stop();
            m_GameOverSoundInstance = m_GameOver.CreateInstance();
            m_GameOverSoundInstance.IsLooped = true;
            m_GameOverSoundInstance.Play();
        }

        public void PlayBGM()
        {
            m_BGMSoundInstance = m_BGMSound.CreateInstance();
            m_BGMSoundInstance.IsLooped = true;
            m_BGMSoundInstance.Play();
        }

        public void PlayFootstepSound()
        {
            m_FootStepSFX.Play(0.15f, 0.0f, 0.0f);
        }

        public void PlayUnlockDoorSound()
        {
            m_OnUnlockDoorSFX.Play(0.15f, 0.0f, 0.0f);
        }

        public void PlayHidingSound()
        {
            m_ButtonSFX.Play(0.30f, 0.0f, 0.0f);
        }

        public void PlayPickUpSound()
        {
            m_OnItemLootedSFX.Play();
        }

        public void PlayHitByMinionSound()
        {
            m_OnMinorDamageSFX.Play();
        }

        public void PlayHitByWizardSound()
        {
            m_OnMajorDamageSFX.Play();
        }

        public void PlayRecoverHPSound()
        {
            m_OnHealSFX.Play();
        }

        public void StopGameOverSound()
        {
            m_GameOverSoundInstance?.Stop();
        }
    }
}
