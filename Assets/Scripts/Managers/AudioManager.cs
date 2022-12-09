using Player;
using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameModeController gameModeController;
        [SerializeField] private AudioClip mirrorEnterSFX;
        [SerializeField] private AudioClip mirrorExitSFX;
        [SerializeField] private AudioClip failedToSwitch;

        private AudioSource _audioPlayer;

        public void Start()
        {
            _audioPlayer = GetComponent<AudioSource>();
            gameModeController.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
            {
                _audioPlayer.PlayOneShot(newGameMode == GameMode.TwoD ? mirrorEnterSFX : mirrorExitSFX);
            };

            gameModeController.OnGameModeChangeFailEvent += () => { _audioPlayer.PlayOneShot(failedToSwitch); };
        }
    }
}