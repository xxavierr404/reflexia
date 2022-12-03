using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [SerializeField] private short timesToLive = 1;
    [SerializeField] private TextAsset script;
    [SerializeField] private float textPause = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.GetInstance().StartDialogue(script, textPause);
            timesToLive--;
            if (timesToLive == 0) Destroy(gameObject);
        }
    }
}