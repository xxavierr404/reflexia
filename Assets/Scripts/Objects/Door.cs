using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Togglable
{
    [SerializeField] private float slideLength;
    [SerializeField] private bool alreadyOpen;
    [SerializeField] private float durationIn;
    [SerializeField] private float durationOut;

    private Coroutine slideCoroutine;
    private Vector3 initialPosition;
    private Vector3 direction;

    private bool isOpen;

    private void Awake()
    {
        initialPosition = transform.position;
        direction = alreadyOpen ? Vector3.up : Vector3.down;
        isOpen = false;
    }

    public override void Toggle()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        if (isOpen)
        {
            Close();
        } else
        {
            Open();
        }
        isOpen = !isOpen;
    }

    public void Open()
    {
        slideCoroutine = StartCoroutine(Utilities.LerpPosition(transform, initialPosition + slideLength * direction, durationOut));
    }
    public void Close()
    {
        slideCoroutine = StartCoroutine(Utilities.LerpPosition(transform, initialPosition, durationIn));
    }
}
