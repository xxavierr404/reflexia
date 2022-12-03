using System.Collections;
using UnityEngine;

public class MovingPlatform : Togglable
{
    [SerializeField] private Transform target;
    [SerializeField] private float duration;
    [SerializeField] private bool alreadyActive;

    private Vector3 currentTarget;
    private Vector3 dampVelocity;
    private Vector3 initialPosition;

    private bool isMoving;

    private void Start()
    {
        initialPosition = transform.position;
        currentTarget = initialPosition;
        isMoving = alreadyActive;
        if (alreadyActive) Activate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(null);
    }

    public override void Toggle()
    {
        if (isMoving)
            Stop();
        else
            Activate();
        isMoving = !isMoving;
    }

    public void Activate()
    {
        StartCoroutine(Slide());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    private IEnumerator Slide()
    {
        while (true)
        {
            float dist;
            currentTarget = currentTarget == initialPosition ? target.position : initialPosition;
            do
            {
                dist = Vector3.Distance(transform.position, currentTarget);
                transform.position = Vector3.SmoothDamp(transform.position, currentTarget, ref dampVelocity, duration);
                yield return null;
            } while (dist > 0.5f);
        }
    }
}