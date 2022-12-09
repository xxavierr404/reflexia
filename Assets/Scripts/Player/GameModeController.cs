using Misc;
using UnityEngine;

namespace Player
{
    public class GameModeController : MonoBehaviour
    {
        [SerializeField] private bool canSwitchModes;
        [SerializeField] private PlayerController player;

        public delegate void OnGameModeChangeFail();
        public delegate void OnGameModeChangeSuccess(GameMode newGameMode, Mirror mirror);
        
        public OnGameModeChangeSuccess OnGameModeChangeSuccessEvent { get; set; }
        public OnGameModeChangeFail OnGameModeChangeFailEvent { get; set; }
        
        public void Start()
        {
            player.GameMode = GameMode.ThreeD;
            player.OnSwitchMode += SwitchGameMode;
            OnGameModeChangeSuccessEvent += (newGameMode, moveVector) => player.MovementStrategy.StopMovement();
        }

        public void Update()
        {
            if (player.GameMode == GameMode.TwoD)
            {
                CheckMirrorBoundaries();
            }
        }

        private void SwitchGameMode()
        {
            if (!canSwitchModes) return;

            if (player.GameMode == GameMode.ThreeD)
                TrySwitch3Dto2D();
            else
                Switch2Dto3D();
        }

        private void CheckMirrorBoundaries()
        {
            if (IsOutOfFOV()) SwitchGameMode();
        }

        private bool IsOutOfFOV()
        {
            if (!player.NearestMirror) return false;
            return !Utilities.IsVisible(player.transform, player.NearestMirror.GetCamera(), 0.025f);
        }

        private void TrySwitch3Dto2D()
        {
            if (!player.NearestMirror)
            {
                OnGameModeChangeFailEvent?.Invoke();
                return;
            }

            var mirrorCam = player.NearestMirror.GetCamera();
            if (!player.IsMirrorAccessible(mirrorCam))
            {
                OnGameModeChangeFailEvent?.Invoke();
                return;
            }

            Switch3Dto2D(player.NearestMirror);
        }

        private void Switch3Dto2D(Mirror mirror)
        {
            mirror.enabled = false;
            Physics.IgnoreLayerCollision(7, 9);
            
            player.GameMode = GameMode.TwoD;

            OnGameModeChangeSuccessEvent?.Invoke(player.GameMode, mirror);
            player.MovementStrategy = new Movement2DStrategy(player);
        }

        private void Switch2Dto3D()
        {
            player.NearestMirror.enabled = true;
            player.IsJumpBlocked = false;
            Physics.IgnoreLayerCollision(7, 9, false);

            player.GameMode = GameMode.ThreeD;

            OnGameModeChangeSuccessEvent?.Invoke(player.GameMode, null);
            player.MovementStrategy = new Movement3DStrategy(player);
        }
    }
}