using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [SerializeField] private List<Door> doors;
    [SerializeField] private List<MovingPlatform> platforms;
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 || collision.transform.CompareTag("Player"))
        {
            anim.SetBool("IsPressed", true);
            if (doors != null) {
                foreach(Door door in doors)
                {
                    door.Open();
                }
            }
            if (platforms != null)
            {
                foreach(MovingPlatform platform in platforms)
                {
                    platform.Activate();
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        anim.SetBool("IsPressed", false);
        if (doors != null)
        {
            foreach (Door door in doors)
            {
                door.Close();
            }
        }
        if (platforms != null)
        {
            foreach (MovingPlatform platform in platforms)
            {
                platform.Stop();
            }
        }
    }
}
