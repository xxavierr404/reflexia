using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public RenderTexture mirrorTexture { get; private set; } //Текстура отражения зеркала
    public Material mirrorMaterial { get; private set; }
    private Camera mirrorCam; //Камера зеркала
    private Vector3 lastRotation; //Предыдущее значение вращения (eulerAngles)
    private Transform camTransform; //Камера игрока

    private void Awake()
    {
        camTransform = Camera.main.transform;
        lastRotation = camTransform.eulerAngles;
        mirrorCam = transform.Find("MirrorCam").GetComponent<Camera>();
        mirrorCam.transform.localEulerAngles = new Vector3(0, 180 - transform.eulerAngles.y, 180); //Задание изначального поворота камеры
        mirrorTexture = new RenderTexture(1024, 1024, 0); //Создание собственной текстуры для зеркала
        mirrorCam.targetTexture = mirrorTexture; //Назначение текстуры зеркала как целевой текстуры камеры
        mirrorMaterial = new Material(Shader.Find("Standard")); //Создание нового материала из стандартного шейдера
        mirrorMaterial.SetTexture("_MainTex", mirrorTexture); //Задание текстуры новому материалу
        transform.Find("Border").GetComponent<MeshRenderer>().material = mirrorMaterial; //Назначение новой текстуры рендереру "стекла" зеркала
    }
    void Update() //Вращение камеры вслед за камерой игрока
    {
        Vector3 delta = camTransform.eulerAngles - lastRotation;
        delta.z = 0;
        mirrorCam.transform.localEulerAngles += delta;
        lastRotation = camTransform.eulerAngles;
    }

}
