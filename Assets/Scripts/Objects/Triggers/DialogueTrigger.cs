using GUI;
using Ink.Runtime;
using Misc;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private float textPause = 0.1f;
    [SerializeField] private TextAsset script;
    [SerializeField] private AudioClip letterSound;
    [SerializeField] private DialogueWindow window;
    
    private DialoguePhrase _phrase;
    private Story _story;
    private float _currentPause;

    public delegate void OnDialogueStart();
    public delegate void OnDialogueContinue(DialoguePhrase phrase);
    public delegate void OnDialogueEnd();

    public OnDialogueStart OnDialogueStartEvent;
    public OnDialogueContinue OnDialogueContinueEvent;
    public OnDialogueEnd OnDialogueEndEvent;

    private void Start()
    {
        OnDialogueStartEvent += () => window.SetActive(true);
        OnDialogueContinueEvent += (phrase) => window.ShowPhrase(phrase);
        OnDialogueEndEvent += () =>
        {
            window.ResetWindow();
            window.SetActive(false);
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnDialogueStartEvent?.Invoke();
            
            var player = other.transform.GetComponent<PlayerController>();
            player.IsJumpBlocked = true;
            player.OnJump += ContinueDialogue;
            Initialize(script, textPause);
        }
    }

    private void Initialize(TextAsset inkScript, float pause)
    {
        _story = new Story(inkScript.text);
        _currentPause = pause;
        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        if (_story.canContinue)
        {
            LoadPhrase();
            OnDialogueContinueEvent?.Invoke(_phrase);
            return;
        }

        OnDialogueEndEvent?.Invoke();
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
}