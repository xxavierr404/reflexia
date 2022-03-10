using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    public static ObjectHolder instance;
    public static Transform Movable { get; private set; } //Передвигаемый объект
    private bool moving; //Передвигает ли игрок объект прямо сейчас?
    private void Awake()
    {
        instance = this;
        Movable = null;
        moving = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && GameManager.gameMode == false)
        {
            if (moving) //Если игрок уже двигает объект
            {
                StopHolding();
            }
            else if (Movable) //Если игрок не двигает объект, но повёрнут к передвигаемому
            {
                Hold();
            }
        }
        if (moving)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.1f)
            {
                Movable.Rotate(new Vector3(0, scroll * 45, 0));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Movable = other.transform;
    }

    private void OnTriggerExit()
    {
        Movable = null;
    }

    private void Hold()
    {
        Movable.position = transform.position;
        Movable.SetParent(transform);
        Movable.GetComponent<Rigidbody>().isKinematic = true;
        moving = true;
        Movable.gameObject.layer = 2;
    }

    private void StopHolding()
    {
        Movable.SetParent(null);
        Movable.GetComponent<Rigidbody>().isKinematic = false;
        moving = false;
        Movable.gameObject.layer = 8;
    }

    public ObjectHolder GetHolder()
    {
        return instance;
    }
}
