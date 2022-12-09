using UnityEngine;

namespace Interfaces
{
    public interface IMovementStrategy
    {
        void Move(Vector3 moveVector, Mirror mirror);
        void StopMovement();
    }
}