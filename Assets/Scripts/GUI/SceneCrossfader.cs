using UnityEngine;

namespace GUI
{
    public class SceneCrossfader : MonoBehaviour
    {
        [SerializeField] private Animator crossfadeAnimator;
        
        private static readonly int Crossfade = Animator.StringToHash("Crossfade");

        public void StartAnimation()
        {
            crossfadeAnimator.SetTrigger(Crossfade);
        }
    }
}