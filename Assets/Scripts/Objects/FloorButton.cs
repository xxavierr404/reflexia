using System.Collections.Generic;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [SerializeField] private List<Togglable> targets;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Utilities.IsCollidedWithMovable(collision))
        {
            anim.SetBool("IsPressed", true);
            ToggleTargets();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        anim.SetBool("IsPressed", false);
        ToggleTargets();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (Utilities.IsTriggeredByPlayer(collider))
        {
            anim.SetBool("IsPressed", true);
            ToggleTargets();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        anim.SetBool("IsPressed", false);
        ToggleTargets();
    }

    private void ToggleTargets()
    {
        if (targets != null)
        {
            foreach (var target in targets)
            {
                target.Toggle();
            }
        }
    }
}
