using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float duration;
    [SerializeField] private bool alreadyActive;
    private Vector3 currentTarget;
    private Vector3 initialPosition;
    private Vector3 dampVelocity;
    private void Start()
    {
        initialPosition = transform.position;
        currentTarget = initialPosition;
        if (alreadyActive) Activate();
    }
    
    public void Activate()
    {
        StartCoroutine(Sliding());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    IEnumerator Sliding()
    {
        while (true) {
            float dist;
            currentTarget = (currentTarget == initialPosition) ? target.position : initialPosition;
            do
            {
                dist = Vector3.Distance(transform.position, currentTarget);
                transform.position = Vector3.SmoothDamp(transform.position, currentTarget, ref dampVelocity, duration);
                yield return null;
            } while (dist > 0.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(null);
    }

}
