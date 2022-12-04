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

            player.OnMoveEvent += (moveVector) => CheckMirrorBoundaries();
            player.OnSwitchMode += SwitchGameMode;
            OnGameModeChangeSuccessEvent += (newGameMode, moveVector) => player.MovementStrategy.StopMovement();
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
            player.NearestMirror.enabled = false;

            player.GameMode = GameMode.TwoD;

            OnGameModeChangeSuccessEvent?.Invoke(player.GameMode, mirror);
            player.MovementStrategy = new Movement2DStrategy(player);
        }

        private void Switch2Dto3D()
        {
            player.NearestMirror.enabled = true;
            player.IsJumpBlocked = false;

            player.GameMode = GameMode.ThreeD;

            OnGameModeChangeSuccessEvent?.Invoke(player.GameMode, null);
            player.MovementStrategy = new Movement3DStrategy(player);
        }
    }
}