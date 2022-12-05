using System.Collections;
using Misc;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    public class DialogueWindow : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Text dialogueName;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Image dialogueSprite;
        [SerializeField] private Animator animator;
        
        private Coroutine _scrollRoutine;
        private bool _phraseFinished;
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        private void Awake()
        {
            dialoguePanel.SetActive(false);
        }

        public void ShowPhrase(DialoguePhrase phrase)
        {
            animator.SetBool(IsOpen, true);
            dialoguePanel.SetActive(true);
            
            if (_scrollRoutine != null)
            {
                StopCoroutine(_scrollRoutine);
            } 
            
            if (!_phraseFinished)
            {
                dialogueText.text = phrase.Text;
                _phraseFinished = true;
                return;
            }

            InitializeFromPhrase(phrase);
            _scrollRoutine = StartCoroutine(ScrollText(phrase));
        }

        private void InitializeFromPhrase(DialoguePhrase phrase)
        {
            dialogueName.text = phrase.Name;
            if (phrase.Sprite)
            {
                dialogueSprite.enabled = true;
                dialogueSprite.sprite = phrase.Sprite;
            }
            else
            {
                dialogueSprite.enabled = false;
            }
            _scrollRoutine = StartCoroutine(ScrollText(phrase));
            _phraseFinished = false;
        }
        
        private IEnumerator ScrollText(DialoguePhrase phrase)
        {
            for (var i = 0; i < phrase.Text.Length; i++)
            {
                dialogueText.text = phrase.Text.Substring(0, i);
                if (i == phrase.Text.Length - 1)
                {
                    _phraseFinished = true;
                }
                _audioPlayer.PlayOneShot(letterSound);
                yield return new WaitForSeconds(phrase.ScrollPause);
            }
        }
    }
}