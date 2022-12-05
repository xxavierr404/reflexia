using System.Collections;
using Ink.Runtime;
using Misc;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    
    [SerializeField] private AudioClip letterSound;

    private AudioSource _audioPlayer;
    private DialoguePhrase _phrase;
    private Story _story;
    private float _currentPause;

    public bool DialoguePlaying { get; private set; }

    private void Awake()
    {
        _instance = this;
        DialoguePlaying = false;
        _audioPlayer = GetComponent<AudioSource>();
    }

    public static DialogueManager GetInstance()
    {
        return _instance;
    }

    public void StartDialogue(TextAsset inkScript, float pause)
    {
        //       Player.GetPlayer().anim.SetBool("Running", false); // ???
        _story = new Story(inkScript.text);
        _currentPause = pause;
        DialoguePlaying = true;
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (!DialoguePlaying) return;

        if (_story.canContinue)
        {
            LoadPhrase();
            return;
        }

        ExitDialogue();
    }

    private void LoadPhrase()
    {
        _phrase = new DialoguePhrase(_story.Continue(), _currentPause);
        foreach (var spriteTag in _story.currentTags)
            if (spriteTag.StartsWith("sprite"))
                LoadSprite(spriteTag);
    }
    
            
    private void LoadSprite(string spriteTag)
    {
        var spriteID = spriteTag.Substring(7);
        if (spriteID != "off")
        {
            _phrase.Sprite = Resources.Load<Sprite>(spriteID);
        }
    }

    private void ExitDialogue()
    {
        DialoguePlaying = false;
        animator.SetBool(IsOpen, false);
        dialoguePanel.SetActive(false);
        dialogueName.text = "";
        dialogueText.text = "";
        dialogueSprite.sprite = null;
    }
}