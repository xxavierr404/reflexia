using System;
using UnityEngine;

namespace Player
{
    public class PlayerGraphicsController : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private GameModeController gameModeController;
        
        [SerializeField] private Transform reflection;
        [SerializeField] private SkinnedMeshRenderer playerMesh;
        [SerializeField] private Animator anim;

        private static readonly int MidAir = Animator.StringToHash("MidAir");
        private static readonly int Running = Animator.StringToHash("Running");
        private static readonly int JumpAnimationId = Animator.StringToHash("Jump");
        
        private void Start()
        {
            player.OnMoveEvent += (movementVector) =>
            {
                anim.SetBool(Running, movementVector.magnitude >= 0.1f);
                if (Utilities.IsGrounded(player.transform))
                {
                    anim.SetBool(MidAir, false);
                }
            };

            player.OnJump += () =>
            {
                anim.SetBool(MidAir, true);
                if (player.JumpCount == 1)
                {
                    anim.SetTrigger(JumpAnimationId);
                }
            };

            if (gameModeController)
            {
                gameModeController.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
                {
                    playerMesh.enabled = (newGameMode == GameMode.ThreeD);
                };
            }
        }

        private void Update()
        {
            RotateReflection();
        }

        private void RotateReflection()
        {
            if (!player.NearestMirror) return;
            reflection.rotation = player.NearestMirror.GetCamera().transform.rotation;
            reflection.Rotate(0, 0, 180);
        }
    }
}