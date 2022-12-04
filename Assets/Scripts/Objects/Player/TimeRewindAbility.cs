using System.Collections;
using Interfaces;
using UnityEngine;

namespace Objects.Player
{
    public class TimeRewindAbility : IAbility
    {
        public void Use(PlayerController player)
        {
            if (!player.CanRewindTime)
            {
                return;
            }
            player.StartCoroutine(TimeShift(player));
        }
        
                    private IEnumerator TimeShift(PlayerController player)
                    {
                        player.NearestMirror.Freeze();
                        
                        var manager = (Movement2DStrategy)player.MovementStrategy;
                        var timeShiftLocation = manager.IsTeleporting()
                            ? manager.GetLastTeleportPosition() + Vector3.up
                            : player.transform.position;
                        player.GameModeController.SwitchGameMode();
                        yield return new WaitForSeconds(5);
                        player.transform.position = timeShiftLocation;
                        
                        player.NearestMirror.Unfreeze();
                    }
    }
}