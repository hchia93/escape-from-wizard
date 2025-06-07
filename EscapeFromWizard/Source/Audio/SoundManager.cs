using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using EscapeFromWizard.Source;

namespace EscapeFromWizard.Source.Audio
{
    public class SoundManager
    {
        // Sound effects
        public SoundEffect m_OnHidingSFX;
        public SoundEffect m_OnMoveSFX;
        public SoundEffect m_OnPickUpSFX;
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

        // Internal timing flags
        private double m_FootStepTimer = 0.0f;
        private const double m_FootStepPlayInterval = 0.2;

        public SoundManager(ContentManager Content)
        {
            m_OnHidingSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.ButtonSFX);
            m_OnMoveSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.FootstepSFX);
            m_BGMSound = Content.Load<SoundEffect>(ResourcePaths.Audio.BGM);
            m_GameOver = Content.Load<SoundEffect>(ResourcePaths.Audio.GameOverSFX);
            m_GameVictory = Content.Load<SoundEffect>(ResourcePaths.Audio.VictorySFX);
            m_OnPickUpSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.PickUpSFX);
            m_OnUnlockDoorSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.UnlockDoorSFX);
            m_OnMajorDamageSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.HitByWizardSFX);
            m_OnMinorDamageSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.HitByMinionSFX);
            m_OnHealSFX = Content.Load<SoundEffect>(ResourcePaths.Audio.HealSFX);
        }

        public void Update(GameTime gameTime)
        {
            // Update footstep timer
            m_FootStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Smart play methods that handle "play once" logic internally
        public void PlayBGM()
        {
            // Only start BGM if it's not already playing
            if (m_BGMSoundInstance == null || m_BGMSoundInstance.State != SoundState.Playing)
            {
                m_BGMSoundInstance = m_BGMSound.CreateInstance();
                m_BGMSoundInstance.IsLooped = true;
                m_BGMSoundInstance.Volume = 0.5f;
                m_BGMSoundInstance.Play();
            }
        }

        public void PlayGameOver()
        {
            // Only start game over sound if it's not already playing
            if (m_GameOverSoundInstance == null || m_GameOverSoundInstance.State != SoundState.Playing)
            {
                m_BGMSoundInstance?.Stop();
                m_GameOverSoundInstance = m_GameOver.CreateInstance();
                m_GameOverSoundInstance.IsLooped = true;
                m_GameOverSoundInstance.Play();
            }
        }

        public void TryPlayFootstepSound(bool isPlayerMoving)
        {
            if (isPlayerMoving && m_FootStepTimer > m_FootStepPlayInterval)
            {
                PlayFootstepSound();
                m_FootStepTimer = 0.0f;
            }
        }

        // Original play methods (now used internally)
        public void OnGameFinished()
        {
            m_BGMSoundInstance?.Stop(); 
            m_GameVictory.Play();
        }

        public void PlayFootstepSound()
        {
            m_OnMoveSFX.Play(0.10f, 0.0f, 0.0f);
        }

        public void PlayUnlockDoorSound()
        {
            m_OnUnlockDoorSFX.Play(0.5f, 0.0f, 0.0f);
        }

        public void PlayHideSound()
        {
            m_OnHidingSFX.Play(0.5f, 0.0f, 0.0f);
        }

        public void PlayPickUpSound()
        {
            m_OnPickUpSFX.Play(0.3f, 0.0f, 0.0f);
        }

        public void PlayHitByMinionSound()
        {
            m_OnMinorDamageSFX.Play(0.8f, 0.0f, 0.0f);
        }

        public void PlayHitByWizardSound()
        {
            m_OnMajorDamageSFX.Play(0.8f, 0.0f, 0.0f);
        }

        public void PlayHealSound()
        {
            m_OnHealSFX.Play(0.5f, 0.0f, 0.0f);
        }

        public void StopGameOverSound()
        {
            m_GameOverSoundInstance?.Stop();
        }
    }
}
