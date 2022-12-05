using Objects;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class ObjectHolder : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    
    private bool _moving;
    private MovableObject Movable { get; set; }

    private void Awake()
    {
        Movable = null;
        _moving = false;
        controller.OnItemGrab += () =>
        {
            if (controller.GameMode == GameMode.TwoD)
            {
                return;
            }

            ToggleHold();
        };
    }

    private void Update()
    {
        if (_moving)
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.1f) Movable.transform.Rotate(new Vector3(0, scroll * 45, 0));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Movable = other.GetComponent<MovableObject>();
    }

    private void OnTriggerExit(Collider other)
    {
        Movable = null;
    }

    private void ToggleHold()
    {
        if (_moving)
            Drop();
        else if (Movable) Hold();
    }

    private void Hold()
    {
        var parent = transform;
        Movable.transform.position = parent.position;
        Movable.transform.SetParent(parent);
        Movable.Rigidbody.isKinematic = true;
        
        _moving = true;
        Movable.gameObject.layer = 2;
    }

    private void Drop()
    {
        Movable.transform.SetParent(null);
        Movable.Rigidbody.isKinematic = false;

        _moving = false;
        Movable.gameObject.layer = 8;
        Movable = null;
    }
}