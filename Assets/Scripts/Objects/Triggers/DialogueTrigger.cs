using GUI;
using Ink.Runtime;
using Misc;
using UnityEngine;

namespace Objects.Triggers
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private float textPause = 0.1f;
        [SerializeField] private TextAsset script;

        private DialogueWindow _window;
        private DialoguePhrase _phrase;
        private Story _story;

        private delegate void OnDialogueEnd();
        private OnDialogueEnd _onDialogueEndEvent;
        
        private void Start() {
            _window = DialogueWindow.GetInstance();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            var player = other.transform.GetComponent<PlayerController>();
            player.IsMovementBlocked = true;
            player.OnJump += ContinueDialogue;
            _onDialogueEndEvent += () =>
            {
                _window.ResetWindow();
                _window.SetActive(false);
                player.IsMovementBlocked = false;
            };    
            
            Initialize(script);
        }

        private void Initialize(TextAsset inkScript)
        {
            _story = new Story(inkScript.text);
            _window.SetActive(true);
            ContinueDialogue();
        }

        private void ContinueDialogue()
        {
            if (!_window.PhraseFinished)
            {
                _window.ShowPhrase(_phrase);
                return;
            }
            
            if (_story.canContinue)
            {
                LoadPhrase();
                _window.ShowPhrase(_phrase);
                return;
            }

            _onDialogueEndEvent?.Invoke();
        }

        private void LoadPhrase()
        {
            _phrase = new DialoguePhrase(_story.Continue(), textPause);
            foreach (var spriteTag in _story.currentTags) {
                if (spriteTag.StartsWith("sprite")) {
                    LoadSprite(spriteTag);
                }
            }
        }
        
        private void LoadSprite(string spriteTag)
        {
            var spriteID = spriteTag.Substring(7);
            if (spriteID != "off") {
                _phrase.Sprite = Resources.Load<Sprite>(spriteID);
            }
        }
    }
}