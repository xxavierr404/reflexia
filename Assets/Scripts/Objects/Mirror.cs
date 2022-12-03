using UnityEngine;

public class Mirror : MonoBehaviour
{
    private Transform _camTransform;
    private Vector3 _lastRotation; 
    private Camera _mirrorCam;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    
    private RenderTexture MirrorTexture { get; set; }
    private Material MirrorMaterial { get; set; }

    private void Awake()
    {
        _mirrorCam = transform.Find("MirrorCam").GetComponent<Camera>();
        _camTransform = Camera.main.transform;
        _lastRotation = _camTransform.eulerAngles;
        _mirrorCam.transform.localEulerAngles =
            new Vector3(0, 180 - transform.eulerAngles.y, 180);
        InitializeTexture();
        GameManager.GetInstance().AddMirrorToPool(this);
    }

    private void InitializeTexture()
    {
        MirrorTexture = new RenderTexture(1024, 1024, 0);
        _mirrorCam.targetTexture = MirrorTexture;
        MirrorMaterial = new Material(Shader.Find("Standard"));
        MirrorMaterial.SetTexture(MainTex, MirrorTexture);
        transform.Find("Border").GetComponent<MeshRenderer>().material =
            MirrorMaterial;
    }

    private void Update()
    {
        var eulerAngles = _camTransform.eulerAngles;
        var delta = eulerAngles - _lastRotation;
        delta.z = 0;
        _mirrorCam.transform.localEulerAngles += delta;
        _lastRotation = eulerAngles;
    }

    public Camera GetCamera()
    {
        return _mirrorCam;
    }
}