using Misc;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private Transform _camTransform;
    private Vector3 _lastRotation;
    private Camera _mirrorCam;
    private Material _mirrorMaterial;
    private RenderTexture _mirrorTexture;
    private RenderTexture _previousTexture;

    private void Awake()
    {
        _mirrorCam = transform.Find("MirrorCam").GetComponent<Camera>();
        _camTransform = Camera.main.transform;
        _lastRotation = _camTransform.eulerAngles;
        _mirrorCam.transform.localEulerAngles =
            new Vector3(0, 180 - transform.eulerAngles.y, 180);
        InitializeTexture();
        MirrorPooler.AddMirror(this);
    }

    private void Update()
    {
        var eulerAngles = _camTransform.eulerAngles;
        var delta = eulerAngles - _lastRotation;
        delta.z = 0;
        _mirrorCam.transform.localEulerAngles += delta;
        _lastRotation = eulerAngles;
    }

    private void InitializeTexture()
    {
        _mirrorTexture = new RenderTexture(1024, 1024, 0);
        _mirrorCam.targetTexture = _mirrorTexture;
        _mirrorMaterial = new Material(Shader.Find("Standard"));
        _mirrorMaterial.SetTexture(MainTex, _mirrorTexture);
        transform.Find("Border").GetComponent<MeshRenderer>().material =
            _mirrorMaterial;
    }

    public void Freeze()
    {
        _previousTexture = _mirrorTexture;
        var freezeTexture = Utilities.ToTexture2D(_previousTexture);
        _mirrorMaterial.SetTexture(MainTex, freezeTexture);
    }

    public void Unfreeze()
    {
        _mirrorMaterial.SetTexture(MainTex, _previousTexture);
    }

    public Camera GetCamera()
    {
        return _mirrorCam;
    }
}