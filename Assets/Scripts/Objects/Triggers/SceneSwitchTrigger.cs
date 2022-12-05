using System;
using System.Collections;
using GUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Objects.Triggers
{
    public class SceneSwitchTrigger : MonoBehaviour
    {
        [SerializeField] private string sceneId;
        [SerializeField] private SceneCrossfader crossfadeAnimator;
        [SerializeField] private bool disableCrossfade;

        private delegate void OnSceneCrossfade();

        private OnSceneCrossfade OnSceneCrossfadeEvent;
        
        private void Start()
        {
            if (crossfadeAnimator != null)
            {
                OnSceneCrossfadeEvent += crossfadeAnimator.StartAnimation;
            }
        }

        private IEnumerator SwitchScene(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (sceneId.Length != 0)
            {
                if (!disableCrossfade)
                {
                    OnSceneCrossfadeEvent?.Invoke();
                    yield return new WaitForSeconds(1.0f);
                }
                SceneManager.LoadScene(sceneId);
            }
        }
    }
}