using System.Collections;
using UnityEngine;

namespace Player.Abilities
{
    public class TimeRewindAbility : MonoBehaviour
    {
        private PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Start()
        {
            _player.OnTimeRewind += () =>
            {
                if (_player.GameMode == GameMode.ThreeD)
                {
                    return;
                }   
                StartCoroutine(TimeShift(_player));
            };
        }

        private IEnumerator TimeShift(PlayerController player)
        {
            player.NearestMirror.Freeze();

            var manager = (Movement2DStrategy)player.MovementStrategy;
            var timeShiftLocation = manager.IsTeleporting()
                ? manager.GetLastTeleportPosition() + Vector3.up
                : player.transform.position;
            player.OnSwitchMode?.Invoke();
            yield return new WaitForSeconds(5);
            player.transform.position = timeShiftLocation;

            player.NearestMirror.Unfreeze();
        }
    }
}