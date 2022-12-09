using System;
using System.Collections;
using Cinemachine;
using Misc;
using Player;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Managers
{
    public class CameraEffects : MonoBehaviour
    {
        [SerializeField] private GameModeController gameModeController;
        [SerializeField] private PlayerController player;
        private Camera _camera;

        private ChromaticAberration _chromaticAberration;
        private CinemachineBrain _cinemachineBrain;
        private Coroutine _focus;
        private Vignette _vignette;

        private void Awake()
        {
            _camera = Camera.main;
            _cinemachineBrain = _camera.GetComponent<CinemachineBrain>();

            SetupVignette();
            SetupAberration();
            PostProcessManager.instance.QuickVolume(11, 0, _vignette, _chromaticAberration);
        }

        private void Start()
        {
            gameModeController.OnGameModeChangeSuccessEvent += (newGameMode, mirror) =>
            {
                if (newGameMode == GameMode.TwoD)
                {
                    Ignore3DLayer();
                    AnimateCameraToMirror(mirror.transform);
                }
                else
                {
                    StopFocus();
                    StopIgnoring3DLayer();
                }
            };

            player.OnTimeRewind += StartTimeShiftFX;
        }

        private void SetupAberration()
        {
            _chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
            _chromaticAberration.enabled.Override(false);
            _chromaticAberration.intensity.Override(1f);
        }

        private void SetupVignette()
        {
            _vignette = ScriptableObject.CreateInstance<Vignette>();
            _vignette.enabled.Override(false);
            _vignette.intensity.Override(0.685f);
            _vignette.smoothness.Override(0.8f);
            _vignette.roundness.Override(0.2f);
        }

        private void StartTimeShiftFX()
        {
            _vignette = ScriptableObject.CreateInstance<Vignette>();
            _vignette.enabled.Override(true);
            _chromaticAberration.enabled.Override(true);
            StartCoroutine(TimeShiftFX());
        }

        private IEnumerator TimeShiftFX()
        {
            while (_vignette.intensity.value > 0f || _chromaticAberration.intensity.value > 0f)
            {
                _vignette.intensity.value -= 0.02f;
                _chromaticAberration.intensity.value -= 0.0137f;
                yield return new WaitForSeconds(0.1f);
            }
            
            _vignette.enabled.Override(false);
            _vignette.intensity.Override(0.685f);
            _chromaticAberration.enabled.Override(false);
            _chromaticAberration.intensity.Override(1f);
        }

        private void Ignore3DLayer()
        {
            _camera.cullingMask = LayerMask.GetMask("Default", "MirrorIgnore");
            _cinemachineBrain.enabled = false;
        }

        private void StopIgnoring3DLayer()
        {
            _camera.cullingMask = ~LayerMask.GetMask("PlayerReflection");
            _cinemachineBrain.enabled = true;
        }

        private void AnimateCameraToMirror(Transform mirror)
        {
            _focus = StartCoroutine(Utilities.Focus(_camera.transform, mirror));
            StartCoroutine(Utilities.LerpRotation(_camera.transform, mirror, 2.0f));
        }

        private void StopFocus()
        {
            if (_focus != null) StopCoroutine(_focus);
        }
    }
}