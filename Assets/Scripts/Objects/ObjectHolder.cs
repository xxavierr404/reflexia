using UnityEngine;

// ReSharper disable once CheckNamespace
public class ObjectHolder : MonoBehaviour
{
    public Transform Movable { get; private set; }
    private bool _moving;
    
    private void Awake()
    {
        Movable = null;
        _moving = false;
    }
    
    private void Update()
    {
        if (_moving)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.1f)
            {
                Movable.Rotate(new Vector3(0, scroll * 45, 0));
            }
        }
    }

    public void ToggleHold()
    {
        if (GameManager.gameMode == true)
        {
            return;
        }
        if (_moving)
        {
            StopHolding();
        }
        else if (Movable)
        {
            Hold();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Movable = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        Movable = null;
    }

    private void Hold()
    {
        var parent = transform;
        Movable.position = parent.position;
        Movable.SetParent(parent);
        Movable.GetComponent<Rigidbody>().isKinematic = true;
        _moving = true;
        Movable.gameObject.layer = 2;
    }

    private void StopHolding()
    {
        Movable.SetParent(null);
        Movable.GetComponent<Rigidbody>().isKinematic = false;
        _moving = false;
        Movable.gameObject.layer = 8;
    }
}
