using UnityEngine;

public class Mirror : MonoBehaviour
{
    private Transform camTransform; //������ ������
    private Vector3 lastRotation; //���������� �������� �������� (eulerAngles)
    private Camera mirrorCam; //������ �������
    public RenderTexture mirrorTexture { get; private set; } //�������� ��������� �������
    public Material mirrorMaterial { get; private set; }

    private void Awake()
    {
        camTransform = Camera.main.transform;
        lastRotation = camTransform.eulerAngles;
        mirrorCam = transform.Find("MirrorCam").GetComponent<Camera>();
        mirrorCam.transform.localEulerAngles =
            new Vector3(0, 180 - transform.eulerAngles.y, 180); //������� ������������ �������� ������
        mirrorTexture = new RenderTexture(1024, 1024, 0); //�������� ����������� �������� ��� �������
        mirrorCam.targetTexture = mirrorTexture; //���������� �������� ������� ��� ������� �������� ������
        mirrorMaterial = new Material(Shader.Find("Standard")); //�������� ������ ��������� �� ������������ �������
        mirrorMaterial.SetTexture("_MainTex", mirrorTexture); //������� �������� ������ ���������
        transform.Find("Border").GetComponent<MeshRenderer>().material =
            mirrorMaterial; //���������� ����� �������� ��������� "������" �������
    }

    private void Update() //�������� ������ ����� �� ������� ������
    {
        var delta = camTransform.eulerAngles - lastRotation;
        delta.z = 0;
        mirrorCam.transform.localEulerAngles += delta;
        lastRotation = camTransform.eulerAngles;
    }
}