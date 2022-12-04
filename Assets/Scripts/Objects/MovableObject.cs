using System;
using UnityEngine;

namespace Objects
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class MovableObject : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        public Rigidbody Rigidbody => _rigidbody;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        
        
        // TODO: Возврат объектов при коллизиях с препятствиями
    }
}