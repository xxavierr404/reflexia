using UnityEngine;

namespace Interfaces
{
    public interface IMovementManager
    {
        void Movement(Vector3 moveVector, GameObject mirror);
        void MovementStop();
    }
}