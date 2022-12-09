using System.Collections;
using Misc;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    public class DialogueWindow : MonoBehaviour
    {
        private static DialogueWindow _instance;
        
        [SerializeField] private Text dialogueName;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Image dialogueSprite;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioPlayer;
        [SerializeField] private AudioClip letterSound;

        private static readonly int IsOnScreen = Animator.StringToHash("IsOnScreen");
        
        public bool PhraseFinished { get; private set; }

        private void Awake()
        {
            _instance = this;
            PhraseFinished = true;
            SetActive(false);
        }

        public static DialogueWindow GetInstance()
        {
            return _instance;
        }

        public void SetActive(bool active) {
            animator.SetBool(IsOnScreen, active);
        }

        public void ShowPhrase(DialoguePhrase phrase)
        {
            if (!PhraseFinished)
            {
                StopAllCoroutines();
                dialogueText.text = phrase.Text;
                PhraseFinished = true;
                return;
            }

            LoadPhrase(phrase);
        }

        private void LoadPhrase(DialoguePhrase phrase)
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
            StartCoroutine(ScrollText(phrase));
            PhraseFinished = false;
        }
        
        private IEnumerator ScrollText(DialoguePhrase phrase)
        {
            for (var i = 0; i < phrase.Text.Length; i++)
            {
                dialogueText.text = phrase.Text.Substring(0, i);
                audioPlayer.PlayOneShot(letterSound);
                yield return new WaitForSeconds(phrase.ScrollPause);
            }
            PhraseFinished = true;
        }
        
        public void ResetWindow()
        {
            SetActive(false);
            dialogueName.text = "";
            dialogueText.text = "";
            dialogueSprite.sprite = null;
            dialogueSprite.enabled = false;
        }
    }
}