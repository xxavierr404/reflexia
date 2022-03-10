using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private float slideLength;
    [SerializeField] private bool inverted;
    [SerializeField] private float durationIn;
    [SerializeField] private float durationOut;
    private Coroutine currentCoroutine;
    private Vector3 initialPosition;
    private Vector3 direction;
    private void Awake()
    {
        initialPosition = transform.position;
        direction = inverted ? Vector3.up : Vector3.down;
    }
    public void Open()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(Utilities.LerpPosition(transform, initialPosition + slideLength * direction, durationOut));
    }
    public void Close()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(Utilities.LerpPosition(transform, initialPosition, durationIn));
    }
}
