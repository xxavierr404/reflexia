using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private AudioClip mirrorEnterSFX;
        [SerializeField] private AudioClip mirrorExitSFX;
        [SerializeField] private AudioClip failedToSwitch;
    
        private AudioSource _audioPlayer;
        
        public void Start()
        {
            _audioPlayer = GetComponent<AudioSource>();
            player.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
            {
                _audioPlayer.PlayOneShot(newGameMode == GameMode.TwoD ? mirrorEnterSFX : mirrorExitSFX);
            };

            player.OnGameModeChangeFailEvent += () =>
            {
                _audioPlayer.PlayOneShot(failedToSwitch);
            };
        }
    }
}