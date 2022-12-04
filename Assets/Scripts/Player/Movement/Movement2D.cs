using Interfaces;
using UnityEngine;

public class Movement2DStrategy : IMovementStrategy
{
    private readonly LayerMask _layerMask;
    private readonly PlayerController _player;

    private Transform _mirrorCam;
    private RaycastHit _mirrorTeleportCollision;
    
    private float _moveSign;
    private bool _reflectionTeleport;
    private Vector3 _teleportPosition;

    public Movement2DStrategy(PlayerController player)
    {
        _player = player;
        player.OnJump += () => { _reflectionTeleport = false; };
        _layerMask = LayerMask.GetMask("MirrorPlatforms", "MovablePlatforms");
    }

    public void Move(Vector3 moveVector, Mirror mirror)
    {
        _mirrorCam = mirror.GetCamera().transform;
        moveVector *= -1;
        moveVector = _mirrorCam.transform.TransformDirection(moveVector);

        var playerRigidbody = _player.Rigidbody;
        var speed = _player.Speed;
        ProcessHorizontalCollision(moveVector, playerRigidbody, speed);
        ProcessVerticalCollision(playerRigidbody);
    }

    public void StopMovement()
    {
        if (_reflectionTeleport)
        {
            _player.transform.position = _teleportPosition + _player.transform.up;
        }
    }

    private void ProcessHorizontalCollision(Vector3 moveVector, Rigidbody playerRigidbody, float speed)
    {
        if (moveVector.magnitude >= 0.1f) _moveSign = Mathf.Sign(moveVector.x);
        if (!CheckHorizontalCollision(playerRigidbody, moveVector) &&
            !CheckHorizontalCollisionOnCenter(playerRigidbody)) //Движение при отсутствии препятствий на горизонтали
        {
            var velocity = playerRigidbody.velocity;
            playerRigidbody.AddForce(moveVector * speed - new Vector3(velocity.x, 0, velocity.z),
                ForceMode.VelocityChange);
        }
        else
        {
            playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, 0);
        }
    }

    private void ProcessVerticalCollision(Rigidbody playerRigidbody)
    {
        if (CheckVerticalCollisionOnHead(playerRigidbody)) //Обработка столкновений головой
        {
            playerRigidbody.AddForce(Vector3.down * 3, ForceMode.VelocityChange);
            _player.IsJumpBlocked = true;
        }
        else
        {
            _player.IsJumpBlocked = false;
        }

        if (!CheckVerticalCollisionOnFeet(playerRigidbody)) return;
        
        playerRigidbody.AddForce(_player.transform.up 
                                 - new Vector3(0, playerRigidbody.velocity.y, 0),
            ForceMode.VelocityChange);
        _teleportPosition = _mirrorTeleportCollision.point;
        _reflectionTeleport = true;
    }

    public bool IsTeleporting()
    {
        return _reflectionTeleport;
    }

    public Vector3 GetLastTeleportPosition()
    {
        return _teleportPosition;
    }

    private bool CheckVerticalCollisionOnFeet(Rigidbody rigidbody)
    {
        var transform = rigidbody.transform;
        var position = _mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position - transform.up - position);
        return Physics.Raycast(mirrorCollisionRayVertical, 
            out _mirrorTeleportCollision,
            Mathf.Infinity,
            _layerMask);
    }

    private bool CheckVerticalCollisionOnHead(Rigidbody rigidbody)
    {
        var transform = rigidbody.transform;
        var position = _mirrorCam.position;
        var mirrorCollisionRayVertical = new Ray(position, transform.position + transform.up - position);
        return Physics.Raycast(mirrorCollisionRayVertical, Mathf.Infinity, _layerMask);
    }

    private bool CheckHorizontalCollisionOnCenter(Rigidbody rigidbody)
    {
        var position = _mirrorCam.position;
        var mirrorCollisionRayHorizontal = new Ray(position,
            rigidbody.transform.position
            + new Vector3(0.5f * _moveSign,
                Mathf.Clamp(rigidbody.velocity.y * -2.0f, -1.0f, 1.0f),
                0)
            - position);
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