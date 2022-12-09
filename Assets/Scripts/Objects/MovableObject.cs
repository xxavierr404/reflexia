using System;
using UnityEngine;

namespace Objects
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class MovableObject : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public Collider Collider { get; private set;  }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
        }
    }
}