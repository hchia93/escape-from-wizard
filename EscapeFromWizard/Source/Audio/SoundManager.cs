using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EscapeFromWizard.Source.Audio
{
    public class SoundManager
    {
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
        public SoundEffectInstance m_BGMSoundInstance;
        public SoundEffectInstance m_GameOverSoundInstance;

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
        }

        public void OnGameFinished()
        {
            m_BGMSoundInstance.Stop(); 
            m_GameVictory.Play();
        }

        public void OnGameOver()
        {
            m_BGMSoundInstance.Stop();
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
            m_GameOverSoundInstance.Stop();
        }

    }
}
