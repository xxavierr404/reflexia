using UnityEngine;
using UnityEngine.Playables;

namespace Objects.Triggers
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private PlayableDirector cutscene;
        [SerializeField] private SceneSwitchTrigger switchAfterCutscene;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            cutscene.Play();

            if (!switchAfterCutscene) return;
            switchAfterCutscene.StartSwitchSceneCoroutine((float)cutscene.duration);
        }
    }
}