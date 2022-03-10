using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public RenderTexture mirrorTexture { get; private set; } //�������� ��������� �������
    public Material mirrorMaterial { get; private set; }
    private Camera mirrorCam; //������ �������
    private Vector3 lastRotation; //���������� �������� �������� (eulerAngles)
    private Transform camTransform; //������ ������

    private void Awake()
    {
        camTransform = Camera.main.transform;
        lastRotation = camTransform.eulerAngles;
        mirrorCam = transform.Find("MirrorCam").GetComponent<Camera>();
        mirrorCam.transform.localEulerAngles = new Vector3(0, 180 - transform.eulerAngles.y, 180); //������� ������������ �������� ������
        mirrorTexture = new RenderTexture(1024, 1024, 0); //�������� ����������� �������� ��� �������
        mirrorCam.targetTexture = mirrorTexture; //���������� �������� ������� ��� ������� �������� ������
        mirrorMaterial = new Material(Shader.Find("Standard")); //�������� ������ ��������� �� ������������ �������
        mirrorMaterial.SetTexture("_MainTex", mirrorTexture); //������� �������� ������ ���������
        transform.Find("Border").GetComponent<MeshRenderer>().material = mirrorMaterial; //���������� ����� �������� ��������� "������" �������
    }
    void Update() //�������� ������ ����� �� ������� ������
    {
        Vector3 delta = camTransform.eulerAngles - lastRotation;
        delta.z = 0;
        mirrorCam.transform.localEulerAngles += delta;
        lastRotation = camTransform.eulerAngles;
    }

}
