using Interfaces;
using UnityEngine;

public class Movement3DManager : IMovementManager
{
    private Player _player;
    private float _turnSmooth; //Скорость поворота игрока
    private Transform _camTransform; //Transform камеры игрока
    private Transform _mirrorCam; //Transform камеры ближайшего зеркала

    public Movement3DManager(Player player)
    {
        this._player = player;
        _turnSmooth = 0.1f;
    }

    public void Movement(Vector3 moveVector, GameObject mirror)
    {
        _camTransform = Camera.main.transform;
        var playerRigidbody = _player.GetRigidbody();
        var speed = _player.GetSpeed();
        if (moveVector.magnitude >= 0.1f)
        {
            var targetAngle = RotatePlayer(moveVector.x, moveVector.z); //Вращение модели игрока при передвижении
            moveVector = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            var velocity = playerRigidbody.velocity;
            if (!Physics.Raycast(_player.transform.position, _player.transform.forward, 1f, ~(1 << 8)))
                playerRigidbody.AddForce(moveVector * speed - new Vector3(velocity.x, 0, velocity.z),
                    ForceMode.VelocityChange); //Движение с постоянной скоростью
        }

        if (mirror)
        {
            _mirrorCam = GameManager.mirror.transform.Find("MirrorCam");
            RotateReflection(); //Вращение отражения, чтобы оно всегда смотрело в камеру ближайшего зеркала
        }
    }

    private float RotatePlayer(float horizontal, float vertical)
    {
        var targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + _camTransform.eulerAngles.y;
        var angle = Mathf.SmoothDampAngle(_player.transform.eulerAngles.y, targetAngle, ref _turnSmooth, 0.1f);
        _player.transform.rotation = Quaternion.Euler(0, angle, 0);
        return targetAngle;
    }

    private void RotateReflection()
    {
        _player.reflection.rotation = _mirrorCam.rotation;
        _player.reflection.Rotate(0, 0, 180);
    }
}