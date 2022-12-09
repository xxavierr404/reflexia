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
        if (!_moving) return;
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.1f) Movable.transform.Rotate(new Vector3(0, scroll * 45, 0));
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
        else if (Movable && IsPositionReachable(Movable.transform.position, 1f)) Hold();
    }

    private void Hold()
    {
        var parent = transform;
        Movable.transform.position = parent.position;
        Movable.transform.SetParent(parent);
        Movable.Collider.enabled = false;
        Movable.Rigidbody.isKinematic = true;

        _moving = true;
        Movable.gameObject.layer = 2;
    }

    private void Drop()
    {
        Movable.Collider.enabled = true;
        var allowedToDrop = IsEnoughSpaceToDrop();
        if (!allowedToDrop)
        {
            Movable.Collider.enabled = false;
            return;
        }
        Movable.transform.SetParent(null);
        Movable.Rigidbody.isKinematic = false;

        _moving = false;
        Movable.gameObject.layer = 8;
        Movable = null;
    }

    private bool IsPositionReachable(Vector3 position, float distance)
    {
        var playerPos = controller.transform.position;
        return !Physics.Raycast(playerPos,
            position - playerPos,
            distance,
            LayerMask.GetMask("Default"));
    }

    private bool IsEnoughSpaceToDrop()
    {
        var bounds = Movable.Collider.bounds;
        var minPoint = bounds.min;
        var maxPoint = bounds.max;
        var playerPos = controller.transform.position;
        return IsPositionReachable(minPoint, (minPoint - playerPos).magnitude)
               && IsPositionReachable(maxPoint, (maxPoint - playerPos).magnitude);
    }
}