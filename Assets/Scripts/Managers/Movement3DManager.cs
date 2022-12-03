using UnityEngine;

public class Movement3DManager : MonoBehaviour
{
    private static Player player;
    private static float turnSmooth; //Скорость поворота игрока
    private static Transform camTransform; //Transform камеры игрока
    private static Transform mirrorCam; //Transform камеры ближайшего зеркала

    private void Awake()
    {
        turnSmooth = 0.1f;
    }

    public static void Movement3D(Vector3 move)
    {
        if (!player) player = Player.GetPlayer();
        camTransform = GameManager.camTransform;
        Debug.DrawRay(player.transform.position, player.transform.forward);
        if (move.magnitude >= 0.1f)
        {
            player.anim.SetBool("Running", true);
            var targetAngle = RotatePlayer(move.x, move.z); //Вращение модели игрока при передвижении
            move = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            var velocity = player.rigidBody.velocity;
            if (!Physics.Raycast(player.transform.position, player.transform.forward, 1f, ~(1 << 8)))
                player.rigidBody.AddForce(move * player.speed - new Vector3(velocity.x, 0, velocity.z),
                    ForceMode.VelocityChange); //Движение с постоянной скоростью
        }
        else
        {
            player.anim.SetBool("Running", false);
        }

        if (GameManager.mirror)
        {
            mirrorCam = GameManager.mirror.transform.Find("MirrorCam");
            RotateReflection(); //Вращение отражения, чтобы оно всегда смотрело в камеру ближайшего зеркала
        }
    }

    private static float RotatePlayer(float horizontal, float vertical)
    {
        var targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
        var angle = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetAngle, ref turnSmooth, 0.1f);
        player.transform.rotation = Quaternion.Euler(0, angle, 0);
        return targetAngle;
    }

    private static void RotateReflection()
    {
        player.reflection.rotation = mirrorCam.rotation;
        player.reflection.Rotate(0, 0, 180);
    }
}