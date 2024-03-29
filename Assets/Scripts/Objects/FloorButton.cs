using System.Collections.Generic;
using Misc;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
    private static readonly int IsPressed = Animator.StringToHash("IsPressed");
    [SerializeField] private List<Togglable> targets;

    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Utilities.IsCollidedWithMovable(collision)
            || Utilities.IsTriggeredByPlayer(collision))
        {
            _anim.SetBool(IsPressed, true);
            ToggleTargets();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _anim.SetBool(IsPressed, false);
        ToggleTargets();
    }

    private void ToggleTargets()
    {
        if (targets != null)
            foreach (var target in targets)
                target.Toggle();
    }
}