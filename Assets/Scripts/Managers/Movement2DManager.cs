using UnityEngine;

public class Movement2DManager : MonoBehaviour
{
    private static Player player; //Игрок
    private static float moveSign; //Направление последнего движения по локальной оси X зеркала

    private static RaycastHit
        mirrorTeleportCollision; //Информация о последней коллизии "ног" игрока с отражением в зеркале

    private static LayerMask layerMask; //Маска слоёв для рейкастов обработки коллизии отражений
    private static Transform mirrorCam; //Камера ближайшего зеркала

    private void Awake()
    {
        layerMask = LayerMask.GetMask("MirrorPlatforms", "MovablePlatforms");
    }

    public static void Movement2D(Vector3 move)
    {
        if (!player) player = Player.GetPlayer();
        mirrorCam = GameManager.mirrorCam;
        move.z = 0; //Обнуление z-компоненты движения
        move = GameManager.mirror.transform.TransformDirection(move);
        if (move.magnitude >= 0.1f) moveSign = Mathf.Sign(move.x);
        if (!CheckHorizontalCollision(move) &&
            !CheckHorizontalCollisionOnCenter()) //Движение при отсутствии препятствий на горизонтали
        {
            var velocity = player.rigidBody.velocity;
            player.rigidBody.AddForce(move * player.speed - new Vector3(velocity.x, 0, velocity.z),
                ForceMode.VelocityChange);
        }
        else //Остановка при горизонтальной коллизии с отражением
        {
            player.rigidBody.velocity = new Vector3(0, player.rigidBody.velocity.y, 0);
        }

        if (CheckVerticalCollisionOnHead()) //Обработка столкновений головой
        {
            player.rigidBody.AddForce(Vector3.down * 3, ForceMode.VelocityChange);
            player.blockJump = true;
        }
        else
        {
            player.blockJump = false;
        }

        if (CheckVerticalCollisionOnFeet()) //Приземление на верхнюю грань отражения
        {
            player.rigidBody.AddForce(player.transform.up - new Vector3(0, player.rigidBody.velocity.y, 0),
                ForceMode.VelocityChange);
            GameManager.teleportLocation = mirrorTeleportCollision.point;
            GameManager.reflectionTeleport = true;
            if (player.FramesDelay == 0) player.JumpCount = 0;
        }
    }

    private static bool CheckVerticalCollisionOnFeet()
    {
        var transform = player.transform;
        var position = mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position - transform.up - position);
        return Physics.Raycast(mirrorCollisionRayVertical, out mirrorTeleportCollision, Mathf.Infinity, layerMask);
    }

    private static bool CheckVerticalCollisionOnHead()
    {
        var transform = player.transform;
        var position = mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position + transform.up * 0.8f - position);
        return Physics.Raycast(mirrorCollisionRayVertical, Mathf.Infinity, layerMask);
    }

    private static bool CheckHorizontalCollisionOnCenter()
    {
        var position = mirrorCam.position;
        var mirrorCollisionRayHorizontal = new Ray(position,
            player.transform.position +
            new Vector3(0.5f * moveSign, Mathf.Clamp(player.rigidBody.velocity.y * -2.0f, -1.0f, 1.0f), 0) - position);
        return Physics.Raycast(mirrorCollisionRayHorizontal, Mathf.Infinity, layerMask);
    }

    private static bool CheckHorizontalCollision(Vector3 move)
    {
        var transform = player.transform;
        var position = mirrorCam.position;
        var mirrorCollisionRayHorizontal =
            new Ray(position, transform.position - transform.up * 0.7f + move * 0.8f - position);
        return Physics.Raycast(mirrorCollisionRayHorizontal, Mathf.Infinity, layerMask);
    }
}