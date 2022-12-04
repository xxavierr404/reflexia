namespace Objects.Player
{
    public class GameModeController
    {
        private readonly PlayerController _player;

        public GameModeController(PlayerController player)
        {
            _player = player;
            GameMode = GameMode.ThreeD;
        }

        public GameMode GameMode { get; private set; }

        public void CheckMirrorBoundaries()
        {
            if (!Utilities.IsVisible(_player.transform, _player.NearestMirror.GetCamera(), 0.025f)) SwitchGameMode();
        }

        public void SwitchGameMode()
        {
            if (!_player.CanSwitchModes) return;

            if (GameMode == GameMode.ThreeD)
                TrySwitch3Dto2D();
            else
                Switch2Dto3D();
        }

        private void TrySwitch3Dto2D()
        {
            if (!_player.NearestMirror)
            {
                _player.OnGameModeChangeFailEvent?.Invoke();
                return;
            }

            var mirrorCam = _player.NearestMirror.GetCamera();
            if (!_player.IsMirrorAccessible(mirrorCam))
            {
                _player.OnGameModeChangeFailEvent?.Invoke();
                return;
            }

            Switch3Dto2D(_player.NearestMirror);
        }

        private void Switch3Dto2D(Mirror mirror)
        {
            _player.NearestMirror.enabled = false;
            _player.SetMeshEnabled(false);

            GameMode = GameMode.TwoD;

            _player.OnGameModeChangeSuccessEvent?.Invoke(GameMode, mirror);
            _player.MovementStrategy = new Movement2DStrategy(_player);
        }

        private void Switch2Dto3D()
        {
            _player.NearestMirror.enabled = true;
            _player.SetMeshEnabled(true);
            _player.SetJumpBlock(false);

            GameMode = GameMode.ThreeD;

            _player.OnGameModeChangeSuccessEvent?.Invoke(GameMode, null);
            _player.MovementStrategy = new Movement3DStrategy(_player);
        }
    }
}