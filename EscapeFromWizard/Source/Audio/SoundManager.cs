using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EscapeFromWizard.Source.Audio
{
    public class SoundManager
    {
        public SoundEffect buttonSound;
        public SoundEffect gameOverSound;
        public SoundEffect getItemSound;
        public SoundEffect footstepSound;
        public SoundEffect unlockDoorSound;
        public SoundEffect hitByMinionSound;
        public SoundEffect hitByWizardSound;
        public SoundEffect recoverHPSound;
        public SoundEffect BGMSound;
        public SoundEffect winningSound;
        public SoundEffectInstance BGMInstance;
        public SoundEffectInstance GameOverBGMInstance;

        public SoundManager(ContentManager Content)
        {
            buttonSound = Content.Load<SoundEffect>(@"Resource\Audio\ButtonSFX");
            gameOverSound = Content.Load<SoundEffect>(@"Resource\Audio\GameOverSFX");
            getItemSound = Content.Load<SoundEffect>(@"Resource\Audio\GetItemSFX");
            footstepSound = Content.Load<SoundEffect>(@"Resource\Audio\FootstepSFX");
            BGMSound = Content.Load<SoundEffect>(@"Resource\Audio\Tombi_Dwarf_Forest_BGM");
            unlockDoorSound = Content.Load<SoundEffect>(@"Resource\Audio\UnlockDoorSFX");
            winningSound = Content.Load<SoundEffect>(@"Resource\Audio\Winning");
            hitByWizardSound = Content.Load<SoundEffect>(@"Resource\Audio\HitByWizardSFX");
            hitByMinionSound = Content.Load<SoundEffect>(@"Resource\Audio\HitByMinionSFX");
            recoverHPSound = Content.Load<SoundEffect>(@"Resource\Audio\HPRecoverySFX");

        }

        public void PlayBGM()
        {
          
            BGMInstance = BGMSound.CreateInstance();
            BGMInstance.IsLooped = true;
            BGMInstance.Play();
                   
        }

        public void PlayFootstepSound()
        {
            footstepSound.Play(0.15f, 0.0f, 0.0f);
        }

        public void PlayUnlockDoorSound()
        {
            unlockDoorSound.Play(0.15f, 0.0f, 0.0f);
        }

        public void PlayHidingSound()
        {
            
           buttonSound.Play(0.30f, 0.0f, 0.0f);
               
        }

        public void PlayPickUpSound()
        {
           getItemSound.Play();
        }

        public void PlayHitByMinionSound()
        {
            hitByMinionSound.Play();
        }

        public void PlayHitByWizardSound()
        {
            hitByWizardSound.Play();
        }

        public void PlayRecoverHPSound()
        {
            recoverHPSound.Play();
        }

        public void PlayWinningSound()
        {
            winningSound.Play();
        }

        public void PlayGameOverSound()
        {
            BGMInstance.Stop();
            GameOverBGMInstance = gameOverSound.CreateInstance();
            GameOverBGMInstance.IsLooped = true;
            GameOverBGMInstance.Play();
        }

        public void StopGameOverSound()
        {
            GameOverBGMInstance.Stop();
        }

        public void StopBGM()
        {
            BGMInstance.Stop();
        }
    }
}
