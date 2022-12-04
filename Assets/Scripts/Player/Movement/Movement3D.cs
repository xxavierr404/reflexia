using Interfaces;
using UnityEngine;

public class Movement3DStrategy : IMovementStrategy
{
    private Transform _camTransform; //Transform камеры игрока
    private Transform _mirrorCam; //Transform камеры ближайшего зеркала
    private readonly PlayerController _player;
    private float _turnSmooth; //Скорость поворота игрока

    public Movement3DStrategy(PlayerController player)
    {
        _player = player;
        _turnSmooth = 0.1f;
    }

    public void Move(Vector3 moveVector, Mirror mirror)
    {
        _camTransform = Camera.main.transform;
        var playerRigidbody = _player.Rigidbody;
        var speed = _player.Speed;
        if (moveVector.magnitude >= 0.1f)
        {
            var targetAngle = RotatePlayer(moveVector.x, moveVector.z);
            moveVector = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            var velocity = playerRigidbody.velocity;
            if (!HasObstacleInFront())
                playerRigidbody.AddForce(moveVector * speed - new Vector3(velocity.x, 0, velocity.z),
                    ForceMode.VelocityChange);
        }
    }

    public void StopMovement()
    {
        _player.Rigidbody.velocity = Vector3.zero;
    }

    private bool HasObstacleInFront()
    {
        var transform = _player.transform;
        return Physics.Raycast(transform.position,
            transform.forward,
            1f,
            ~(1 << 8));
    }

    private float RotatePlayer(float horizontal, float vertical)
    {
        var targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + _camTransform.eulerAngles.y;
        var angle = Mathf.SmoothDampAngle(_player.transform.eulerAngles.y,
            targetAngle,
            ref _turnSmooth,
            0.1f);
        _player.transform.rotation = Quaternion.Euler(0, angle, 0);
        return targetAngle;
    }
}