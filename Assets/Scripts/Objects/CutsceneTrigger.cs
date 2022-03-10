using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector cutscene;
    [SerializeField] private string nextScene;
    [SerializeField] private bool disableCrossfadeOut;
    public Animator crossfade;
    private bool activated;
    private DialogueManager dialogueManager;
    
    private void Update()
    {
        if(!dialogueManager) dialogueManager = DialogueManager.GetInstance();
        if (activated && !dialogueManager.DialoguePlaying)
        {
            activated = false;
            if (cutscene)
            {
                cutscene.Play();
            }
            StartCoroutine(SwitchScene());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
        }
    }

    IEnumerator SwitchScene()
    {
        if(cutscene) yield return new WaitForSeconds((float)cutscene.duration);
        if (nextScene.Length != 0)
        {
            if(!disableCrossfadeOut) crossfade.SetTrigger("Crossfade");
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene(nextScene);
        }
    }
}
