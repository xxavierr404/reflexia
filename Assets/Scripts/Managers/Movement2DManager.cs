using Interfaces;
using UnityEngine;

public class Movement2DManager : IMovementManager
{
    private Player player;
    private float _moveSign; //Направление последнего движения по локальной оси X зеркала
    private RaycastHit
        _mirrorTeleportCollision; //Информация о последней коллизии "ног" игрока с отражением в зеркале
    private LayerMask _layerMask; //Маска слоёв для рейкастов обработки коллизии отражений
    private Transform _mirrorCam; //Камера ближайшего зеркала
    private bool _reflectionTeleport;
    private Vector3 _teleportPosition;
    
    public Movement2DManager(Player player)
    {
        this.player = player;
        player.OnSpaceEvent += () =>
        {
            _reflectionTeleport = false;
        };
        _layerMask = LayerMask.GetMask("MirrorPlatforms", "MovablePlatforms");
    }

    public void Movement(Vector3 moveVector, GameObject mirror)
    {
        var playerRigidbody = player.GetRigidbody();
        var speed = player.GetSpeed();
        _mirrorCam = _mirrorCam ? _mirrorCam : mirror.GetComponent<Camera>().transform;
        moveVector = mirror.transform.TransformDirection(moveVector);
        
        ProcessHorizontalCollision(moveVector, playerRigidbody, speed);
        ProcessVerticalCollision(playerRigidbody);
    }

    private void ProcessHorizontalCollision(Vector3 moveVector, Rigidbody playerRigidbody, float speed)
    {
        moveVector.z = 0; //Обнуление z-компоненты движения
        if (moveVector.magnitude >= 0.1f) _moveSign = Mathf.Sign(moveVector.x);
        if (!CheckHorizontalCollision(playerRigidbody, moveVector) &&
            !CheckHorizontalCollisionOnCenter(playerRigidbody)) //Движение при отсутствии препятствий на горизонтали
        {
            var velocity = playerRigidbody.velocity;
            playerRigidbody.AddForce(moveVector * speed - new Vector3(velocity.x, 0, velocity.z),
                ForceMode.VelocityChange);
        }
        else //Остановка при горизонтальной коллизии с отражением
        {
            playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, 0);
        }
    }

    private void ProcessVerticalCollision(Rigidbody playerRigidbody)
    {
        if (CheckVerticalCollisionOnHead(playerRigidbody)) //Обработка столкновений головой
        {
            playerRigidbody.AddForce(Vector3.down * 3, ForceMode.VelocityChange);
            player.SetJumpBlock(true);
        }
        else
        {
            player.SetJumpBlock(false);
        }

        if (CheckVerticalCollisionOnFeet(playerRigidbody)) //Приземление на верхнюю грань отражения
        {
            playerRigidbody.AddForce(player.transform.up - new Vector3(0, playerRigidbody.velocity.y, 0),
                ForceMode.VelocityChange);
            _teleportPosition = _mirrorTeleportCollision.point;
            _reflectionTeleport = true;
        }
    }

    public void MovementStop()
    {
        if (_reflectionTeleport)
        {
            player.transform.position = _teleportPosition + player.transform.up;
        }
    }

    private bool CheckVerticalCollisionOnFeet(Rigidbody rigidbody)
    {
        var transform = rigidbody.transform;
        var position = _mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position - transform.up - position);
        return Physics.Raycast(mirrorCollisionRayVertical, out _mirrorTeleportCollision, Mathf.Infinity, _layerMask);
    }

    private bool CheckVerticalCollisionOnHead(Rigidbody rigidbody)
    {
        var transform = rigidbody.transform;
        var position = _mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position + transform.up * 0.8f - position);
        return Physics.Raycast(mirrorCollisionRayVertical, Mathf.Infinity, _layerMask);
    }

    private bool CheckHorizontalCollisionOnCenter(Rigidbody rigidbody)
    {
        var position = _mirrorCam.position;
        var mirrorCollisionRayHorizontal = new Ray(position,
            rigidbody.transform.position +
            new Vector3(0.5f * _moveSign, Mathf.Clamp(rigidbody.velocity.y * -2.0f, -1.0f, 1.0f), 0) - position);
        return Physics.Raycast(mirrorCollisionRayHorizontal, Mathf.Infinity, _layerMask);
    }

    private bool CheckHorizontalCollision(Rigidbody rigidbody, Vector3 move)
    {
        var transform = rigidbody.transform;
        var position = _mirrorCam.position;
        var mirrorCollisionRayHorizontal =
            new Ray(position, transform.position - transform.up * 0.7f + move * 0.8f - position);
        return Physics.Raycast(mirrorCollisionRayHorizontal, Mathf.Infinity, _layerMask);
    }
}