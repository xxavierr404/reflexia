using UnityEngine;

namespace Interfaces
{
    public interface IMovementManager
    {
        void Move(Vector3 moveVector, Mirror mirror);
        void StopMovement();
    }
}