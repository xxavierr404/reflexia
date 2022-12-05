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

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            StartTrigger(0);
        }

        public void StartTrigger(float delay)
        {
            StartCoroutine(SwitchScene(delay));
        }
        
        private IEnumerator SwitchScene(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (sceneId.Length != 0)
            {
                if (!disableCrossfade)
                {
                    crossfadeAnimator.StartAnimation();
                    yield return new WaitForSeconds(1.0f);
                }
                SceneManager.LoadScene(sceneId);
            }
        }
    }
}